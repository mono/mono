// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;

namespace NUnit.Core.Extensibility
{
	/// <summary>
	/// The IFrameworkRegistry allows extensions to register new
	/// frameworks or emulations of other frameworks.
	/// </summary>
	public interface IFrameworkRegistry
	{
		/// <summary>
		/// Register a framework
		/// </summary>
		/// <param name="frameworkName">The name of the framework</param>
		/// <param name="assemblyName">The name of the assembly that the tests reference</param>
		void Register( string frameworkName, string assemblyName );
	}
}
