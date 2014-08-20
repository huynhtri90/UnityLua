using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using LuaInterface;
using System.Text;
using System.IO;
namespace X
{
	public class Luna : MonoBehaviour 
	{  
		public string luaFile;
		public string parameter;
				
		void Start () 
		{
			if (string.IsNullOrEmpty(luaFile))
			{
				luaFile = string.Format("scripts/{0}.lua", this.gameObject.name.ToLower());
			}
			LunaCallFunction(m_LuaState.L, luaFile, "Start", this, parameter);
		}
		
		void Update () 
		{
			LunaCallFunction(m_LuaState.L, luaFile, "Update", this);
		}

		public object[] Call(string function, params object[] args)
		{
			return LunaCallFunction(m_LuaState.L, luaFile, function, args);
		}
				
		/// <summary>
		/// core!
		/// </summary>
		static LuaState m_LuaState;
		
		static Luna()
		{
			m_LuaState = new LuaState();
			SetLunaCallback(ReadFile, FreeData);
			            
			OpenLuna(m_LuaState.L);
		}

		public static void CheckStack()
		{
			CheckStack(m_LuaState.L);
		}

        public static void RegisterLuaDelegateType(Type delegateType, Type luaDelegateType)
        {
            m_LuaState.RegisterLuaDelegateType(delegateType, luaDelegateType);
        }
		
		[UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]     
		delegate bool ReadFileCallback([MarshalAs(UnmanagedType.LPStr)]string fileName, ref IntPtr retData, ref int retLen); 
		
		[UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]     
		delegate void FreeDataCallback(IntPtr data); 
		
		static bool CanUsingFileSystem(string path)
		{
			return !path.Contains("://");
		}
		
		static byte[] ReadAllBytes(string path)
		{
			if (CanUsingFileSystem(path))
			{
				return File.ReadAllBytes(path);
			} 
			else
			{
				WWW www = new WWW(path);
				if (!www.isDone)
				{
					Debug.LogError("Luna.ReadAllBytes not support IEnumerator.");
					return null;
				}
				return www.bytes;
			}
		}
		
		static bool ReadFile([MarshalAs(UnmanagedType.LPStr)]string fileName, ref IntPtr retData, ref int retLen)
		{
			string fullPath = Path.Combine(Application.streamingAssetsPath, fileName);
			try
			{
				var data = ReadAllBytes(fullPath);
				if (data == null)
					return false;
				
				retData = Marshal.AllocHGlobal(data.Length);
				retLen = data.Length;
				
				Marshal.Copy(data, 0, retData, data.Length);
				
				Debug.Log(string.Format("Alloc {0}:{1}", retData, retLen));
				return true;
			}
			catch
			{
				return false;
			}
		}
		
		static void FreeData(IntPtr data)
		{
			Debug.Log(string.Format("Free {0}", data));
			Marshal.FreeHGlobal(data);
		}

        [DllImport(LuaLib.LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Lua_SetLunaCallback")]
		static extern void SetLunaCallback(ReadFileCallback fn1, FreeDataCallback fn2);

		[DllImport(LuaLib.LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Lua_GetFunction")]
		static extern bool GetFunction(IntPtr luastate, [MarshalAs(UnmanagedType.LPStr)]string fileName, [MarshalAs(UnmanagedType.LPStr)]string function);

        [DllImport(LuaLib.LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Lua_CheckStack")]
		static extern void CheckStack(IntPtr luastate);

        [DllImport(LuaLib.LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaopen_luna")]
		static extern int OpenLuna(IntPtr luastate); 
		
		static object[] LunaCallFunction(IntPtr luastate, string fileName, string function, params object[] args)
		{
			var oldTop = LuaLib.LuaGetTop(m_LuaState.L);
			
			if (!GetFunction(m_LuaState.L, fileName, function))
				return null;
			
			var translator = ObjectTranslator.FromState(luastate);
			
			for(var i = 0; i < args.Length; i++)
			{
				translator.push(luastate, args[i]);
			}
			
			var error = LuaLib.LuaPCall(luastate, args.Length, -1, 0);
			if (error != 0)
				m_LuaState.ThrowExceptionFromError(oldTop);
			
			var ret = translator.popValues(m_LuaState.L, oldTop);
			return ret;
		}
	}
}
