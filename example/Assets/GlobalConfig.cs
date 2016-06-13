using UnityEngine;
using System.Collections;

public class GlobalConfig : MonoBehaviour
{
    public float MoveMin = -0.5f;
    public float MoveMax = 0.5f;


    public void Awake()
    {
        Instance = this;
    }

    public static GlobalConfig Instance;
}
