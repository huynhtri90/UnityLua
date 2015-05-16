using UnityEngine;
using System.Collections;

public class IL2CPP 
{
	public static void Reg(GameObject go)
	{
		go.transform.position = go.transform.position + go.transform.up;
	}
}
