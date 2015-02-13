using System;
using System.Text;
using NLua;
using LuaCore  = KeraLua.Lua;
using LuaState = KeraLua.LuaState;
using LuaNativeFunction = KeraLua.LuaNativeFunction;

namespace UnityLua
{
	public static class Luna
	{
		static LuaNativeFunction PrintFunction;
		
		const string LunaPackage = @"
			_G.ImportFiles = _G.ImportFiles or {};
			_G.io.readfile = _G.io.readfile or function(name)
 				local file = _G.io.open(name, 'rb');
				if file then
					return file:read('*all');
				end
			end

			function _G.Import(name) 
		        local key = name;
				local tb = _G.ImportFiles[key];

		        if not tb then 
			        tb = {}; 
			        setmetatable(tb, {__index = _G});
			        _G.ImportFiles[key] = tb;
			    
			        local text = _G.io.readfile(name);
			        if text then
						if text:find('^\xef\xbb\xbf') then
				            text = text:sub(4);
				        end

					    tb.getfenv = function ()
					        return tb;
					    end

					    local fn, msg = load(text, name, 't', tb);
					    if not fn then
					        print('Import ' .. name  .. ' filed.' .. msg);
					        return _G[key];
					    end    
					    fn();
					else
						print('read ' .. name  .. ' filed.' .. msg);
			        end

			         
					
		        end
		    


		        return _G.ImportFiles[key];
		    end

			function _G.CallImportFuncion(file, fnName, ...)
				local tb = Import(file);
				local fn = tb[fnName];
				if not fn then
					return;
				end

				return fn(...);
			end
		";
		public static void LoadLunaExpand(this Lua lua)
		{
			LuaLib.LuaLDoString(lua.LuaState, LunaPackage);
		}

		public static object[] CallLunaFunction(this Lua lua, string fileName, string function, params object[] args)
		{
			var luaState = lua.LuaState;
			var translator = lua.Translator;
			
			var oldTop = LuaLib.LuaGetTop(luaState);
			LuaLib.LuaGetGlobal(luaState, "CallImportFuncion");

			translator.Push(luaState, fileName);
			translator.Push(luaState, function);
			
			for (var i = 0; i < args.Length; i++)
			{
				translator.Push(luaState, args[i]);
			}
			
			var error = LuaLib.LuaPCall(luaState, 2 + args.Length, -1, 0);
			if (error != 0)
			{
				var curTop = LuaLib.LuaGetTop(luaState);
				if (curTop > oldTop)
				{
					object err = translator.GetObject(luaState, -1);
					UnityEngine.Debug.LogError(err);
				}
				
				LuaLib.LuaSetTop(luaState, oldTop);
				return null;
			}
			var ret = translator.PopValues(luaState, oldTop);
			return ret;
		}

		public static void CheckStack(this Lua lua)
		{
			var luaState = lua.LuaState;
			var top = LuaLib.LuaGetTop(luaState);
			if (top == 0)
				return;

			LuaLib.LuaPushString(luaState, "Script stack leak!");
			LuaLib.LuaInsert(luaState, 1);
			
			LuaLib.LuaPushNumber(luaState, top);
			LuaLib.LuaInsert(luaState, 1);
			
			LuaLib.LuaGetGlobal(luaState, "print");
			LuaLib.LuaInsert(luaState, 1);

			LuaLib.LuaPCall(luaState, top + 2, 0, 0);
			LuaLib.LuaSetTop(luaState, 0);
		}
	}
}