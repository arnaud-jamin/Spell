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
        private List<NodeConnection> m_connections = new List<NodeConnection>();
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
                CreateNodeMenu(m_graph.GetAllNodes(), ScreenToWorld(m_screenRect.size * 0.5f));
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
            if (nodeInfo.parameters.Count == 0)
            {
                //-------------------------
                // Node Handle
                //-------------------------
                GUI.backgroundColor = nodeInfo.color;
                var style = false ? "ValueHandleLeft" : "ValueHandleRight";
                var handleRect = false ? new Rect(0, 0, s_nodeHandleWidth, nodeInfo.rect.size.y)
                                       : new Rect(nodeInfo.rect.size.x - s_nodeHandleWidth, 0, s_nodeHandleWidth, nodeInfo.rect.size.y);
                GUI.Box(handleRect, GUIContent.none, style);
                GUI.backgroundColor = Color.white;

                //-------------------------
                // Node Input
                //-------------------------
                var inputRect = false ? new Rect(handleRect.size.x, 1, nodeInfo.rect.size.x - handleRect.size.x - 1, nodeInfo.rect.size.y - 2)
                                      : new Rect(1, 1, nodeInfo.rect.size.x - handleRect.size.x - 1, nodeInfo.rect.size.y - 2);
                //DrawField(nodeInfo.node, inputRect);
            }
            //-------------------------
            // Multi Pin Node
            //-------------------------
            else
            {
                // Node Title
                GUI.backgroundColor = nodeInfo.color;
                GUI.Box(new Rect(0, 0, nodeInfo.rect.size.x, 16), nodeInfo.name, "NodeTitle");
                GUI.backgroundColor = Color.white;

                // Node Fields
                for (int i = 0; i < nodeInfo.parameters.Count; ++i)
                {
                    var pin = nodeInfo.parameters[i];

                    //---------------------
                    // Field Pin
                    //---------------------
                    var pinStyle = "NodePinNormal";
                    var pinBorderStyle = "NodePinBorderNormal";
                    if (m_draggedPin != null && pin != m_draggedPin)
                    {
                        var enable = m_graph.CanConnectParameters(m_draggedPin.parameterIndex, pin.parameterIndex);
                        pinStyle = enable ? "NodePinEnable" : "NodePinDisable";
                        pinBorderStyle = enable ? "NodePinBorderEnable" : "NodePinBorderDisable";
                    }

                    GUI.backgroundColor = pin.color;
                    GUI.Box(pin.pinLocalRect, GUIContent.none, pinStyle);
                    GUI.backgroundColor = Color.white;
                    GUI.Box(pin.pinLocalRect, GUIContent.none, pinBorderStyle);
                    EditorGUIUtility.AddCursorRect(pin.pinLocalRect, MouseCursor.ArrowPlus);

                    //---------------------
                    // Field Value
                    //---------------------
                    var hasFieldValue = false;
                    var fieldValueRect = new Rect(0, pin.pinLocalRect.y, 0, 0);
                    var fieldSize = pin.size;

                    if (pin.parameter.Side == ParameterSide.Left)
                    {
                        GUI.color = Color.white;
                        GUI.backgroundColor = Color.white;
                        if (pin.isAttached)
                        {
                            fieldValueRect = pin.parameter.Side == ParameterSide.Left ? new Rect(pin.pinLocalRect.xMax + nodeInfo.fieldNameMaxWidth + s_controlMargin * 2, pin.fieldPosition.y, nodeInfo.fieldValueMaxWidth, fieldSize.y)
                                                                                      : new Rect(pin.pinLocalRect.xMin - s_controlMargin - nodeInfo.fieldValueMaxWidth, pin.fieldPosition.y, nodeInfo.fieldValueMaxWidth, fieldSize.y);
                            DrawField(pin.parameterIndex, fieldValueRect);
                            hasFieldValue = true;
                        }
                    }

                    //---------------------
                    // Field Name
                    //---------------------
                    var fieldWidth = hasFieldValue ? nodeInfo.fieldValueMaxWidth + s_controlMargin : 0;
                    var nodeFieldNameStyle = pin.parameter.Side == ParameterSide.Left ? "NodeFieldNameLeft" : "NodeFieldNameRight";
                    var nodeFieldNameRect = pin.parameter.Side == ParameterSide.Left ? new Rect(pin.pinLocalRect.xMax + s_controlMargin, pin.fieldPosition.y, nodeInfo.fieldNameMaxWidth, fieldSize.y)
                                                                                     : new Rect(pin.pinLocalRect.xMin - s_controlMargin - nodeInfo.fieldNameMaxWidth - fieldWidth, pin.fieldPosition.y, nodeInfo.fieldNameMaxWidth, fieldSize.y);
                    GUI.Label(nodeFieldNameRect, pin.parameter.Name, nodeFieldNameStyle);
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        //private void DrawField(INode node, Rect containerRect)
        //{
        //    EditorGUI.BeginChangeCheck();

        //    GUI.SetNextControlName(s_fieldControlName);
        //    GUI.color = Color.white;
        //    //m_graph.DrawField(node, new Rect(containerRect.position.x, containerRect.position.y, containerRect.width, containerRect.height));
        //    m_graph.DrawField(node, new Rect(containerRect.position.x, containerRect.position.y, containerRect.width, containerRect.height));

        //    GUI.skin = m_skin;

        //    if (EditorGUI.EndChangeCheck())
        //    {
        //        RecordAndSave("Set Node Parameter");
        //    }
        //}

        // ----------------------------------------------------------------------------------------
        private void DrawField(ParameterIndex p, Rect containerRect)
        {
            EditorGUI.BeginChangeCheck();

            GUI.SetNextControlName(s_fieldControlName);
            GUI.color = Color.white;
            //m_graph.DrawField(node, new Rect(containerRect.position.x, containerRect.position.y, containerRect.width, containerRect.height));
            m_graph.DrawField(p, new Rect(containerRect.position.x, containerRect.position.y, containerRect.width, containerRect.height));

            GUI.skin = m_skin;

            if (EditorGUI.EndChangeCheck())
            {
                RecordAndSave("Set Node Parameter");
            }

        }

        // ----------------------------------------------------------------------------------------
        private void DrawConnections()
        {
            for (var i = 0; i < m_connections.Count; ++i)
            {
                var connection = m_connections[i];
                var endPosition = GetConnectionEndPosition(connection);
                DrawConnection(connection, endPosition, connection.parameter.parameter.IsList);
            }

            if (m_draggedPin != null)
            {
                DrawConnection(new NodeConnection { parameter = m_draggedPin }, Event.current.mousePosition);
            }
        }

        // ----------------------------------------------------------------------------------------
        void DrawConnection(NodeConnection connection, Vector2 end, bool drawIndex = false)
        {
            var start = connection.parameter.pinGlobalRect.center;

            if (connection.parameter.parameter.Side == ParameterSide.Left)
            {
                Utils.Swap(ref start, ref end);
            }

            var color = connection == m_selectedConnection ? connection.parameter.color.NewAlpha(1.0f) : connection.parameter.color.NewAlpha(0.75f);
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

            m_connections.Clear();
            m_nodeInfos.Clear();

            for (var i = 0; i < m_graph.Nodes.Count; ++i)
            {
                var node = m_graph.Nodes[i];
                var nodeInfo = new NodeInfo();
                nodeInfo.name = node.GetType().Name;
                nodeInfo.index = i;
                nodeInfo.node = node;
                nodeInfo.color = m_graph.GetNodeColor(i);

                Vector2 nodeSize;
                ComputeNodeSize(i, node, out nodeSize, out nodeInfo.fieldValueMaxWidth, out nodeInfo.fieldNameMaxWidth);
                nodeInfo.rect = new Rect(node.GraphPosition, nodeSize);
                nodeInfo.connectionPosition = node.GraphPosition + new Vector2(nodeSize.x - 5, s_nodeHeaderHeight * 0.5f);

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

                nodeInfo.parameters.Clear();
                var parameters = node.GetParameters();
                var fieldPosition = new Vector2(0, s_nodeHeaderHeight);

                //---------------------------------------------------------------------------------
                // Compute node fields info and place them vertically. Also compute the field 
                // connection infos
                //---------------------------------------------------------------------------------
                for (int j = 0; j < parameters.Count; ++j)
                {
                    var parameter = parameters[j];
                    var connectedNodeIndex = m_graph.Nodes.IndexOf(parameter.ConnectedNode);

                    var connectedParameterIndex = -1;
                    if (connectedNodeIndex == -1)
                    {
                        GetParameterIndex(nodeInfo.node, parameter, out connectedNodeIndex, out connectedParameterIndex);
                    }

                    var isFieldAttached = connectedNodeIndex == -1;

                    var pinX = parameter.Side == ParameterSide.Left ? s_controlMargin : nodeInfo.rect.size.x - s_nodePinSize.x - s_nodePinOffset.x;
                    var pinLocalRect = new Rect(pinX, fieldPosition.y + s_nodePinOffset.y, s_nodePinSize.x, s_nodePinSize.y);

                    var parameterIndex = new ParameterIndex { nodeIndex = i, parameterIndex = j };

                    var pin = new NodePin
                    {
                        index = j,
                        nodeInfo = nodeInfo,
                        parameter = parameter,
                        isAttached = isFieldAttached && parameter.IsList == false,
                        connections = new List<NodeConnection>(),
                        fieldPosition = fieldPosition,
                        pinLocalRect = pinLocalRect,
                        pinGlobalRect = new Rect(nodeInfo.rect.position + pinLocalRect.position, pinLocalRect.size),
                        parameterIndex = parameterIndex,
                        color = m_graph.GetParameterColor(parameterIndex),
                        size = m_graph.GetParameterSize(parameterIndex)
                    };
                    nodeInfo.parameters.Add(pin);

                    //-----------------------------------------------------------------------------
                    // Save the pin where the mouse is.
                    //-----------------------------------------------------------------------------
                    if (pin.pinGlobalRect.Contains(m_worldMousePosition))
                    {
                        m_pinAtMousePosition = pin;
                    }

                    //-----------------------------------------------------------------------------
                    // Compute the field connection infos. If the field is a list of Node, it can 
                    // have multiple connections. It cannot have any if the field is attached.
                    //-----------------------------------------------------------------------------
                    if (pin.isAttached == false && parameter.IsList == false)
                    {
                        var connection = new NodeConnection()
                        {
                            parameter = pin,
                            index = 0,
                            connectedNode = connectedNodeIndex,
                            connectedParameter = connectedParameterIndex
                        };

                        pin.connections.Add(connection);
                        m_connections.Add(connection);
                    }
                    else if (parameter.IsList)
                    {
                        var connectedNodes = parameter.List;
                        for (var k = 0; k < connectedNodes.Count; ++k)
                        {
                            var connectedNode = connectedNodes[k] as INode;
                            var listConnectedNodeIndex = m_graph.Nodes.IndexOf(connectedNode);
                            if (listConnectedNodeIndex != -1)
                            {
                                var connection = new NodeConnection() { connectedNode = listConnectedNodeIndex, index = k, parameter = pin };
                                pin.connections.Add(connection);
                            }
                        }
                    }

                    fieldPosition.y += pin.size.y + s_nodeFieldVerticalSpacing;
                }
            }

            //-------------------------------------------------------------------------
            // Save the connection where the mouse is.
            //-------------------------------------------------------------------------
            for (int i = 0; i < m_connections.Count; ++i)
            {
                var connection = m_connections[i];
                if (IsNearConnection(m_worldMousePosition, connection))
                {
                    m_connectionAtMousePosition = connection;
                }
            }

            if (m_draggedPin != null)
            {
                m_draggedPin = m_nodeInfos[m_draggedPin.nodeInfo.index].parameters[m_draggedPin.index];
            }

            if (m_selectedConnection != null)
            {
                m_selectedConnection = m_nodeInfos[m_selectedConnection.parameter.nodeInfo.index].parameters[m_selectedConnection.parameter.index].connections[m_selectedConnection.index];
            }

            if (m_pinToRebuildIndices != null)
            {
                var pin = m_nodeInfos[m_pinToRebuildIndices.nodeInfo.index].parameters[m_pinToRebuildIndices.index];
                RebuildPinListIndices(pin);
                m_pinToRebuildIndices = null;
            }
        }

        // ----------------------------------------------------------------------------------------
        private void GetParameterIndex(INode source, IParameterInfo parameter, out int nodeIndex, out int parameterIndex)
        {
            nodeIndex = -1;
            parameterIndex = -1;

            for (int i = 0; i < m_nodeInfos.Count; ++i)
            {
                var nodeInfo = m_nodeInfos[i];
                if (nodeInfo.node == source)
                    continue;

                var parameters = nodeInfo.node.GetParameters();
                for (int j = 0; j < parameters.Count; ++j)
                {
                    if (parameter == parameters[j])
                    {
                        nodeIndex = i;
                        parameterIndex = j;
                        return;
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        // TODO: we don't need to request GetFields one again. See ComputeNodeInfo.
        private void ComputeNodeSize(int nodeIndex, INode node, out Vector2 nodeSize, out float fieldValueMaxWidth, out float fieldNameMaxWidth)
        {
            fieldNameMaxWidth = 0;
            fieldValueMaxWidth = 0;
            nodeSize = Vector2.zero;
            var sideCount = 1;

            ParameterSide? nodeSides = null;

            var parameters = node.GetParameters();

            if (parameters.Count == 0)
            {
                nodeSize = Vector2.zero; //m_graph.GetPrimitiveNodeSize(node) + new Vector2(2, 2);
            }
            else
            {
                for (int i = 0; i < parameters.Count; ++i)
                {
                    var parameter = parameters[i];

                    float minWidth, maxWidth = 0;
                    m_fieldNameStyle.CalcMinMaxWidth(new GUIContent(parameter.Name), out minWidth, out maxWidth);
                    fieldNameMaxWidth = Mathf.Max(fieldNameMaxWidth, maxWidth);

                    var parameterIndex = new ParameterIndex(nodeIndex, i);

                    var fieldSize = m_graph.GetParameterSize(parameterIndex);
                    fieldValueMaxWidth = Mathf.Max(fieldValueMaxWidth, fieldSize.x);
                    nodeSize.y += fieldSize.y + s_nodeFieldVerticalSpacing;

                    var parameterSide = parameter.Side;
                    if (nodeSides.HasValue)
                    {
                        if (parameterSide != nodeSides.Value)
                        {
                            //sideCount = 2;
                        }
                    }
                    else
                    {
                        nodeSides = parameterSide;
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
        INode GetConnectedNode(NodeConnection connection)
        {
            if (connection.connectedParameter != -1)
            {
                var nodeInfo = m_nodeInfos[connection.connectedNode];
                var parameter = nodeInfo.parameters[connection.connectedParameter];
                return parameter.parameter.ConnectedNode;
            }
            else if (connection.connectedNode != -1)
            {
                var nodeInfo = m_nodeInfos[connection.connectedNode];
                return nodeInfo.node;
            }

            return null;
        }

        // ----------------------------------------------------------------------------------------
        Vector2 GetConnectionEndPosition(NodeConnection connection)
        {
            if (connection.connectedParameter != -1)
            {
                var nodeInfo = m_nodeInfos[connection.connectedNode];
                var parameter = nodeInfo.parameters[connection.connectedParameter];
                return parameter.pinGlobalRect.center;
            }
            else if (connection.connectedNode != -1)
            {
                var nodeInfo = m_nodeInfos[connection.connectedNode];
                return nodeInfo.connectionPosition;
            }

            return Vector3.zero;
        }

        // ----------------------------------------------------------------------------------------
        bool IsNearConnection(Vector2 position, NodeConnection connection)
        {
            var start = connection.parameter.pinGlobalRect.center;
            var end = GetConnectionEndPosition(connection);

            if (connection.parameter.parameter.Side == ParameterSide.Left)
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
                        CreateNodeMenu(m_graph.GetAllNodes(), m_worldMousePosition);
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

                    if (IsReallyRectSelecting())
                    {

                    }
                    else if (m_draggedPin != null)
                    {
                        if (m_pinAtMousePosition != null && m_pinAtMousePosition != m_draggedPin)
                        {
                            ConnectParameterToParameter(m_draggedPin, m_pinAtMousePosition);
                            m_draggedPin = null;
                            e.Use();
                        }
                        else if (m_nodeAtMousePosition != null)
                        {
                            ConnectParameterToNode(m_draggedPin, m_nodeAtMousePosition);
                            m_draggedPin = null;
                            e.Use();
                        }
                        else
                        {
                            var selectedPin = m_draggedPin;
                            CreateNodeMenu(m_graph.GetAssignableNodes(selectedPin.parameterIndex), m_worldMousePosition, 
                                           newNode => { ConnectParameterToNode(selectedPin, new NodeInfo() { node = newNode }); });
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
        private void ConnectParameterToParameter(NodePin source, NodePin dest)
        {
            if (m_graph.ConnectParameters(source.parameterIndex, dest.parameterIndex))
            {
                m_pinToRebuildIndices = source;
            }
            RecordAndSave("Connect");
        }


        // ----------------------------------------------------------------------------------------
        private void ConnectParameterToNode(NodePin parameter, NodeInfo value)
        {
            //if (parameter.parameter.ConnectToNode(value.node))
            //{
            //    m_pinToRebuildIndices = parameter;
            //}
            //RecordAndSave("Connect");
        }

        // ----------------------------------------------------------------------------------------
        //private bool IsCreatingCycle(NodeInfo nodeInfo, NodeInfo value)
        //{
        //    for (int i = 0; i < nodeInfo.pins.Count; ++i)
        //    {
        //        var nodePin = nodeInfo.pins[i];
        //        for (var j = 0; j < nodePin.connections.Count; ++j)
        //        {
        //            var connection = nodePin.connections[j];
        //            if (connection.connectedNodeInfo == value)
        //                return true;

        //            if (IsCreatingCycle(connection.connectedNodeInfo, value))
        //                return true;
        //        }
        //    }
        //    return false;
        //}

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
            var parameter = connection.parameter.parameter;
            var node = GetConnectedNode(connection);
            m_graph.Disconnect(connection.parameter.parameterIndex, new ParameterIndex());

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

            var references = FindNodeReferences(nodeInfo);
            for (var j = 0; j < references.Count; ++j)
            {
                var reference = references[j];
                DeleteConnection(reference);
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
                for (var j = 0; j < nodeInfo.parameters.Count; ++j)
                {
                    var pin = nodeInfo.parameters[j];
                    for (var k = 0; k < pin.connections.Count; ++k)
                    {
                        var connection = pin.connections[k];
                        if (connection.connectedNode == nodeToFind.index)
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
                if (reference.parameter.parameter.IsList)
                {
                    RebuildPinListIndices(reference.parameter);
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private void RebuildPinListIndices(NodePin pin)
        {
            var list = pin.parameter.List;
            if (list == null)
                return;

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
        private GenericMenu CreateNodeMenu(List<NodeTypeInfo> nodeInfos, Vector2 nodeWorldPosition, Action<INode> onNodeCreated = null)
        {
            var menu = new GenericMenu();
            foreach (var nodeInfo in nodeInfos)
            {
                var path = nodeInfo.menuPath != string.Empty ? nodeInfo.menuPath + "/" + nodeInfo.name : nodeInfo.name;
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
            menu.ShowAsContext();
            return menu;
        }

        // ----------------------------------------------------------------------------------------
        #endregion
    }
}
