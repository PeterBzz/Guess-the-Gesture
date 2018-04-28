
using System.Collections.Generic;
using UnityEngine;

public class DrawGL
{
    
    public static Rect GetBounds(List<Vector2> vertices)
    {
        Rect? bounds = null;
        for (int i = 0; i < vertices.Count; i++)
        {
            bounds = UpdateBounds(bounds, vertices[i]);
        }
        return bounds.Value;
    }

    public static Rect UpdateBounds(Rect? bounds, Vector2 vertice)
    {
        Rect result = new Rect();
        if (bounds == null)
        {
            result = new Rect(vertice, Vector2.zero);
        }
        else
        {
            result.xMin = Mathf.Min(bounds.Value.xMin, vertice.x);
            result.yMin = Mathf.Min(bounds.Value.yMin, vertice.y);
            result.xMax = Mathf.Max(bounds.Value.xMax, vertice.x);
            result.yMax = Mathf.Max(bounds.Value.yMax, vertice.y);
        }
        return result;
    }

    public static void DrawLine(Material material, List<Vector2> points, bool isContinuous)
    {
        GL.Begin(GL.LINES);
        material.SetPass(0);
        GL.Color(Color.red);
        Vector2 prev = new Vector2();
        Vector2 current = new Vector2();
        for (int i = 1; i < points.Count; i++)
        {
            if (isContinuous)
            {
                prev = Camera.main.ViewportToWorldPoint(points[i - 1]);
            }
            current = Camera.main.ViewportToWorldPoint(points[i]);
            if (isContinuous)
            {
                GL.Vertex3(prev.x, prev.y, 0);
            }
            GL.Vertex3(current.x, current.y, 0);
        }
        GL.End();
    }
    
}
