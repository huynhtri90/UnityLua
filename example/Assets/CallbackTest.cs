using UnityEngine;
using System.Collections;

public class CallbackTest : MonoBehaviour
{
	class LuaBoolDelegateEventArgsHandler : NLua.Method.LuaDelegate
    {
        bool CallFunction(GameObject go)
        {
            object[] args = new object[] { go };
            object[] inArgs = new object[] { go };
            int[] outArgs = new int[] { };
            return (bool)base.CallFunction(args, inArgs, outArgs);
        }
    }

    public delegate bool BoolDelegate(GameObject go);
    public BoolDelegate OnStart;

    void Awake()
    {
        // for ios!
    }

	void Start () 
    {
        if (OnStart != null)
        {
            bool ret = OnStart(this.gameObject);
            Debug.Log(string.Format("CallbackTest Return:{0}", ret));
        }
	}
	
	void Update ()
    {
	
	}
}
