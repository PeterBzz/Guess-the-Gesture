
using System.Collections.Generic;
using UnityEngine;

public class GestureTask : MonoBehaviour
{

    public List<Vector2> gestureVertices = new List<Vector2>();
    public Material materialForTask;
    public Rect bounds;

    void Start ()
    {
        bounds = DrawGL.GetBounds(gestureVertices);
	}
	
    void OnRenderObject()
    {
        if (gestureVertices.Count > 1)
        {
            //DrawGL.DrawLine(materialForTask, gestureVertices, true);
        }
    }

}
