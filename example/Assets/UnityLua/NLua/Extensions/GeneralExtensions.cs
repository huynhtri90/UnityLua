/*
 * This file is part of NLua.
 * 
 * Copyright (c) 2014 Vinicius Jarina (viniciusjarina@gmail.com)
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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NLua.Extensions
{
	/// <summary>
	/// Some random extension stuff.
	/// </summary>
	static class CheckNull
	{
		/// <summary>
		/// Determines whether the specified obj is null.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns>
		/// 	<c>true</c> if the specified obj is null; otherwise, <c>false</c>.
		/// </returns>
		/// 
		
#if USE_KOPILUA
		public static bool IsNull (object obj)
		{
			return (obj == null);
		}
#else

		public static bool IsNull (IntPtr ptr)
		{
			return (ptr.Equals (IntPtr.Zero));
		}
#endif
	}

	static class TypeExtensions
	{
		public static bool HasMethod (this Type t, string name)
		{
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

            MethodInfo[] res = CacheReflection.GetMethodInfoCache(t, name, flags);

            if(res == null)
            {
                res = t.GetMethods(flags).Where(m => m.Name == name).ToArray();
                CacheReflection.UpdateMethodInfoCache(t, name, flags, res);
            }

            for (int i = 0; i < res.Length; i++)
            {
                if (res[i].Name == name)
                    return true;
            }

            return false;
        }

		public static bool HasAdditionOpertator (this Type t)
		{
			if (t.IsPrimitive ()) 
				return true;

			return t.HasMethod ("op_Addition");
		}

		public static bool HasSubtractionOpertator (this Type t)
		{
			if (t.IsPrimitive ())
				return true;

			return t.HasMethod ("op_Subtraction");
		}

		public static bool HasMultiplyOpertator (this Type t)
		{
			if (t.IsPrimitive ())
				return true;

			return t.HasMethod ("op_Multiply");
		}

		public static bool HasDivisionOpertator (this Type t)
		{
			if (t.IsPrimitive ())
				return true;

			return t.HasMethod ("op_Division");
		}

		public static bool HasModulusOpertator (this Type t)
		{
			if (t.IsPrimitive ())
				return true;

			return t.HasMethod ("op_Modulus");
		}

		public static bool HasUnaryNegationOpertator (this Type t)
		{
			if (t.IsPrimitive ())
				return true;
			// Unary - will always have only one version.
			var op = t.GetMethod ("op_UnaryNegation", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
			return op != null;
		}

		public static bool HasEqualityOpertator (this Type t)
		{
			if (t.IsPrimitive ())
				return true;
			return t.HasMethod ("op_Equality");
		}

		public static bool HasLessThanOpertator (this Type t)
		{
			if (t.IsPrimitive ())
				return true;

			return t.HasMethod ("op_LessThan");
		}

		public static bool HasLessThanOrEqualOpertator (this Type t)
		{
			if (t.IsPrimitive ())
				return true;
			return t.HasMethod ("op_LessThanOrEqual");
		}

		public static MethodInfo [] GetMethods (this Type t, string name, BindingFlags flags)
		{
            MethodInfo[] res = CacheReflection.GetMethodInfoCache(t, name, flags);
            if (res != null)
                return res;

            res = t.GetMethods(flags).Where(m => m.Name == name).ToArray();

            CacheReflection.UpdateMethodInfoCache(t, name, flags, res);
            return res;
		}

		public static MethodInfo [] GetExtensionMethods (this Type type, IEnumerable<Assembly> assemblies = null)
		{
			List<Type> types = new List<Type> ();

			types.AddRange (type.GetAssembly().GetTypes ().Where (t => t.IsPublic ()));

			if (assemblies != null) {
				foreach (Assembly item in assemblies) {
					if (item == type.GetAssembly ())
						continue;
					types.AddRange (item.GetTypes ().Where (t => t.IsPublic ()));
				}
			}

			var query = from extensionType in types
						where extensionType.IsSealed() && !extensionType.IsGenericType() && !extensionType.IsNested
						from method in extensionType.GetMethods (BindingFlags.Static | BindingFlags.Public)
						where method.IsDefined (typeof (ExtensionAttribute), false)
						where method.GetParameters () [0].ParameterType == type
						select method;
			return query.ToArray<MethodInfo> ();
		}

		/// <summary>
		/// Extends the System.Type-type to search for a given extended MethodeName.
		/// </summary>
		/// <param name="MethodeName">Name of the Methode</param>
		/// <returns>the found Method or null</returns>
		public static MethodInfo GetExtensionMethod (this Type t, string name, IEnumerable<Assembly> assemblies = null)
		{
			var mi = from methode in t.GetExtensionMethods (assemblies)
					 where methode.Name == name
					 select methode;
			if (!mi.Any<MethodInfo> ())
				return null;
			else
				return mi.First<MethodInfo> ();
		}

		public static bool IsPrimitive (this Type t)
		{
#if NETFX_CORE
			return t.GetTypeInfo ().IsPrimitive;
#else
			return t.IsPrimitive;
#endif
		}

		public static bool IsClass (this Type t)
		{
#if NETFX_CORE
			return t.GetTypeInfo ().IsClass;
#else
			return t.IsClass;
#endif
		}

		public static bool IsEnum (this Type t)
		{
#if NETFX_CORE
			return t.GetTypeInfo ().IsEnum;
#else
			return t.IsEnum;
#endif
		}

		public static bool IsPublic (this Type t)
		{
#if NETFX_CORE
			return t.GetTypeInfo ().IsPublic;
#else
			return t.IsPublic;
#endif
		}

		public static bool IsSealed (this Type t)
		{
#if NETFX_CORE
			return t.GetTypeInfo ().IsSealed;
#else
			return t.IsSealed;
#endif
		}

		public static bool IsGenericType (this Type t)
		{
#if NETFX_CORE
			return t.GetTypeInfo ().IsGenericType;
#else
			return t.IsGenericType;
#endif
		}

		public static bool IsInterface (this Type t)
		{
#if NETFX_CORE
			return t.GetTypeInfo ().IsInterface;
#else
			return t.IsInterface;
#endif
		}		

		public static Assembly GetAssembly (this Type t)
		{
#if NETFX_CORE
			return t.GetTypeInfo ().Assembly;
#else
			return t.Assembly;
#endif
		}
	}

	static class StringExtensions
	{
		public static IEnumerable<string> SplitWithEscape (this string input, char separator, char escapeCharacter)
		{
			int start = 0;
			int index = 0;
			while (index < input.Length) {
				index = input.IndexOf (separator, index);
				if (index == -1)
					break;

				if (input [index - 1] == escapeCharacter) {
					input = input.Remove (index - 1, 1);
					continue;
				}


				yield return input.Substring (start, index - start);
				index++;
				start = index;
			}
			yield return input.Substring (start);
		}
	}

    static class CacheReflection
    {
        public static Dictionary<string, MethodInfo[]> SCacheGetMethod = new Dictionary<string, MethodInfo[]>();
        public static Dictionary<Type, string> SCacheAssemblyQualifiedName = new Dictionary<Type, string>();
        public static Dictionary<string, string[]> SCacheFullPathToArray = new Dictionary<string, string[]>();


        public static MethodInfo[] GetMethodInfoCache(this Type t, string name, BindingFlags flags)
        {
            string key = t.FullName + "_" + name + ((int)flags).ToString();

            if (SCacheGetMethod.ContainsKey(key))
                return SCacheGetMethod[key];

            return null;
        }

        public static void UpdateMethodInfoCache(this Type t, string name, BindingFlags flags, MethodInfo[] value)
        {
            string key = t.FullName + "_" + name + ((int)flags).ToString();
            SCacheGetMethod[key] = value;
        }

        public static string GetAssemblyQualifiedName(Type type)
        {
            if (SCacheAssemblyQualifiedName.ContainsKey(type))
                return SCacheAssemblyQualifiedName[type];

            string name = type.AssemblyQualifiedName;
            SCacheAssemblyQualifiedName[type] = name;

            return name;
        }

        public static string[] GetFullPathToArrayCache(string key)
        {
            if (SCacheFullPathToArray.ContainsKey(key))
                return SCacheFullPathToArray[key];

            return null;
        }

        public static void SetFullPathToArrayCache(string key, string[] data)
        {
            SCacheFullPathToArray[key] = data;
        }

    }
}