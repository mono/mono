//
// OptionList.cs
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Rafael Teixeira
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace Mono.GetOptions
{

	[Flags]
	public enum OptionsParsingMode 
	{ 
		Linux   = 1, 
		Windows = 2,
		Both    = 3
	}

	/// <summary>
	/// Option Parsing
	/// </summary>
	public class OptionList
	{
	
		private Options optionBundle = null;
		private OptionsParsingMode parsingMode;
		private bool endOptionProcessingWithDoubleDash;
		
		private string appExeName;
		private string appVersion;

		private string appTitle = "Add a [assembly: AssemblyTitle(\"Here goes the application name\")] to your assembly";
		private string appCopyright = "Add a [assembly: AssemblyCopyright(\"(c)200n Here goes the copyright holder name\")] to your assembly";
		private string appDescription = "Add a [assembly: AssemblyDescription(\"Here goes the short description\")] to your assembly";
		private string appAboutDetails = "Add a [assembly: Mono.About(\"Here goes the short about details\")] to your assembly";
		private string appUsageComplement = "Add a [assembly: Mono.UsageComplement(\"Here goes the usage clause complement\")] to your assembly";
		private string[] appAuthors;
 
		private ArrayList list = new ArrayList();
		private ArrayList arguments = new ArrayList();
		private ArrayList argumentsTail = new ArrayList();
		private MethodInfo argumentProcessor = null;

		public string Usage
		{
			get
			{
				return "Usage: " + appExeName + " [options] " + appUsageComplement;
			}
		}

		public string AboutDetails
		{
			get
			{
				return appAboutDetails;
			}
		}

		#region Assembly Attributes

		Assembly entry;
		
		private object[] GetAssemblyAttributes(Type type)
		{
			return entry.GetCustomAttributes(type, false);
		}
			
		private string[] GetAssemblyAttributeStrings(Type type)
		{
			object[] result = GetAssemblyAttributes(type);
			
			if ((result == null) || (result.Length == 0))
				return new string[0];

			int i = 0;
			string[] var = new string[result.Length];

			foreach(object o in result)
				var[i++] = o.ToString(); 

			return var;
		}

		private void GetAssemblyAttributeValue(Type type, string propertyName, ref string var)
		{
			object[] result = GetAssemblyAttributes(type);
			
			if ((result != null) && (result.Length > 0))
				var = (string)type.InvokeMember(propertyName, BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance, null, result[0], new object [] {}); ;
		}

		private void GetAssemblyAttributeValue(Type type, ref string var)
		{
			object[] result = GetAssemblyAttributes(type);
			
			if ((result != null) && (result.Length > 0))
				var = result[0].ToString();
		}

		#endregion

		#region Constructors

		private void AddArgumentProcessor(MemberInfo memberInfo)
		{
			if (argumentProcessor != null)
				throw new NotSupportedException("More than one argument processor method found");

			if ((memberInfo.MemberType == MemberTypes.Method && memberInfo is MethodInfo))
			{
				if (((MethodInfo)memberInfo).ReturnType.FullName != typeof(void).FullName)
					throw new NotSupportedException("Argument processor method must return 'void'");

				ParameterInfo[] parameters = ((MethodInfo)memberInfo).GetParameters();
				if ((parameters == null) || (parameters.Length != 1) || (parameters[0].ParameterType.FullName != typeof(string).FullName))
					throw new NotSupportedException("Argument processor method must have a string parameter");
				
				argumentProcessor = (MethodInfo)memberInfo; 
			}
			else
				throw new NotSupportedException("Argument processor marked member isn't a method");
		}

		private void Initialize(Options optionBundle)
		{
			if (optionBundle == null)
				throw new ArgumentNullException("optionBundle");

			entry = Assembly.GetEntryAssembly();
			appExeName = entry.GetName().Name;
			appVersion = entry.GetName().Version.ToString();

			this.optionBundle = optionBundle; 
			this.parsingMode = optionBundle.ParsingMode ;
			this.endOptionProcessingWithDoubleDash = optionBundle.EndOptionProcessingWithDoubleDash;

			GetAssemblyAttributeValue(typeof(AssemblyTitleAttribute), "Title", ref appTitle);
			GetAssemblyAttributeValue(typeof(AssemblyCopyrightAttribute), "Copyright", ref appCopyright);
			GetAssemblyAttributeValue(typeof(AssemblyDescriptionAttribute), "Description", ref appDescription);
			GetAssemblyAttributeValue(typeof(Mono.AboutAttribute), ref appAboutDetails);
			GetAssemblyAttributeValue(typeof(Mono.UsageComplementAttribute), ref appUsageComplement);
			appAuthors = GetAssemblyAttributeStrings(typeof(AuthorAttribute));
			if (appAuthors.Length == 0)
			{
				appAuthors = new String[1];
				appAuthors[0] = "Add one or more [assembly: Mono.GetOptions.Author(\"Here goes the author name\")] to your assembly";
			}

			foreach(MemberInfo mi in optionBundle.GetType().GetMembers())
			{
				object[] attribs = mi.GetCustomAttributes(typeof(OptionAttribute), true);
				if (attribs != null && attribs.Length > 0)
					list.Add(new OptionDetails(mi, (OptionAttribute)attribs[0], optionBundle));
				else
				{
					attribs = mi.GetCustomAttributes(typeof(ArgumentProcessorAttribute), true);
					if (attribs != null && attribs.Length > 0)
						AddArgumentProcessor(mi);
				}
			}
		}

		public OptionList(Options optionBundle)
		{
			Initialize(optionBundle);
		}

		#endregion

		#region Prebuilt Options

		private void ShowTitleLines()
		{
			Console.WriteLine(appTitle + "  " + appVersion + " - " + appCopyright); 
			Console.WriteLine(appDescription); 
			Console.WriteLine();
		}

		private void ShowAbout()
		{
			ShowTitleLines();
			Console.WriteLine(appAboutDetails); 
			Console.WriteLine();
			Console.WriteLine("Authors:");
			foreach(string s in appAuthors)
				Console.WriteLine ("\t" + s);
		}

		private void ShowHelp()
		{
			ShowTitleLines();
			Console.WriteLine(Usage);
			Console.WriteLine("Options:");
			foreach (OptionDetails option in list)
				Console.WriteLine(option);
		}

		private void ShowUsage()
		{
			Console.WriteLine(Usage);
			Console.Write("Short Options: ");
			foreach (OptionDetails option in list)
				Console.Write((option.ShortForm != ' ') ? option.ShortForm.ToString() : "");
			Console.WriteLine();
			
		}

		private void ShowUsage(string errorMessage)
		{
			Console.WriteLine("ERROR: " + errorMessage.TrimEnd());
			ShowUsage();
		}

		internal WhatToDoNext DoUsage()
		{
			ShowUsage();
			return WhatToDoNext.AbandonProgram;
		}

		internal WhatToDoNext DoAbout()
		{
			ShowAbout();
			return WhatToDoNext.GoAhead;
		}

		internal WhatToDoNext DoHelp()
		{
			ShowHelp();
			return WhatToDoNext.AbandonProgram;
		}

		#endregion

		#region Arguments Processing

		public string[] ExpandResponseFiles(string[] args)
		{
			ArrayList result = new ArrayList();

			foreach(string arg in args)
			{
				if (arg.StartsWith("@"))
				{
					try 
					{
						StreamReader tr = new StreamReader(arg.Substring(1));
						string line;
						while ((line = tr.ReadLine()) != null)
						{
							result.AddRange(line.Split());
						}
						tr.Close(); 
					}
					catch (FileNotFoundException exception)
					{
						Console.WriteLine("Could not find response file: " + arg.Substring(1));
						continue;
					}
					catch (Exception exception)
					{
						Console.WriteLine("Error trying to read response file: " + arg.Substring(1));
						Console.WriteLine(exception.Message);
						continue;
					}
				}
				else
					result.Add(arg);
			}

			return (string[])result.ToArray(typeof(string));
		}

		public string[] NormalizeArgs(string[] args)
		{
			bool ParsingOptions = true;
			ArrayList result = new ArrayList();

			foreach(string arg in ExpandResponseFiles(args))
			{
				if (arg.Length > 0)
				{
					if (ParsingOptions)
					{
						if (endOptionProcessingWithDoubleDash && (arg == "--"))
						{
							ParsingOptions = false;
							continue;
						}

						if ((parsingMode & OptionsParsingMode.Windows) > 0)
						{
						 	if (arg[0] == '/')
								result.AddRange(arg.Split(':')); 
						}

						if ((parsingMode & OptionsParsingMode.Linux) > 0)
						{
							if ((arg[0] == '-') && (arg[1] != '-'))
							{
								foreach(char c in arg.Substring(1)) // many single-letter options
									result.Add("-" + c); // expand into individualized options
								continue;
							}

							if (arg.StartsWith("--"))
							{
								result.AddRange(arg.Split('='));  // put in the same form of one-letter options with a parameter
								continue;
							}
						}
					}
					else
					{
						argumentsTail.Add(arg);
						continue;
					}

					// if nothing else matches then it get here
					result.Add(arg);
				}
			}

			return (string[])result.ToArray(typeof(string));
		}

		public string[] ProcessArgs(string[] args)
		{
			string arg;
			string nextArg;
			bool OptionWasProcessed;

			list.Sort();

			args = NormalizeArgs(args);

			try
			{
				int argc = args.Length;
				for(int i = 0; i < argc; i++)
				{
					arg =  args[i];
					if (i+1 < argc)
					{
						nextArg = args[i+1];
						if (nextArg.StartsWith("-"))
							nextArg = null;
					}
					else
						nextArg = null;

					OptionWasProcessed = false;

					if (arg.StartsWith("-") || arg.StartsWith("/"))
					{
						foreach(OptionDetails option in list)
						{
							if (option.ProcessArgument(arg, nextArg))
							{
								OptionWasProcessed = true;
								if (nextArg != null)
									i++;
								break;
							}
						}
					}

					if (!OptionWasProcessed)
						arguments.Add(arg); 
				}

				foreach(OptionDetails option in list)
					option.TransferValues(); 

				foreach(string argument in argumentsTail)
					arguments.Add(argument);

				if (argumentProcessor == null)
					return (string[])arguments.ToArray(typeof(string));
			
				foreach(string argument in arguments)
					argumentProcessor.Invoke(optionBundle, new object[] { argument });  
			}
			catch (Exception ex)
			{
				ShowUsage(ex.Message);
				System.Environment.Exit(1);
			}

			return null;
		}
		
		#endregion

	}
}
