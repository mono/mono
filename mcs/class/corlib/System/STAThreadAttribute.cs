//
// System.STAThreadAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System
{
     [AttributeUsage (AttributeTargets.Method)]
	public class STAThreadAttribute : Attribute
	{
		   // Constructors
		   public STAThreadAttribute () : base ()
		   {
		   }
	}
}
