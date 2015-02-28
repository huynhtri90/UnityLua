using System;
using System.Text;
using LuaCore  = KeraLua.Lua;
using LuaState = KeraLua.LuaState;
using LuaNativeFunction = KeraLua.LuaNativeFunction;

namespace NLua
{
	public static class UnityExpand
	{
		public delegate UnityEngine.Object LoadAssetDelegate(string path);
		public static LoadAssetDelegate LoadAsset = UnityEngine.Resources.Load;

		static LuaNativeFunction PrintFunction;
		static LuaNativeFunction ReadFileFunction;
		static LuaNativeFunction SearcherUnityLuaFunction;

		public static void LoadUnityExpand(this Lua lua)
		{
			SetPrint(lua);
			SetReadFile(lua);
			SetSearcher(lua);
		}

		// this LoadCLRPackage can rename the 'import' function
		public static void LoadCLRPackage(this Lua lua, string newImportName)
		{
			lua.LoadCLRPackage();
			if (!string.IsNullOrEmpty(newImportName))
			{
				string str = string.Format(@"
					if _G.import then -- To avoid ambiguity, rename _G.import 
						_G.{0} = _G.import;
						_G.import = nil;
					end", newImportName);
				LuaLib.LuaLDoString(lua.LuaState, str);
			}
		}

		public static void SetPrint(Lua lua)
		{
			if (PrintFunction == null)
				PrintFunction = new LuaNativeFunction(Print);
			
			LuaLib.LuaPushStdCallCFunction(lua.LuaState, PrintFunction);
			LuaLib.LuaSetGlobal(lua.LuaState, "print");
		}

		public static void SetReadFile(Lua lua)
		{
			if (ReadFileFunction == null)
				ReadFileFunction = new LuaNativeFunction(ReadFile);
			
			LuaLib.LuaGetGlobal(lua.LuaState, "io");
			LuaLib.LuaPushString(lua.LuaState, "readfile");
			LuaLib.LuaPushStdCallCFunction(lua.LuaState, ReadFileFunction);
			LuaLib.LuaSetTable(lua.LuaState, -3);
			LuaLib.LuaPop(lua.LuaState, 1);
		}

		public static void SetSearcher(Lua lua)
		{
			if (SearcherUnityLuaFunction == null)
				SearcherUnityLuaFunction = new LuaNativeFunction(Searcher_UnityLua);
			
			LuaLib.LuaPushStdCallCFunction(lua.LuaState, SearcherUnityLuaFunction);
			LuaLib.LuaSetGlobal(lua.LuaState, "____SearcherUnityLuaFunction");
			
			string str = @"
				if _G.____SearcherUnityLuaFunction then
					table.insert(package.searchers, 1, _G.____SearcherUnityLuaFunction);
					_G.____SearcherUnityLuaFunction = nil
				end";
			LuaLib.LuaLDoString(lua.LuaState, str);
		}

		[MonoPInvokeCallback (typeof (LuaNativeFunction))]
		static int Print(LuaState luaState)
		{
			// For each argument we'll 'tostring' it
			int n = LuaLib.LuaGetTop(luaState);
			var sb = new StringBuilder("Lua:");
			
			LuaLib.LuaGetGlobal(luaState, "tostring");
			
			for( int i = 1; i <= n; i++ ) 
			{
				LuaLib.LuaPushValue(luaState, -1);  /* function to be called */
				LuaLib.LuaPushValue(luaState, i);   /* value to print */
				LuaLib.LuaPCall(luaState, 1, 1, 0);

				var s = LuaLib.LuaToString(luaState, -1);
				sb.Append(s);
					
				if(i > 1) 
				{
					sb.Append("\t");
				}
				
				LuaLib.LuaPop(luaState, 1);  /* pop result */
			}
			UnityEngine.Debug.Log(sb.ToString());
			return 0;
		}

		[MonoPInvokeCallback (typeof (LuaNativeFunction))]
		static int ReadFile(LuaState luaState)
		{
			string fileName = LuaLib.LuaToString(luaState, 1);
			var file = LoadAsset(fileName) as UnityEngine.TextAsset;
			if(file == null)
				return 0;
			
			LuaLib.LuaPushString(luaState, file.bytes);
            return 1;
		}

		[MonoPInvokeCallback (typeof (LuaNativeFunction))]
		public static int Searcher_UnityLua(LuaState luaState)
		{
			string fileName = LuaLib.LuaToString(luaState, 1);
			fileName = fileName.Replace('.', '/');
			fileName += ".lua";
			
			var file = LoadAsset(fileName) as UnityEngine.TextAsset;
			if(file == null)
				return 0;

			var data = file.bytes;
			LuaLib.LuaLLoadBuffer(luaState, data, fileName);
			
			return 1;
		}
	}
}