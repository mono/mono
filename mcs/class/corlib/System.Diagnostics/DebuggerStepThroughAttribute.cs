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
	[AttributeUsage (AttributeTargets.Constructor |
			 AttributeTargets.Method | AttributeTargets.Property)]
	[Serializable]
	public sealed class DebuggerStepThroughAttribute : Attribute
	{
		public DebuggerStepThroughAttribute ()
			: base ()
			{
			}
	}
}
