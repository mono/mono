//
// System.Runtime.InteropServices.SetWin32ContextInIDispatchAttribute.cs
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) Novell, Inc.  http://www.ximian.com
//

#if NET_2_0

using System;

namespace System.Runtime.InteropServices 
{
	[AttributeUsage (AttributeTargets.Assembly, Inherited = false)]
	public sealed class SetWin32ContextInIDispatchAttribute : Attribute
	{
		public SetWin32ContextInIDispatchAttribute ()
		{
		}
	}
}

#endif
