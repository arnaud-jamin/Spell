using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public class SpellEditor : EditorWindow
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

        private readonly static int s_noddTitleHeight = 20;
        private readonly static Vector2 s_nodePinOffset = new Vector2(-6, 6);
        private readonly static Vector2 s_nodeShadowOffset = new Vector2(5, 5);

        private readonly static float s_connectionWidth = 3;
        private readonly static float s_selectedConnectionWidth = 4;
        private readonly static float s_connectionSelectionDistance = 5.0f;

        // ----------------------------------------------------------------------------------------
        private Graph m_graph;
        private GUISkin m_darkSkin;
        private GUISkin m_lightSkin;
        private Rect m_screenRect; 
        private Rect m_zoomRect; 
        private Rect m_viewRect;
        private Rect m_nodeBounds;
        private Matrix4x4 m_backupGuiMatrix;
        private bool m_forceRepaint;
        private Node.Pin? m_draggedPin;
        private Node.Pin? m_selectedConnection;

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
        [MenuItem("Window/Spell Editor")]
        public static SpellEditor ShowWindow()
        {
            return ShowWindow(Selection.activeObject as Graph);
        }

        // ----------------------------------------------------------------------------------------
        public static SpellEditor ShowWindow(Graph graph)
        {
            var spellEditor = GetWindow<SpellEditor>();
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
            if (m_graph == null)
            {
                EditorGUILayout.HelpBox("Please select a graph asset", MessageType.Info, true);
                return;
            }

            GUI.skin = UseDarkSkin ? m_darkSkin : m_lightSkin;

            var e = Event.current;

            HandleEvents(e);

            m_screenRect = new Rect(s_leftMargin, 
                                    s_topMargin, 
                                    position.width - s_rightMargin - s_leftMargin, 
                                    position.height - s_topMargin - s_bottomMargin);

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
        void HandleEvents(Event e)
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
        private Rect GetNodeBounds(List<Node> nodes, Rect m_viewRect, bool v)
        {
            return new Rect(0, 0, 1500, 1500);
        }

        // ----------------------------------------------------------------------------------------
        void DrawNodes()
        {
            GUI.color = Color.white;

            BeginWindows();
            for (var i = 0; i < m_graph.Nodes.Count; ++i)
            {
                var node = m_graph.Nodes[i];
                var rect = new Rect(node.rectPosition, new Vector2(100, 200));

                GUI.Box(new Rect(rect.position + s_nodeShadowOffset, rect.size), GUIContent.none, "NodeShadow");

                rect = GUI.Window(i, rect, DrawNode, GUIContent.none);
                node.rectPosition = MathHelper.Step(rect.position, Vector2.one);
                node.rectSize = rect.size;
            }
            EndWindows();

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
        }

        // ----------------------------------------------------------------------------------------
        void DrawNode(int id)
        {
            var e = Event.current;

            var node = m_graph.Nodes[id];
            var nodeRect = new Rect(node.rectPosition, node.rectSize);
            var nodeType = node.GetType();

            // Node Title
            var nodeInfo = NodeInfo.GetNodeInfo(nodeType);
            GUI.backgroundColor = nodeInfo.color;
            GUI.Box(new Rect(0, 0, nodeRect.width, 16), nodeType.Name, "NodeTitle");
            GUI.color = Color.white;

            // End window group to be able to draw pins outside
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(nodeRect.position, nodeRect.size + new Vector2(5, 0)));

            // Node Fields
            node.pins.Clear();
            var fieldsPosition = new Vector2(0, s_noddTitleHeight);
            var fields = nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < fields.Length; ++i)
            {
                var field = fields[i];
                if (typeof(Node).IsAssignableFrom(field.FieldType))
                {
                    var fieldInfo = NodeInfo.GetNodeInfo(field.FieldType);

                    // Field Name
                    GUI.Label(new Rect(fieldsPosition.x + s_nodePinOffset.x, fieldsPosition.y, nodeRect.width, s_noddTitleHeight), field.Name, "NodeParameter");

                    // Field Pin
                    var pinRect = new Rect(node.rectSize.x + s_nodePinOffset.x, fieldsPosition.y + s_nodePinOffset.y, 10, 10);
                    GUI.backgroundColor = fieldInfo.color;
                    GUI.Box(pinRect, GUIContent.none, "NodePin");
                    EditorGUIUtility.AddCursorRect(pinRect, MouseCursor.ArrowPlus);

                    var pin = new Node.Pin
                    {
                        index = i,
                        node = node,
                        rect = new Rect(nodeRect.position + pinRect.position, pinRect.size),
                        center = nodeRect.position + pinRect.position + pinRect.size * 0.5f,
                        color = fieldInfo.color,
                        field = field,
                    };

                    node.pins.Add(pin);

                    fieldsPosition.y += s_noddTitleHeight;
                }
            }

            GUI.EndGroup();
            GUI.BeginGroup(nodeRect);

            GUI.DragWindow();
        }

        // ----------------------------------------------------------------------------------------
        void DrawConnections()
        {
            var e = Event.current;

            var isMouseDown = (e.type == EventType.mouseDown && e.button == 0);
            if (isMouseDown)
            {
                m_selectedConnection = null;
                m_draggedPin = null;
            }
   
            for (int i = 0; i < m_graph.Nodes.Count; ++i)
            {
                var node = m_graph.Nodes[i];
                for (int j = 0; j < node.pins.Count; ++j)
                {
                    var pin = node.pins[j];

                    // Handle pin clicking to create a new connection.
                    if (isMouseDown && pin.rect.Contains(e.mousePosition))
                    {
                        m_draggedPin = pin;
                        e.Use();
                    }

                    var fieldValue = pin.field.GetValue(node) as Node;
                    if (fieldValue != null)
                    {
                        var start = pin.center;
                        var end = fieldValue.rectPosition + new Vector2(0, 10);

                        // Handle connection selection.
                        if (m_draggedPin == null && isMouseDown && IsNearConnection(e.mousePosition, start, end))
                        {
                            m_selectedConnection = pin;
                            e.Use();
                        }

                        var color = m_selectedConnection == pin ? pin.color.SetAlpha(1.0f) : pin.color.SetAlpha(0.75f); 
                        var width = m_selectedConnection == pin ? s_selectedConnectionWidth : s_connectionWidth;
                        DrawConnection(start, end, color, width);
                    }
                }
            }

            if (e.type == EventType.KeyUp && GUIUtility.keyboardControl == 0)
            {
                if (e.keyCode == KeyCode.Delete)
                {
                    if (m_selectedConnection != null)
                    {
                        m_selectedConnection.Value.field.SetValue(m_selectedConnection.Value.node, null);
                        e.Use();
                    }
                }
            }

            if (m_draggedPin != null && e.type == EventType.MouseUp && e.button == 0)
            {
                var nodeAtCursor = GetNodeAtPosition(e.mousePosition);
                if (nodeAtCursor != null)
                {
                    if (m_draggedPin.Value.field.FieldType.IsAssignableFrom(nodeAtCursor.GetType()))
                    {
                        m_draggedPin.Value.field.SetValue(m_draggedPin.Value.node, nodeAtCursor);
                        m_draggedPin = null;
                        e.Use();
                    }
                    else
                    {

                    }
                }
                else
                {
                    var selectedPin = m_draggedPin;
                    CreateNodeMenu(m_draggedPin.Value.field.FieldType, e.mousePosition, (newNode) =>
                    {
                        selectedPin.Value.field.SetValue(selectedPin.Value.node, newNode);
                    });
                    m_draggedPin = null;
                }
            }

            if (m_draggedPin != null)
            {
                DrawConnection(m_draggedPin.Value.center, e.mousePosition, m_draggedPin.Value.color, s_connectionWidth);
            }
        }

        // ----------------------------------------------------------------------------------------
        Node GetNodeAtPosition(Vector2 position)
        {
            for (int i = 0; i < m_graph.Nodes.Count; ++i)
            {
                var node = m_graph.Nodes[i];
                var nodeRect = new Rect(node.rectPosition, node.rectSize);
                if (nodeRect.Contains(position))
                {
                    return node;
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
        bool IsNearConnection(Vector2 cursor, Vector2 start, Vector2 end)
        {
            Vector2 startTangent, endTangent;
            GetConnectionTangents(start, end, out startTangent, out endTangent);
            var distance = HandleUtility.DistancePointBezier(cursor, start, end, startTangent, endTangent);
            return (distance <= s_connectionSelectionDistance);
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
                menu.AddItem(new GUIContent("Reload Skin"), false, () => 
                    {
                        m_lightSkin = Resources.Load("LightSkin") as GUISkin;
                        m_darkSkin = Resources.Load("DarkSkin") as GUISkin;
                    });

                menu.ShowAsContext();
            }

            if (GUILayout.Button("Create", EditorStyles.toolbarDropDown))
            {
                CreateNodeMenu(typeof(Node), ScreenToWorld(m_screenRect.size * 0.5f));
            }

            if (GUILayout.Button("Options", EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
				menu.AddItem (new GUIContent ("Dark Skin"), UseDarkSkin, ()=> { UseDarkSkin = !UseDarkSkin; });
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

            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                if (m_zoomRect.Contains(Event.current.mousePosition))
                {
                    CreateNodeMenu(typeof(Node), ScreenToWorld(Event.current.mousePosition));
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        private GenericMenu CreateNodeMenu(Type baseType, Vector2 nodeWorldPosition, Action<Node> onNodeCreated = null)
        {
            var menu = new GenericMenu();
            if (m_graph != null)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (var i = 0; i < assemblies.Length; ++i)
                {
                    var assembly = assemblies[i];
                    var types = assembly.GetTypes();
                    for (var j = 0; j < types.Length; ++j)
                    {
                        var nodeType = types[j];
                        if (baseType.IsAssignableFrom(nodeType) == false || nodeType.IsAbstract || nodeType.IsInterface)
                            continue;

                        var nodeInfo = NodeInfo.GetNodeInfo(nodeType);
                        menu.AddItem(new GUIContent(nodeInfo.menuPath), false, (n) => 
                        {
                            var node = m_graph.CreateNode(n as Type);
                            node.rectPosition = MathHelper.Step(nodeWorldPosition, Vector2.one);
                            if (onNodeCreated != null)
                            {
                                onNodeCreated(node);
                            }
                        }, nodeType);
                    }
                }
            }
            menu.ShowAsContext();
            return menu;
        }
    }
}