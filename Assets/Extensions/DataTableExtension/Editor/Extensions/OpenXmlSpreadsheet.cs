using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace DE.Editor
{
    public sealed class OpenXmlWorkbook : IDisposable
    {
        private const string RelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
        private readonly ZipArchive m_Archive;
        private readonly List<OpenXmlWorksheet> m_Worksheets = new List<OpenXmlWorksheet>();
        private readonly string[] m_SharedStrings;

        public IReadOnlyList<OpenXmlWorksheet> Worksheets => m_Worksheets;

        private OpenXmlWorkbook(ZipArchive archive)
        {
            m_Archive = archive;
            m_SharedStrings = ReadSharedStrings(archive);
            LoadWorksheets();
        }

        public static OpenXmlWorkbook Open(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return new OpenXmlWorkbook(new ZipArchive(stream, ZipArchiveMode.Read, true));
        }

        public void Dispose()
        {
            m_Archive.Dispose();
        }

        private void LoadWorksheets()
        {
            XmlDocument workbookDocument = LoadXml("xl/workbook.xml");
            Dictionary<string, string> workbookRelationships = LoadRelationships("xl/_rels/workbook.xml.rels");
            XmlNamespaceManager namespaceManager = CreateNamespaceManager(workbookDocument);
            XmlNodeList sheetNodes = workbookDocument.SelectNodes("//x:sheet", namespaceManager);
            if (sheetNodes == null)
            {
                return;
            }

            foreach (XmlNode sheetNode in sheetNodes)
            {
                string name = sheetNode.Attributes?["name"]?.Value;
                string relationshipId = sheetNode.Attributes?["id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"]?.Value;
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(relationshipId) ||
                    !workbookRelationships.TryGetValue(relationshipId, out string target))
                {
                    continue;
                }

                string worksheetPath = NormalizePackagePath("xl/" + target);
                m_Worksheets.Add(OpenXmlWorksheet.Load(m_Archive, worksheetPath, name, m_SharedStrings));
            }
        }

        private static string[] ReadSharedStrings(ZipArchive archive)
        {
            ZipArchiveEntry entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null)
            {
                return new string[0];
            }

            XmlDocument document = LoadXml(entry);
            XmlNamespaceManager namespaceManager = CreateNamespaceManager(document);
            XmlNodeList stringNodes = document.SelectNodes("//x:si", namespaceManager);
            if (stringNodes == null)
            {
                return new string[0];
            }

            string[] values = new string[stringNodes.Count];
            for (int i = 0; i < stringNodes.Count; i++)
            {
                values[i] = string.Concat(stringNodes[i].SelectNodes(".//x:t", namespaceManager)
                    ?.Cast<XmlNode>()
                    .Select(node => node.InnerText) ?? Enumerable.Empty<string>());
            }

            return values;
        }

        private XmlDocument LoadXml(string path)
        {
            ZipArchiveEntry entry = m_Archive.GetEntry(path);
            if (entry == null)
            {
                throw new InvalidDataException($"XLSX entry '{path}' is missing.");
            }

            return LoadXml(entry);
        }

        private static XmlDocument LoadXml(ZipArchiveEntry entry)
        {
            XmlDocument document = new XmlDocument
            {
                XmlResolver = null
            };

            using (Stream stream = entry.Open())
            {
                document.Load(stream);
            }

            return document;
        }

        private Dictionary<string, string> LoadRelationships(string path)
        {
            ZipArchiveEntry entry = m_Archive.GetEntry(path);
            if (entry == null)
            {
                return new Dictionary<string, string>(StringComparer.Ordinal);
            }

            XmlDocument document = LoadXml(entry);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace("r", RelationshipNamespace);
            XmlNodeList relationshipNodes = document.SelectNodes("//r:Relationship", namespaceManager);
            Dictionary<string, string> relationships = new Dictionary<string, string>(StringComparer.Ordinal);
            if (relationshipNodes == null)
            {
                return relationships;
            }

            foreach (XmlNode relationshipNode in relationshipNodes)
            {
                string id = relationshipNode.Attributes?["Id"]?.Value;
                string target = relationshipNode.Attributes?["Target"]?.Value;
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(target))
                {
                    relationships[id] = target;
                }
            }

            return relationships;
        }

        private static XmlNamespaceManager CreateNamespaceManager(XmlDocument document)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace("x", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
            return namespaceManager;
        }

        private static string NormalizePackagePath(string path)
        {
            Stack<string> parts = new Stack<string>();
            foreach (string rawPart in path.Replace('\\', '/').Split('/'))
            {
                if (string.IsNullOrEmpty(rawPart) || rawPart == ".")
                {
                    continue;
                }

                if (rawPart == "..")
                {
                    if (parts.Count > 0)
                    {
                        parts.Pop();
                    }

                    continue;
                }

                parts.Push(rawPart);
            }

            return string.Join("/", parts.Reverse());
        }
    }

    public sealed class OpenXmlWorksheet
    {
        private static readonly Regex CellReferenceRegex = new Regex(@"^([A-Z]+)(\d+)$", RegexOptions.Compiled);
        private readonly Dictionary<CellAddress, string> m_Cells;

        public string Name { get; }
        public int RowCount { get; }
        public int ColumnCount { get; }

        private OpenXmlWorksheet(string name, Dictionary<CellAddress, string> cells, int rowCount, int columnCount)
        {
            Name = name;
            m_Cells = cells;
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public string GetCellValue(int row, int column)
        {
            return m_Cells.TryGetValue(new CellAddress(row, column), out string value) ? value : null;
        }

        internal static OpenXmlWorksheet Load(ZipArchive archive, string worksheetPath, string name, string[] sharedStrings)
        {
            ZipArchiveEntry entry = archive.GetEntry(worksheetPath);
            if (entry == null)
            {
                throw new InvalidDataException($"XLSX worksheet entry '{worksheetPath}' is missing.");
            }

            XmlDocument document = new XmlDocument
            {
                XmlResolver = null
            };
            using (Stream stream = entry.Open())
            {
                document.Load(stream);
            }

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace("x", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

            Dictionary<CellAddress, string> cells = new Dictionary<CellAddress, string>();
            int maxRow = 0;
            int maxColumn = 0;
            XmlNodeList cellNodes = document.SelectNodes("//x:c", namespaceManager);
            if (cellNodes != null)
            {
                foreach (XmlNode cellNode in cellNodes)
                {
                    string reference = cellNode.Attributes?["r"]?.Value;
                    if (!TryParseCellReference(reference, out int row, out int column))
                    {
                        continue;
                    }

                    string value = ReadCellValue(cellNode, namespaceManager, sharedStrings);
                    if (value != null)
                    {
                        cells[new CellAddress(row, column)] = value;
                    }

                    maxRow = Math.Max(maxRow, row);
                    maxColumn = Math.Max(maxColumn, column);
                }
            }

            ReadDimension(document, namespaceManager, ref maxRow, ref maxColumn);
            return new OpenXmlWorksheet(name, cells, maxRow, maxColumn);
        }

        private static string ReadCellValue(XmlNode cellNode, XmlNamespaceManager namespaceManager, string[] sharedStrings)
        {
            string dataType = cellNode.Attributes?["t"]?.Value;
            if (dataType == "inlineStr")
            {
                return string.Concat(cellNode.SelectNodes(".//x:is//x:t", namespaceManager)
                    ?.Cast<XmlNode>()
                    .Select(node => node.InnerText) ?? Enumerable.Empty<string>());
            }

            XmlNode valueNode = cellNode.SelectSingleNode("x:v", namespaceManager);
            if (valueNode == null)
            {
                return null;
            }

            string value = valueNode.InnerText;
            if (dataType == "s")
            {
                return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index) &&
                       index >= 0 && index < sharedStrings.Length
                    ? sharedStrings[index]
                    : string.Empty;
            }

            if (dataType == "b")
            {
                return value == "1" ? bool.TrueString : bool.FalseString;
            }

            return value;
        }

        private static void ReadDimension(XmlDocument document, XmlNamespaceManager namespaceManager, ref int rowCount, ref int columnCount)
        {
            string reference = document.SelectSingleNode("//x:dimension", namespaceManager)?.Attributes?["ref"]?.Value;
            if (string.IsNullOrEmpty(reference))
            {
                return;
            }

            string lastReference = reference.Contains(":")
                ? reference.Substring(reference.LastIndexOf(':') + 1)
                : reference;
            if (TryParseCellReference(lastReference, out int row, out int column))
            {
                rowCount = Math.Max(rowCount, row);
                columnCount = Math.Max(columnCount, column);
            }
        }

        private static bool TryParseCellReference(string reference, out int row, out int column)
        {
            row = 0;
            column = 0;
            if (string.IsNullOrEmpty(reference))
            {
                return false;
            }

            Match match = CellReferenceRegex.Match(reference);
            if (!match.Success || !int.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out row))
            {
                return false;
            }

            foreach (char c in match.Groups[1].Value)
            {
                column = column * 26 + c - 'A' + 1;
            }

            return row > 0 && column > 0;
        }

        private readonly struct CellAddress : IEquatable<CellAddress>
        {
            private readonly int m_Row;
            private readonly int m_Column;

            public CellAddress(int row, int column)
            {
                m_Row = row;
                m_Column = column;
            }

            public bool Equals(CellAddress other)
            {
                return m_Row == other.m_Row && m_Column == other.m_Column;
            }

            public override bool Equals(object obj)
            {
                return obj is CellAddress other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (m_Row * 397) ^ m_Column;
                }
            }
        }
    }
}
