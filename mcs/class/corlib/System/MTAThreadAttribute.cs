//
// System.MTAThreadAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	[AttributeUsage (AttributeTargets.Method)]
	public sealed class MTAThreadAttribute : Attribute
	{
		// Constructors
		public MTAThreadAttribute () : base ()
		{
		}
	}
}
