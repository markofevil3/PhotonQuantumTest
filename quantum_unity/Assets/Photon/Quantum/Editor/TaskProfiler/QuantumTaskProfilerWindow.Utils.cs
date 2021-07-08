using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {

  public partial class QuantumTaskProfilerWindow {

    private static readonly Action ApplyWireMaterial = typeof(HandleUtility).CreateMethodDelegate<Action>("ApplyWireMaterial");
    private static readonly ToolbarSearchFieldDelegate ToolbarSearchField = typeof(EditorGUI).CreateMethodDelegate<ToolbarSearchFieldDelegate>("ToolbarSearchField");

    private delegate string ToolbarSearchFieldDelegate(int id, Rect position, string text, bool showWithPopupArrow);
    

    private static Vector3[] _rectVertices = new Vector3[4];
    private static Vector3[] _graphPoints = new Vector3[1024];

    public static void DrawSolidRectangleWithOutline(Rect rect, Color faceColor, Color outlineColor) {

      _rectVertices[0] = new Vector3(rect.xMin, rect.yMin, 0f);
      _rectVertices[1] = new Vector3(rect.xMax, rect.yMin, 0f);
      _rectVertices[2] = new Vector3(rect.xMax, rect.yMax, 0f);
      _rectVertices[3] = new Vector3(rect.xMin, rect.yMax, 0f);
      Handles.DrawSolidRectangleWithOutline(_rectVertices, faceColor, outlineColor);
    }

    public static void DrawRectFast(Rect r, Color color) {
      GL.Color(color);
      GL.Vertex(new Vector3(r.xMin, r.yMin, 0f));
      GL.Vertex(new Vector3(r.xMax, r.yMin, 0f));
      GL.Vertex(new Vector3(r.xMax, r.yMax, 0f));
      GL.Vertex(new Vector3(r.xMin, r.yMax, 0f));
    }

    public static void DrawVerticalLineFast(float x, float minY, float maxY, Color color) {
      GL.Color(color);
      GL.Vertex(new Vector3(x, minY, 0f));
      GL.Vertex(new Vector3(x, maxY, 0f));
    }
    private static void CalculateMeanStdDev(List<float> values, out double mean, out double stdDev) {
      mean = 0;
      foreach (var v in values)
        mean += v;
      mean /= values.Count;

      stdDev = 0;
      foreach (var v in values) {
        stdDev += (v - mean) * (v - mean);
      }
      stdDev = Math.Sqrt(stdDev / values.Count);
    }

    private static Rect DrawDropShadowLabel(float time, float x, float y, float sizeXMul, float sizeYMul) {
      var content = new GUIContent(FormatTime(time));
      var size = Styles.whiteLabel.CalcSize(content);
      var rect = new Rect(x + size.x * sizeXMul, y + size.y * sizeYMul, size.x, size.y);
      EditorGUI.DropShadowLabel(rect, content, Styles.whiteLabel);
      return rect;
    }

    private static Rect DrawDropShadowLabelWithMargins(Rect r, float time, float maxTime, float x, float sizeXMul = 0.0f, float sizeYMul = -0.5f, Color? color = null) {
      var content = new GUIContent(FormatTime(time));
      var size = Styles.whiteLabel.CalcSize(content);

      var y = (maxTime - time) * r.height / maxTime;
      y += size.y * sizeYMul;
      y = Mathf.Clamp(y, 0.0f, r.height - size.y) + r.y;

      x += size.x * sizeXMul;
      x = Mathf.Clamp(x, 0.0f, r.width - size.x) + r.x;

      var rect = new Rect(x, y, size.x, size.y);

      var oldContentColor = GUI.contentColor;
      try {
        if (color != null) {
          GUI.contentColor = color.Value;
        }
        EditorGUI.DropShadowLabel(rect, content, Styles.whiteLabel);
        return rect;
      } finally {
        GUI.contentColor = oldContentColor;
      }
    }

    private static float LinearRoot(float x, float y, float dx, float dy) {
      return x - y * dx / dy;
    }

    private static void DrawGraph(Rect rect, List<float> durations, ZoomPanel panel, float maxDuration, Color? color = null, float lineWidth = 2) {
      var r = rect.Adjust(0, 3, 0, -4);

      int p = 0;
      var durationToY = r.height / maxDuration;

      float dx = rect.width / panel.range;
      var start = Mathf.FloorToInt(panel.start);
      var end = Mathf.Min(durations.Count-1, Mathf.CeilToInt(panel.start + panel.range));
      var x = panel.TimeToPixel(start, rect);

      for (int i = start; i <= end; ++i, ++p, x += dx) {
        if (_graphPoints.Length - 1 <= p) {
          Array.Resize(ref _graphPoints, p * 2);
        }

        var d = durations[i];
        var y = (maxDuration - d);

        _graphPoints[p].x = x;
        _graphPoints[p].y = (maxDuration - d) * durationToY + r.y;
      }

      using (new Handles.DrawingScope(color ?? Color.white)) {
        Handles.DrawAAPolyLine(lineWidth, p, _graphPoints);
      }
    }

    private static void DrawLargeTooltip(Rect areaRect, Rect itemRect, GUIContent content) {

      const float ArrowWidth = 64.0f;
      const float ArrowHeight = 6.0f;
      GUIStyle style = "AnimationEventTooltip";
      GUIStyle arrowStyle = "AnimationEventTooltipArrow";

      var size = style.CalcSize(content);
      var anchor = new Vector2(itemRect.center.x, itemRect.yMax);

      var arrowRect = new Rect(anchor.x - ArrowWidth / 2.0f, anchor.y, ArrowWidth, ArrowHeight);
      var labelRect = new Rect(anchor.x, anchor.y + ArrowHeight, size.x, size.y);

      // these are some magic values that Unity seems to be using with this tyle
      if (labelRect.xMax > areaRect.xMax + 16)
        labelRect.x = areaRect.xMax - labelRect.width + 16;
      if (arrowRect.xMax > areaRect.xMax + 20)
        arrowRect.x = areaRect.xMax - arrowRect.width + 20;
      if (labelRect.xMin < areaRect.xMin + 30)
        labelRect.x = areaRect.xMin + 30;
      if (arrowRect.xMin < areaRect.xMin - 20)
        arrowRect.x = areaRect.xMin - 20;

      // flip tooltip if too close to bottom (but do not flip if flipping would mean the tooltip is too high up)
      var flipRectAdjust = (itemRect.height + labelRect.height + 2 * arrowRect.height);
      var flipped = (anchor.y + size.y + 6 > areaRect.yMax) && (labelRect.y - flipRectAdjust > 0);
      if (flipped) {
        labelRect.y -= flipRectAdjust;
        arrowRect.y -= (itemRect.height + 2 * arrowRect.height);
      }

      using (new GUI.ClipScope(arrowRect)) {
        var oldMatrix = GUI.matrix;
        try {
          if (flipped)
            GUIUtility.ScaleAroundPivot(new Vector2(1.0f, -1.0f), new Vector2(arrowRect.width * 0.5f, arrowRect.height));
          GUI.Label(new Rect(0, 0, arrowRect.width, arrowRect.height), GUIContent.none, arrowStyle);
        } finally {
          GUI.matrix = oldMatrix;
        }
      }

      GUI.Label(labelRect, content, style);
    }

    private static void DrawLegendLabel(Rect rect, string label) {
      GUI.Box(rect, GUIContent.none, Styles.legendBackground);
      rect = rect.Adjust(5, 5, 0, 0);
      EditorGUI.LabelField(rect, label);
    }

    private static float DrawSplitter(Rect rect) {
      float delta = 0.0f;
      var controlId = GUIUtility.GetControlID(Styles.SplitterControlId, FocusType.Passive);
      switch (Event.current.GetTypeForControl(controlId)) {
        case EventType.MouseDown:
          if ((Event.current.button == 0) && (Event.current.clickCount == 1) && rect.Contains(Event.current.mousePosition)) {
            GUIUtility.hotControl = controlId;
          }
          break;

        case EventType.MouseDrag:
          if (GUIUtility.hotControl == controlId) {
            delta = Event.current.delta.y;
            Event.current.Use();
          }
          break;

        case EventType.MouseUp:
          if (GUIUtility.hotControl == controlId) {
            GUIUtility.hotControl = 0;
            Event.current.Use();
          }
          break;

        case EventType.Repaint:
          EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical, controlId);
          break;
      }
      return delta;
    }

    private static string FormatTime(float time) {
      return string.Format("{0:F4}ms", time);
    }
    internal sealed class TickHandler {

      private readonly float[] _tickModulos = new float[] {
        0.00001f,
        0.00005f,
        0.0001f,
        0.0005f,
        0.001f,
        0.005f,
        0.01f,
        0.05f,
        0.1f,
        0.5f,
        1f,
        5f,
        10f,
        50f,
        100f,
        500f,
        1000f,
        5000f,
        10000f,
      };

      private readonly float[] _tickStrengths;

      private int _maxVisibleLevel = -1;
      private int _minVisibleLevel = 0;
      private float _timeMin = 0;
      private float _timeRange = 1;
      private float _timeToPixel = 1;

      private List<float> m_TickList = new List<float>(1000);

      public TickHandler() {
        _tickStrengths = new float[_tickModulos.Length];
      }

      public int VisibleLevelsCount => _maxVisibleLevel - _minVisibleLevel + 1;

      public int GetLevelWithMinSeparation(float pixelSeparation) {
        for (int i = 0; i < _tickModulos.Length; i++) {
          float tickSpacing = _tickModulos[i] * _timeToPixel;
          if (tickSpacing >= pixelSeparation)
            return i - _minVisibleLevel;
        }
        return -1;
      }

      public float GetPeriodOfLevel(int level) {
        return _tickModulos[Mathf.Clamp(_minVisibleLevel + level, 0, _tickModulos.Length - 1)];
      }

      public float GetStrengthOfLevel(int level) {
        return _tickStrengths[_minVisibleLevel + level];
      }

      public List<float> GetTicksAtLevel(int level, bool excludeTicksFromHigherLevels) {
        m_TickList.Clear();

        if (level > 0) {
          GetTicksAtLevel(level, excludeTicksFromHigherLevels, m_TickList);
        }

        return m_TickList;
      }

      public void Refresh(float minTime, float timeRange, float pixelWidth, float minTickSpacing = 3.0f, float maxTickSpacing = 80.0f) {
        _timeMin = minTime;
        _timeRange = timeRange;
        _timeToPixel = pixelWidth / timeRange;

        _minVisibleLevel = 0;
        _maxVisibleLevel = _tickModulos.Length - 1;

        for (int i = _tickModulos.Length - 1; i >= 0; i--) {
          // how far apart (in pixels) these modulo ticks are spaced:
          float tickSpacing = _tickModulos[i] * _timeToPixel;

          // calculate the strength of the tick markers based on the spacing:
          _tickStrengths[i] = (tickSpacing - minTickSpacing) / (maxTickSpacing - minTickSpacing);

          if (_tickStrengths[i] >= 1) {
            _maxVisibleLevel = i;
          }

          if (tickSpacing <= minTickSpacing) {
            _minVisibleLevel = i;
            break;
          }
        }

        for (int i = _minVisibleLevel; i <= _maxVisibleLevel; i++) {
          _tickStrengths[i] = Mathf.Sqrt(Mathf.Clamp01(_tickStrengths[i]));
        }
      }
      private void GetTicksAtLevel(int level, bool excludeTicksFromHigherlevels, List<float> list) {
        if (list == null)
          throw new System.ArgumentNullException("list");

        int l = Mathf.Clamp(_minVisibleLevel + level, 0, _tickModulos.Length - 1);
        int startTick = Mathf.FloorToInt(_timeMin / _tickModulos[l]);
        int endTick = Mathf.FloorToInt((_timeMin + _timeRange) / _tickModulos[l]);
        for (int i = startTick; i <= endTick; i++) {
          // return if tick mark is at same time as larger tick mark
          if (excludeTicksFromHigherlevels
              && l < _maxVisibleLevel
              && (i % Mathf.RoundToInt(_tickModulos[l + 1] / _tickModulos[l]) == 0))
            continue;
          list.Add(i * _tickModulos[l]);
        }
      }
    }

    [Serializable]
    internal class ZoomPanel {
      public bool allowScrollPastLimits;
      public bool enableRangeSelect;

      public int controlId;

      public Rect areaRect;

      public float minRange;
      public float range;
      public float start;
      public float verticalScroll;

      public Vector2? selectionRange;
      private Vector2? _dragStart;

      public float DurationToPixelLength(float duration, Rect rect) {
        return (duration) / range * rect.width;
      }


      public void OnGUI(Rect r, float minValue, float maxValue, out bool unselect, float minY = 0.0f, float maxY = 1.0f, bool verticalSlider = false) {

        unselect = false;

        var areaRect = r.Adjust(0, 0, -Styles.ScrollBarWidth, -Styles.ScrollBarWidth);
        this.areaRect = areaRect;

        var hScrollbarRect = r.SetY(r.yMax - Styles.ScrollBarWidth).SetHeight(Styles.ScrollBarWidth).AddWidth(-Styles.ScrollBarWidth);
        DrawHorizontalScrollbar(hScrollbarRect, maxValue, ref start, ref range);

        var vScrollbarRect = r.SetX(r.xMax - Styles.ScrollBarWidth).SetWidth(Styles.ScrollBarWidth).AddHeight(-Styles.ScrollBarWidth);
        if (verticalSlider) {
          DrawPowerSlider(vScrollbarRect, minY, maxY, 4.0f, ref verticalScroll);
        } else {
          Debug.Assert(minY == 0.0f);
          DrawVerticalScrollbar(vScrollbarRect, maxY < 0 ? areaRect.height : maxY, areaRect.height, ref verticalScroll);
        }
        verticalScroll = Mathf.Clamp(verticalScroll, minY, maxY);

        //GUI.Box(hScrollbarRect.SetX(0).SetWidth(Styles.LeftPaneWidth), GUIContent.none, EditorStyles.toolbar);

        var id = GUIUtility.GetControlID(controlId, FocusType.Passive);



        using (new GUI.GroupScope(areaRect)) {
          if (Event.current.isMouse || Event.current.isScrollWheel) {
            bool doingSelect = Event.current.button == 0 && !Event.current.modifiers.HasFlag(EventModifiers.Alt);
            bool doingDragScroll = Event.current.button == 2 || Event.current.button == 0 && !doingSelect;
            bool doingZoom = Event.current.button == 1 && Event.current.modifiers.HasFlag(EventModifiers.Alt);
            var inRect = r.ZeroXY().Contains(Event.current.mousePosition);

            switch (Event.current.type) {
              case EventType.ScrollWheel:
                if (inRect) {
                  if (Event.current.modifiers.HasFlag(EventModifiers.Shift)) {
                    if (verticalSlider) {
                      var delta = Event.current.delta.x + Event.current.delta.y;
                      var amount = Mathf.Clamp(delta * 0.01f, -0.9f, 0.9f);
                      verticalScroll *= (1 - amount);
                      verticalScroll = Mathf.Clamp(verticalScroll, minY, maxY);
                      Event.current.Use();
                    }
                  } else {
                    PerfomFocusedZoom(Event.current.mousePosition, r.ZeroXY(), -Event.current.delta.x - Event.current.delta.y, minRange,
                      ref start, ref range);
                    Event.current.Use();
                  }
                }
                break;

              case EventType.MouseDown:
                if (inRect && (doingDragScroll || doingSelect || doingZoom)) {
                  _dragStart = Event.current.mousePosition;
                  selectionRange = null;
                  if (doingDragScroll || doingZoom) {
                    GUIUtility.hotControl = id;
                  } else if (!enableRangeSelect) {
                    GUIUtility.hotControl = id;
                    var x = PixelToTime(Event.current.mousePosition.x, areaRect.ZeroXY());
                    selectionRange = new Vector2(x, x);
                  } else {
                    // wait with tracking as this might as well be click-select
                  }
                  Event.current.Use();
                }
                break;

              case EventType.MouseDrag:
                if (_dragStart.HasValue) {
                  if (inRect && GUIUtility.hotControl != id) {
                    var deltaPixels = Event.current.mousePosition - _dragStart.Value;
                    if (Mathf.Abs(deltaPixels.x) > Styles.DragPixelsThreshold) {
                      GUIUtility.hotControl = id;
                      unselect = true;
                    }
                  }

                  if (GUIUtility.hotControl == id) {
                    if (doingSelect) {
                      if (enableRangeSelect) {
                        var minX = Mathf.Min(_dragStart.Value.x, Event.current.mousePosition.x);
                        var maxX = Mathf.Max(_dragStart.Value.x, Event.current.mousePosition.x);
                        selectionRange = new Vector2(minX, maxX) / r.width * range + new Vector2(start, start);
                      } else {
                        var x = PixelToTime(Event.current.mousePosition.x, areaRect.ZeroXY());
                        selectionRange = new Vector2(x, x);
                      }
                    } else if (doingDragScroll) {
                      var deltaTime = (Event.current.delta.x / r.width) * (range);
                      start -= deltaTime;
                    } else if (doingZoom) {
                      PerfomFocusedZoom(_dragStart.Value, r.ZeroXY(), Event.current.delta.x, minRange,
                        ref start, ref range);
                    }

                    Event.current.Use();
                  }
                }
                break;

              case EventType.MouseUp:
                _dragStart = null;
                if (GUIUtility.hotControl == id) {
                  GUIUtility.hotControl = 0;
                  Event.current.Use();
                } else {
                  selectionRange = null;
                  unselect = true;
                }
                break;
            }
          }
        }

        if (!allowScrollPastLimits) {
          range = Mathf.Clamp(range, minRange, maxValue - minValue);
          start = Mathf.Clamp(start, minValue, maxValue - range);
        }
      }

      public float PixelToTime(float pixel, Rect rect) {
        return (pixel - rect.x) * (range / rect.width) + start;
      }

      public float TimeToPixel(float time) => TimeToPixel(time, areaRect);

      public float TimeToPixel(float time, Rect rect) {
        return (time - start) / range * rect.width + rect.x;
      }
      private static void DrawHorizontalScrollbar(Rect rect, float maxValue, ref float start, ref float range) {
        var minScrollbarValue = 0.0f;

        maxValue = Mathf.Max(start + range, maxValue);
        minScrollbarValue = Mathf.Min(start, minScrollbarValue);

        if (Mathf.Abs((maxValue - minScrollbarValue) - range) <= 0.001f) {
          // fill scrollbar
          GUI.HorizontalScrollbar(rect, 0.0f, 1.0f, 0.0f, 1.0f);
        } else {
          // a workaround for
          maxValue += 0.00001f;
          start = GUI.HorizontalScrollbar(rect, start, range, minScrollbarValue, maxValue);
        }
      }

      private static void DrawVerticalScrollbar(Rect rect, float workspaceHeightNeeded, float workspaceHeight, ref float scroll) {
        if (workspaceHeight > workspaceHeightNeeded) {
          scroll = 0.0f;
          GUI.VerticalScrollbar(rect, 0, 1, 0, 1);
        } else {
          scroll = Mathf.Min(scroll, workspaceHeightNeeded - workspaceHeight);
          scroll = GUI.VerticalScrollbar(rect, scroll, workspaceHeight, 0, workspaceHeightNeeded);
        }
      }

      private static void DrawPowerSlider(Rect rect, float min, float max, float power, ref float scroll) {

        var pmin = Mathf.Pow(min, 1f / power);
        var pmax = Mathf.Pow(max, 1f / power);
        var pval = Mathf.Pow(scroll, 1f / power);

        pval = GUI.VerticalSlider(rect, pval, pmax, pmin);

        scroll = Mathf.Pow(pval, power);
      }


      private static void PerfomFocusedZoom(Vector2 zoomAround, Rect rect, float delta, float minRange, ref float start, ref float range) {
        var amount = Mathf.Clamp(delta * 0.01f, -0.9f, 0.9f);

        var oldRange = range;
        range *= (1 - amount);

        if (range < minRange) {
          range = minRange;
          amount = 1.0f - range / oldRange;
        }

        var pivot = zoomAround.x / rect.width;
        start += pivot * oldRange * amount;
      }
    }
  }
}