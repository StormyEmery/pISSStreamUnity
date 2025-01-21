using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using ToolbarExtender;
using Unity.EditorCoroutines.Editor;

namespace LeTai.pISSStream
{
// ReSharper disable once InconsistentNaming
public class pISSIndicator : ScriptableObject
{
    pISSStream stream;
    float      pISSPercentage = float.NaN;

    Action   repaintAction;
    GUIStyle tankStyle;
    GUIStyle fillStyle;
    GUIStyle labelStyle;


    [InitializeOnLoadMethod]
    static void Inject()
    {
        var pi = CreateInstance<pISSIndicator>();
        pi.hideFlags = HideFlags.HideAndDontSave;
        ToolbarExtender.ToolbarExtender.RightToolbarGUI.Add(pi.OnToolbarGUI);
    }

    void OnEnable()
    {
        stream = new pISSStream();

        EditorCoroutineUtility.StartCoroutine(Poll(), this);
    }

    void OnDisable()
    {
        stream.Dispose();
    }

    IEnumerator Poll()
    {
        while (true)
        {
            if (stream.TryGetLatestData(out var last))
            {
                pISSPercentage = last.pISSPercentage;
                Repaint();
            }

            yield return new EditorWaitForSeconds(1);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    void CacheRepaint()
    {
        if (repaintAction != null)
            return;

        repaintAction = (Action)ToolbarCallback.m_currentToolbar.GetType()
                                               .GetMethod("Repaint")
                                              ?.CreateDelegate(typeof(Action), ToolbarCallback.m_currentToolbar);
    }

    void Repaint()
    {
        if (ToolbarCallback.m_currentToolbar)
            repaintAction?.Invoke();
    }

    void CacheStyle()
    {
        if (tankStyle != null)
            return;

        tankStyle = new GUIStyle("AC Button") {
            fixedWidth  = 0,
            fixedHeight = 0,
        };

        fillStyle = new GUIStyle(tankStyle) { };

        labelStyle = new GUIStyle() {
            alignment = TextAnchor.MiddleCenter,
            wordWrap  = false,
        };
        var labelColor = EditorStyles.label.normal.textColor;
        labelColor.a                = 0.69f;
        labelStyle.normal.textColor = labelColor;
    }

    internal void OnToolbarGUI()
    {
        CacheRepaint();
        CacheStyle();

        var  width  = EditorGUIUtility.pixelsPerPoint * 60;
        var  height = EditorGUIUtility.pixelsPerPoint * 22;
        Rect rect   = GUILayoutUtility.GetRect(width, height);

        // pISSPercentage = (Mathf.Sin(Time.time * 2f) / 2f + .5f) * 100f;
        // pISSPercentage = 69;
        var padding = EditorGUIUtility.pixelsPerPoint * 1;
        Rect fillRect = new Rect(rect.x + padding,
                                 rect.y + padding,
                                 rect.width - padding * 2,
                                 rect.height - padding * 2);
        var fillHeight = fillRect.height * (pISSPercentage / 100f);
        fillRect.y      += fillRect.height - fillHeight;
        fillRect.height =  fillHeight;

        GUI.Box(rect, GUIContent.none, tankStyle);
        var color = GUI.color;
        GUI.color = Color.yellow;
        GUI.Box(fillRect, GUIContent.none, fillStyle);
        GUI.color = color;
        if (float.IsNaN(pISSPercentage))
            GUI.Label(rect, $"🚀🚽  !?", labelStyle);
        else
            GUI.Label(rect, $"🚀🚽 {pISSPercentage:N0}", labelStyle);
    }
}
}
