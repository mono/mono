//
// System.Reflection.AssemblyDelaySignAttribute.cs
//
// Author: Duncan Mak <duncan@ximian.com>
//
// (C) Ximian, Inc. http://www.ximian.com
//


namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyDelaySignAttribute : Attribute
	{
		// Field
		private bool delay;
		
		// Constructor
		public AssemblyDelaySignAttribute (bool delaySign)
		{
			delay = delaySign;
		}

		// Property
		public bool DelaySIgn
		{
			get { return delay; }
		}
	}
}

       
