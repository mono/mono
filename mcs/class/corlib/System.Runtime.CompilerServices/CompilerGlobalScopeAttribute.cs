//
// System.Runtime.CompilerServices.CompilerGlobalScopeAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Copyright, Ximian Inc.

using System;

namespace System.Runtime.CompilerServices {

	[AttributeUsage (AttributeTargets.Class)] [Serializable]
	public class CompilerGlobalScopeAttribute : Attribute
	{
		public CompilerGlobalScopeAttribute ()
		{
		}
	}
}
