using UnityEngine;
using System.Collections;
using LuaInterface;
// using UniLua;
// using NLua;

public class TestClick : MonoBehaviour
{
	void OnClick()
	{
		Debug.Log ("Click by CSharp1!");
		this.GetComponent<UILabel>().text = "CSharp1";
	}
}

class LuaEventArgsHandler : LuaInterface.LuaDelegate
{
	void CallFunction (GameObject go)
	{
		object [] args = new object [] {go };
		object [] inArgs = new object [] { go };
		int [] outArgs = new int [] { };
		base.callFunction (args, inArgs, outArgs);
	}
}

public class TestLua : MonoBehaviour 
{
	public GameObject PrefabFile;
	LuaInterface.LuaState uLua = new LuaInterface.LuaState();
	// UniLua.ILuaState uniLua = LuaAPI.NewState();
	// NLua.Lua nlua = new NLua.Lua();

	// Use this for initialization
	void Start () 
	{
		// GameObject labelas = GameObject.Find ("Label1");
		GameObject label1 = this.transform.FindChild("Label1").gameObject;
		label1.AddComponent<TestClick>();

		UIEventListener com = this.transform.FindChild ("Label4").GetComponent<UIEventListener> ();

		com.onClick = OnClick;

		uLua.RegisterLuaDelegateType (typeof(UIEventListener.VoidDelegate), typeof(LuaEventArgsHandler));
		

		object[] aa = uLua.DoFile("TestLua");

		LuaInterface.LuaFunction fun = uLua.GetFunction ("TB.TB1.Start");
		uLua.callFunction (fun, new object[]{this.gameObject});

		// uniLua.L_OpenLibs();

		// var status = uniLua.L_DoFile("TestUniLua.lua");
		// if( status != ThreadStatus.LUA_OK )
		// {
		// 	Debug.Log(uniLua.ToString(-1));
		// }


		// TextAsset file = Resources.Load<TextAsset>("TestNLua");
		// object[] aa1 = nlua.DoString(file.text, "TestNLua");
		// NLua.LuaFunction fun1 = nlua.GetFunction ("TB.TB1.Start");
		// nlua.CallFunction (fun1, new object[]{this.gameObject});
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	void OnClick(GameObject obj)
	{
		Debug.Log ("Click by CSharp4!");

		//this.SendMessage("settext", "CSharp4");
	}
}


