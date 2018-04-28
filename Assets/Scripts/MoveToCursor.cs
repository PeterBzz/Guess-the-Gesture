
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCursor : MonoBehaviour
{
    
	void Start ()
    {
		
	}
	
	void Update ()
    {
        this.transform.position = (this.transform.position + Camera.main.ScreenToWorldPoint(Input.mousePosition)) / 2;
	}

}
