using System;

namespace NUnit.Framework
{
	/// <summary>
	/// ExplicitAttribute marks a test or test fixture so that it will
	/// only be run if explicitly executed from the gui or command line
	/// or if it is included by use of a filter. The test will not be
	/// run simply because an enclosing suite is run.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method, AllowMultiple=false)]
	public sealed class ExplicitAttribute : Attribute
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ExplicitAttribute()
		{
		}
	}
}
