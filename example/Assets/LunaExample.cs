using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using NLua;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace X
{
    public class LunaExample : MonoBehaviour
    {
		private Lua LuaState;
		
        void Start()
        {
			LuaState = new Lua();
			LuaState.LoadUnityExpand();
			LuaState.LoadImportExpand();
			LuaState.LoadCLRPackage("Using");

			LuaState.CallLunaFunction("lunatest.lua", "Start", this.gameObject);
			LuaState.CheckStack();
        }

		void Update() 
		{
			LuaState.CallLunaFunction("lunatest.lua", "Update", this.gameObject, Time.deltaTime);
		}

		void OnGUI() 
		{
			LuaState.CallLunaFunction("lunatest.lua", "OnGUI", this.gameObject);
		}
    }
}
