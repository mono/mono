#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Util
{
	using System;
	using Codeblast;

	public class ConsoleOptions : CommandLineOptions
	{
		[Option(Description = "Fixture to test")]
		public string fixture;

		[Option(Description = "Name of XML output file")]
		public string xml;

		[Option(Description = "Name of transform file")]
		public string transform;

		[Option(Description = "Do not display the logo")]
		public bool nologo = false;

		[Option(Short="?", Description = "Display help")]
		public bool help = false;

		[Option(Description = "Require input to close console window")]
		public bool wait = false;

		[Option(Description = "Display XML to the console")]
		public bool xmlConsole;

		[Option(Description = "Project configuration to load")]
		public string config;

		private bool isInvalid = false; 

		public ConsoleOptions(String[] args) : base(args) 
		{}

		protected override void InvalidOption(string name)
		{
			isInvalid = true;
		}

		public bool Validate()
		{
			if(isInvalid) return false; 

			if(NoArgs) return true; 

			if(IsFixture) return true; 

			if(ParameterCount >= 1) return true; 

			return false;
		}

		public bool IsAssembly 
		{
			get 
			{
				return ParameterCount >= 1 && !IsFixture;
			}
		}

		public bool IsTestProject
		{
			get
			{
				return ParameterCount == 1 && NUnitProject.CanLoadAsProject( (string)Parameters[0] );
			}
		}

		public bool IsFixture 
		{
			get 
			{
				return ParameterCount >= 1 && 
					   ((fixture != null) && (fixture.Length > 0));
			}
		}

		public bool IsXml 
		{
			get 
			{
				return (xml != null) && (xml.Length != 0);
			}
		}

		public bool IsTransform 
		{
			get 
			{
				return (transform != null) && (transform.Length != 0);
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
		}
	}
}
