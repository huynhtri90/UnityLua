namespace LuaInterface
{

	using System;
	using System.Runtime.InteropServices;
	using System.Reflection;
	using System.Collections;
	using System.Text;
    using System.Security;

	#pragma warning disable 414
	public class MonoPInvokeCallbackAttribute : System.Attribute
	{
		private Type type;
		internal MonoPInvokeCallbackAttribute( Type t ) { type = t; }
	}
	#pragma warning restore 414

    public enum LuaTypes
	{
		LUA_TNONE=-1,
		LUA_TNIL=0,
		LUA_TNUMBER=3,
		LUA_TSTRING=4,
		LUA_TBOOLEAN=1,
		LUA_TTABLE=5,
		LUA_TFUNCTION=6,
		LUA_TUSERDATA=7,
		LUA_TTHREAD=8,
		LUA_TLIGHTUSERDATA=2
	}

    public enum LuaGCOptions
	{
		LUA_GCSTOP = 0,
		LUA_GCRESTART = 1,
		LUA_GCCOLLECT = 2,
		LUA_GCCOUNT = 3,
		LUA_GCCOUNTB = 4,
		LUA_GCSTEP = 5,
		LUA_GCSETPAUSE = 6,
		LUA_GCSETSTEPMUL = 7,
	}

    public enum LuaThreadStatus
    {
        LUA_YIELD       = 1,
        LUA_ERRRUN      = 2,
        LUA_ERRSYNTAX   = 3,
        LUA_ERRMEM      = 4,
        LUA_ERRERR      = 5,
    }

	sealed class LuaIndexes
	{
		internal static int LUA_REGISTRYINDEX=-10000;
		internal static int LUA_ENVIRONINDEX=-10001;
		internal static int LUA_GLOBALSINDEX=-10002;
	}

	[StructLayout(LayoutKind.Sequential)]
    public struct ReaderInfo
	{
		internal string chunkData;
		internal bool finished;
	}

    public delegate int LuaCSFunction(IntPtr luaState);
    public delegate string LuaChunkReader(IntPtr luaState, ref ReaderInfo data, ref uint size);

    public delegate int LuaFunctionCallback(IntPtr luaState);
	sealed class LuaLib
	{
        internal static int LUA_MULTRET = -1;
#if UNITY_IPHONE
		internal const string LUADLL = "__Internal";
#else
		internal const string LIBNAME = "lua";
#endif

        internal static int LuaLGetN(IntPtr luaState, int i)
        {
            return (int)LuaLib.LuaObjLen(luaState, i);
        }

        internal static int LuaLDoString(IntPtr luaState, string chunk)
        {
            int result = LuaLib.LuaLLoadString(luaState, chunk);
            if (result != 0)
                return result;

            return LuaLib.LuaPCall(luaState, 0, -1, 0);
        }       
  
        internal static void LuaNewTable(IntPtr luaState)
        {
            LuaLib.LuaCreateTable(luaState, 0, 0);
        }

        internal static void LuaGetGlobal(IntPtr luaState, string name)
        {
            LuaLib.LuaPushString(luaState, name);
            LuaLib.LuaGetTable(luaState, LuaIndexes.LUA_GLOBALSINDEX);
        }

        internal static void LuaSetGlobal(IntPtr luaState, string name)
        {
            LuaLib.LuaPushString(luaState, name);
            LuaLib.LuaInsert(luaState, -2);
            LuaLib.LuaSetTable(luaState, LuaIndexes.LUA_GLOBALSINDEX);
        }

        internal static void LuaPop(IntPtr luaState, int amount)
        {
            LuaLib.LuaSetTop(luaState, -(amount) - 1);
        }

        internal static void LuaLGetMetaTable(IntPtr luaState, string meta)
        {
            LuaLib.LuaGetField(luaState, LuaIndexes.LUA_REGISTRYINDEX, meta);
        }

        internal static string LuaToString(IntPtr luaState, int index)
        {
            int strlen;
            IntPtr str = LuaToLString(luaState, index, out strlen);
            if (str != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(str, strlen);
            }
            else
            {
                return null;
            }
        }

        internal static void LuaPushStdCallCFunction(IntPtr luaState, LuaCSFunction function)
        {
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(function);
            LuaPushStdCallCFunction(luaState, fn);
        }

        internal static bool LuaIsNil(IntPtr luaState, int index)
        {
            return (LuaLib.LuaType(luaState, index) == LuaTypes.LUA_TNIL);
        }

        internal static bool LuaIsBoolean(IntPtr luaState, int index)
        {
            return LuaLib.LuaType(luaState, index) == LuaTypes.LUA_TBOOLEAN;
        }

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_newthread")]
        internal static extern IntPtr LuaNewThread(IntPtr luaState);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_resume")]
        internal static extern int LuaResume(IntPtr luaState, int arg);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_status")]
        internal static extern int LuaStatus(IntPtr luaState);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_typename")]
		internal static extern string LuaTypeName(IntPtr luaState, LuaTypes type);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_setfenv")]
		internal static extern int LuaSetFEnv(IntPtr luaState, int stackPos);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_setfield")]
		internal static extern void LuaSetField(IntPtr luaState, int stackPos, string name);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_newstate")]
		internal static extern IntPtr LuaLNewState();

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_close")]
		internal static extern void LuaClose(IntPtr luaState);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_openlibs")]
		internal static extern void LuaLOpenLibs(IntPtr luaState);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_objlen")]
		internal static extern int LuaObjLen(IntPtr luaState, int stackPos);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "luaL_loadstring")]
		internal static extern int LuaLLoadString(IntPtr luaState, string chunk);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_createtable")]
		internal static extern void LuaCreateTable(IntPtr luaState, int narr, int nrec);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_settop")]
		internal static extern void LuaSetTop(IntPtr luaState, int newTop);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_insert")]
		internal static extern void LuaInsert(IntPtr luaState, int newTop);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_remove")]
        internal static extern void LuaRemove(IntPtr luaState, int index);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_gettable")]
		internal static extern void LuaGetTable(IntPtr luaState, int index);
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_rawget")]
		internal static extern void Lua_RawGet(IntPtr luaState, int index);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_settable")]
		internal static extern void LuaSetTable(IntPtr luaState, int index);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_rawset")]
		internal static extern void LuaRawSet(IntPtr luaState, int index);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_setmetatable")]
		internal static extern void LuaSetMetaTable(IntPtr luaState, int objIndex);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_getmetatable")]
		internal static extern int LuaGetMetaTable(IntPtr luaState, int objIndex);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_equal")]
		internal static extern int LuaEqual(IntPtr luaState, int index1, int index2);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pushvalue")]
		internal static extern void LuaPushValue(IntPtr luaState, int index);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_replace")]
		internal static extern void LuaReplace(IntPtr luaState, int index);
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_gettop")]
		internal static extern int LuaGetTop(IntPtr luaState);
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_type")]
		internal static extern LuaTypes LuaType(IntPtr luaState, int index);
	
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_isnumber")]
		internal static extern bool LuaIsNumber(IntPtr luaState, int index);
	
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_ref")]
		internal static extern int LuaLRef(IntPtr luaState, int registryIndex);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_rawgeti")]
		internal static extern void LuaRawGetI(IntPtr luaState, int tableIndex, int index);
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_rawseti")]
		internal static extern void LuaRawSetI(IntPtr luaState, int tableIndex, int index);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_touserdata")]
		internal static extern IntPtr LuaToUserdata(IntPtr luaState, int index);
		internal static void lua_getref(IntPtr luaState, int reference)
		{
			LuaLib.LuaRawGetI(luaState,LuaIndexes.LUA_REGISTRYINDEX,reference);
		}
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_unref")]
		internal static extern void LuaLUnRef(IntPtr luaState, int registryIndex, int reference);
		internal static void lua_unref(IntPtr luaState, int reference)
		{
			LuaLib.LuaLUnRef(luaState,LuaIndexes.LUA_REGISTRYINDEX,reference);
		}
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_isstring")]
		internal static extern bool LuaIsString(IntPtr luaState, int index);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pushnil")]
		internal static extern void LuaPushNil(IntPtr luaState);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pushstdcallcfunction")]
		internal static extern void LuaPushStdCallCFunction(IntPtr luaState, IntPtr wrapper);

	

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_call")]
		internal static extern int LuaCall(IntPtr luaState, int nArgs, int nResults);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pcall")]
		internal static extern int LuaPCall(IntPtr luaState, int nArgs, int nResults, int errfunc);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_tonumber")]
		internal static extern double LuaToNumber(IntPtr luaState, int index);
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_toboolean")]
		internal static extern bool LuaToBoolean(IntPtr luaState, int index);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_tolstring")]
		internal static extern IntPtr LuaToLString(IntPtr luaState, int index, out int strLen);

		

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_atpanic")]
		internal static extern void LuaAtPanic(IntPtr luaState, LuaCSFunction panicf);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pushnumber")]
		internal static extern void LuaPushNumber(IntPtr luaState, double number);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pushboolean")]
		internal static extern void LuaPushBoolean(IntPtr luaState, bool value);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_pushstring")]
		internal static extern void LuaPushString(IntPtr luaState, string str);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "luaL_newmetatable")]
		internal static extern int LuaLNewMetaTable(IntPtr luaState, string meta);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_getfield")]
		internal static extern void LuaGetField(IntPtr luaState, int stackPos, string meta);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "luaL_getmetafield")]
		internal static extern bool LuaLGetMetaField(IntPtr luaState, int stackPos, string field);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "luaL_loadbuffer")]
        internal static extern int LuaL_LoadBuffer(IntPtr luaState, string buff, int size, string name);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "luaL_loadbuffer")]
		internal static extern int LuaL_LoadBuffer(IntPtr luaState, byte[] buff, int size, string name);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_checkmetatable")]
		internal static extern bool LuaLCheckMetaTable(IntPtr luaState,int obj);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_error")]
		internal static extern void LuaError(IntPtr luaState);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_checkstack")]
		internal static extern bool LuaCheckStack(IntPtr luaState, int extra);             

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_next")]
		internal static extern int LuaNext(IntPtr luaState,int index);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pushlightuserdata")]
		internal static extern void LuaPushLightUserdata(IntPtr luaState, IntPtr udata);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_where")]
        internal static extern void LuaLWhere(IntPtr luaState, int level);

        // luanet
        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_gettag")]
        internal static extern IntPtr LuaNetGetTag();

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_tonetobject")]
		internal static extern int LuaNetToNetObject(IntPtr luaState,int obj);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_newudata")]
		internal static extern int LuaNetNewUdata(IntPtr luaState,int val);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_rawnetobj")]
		internal static extern int LuaNetRawNetObj(IntPtr luaState,int obj);

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "luanet_checkudata")]
		internal static extern int LuaNetCheckUdata(IntPtr luaState,int obj,string meta);

	}
}
