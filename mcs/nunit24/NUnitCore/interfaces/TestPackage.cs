// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

namespace NUnit.Core
{
	/// <summary>
	/// TestPackage holds information about a set of tests to
	/// be loaded by a TestRunner. It may represent a single
	/// assembly or a set of assemblies. It supports selection
	/// of a single test fixture for loading.
	/// </summary>
	[Serializable]
	public class TestPackage
	{
		private string name;
		private string fullName;

		private ListDictionary settings = new ListDictionary();

		private string basePath;
		private string configFile;
		private string binPath;
		private bool autoBinPath;

		private ArrayList assemblies;
		private string testName;
		private bool isSingleAssembly;


		/// <summary>
		/// Construct a package, specifying the name of the package.
		/// If the package name is an assembly file type (dll or exe)
		/// then the resulting package represents a single assembly.
		/// Otherwise it is a container for multiple assemblies.
		/// </summary>
		/// <param name="name">The name of the package</param>
		public TestPackage( string name )
		{
			this.fullName = name;
			this.name = Path.GetFileName( name );
			this.assemblies = new ArrayList();
			if ( IsAssemblyFileType( name ) )
			{
				this.isSingleAssembly = true;
				this.assemblies.Add( name );
			}
		}

		/// <summary>
		/// Construct a package, specifying the name to be used
		/// and a list of assemblies.
		/// </summary>
		/// <param name="name">The package name, used to name the top-level test node</param>
		/// <param name="assemblies">The list of assemblies comprising the package</param>
		public TestPackage( string name, IList assemblies )
		{
			this.fullName = name;
			this.name = Path.GetFileName( name );
			this.assemblies = new ArrayList( assemblies );
			this.isSingleAssembly = false;
		}

		/// <summary>
		/// Gets the name of the package
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Gets the full name of the package, which is usually
		/// the path to the NUnit project used to create the it
		/// </summary>
		public string FullName
		{
			get { return fullName; }
		}

		/// <summary>
		/// The BasePath to be used in loading the assemblies
		/// </summary>
		public string BasePath
		{
			get { return basePath; }
			set { basePath = value; }
		}

		/// <summary>
		/// The configuration file to be used
		/// </summary>
		public string ConfigurationFile
		{
			get { return configFile; }
			set { configFile = value; }
		}

		/// <summary>
		/// Addditional directories to be probed when loading assemblies
		/// </summary>
		public string PrivateBinPath
		{
			get { return binPath; }
			set { binPath = value; }
		}

		/// <summary>
		/// Indicates whether the probing path should be generated
		/// automatically based on the list of assemblies.
		/// </summary>
		public bool AutoBinPath
		{
			get { return autoBinPath; }
			set { autoBinPath = value; }
		}

		/// <summary>
		/// Assemblies to be loaded. At least one must be specified.
		/// </summary>
		public IList Assemblies
		{
			get { return assemblies; }
		}

		/// <summary>
		/// Return true if the package represents a single assembly.
		/// No root node is displayed in that case.
		/// </summary>
		public bool IsSingleAssembly
		{
			get { return isSingleAssembly; }
		}

		/// <summary>
		/// Fully qualified name of test to be loaded. If not 
		/// specified, all the tests in the assemblies are loaded.
		/// </summary>
		public string TestName
		{
			get { return testName; }
			set { testName = value; }
		}

		/// <summary>
		/// Gets the dictionary of settings for this TestPackage
		/// </summary>
		public IDictionary Settings
		{
			get { return settings; }
		}

		/// <summary>
		/// Return the value of a bool setting or a default.
		/// </summary>
		/// <param name="name">The name of the setting</param>
		/// <param name="defaultSetting">The default value</param>
		/// <returns></returns>
		public bool GetSetting( string name, bool defaultSetting )
		{
			object setting = settings[name];
			
			return setting == null ? defaultSetting : (bool)setting;
		}

		private static bool IsAssemblyFileType( string path )
		{
			string extension = Path.GetExtension( path ).ToLower();
			return extension == ".dll" || extension == ".exe";
		}
	}
}
