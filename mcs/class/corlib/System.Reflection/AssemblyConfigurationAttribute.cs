//
// System.Reflection.AssemblyConfigurationAttribute.cs
//
// Author: Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyConfigurationAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyConfigurationAttribute (string configuration)
		{
			name = configuration;
		}
		
		// Properties
		public string Configuration
		{
			get { return name; }
		}
	}
}
