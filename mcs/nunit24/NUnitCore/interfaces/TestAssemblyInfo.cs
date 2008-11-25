// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Text;

namespace NUnit.Core
{
	/// <summary>
	/// TestAssemblyInfo holds information about a loaded test assembly
	/// </summary>
	[Serializable]
	public class TestAssemblyInfo
	{
		private string assemblyName;
		private Version runtimeVersion;
		private IList testFrameworks;

        /// <summary>
        /// Constructs a TestAssemblyInfo
        /// </summary>
        /// <param name="assemblyName">The name of the assembly</param>
        /// <param name="runtimeVersion">The version of the runtime for which the assembly was built</param>
        /// <param name="testFrameworks">A list of test framework useds by the assembly</param>
		public TestAssemblyInfo( string assemblyName, Version runtimeVersion, IList testFrameworks )
		{
			this.assemblyName = assemblyName;
			this.runtimeVersion = runtimeVersion;
			this.testFrameworks = testFrameworks;
		}

        /// <summary>
        /// Gets the name of the assembly
        /// </summary>
		public string Name
		{
			get { return assemblyName; }
		}

        /// <summary>
        /// Gets the runtime version for which the assembly was built
        /// </summary>
		public Version RuntimeVersion
		{
			get { return runtimeVersion; }
		}

        /// <summary>
        /// Gets a list of testframeworks referenced by the assembly
        /// </summary>
		public IList TestFrameworks
		{
			get { return testFrameworks; }
		}
    }
}
