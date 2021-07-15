#if  UNITY_2020_1_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace UnityInternalBridge
{
    public class ReorderableList
    {
        public ReorderableList.HeaderCallbackDelegate drawHeaderCallback;

        public ReorderableList.FooterCallbackDelegate
            drawFooterCallback = (ReorderableList.FooterCallbackDelegate) null;

        public ReorderableList.ElementCallbackDelegate drawElementCallback;
        public ReorderableList.ElementCallbackDelegate drawElementBackgroundCallback;

        public ReorderableList.DrawNoneElementCallback drawNoneElementCallback =
            (ReorderableList.DrawNoneElementCallback) null;

        public ReorderableList.ElementHeightCallbackDelegate elementHeightCallback;
        public ReorderableList.ReorderCallbackDelegateWithDetails onReorderCallbackWithDetails;
        public ReorderableList.ReorderCallbackDelegate onReorderCallback;
        public ReorderableList.SelectCallbackDelegate onSelectCallback;
        public ReorderableList.AddCallbackDelegate onAddCallback;
        public ReorderableList.AddDropdownCallbackDelegate onAddDropdownCallback;
        public ReorderableList.RemoveCallbackDelegate onRemoveCallback;
        public ReorderableList.DragCallbackDelegate onMouseDragCallback;
        public ReorderableList.SelectCallbackDelegate onMouseUpCallback;
        public ReorderableList.CanRemoveCallbackDelegate onCanRemoveCallback;
        public ReorderableList.CanAddCallbackDelegate onCanAddCallback;
        public ReorderableList.ChangedCallbackDelegate onChangedCallback;
        private int m_ActiveElement = -1;
        private float m_DragOffset = 0.0f;
        private GUISlideGroup m_SlideGroup;
        private SerializedObject m_SerializedObject;
        private SerializedProperty m_Elements;
        private IList m_ElementList;
        private bool m_Draggable;
        private float m_DraggedY;
        private bool m_Dragging;
        private List<int> m_NonDragTargetIndices;
        private bool m_DisplayHeader;
        public bool displayAdd;
        public bool displayRemove;
        private int id = -1;
        private static ReorderableList.Defaults s_Defaults;
        public float elementHeight = 21f;
        public float headerHeight = 18f;
        public float footerHeight = 13f;
        public bool showDefaultBackground = true;

        public ReorderableList(IList elements, System.Type elementType) => this.InitList((SerializedObject) null,
            (SerializedProperty) null, elements, true, true, true, true);

        public ReorderableList(
            IList elements,
            System.Type elementType,
            bool draggable,
            bool displayHeader,
            bool displayAddButton,
            bool displayRemoveButton)
        {
            this.InitList((SerializedObject) null, (SerializedProperty) null, elements, draggable, displayHeader,
                displayAddButton, displayRemoveButton);
        }

        public ReorderableList(SerializedObject serializedObject, SerializedProperty elements) =>
            this.InitList(serializedObject, elements, (IList) null, true, true, true, true);

        public ReorderableList(
            SerializedObject serializedObject,
            SerializedProperty elements,
            bool draggable,
            bool displayHeader,
            bool displayAddButton,
            bool displayRemoveButton)
        {
            this.InitList(serializedObject, elements, (IList) null, draggable, displayHeader, displayAddButton,
                displayRemoveButton);
        }

        public static ReorderableList.Defaults defaultBehaviours => ReorderableList.s_Defaults;

        private void InitList(
            SerializedObject serializedObject,
            SerializedProperty elements,
            IList elementList,
            bool draggable,
            bool displayHeader,
            bool displayAddButton,
            bool displayRemoveButton)
        {
            this.id = GUIUtility.GetPermanentControlID();
            this.m_SerializedObject = serializedObject;
            this.m_Elements = elements;
            this.m_ElementList = elementList;
            this.m_Draggable = draggable;
            this.m_Dragging = false;
            this.m_SlideGroup = new GUISlideGroup();
            this.displayAdd = displayAddButton;
            this.m_DisplayHeader = displayHeader;
            this.displayRemove = displayRemoveButton;
            if (this.m_Elements != null && !this.m_Elements.editable)
                this.m_Draggable = false;
            if (this.m_Elements == null || this.m_Elements.isArray)
                return;
            Debug.LogError((object) "Input elements should be an Array SerializedProperty");
        }

        public SerializedProperty serializedProperty
        {
            get => this.m_Elements;
            set => this.m_Elements = value;
        }

        public IList list
        {
            get => this.m_ElementList;
            set => this.m_ElementList = value;
        }

        public int index
        {
            get => this.m_ActiveElement;
            set => this.m_ActiveElement = value;
        }

        public bool draggable
        {
            get => this.m_Draggable;
            set => this.m_Draggable = value;
        }

        private Rect GetContentRect(Rect rect)
        {
            Rect rect1 = rect;
            if (this.draggable)
                rect1.xMin += 20f;
            else
                rect1.xMin += 6f;
            rect1.xMax -= 6f;
            return rect1;
        }

        private float GetElementYOffset(int index) => this.GetElementYOffset(index, -1);

        private float GetElementYOffset(int index, int skipIndex)
        {
            if (this.elementHeightCallback == null)
                return (float) index * this.elementHeight;
            float num = 0.0f;
            for (int index1 = 0; index1 < index; ++index1)
            {
                if (index1 != skipIndex)
                    num += this.elementHeightCallback(index1);
            }

            return num;
        }

        private float GetElementHeight(int index) => this.elementHeightCallback == null
            ? this.elementHeight
            : this.elementHeightCallback(index);

        private Rect GetRowRect(int index, Rect listRect) => new Rect(listRect.x,
            listRect.y + this.GetElementYOffset(index), listRect.width, this.GetElementHeight(index));

        public int count
        {
            get
            {
                if (this.m_Elements == null)
                    return this.m_ElementList.Count;
                if (!this.m_Elements.hasMultipleDifferentValues)
                    return this.m_Elements.arraySize;
                int val2 = this.m_Elements.arraySize;
                foreach (UnityEngine.Object targetObject in this.m_Elements.serializedObject.targetObjects)
                    val2 = Math.Min(
                        new SerializedObject(targetObject).FindProperty(this.m_Elements.propertyPath).arraySize, val2);
                return val2;
            }
        }

        public void DoLayoutList()
        {
            if (ReorderableList.s_Defaults == null)
                ReorderableList.s_Defaults = new ReorderableList.Defaults();
            Rect rect1 = GUILayoutUtility.GetRect(0.0f, this.headerHeight, GUILayout.ExpandWidth(true));
            Rect rect2 = GUILayoutUtility.GetRect(10f, this.GetListElementHeight(), GUILayout.ExpandWidth(true));
            Rect rect3 = GUILayoutUtility.GetRect(4f, this.footerHeight, GUILayout.ExpandWidth(true));
            this.DoListHeader(rect1);
            this.DoListElements(rect2);
            this.DoListFooter(rect3);
        }

        public void DoList(Rect rect)
        {
            if (ReorderableList.s_Defaults == null)
                ReorderableList.s_Defaults = new ReorderableList.Defaults();
            Rect headerRect = new Rect(rect.x, rect.y, rect.width, this.headerHeight);
            Rect listRect = new Rect(rect.x, headerRect.y + headerRect.height, rect.width, this.GetListElementHeight());
            Rect footerRect = new Rect(rect.x, listRect.y + listRect.height, rect.width, this.footerHeight);
            this.DoListHeader(headerRect);
            this.DoListElements(listRect);
            this.DoListFooter(footerRect);
        }

        public float GetHeight() => 0.0f + this.GetListElementHeight() + this.headerHeight + this.footerHeight;

        private float GetListElementHeight()
        {
            int count = this.count;
            if (count == 0)
                return this.elementHeight + 7f;
            return this.elementHeightCallback != null
                ? (float) ((double) this.GetElementYOffset(count - 1) + (double) this.GetElementHeight(count - 1) + 7.0)
                : (float) ((double) this.elementHeight * (double) count + 7.0);
        }

        private void DoListElements(Rect listRect)
        {
            int count = this.count;
            if (this.showDefaultBackground && Event.current.type == EventType.Repaint)
                ReorderableList.s_Defaults.boxBackground.Draw(listRect, false, false, false, false);
            listRect.yMin += 2f;
            listRect.yMax -= 5f;
            Rect rect1 = listRect;
            rect1.height = this.elementHeight;
            if ((this.m_Elements != null && this.m_Elements.isArray || this.m_ElementList != null) && count > 0)
            {
                if (this.IsDragging() && Event.current.type == EventType.Repaint)
                {
                    int rowIndex = this.CalculateRowIndex();
                    this.m_NonDragTargetIndices.Clear();
                    for (int index = 0; index < count; ++index)
                    {
                        if (index != this.m_ActiveElement)
                            this.m_NonDragTargetIndices.Add(index);
                    }

                    this.m_NonDragTargetIndices.Insert(rowIndex, -1);
                    bool flag = false;
                    for (int index = 0; index < this.m_NonDragTargetIndices.Count; ++index)
                    {
                        if (this.m_NonDragTargetIndices[index] != -1)
                        {
                            rect1.height = this.GetElementHeight(index);
                            if (this.elementHeightCallback == null)
                            {
                                rect1.y = listRect.y + this.GetElementYOffset(index, this.m_ActiveElement);
                            }
                            else
                            {
                                rect1.y = listRect.y + this.GetElementYOffset(this.m_NonDragTargetIndices[index],
                                    this.m_ActiveElement);
                                if (flag)
                                    rect1.y += this.elementHeightCallback(this.m_ActiveElement);
                            }

                            rect1 = this.m_SlideGroup.GetRect(this.m_NonDragTargetIndices[index], rect1);
                            if (this.drawElementBackgroundCallback == null)
                                ReorderableList.s_Defaults.DrawElementBackground(rect1, index, false, false,
                                    this.m_Draggable);
                            else
                                this.drawElementBackgroundCallback(rect1, index, false, false);
                            ReorderableList.s_Defaults.DrawElementDraggingHandle(rect1, index, false, false,
                                this.m_Draggable);
                            Rect contentRect = this.GetContentRect(rect1);
                            if (this.drawElementCallback == null)
                            {
                                if (this.m_Elements != null)
                                    ReorderableList.s_Defaults.DrawElement(contentRect,
                                        this.m_Elements.GetArrayElementAtIndex(this.m_NonDragTargetIndices[index]),
                                        (object) null, false, false, this.m_Draggable);
                                else
                                    ReorderableList.s_Defaults.DrawElement(contentRect, (SerializedProperty) null,
                                        this.m_ElementList[this.m_NonDragTargetIndices[index]], false, false,
                                        this.m_Draggable);
                            }
                            else
                                this.drawElementCallback(contentRect, this.m_NonDragTargetIndices[index], false, false);
                        }
                        else
                            flag = true;
                    }

                    rect1.y = this.m_DraggedY - this.m_DragOffset + listRect.y;
                    if (this.drawElementBackgroundCallback == null)
                        ReorderableList.s_Defaults.DrawElementBackground(rect1, this.m_ActiveElement, true, true,
                            this.m_Draggable);
                    else
                        this.drawElementBackgroundCallback(rect1, this.m_ActiveElement, true, true);
                    ReorderableList.s_Defaults.DrawElementDraggingHandle(rect1, this.m_ActiveElement, true, true,
                        this.m_Draggable);
                    Rect contentRect1 = this.GetContentRect(rect1);
                    if (this.drawElementCallback == null)
                    {
                        if (this.m_Elements != null)
                            ReorderableList.s_Defaults.DrawElement(contentRect1,
                                this.m_Elements.GetArrayElementAtIndex(this.m_ActiveElement), (object) null, true, true,
                                this.m_Draggable);
                        else
                            ReorderableList.s_Defaults.DrawElement(contentRect1, (SerializedProperty) null,
                                this.m_ElementList[this.m_ActiveElement], true, true, this.m_Draggable);
                    }
                    else
                        this.drawElementCallback(contentRect1, this.m_ActiveElement, true, true);
                }
                else
                {
                    for (int index = 0; index < count; ++index)
                    {
                        bool flag1 = index == this.m_ActiveElement;
                        bool flag2 = index == this.m_ActiveElement && this.HasKeyboardControl();
                        rect1.height = this.GetElementHeight(index);
                        rect1.y = listRect.y + this.GetElementYOffset(index);
                        if (this.drawElementBackgroundCallback == null)
                            ReorderableList.s_Defaults.DrawElementBackground(rect1, index, flag1, flag2,
                                this.m_Draggable);
                        else
                            this.drawElementBackgroundCallback(rect1, index, flag1, flag2);
                        ReorderableList.s_Defaults.DrawElementDraggingHandle(rect1, index, flag1, flag2,
                            this.m_Draggable);
                        Rect contentRect = this.GetContentRect(rect1);
                        if (this.drawElementCallback == null)
                        {
                            if (this.m_Elements != null)
                                ReorderableList.s_Defaults.DrawElement(contentRect,
                                    this.m_Elements.GetArrayElementAtIndex(index), (object) null, flag1, flag2,
                                    this.m_Draggable);
                            else
                                ReorderableList.s_Defaults.DrawElement(contentRect, (SerializedProperty) null,
                                    this.m_ElementList[index], flag1, flag2, this.m_Draggable);
                        }
                        else
                            this.drawElementCallback(contentRect, index, flag1, flag2);
                    }
                }

                this.DoDraggingAndSelection(listRect);
            }
            else
            {
                rect1.y = listRect.y;
                if (this.drawElementBackgroundCallback == null)
                    ReorderableList.s_Defaults.DrawElementBackground(rect1, -1, false, false, false);
                else
                    this.drawElementBackgroundCallback(rect1, -1, false, false);
                ReorderableList.s_Defaults.DrawElementDraggingHandle(rect1, -1, false, false, false);
                Rect rect2 = rect1;
                rect2.xMin += 6f;
                rect2.xMax -= 6f;
                if (this.drawNoneElementCallback == null)
                    ReorderableList.s_Defaults.DrawNoneElement(rect2, this.m_Draggable);
                else
                    this.drawNoneElementCallback(rect2);
            }
        }

        private void DoListHeader(Rect headerRect)
        {
            if (this.showDefaultBackground && Event.current.type == EventType.Repaint)
                ReorderableList.s_Defaults.DrawHeaderBackground(headerRect);
            headerRect.xMin += 6f;
            headerRect.xMax -= 6f;
            headerRect.height -= 2f;
            ++headerRect.y;
            if (this.drawHeaderCallback != null)
            {
                this.drawHeaderCallback(headerRect);
            }
            else
            {
                if (!this.m_DisplayHeader)
                    return;
                ReorderableList.s_Defaults.DrawHeader(headerRect, this.m_SerializedObject, this.m_Elements,
                    this.m_ElementList);
            }
        }

        private void DoListFooter(Rect footerRect)
        {
            if (this.drawFooterCallback != null)
            {
                this.drawFooterCallback(footerRect);
            }
            else
            {
                if (!this.displayAdd && !this.displayRemove)
                    return;
                ReorderableList.s_Defaults.DrawFooter(footerRect, this);
            }
        }

        private void DoDraggingAndSelection(Rect listRect)
        {
            Event current = Event.current;
            int activeElement1 = this.m_ActiveElement;
            bool flag = false;
            switch (current.GetTypeForControl(this.id))
            {
                case EventType.MouseDown:
                    if (listRect.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    {
                        EditorGUI.EndEditingActiveTextField();
                        this.m_ActiveElement = this.GetRowIndex(Event.current.mousePosition.y - listRect.y);
                        if (this.m_Draggable)
                        {
                            this.m_DragOffset = Event.current.mousePosition.y - listRect.y -
                                                this.GetElementYOffset(this.m_ActiveElement);
                            this.UpdateDraggedY(listRect);
                            GUIUtility.hotControl = this.id;
                            this.m_SlideGroup.Reset();
                            this.m_NonDragTargetIndices = new List<int>();
                        }

                        this.GrabKeyboardFocus();
                        current.Use();
                        flag = true;
                        break;
                    }

                    break;
                case EventType.MouseUp:
                    if (!this.m_Draggable)
                    {
                        if (this.onMouseUpCallback != null && this.IsMouseInsideActiveElement(listRect))
                        {
                            this.onMouseUpCallback(this);
                            break;
                        }

                        break;
                    }

                    if (GUIUtility.hotControl == this.id)
                    {
                        current.Use();
                        this.m_Dragging = false;
                        try
                        {
                            int rowIndex = this.CalculateRowIndex();
                            if (this.m_ActiveElement != rowIndex)
                            {
                                if (this.m_SerializedObject != null && this.m_Elements != null)
                                {
                                    this.m_Elements.MoveArrayElement(this.m_ActiveElement, rowIndex);
                                    this.m_SerializedObject.ApplyModifiedProperties();
                                    this.m_SerializedObject.Update();
                                }
                                else if (this.m_ElementList != null)
                                {
                                    object element = this.m_ElementList[this.m_ActiveElement];
                                    for (int index = 0; index < this.m_ElementList.Count - 1; ++index)
                                    {
                                        if (index >= this.m_ActiveElement)
                                            this.m_ElementList[index] = this.m_ElementList[index + 1];
                                    }

                                    for (int index = this.m_ElementList.Count - 1; index > 0; --index)
                                    {
                                        if (index > rowIndex)
                                            this.m_ElementList[index] = this.m_ElementList[index - 1];
                                    }

                                    this.m_ElementList[rowIndex] = element;
                                }

                                int activeElement2 = this.m_ActiveElement;
                                int newIndex = rowIndex;
                                this.m_ActiveElement = rowIndex;
                                if (this.onReorderCallbackWithDetails != null)
                                    this.onReorderCallbackWithDetails(this, activeElement2, newIndex);
                                else if (this.onReorderCallback != null)
                                    this.onReorderCallback(this);
                                if (this.onChangedCallback != null)
                                {
                                    this.onChangedCallback(this);
                                    break;
                                }

                                break;
                            }

                            if (this.onMouseUpCallback != null)
                                this.onMouseUpCallback(this);
                            break;
                        }
                        finally
                        {
                            GUIUtility.hotControl = 0;
                            this.m_NonDragTargetIndices = (List<int>) null;
                        }
                    }
                    else
                        break;
                case EventType.MouseDrag:
                    if (this.m_Draggable && GUIUtility.hotControl == this.id)
                    {
                        this.m_Dragging = true;
                        if (this.onMouseDragCallback != null)
                            this.onMouseDragCallback(this);
                        this.UpdateDraggedY(listRect);
                        current.Use();
                        break;
                    }

                    break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl != this.id)
                        return;
                    if (current.keyCode == KeyCode.DownArrow)
                    {
                        ++this.m_ActiveElement;
                        current.Use();
                    }

                    if (current.keyCode == KeyCode.UpArrow)
                    {
                        --this.m_ActiveElement;
                        current.Use();
                    }

                    if (current.keyCode == KeyCode.Escape && GUIUtility.hotControl == this.id)
                    {
                        GUIUtility.hotControl = 0;
                        this.m_Dragging = false;
                        current.Use();
                    }

                    this.m_ActiveElement = Mathf.Clamp(this.m_ActiveElement, 0,
                        this.m_Elements == null ? this.m_ElementList.Count - 1 : this.m_Elements.arraySize - 1);
                    break;
            }

            if (this.m_ActiveElement == activeElement1 && !flag || this.onSelectCallback == null)
                return;
            this.onSelectCallback(this);
        }

        private bool IsMouseInsideActiveElement(Rect listRect)
        {
            int rowIndex = this.GetRowIndex(Event.current.mousePosition.y - listRect.y);
            return rowIndex == this.m_ActiveElement &&
                   this.GetRowRect(rowIndex, listRect).Contains(Event.current.mousePosition);
        }

        private void UpdateDraggedY(Rect listRect) => this.m_DraggedY = Mathf.Clamp(
            Event.current.mousePosition.y - listRect.y, this.m_DragOffset,
            listRect.height - (this.GetElementHeight(this.m_ActiveElement) - this.m_DragOffset));

        private int CalculateRowIndex() => this.GetRowIndex(this.m_DraggedY);

        private int GetRowIndex(float localY)
        {
            if (this.elementHeightCallback == null)
                return Mathf.Clamp(Mathf.FloorToInt(localY / this.elementHeight), 0, this.count - 1);
            float num1 = 0.0f;
            for (int index = 0; index < this.count; ++index)
            {
                float num2 = this.elementHeightCallback(index);
                float num3 = num1 + num2;
                if ((double) localY >= (double) num1 && (double) localY < (double) num3)
                    return index;
                num1 += num2;
            }

            return this.count - 1;
        }

        private bool IsDragging() => this.m_Dragging;

        public void GrabKeyboardFocus() => GUIUtility.keyboardControl = this.id;

        public void ReleaseKeyboardFocus()
        {
            if (GUIUtility.keyboardControl != this.id)
                return;
            GUIUtility.keyboardControl = 0;
        }

        public bool HasKeyboardControl() => GUIUtility.keyboardControl == this.id;

        public delegate void HeaderCallbackDelegate(Rect rect);

        public delegate void FooterCallbackDelegate(Rect rect);

        public delegate void ElementCallbackDelegate(
            Rect rect,
            int index,
            bool isActive,
            bool isFocused);

        public delegate float ElementHeightCallbackDelegate(int index);

        public delegate void DrawNoneElementCallback(Rect rect);

        public delegate void ReorderCallbackDelegateWithDetails(
            ReorderableList list,
            int oldIndex,
            int newIndex);

        public delegate void ReorderCallbackDelegate(ReorderableList list);

        public delegate void SelectCallbackDelegate(ReorderableList list);

        public delegate void AddCallbackDelegate(ReorderableList list);

        public delegate void AddDropdownCallbackDelegate(Rect buttonRect, ReorderableList list);

        public delegate void RemoveCallbackDelegate(ReorderableList list);

        public delegate void ChangedCallbackDelegate(ReorderableList list);

        public delegate bool CanRemoveCallbackDelegate(ReorderableList list);

        public delegate bool CanAddCallbackDelegate(ReorderableList list);

        public delegate void DragCallbackDelegate(ReorderableList list);

        public class Defaults
        {
            public GUIContent iconToolbarPlus = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add to list");

            public GUIContent iconToolbarPlusMore =
                EditorGUIUtility.TrIconContent("Toolbar Plus More", "Choose to add to list");

            public GUIContent iconToolbarMinus =
                EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove selection from list");

            public readonly GUIStyle draggingHandle = (GUIStyle) "RL DragHandle";
            public readonly GUIStyle headerBackground = (GUIStyle) "RL Header";
            public readonly GUIStyle footerBackground = (GUIStyle) "RL Footer";
            public readonly GUIStyle boxBackground = (GUIStyle) "RL Background";
            public readonly GUIStyle preButton = (GUIStyle) "RL FooterButton";
            public readonly GUIStyle elementBackground = (GUIStyle) "RL Element";
            public const int padding = 6;
            public const int dragHandleWidth = 20;
            private static GUIContent s_ListIsEmpty = EditorGUIUtility.TrTextContent("List is Empty");

            public void DrawFooter(Rect rect, ReorderableList list)
            {
                float xMax = rect.xMax;
                float x = xMax - 8f;
                if (list.displayAdd)
                    x -= 25f;
                if (list.displayRemove)
                    x -= 25f;
                rect = new Rect(x, rect.y, xMax - x, rect.height);
                Rect rect1 = new Rect(x + 4f, rect.y - 3f, 25f, 13f);
                Rect position = new Rect(xMax - 29f, rect.y - 3f, 25f, 13f);
                if (Event.current.type == EventType.Repaint)
                    this.footerBackground.Draw(rect, false, false, false, false);
                if (list.displayAdd)
                {
                    using (new EditorGUI.DisabledScope(list.onCanAddCallback != null && !list.onCanAddCallback(list)))
                    {
                        if (GUI.Button(rect1,
                            list.onAddDropdownCallback == null ? this.iconToolbarPlus : this.iconToolbarPlusMore,
                            this.preButton))
                        {
                            if (list.onAddDropdownCallback != null)
                                list.onAddDropdownCallback(rect1, list);
                            else if (list.onAddCallback != null)
                                list.onAddCallback(list);
                            else
                                this.DoAddButton(list);
                            if (list.onChangedCallback != null)
                                list.onChangedCallback(list);
                        }
                    }
                }

                if (!list.displayRemove)
                    return;
                using (new EditorGUI.DisabledScope(list.index < 0 || list.index >= list.count ||
                                                   list.onCanRemoveCallback != null && !list.onCanRemoveCallback(list)))
                {
                    if (GUI.Button(position, this.iconToolbarMinus, this.preButton))
                    {
                        if (list.onRemoveCallback == null)
                            this.DoRemoveButton(list);
                        else
                            list.onRemoveCallback(list);
                        if (list.onChangedCallback != null)
                            list.onChangedCallback(list);
                    }
                }
            }

            public void DoAddButton(ReorderableList list)
            {
                if (list.serializedProperty != null)
                {
                    ++list.serializedProperty.arraySize;
                    list.index = list.serializedProperty.arraySize - 1;
                }
                else
                {
                    System.Type elementType = list.list.GetType().GetElementType();
                    if (elementType == typeof(string))
                        list.index = list.list.Add((object) "");
                    else if (elementType != null && elementType.GetConstructor(System.Type.EmptyTypes) == null)
                        Debug.LogError((object) ("Cannot add element. Type " + elementType.ToString() +
                                                 " has no default constructor. Implement a default constructor or implement your own add behaviour."));
                    else if (list.list.GetType().GetGenericArguments()[0] != null)
                        list.index =
                            list.list.Add(Activator.CreateInstance(list.list.GetType().GetGenericArguments()[0]));
                    else if (elementType != null)
                        list.index = list.list.Add(Activator.CreateInstance(elementType));
                    else
                        Debug.LogError((object) "Cannot add element of type Null.");
                }
            }

            public void DoRemoveButton(ReorderableList list)
            {
                if (list.serializedProperty != null)
                {
                    list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                    if (list.index < list.serializedProperty.arraySize - 1)
                        return;
                    list.index = list.serializedProperty.arraySize - 1;
                }
                else
                {
                    list.list.RemoveAt(list.index);
                    if (list.index >= list.list.Count - 1)
                        list.index = list.list.Count - 1;
                }
            }

            public void DrawHeaderBackground(Rect headerRect)
            {
                if (Event.current.type != EventType.Repaint)
                    return;
                this.headerBackground.Draw(headerRect, false, false, false, false);
            }

            public void DrawHeader(
                Rect headerRect,
                SerializedObject serializedObject,
                SerializedProperty element,
                IList elementList)
            {
                EditorGUI.LabelField(headerRect,
                    EditorGUIUtility.TempContent(element == null ? "IList" : "Serialized Property"));
            }

            public void DrawElementBackground(
                Rect rect,
                int index,
                bool selected,
                bool focused,
                bool draggable)
            {
                if (Event.current.type != EventType.Repaint)
                    return;
                this.elementBackground.Draw(rect, false, selected, selected, focused);
            }

            public void DrawElementDraggingHandle(
                Rect rect,
                int index,
                bool selected,
                bool focused,
                bool draggable)
            {
                if (Event.current.type != EventType.Repaint || !draggable)
                    return;
                this.draggingHandle.Draw(new Rect(rect.x + 5f, rect.y + 7f, 10f, rect.height - (rect.height - 7f)),
                    false, false, false, false);
            }

            public void DrawElement(
                Rect rect,
                SerializedProperty element,
                object listItem,
                bool selected,
                bool focused,
                bool draggable)
            {
                EditorGUI.LabelField(rect,
                    EditorGUIUtility.TempContent(element == null ? listItem.ToString() : element.displayName));
            }

            public void DrawNoneElement(Rect rect, bool draggable) =>
                EditorGUI.LabelField(rect, ReorderableList.Defaults.s_ListIsEmpty);
        }
    }
}
#endif