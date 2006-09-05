using System;
using System.Collections;
using Mono.GetOptions;

namespace GetOptTest
{
	class GetOptTestOptions : Options
	{
		[Option(3, "Just a testing Parameter", 'p')]
		public string[] param = new string[] { "Default value" };

		[Option("Just a boolean testing parameter", 't')]
		public bool turnItOn = false;

		private bool verboseOn = false;

		[Option("Be verbose", 'v')]
		public bool verbose
		{
			set 
			{ 
				verboseOn = value; 
				Console.WriteLine("verbose was set to : " + verboseOn);
			}
		}

		[Option(-1, "Execute a test routine", 's', null)]
		public WhatToDoNext simpleProcedure(int dids)
		{
			Console.WriteLine("Inside simpleProcedure({0})", dids);
			return WhatToDoNext.GoAhead;
		}

		[Option("Show usage syntax", 'u', "usage")]
		public override WhatToDoNext DoUsage()
		{
			base.DoUsage();
			return WhatToDoNext.GoAhead; 
		}

		public override WhatToDoNext DoHelp() // uses parent's OptionAttribute as is
		{
			base.DoHelp();
			return WhatToDoNext.GoAhead; 
		}

		public GetOptTestOptions()
		{
			this.ParsingMode = OptionsParsingMode.Both;
		}
	}

	/// <summary>
	/// Summary description for GetOptTester.
	/// </summary>
	class GetOptTester 
	{

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Console.WriteLine("------------ Original 'args'");
			for(int i = 0; i < args.Length; i++)
				Console.WriteLine("args[{0}] = \"{1}\"",i,args[i]);
			Console.WriteLine("----------------------------------------");
			Console.WriteLine("------------ GetOptions Processing");
			GetOptTestOptions options = new GetOptTestOptions(); 
			options.ProcessArgs(args);
			Console.WriteLine("----------------------------------------");
			Console.WriteLine("------------ Results");
			if (options.param != null)
			{
				Console.WriteLine("Parameters supplied for 'param' were:");
				foreach (string Parameter in options.param)
					Console.WriteLine("\t" + Parameter);
			}
			for(int i = 0; i < options.RemainingArguments.Length; i++)
				Console.WriteLine("remaining args[{0}] = \"{1}\"",i,options.RemainingArguments[i]);
			Console.WriteLine("----------------------------------------");
		}
	}
}
