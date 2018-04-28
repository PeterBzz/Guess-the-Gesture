
using System.Collections.Generic;
using UnityEngine;

public class VectorHelper
{

    public static List<Vector4> GetVec4ListFilled(int count)
    {
        List<Vector4> list = new List<Vector4>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(Vector4.zero);
        }
        return list;
    }

    public static List<Vector4> Vec4FromVec2(List<Vector2> list)
    {
        List<Vector4> res = new List<Vector4>();
        foreach (Vector2 val in list)
        {
            res.Add(new Vector4(val.x, val.y, 0, 1));
            //res.Add(new Vector4((val.x - 0.5f) * 10, (val.y - 0.5f) * 10, 0, 1));
            //Vector3 v = Camera.main.ScreenToWorldPoint(new Vector3(val.x, val.y, 0));
            //res.Add(new Vector4(v.x, v.y, v.z, 1));
        }
        return res;
    }

}
