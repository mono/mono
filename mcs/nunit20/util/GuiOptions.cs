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
	using System.Text;
	using Codeblast;

	public class GuiOptions : CommandLineOptions
	{
		private bool isInvalid = false; 

		[Option(Short="?", Description = "Display help")]
		public bool help = false;

		[Option(Description = "Project configuration to load")]
		public string config;

		[Option(Description = "Suppress loading of last project")]
		public bool noload;

		[Option(Description = "Automatically run the loaded project")]
		public bool run;

		[Option(Description = "Fixture to test")]
		public string fixture;

		public GuiOptions(String[] args) : base(args) 
		{}

		protected override void InvalidOption(string name)
		{ isInvalid = true; }

		public string Assembly
		{
			get 
			{
				return (string)Parameters[0];
			}
		}

		public bool IsAssembly
		{
			get 
			{
				return ParameterCount == 1;
			}
		}

		public bool Validate()
		{
			return (NoArgs || ParameterCount <= 1) && !isInvalid;
		}

		public override string GetHelpText()
		{
			const string initialText =
				"NUNIT-GUI [inputfile] [options]\r\rRuns a set of NUnit tests from the console. You may specify\ran assembly or a project file of type .nunit as input.\r\rOptions:\r";

			const string finalText = 
				"\rOptions that take values may use an equal sign, a colon\ror a space to separate the option from its value.";

			return initialText + base.GetHelpText() + finalText;
		}

	}
}