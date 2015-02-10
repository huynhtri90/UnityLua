using System;
using System.Runtime.InteropServices;
/*
#if UNITY_3D
*/
namespace NLua {
	/// <summary>
	/// Disables CLS Compliance in Unity3D.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.All)]
	[ComVisible(true)]
	public class CLSCompliantAttribute : Attribute {
		private bool _compliant;
		public CLSCompliantAttribute(bool isCompliant) { _compliant = isCompliant; }
		public bool IsCompliant { get { return _compliant; } }
	}

	#pragma warning disable 414
	public class MonoPInvokeCallbackAttribute : System.Attribute
	{
		private Type type;
		internal MonoPInvokeCallbackAttribute( Type t ) { type = t; }
	}
	#pragma warning restore 414
}
/*
#endif
*/