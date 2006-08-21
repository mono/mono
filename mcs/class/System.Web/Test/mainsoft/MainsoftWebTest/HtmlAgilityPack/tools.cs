// HtmlAgilityPack V1.0 - Simon Mourier <simonm@microsoft.com>
using System;
using System.Diagnostics;
using System.IO;

namespace HtmlAgilityPack
{
	internal struct IOLibrary
	{
		internal static void MakeWritable(string path)
		{
			if (!File.Exists(path))
				return;
			File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly);
		}

		internal static void CopyAlways(string source, string target)
		{
			if (!File.Exists(source))
				return;
			Directory.CreateDirectory(Path.GetDirectoryName(target));
			MakeWritable(target);
			File.Copy(source, target, true);
		}
    }
#if TARGET_JVM1
    internal struct HtmlLibrary
	{
		[Conditional("DEBUG")]
		internal static void GetVersion(out string version)
		{
			System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(1, true);
			version = sf.GetMethod().DeclaringType.Assembly.GetName().Version.ToString();
		}

		[Conditional("DEBUG")]
		[Conditional("TRACE")]
		internal static void Trace(object Value)
		{
			// category is the method
			string name = null;
			GetCurrentMethodName(2, out name);
			System.Diagnostics.Trace.WriteLine(Value, name);
		}

		[Conditional("DEBUG")]
		[Conditional("TRACE")]
		internal static void TraceStackFrame(int steps)
		{
			string name = null;
			GetCurrentMethodName(2, out name);
			string trace = "";
			for(int i=1;i<steps;i++)
			{
				System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(i, true);
				trace += sf.ToString();
			}
			System.Diagnostics.Trace.WriteLine(trace, name);
			System.Diagnostics.Trace.WriteLine("");
		}

		[Conditional("DEBUG")]
		internal static void GetCurrentMethodName(out string name)
		{
			name = null;
			GetCurrentMethodName(2, out name);
		}

		[Conditional("DEBUG")]
		internal static void GetCurrentMethodName(int skipframe, out string name)
		{
			StackFrame sf = new StackFrame(skipframe, true);
			name = sf.GetMethod().DeclaringType.Name + "." + sf.GetMethod().Name;
		}

    }
#endif
    
    internal class HtmlCmdLine
	{
		static internal bool Help;

		static HtmlCmdLine()
		{
			Help = false;
			ParseArgs();
		}

		internal static string GetOption(string name, string def)
		{
			string p = def;
			string[] args = Environment.GetCommandLineArgs();
			for (int i=1;i<args.Length;i++)
			{
				GetStringArg(args[i], name, ref p);
			}
			return p;
		}

		internal static string GetOption(int index, string def)
		{
			string p = def;
			string[] args = Environment.GetCommandLineArgs();
			int j = 0;
			for (int i=1;i<args.Length;i++)
			{
				if (GetStringArg(args[i], ref p))
				{
					if (index==j)
						return p;
					else
						p = def;
					j++;
				}
			}
			return p;
		}

		internal static bool GetOption(string name, bool def)
		{
			bool p = def;
			string[] args = Environment.GetCommandLineArgs();
			for (int i=1;i<args.Length;i++)
			{
				GetBoolArg(args[i], name, ref p);
			}
			return p;
		}

		internal static int GetOption(string name, int def)
		{
			int p = def;
			string[] args = Environment.GetCommandLineArgs();
			for (int i=1;i<args.Length;i++)
			{
				GetIntArg(args[i], name, ref p);
			}
			return p;
		}

		private static void ParseArgs()
		{
			string[] args = Environment.GetCommandLineArgs();
			for (int i=1;i<args.Length;i++)
			{
				// help
				GetBoolArg(args[i], "?", ref Help);
				GetBoolArg(args[i], "h", ref Help);
				GetBoolArg(args[i], "help", ref Help);
			}
		}

		private static bool GetStringArg(string Arg, ref string ArgValue)
		{
			if (('/'==Arg[0]) || ('-'==Arg[0]))
				return false;
			ArgValue = Arg;
			return true;
		}

		private static void GetStringArg(string Arg, string Name, ref string ArgValue)
		{
			if (Arg.Length<(Name.Length+3)) // -name:x is 3 more than name
				return;
			if (('/'!=Arg[0]) && ('-'!=Arg[0]))	// not a param
				return;
			if (Arg.Substring(1, Name.Length).ToLower()==Name.ToLower())
				ArgValue = Arg.Substring(Name.Length+2, Arg.Length-Name.Length-2);
		}

		private static void GetBoolArg(string Arg, string Name, ref bool ArgValue)
		{
			if (Arg.Length<(Name.Length+1)) // -name is 1 more than name
				return;
			if (('/'!=Arg[0]) && ('-'!=Arg[0]))	// not a param
				return;
			if (Arg.Substring(1, Name.Length).ToLower()==Name.ToLower())
				ArgValue = true;
		}

		private static void GetIntArg(string Arg, string Name, ref int ArgValue)
		{
			if (Arg.Length<(Name.Length+3)) // -name:12 is 3 more than name
				return;
			if (('/'!=Arg[0]) && ('-'!=Arg[0]))	// not a param
				return;
			if (Arg.Substring(1, Name.Length).ToLower()==Name.ToLower())
			{
				try
				{
					ArgValue = Convert.ToInt32(Arg.Substring(Name.Length+2, Arg.Length-Name.Length-2));
				}
				catch
				{
				}
				
			}
		}
	}

	internal class HtmlConsoleListener: System.Diagnostics.TraceListener
	{
		public override void WriteLine(string Message)
		{
			Write(Message + "\n");
		}

		public override void Write(string Message)
		{
			Write(Message, "");
		}
	
		public override void Write(string Message, string Category)
		{
			Console.Write("T:" + Category + ": " + Message);
		}

		public override void WriteLine(string Message, string Category)
		{
			Write(Message + "\n", Category);
		}
	}

}
