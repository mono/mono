namespace NUnit.Util
{
	using System;
	using Codeblast;

	public class ConsoleOptions : CommandLineOptions
	{
		[Option(Description = "Fixture to test")]
		public string fixture;

		[Option(Description = "Name of Xml output file")]
		public string xml;

		[Option(Description = "Name of transform file")]
		public string transform;

		[Option(Description = "Do not display the logo")]
		public bool nologo = false;

		[Option(Short="?", Description = "Display help")]
		public bool help = false;

		[Option(Description = "Require input to close console window")]
		public bool wait = false;

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

			if(ParameterCount == 1) return true; 

			return false;
		}

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
				return ParameterCount == 1 && !IsFixture;
			}
		}

		public bool IsFixture 
		{
			get 
			{
				return ParameterCount == 1 && 
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
	}
}