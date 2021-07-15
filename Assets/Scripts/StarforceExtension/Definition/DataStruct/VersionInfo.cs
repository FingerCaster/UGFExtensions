

namespace Sudoku
{
    public class VersionInfo
    {
        public bool ForceGameUpdate
        {
            get;
            set;
        }

        public string LatestGameVersion
        {
            get;
            set;
        }

        public string GameUpdateUrl
        {
            get;
            set;
        }
        public int InternalGameVersion
        {
            get;
            set;
        }
        public int InternalResourceVersion
        {
            get;
            set;
        }

        public int VersionListLength
        {
            get;
            set;
        }

        public int VersionListHashCode
        {
            get;
            set;
        }

        public int VersionListZipLength
        {
            get;
            set;
        }

        public int VersionListZipHashCode
        {
            get;
            set;
        }

    }
}
