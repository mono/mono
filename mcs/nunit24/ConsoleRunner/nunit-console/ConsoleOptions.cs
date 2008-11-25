// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.ConsoleRunner
{
	using System;
	using Codeblast;
	using NUnit.Util;

	public class ConsoleOptions : CommandLineOptions
	{
		public enum DomainUsage
		{
			Default,
			None,
			Single,
			Multiple
		}

		[Option(Short="load", Description = "Test fixture to be loaded")]
		public string fixture;

		[Option(Description = "Name of the test to run")]
		public string run;

		[Option(Description = "Project configuration to load")]
		public string config;

		[Option(Description = "Name of XML output file")]
		public string xml;

		[Option(Description = "Name of transform file")]
		public string transform;

		[Option(Description = "Display XML to the console")]
		public bool xmlConsole;

		[Option(Short="out", Description = "File to receive test output")]
		public string output;

		[Option(Description = "File to receive test error output")]
		public string err;

		[Option(Description = "Label each test in stdOut")]
		public bool labels = false;

		[Option(Description = "List of categories to include")]
		public string include;

		[Option(Description = "List of categories to exclude")]
		public string exclude;

//		[Option(Description = "Run in a separate process")]
//		public bool process;

		[Option(Description = "AppDomain Usage for Tests")]
		public DomainUsage domain;

		[Option(Description = "Disable shadow copy when running in separate domain")]
		public bool noshadow;

		[Option (Description = "Disable use of a separate thread for tests")]
		public bool nothread;

		[Option(Description = "Wait for input before closing console window")]
		public bool wait = false;

		[Option(Description = "Do not display the logo")]
		public bool nologo = false;

		[Option(Description = "Do not display progress" )]
		public bool nodots = false;

		[Option(Short="?", Description = "Display help")]
		public bool help = false;

		public ConsoleOptions( params string[] args ) : base( args ) {}

		public ConsoleOptions( bool allowForwardSlash, params string[] args ) : base( allowForwardSlash, args ) {}

		public bool Validate()
		{
			if(isInvalid) return false; 

			if(NoArgs) return true; 

			if(ParameterCount >= 1) return true; 

			return false;
		}

		protected override bool IsValidParameter(string parm)
		{
			return NUnitProject.CanLoadAsProject( parm ) || PathUtils.IsAssemblyFileType( parm );
		}


        public bool IsTestProject
        {
            get
            {
                return ParameterCount == 1 && NUnitProject.CanLoadAsProject((string)Parameters[0]);
            }
        }

		public override void Help()
		{
			Console.WriteLine();
			Console.WriteLine( "NUNIT-CONSOLE [inputfiles] [options]" );
			Console.WriteLine();
			Console.WriteLine( "Runs a set of NUnit tests from the console." );
			Console.WriteLine();
			Console.WriteLine( "You may specify one or more assemblies or a single" );
			Console.WriteLine( "project file of type .nunit." );
			Console.WriteLine();
			Console.WriteLine( "Options:" );
			base.Help();
			Console.WriteLine();
			Console.WriteLine( "Options that take values may use an equal sign, a colon" );
			Console.WriteLine( "or a space to separate the option from its value." );
			Console.WriteLine();
		}
	}
}