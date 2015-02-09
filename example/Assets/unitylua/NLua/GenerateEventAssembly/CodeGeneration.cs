/*
 * This file is part of NLua.
 * 
 * Copyright (c) 2014 Vinicius Jarina (viniciusjarina@gmail.com)
 * Copyright (C) 2003-2005 Fabio Mascarenhas de Queiroz.
 * Copyright (C) 2012 Megax <http://megax.yeahunter.hu/>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System;
using System.Linq;
using System.Threading;
using System.Reflection;
using NLua.Extensions;
using System.Collections;
using System.Collections.Generic;
using NLua.Method;

namespace NLua
{
	/*
	 * Dynamically generates new types from existing types and
	 * Lua function and table values. Generated types are event handlers, 
	 * delegates, interface implementations and subclasses.
	 * 
	 * Author: Fabio Mascarenhas
	 * Version: 1.0
	 */
	class CodeGeneration
	{
		private Dictionary<Type, LuaClassType> classCollection = new Dictionary<Type, LuaClassType> ();
		private Dictionary<Type, Type> delegateCollection = new Dictionary<Type, Type> ();
		private static readonly CodeGeneration instance = new CodeGeneration ();
		private AssemblyName assemblyName;

		static CodeGeneration ()
		{
		}

		private CodeGeneration ()
		{
			// Create an assembly name
			assemblyName = new AssemblyName ();
			assemblyName.Name = "NLua_generatedcode";
		}

		/*
		 * Singleton instance of the class
		 */
		public static CodeGeneration Instance {
			get { return instance; }
		}
		
		/*
		 *  Generates an event handler that calls a Lua function
		 */
		private Type GenerateEvent (Type eventHandlerType)
		{
			throw new NotImplementedException (" Emit not available on Unity");
		}

		/*
		 * Generates a type that can be used for instantiating a delegate
		 * of the provided type, given a Lua function.
		 */
		private Type GenerateDelegate (Type delegateType)
		{
			throw new NotImplementedException ("GenerateDelegate is not available on iOS, please register your LuaDelegate type with Lua.RegisterLuaDelegateType( yourDelegate, theLuaDelegateHandler) ");
		}

		void GetReturnTypesFromClass (Type klass, out Type[][] returnTypes)
		{
			var classMethods = klass.GetMethods ();
			returnTypes = new Type[classMethods.Length][];

			int i = 0;

			foreach (var method in classMethods) {
				if (klass.IsInterface ()) {
					GetReturnTypesFromMethod (method, out returnTypes [i]);
					i++;
				} else {
					if (!method.IsPrivate && !method.IsFinal && method.IsVirtual) {
						GetReturnTypesFromMethod (method, out returnTypes [i]);
						i++;
					}
				}
			}
		}

		/*
		 * Generates an implementation of klass, if it is an interface, or
		 * a subclass of klass that delegates its virtual methods to a Lua table.
		 */
		public void GenerateClass (Type klass, out Type newType, out Type[][] returnTypes)
		{
			throw new NotImplementedException (" Emit not available on Unity ");
		}

		void GetReturnTypesFromMethod (MethodInfo method, out Type[] returnTypes)
		{
			var paramInfo = method.GetParameters ();
			var paramTypes = new Type[paramInfo.Length];
			var returnTypesList = new List<Type> ();
			
			// Counts out and ref parameters, for later use, 
			// and creates the list of return types
			int nOutParams = 0;
			int nOutAndRefParams = 0;
			var returnType = method.ReturnType;
			returnTypesList.Add (returnType);
			
			for (int i = 0; i < paramTypes.Length; i++) {
				paramTypes [i] = paramInfo [i].ParameterType;
				if ((!paramInfo [i].IsIn) && paramInfo [i].IsOut) {
					nOutParams++;
				}
				
				if (paramTypes [i].IsByRef) {
					returnTypesList.Add (paramTypes [i].GetElementType ());
					nOutAndRefParams++;
				}
			}

			returnTypes = returnTypesList.ToArray ();
		}

		/*
		 * Gets an event handler for the event type that delegates to the eventHandler Lua function.
		 * Caches the generated type.
		 */
		public LuaEventHandler GetEvent (Type eventHandlerType, LuaFunction eventHandler)
		{
			throw new NotImplementedException (" Emit not available on Unity ");
		}

		public void RegisterLuaDelegateType (Type delegateType, Type luaDelegateType)
		{
			delegateCollection [delegateType] = luaDelegateType;
		}

		public void RegisterLuaClassType (Type klass, Type luaClass)
		{
			LuaClassType luaClassType = new LuaClassType ();
			luaClassType.klass = luaClass;
			GetReturnTypesFromClass (klass, out luaClassType.returnTypes);
			classCollection [klass] = luaClassType;
		}
		/*
		 * Gets a delegate with delegateType that calls the luaFunc Lua function
		 * Caches the generated type.
		 */
		public Delegate GetDelegate (Type delegateType, LuaFunction luaFunc)
		{
			var returnTypes = new List<Type> ();
			Type luaDelegateType;

			if (delegateCollection.ContainsKey (delegateType)) 
				luaDelegateType = delegateCollection [delegateType];
			else {
				luaDelegateType = GenerateDelegate (delegateType);
				delegateCollection [delegateType] = luaDelegateType;
			}

			var methodInfo = delegateType.GetMethod ("Invoke");
			returnTypes.Add (methodInfo.ReturnType);

			foreach (ParameterInfo paramInfo in methodInfo.GetParameters()) {
				if (paramInfo.ParameterType.IsByRef)
					returnTypes.Add (paramInfo.ParameterType);
			}

			var luaDelegate = (LuaDelegate)Activator.CreateInstance (luaDelegateType);
			luaDelegate.function = luaFunc;
			luaDelegate.returnTypes = returnTypes.ToArray ();

			return Delegate.CreateDelegate (delegateType, luaDelegate, "CallFunction");
		}

		/*
		 * Gets an instance of an implementation of the klass interface or
		 * subclass of klass that delegates public virtual methods to the
		 * luaTable table.
		 * Caches the generated type.
		 */
		public object GetClassInstance (Type klass, LuaTable luaTable)
		{
			LuaClassType luaClassType;

			if (classCollection.ContainsKey (klass)) 
				luaClassType = classCollection [klass];
			else {
				luaClassType = new LuaClassType ();
				GenerateClass (klass, out luaClassType.klass, out luaClassType.returnTypes);
				classCollection [klass] = luaClassType;
			}

			return Activator.CreateInstance (luaClassType.klass, new object[] {
				luaTable,
				luaClassType.returnTypes
			});
		}
	}
}