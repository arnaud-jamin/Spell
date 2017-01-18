using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public class GraphEditor : EditorWindow
    {
        // ----------------------------------------------------------------------------------------
        #region Settings
        // ----------------------------------------------------------------------------------------
        private readonly static float s_closeZoom = 2.0f;
        private readonly static float s_farZoom = 0.25f;
        private readonly static float s_zoomDelta = 0.1f;

        private readonly static float s_topMargin = 20;

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
        private readonly static Vector2 s_nodeShadowOffset = new Vector2(5, 5);
        private readonly static Vector2 s_nodePinOffset = new Vector2(5, 3);
        private readonly static Vector2 s_nodePinSize = new Vector2(10, 10);

        private readonly static float s_connectionWidth = 3;
        private readonly static float s_selectedConnectionWidth = 4;
        private readonly static float s_connectionSelectionDistance = 5.0f;
        private readonly static Vector2 s_connectionIndexPadding = new Vector2(4, 2);

        private readonly static string s_backgroundControlName = "Background";
        private readonly static string s_nodeControlName = "Node";
        private readonly static string s_fieldControlName = "Field";

        #endregion

        // ----------------------------------------------------------------------------------------
        #region Fields
        private IGraph m_graph;

        // Skin
        private GUISkin m_darkSkin;
        private GUISkin m_lightSkin;
        private GUISkin m_skin;
        private GUIStyle m_fieldNameStyle = null;
        private GUIStyle m_connectionIndexStyle = null;

        // View management
        private Rect m_screenRect; 
        private Rect m_zoomRect; 
        private Rect m_viewRect;
        private Rect m_nodesBounds;
        private Matrix4x4 m_backupGuiMatrix;
        private bool m_forceRepaint;

        // Persistent infos
        private List<int> m_selectedNodes = new List<int>();
        private NodePin m_draggedPin;
        private NodeConnection m_selectedConnection;
        private bool m_isDraggingSelectedNodes;
        private NodePin m_pinToRebuildIndices;

        // Info recomputed every repaint
        private Vector2 m_worldMousePosition;
        private List<NodeInfo> m_nodeInfos = new List<NodeInfo>();
        private NodeInfo m_nodeAtMousePosition;
        private NodePin m_pinAtMousePosition;
        private NodeConnection m_connectionAtMousePosition;

        // Rect Selection
        private bool m_isRectSelectionInitiated;
        private Rect m_selectionRect;
        private Vector2 m_initialSelectionPosition;

        #endregion Fields

        // ----------------------------------------------------------------------------------------
        #region Properties

        // ----------------------------------------------------------------------------------------
        private IGraph Graph
        {
            get
            {
                if (m_graph == null)
                {
                    m_graph = EditorUtility.InstanceIDToObject(EditorPrefs.GetInt("SpellEditor.GraphId")) as IGraph;
                }
                return m_graph;
            }

            set
            {
                m_graph = value;
                if (m_graph != null)
                {
                    EditorPrefs.SetInt("SpellEditor.GraphId", m_graph.GetInstanceID());
                }
            }
        }

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
        public bool ShowDebug
        {
            get { return EditorPrefs.GetBool("SpellEditor.ShowDebug"); }
            set { EditorPrefs.SetBool("SpellEditor.ShowDebug", value); }
        }

        #endregion

        #region Unity Callbacks
        // ----------------------------------------------------------------------------------------
        [MenuItem("Window/Spell Editor")]
        public static GraphEditor ShowWindow()
        {
            var editor = GetWindow<GraphEditor>();
            editor.Show();
            return editor;
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
        void OnGUI()
        {
            m_skin = UseDarkSkin ? m_darkSkin : m_lightSkin;
            GUI.skin = m_skin;
            m_fieldNameStyle = m_skin.GetStyle("NodeFieldNameLeft");
            m_connectionIndexStyle = m_skin.GetStyle("ConnectionIndex");

            Draw();
            HandleEvents();

            if (m_forceRepaint)
            {
                Repaint();
            }

            if (Event.current.type == EventType.Repaint)
            {
                m_forceRepaint = false;
            }
        }

        #endregion

        // ----------------------------------------------------------------------------------------
        #region Drawing

        // ----------------------------------------------------------------------------------------
        void Draw()
        {
            GUI.SetNextControlName(s_backgroundControlName);
            GUI.Box(new Rect(Vector2.zero, position.size), GUIContent.none, "Background");
            
            if (EditorApplication.isCompiling)
            {
                DrawMessage("Compiling...");
                return;
            }

            if (Graph == null)
            {
                DrawMessage("Please select a graph asset.");
                return;
            }

            m_screenRect = new Rect(0, s_topMargin, position.width, position.height - s_topMargin);

            CreateNodeInfos();

            m_zoomRect = BeginZoom(m_screenRect);
            {
                m_viewRect = new Rect(ViewOffset, m_zoomRect.size);

                DrawGrid();
                DrawConnections();
                DrawNodes();

                if (IsReallyRectSelecting())
                {
                    GUI.Box(m_selectionRect, GUIContent.none, "Selection");
                }
            }
            EndZoom();

            DrawScrollBars();
            DrawMainMenu();
            DrawDebug();
        }

        // ----------------------------------------------------------------------------------------
        private void DrawMessage(string message)
        {
            GUI.Box(new Rect(Vector2.zero, position.size), message, "Message");
        }

        // ----------------------------------------------------------------------------------------
        void DrawMainMenu()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("File", EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Clear"), false, () => { ShowClear(); });
                menu.AddItem(new GUIContent("New/Ability"), false, () => { ScriptableObjectHelper.CreateGraph<Ability>(); });
                menu.AddItem(new GUIContent("New/Caster"), false, () => { ScriptableObjectHelper.CreateGraph<Caster>(); });
                if (ShowDebug)
                {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Save"), false, () => { m_graph.Save(); });
                    menu.AddItem(new GUIContent("Load"), false, () => { m_graph.Load(); });
                }

                menu.ShowAsContext();
            }

            if (GUILayout.Button("Create", EditorStyles.toolbarDropDown))
            {
                CreateNodeMenu(typeof(INode), ScreenToWorld(m_screenRect.size * 0.5f));
            }

            if (GUILayout.Button("Options", EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Dark Skin"), UseDarkSkin, () => { UseDarkSkin = !UseDarkSkin; });
                menu.AddItem(new GUIContent("Show Shadow"), ShowShadow, () => { ShowShadow = !ShowShadow; });
                menu.AddItem(new GUIContent("Show Debug"), ShowDebug, () => { ShowDebug = !ShowDebug; });
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
        void DrawNodes()
        {
            GUI.color = Color.white;

            BeginWindows();

            for (var i = 0; i < m_nodeInfos.Count; ++i)
            {
                var nodeInfo = m_nodeInfos[i];

                //--------------
                // Shadow
                //--------------
                if (ShowShadow)
                {
                    GUI.Box(new Rect(nodeInfo.rect.position, nodeInfo.rect.size + s_nodeShadowOffset), GUIContent.none, "NodeShadow");
                }

                //--------------
                // Selected Nodes
                //--------------
                if (m_selectedNodes.Contains(nodeInfo.index))
                {
                    GUI.Box(new Rect(nodeInfo.rect.position - new Vector2(s_selectionBorder, s_selectionBorder),
                                     nodeInfo.rect.size + new Vector2(s_selectionBorder * 2, s_selectionBorder * 2)),
                                     GUIContent.none, "NodeSelection");
                }

                //--------------
                // Window
                //--------------
                GUI.SetNextControlName(s_nodeControlName + i);
                GUI.color = new Color(1, 1, 1, 0.8f);
                nodeInfo.rect = GUI.Window(i, nodeInfo.rect, DrawNode, string.Empty, "NodeWindow");
                if (m_isDraggingSelectedNodes == false)
                {
                    SnapNode(nodeInfo.node);
                }
                //EditorGUIUtility.AddCursorRect(nodeInfo.rect, MouseCursor.Link);

                //--------------
                // Root Text
                //--------------
                if (nodeInfo.node == m_graph.Root)
                {
                    GUI.Box(new Rect(nodeInfo.rect.position.x, nodeInfo.rect.position.y - 20, nodeInfo.rect.size.x, 20), "Root", "RootNode");
                }
            }
            EndWindows();

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
        }

        // ----------------------------------------------------------------------------------------
        private void DrawNode(int id)
        {
            var e = Event.current;

            if (id >= m_nodeInfos.Count)
                return;

            var nodeInfo = m_nodeInfos[id];

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;

            //-------------------------
            // No Pin Node
            //-------------------------
            if (nodeInfo.pins.Count == 0)
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
                DrawField(nodeInfo.node, inputRect);
            }
            //-------------------------
            // Multi Pin Node
            //-------------------------
            else
            {
                // Node Title
                GUI.backgroundColor = nodeInfo.baseTypeInfo.color;
                GUI.Box(new Rect(0, 0, nodeInfo.rect.size.x, 16), nodeInfo.derivedTypeInfo.name, "NodeTitle");
                GUI.backgroundColor = Color.white;

                // Node Fields
                for (int i = 0; i < nodeInfo.pins.Count; ++i)
                {
                    var pin = nodeInfo.pins[i];

                    //---------------------
                    // Field Pin
                    //---------------------
                    GUI.backgroundColor = pin.typeInfo.color;
                    GUI.Box(pin.pinLocalRect, GUIContent.none, "NodePin");
                    GUI.backgroundColor = Color.white;
                    GUI.Box(pin.pinLocalRect, GUIContent.none, "NodePinBorder");
                    EditorGUIUtility.AddCursorRect(pin.pinLocalRect, MouseCursor.ArrowPlus);

                    //---------------------
                    // Field Value
                    //---------------------
                    GUI.color = Color.white;
                    GUI.backgroundColor = Color.white;
                    var fieldSize = ComputeFieldSize(pin.type);
                    var fieldValueRect = new Rect(0, pin.pinLocalRect.y, 0, 0);
                    var fieldValue = pin.field.GetValue(pin.nodeInfo.node) as INode;
                    var hasFieldValue = false;
                    if (pin.isAttached && fieldValue != null)
                    {
                        fieldValueRect = pin.typeInfo.side == NodeSide.Left ? new Rect(pin.pinLocalRect.xMax + nodeInfo.fieldNameMaxWidth + s_controlMargin * 2, pin.fieldPosition.y, nodeInfo.fieldValueMaxWidth, fieldSize.y)
                                                                            : new Rect(pin.pinLocalRect.xMin - s_controlMargin - nodeInfo.fieldValueMaxWidth, pin.fieldPosition.y, nodeInfo.fieldValueMaxWidth, fieldSize.y);
                        DrawField(fieldValue, fieldValueRect);
                        hasFieldValue = true;
                    }

                    //---------------------
                    // Field Name
                    //---------------------
                    var fieldWidth = hasFieldValue ? nodeInfo.fieldValueMaxWidth + s_controlMargin : 0;
                    var nodeFieldNameStyle = pin.typeInfo.side == NodeSide.Left ? "NodeFieldNameLeft" : "NodeFieldNameRight";
                    var nodeFieldNameRect = pin.typeInfo.side == NodeSide.Left ? new Rect(pin.pinLocalRect.xMax + s_controlMargin, pin.fieldPosition.y, nodeInfo.fieldNameMaxWidth, fieldSize.y)
                                                                                : new Rect(pin.pinLocalRect.xMin - s_controlMargin - nodeInfo.fieldNameMaxWidth - fieldWidth, pin.fieldPosition.y, nodeInfo.fieldNameMaxWidth, fieldSize.y);
                    GUI.Label(nodeFieldNameRect, pin.field.Name, nodeFieldNameStyle);
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private void DrawField(INode node, Rect containerRect)
        {
            GUI.color = Color.white;
            var rect = new Rect(containerRect.position.x, containerRect.position.y, containerRect.width, containerRect.height);

            EditorGUI.BeginChangeCheck();

            if (node.ValueType == typeof(string))
            {
                GUI.SetNextControlName(s_fieldControlName);
                node.BoxedValue = EditorGUI.TextField(rect, (string)node.BoxedValue);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(node.ValueType))
            {
                node.BoxedValue = EditorGUI.ObjectField(rect, (UnityEngine.Object)node.BoxedValue, node.ValueType, false);
            }
            else if (node.BoxedValue != null)
            {
                if (node.ValueType == typeof(bool))
                {
                    node.BoxedValue = EditorGUI.Toggle(rect, (bool)node.BoxedValue, "Toggle");
                }
                else if (node.ValueType == typeof(int))
                {
                    node.BoxedValue = EditorGUI.IntField(rect, GUIContent.none, (int)node.BoxedValue, "NodeFieldValue");
                }
                else if (node.ValueType == typeof(float))
                {
                    node.BoxedValue = EditorGUI.FloatField(rect, GUIContent.none, (float)node.BoxedValue, "NodeFieldValue");
                }
                else if (node.ValueType == typeof(Vector2))
                {
                    node.BoxedValue = EditorHelper.Vector2Field(rect, (Vector2)node.BoxedValue, new GUIStyle("NodeFieldNameLeft"), new GUIStyle("NodeFieldValue"));
                }
                else if (node.ValueType == typeof(Vector3))
                {
                    node.BoxedValue = EditorHelper.Vector3Field(rect, (Vector3)node.BoxedValue, new GUIStyle("NodeFieldNameLeft"), new GUIStyle("NodeFieldValue"));
                }
                else if (node.ValueType == typeof(Color))
                {
                    GUI.skin = null;
                    node.BoxedValue = EditorGUI.ColorField(rect, GUIContent.none, (Color)node.BoxedValue, false, true, false, new ColorPickerHDRConfig(0, 0, 0, 0));
                    GUI.skin = m_skin;
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
            }

            if (EditorGUI.EndChangeCheck())
            {
                RecordAndSave("Set Node Parameter");
            }
        }

        // ----------------------------------------------------------------------------------------
        private void DrawConnections()
        {
            for (int i = 0; i < m_nodeInfos.Count; ++i)
            {
                var nodeInfo = m_nodeInfos[i];
                for (int j = 0; j < nodeInfo.pins.Count; ++j)
                {
                    var pin = nodeInfo.pins[j];
                    for (var k = 0; k < pin.connections.Count; ++k)
                    {
                        var connection = pin.connections[k];
                        DrawConnection(connection, connection.connectedNodeInfo.connectionPosition, pin.isList);
                    }
                }
            }

            if (m_draggedPin != null)
            {
                DrawConnection(new NodeConnection { pin = m_draggedPin, connectedNodeInfo = null }, Event.current.mousePosition);
            }
        }

        // ----------------------------------------------------------------------------------------
        void DrawConnection(NodeConnection connection, Vector2 end, bool drawIndex = false)
        {
            var start = connection.pin.pinGlobalRect.center;

            if (connection.pin.typeInfo.side == NodeSide.Left)
            {
                Utils.Swap(ref start, ref end);
            }

            var color = connection == m_selectedConnection ? connection.pin.typeInfo.color.NewAlpha(1.0f) : connection.pin.typeInfo.color.NewAlpha(0.75f);
            var width = connection == m_selectedConnection ? s_selectedConnectionWidth : s_connectionWidth;
            DrawConnection(start, end, color, width);

            if (drawIndex)
            {
                var content = new GUIContent(connection.index.ToString());
                float minWidth, maxWidth;
                m_connectionIndexStyle.CalcMinMaxWidth(content, out minWidth, out maxWidth);
                var height = m_connectionIndexStyle.CalcHeight(content, maxWidth);
                var size = new Vector2(maxWidth, height) + s_connectionIndexPadding * 2;
                var position = (start + (end - start) * 0.5f) - size * 0.5f;
                GUI.Box(new Rect(position, size), content, m_connectionIndexStyle);
            }
        }

        // ----------------------------------------------------------------------------------------
        void DrawConnection(Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 startTangent, endTangent;
            GetConnectionTangents(start, end, out startTangent, out endTangent);
            Handles.DrawBezier(start, end, startTangent, endTangent, color, null, width);
        }

        // ----------------------------------------------------------------------------------------
        void DrawScrollBars()
        {
            var width = m_nodesBounds.width - Mathf.Max(m_viewRect.xMin - m_nodesBounds.xMin, 0) - Mathf.Max(m_nodesBounds.xMax - m_viewRect.xMax, 0);
            var rectH = new Rect(5, position.height - 18, position.width - 10, 10);
            GUI.HorizontalScrollbar(rectH, ViewOffset.x, width, m_nodesBounds.x, m_nodesBounds.xMax);

            var height = m_nodesBounds.height - Mathf.Max(m_viewRect.yMin - m_nodesBounds.yMin, 0) - Mathf.Max(m_nodesBounds.yMax - m_viewRect.yMax, 0);
            var rectV = new Rect(position.width - 20, s_topMargin, 10, position.height - s_topMargin - 20);
            GUI.VerticalScrollbar(rectV, ViewOffset.y, height, m_nodesBounds.y, m_nodesBounds.yMax);
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
        void DrawDebug()
        {
            if (ShowDebug == false)
                return;

            var rect = new Rect(new Vector2(5, 40), new Vector2(200, 20));
            GUI.Box(rect, "Mouse:" + Event.current.mousePosition.ToString());
            rect.y += 20;

            GUI.Box(rect, "World Mouse:" + m_worldMousePosition.ToString());
            rect.y += 20;

            if (m_nodeAtMousePosition != null)
            {
                GUI.Box(rect, "Node:" + m_nodeAtMousePosition.rect.position.ToString());
                rect.y += 20;
            }

            if (m_pinAtMousePosition != null)
            {
                GUI.Box(rect, "Pin:" + m_pinAtMousePosition.pinGlobalRect.position.ToString());
                rect.y += 20;
            }

            if (m_selectedNodes != null)
            {
                GUI.Box(rect, "Selected Nodes:" + m_selectedNodes.Count.ToString());
                rect.y += 20;
            }
        }

        #endregion

        // ----------------------------------------------------------------------------------------
        #region Utility 

        // ----------------------------------------------------------------------------------------
        private void CreateNodeInfos()
        {
            // This must be done before the view modification because
            // Event.current.mousePosition is modified by the view changes.
            m_worldMousePosition = ScreenToWorld(Event.current.mousePosition);

            m_nodesBounds = new Rect();
            m_nodesBounds.xMin = float.MaxValue;
            m_nodesBounds.xMax = float.MinValue;
            m_nodesBounds.yMin = float.MaxValue;
            m_nodesBounds.yMax = float.MinValue;

            m_nodeInfos.Clear();
            for (var i = 0; i < m_graph.Nodes.Count; ++i)
            {
                var node = m_graph.Nodes[i];
                var nodeInfo = new NodeInfo();
                nodeInfo.index = i;
                nodeInfo.node = node;
                nodeInfo.derivedTypeInfo = NodeTypeInfo.GetNodeInfo(node.GetType());
                nodeInfo.baseTypeInfo = m_graph.GetTypeInfo(node.GetType());

                Vector2 nodeSize;
                ComputeNodeSize(node, out nodeSize, out nodeInfo.fieldValueMaxWidth, out nodeInfo.fieldNameMaxWidth);
                nodeInfo.rect = new Rect(node.GraphPosition, nodeSize);
                nodeInfo.connectionPosition = nodeInfo.baseTypeInfo.side == NodeSide.Right ? node.GraphPosition + new Vector2(5, s_nodeHeaderHeight * 0.5f)
                                                                                           : node.GraphPosition + new Vector2(nodeSize.x - 5, s_nodeHeaderHeight * 0.5f);

                m_nodesBounds.xMin = Mathf.Min(m_nodesBounds.xMin, nodeInfo.rect.xMin);
                m_nodesBounds.xMax = Mathf.Max(m_nodesBounds.xMax, nodeInfo.rect.xMax);
                m_nodesBounds.yMin = Mathf.Min(m_nodesBounds.yMin, nodeInfo.rect.yMin);
                m_nodesBounds.yMax = Mathf.Max(m_nodesBounds.yMax, nodeInfo.rect.yMax);

                m_nodeInfos.Add(nodeInfo);
            }

            m_nodeAtMousePosition = null;
            m_pinAtMousePosition = null;
            m_connectionAtMousePosition = null;

            for (var i = 0; i < m_nodeInfos.Count; ++i)
            {
                var nodeInfo = m_nodeInfos[i];
                var node = nodeInfo.node;

                //---------------------------------------------------------------------------------
                // Save the node where the mouse is.
                //---------------------------------------------------------------------------------
                if (nodeInfo.rect.Contains(m_worldMousePosition))
                {
                    m_nodeAtMousePosition = nodeInfo;
                }

                nodeInfo.pins.Clear();
                var fields = node.GetFields();
                var fieldPosition = new Vector2(0, s_nodeHeaderHeight);

                //---------------------------------------------------------------------------------
                // Compute node fields info and place them verticaly. Also compute the field 
                // connection infos
                //---------------------------------------------------------------------------------
                for (int j = 0; j < fields.Count; ++j)
                {
                    var field = fields[j];
                    var fieldValue = field.GetValue(node) as INode;
                    var fieldValueNodeIndex = m_graph.Nodes.IndexOf(fieldValue);
                    var isFieldList = field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>);
                    var fieldType = isFieldList ? field.FieldType.GetGenericArguments()[0] : field.FieldType;
                    var fieldTypeInfo = m_graph.GetTypeInfo(fieldType);
                    var isFieldAttached = fieldValueNodeIndex == -1;
                    var fieldSize = ComputeFieldSize(fieldType);

                    var pinX = fieldTypeInfo.side == NodeSide.Left ? s_controlMargin : nodeInfo.rect.size.x - s_nodePinSize.x - s_nodePinOffset.x;
                    var pinLocalRect = new Rect(pinX, fieldPosition.y + s_nodePinOffset.y, s_nodePinSize.x, s_nodePinSize.y);

                    var pin = new NodePin
                    {
                        index = j,
                        nodeInfo = nodeInfo,
                        field = field,
                        type = fieldType,
                        typeInfo = fieldTypeInfo,
                        isList = isFieldList,
                        isAttached = isFieldAttached && isFieldList == false,
                        connections = new List<NodeConnection>(),
                        fieldPosition = fieldPosition,
                        pinLocalRect = pinLocalRect,
                        pinGlobalRect = new Rect(nodeInfo.rect.position + pinLocalRect.position, pinLocalRect.size),
                    };
                    nodeInfo.pins.Add(pin);

                    //-----------------------------------------------------------------------------
                    // Save the pin where the mouse is.
                    //-----------------------------------------------------------------------------
                    if (pin.pinGlobalRect.Contains(m_worldMousePosition))
                    {
                        m_pinAtMousePosition = pin;
                    }

                    //-----------------------------------------------------------------------------
                    // Compute the field connection infos. If the field is a list of Node, it can 
                    // have multiple connection. It cannot have any if the field is attached.
                    //-----------------------------------------------------------------------------
                    if (pin.isAttached == false && isFieldList == false)
                    {
                        var connection = new NodeConnection() { connectedNodeInfo = m_nodeInfos[fieldValueNodeIndex], index = 0, pin = pin };
                        pin.connections.Add(connection);

                        //-------------------------------------------------------------------------
                        // Save the connection where the mouse is.
                        //-------------------------------------------------------------------------
                        if (IsNearConnection(m_worldMousePosition, connection))
                        {
                            m_connectionAtMousePosition = connection;
                        }
                    }
                    else if (isFieldList)
                    {
                        var connectedNodes = field.GetValue(node) as IList;
                        if (connectedNodes == null)
                        {
                            connectedNodes = Activator.CreateInstance(field.FieldType) as IList;
                            field.SetValue(node, connectedNodes);
                        }

                        for (var k = 0; k < connectedNodes.Count; ++k)
                        {
                            var connectedNode = connectedNodes[k] as INode;
                            var connectedNodeIndex = m_graph.Nodes.IndexOf(connectedNode);
                            if (connectedNodeIndex != -1)
                            {
                                var connection = new NodeConnection() { connectedNodeInfo = m_nodeInfos[connectedNodeIndex], index = k, pin = pin };
                                pin.connections.Add(connection);

                                //-----------------------------------------------------------------
                                // Save the connection where the mouse is.
                                //-----------------------------------------------------------------
                                if (IsNearConnection(m_worldMousePosition, connection))
                                {
                                    m_connectionAtMousePosition = connection;
                                }
                            }
                        }
                    }

                    fieldPosition.y += fieldSize.y + s_nodeFieldVerticalSpacing;
                }
            }

            if (m_draggedPin != null)
            {
                m_draggedPin = m_nodeInfos[m_draggedPin.nodeInfo.index].pins[m_draggedPin.index];
            }

            if (m_selectedConnection != null)
            {
                m_selectedConnection = m_nodeInfos[m_selectedConnection.pin.nodeInfo.index].pins[m_selectedConnection.pin.index].connections[m_selectedConnection.index];
            }

            if (m_pinToRebuildIndices != null)
            {
                var pin = m_nodeInfos[m_pinToRebuildIndices.nodeInfo.index].pins[m_pinToRebuildIndices.index];
                RebuildPinListIndices(pin);
                m_pinToRebuildIndices = null;
            }
        }

        // ----------------------------------------------------------------------------------------
        Vector2 ComputeFieldSize(Type type)
        {
            if (type == typeof(float))              { return new Vector2(75, 16); }
            else if (type == typeof(bool))          { return new Vector2(75, 16); }
            else if (type == typeof(int))           { return new Vector2(75, 16); }
            else if (type == typeof(Vector3))       { return new Vector2(150, 16); }
            else if (type == typeof(GameObject))    { return new Vector2(100, 16); }
            else if (type.IsEnum)                   { return new Vector2(75, 16); }
            else                                    { return new Vector2(100, 16); }
        }

        // ----------------------------------------------------------------------------------------
        // TODO: we don't need to request GetFields one again. See ComputeNodeInfo.
        private void ComputeNodeSize(INode node, out Vector2 nodeSize, out float fieldValueMaxWidth, out float fieldNameMaxWidth)
        {
            fieldNameMaxWidth = 0;
            fieldValueMaxWidth = 0;
            nodeSize = Vector2.zero;
            var sideCount = 1;

            NodeSide? side = null;

            var fields = node.GetFields();

            if (fields.Count == 0)
            {
                nodeSize = ComputeFieldSize(node.ValueType) + new Vector2(2, 2);
            }
            else
            {
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

                    var fieldSide = m_graph.GetTypeInfo(field.FieldType).side;
                    if (side.HasValue)
                    {
                        if (fieldSide != side.Value)
                        {
                            sideCount = 2;
                        }
                    }
                    else
                    {
                        side = fieldSide;
                    }
                }

                nodeSize.x = fieldValueMaxWidth + fieldNameMaxWidth + s_nodePinSize.x * sideCount + s_controlMargin * (3 + sideCount);
                nodeSize.y += s_nodeHeaderHeight + s_nodeFooterHeight;
            }
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
        bool IsNearConnection(Vector2 position, NodeConnection connection)
        {
            var start = connection.pin.pinGlobalRect.center;
            var end = connection.connectedNodeInfo.connectionPosition;

            if (connection.pin.typeInfo.side == NodeSide.Left)
            {
                Utils.Swap(ref start, ref end);
            }

            Vector2 startTangent, endTangent;
            GetConnectionTangents(start, end, out startTangent, out endTangent);
            var distance = HandleUtility.DistancePointBezier(position, start, end, startTangent, endTangent);
            return (distance <= s_connectionSelectionDistance);
        }

        #endregion

        // ----------------------------------------------------------------------------------------
        #region Events Management

        // ----------------------------------------------------------------------------------------
        private void OnSelectionChanged()
        {
            if (Graph != null)
            {
                Graph.Save();
            }

            Graph = Selection.activeObject as Graph;
            if (Graph != null)
            {
                var lastEditor = focusedWindow;
                ShowWindow();
                if (lastEditor != null)
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
        private void HandleEvents()
        {
            var e = Event.current;

            if (mouseOverWindow == this && (e.isMouse || e.isKey))
            {
                m_forceRepaint = true;
            }

            //-------------------------------------------------------------------------------------
            // Update selection rect and select the nodes inside
            //-------------------------------------------------------------------------------------
            if (m_isRectSelectionInitiated)
            {
                m_selectionRect.x = (m_worldMousePosition.x > m_initialSelectionPosition.x) ? m_initialSelectionPosition.x : m_worldMousePosition.x;
                m_selectionRect.y = (m_worldMousePosition.y > m_initialSelectionPosition.y) ? m_initialSelectionPosition.y : m_worldMousePosition.y;
                m_selectionRect.width = Mathf.Abs(m_worldMousePosition.x - m_initialSelectionPosition.x);
                m_selectionRect.height = Mathf.Abs(m_worldMousePosition.y - m_initialSelectionPosition.y);

                if (IsReallyRectSelecting())
                {
                    m_selectedNodes.Clear();
                    for (int i = 0; i < m_nodeInfos.Count; ++i)
                    {
                        var nodeInfo = m_nodeInfos[i];
                        if (m_selectionRect.Overlaps(nodeInfo.rect))
                        {
                            m_selectedNodes.Add(nodeInfo.index);
                        }
                    }
                }
            }

            //-------------------------------------------------------------------------------------
            // Validate Command
            //-------------------------------------------------------------------------------------
            if (e.type == EventType.ValidateCommand)
            {
                if (e.commandName == "UndoRedoPerformed")
                {
                    m_graph.Load();
                    e.Use();
                    return;
                }
            }
            //-------------------------------------------------------------------------------------
            // Mouse Down
            //-------------------------------------------------------------------------------------
            else if (e.type == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    m_selectedConnection = null;
                    m_draggedPin = null;

                    // Handle pin clicking to create a new connection.
                    if (m_pinAtMousePosition != null)
                    {
                        m_draggedPin = m_pinAtMousePosition;
                        e.Use();
                    }
                    else if (m_nodeAtMousePosition != null)
                    {
                        // Give focus to the node below the mouse to unfocus
                        // a text field inside the node for example.
                        GUI.FocusControl(s_nodeControlName + m_nodeAtMousePosition.index);

                        // Dont change the selected items if the node we try to select is already
                        // inside the selected nodes.
                        if (m_selectedNodes.Contains(m_nodeAtMousePosition.index) == false)
                        {
                            m_selectedNodes.Clear();
                            m_selectedNodes.Add(m_nodeAtMousePosition.index);
                            e.Use();
                        }
                    }
                    // Handle connection selection. Check if the mouse is over a node to prevent 
                    // selecting a connection if below a block. Otherwise can't move the block
                    else if ((m_draggedPin == null) && (m_nodeAtMousePosition == null) && m_connectionAtMousePosition != null)
                    {
                        m_selectedConnection = m_connectionAtMousePosition;
                        m_selectedNodes.Clear();
                        e.Use();
                    }
                    else if (m_isRectSelectionInitiated == false)
                    {
                        // Give focus to the background to unfocus a focused field inside a node.
                        GUI.FocusControl(s_backgroundControlName);

                        m_selectedNodes.Clear();
                        m_isRectSelectionInitiated = true;
                        m_initialSelectionPosition = m_worldMousePosition;
                        m_selectionRect = new Rect(m_worldMousePosition, Vector2.zero);

                        //e.Use();
                    }
                }
                else if (e.button == 1)
                {
                    if (m_nodeAtMousePosition != null)
                    {
                        CreateNodeContextMenu(m_nodeAtMousePosition);
                    }
                    else if (m_zoomRect.Contains(e.mousePosition))
                    {
                        CreateNodeMenu(typeof(INode), m_worldMousePosition);
                    }
                }
            }
            //-------------------------------------------------------------------------------------
            // Mouse Up
            //-------------------------------------------------------------------------------------
            // use rawType because we want to receive mouse up event even if the mouse is not
            // inside the graph editor which often hapen when doing a selection rect.
            //-------------------------------------------------------------------------------------
            else if (e.rawType == EventType.MouseUp)
            {
                if (m_isDraggingSelectedNodes)
                {
                    for (int i = 0; i < m_selectedNodes.Count; ++i)
                    {
                        var nodeInfo = m_nodeInfos[m_selectedNodes[i]];
                        SnapNode(nodeInfo.node);
                        RebuildListIndices(nodeInfo);
                    }
                    m_isDraggingSelectedNodes = false;
                    RecordAndSave("Move");
                }

                if (e.button == 0)
                {
                    if (m_isRectSelectionInitiated)
                    {
                        m_isRectSelectionInitiated = false;
                        m_selectionRect = new Rect();
                    }

                    if (m_selectionRect.width > 2 && m_selectionRect.height > 2)
                    {

                    }
                    else if (m_draggedPin != null)
                    {
                        if (m_nodeAtMousePosition != null)
                        {
                            AssignValueToPin(m_draggedPin, m_nodeAtMousePosition);
                            m_draggedPin = null;
                            e.Use();
                        }
                        else
                        {
                            var selectedPin = m_draggedPin;
                            CreateNodeMenu(m_draggedPin.type, m_worldMousePosition, (newNode) => { AssignValueToPin(selectedPin, new NodeInfo() { node = newNode }); });
                            m_draggedPin = null;
                            e.Use();
                        }
                    }
                    else
                    {
                        m_isRectSelectionInitiated = false;
                        e.Use();
                    }
                }
            }
            //-------------------------------------------------------------------------------------
            // Mouse Drag
            //-------------------------------------------------------------------------------------
            else if (e.type == EventType.MouseDrag)
            {
                if (e.button == 0)
                {
                    if (m_draggedPin == null && m_isRectSelectionInitiated == false)
                    {
                        m_isDraggingSelectedNodes = true;

                        for (int i = 0; i < m_selectedNodes.Count; ++i)
                        {
                            var nodeInfo = m_nodeInfos[m_selectedNodes[i]];
                            nodeInfo.node.GraphPosition += e.delta / ViewZoom;
                        }
                    }
                }
                else if (e.button == 2)
                {
                    ViewOffset -= e.delta / ViewZoom;
                    e.Use();
                }
            }
            //-------------------------------------------------------------------------------------
            // ScrollWheel
            //-------------------------------------------------------------------------------------
            else if (e.type == EventType.ScrollWheel)
            {
                if (m_zoomRect.Contains(e.mousePosition))
                {
                    ZoomAtCursor(e.mousePosition, -e.delta.y > 0 ? s_zoomDelta : -s_zoomDelta);
                    m_forceRepaint = true;
                }
            }
            //-------------------------------------------------------------------------------------
            // KeyUp
            //-------------------------------------------------------------------------------------
            else if (e.type == EventType.KeyUp)
            {
                if (e.keyCode == KeyCode.Delete)
                {
                    var focusedControl = GUI.GetNameOfFocusedControl();
                    if (focusedControl != s_fieldControlName)
                    {
                        DeleteSelection();
                        e.Use();
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private bool IsReallyRectSelecting()
        {
            return m_isRectSelectionInitiated && m_selectionRect.width > 3 && m_selectionRect.height > 3;
        }

        // ----------------------------------------------------------------------------------------
        private void SnapNode(INode node)
        {
            node.GraphPosition = MathHelper.Step(node.GraphPosition, Vector2.one);
        }

        #endregion

        // ----------------------------------------------------------------------------------------
        #region Graph Manipulation

        // ----------------------------------------------------------------------------------------
        private void RecordAndSave(string action)
        {
            Undo.RecordObject(m_graph as ScriptableObject, action);
            m_graph.Save();
        }

        // ----------------------------------------------------------------------------------------
        private void ShowClear()
        {
            if (EditorUtility.DisplayDialog("Clear Graph", "Are you sure you want to delete all the nodes?", "Yes", "No"))
            {
                m_graph.Clear();
                m_graph.Save();
            }
        }

        // ----------------------------------------------------------------------------------------
        private void AssignValueToPin(NodePin pin, NodeInfo newValue)
        {
            var node = pin.nodeInfo.node;
            if (node == newValue)
                return;

            pin.connections.Add(new NodeConnection() { connectedNodeInfo = newValue, index = 0, pin = pin });
            var isCreatingCycle = IsCreatingCycle(newValue, newValue);
            pin.connections.RemoveAt(pin.connections.Count - 1);

            if (isCreatingCycle)
                return;

            var fieldType = pin.field.FieldType;
            if (fieldType.IsAssignableFrom(newValue.node.GetType()))
            {
                var previousValue = pin.field.GetValue(node) as INode;
                pin.field.SetValue(node, newValue.node);

                // When we create a connection we try to retain the value
                // of the old node to the new one.
                if (previousValue.BoxedValue != null)
                {
                    newValue.node.BoxedValue = previousValue.BoxedValue;
                }
            }
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var listItemType = fieldType.GetGenericArguments()[0];
                if (typeof(INode).IsAssignableFrom(listItemType))
                {
                    var list = pin.field.GetValue(node) as IList;
                    if (list.Contains(newValue.node) == false)
                    {
                        list.Add(newValue.node);
                        m_pinToRebuildIndices = pin;
                    }
                }
            }

            RecordAndSave("Assign Selection");
        }

        // ----------------------------------------------------------------------------------------
        private bool IsCreatingCycle(NodeInfo nodeInfo, NodeInfo value)
        {
            for (int i = 0; i < nodeInfo.pins.Count; ++i)
            {
                var nodePin = nodeInfo.pins[i];
                for (var j = 0; j < nodePin.connections.Count; ++j)
                {
                    var connection = nodePin.connections[j];
                    if (connection.connectedNodeInfo == value)
                        return true;

                    if (IsCreatingCycle(connection.connectedNodeInfo, value))
                        return true;
                }
            }
            return false;
        }

        // ----------------------------------------------------------------------------------------
        private void DeleteSelection()
        {
            if (m_selectedConnection != null)
            {
                DeleteConnection(m_selectedConnection);
            }
            else
            {
                var selectedNodes = m_selectedNodes.OrderByDescending(i => i).ToList();
                for (int i = 0; i < selectedNodes.Count; ++i)
                {
                    DeleteNode(m_nodeInfos[selectedNodes[i]]);
                }
            }

            RecordAndSave("Delete Selection");
        }

        // ----------------------------------------------------------------------------------------
        private void DeleteConnection(NodeConnection connection)
        {
            var pinfield = connection.pin.field;
            var pinNode = connection.pin.nodeInfo.node;

            if (connection.pin.isList)
            {
                var list = connection.pin.field.GetValue(pinNode) as IList;
                list.Remove(connection.connectedNodeInfo.node);
            }
            else
            {
                var fieldValue = pinfield.GetValue(pinNode) as INode;
                if (fieldValue.ValueType != null)
                {
                    var newValue = m_graph.CreateFixedValue(fieldValue.ValueType);
                    pinfield.SetValue(pinNode, newValue);

                    // When we delete a connection we try to retain the value
                    // of the old node to the new one.
                    if (fieldValue.BoxedValue != null)
                    {
                        newValue.BoxedValue = fieldValue.BoxedValue;
                    }
                }
                else
                {
                    pinfield.SetValue(pinNode, null);
                }
            }

            if (m_selectedConnection == connection)
            {
                m_selectedConnection = null;
            }
        }

        // ----------------------------------------------------------------------------------------
        private void DeleteNode(NodeInfo nodeInfo)
        {
            if (nodeInfo.node == m_graph.Root)
                return;

            m_graph.Nodes.Remove(nodeInfo.node);

            for (var j = 0; j < nodeInfo.pins.Count; ++j)
            {
                var pin = nodeInfo.pins[j];
                for (var k = 0; k < pin.connections.Count; ++k)
                {
                    var connection = pin.connections[k];
                    if (connection.connectedNodeInfo == nodeInfo) 
                    {
                        DeleteConnection(connection);
                    }
                }
            }

            m_selectedNodes.Remove(nodeInfo.index);
        }

        // ----------------------------------------------------------------------------------------
        private List<NodeConnection> FindNodeReferences(NodeInfo nodeToFind)
        {
            var connections = new List<NodeConnection>();
            for (var i = 0; i < m_nodeInfos.Count; ++i)
            {
                var nodeInfo = m_nodeInfos[i];
                for (var j = 0; j < nodeInfo.pins.Count; ++j)
                {
                    var pin = nodeInfo.pins[j];
                    for (var k = 0; k < pin.connections.Count; ++k)
                    {
                        var connection = pin.connections[k];
                        if (connection.connectedNodeInfo == nodeToFind)
                        {
                            connections.Add(connection);
                        }
                    }
                }
            }

            return connections;
        }

        // ----------------------------------------------------------------------------------------
        private void RebuildListIndices(NodeInfo nodeInfo)
        {
            var references = FindNodeReferences(nodeInfo);
            for (var i = 0; i < references.Count; ++i)
            {
                var reference = references[i];
                if (reference.pin.isList)
                {
                    RebuildPinListIndices(reference.pin);
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private void RebuildPinListIndices(NodePin pin)
        {
            var list = pin.field.GetValue(pin.nodeInfo.node) as IList;

            var tempArray = new INode[list.Count];
            list.CopyTo(tempArray, 0);

            var orderedList = new List<INode>(tempArray).OrderBy(n => n.GraphPosition.y).ToList();

            list.Clear();
            for (var i = 0; i < orderedList.Count; ++i)
            {
                list.Add(orderedList[i]);
            }
        }

        // ----------------------------------------------------------------------------------------
        private void SetAsRoot(INode node)
        {
            if (m_graph.RootType != null && m_graph.RootType.IsAssignableFrom(node.GetType()))
            {
                m_graph.Root = node;
            }
            RecordAndSave("Set As Root");
        }

        #endregion

        // ----------------------------------------------------------------------------------------
        #region View Management

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
        void ZoomAtCursor(Vector2 screenPosition, float zoomDelta)
        {
            var worldPosition = m_worldMousePosition;
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

        #endregion

        // ----------------------------------------------------------------------------------------
        #region Context Menus

        // ----------------------------------------------------------------------------------------
        private GenericMenu CreateNodeContextMenu(NodeInfo nodeInfo)
        {
            var menu = new GenericMenu();
            if (m_graph != null)
            {
                if (m_graph.RootType.IsAssignableFrom(nodeInfo.node.GetType()))
                {
                    menu.AddItem(new GUIContent("Set as root"), false, () => { SetAsRoot(nodeInfo.node); });
                }
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
                var allTypes = assembly.GetTypes();
                var nodeInfos = allTypes.Where(t => baseType.IsAssignableFrom(t) && t.IsAbstract == false && t.IsInterface == false)
                                        .Select(t => NodeTypeInfo.GetNodeInfo(t))
                                        .Where(t => t.excludeFromMenu == false)
                                        .OrderBy(t => t.menuPath + "/" + t.name)
                                        .GroupBy(t => t.menuPath)
                                        .ToList();

                // TODO: collapse things like 'Action/' and 'Action/Select' to '' and 'Select'

                foreach (var menuPathGroup in nodeInfos)
                {
                    var menuPath = menuPathGroup.Key;

                    // If we have only one category, we collapse the items to the root category.
                    if (nodeInfos.Count == 1)
                    {
                        menuPath = string.Empty;
                    }

                    foreach (var nodeInfo in menuPathGroup)
                    {
                        var path = menuPath != string.Empty ? menuPath + "/" + nodeInfo.name : nodeInfo.name;
                        menu.AddItem(new GUIContent(path), false, (userData) =>
                        {
                            var node = m_graph.CreateNode(userData as Type);
                            node.GraphPosition = nodeWorldPosition;
                            SnapNode(node);
                            if (onNodeCreated != null)
                            {
                                onNodeCreated(node);
                            }
                            RecordAndSave("Create Node");
                        }, nodeInfo.type);
                    }
                }
            }
            menu.ShowAsContext();
            return menu;
        }

        // ----------------------------------------------------------------------------------------
        #endregion
    }
}
