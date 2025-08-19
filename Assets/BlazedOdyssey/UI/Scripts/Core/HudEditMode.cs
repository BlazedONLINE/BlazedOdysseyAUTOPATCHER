#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace BlazedOdyssey.UI
{
    /// F2 toggles edit mode. Left-drag moves any HudMovable under the cursor.
    /// Ctrl+S saves, Ctrl+R resets. Snaps to grid. Works in Overlay or Camera canvas.
    /// Only works in gameplay scenes (maps/dungeons), not in character selection.
    public class HudEditMode : MonoBehaviour
    {
        [Header("Hotkeys")]
        public KeyCode toggleKey = KeyCode.F2;
        [Tooltip("Require Alt to drag. Disable if you just want click-drag.")]
        public bool requireAltToDrag = false;

        [Header("Snap")]
        [Range(1, 32)] public int snapPixels = 8;
        
        [Header("Scene Control")]
        public bool onlyAllowInGameplayScenes = true; // Restored to true for proper functionality
        [Tooltip("Allow editing even in non-gameplay scenes")]
        public bool allowEditingAnywhere = false;

        static readonly Color kPanel = new(0,0,0,0.35f);
        static readonly Color kHi    = new(1,1,1,0.9f);

        public bool _editing;
        public bool IsEditing => _editing;
        
        public void ExitEditMode()
        {
            if (_editing)
            {
                _editing = false;
                Debug.Log("[HUD] Edit mode disabled externally");
            }
        }
        Camera? _uiCam;
        Canvas _canvas = default!;
        HudMovable? _hover;
        HudMovable? _drag;
        Vector2 _startAnchored;
        Vector2 _startMouseLocal;
        Vector2 _startSize;
        Dictionary<string, Vector2> _positions = new();
        Dictionary<string, HudTransformData> _transforms = new();
        
        bool _isResizing = false;
        enum ResizeHandle { None, BottomRight, TopLeft, TopRight, BottomLeft }
        ResizeHandle _currentHandle = ResizeHandle.None;

        void Awake()
        {
            _canvas = GetComponentInParent<Canvas>(true);
            _uiCam  = ResolveUiCamera(_canvas);
        }

        void Start()
        {
            // Apply saved positions after all HudMovable components have been initialized
            ForceApplySavedLayout();
        }

        Camera? ResolveUiCamera(Canvas c)
        {
            if (!c) return Camera.main;
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) return null;
            return c.worldCamera ? c.worldCamera : Camera.main;
        }

        /// <summary>
        /// Public method to force-apply saved layout to all HudMovables
        /// </summary>
        public static void ForceApplySavedLayout()
        {
            Debug.Log("[HUD] ForceApplySavedLayout called");
            
            // Try to load complete transform data first
            if (HudLayoutStore.LoadComplete(out var transformMap))
            {
                Debug.Log($"[HUD] Loaded complete layout with {transformMap.Count} saved transforms");
                foreach (var kv in transformMap)
                {
                    Debug.Log($"[HUD] Saved transform: {kv.Key} = pos:{kv.Value.position}, size:{kv.Value.size}");
                }
                
                var hudMovables = FindObjectsOfType<HudMovable>(true);
                Debug.Log($"[HUD] Found {hudMovables.Length} HudMovable components");
                int applied = 0;
                
                foreach (var mv in hudMovables)
                {
                    // Ensure the Rect is properly assigned
                    mv.EnsureRect();
                    
                    Debug.Log($"[HUD] Checking element: {mv.Id} (active: {mv.gameObject.activeInHierarchy})");
                    
                    if (mv.Rect != null && transformMap.TryGetValue(mv.Id, out var transform))
                    {
                        mv.Rect.anchoredPosition = transform.position;
                        if (transform.size != Vector2.zero && transform.size.x > 0 && transform.size.y > 0)
                        {
                            mv.Rect.sizeDelta = transform.size;
                        }
                        applied++;
                    }
                    else
                    {
                        Debug.Log($"[HUD] No saved transform found for {mv.Id}");
                    }
                }
                
                Debug.Log($"[HUD] Applied saved layout to {applied}/{hudMovables.Length} elements");
            }
            // Fallback to old position-only loading
            else if (HudLayoutStore.Load(out var positionMap))
            {
                Debug.Log($"[HUD] Loaded legacy position-only layout with {positionMap.Count} saved positions");
                
                var hudMovables = FindObjectsOfType<HudMovable>(true);
                int applied = 0;
                
                foreach (var mv in hudMovables)
                {
                    mv.EnsureRect();
                    
                    if (mv.Rect != null && positionMap.TryGetValue(mv.Id, out var p))
                    {
                        Debug.Log($"[HUD] Applying legacy position {p} to {mv.Id}");
                        mv.Rect.anchoredPosition = p;
                        applied++;
                    }
                }
                
                Debug.Log($"[HUD] Applied legacy layout to {applied}/{hudMovables.Length} elements");
            }
            else
            {
                Debug.Log("[HUD] No saved layout found or failed to load");
            }
        }

        bool CanEditInCurrentScene()
        {
            if (!onlyAllowInGameplayScenes || allowEditingAnywhere) return true;
            return SceneStateDetector.ShouldAllowHUDEditing();
        }

        void Update()
        {
            // Check if we can edit in the current scene
            if (!CanEditInCurrentScene())
            {
                if (_editing)
                {
                    _editing = false;
                    Debug.Log("[HUD] Edit mode disabled - not in gameplay scene");
                }
                return;
            }

            // toggle
            if (Input.GetKeyDown(toggleKey))
            {
                _editing = !_editing;
                Debug.Log($"[HUD] Edit Mode: {(_editing ? "ON" : "OFF")}");
                
                if (_editing)
                {
                    // Log available HUD elements for debugging
                    var hudMovables = FindObjectsOfType<HudMovable>(true);
                    Debug.Log($"[HUD] Found {hudMovables.Length} HudMovable elements:");
                    foreach (var hm in hudMovables)
                    {
                        Debug.Log($"  - {hm.Id} on {hm.gameObject.name} (active: {hm.gameObject.activeInHierarchy})");
                    }
                }
            }

            if (!_editing) return;

            // save/reset - using Ctrl+L to avoid Unity Ctrl+S conflict
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.L))
                {
                    Debug.Log("[HUD] Ctrl+L pressed - attempting to save layout");
                    CaptureAllTransforms();
                    Debug.Log($"[HUD] Captured {_transforms.Count} element transforms");
                    HudLayoutStore.SaveComplete(_transforms);
                    Debug.Log("[HUD] Layout saved manually with Ctrl+L");
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    _transforms.Clear();
                    HudLayoutStore.SaveComplete(_transforms);
                    Debug.Log("[HUD] Layout reset (empty file saved).");
                }
            }

            // toggle background panels with B key
            if (Input.GetKeyDown(KeyCode.B))
            {
                var bootstrap = FindObjectOfType<BlazedUIBootstrap>();
                if (bootstrap != null)
                {
                    bootstrap.ToggleBackgroundPanels(!bootstrap.showBackgroundPanel);
                }
                else
                {
                    Debug.LogWarning("[HUD] BlazedUIBootstrap not found for background toggle");
                }
            }

            // find hovered item by screen rect (no raycasts)
            _hover = FindTopMostUnderMouse();

            // start drag or resize
            bool altOk = !requireAltToDrag || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            // Detect resize handle under cursor regardless of modifier, so resizing is always available in edit mode
            _currentHandle = (_hover && _hover.Resizable) ? GetResizeHandle(_hover, Input.mousePosition) : ResizeHandle.None;
            
            // Start drag: require Alt only for moving; allow resizing without Alt
            if (_hover && (altOk || _currentHandle != ResizeHandle.None) && Input.GetMouseButtonDown(0))
            {
                _drag = _hover;
                _startAnchored = _drag.Rect.anchoredPosition;
                _startSize = _drag.Rect.sizeDelta;
                ScreenToLocal(_drag.Rect.parent as RectTransform, Input.mousePosition, out _startMouseLocal);
                
                // Determine if we're resizing or moving - resizing if a corner handle is under the cursor
                _isResizing = _currentHandle != ResizeHandle.None;
                
                if (_isResizing)
                {
                    Debug.Log($"[HUD] Started resizing: {_drag.Id} handle: {_currentHandle}");
                }
                else
                {
                    Debug.Log($"[HUD] Started dragging: {_drag.Id}");
                }
            }

            // drag or resize
            if (_drag && Input.GetMouseButton(0))
            {
                if (ScreenToLocal(_drag.Rect.parent as RectTransform, Input.mousePosition, out var nowLocal))
                {
                    var delta = nowLocal - _startMouseLocal;
                    
                    if (_isResizing)
                    {
                        // Handle resizing from corners with clamping to parent bounds
                        var newSize = CalculateNewSize(_startSize, delta, _currentHandle);
                        newSize.x = Mathf.Round(newSize.x / snapPixels) * snapPixels;
                        newSize.y = Mathf.Round(newSize.y / snapPixels) * snapPixels;

                        // Minimum size constraints
                        newSize.x = Mathf.Max(newSize.x, 40f);
                        newSize.y = Mathf.Max(newSize.y, 20f);

                        // Optional: clamp to parent rect
                        var parent = _drag.Rect.parent as RectTransform;
                        if (parent)
                        {
                            var maxX = parent.rect.width - 2f;
                            var maxY = parent.rect.height - 2f;
                            newSize.x = Mathf.Min(newSize.x, maxX);
                            newSize.y = Mathf.Min(newSize.y, maxY);
                        }

                        _drag.Rect.sizeDelta = newSize;
                    }
                    else
                    {
                        // Handle moving
                        var target = _startAnchored + delta;
                        target.x = Mathf.Round(target.x / snapPixels) * snapPixels;
                        target.y = Mathf.Round(target.y / snapPixels) * snapPixels;
                        _drag.Rect.anchoredPosition = target;
                    }
                }
            }

            // end drag/resize
            if (_drag && Input.GetMouseButtonUp(0))
            {
                // Save both position and size
                _transforms[_drag.Id] = new HudTransformData(_drag.Rect.anchoredPosition, _drag.Rect.sizeDelta);
                HudLayoutStore.SaveComplete(_transforms);
                
                if (_isResizing)
                {
                    Debug.Log($"[HUD] Finished resizing: {_drag.Id} to size {_drag.Rect.sizeDelta}");
                }
                else
                {
                    Debug.Log($"[HUD] Finished dragging: {_drag.Id} to {_drag.Rect.anchoredPosition}");
                }
                
                _drag = null;
                _isResizing = false;
                _currentHandle = ResizeHandle.None;
            }
            
        }

        void OnGUI()
        {
            if (!_editing || !CanEditInCurrentScene()) return;

            const int pad = 8;
            var rect = new Rect(pad, pad, 500, 100);
            GUI.color = kPanel; GUI.Box(rect, GUIContent.none); GUI.color = Color.white;
            GUILayout.BeginArea(rect); GUILayout.Space(6);
            GUILayout.Label("<b>HUD Edit Mode</b>  (F2 toggle)", new GUIStyle(GUI.skin.label){richText=true});
            GUILayout.Label($"Move: Drag{(requireAltToDrag ? " (hold Alt)" : "")} • Resize: Hold Shift+Drag corner • Snap {snapPixels}px");
            GUILayout.Label($"Ctrl+L Save • Ctrl+R Reset • B Toggle BG • Scene: {SceneStateDetector.GetSceneTypeDisplayName()}", new GUIStyle(GUI.skin.label){fontSize = 10});
            if (_hover != null)
                GUILayout.Label($"Hover: {_hover.Id}", new GUIStyle(GUI.skin.label){fontSize = 10});
            GUILayout.EndArea();

            // Highlight hovered element
            if (_hover && _hover.HighlightInEdit)
            {
                var r = GetScreenRect(_hover.Rect);
                DrawRectOutline(r, kHi);
                
                // Draw resize handles if Shift is held
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    DrawResizeHandles(r, _currentHandle);
                }
                
                // Show ID label when dragging/resizing
                if (_drag == _hover)
                {
                    var labelRect = new Rect(r.x, r.y - 20, 250, 20);
                    GUI.color = kPanel; GUI.Box(labelRect, GUIContent.none); GUI.color = Color.white;
                    
                    string action = _isResizing ? "RESIZING" : "MOVING";
                    string sizeInfo = _isResizing ? $" Size: {_drag.Rect.sizeDelta.x:F0}x{_drag.Rect.sizeDelta.y:F0}" : "";
                    GUI.Label(labelRect, $" {action}: {_drag.Id}{sizeInfo}", new GUIStyle(GUI.skin.label) { fontSize = 12 });
                }
            }
        }

        void CaptureAll()
        {
            _positions.Clear();
            foreach (var mv in FindObjectsOfType<HudMovable>(true))
            {
                mv.EnsureRect();
                if (mv.Rect != null)
                {
                    _positions[mv.Id] = mv.Rect.anchoredPosition;
                }
            }
        }
        
        void CaptureAllTransforms()
        {
            _transforms.Clear();
            foreach (var mv in FindObjectsOfType<HudMovable>(true))
            {
                mv.EnsureRect();
                if (mv.Rect != null)
                {
                    _transforms[mv.Id] = new HudTransformData(mv.Rect.anchoredPosition, mv.Rect.sizeDelta);
                }
            }
        }

        HudMovable? FindTopMostUnderMouse()
        {
            var all = FindObjectsOfType<HudMovable>(true);
            HudMovable? best = null;
            float bestArea = float.MaxValue;
            
            foreach (var mv in all)
            {
                if (!mv.gameObject.activeInHierarchy) continue; // Skip inactive objects
                
                mv.EnsureRect();
                if (mv.Rect == null) continue;
                
                var r = GetScreenRect(mv.Rect);
                if (r.Contains(Input.mousePosition))
                {
                    float area = r.width * r.height;
                    if (area < bestArea) { bestArea = area; best = mv; }
                }
            }
            return best;
        }

        Rect GetScreenRect(RectTransform rt)
        {
            if (rt == null) return new Rect(0, 0, 0, 0);
            
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            if (_canvas && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                var cam = _uiCam ? _uiCam : Camera.main;
                if (cam != null)
                {
                    for (int i=0;i<4;i++) 
                        corners[i] = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);
                }
            }
            // else Overlay: worldCorner == screen coords already
            float xMin=float.MaxValue,yMin=float.MaxValue,xMax=float.MinValue,yMax=float.MinValue;
            for (int i=0;i<4;i++){ xMin=Mathf.Min(xMin,corners[i].x); yMin=Mathf.Min(yMin,corners[i].y); xMax=Mathf.Max(xMax,corners[i].x); yMax=Mathf.Max(yMax,corners[i].y);}
            return new Rect(xMin, yMin, xMax-xMin, yMax-yMin);
        }

        void DrawRectOutline(Rect r, Color c)
        {
            var old = GUI.color; GUI.color = c;
            GUI.DrawTexture(new Rect(r.xMin, r.yMin, r.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.xMin, r.yMax-1, r.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.xMin, r.yMin, 1, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.xMax-1, r.yMin, 1, r.height), Texture2D.whiteTexture);
            GUI.color = old;
        }
        
        void DrawResizeHandles(Rect rect, ResizeHandle activeHandle)
        {
            var handleSize = 8f;
            var halfSize = handleSize * 0.5f;
            
            // Convert screen rect to GUI rect (flip Y axis)
            var guiRect = new Rect(rect.x, Screen.height - rect.y - rect.height, rect.width, rect.height);
            
            // Define handle positions in GUI space
            var handles = new Dictionary<ResizeHandle, Vector2>
            {
                { ResizeHandle.TopLeft, new Vector2(guiRect.xMin, guiRect.yMin) },
                { ResizeHandle.TopRight, new Vector2(guiRect.xMax, guiRect.yMin) },
                { ResizeHandle.BottomLeft, new Vector2(guiRect.xMin, guiRect.yMax) },
                { ResizeHandle.BottomRight, new Vector2(guiRect.xMax, guiRect.yMax) }
            };
            
            var old = GUI.color;
            foreach (var handle in handles)
            {
                var pos = handle.Value;
                var handleRect = new Rect(pos.x - halfSize, pos.y - halfSize, handleSize, handleSize);
                
                // Highlight active handle
                GUI.color = handle.Key == activeHandle ? Color.red : Color.white;
                GUI.DrawTexture(handleRect, Texture2D.whiteTexture);
                
                // Draw border
                GUI.color = Color.black;
                GUI.DrawTexture(new Rect(handleRect.x, handleRect.y, handleRect.width, 1), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(handleRect.x, handleRect.yMax-1, handleRect.width, 1), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(handleRect.x, handleRect.y, 1, handleRect.height), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(handleRect.xMax-1, handleRect.y, 1, handleRect.height), Texture2D.whiteTexture);
            }
            GUI.color = old;
        }

        bool ScreenToLocal(RectTransform? parent, Vector3 screen, out Vector2 local)
        {
            local = default;
            if (!parent) return false;
            var cam = (_canvas && _canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? (_uiCam ? _uiCam : Camera.main) : null;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, cam, out local);
        }
        
        ResizeHandle GetResizeHandle(HudMovable hudElement, Vector3 mouseScreenPos)
        {
            var rect = GetScreenRect(hudElement.Rect);
            var handleSize = 12f; // Size of resize handle area in pixels
            
            // Convert screen rect to GUI rect (flip Y axis) 
            var guiRect = new Rect(rect.x, Screen.height - rect.y - rect.height, rect.width, rect.height);
            
            // Convert mouse position to GUI coordinates
            var mouseGUIPos = new Vector2(mouseScreenPos.x, Screen.height - mouseScreenPos.y);
            
            // Check corners for resize handles
            if (IsInCorner(mouseGUIPos, guiRect.xMax, guiRect.yMax, handleSize)) return ResizeHandle.BottomRight;
            if (IsInCorner(mouseGUIPos, guiRect.xMin, guiRect.yMin, handleSize)) return ResizeHandle.TopLeft;
            if (IsInCorner(mouseGUIPos, guiRect.xMax, guiRect.yMin, handleSize)) return ResizeHandle.TopRight;
            if (IsInCorner(mouseGUIPos, guiRect.xMin, guiRect.yMax, handleSize)) return ResizeHandle.BottomLeft;
            
            return ResizeHandle.None;
        }
        
        bool IsInCorner(Vector2 mousePos, float cornerX, float cornerY, float handleSize)
        {
            var halfSize = handleSize * 0.5f;
            return Mathf.Abs(mousePos.x - cornerX) <= halfSize && Mathf.Abs(mousePos.y - cornerY) <= halfSize;
        }
        
        Vector2 CalculateNewSize(Vector2 startSize, Vector2 delta, ResizeHandle handle)
        {
            var newSize = startSize;
            
            switch (handle)
            {
                case ResizeHandle.BottomRight:
                    newSize.x += delta.x;
                    newSize.y -= delta.y; // Y is inverted in screen space
                    break;
                case ResizeHandle.TopLeft:
                    newSize.x -= delta.x;
                    newSize.y += delta.y;
                    break;
                case ResizeHandle.TopRight:
                    newSize.x += delta.x;
                    newSize.y += delta.y;
                    break;
                case ResizeHandle.BottomLeft:
                    newSize.x -= delta.x;
                    newSize.y -= delta.y;
                    break;
            }
            
            return newSize;
        }
    }
}