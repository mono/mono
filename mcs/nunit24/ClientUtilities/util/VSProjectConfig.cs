// ****************************************************************
// Copyright 2002-2003, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Collections.Specialized;

namespace NUnit.Util
{
	/// <summary>
	/// Originally, we used the same ProjectConfig class for both
	/// NUnit and Visual Studio projects. Since we really do very
	/// little with VS Projects, this class has been created to 
	/// hold the name and the collection of assembly paths.
	/// </summary>
	public class VSProjectConfig
	{
		private string name;
		
		private StringCollection assemblies = new StringCollection();

		public VSProjectConfig( string name )
		{
			this.name = name;
		}

		public string Name
		{
			get { return name; }
		}

		public StringCollection Assemblies
		{
			get { return assemblies; }
		}
	}
}
