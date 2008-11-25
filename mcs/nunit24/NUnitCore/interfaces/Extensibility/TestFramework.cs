// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// Summary description for TestFramework.
	/// </summary>
	[Serializable]
	public class TestFramework
	{
		#region Instance Fields
		/// <summary>
		/// The name of the framework
		/// </summary>
		public string Name;

		/// <summary>
		/// The file name of the assembly that defines the framwork
		/// </summary>
		public string AssemblyName;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructs a TestFramwork object given its name and assembly name.
		/// </summary>
		/// <param name="frameworkName"></param>
		/// <param name="assemblyName"></param>
		public TestFramework( string frameworkName, string assemblyName ) 
		{
			this.Name = frameworkName;
			this.AssemblyName = assemblyName;
		}
		#endregion
	}
}
