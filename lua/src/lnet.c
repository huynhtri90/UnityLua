
#define LUA_LIB

#include "lua.h"
#include "lauxlib.h"
#include "lnet.h"

#include <string.h>

#if defined _MSC_VER
#include <Windows.h>
#endif

#define lua_popglobaltable(L)  \
	lua_rawseti(L, LUA_REGISTRYINDEX, LUA_RIDX_GLOBALS)

/*
** stdcall C function support
*/

static int tag = 0;
static int tagHook = 0;

static int stdcall_closure (lua_State *L1) 
{
	lua_stdcallCFunction function = (lua_stdcallCFunction) lua_touserdata(L1, lua_upvalueindex(1));
	return function (L1);
}

LUA_API void lua_pushstdcallcfunction (lua_State *L, lua_stdcallCFunction function) 
{
	lua_pushlightuserdata (L, (void *) function);
	lua_pushcclosure (L, stdcall_closure, 1);
}



LUA_API int luaL_checkmetatable(lua_State *L,int index) 
{
	int retVal = 0;

	if(lua_getmetatable (L,index) != 0) 
	{
		lua_pushlightuserdata (L, &tag);
		lua_rawget (L,-2);
		retVal = !lua_isnil (L,-1);
		lua_settop (L,-3);
	}
	return retVal;
}

LUA_API void *luanet_gettag () 
{
	return &tag;
}


// Starting with 5.1 the auxlib version of checkudata throws an exception if the type isn't right
// Instead, we want to run our own version that checks the type and just returns null for failure
static void *checkudata(lua_State *L, int ud, const char *tname)
{
	void *p = lua_touserdata (L, ud);

	if (p != NULL) 
	{  /* value is a userdata? */
		if (lua_getmetatable (L, ud))
		{
			int isEqual;

			/* does it have a metatable? */
			lua_getfield (L, LUA_REGISTRYINDEX, tname);  /* get correct metatable */

			isEqual = lua_rawequal(L, -1, -2);

			lua_pop(L, 2);  /* remove both metatables */

			if (isEqual)   /* does it have the correct mt? */
				return p;
		}
	}

	return NULL;
}

LUA_API int luanet_tonetobject(lua_State *L,int index) 
{
	int *udata;
	if (lua_type(L,index)==LUA_TUSERDATA) 
	{
		if (luaL_checkmetatable(L,index)) 
		{
			udata = (int*)lua_touserdata (L,index);
			if (udata != NULL) 
				return *udata;
		}

		udata = (int*)checkudata (L, index, "luaNet_class");
		if (udata!=NULL) 
			return *udata;

		udata = (int*)checkudata (L, index, "luaNet_searchbase");
		if (udata!=NULL) 
			return *udata;

		udata = (int*)checkudata(L, index, "luaNet_function");
		if (udata!=NULL) 
			return *udata;
	}
	return -1;
}

LUA_API void luanet_newudata (lua_State *L,int val) 
{
	int* pointer = (int*)lua_newuserdata (L, sizeof(int));
	*pointer=val;
}

LUA_API int luanet_rawnetobj (lua_State *L,int index) {
	int *udata = (int *)lua_touserdata (L,index);

	if(udata!=NULL) 
		return *udata;
	return -1;
}

LUA_API int luanet_checkudata (lua_State *L,int index,const char *meta) 
{
	int *udata = (int*)checkudata (L, index, meta);
	if(udata!=NULL) 
		return *udata;
	return -1;
}

LUA_API int luanet_registryindex () 
{
	return LUA_REGISTRYINDEX;
}

LUA_API void luanet_pushglobaltable (lua_State *L) {
	lua_pushglobaltable (L);
}

LUA_API void luanet_popglobaltable (lua_State *L) {
	lua_popglobaltable (L);
}

LUA_API int luanet_pcall (lua_State *L, int nargs, int nresults, int errfunc)
{
	return lua_pcall (L, nargs, nresults, errfunc);
}
LUA_API double luanet_tonumber (lua_State *L, int idx)
{
	return lua_tonumber (L, idx);
}

LUA_API int luanet_equal (lua_State *L, int idx1, int idx2)
{
	return lua_compare(L,(idx1),(idx2),LUA_OPEQ);
}

static void hook_trampoline (lua_State *L, lua_Debug *ar)
{
	lua_pushlightuserdata (L, &tagHook);
	lua_gettable (L, LUA_REGISTRYINDEX);
	lua_stdcallHook hook = (lua_stdcallHook) lua_touserdata (L, -1);
	hook (L, ar);
}

LUA_API int luanet_sethook (lua_State *L, lua_stdcallHook func, int mask, int count)
{
	lua_pushlightuserdata (L, &tagHook);
	lua_pushlightuserdata (L, (void*)func);
	lua_settable(L, LUA_REGISTRYINDEX);
	return lua_sethook(L, hook_trampoline, mask, count);
}

LUA_API lua_State* luanet_get_main_state(lua_State* L1)
{
	lua_rawgeti(L1, LUA_REGISTRYINDEX, LUA_RIDX_MAINTHREAD);
	lua_State* L = lua_tothread(L1, -1);
	lua_pop(L1, 1);
	return L;
}

LUA_API int luanet_isstring_strict(lua_State *L, int idx) {
	int t = lua_type(L, idx);
	return (t == LUA_TSTRING);
}
