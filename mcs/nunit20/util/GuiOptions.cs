namespace NUnit.Util
{
	using System;
	using Codeblast;

	public class GuiOptions : CommandLineOptions
	{
		private bool isInvalid = false; 

		[Option(Short="?", Description = "Display help")]
		public bool help = false;

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
			return (NoArgs || ParameterCount == 1) && !isInvalid;
		}
	}
}