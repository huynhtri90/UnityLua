using UnityEngine;
using System.Collections;
using NLua;
using System.Text;
using System.IO;
using System.Collections.Generic;
using AOT;

namespace X
{
	public class RequireExampleV1 : MonoBehaviour
	{
		private Lua LuaState;
		private LuaTable TestLib;

        LuaFunction StartCallback;
        LuaFunction UpdateCallback;

        private static Dictionary<int, object> Context = new Dictionary<int, object>();


        void Start()
        {
            LuaState = new Lua();

            //No need Expand CLR Package
            //LuaState.LoadCLRPackage();

            //Binding Print, Asset load func
            LuaState.LoadUnityExpand();

            //Register
            CustomRegisterFunction();

            //Update context DB
            Context[gameObject.GetInstanceID()] = gameObject;

            var ret = LuaState.DoString(@"return require 'requiretestV1'");
            TestLib = ret[0] as LuaTable;

            StartCallback = TestLib["Start"] as LuaFunction;
            UpdateCallback = TestLib["Update1"] as LuaFunction;

            StartCallback.Call(this.gameObject.GetInstanceID());
        }

        void Update()
        {
            UpdateCallback.Call(this.gameObject.GetInstanceID());
        }


        //-----------------------------------------------------------------------------------------------------------------------//
        // Lua Register Funcion
        //-----------------------------------------------------------------------------------------------------------------------//

        private void CustomRegisterFunction()
        {
            LuaState.RegisterFunction("getContextObjProp", new KeraLua.LuaNativeFunction(QueryObject));
            LuaState.RegisterFunction("moveTo", new KeraLua.LuaNativeFunction(MoveTo));
        }

        [MonoPInvokeCallback(typeof(KeraLua.LuaNativeFunction))]
        public static int QueryObject(KeraLua.LuaState luaState)
        {
            int udid = (int)LuaLib.LuaToNumber(luaState, 1);
            int prop = (int)LuaLib.LuaToNumber(luaState, 2);

            GameObject tmp = Context[udid] as GameObject;

            if (tmp == null)
            {
                LuaLib.LuaPushNil(luaState);
                return 1;
            }

            if (prop == 1)
                LuaLib.LuaPushString(luaState, tmp.name);
            else if (prop == 2)
                LuaLib.LuaPushString(luaState, tmp.tag);
            else if (prop == 3)
            {
                LuaLib.LuaPushNumber(luaState, tmp.transform.position.x);
                LuaLib.LuaPushNumber(luaState, tmp.transform.position.y);
                LuaLib.LuaPushNumber(luaState, tmp.transform.position.z);
                return 3;
            }
                

            return 1;
        }

        public static int MoveTo(KeraLua.LuaState luaState)
        {
            int udid = (int)LuaLib.LuaToNumber(luaState, 1);

            float x = (float)LuaLib.LuaToNumber(luaState, 2);
            float y = (float)LuaLib.LuaToNumber(luaState, 3);
            float z = (float)LuaLib.LuaToNumber(luaState, 4);

            GameObject tmp = Context[udid] as GameObject;

            if (tmp == null)
            {
                LuaLib.LuaPushNil(luaState);
                return 1;
            }

            tmp.transform.position = Vector3.MoveTowards(tmp.transform.position, new Vector3(x, y, z), Time.deltaTime);

            return 1;
        }

    }
}
