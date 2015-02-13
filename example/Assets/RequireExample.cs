using UnityEngine;
using System.Collections;
using NLua;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityLua;

namespace X
{
	public class RequireExample : MonoBehaviour
	{
		private Lua LuaState;
		private LuaTable TestLib;
		
		void Start()
		{
			LuaState = new Lua();
			LuaState.LoadCLRPackage();
			LuaState.LoadUnityExpand();

			var ret = LuaState.DoString(@"return require 'requiretest'");
			TestLib = ret[0] as LuaTable;

			var startCallback = TestLib["Start"] as LuaFunction;
			startCallback.Call(this.gameObject);
		}
		
		void Update() 
		{
			var startCallback = TestLib["Update"] as LuaFunction;
			startCallback.Call(this.gameObject);
		}
	}
}
