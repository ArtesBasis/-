using UnityEngine;
using System.Collections;

public class Hide : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        GetComponent<MeshRenderer>().enabled = false;	// Выкл рендера чтобы скрыть точки назначения
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
