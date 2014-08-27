using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using LuaInterface;
using System.Text;
using System.IO;
using System.Collections.Generic;


namespace X
{
    public class Luna : MonoBehaviour
    {
        public string LuaFile;
        public string Parameter;

        [HideInInspector]
        [System.NonSerialized]
        Dictionary<string, object> ObjectCache;

        void Start()
        {
            if (string.IsNullOrEmpty(LuaFile))
            {
                LuaFile = string.Format("scripts/{0}.lua", this.gameObject.name.ToLower());
            }

            LunaCallFunction(m_LuaState.L, LuaFile, "Start", this);
        }

        void Update()
        {
            LunaCallFunction(m_LuaState.L, LuaFile, "Update", this);
        }

        public object[] Call(string function, params object[] args)
        {
            return LunaCallFunction(m_LuaState.L, LuaFile, function, args);
        }

        public void SetObject(string key, object value)
        {
            if (ObjectCache == null)
                ObjectCache = new Dictionary<string, object>();
            ObjectCache[key] = value;
        }

        public object GetObject(string key)
        {
            object obj = null;
            ObjectCache.TryGetValue(key, out obj);
            return obj;
        }


        public GameObject FindChildrenObject(string name)
        {
            return FindChildrenObject(this.gameObject, name);
        }

        GameObject FindChildrenObject(GameObject go, string name)
        {
            var items = go.GetComponentsInChildren(typeof(Transform));
            foreach (var item in items)
            {
                if (item.name == name)
                {
                    return item.gameObject;
                }
            }

            foreach (var item in items)
            {
                var ret = FindChildrenObject(item.gameObject, name);
                if (ret != null)
                    return ret;
            }

            return null;
        }

        /// <summary>
        /// core!
        /// </summary>
        static LuaState m_LuaState;

        static Luna()
        {
            m_LuaState = new LuaState();
            Lua_SetLunaCallback(ReadFile, FreeData);

            luaopen_luna(m_LuaState.L);
        }

        public static void CheckStack()
        {
            Lua_CheckStack(m_LuaState.L);
        }

        public static void RegisterLuaDelegateType(Type delegateType, Type luaDelegateType)
        {
            m_LuaState.RegisterLuaDelegateType(delegateType, luaDelegateType);
        }

        delegate bool ReadFileCallback(string fileName, ref IntPtr retData, ref int retLen);
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

        [MonoPInvokeCallbackAttribute(typeof(ReadFileCallback))]
        static bool ReadFile(string fileName, ref IntPtr retData, ref int retLen)
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

        [MonoPInvokeCallbackAttribute(typeof(FreeDataCallback))]
        static void FreeData(IntPtr data)
        {
            Debug.Log(string.Format("Free {0}", data));
            Marshal.FreeHGlobal(data);
        }

        [DllImport(LuaLib.LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Lua_SetLunaCallback")]
        static extern void Lua_SetLunaCallback(ReadFileCallback fn1, FreeDataCallback fn2);

        [DllImport(LuaLib.LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Lua_GetFunction")]
        static extern int Lua_GetFunction(IntPtr luastate, [MarshalAs(UnmanagedType.LPStr)]string fileName, [MarshalAs(UnmanagedType.LPStr)]string function);

        [DllImport(LuaLib.LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Lua_CheckStack")]
        static extern void Lua_CheckStack(IntPtr luastate);

        [DllImport(LuaLib.LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaopen_luna")]
        static extern int luaopen_luna(IntPtr luastate);

        static object[] LunaCallFunction(IntPtr luastate, string fileName, string function, params object[] args)
        {
            var oldTop = LuaLib.LuaGetTop(m_LuaState.L);
            var translator = ObjectTranslator.FromState(luastate);
            var error = Lua_GetFunction(m_LuaState.L, fileName, function);

            if (error != 0)
            {
                var curTop = LuaLib.LuaGetTop(m_LuaState.L);
                if (curTop > oldTop)
                {
                    object err = translator.getObject(m_LuaState.L, -1);
                    Debug.LogError(err);
                }

                LuaLib.LuaSetTop(m_LuaState.L, oldTop);
                return null;
            }


            for (var i = 0; i < args.Length; i++)
            {
                translator.push(luastate, args[i]);
            }

            error = LuaLib.LuaPCall(luastate, args.Length, -1, 0);
            if (error != 0)
            {
                var curTop = LuaLib.LuaGetTop(m_LuaState.L);
                if (curTop > oldTop)
                {
                    object err = translator.getObject(m_LuaState.L, -1);
                    Debug.LogError(err);
                }

                LuaLib.LuaSetTop(m_LuaState.L, oldTop);
                return null;
            }
            var ret = translator.popValues(m_LuaState.L, oldTop);
            return ret;
        }
    }
}
