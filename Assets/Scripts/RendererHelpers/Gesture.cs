
using System.Collections.Generic;
using UnityEngine;

public class Gesture : MonoBehaviour
{
    public Material mat;

    void Start ()
    {      
        GetComponent<Renderer>().material = mat;
    }

    void Update ()
    {
        if (Time.deltaTime > 1.0)
        {
            Application.Quit();
        }
		GetComponent<Renderer> ().material.SetFloat ("_ScreenRatio", (float)Screen.width / Screen.height);
    }

    public void SetPoints(List<Vector4> points)
    {
        GetComponent<Renderer>().material.SetVectorArray("_Points", points.ToArray());
    }

}
