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
			LuaState.LoadCLRPackage();

			UnityLua.UnityExpand.Open(LuaState);
			UnityLua.Luna.Open(LuaState);

			UnityLua.Luna.CallFunction(LuaState, "lunatest.lua", "Start", this.gameObject);

			UnityLua.Luna.CheckStack(LuaState);
        }

		void Update() 
		{
			UnityLua.Luna.CallFunction(LuaState, "lunatest.lua", "Update", this.gameObject);
		}

		void OnGUI() 
		{
			UnityLua.Luna.CallFunction(LuaState, "lunatest.lua", "OnGUI", this.gameObject);
		}
    }
}
