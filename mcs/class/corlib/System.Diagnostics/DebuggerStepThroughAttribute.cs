//
// System.Diagnostics.DebuggerStepThroughAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Diagnostics
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct |
                         AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class DebuggerStepThroughAttribute : Attribute
	{
		public DebuggerStepThroughAttribute ()
			: base ()
			{
			}
	}
}
