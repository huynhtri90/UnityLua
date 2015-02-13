using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using NLua;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityLua;

namespace X
{
    public class LunaExample : MonoBehaviour
    {
		private Lua LuaState;
		
        void Start()
        {
			LuaState = new Lua();
			LuaState.LoadUnityExpand();
			LuaState.LoadLunaExpand();

			LuaState.CallLunaFunction("lunatest.lua", "Start", this.gameObject);
			UnityLua.Luna.CheckStack(LuaState);
        }

		void Update() 
		{
			LuaState.CallLunaFunction("lunatest.lua", "Update", this.gameObject);
		}

		void OnGUI() 
		{
			LuaState.CallLunaFunction("lunatest.lua", "OnGUI", this.gameObject);
		}
    }
}
