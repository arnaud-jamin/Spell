using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public class GraphEditor : EditorWindow
    {
        // ----------------------------------------------------------------------------------------
        private readonly static float s_closeZoom = 2.0f;
        private readonly static float s_farZoom = 0.25f;
        private readonly static float s_zoomDelta = 0.1f;

        private readonly static float s_topMargin = 20;
        private readonly static float s_bottomMargin = 4;
        private readonly static float s_leftMargin = 4;
        private readonly static float s_rightMargin = 4;

        private readonly static int s_gridSize = 15;
        private readonly static int s_gridDivisions = 5;
        private readonly static Color s_gridLineColor = new Color(0, 0, 0, 0.2f);
        private readonly static Color s_gridLineLightColor = new Color(0, 0, 0, 0.1f);

        private readonly static int s_nodeHeaderHeight = 20;
        private readonly static int s_nodeFooterHeight = 5;
        private readonly static int s_nodeHandleWidth = 15;
        private readonly static int s_nodeFieldVerticalSpacing = 2;
        private readonly static int s_controlMargin = 5;
        private readonly static int s_selectionBorder = 2;
        private readonly static int s_nodePinVerticalOffset = 3;
        private readonly static Vector2 s_nodeShadowOffset = new Vector2(5, 5);
        private readonly static Vector2 s_nodePinSize = new Vector2(10, 10);

        private readonly static float s_connectionWidth = 3;
        private readonly static float s_selectedConnectionWidth = 4;
        private readonly static float s_connectionSelectionDistance = 5.0f;

        // ----------------------------------------------------------------------------------------
        private IGraph m_graph;
        private GUISkin m_darkSkin;
        private GUISkin m_lightSkin;
        private GUISkin m_skin;
        private Rect m_screenRect; 
        private Rect m_zoomRect; 
        private Rect m_viewRect;
        private Rect m_nodeBounds;
        private Matrix4x4 m_backupGuiMatrix;
        private bool m_forceRepaint;
        private NodePin? m_draggedPin;
        private INode m_selectedNode;
        private NodePin? m_selectedConnection;
        private List<NodeInfo> m_nodeInfos = new List<NodeInfo>();
        private GUIStyle m_fieldNameStyle = null;

        // ----------------------------------------------------------------------------------------
        private float ViewZoom
        {
            get { return m_graph != null ? Mathf.Clamp(m_graph.ViewZoom, s_farZoom, s_closeZoom) : 1f; }
            set { if (m_graph != null) m_graph.ViewZoom = Mathf.Clamp(value, s_farZoom, s_closeZoom); }
        }

        // ----------------------------------------------------------------------------------------
        private Vector2 ViewOffset
        {
            get { return m_graph != null ? m_graph.ViewOffset : Vector2.zero; }
            set
            {
                if (m_graph != null)
                {
                    var offset = value;

                    // pixel perfect correction
                    offset.x = Mathf.Round(offset.x); 
                    offset.y = Mathf.Round(offset.y);

                    m_graph.ViewOffset = offset;
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private bool IsAnyNodesOutOfView { get { return m_viewRect.center != m_nodeBounds.center; } }

        // ----------------------------------------------------------------------------------------
        public bool UseDarkSkin
        {
            get { return EditorPrefs.GetBool("SpellEditor.DarkSkin"); }
            set { EditorPrefs.SetBool("SpellEditor.DarkSkin", value); }
        }

        // ----------------------------------------------------------------------------------------
        public bool ShowShadow
        {
            get { return EditorPrefs.GetBool("SpellEditor.ShowShadow"); }
            set { EditorPrefs.SetBool("SpellEditor.ShowShadow", value); }
        }

        // ----------------------------------------------------------------------------------------
        [MenuItem("Window/Spell Editor")]
        public static GraphEditor ShowWindow()
        {
            return ShowWindow(Selection.activeObject as Graph);
        }

        // ----------------------------------------------------------------------------------------
        public static GraphEditor ShowWindow(Graph graph)
        {
            var spellEditor = GetWindow<GraphEditor>();
            spellEditor.Show();
            spellEditor.m_graph = graph;
            return spellEditor;
        }

        // ----------------------------------------------------------------------------------------
        void OnEnable()
        {
            var icon = (Texture)Resources.Load("SpellEditorIcon");
            titleContent = new GUIContent("Spell Editor", icon);
            minSize = new Vector2(700, 300);
            wantsMouseMove = true;

            m_lightSkin = Resources.Load("LightSkin") as GUISkin; 
            m_darkSkin = Resources.Load("DarkSkin") as GUISkin;

            EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        // ----------------------------------------------------------------------------------------
        void OnDisable()
        {
            EditorApplication.playmodeStateChanged -= OnPlayModeStateChanged;
            Selection.selectionChanged -= OnSelectionChanged;
        }

        // ----------------------------------------------------------------------------------------
        private void OnSelectionChanged()
        {
            var graph = Selection.activeObject as Graph;
            if (graph != null)
            {
                var lastEditor = focusedWindow;
                ShowWindow(graph);
                if (lastEditor)
                {
                    lastEditor.Focus();
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private void OnPlayModeStateChanged()
        {
        }

        // ----------------------------------------------------------------------------------------
        void OnGUI()
        {
            var e = Event.current;

            if (m_graph == null)
            {
                m_graph = Selection.activeObject as Graph;
                if (m_graph != null)
                {
                    m_graph.Load();
                }
                else
                {
                    EditorGUILayout.HelpBox("Please select a graph asset", MessageType.Info, true);
                    return;
                }
            }

            m_skin = UseDarkSkin ? m_darkSkin : m_lightSkin;
            GUI.skin = m_skin;
            m_fieldNameStyle = m_skin.GetStyle("NodeFieldNameLeft");

            m_screenRect = new Rect(s_leftMargin, 
                                    s_topMargin, 
                                    position.width - s_rightMargin - s_leftMargin, 
                                    position.height - s_topMargin - s_bottomMargin);

            GUI.SetNextControlName("Background");
            GUI.Box(m_screenRect, m_graph.GetType().Name, "Background");

            m_zoomRect = BeginZoom(m_screenRect);
            {
                m_viewRect = new Rect(ViewOffset, m_zoomRect.size);
                m_nodeBounds = GetNodeBounds(m_graph.Nodes, m_viewRect, true);
                DrawGrid();
                DrawConnections();
                DrawNodes();
            }
            EndZoom();

            DrawScrollBars();

            DrawMenu();

            HandleGraphEvents(e);

            if (m_forceRepaint || e.type == EventType.MouseMove)
            {
                Repaint();
            }

            if (e.type == EventType.Repaint)
            {
                m_forceRepaint = false;
            }
        }

        // ----------------------------------------------------------------------------------------
        void HandleGraphEvents(Event e)
        {
            if (mouseOverWindow == this && (e.isMouse || e.isKey))
            {
                m_forceRepaint = true;
            }

            if (e.type == EventType.ScrollWheel)
            {
                if (m_zoomRect.Contains(e.mousePosition))
                {
                    ZoomAtCursor(e.mousePosition, -e.delta.y > 0 ? s_zoomDelta : -s_zoomDelta);
                    m_forceRepaint = true;
                }
            }

            if ((e.button == 2 && e.type == EventType.MouseDrag && m_zoomRect.Contains(e.mousePosition))
                || ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.alt && e.isMouse))
            {
                ViewOffset -= e.delta / ViewZoom;
                e.Use();
            }

            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    m_selectedNode = null;
                    m_selectedConnection = null;
                }
                else if (e.button == 1)
                {
                    if (m_zoomRect.Contains(Event.current.mousePosition))
                    {
                        CreateNodeMenu(typeof(INode), ScreenToWorld(Event.current.mousePosition));
                    }
                }
            }

            if (e.type == EventType.KeyUp)
            {
                if (e.keyCode == KeyCode.Delete)
                {
                    DeleteSelection();
                    e.Use();
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        void HandleNodeEvents(INode node, Event e)
        {
            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    m_selectedNode = node;
                    m_selectedConnection = null;
                }
                else if (e.button == 1)
                {
                    CreateNodeContextMenu(node);
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private void DeleteSelection()
        {
            if (m_selectedNode != null)
            {
                DeleteNode(m_selectedNode);
            }
            else if (m_selectedConnection != null)
            {
                DeleteConnection(m_selectedConnection.Value);
            }
        }

        // ----------------------------------------------------------------------------------------
        private void DeleteConnection(NodePin connection)
        {
            m_graph.DisconnectField(connection.ownerNode, connection.field);
            if (m_selectedConnection == connection)
            {
                m_selectedConnection = null;
            }
        }

        // ----------------------------------------------------------------------------------------
        private void DeleteNode(INode node)
        {
            if (node == m_graph.Root)
                return;

            m_graph.DestroyNode(node);

            if (node == m_selectedNode)
            {
                m_selectedNode = null;
            }
            return;
        }

        // ----------------------------------------------------------------------------------------
        private void SetAsRoot(INode node)
        {
            if (m_graph.RootType.IsAssignableFrom(node.GetType()))
            {
                m_graph.Root = node;
            }
        }

        // ----------------------------------------------------------------------------------------
        void FocusPosition(Vector2 targetPos)
        {
            var targetPan = -targetPos;
            targetPan += new Vector2(m_viewRect.width / 2, m_viewRect.height / 2);
            targetPan *= ViewZoom;
        }

        // ----------------------------------------------------------------------------------------
        void ZoomAtCursor(Vector2 screenPosition, float zoomDelta)
        {
            var worldPosition = ScreenToWorld(screenPosition);
            ViewZoom += zoomDelta;
            var newScreenPosition = WorldToScreen(worldPosition);
            ViewOffset += (newScreenPosition - screenPosition) / ViewZoom;
        }

        // ----------------------------------------------------------------------------------------
        static Vector2 ScreenToWorld(Vector2 screenPos, Vector2 viewOffset, float zoom)
        {
            return (screenPos / zoom) + viewOffset;
        }

        // ----------------------------------------------------------------------------------------
        Vector2 ScreenToWorld(Vector2 screenPos)
        {
            return ScreenToWorld(screenPos, ViewOffset, ViewZoom);
        }

        // ----------------------------------------------------------------------------------------
        static Vector2 WorldToScreen(Vector2 worldPos, Vector2 viewOffset, float zoom)
        {
            return (worldPos - viewOffset) * zoom;
        }

        // ----------------------------------------------------------------------------------------
        Vector2 WorldToScreen(Vector2 worldPos)
        {
            return WorldToScreen(worldPos, ViewOffset, ViewZoom);
        }

        // ----------------------------------------------------------------------------------------
        private Rect GetNodeBounds(List<INode> nodes, Rect m_viewRect, bool v)
        {
            return new Rect(0, 0, 1500, 1500);
        }

        // ----------------------------------------------------------------------------------------
        void DrawNodes()
        {
            GUI.color = Color.white;

            BeginWindows();

            m_nodeInfos.Clear();
            for (var i = 0; i < m_graph.Nodes.Count; ++i)
            {
                var node = m_graph.Nodes[i];
                var nodeInfo = new NodeInfo(node);
                nodeInfo.baseTypeInfo = m_graph.GetBaseTypeInfo(node.GetType());
                m_nodeInfos.Add(nodeInfo);

                Vector2 nodeSize;
                ComputeNodeSize(node, out nodeSize, out nodeInfo.fieldValueMaxWidth, out nodeInfo.fieldNameMaxWidth, out nodeInfo.isMultiSided);
                nodeInfo.rect = new Rect(node.GraphPosition, nodeSize);
                nodeInfo.connectionPosition = nodeInfo.baseTypeInfo.side == NodeSide.Right ? node.GraphPosition + new Vector2(5, s_nodeHeaderHeight * 0.5f) 
                                                                                           : node.GraphPosition + new Vector2(nodeSize.x - 5, s_nodeHeaderHeight * 0.5f);

                //--------------
                // Shadow
                //--------------
                if (ShowShadow)
                { 
                    GUI.Box(new Rect(nodeInfo.rect.position, nodeInfo.rect.size + s_nodeShadowOffset), GUIContent.none, "NodeShadow");
                }

                //--------------
                // Selection
                //--------------
                if (node == m_selectedNode)
                {
                    GUI.Box(new Rect(nodeInfo.rect.position - new Vector2(s_selectionBorder, s_selectionBorder), 
                                     nodeInfo.rect.size + new Vector2(s_selectionBorder * 2, s_selectionBorder * 2)), 
                                     GUIContent.none, "NodeSelection");
                }

                //--------------
                // Window
                //--------------
                GUI.SetNextControlName("Node" + i);
                GUI.color = new Color(1, 1, 1, 0.8f);
                nodeInfo.rect = GUI.Window(i, nodeInfo.rect, DrawNode, string.Empty, node.IsFixedValue ? "ValueWindow" : "NodeWindow");
                node.GraphPosition = MathHelper.Step(nodeInfo.rect.position, Vector2.one);

                //--------------
                // Root Text
                //--------------
                if (node == m_graph.Root)
                {
                    GUI.Box(new Rect(nodeInfo.rect.position.x, nodeInfo.rect.position.y - 20, nodeInfo.rect.size.x, 20), "Root", "RootNode");
                }
            }
            EndWindows();

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
        }

        // ----------------------------------------------------------------------------------------
        Vector2 ComputeFieldSize(Type type)
        {
            if (type == typeof(float))
            {
                return new Vector2(75, 16);
            }
            else if (type == typeof(bool))
            {
                return new Vector2(75, 16);
            }
            else if (type == typeof(Vector3))
            {
                return new Vector2(150, 16);
            }
            else if (type == typeof(GameObject))
            {
                return new Vector2(100, 16);
            }
            else if (type.IsEnum)
            {
                return new Vector2(75, 16);
            }
            else
            {
                return new Vector2(100, 16);
            }
        }

        // ----------------------------------------------------------------------------------------
        void ComputeNodeSize(INode node, out Vector2 nodeSize, out float fieldValueMaxWidth, out float fieldNameMaxWidth, out bool isMultisided)
        {
            fieldNameMaxWidth = 0;
            fieldValueMaxWidth = 0;
            nodeSize = Vector2.zero;
            isMultisided = false;

            NodeSide? side = null;

            if (node.IsFixedValue)
            {
                nodeSize = ComputeFieldSize(node.ValueType) + new Vector2(2, 2);
            }
            else
            {
                var fields = node.GetFields();

                for (int i = 0; i < fields.Count; ++i)
                {
                    var field = fields[i];

                    float minWidth, maxWidth = 0;
                    m_fieldNameStyle.CalcMinMaxWidth(new GUIContent(field.Name), out minWidth, out maxWidth);
                    fieldNameMaxWidth = Mathf.Max(fieldNameMaxWidth, maxWidth);

                    var value = field.GetValue(node) as INode;
                    var type = (value != null && value.ValueType != null) ? value.ValueType : field.FieldType;
                    var fieldSize = ComputeFieldSize(type);
                    fieldValueMaxWidth = Mathf.Max(fieldValueMaxWidth, fieldSize.x);
                    nodeSize.y += fieldSize.y + s_nodeFieldVerticalSpacing;

                    var fieldSide = m_graph.GetBaseTypeInfo(field.FieldType).side;
                    if (side.HasValue)
                    {
                        if (fieldSide != side.Value)
                        {
                            isMultisided = true;
                        }
                    }
                    else
                    {
                        side = fieldSide;
                    }
                }

                if (isMultisided)
                {
                    nodeSize.x = fieldValueMaxWidth + fieldNameMaxWidth + s_nodePinSize.x * 2 + s_controlMargin * 5;
                }
                else
                {
                    nodeSize.x = fieldValueMaxWidth + fieldNameMaxWidth + s_nodePinSize.x * 1 + s_controlMargin * 4;
                }

                nodeSize.y += s_nodeHeaderHeight + s_nodeFooterHeight;
            }
        }

        // ----------------------------------------------------------------------------------------
        private void DrawFixedValue(INode node, Rect containerRect)
        {
            GUI.color = Color.white;
            var rect = new Rect(containerRect.position.x, containerRect.position.y, containerRect.width, containerRect.height);

            if (node.ValueType == typeof(bool))
            {
                node.BoxedValue = EditorGUI.Toggle(rect, (bool)node.BoxedValue, "Toggle");
            }
            else if (node.ValueType == typeof(int))
            {
                node.BoxedValue = EditorGUI.IntField(rect, GUIContent.none, (int)node.BoxedValue);
            }
            else if (node.ValueType == typeof(float))
            {
                node.BoxedValue = EditorGUI.FloatField(rect, GUIContent.none, (float)node.BoxedValue, "NodeFieldValue");
            }
            else if (node.ValueType == typeof(Vector2))
            {
                node.BoxedValue = EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)node.BoxedValue);
            }
            else if (node.ValueType == typeof(Vector3))
            {
                node.BoxedValue = Vector3Field(rect, (Vector3)node.BoxedValue, "NodeFieldValue");
            }
            else if (node.ValueType == typeof(Color))
            {
                GUI.skin = null;
                node.BoxedValue = EditorGUI.ColorField(rect, GUIContent.none, (Color)node.BoxedValue, false, true, false, new ColorPickerHDRConfig(0, 0, 0, 0));
                GUI.skin = m_skin;
            }
            if (node.ValueType == typeof(string))
            {
                node.BoxedValue = EditorGUI.TextField(rect, (string)node.BoxedValue);
            }
            else if (node.ValueType.IsEnum)
            {
                if (node.ValueType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                {
                    node.BoxedValue = EditorGUI.MaskField(rect, (int)node.BoxedValue, Enum.GetNames(node.ValueType), "NodeFieldValue");
                }
                else
                {
                    node.BoxedValue = EditorGUI.Popup(rect, (int)node.BoxedValue, Enum.GetNames(node.ValueType), "NodeFieldValue");
                }
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(node.ValueType))
            {
                node.BoxedValue = EditorGUI.ObjectField(rect, (UnityEngine.Object)node.BoxedValue, node.ValueType, false);
            }
        }

        // ----------------------------------------------------------------------------------------
        public static Vector3 Vector3Field(Rect rect, Vector3 value, GUIStyle style)
        {
            var labelWidth = 15;
            rect.width -= labelWidth * 3;
            var fieldWidth = Mathf.RoundToInt(rect.width / 3.0f);

            var x = rect.x;
            EditorGUI.LabelField(new Rect(x + 2, rect.y, labelWidth, rect.height), "X:", new GUIStyle("NodeFieldNameLeft"));
            x += labelWidth;

            value.x = EditorGUI.FloatField(new Rect(x, rect.y, fieldWidth, rect.height), GUIContent.none, value.x, "NodeFieldValue");
            x += fieldWidth;

            EditorGUI.LabelField(new Rect(x + 2, rect.y, labelWidth, rect.height), "Y:", new GUIStyle("NodeFieldNameLeft"));
            x += labelWidth;

            value.y = EditorGUI.FloatField(new Rect(x, rect.y, fieldWidth, rect.height), GUIContent.none, value.y, "NodeFieldValue");
            x += fieldWidth;

            EditorGUI.LabelField(new Rect(x + 2, rect.y, labelWidth, rect.height), "Z:", new GUIStyle("NodeFieldNameLeft"));
            x += labelWidth;

            value.z = EditorGUI.FloatField(new Rect(x, rect.y, fieldWidth, rect.height), GUIContent.none, value.z, "NodeFieldValue");
            x += fieldWidth;

            return value;
        }

        // ----------------------------------------------------------------------------------------
        void DrawNode(int id)
        {
            var e = Event.current;

            if (id >= m_nodeInfos.Count)
                return;

            var nodeInfo = m_nodeInfos[id];
            var node = nodeInfo.node;
            var nodeRect = nodeInfo.rect;

            HandleNodeEvents(node, e);

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;

            //-------------------------
            // One Parameter Node
            //-------------------------
            var fieldPosition = new Vector2(0, s_nodeHeaderHeight);
            if (node.IsFixedValue)
            {
                //-------------------------
                // Node Handle
                //-------------------------
                GUI.backgroundColor = nodeInfo.baseTypeInfo.color;
                var style = nodeInfo.baseTypeInfo.side == NodeSide.Right ? "ValueHandleLeft" : "ValueHandleRight";
                var handleRect = nodeInfo.baseTypeInfo.side == NodeSide.Right ? new Rect(0, 0, s_nodeHandleWidth, nodeInfo.rect.size.y) 
                                                                              : new Rect(nodeInfo.rect.size.x - s_nodeHandleWidth, 0, s_nodeHandleWidth, nodeInfo.rect.size.y);
                GUI.Box(handleRect, GUIContent.none, style);
                GUI.backgroundColor = Color.white;

                //-------------------------
                // Node Input
                //-------------------------
                var inputRect = nodeInfo.baseTypeInfo.side == NodeSide.Right ? new Rect(handleRect.size.x, 1, nodeInfo.rect.size.x - handleRect.size.x - 1, nodeInfo.rect.size.y - 2)
                                                                             : new Rect(1, 1, nodeInfo.rect.size.x - handleRect.size.x - 1, nodeInfo.rect.size.y - 2);
                DrawFixedValue(node, inputRect);
            }
            //-------------------------
            // Multi Parameter Node
            //-------------------------
            else
            {
                // Node Title
                GUI.backgroundColor = nodeInfo.baseTypeInfo.color;
                GUI.Box(new Rect(0, 0, nodeRect.size.x, 16), nodeInfo.derivedTypeInfo.name, "NodeTitle");
                GUI.backgroundColor = Color.white;

                // Node Fields
                nodeInfo.pins.Clear();
                var fields = node.GetFields();
                for (int i = 0; i < fields.Count; ++i)
                {
                    var field = fields[i];
                    var fieldValue = field.GetValue(node) as INode;
                    var baseTypeInfo = m_graph.GetBaseTypeInfo(field.FieldType);
                    var fieldValueNodeIndex = m_graph.Nodes.IndexOf(fieldValue);
                    var isAttached = fieldValueNodeIndex == -1;

                    var pinX = baseTypeInfo.side == NodeSide.Left ? s_controlMargin : nodeRect.size.x - s_nodePinVerticalOffset - s_nodePinSize.x;
                    var pinRect = new Rect(pinX, fieldPosition.y + s_nodePinVerticalOffset, s_nodePinSize.x, s_nodePinSize.y);

                    var pin = new NodePin
                    {
                        indexInOwner = i,
                        ownerNode = node,
                        field = field,
                        valueNode = fieldValue,
                        baseTypeInfo = baseTypeInfo,
                        isAttached = isAttached,
                        connectedNodes = new List<NodeInfo>(),
                        rect = new Rect(nodeRect.position + pinRect.position, pinRect.size),
                        center = nodeRect.position + pinRect.position + pinRect.size * 0.5f,
                    };


                    if (field.FieldType.IsAssignableFrom(typeof(INode)))
                    {
                        pin.connectedNodes.Add(m_nodeInfos[fieldValueNodeIndex]);
                    }
                    else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var connectedNodes = field.GetValue(node) as IList;
                        for (var j = 0; j < connectedNodes.Count; ++j)
                        {
                            var connectedNode = connectedNodes[j] as INode;
                            var connectedNodeIndex = m_graph.Nodes.IndexOf(connectedNode);
                            if (connectedNodeIndex != -1)
                            {
                                var connectedNodeInfo = m_nodeInfos[connectedNodeIndex];
                                pin.connectedNodes.Add(connectedNodeInfo);
                            }
                        }
                    }

                    //---------------------
                    // Field Pin
                    //---------------------
                    GUI.backgroundColor = baseTypeInfo.color;
                    GUI.Box(pinRect, GUIContent.none, "NodePin");
                    EditorGUIUtility.AddCursorRect(pinRect, MouseCursor.ArrowPlus);

                    //---------------------
                    // Field Value
                    //---------------------
                    GUI.color = Color.white;
                    GUI.backgroundColor = Color.white;
                    var fieldSize = ComputeFieldSize(pin.valueNode != null && pin.valueNode.ValueType != null ? pin.valueNode.ValueType : field.FieldType);
                    var fieldValueRect = new Rect(0, fieldPosition.y, 0, 0);
                    var hasFieldValue = false;
                    if (fieldValue != null && pin.isAttached)
                    {
                        fieldValueRect = baseTypeInfo.side == NodeSide.Left ? new Rect(pinRect.xMax + nodeInfo.fieldNameMaxWidth + s_controlMargin * 2, fieldPosition.y, nodeInfo.fieldValueMaxWidth, fieldSize.y)
                                                                            : new Rect(pinRect.xMin - s_controlMargin - nodeInfo.fieldValueMaxWidth, fieldPosition.y, nodeInfo.fieldValueMaxWidth, fieldSize.y);
                        DrawFixedValue(fieldValue, fieldValueRect);
                        hasFieldValue = true;
                    }

                    //---------------------
                    // Field Name
                    //---------------------
                    var fieldWidth = hasFieldValue ? nodeInfo.fieldValueMaxWidth + s_controlMargin : 0;
                    var nodeFieldNameStyle = baseTypeInfo.side == NodeSide.Left ? "NodeFieldNameLeft" : "NodeFieldNameRight";
                    var nodeFieldNameRect  = baseTypeInfo.side == NodeSide.Left ? new Rect(pinRect.xMax + s_controlMargin, fieldPosition.y, nodeInfo.fieldNameMaxWidth, fieldSize.y)
                                                                                : new Rect(pinRect.xMin - s_controlMargin - nodeInfo.fieldNameMaxWidth - fieldWidth, fieldPosition.y, nodeInfo.fieldNameMaxWidth, fieldSize.y);
                    GUI.Label(nodeFieldNameRect, field.Name, nodeFieldNameStyle);


                    nodeInfo.pins.Add(pin);
                    fieldPosition.y += fieldSize.y + s_nodeFieldVerticalSpacing;
                }
            }

            GUI.DragWindow();
        }

        // ----------------------------------------------------------------------------------------
        void DrawConnections()
        {
            var e = Event.current;

            var handleMouseDown = (e.type == EventType.mouseDown && e.button == 0);
            if (handleMouseDown)
            {
                m_selectedConnection = null;
                m_draggedPin = null;
            }

            var isMouseOverBlock = false;
            for (int i = 0; i < m_nodeInfos.Count; ++i)
            {
                var nodeInfo = m_nodeInfos[i];
                if (nodeInfo.rect.Contains(e.mousePosition))
                {
                    isMouseOverBlock = true;
                    break;
                }
            }

            for (int i = 0; i < m_nodeInfos.Count; ++i)
            {
                var nodeInfo = m_nodeInfos[i];

                for (int j = 0; j < nodeInfo.pins.Count; ++j)
                {
                    var pin = nodeInfo.pins[j];

                    if (handleMouseDown)
                    {
                        // Handle pin clicking to create a new connection.
                        if (pin.rect.Contains(e.mousePosition))
                        {
                            m_draggedPin = pin;
                            handleMouseDown = false;
                            e.Use();
                        }
                    }

                    if (pin.valueNode != null && pin.isAttached == false)
                    {
                        for (var k = 0; k < pin.connectedNodes.Count; ++k)
                        {
                            var connectedNodeInfo = pin.connectedNodes[k];
                            DrawConnection(pin, connectedNodeInfo.connectionPosition);

                            // Handle connection selection.
                            // Check if the mouse is over a node to prevent selecting a connection if below a block. Otherwise can't move the block
                            if (handleMouseDown && m_draggedPin == null && (isMouseOverBlock == false) && IsNearConnection(e.mousePosition, pin, connectedNodeInfo))
                            {
                                m_selectedConnection = pin;
                                m_selectedNode = null;
                                e.Use();
                            }
                        }
                    }
                }
            }

            if (m_draggedPin != null && e.type == EventType.MouseUp && e.button == 0)
            {
                var nodeAtCursor = GetNodeAtPosition(e.mousePosition);
                if (nodeAtCursor != null)
                {
                    AssignValueToPin(m_draggedPin.Value, nodeAtCursor);
                    m_draggedPin = null;
                    e.Use();
                }
                else
                {
                    var selectedPin = m_draggedPin;
                    CreateNodeMenu(m_draggedPin.Value.field.FieldType, e.mousePosition, (newNode) =>
                    {
                        selectedPin.Value.field.SetValue(selectedPin.Value.ownerNode, newNode);
                    });
                    m_draggedPin = null;
                }
            }

            if (m_draggedPin != null)
            {
                DrawConnection(m_draggedPin.Value, e.mousePosition);
            }
        }

        // ----------------------------------------------------------------------------------------
        void AssignValueToPin(NodePin pin, INode value)
        {
            var fieldType = pin.field.FieldType;

            if (fieldType.IsAssignableFrom(value.GetType()))
            {
                pin.field.SetValue(pin.ownerNode, value);
            }
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var listItemType = fieldType.GetGenericArguments()[0];
                if (typeof(INode).IsAssignableFrom(listItemType))
                {
                    var list = pin.field.GetValue(pin.ownerNode) as IList;
                    list.Add(value);

                    //fieldType.GetMethod("Add").Invoke(pin.ownerNode, new[] { node });
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        INode GetNodeAtPosition(Vector2 position)
        {
            for (int i = 0; i < m_nodeInfos.Count; ++i)
            {
                var nodeInfo = m_nodeInfos[i];
                if (nodeInfo.rect.Contains(position))
                {
                    return nodeInfo.node;
                }
            }
            return null;
        }

        // ----------------------------------------------------------------------------------------
        void GetConnectionTangents(Vector2 start, Vector2 end, out Vector2 startTangent, out Vector2 endTangent)
        {
            var tangent = new Vector2((start.x - end.x) * 0.5f, 0.0f);
            tangent = end.x > start.x ? -tangent : tangent;
            startTangent = start + tangent;
            endTangent = end - tangent;
        }

        // ----------------------------------------------------------------------------------------
        bool IsNearConnection(Vector2 cursor, NodePin pin, NodeInfo connectedNodeInfo)
        {
            var start = pin.center;
            var end = connectedNodeInfo.connectionPosition;

            if (pin.baseTypeInfo.side == NodeSide.Left)
            {
                Utils.Swap(ref start, ref end);
            }

            Vector2 startTangent, endTangent;
            GetConnectionTangents(start, end, out startTangent, out endTangent);
            var distance = HandleUtility.DistancePointBezier(cursor, start, end, startTangent, endTangent);
            return (distance <= s_connectionSelectionDistance);
        }

        // ----------------------------------------------------------------------------------------
        void DrawConnection(NodePin pin, Vector2 end)
        {
            var start = pin.center;

            if (pin.baseTypeInfo.side == NodeSide.Left)
            {
                Utils.Swap(ref start, ref end);
            }

            var color = m_selectedConnection == pin ? pin.baseTypeInfo.color.SetAlpha(1.0f) : pin.baseTypeInfo.color.SetAlpha(0.75f);
            var width = m_selectedConnection == pin ? s_selectedConnectionWidth : s_connectionWidth;
            DrawConnection(start, end, color, width);
        }
        
        // ----------------------------------------------------------------------------------------
        void DrawConnection(Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 startTangent, endTangent;
            GetConnectionTangents(start, end, out startTangent, out endTangent);
            Handles.DrawBezier(start, end, startTangent, endTangent, color, null, width);
        }

        // ----------------------------------------------------------------------------------------
        Rect BeginZoom(Rect screenRect)
        {
            GUI.EndGroup();

            m_backupGuiMatrix = GUI.matrix;
            var m1 = Matrix4x4.TRS(new Vector2(0, s_topMargin), Quaternion.identity, Vector3.one);
            var m2 = Matrix4x4.Scale(new Vector3(ViewZoom, ViewZoom, 1f));
            GUI.matrix = m1 * m2 * m1.inverse * GUI.matrix;

            var zoomRect = screenRect;
            zoomRect.size /= ViewZoom;

            GUI.BeginGroup(zoomRect);
            GUI.BeginGroup(new Rect(-ViewOffset, m_zoomRect.size + ViewOffset));

            return zoomRect;
        }

        // ----------------------------------------------------------------------------------------
        void EndZoom()
        {
            GUI.EndGroup();
            GUI.EndGroup();

            GUI.matrix = m_backupGuiMatrix;
            var screenRect = new Rect(0, s_topMargin, EditorGUIUtility.currentViewWidth, Screen.height);
            GUI.BeginGroup(screenRect);
        }

        // ----------------------------------------------------------------------------------------
        void DrawScrollBars()
        {
            if (IsAnyNodesOutOfView == false)
                return;

            var scrollPos = ViewOffset;

            var diffLeft = Mathf.Max(m_viewRect.xMin - m_nodeBounds.xMin, 0);
            var diffRight = Mathf.Max(m_nodeBounds.xMax - m_viewRect.xMax, 0);
            var barSizeX = m_nodeBounds.width - diffLeft - diffRight;
            //if (diffLeft > 0 || diffRight > 0)
            {
                var rect = new Rect(5, position.height - 18, position.width - 10, 10);
                scrollPos.x = GUI.HorizontalScrollbar(rect, scrollPos.x, barSizeX, m_nodeBounds.x, m_nodeBounds.xMax);
            }

            var diffTop = Mathf.Max(m_viewRect.yMin - m_nodeBounds.yMin, 0);
            var diffBottom = Mathf.Max(m_nodeBounds.yMax - m_viewRect.yMax, 0);
            var barSizeY = m_nodeBounds.height - diffTop - diffBottom;
            //if (diffTop > 0 || diffBottom > 0)
            {
                var rect = new Rect(position.width - 20, s_topMargin, 10, position.height - s_topMargin - 20);
                scrollPos.y = GUI.VerticalScrollbar(rect, scrollPos.y, barSizeY, m_nodeBounds.y, m_nodeBounds.yMax);
            }

            ViewOffset = scrollPos;
        }

        // ----------------------------------------------------------------------------------------
        void DrawGrid()
        {
            var backupColor = Handles.color;
            var divisions = s_gridSize * s_gridDivisions;
            var count = m_viewRect.size / ViewZoom;
            var startX = ViewOffset.x - (ViewOffset.x % s_gridSize);
            var startY = ViewOffset.y - (ViewOffset.y % s_gridSize);

            for (var x = startX; x < count.x + startX; x += s_gridSize)
            {
                Handles.color = x % divisions == 0 ? s_gridLineColor : s_gridLineLightColor;
                Handles.DrawLine(new Vector3(x, startY, 0), new Vector3(x, startY + count.y, 0));
            }

            for (var y = startY; y < count.y + startY; y += s_gridSize)
            {
                Handles.color = y % divisions == 0 ? s_gridLineColor : s_gridLineLightColor;
                Handles.DrawLine(new Vector3(startX, y, 0), new Vector3(startX + count.x, y, 0));
            }

            Handles.color = backupColor;
        }

        // ----------------------------------------------------------------------------------------
        void DrawMenu()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("File", EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Clear"), false, () => { m_graph.Clear(); });
                menu.AddItem(new GUIContent("Load"), false, () => { m_graph.Load(); });
                menu.AddItem(new GUIContent("Save"), false, () => { m_graph.Save(); });
                menu.ShowAsContext();
            }

            if (GUILayout.Button("Create", EditorStyles.toolbarDropDown))
            {
                CreateNodeMenu(typeof(INode), ScreenToWorld(m_screenRect.size * 0.5f));
            }

            if (GUILayout.Button("Options", EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
				menu.AddItem(new GUIContent("Dark Skin"), UseDarkSkin, ()=> { UseDarkSkin = !UseDarkSkin; });
				menu.AddItem(new GUIContent("Show Shadow"), ShowShadow, ()=> { ShowShadow = !ShowShadow; });
                menu.ShowAsContext();
            }

            GUILayout.FlexibleSpace();

            ViewZoom = GUILayout.HorizontalSlider(ViewZoom, s_farZoom, s_closeZoom, GUILayout.MinWidth(80));

            if (GUILayout.Button("Reset", EditorStyles.toolbarButton, new GUILayoutOption[0]))
            {
                ViewOffset = Vector2.zero;
                ViewZoom = 1.0f;
            }

            GUILayout.EndHorizontal();
        }

        // ----------------------------------------------------------------------------------------
        private GenericMenu CreateNodeContextMenu(INode node)
        {
            var menu = new GenericMenu();
            if (m_graph != null)
            {
                menu.AddItem(new GUIContent("Set as root"), false, () => { SetAsRoot(node); });
            }
            menu.ShowAsContext();
            return menu;
        }

        // ----------------------------------------------------------------------------------------
        private GenericMenu CreateNodeMenu(Type baseType, Vector2 nodeWorldPosition, Action<INode> onNodeCreated = null)
        {
            var menu = new GenericMenu();
            if (m_graph != null)
            {
                var assembly = Assembly.GetAssembly(typeof(INode));
                var types = assembly.GetTypes();
                for (var j = 0; j < types.Length; ++j)
                {
                    var nodeType = types[j];
                    if (nodeType.IsAbstract || nodeType.IsInterface || baseType.IsAssignableFrom(nodeType) == false)
                        continue;

                    var nodeInfo = NodeTypeInfo.GetNodeInfo(nodeType);
                    menu.AddItem(new GUIContent(nodeInfo.menuPath), false, (n) => 
                    {
                        var node = m_graph.CreateNode(n as Type);
                        node.GraphPosition = MathHelper.Step(nodeWorldPosition, Vector2.one);
                        node.VariableName = "pouet";
                        if (onNodeCreated != null)
                        {
                            onNodeCreated(node);
                        }
                    }, nodeType);
                }
            }
            menu.ShowAsContext();
            return menu;
        }
    }
}