//
// OptionList.cs
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Rafael Teixeira
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;

namespace Mono.GetOptions
{

	/// <summary>
	/// Option Parsing
	/// </summary>
	public class OptionList
	{
	
		private Options optionBundle = null;
		private OptionsParsingMode parsingMode;
		private bool breakSingleDashManyLettersIntoManyOptions;
		private bool endOptionProcessingWithDoubleDash;
		public ErrorReporter ReportError;
		
		private string appExeName;
		private string appVersion;

		private string appTitle = "Add a [assembly: AssemblyTitle(\"Here goes the application name\")] to your assembly";
		private string appCopyright = "Add a [assembly: AssemblyCopyright(\"(c)200n Here goes the copyright holder name\")] to your assembly";
		private string appDescription = "Add a [assembly: AssemblyDescription(\"Here goes the short description\")] to your assembly";
		private string appAboutDetails = "Add a [assembly: Mono.About(\"Here goes the short about details\")] to your assembly";
		private string appUsageComplement = "Add a [assembly: Mono.UsageComplement(\"Here goes the usage clause complement\")] to your assembly";
		private string appAdditionalInfo = null;
		private string appReportBugsTo = null;
		private string[] appAuthors;
 
		private ArrayList list = new ArrayList();
		private ArrayList arguments = new ArrayList();
		private ArrayList argumentsTail = new ArrayList();
		private MethodInfo argumentProcessor = null;
		
		private bool HasSecondLevelHelp = false;

		internal bool MaybeAnOption(string arg)
		{
			return 	((parsingMode & OptionsParsingMode.Windows) > 0 && arg[0] == '/') || 
					((parsingMode & OptionsParsingMode.Linux)   > 0 && arg[0] == '-');
		}

		public string Usage
		{
			get {
				return "Usage: " + appExeName + " [options] " + appUsageComplement;
			}
		}

		public string AboutDetails
		{
			get {
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

		private void ExtractEntryAssemblyInfo(Type optionsType)
		{
			entry = optionsType.Assembly;
			if (entry == this.GetType().Assembly)	{		
				entry = Assembly.GetEntryAssembly();
			}

			appExeName = entry.GetName().Name;
			appVersion = entry.GetName().Version.ToString();
			GetAssemblyAttributeValue(typeof(AssemblyTitleAttribute), "Title", ref appTitle);
			GetAssemblyAttributeValue(typeof(AssemblyCopyrightAttribute), "Copyright", ref appCopyright);
			GetAssemblyAttributeValue(typeof(AssemblyDescriptionAttribute), "Description", ref appDescription);
			GetAssemblyAttributeValue(typeof(Mono.AboutAttribute), ref appAboutDetails);
			GetAssemblyAttributeValue(typeof(Mono.UsageComplementAttribute), ref appUsageComplement);
			GetAssemblyAttributeValue(typeof(Mono.AdditionalInfoAttribute), ref appAdditionalInfo);
			GetAssemblyAttributeValue(typeof(Mono.ReportBugsToAttribute), ref appReportBugsTo);
			appAuthors = GetAssemblyAttributeStrings(typeof(AuthorAttribute));
			if (appAuthors.Length == 0) {
				appAuthors = new String[1];
				appAuthors[0] = "Add one or more [assembly: Mono.Author(\"Here goes the author name\")] to your assembly";
			}		
		}

		#endregion

		#region Constructors

		private void AddArgumentProcessor(MemberInfo memberInfo)
		{
			if (argumentProcessor != null)
				throw new NotSupportedException("More than one argument processor method found");

			if ((memberInfo.MemberType == MemberTypes.Method && memberInfo is MethodInfo)) {
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

		public OptionList(Options optionBundle)
		{
			if (optionBundle == null)
				throw new ArgumentNullException("optionBundle");

			Type optionsType = optionBundle.GetType();
			this.optionBundle = optionBundle; 
			this.parsingMode = optionBundle.ParsingMode ;
			this.breakSingleDashManyLettersIntoManyOptions = optionBundle.BreakSingleDashManyLettersIntoManyOptions;
			this.endOptionProcessingWithDoubleDash = optionBundle.EndOptionProcessingWithDoubleDash;
			this.ReportError = optionBundle.ReportError;
			
			ExtractEntryAssemblyInfo(optionsType);

			foreach(MemberInfo mi in optionsType.GetMembers()) {
				object[] attribs = mi.GetCustomAttributes(typeof(KillOptionAttribute), true);
				if (attribs == null || attribs.Length == 0) {
					attribs = mi.GetCustomAttributes(typeof(OptionAttribute), true);
					if (attribs != null && attribs.Length > 0) {
						OptionDetails option = new OptionDetails(mi, (OptionAttribute)attribs[0], optionBundle);
						list.Add(option);
						HasSecondLevelHelp = HasSecondLevelHelp || option.SecondLevelHelp;
					} else if (mi.DeclaringType == mi.ReflectedType) { // not inherited
						attribs = mi.GetCustomAttributes(typeof(ArgumentProcessorAttribute), true); 
						if (attribs != null && attribs.Length > 0)
							AddArgumentProcessor(mi);
					}
				}
			}
			
			if (argumentProcessor == null) // try to find an inherited one
				foreach(MemberInfo mi in optionsType.GetMembers()) 
					if (mi.DeclaringType != mi.ReflectedType) { // inherited
						object[] attribs = mi.GetCustomAttributes(typeof(ArgumentProcessorAttribute), true);
						if (attribs != null && attribs.Length > 0)
							AddArgumentProcessor(mi);
					}
		}

		#endregion

		#region Prebuilt Options

		private bool bannerAlreadyShown = false;
		
		internal string AdditionalBannerInfo;
		
		public void ShowBanner()
		{
			if (!bannerAlreadyShown) {
				Console.WriteLine(appTitle + "  " + appVersion + " - " + appCopyright); 
				if (AdditionalBannerInfo != null)
					Console.WriteLine(AdditionalBannerInfo);
			}
			bannerAlreadyShown = true;
		}
		
		private void ShowTitleLines()
		{
			ShowBanner();
			Console.WriteLine(appDescription); 
			Console.WriteLine();
		}

		private void ShowAbout()
		{
			ShowTitleLines();
			Console.WriteLine(appAboutDetails); 
			Console.Write("Authors: ");
			Console.WriteLine(string.Join(", ", appAuthors));
		}

		private void ShowHelp(bool showSecondLevelHelp)
		{
			ShowTitleLines();
			Console.WriteLine(Usage);
			Console.WriteLine("Options:");
			ArrayList lines = new ArrayList(list.Count);
			int tabSize = 0;
			foreach (OptionDetails option in list)
				if (option.SecondLevelHelp == showSecondLevelHelp) {
					string[] optionLines = option.ToString().Split('\n');
					foreach(string line in optionLines) {
						int pos = line.IndexOf('\t');
						if (pos > tabSize)
							tabSize = pos;
						lines.Add(line);
					}
				}
			tabSize += 2;
			foreach (string line in lines) {
				string[] parts = line.Split('\t');
				Console.Write(parts[0].PadRight(tabSize));
				Console.WriteLine(parts[1]);
				if (parts.Length > 2) {
					string spacer = new string(' ', tabSize);
					for(int i = 2; i < parts.Length; i++) {
						Console.Write(spacer);
						Console.WriteLine(parts[i]);
					}
				}
			}
			if (appAdditionalInfo != null)
				Console.WriteLine("\n{0}", appAdditionalInfo);
			if (appReportBugsTo != null)
				Console.WriteLine("\nPlease report bugs {0} <{1}>", (appReportBugsTo.IndexOf('@')>0)?"to":"at" , appReportBugsTo);
				
		}

		private void ShowUsage()
		{
			Console.WriteLine(Usage);
			Console.Write("Short Options: ");
			foreach (OptionDetails option in list)
				Console.Write(option.ShortForm.Trim());
			Console.WriteLine();
			
		}

		internal WhatToDoNext DoUsage()
		{
			ShowUsage();
			return WhatToDoNext.AbandonProgram;
		}

		internal WhatToDoNext DoAbout()
		{
			ShowAbout();
			return WhatToDoNext.AbandonProgram;
		}

		internal WhatToDoNext DoHelp()
		{
			ShowHelp(false);
			return WhatToDoNext.AbandonProgram;
		}

		internal WhatToDoNext DoHelp2()
		{
			ShowHelp(true);
			return WhatToDoNext.AbandonProgram;
		}
		
		#endregion

		#region Response File Expansion
		
		private void processResponseFileLine(string line, ArrayList result, StringBuilder sb)
		{
			int t = line.Length;
			for (int i = 0; i < t; i++) {
				char c = line [i];
				if (c == '"' || c == '\'') {
					char end = c;
					for (i++; i < t; i++) {
						c = line [i];	
						if (c == end)
							break;
						sb.Append(c);
					}
				} else if (c == ' ') {
					if (sb.Length > 0) {
						result.Add(sb.ToString());
						sb.Length = 0;
					}
				} else {
					sb.Append(c);
				}
			}
			if (sb.Length > 0) {
				result.Add(sb.ToString());
				sb.Length = 0;
			}
		}
		
		private void processResponseFile(string filename, ArrayList result)
		{
			StringBuilder sb = new StringBuilder();
			string line;
			try {
				using (StreamReader responseFile = new StreamReader(filename)) {
					while ((line = responseFile.ReadLine()) != null)
						processResponseFileLine(line, result, sb);
					responseFile.Close ();	
				} 
			} catch (FileNotFoundException) {
				ReportError(2011, "Unable to find response file '" + filename + "'");
			} catch (Exception exception) {
				ReportError(2011, "Unable to open response file '" + filename + "'. " + exception.Message);
			}
		}

		private ArrayList ExpandResponseFiles(string[] args)
		{
			ArrayList result = new ArrayList();
			foreach(string arg in args)
				if (arg.StartsWith("@")) 
					processResponseFile(arg.Substring(1), result);
				else
					result.Add(arg);
			return result;
		}
		
		#endregion

		#region Arguments Processing


		private static int IndexOfAny(string where, params char[] what)
		{
			return where.IndexOfAny(what);
		}
		
		private string[] NormalizeArgs(string[] args)
		{
			bool ParsingOptions = true;
			ArrayList result = new ArrayList();
			
			foreach(string arg in ExpandResponseFiles(args)) {
				if (arg.Length > 0) {
					if (ParsingOptions) {
						if (endOptionProcessingWithDoubleDash && (arg == "--")) {
							ParsingOptions = false;
							continue;
						}

						if ((parsingMode & OptionsParsingMode.Linux) > 0 && 
							 arg[0] == '-' && arg.Length > 1 && arg[1] != '-' &&
							 breakSingleDashManyLettersIntoManyOptions) {
							foreach(char c in arg.Substring(1)) // many single-letter options
								result.Add("-" + c); // expand into individualized options
							continue;
						}

						if (MaybeAnOption(arg)) {
							int pos = IndexOfAny(arg, ':', '=');

							if(pos < 0)
								result.Add(arg);
							else {
								result.Add(arg.Substring(0, pos));
								result.Add(arg.Substring(pos+1));
							}
							continue;
						}
					} else {
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
			
			OptionDetails.LinkAlternatesInsideList(list);

			args = NormalizeArgs(args);

			try {
				int argc = args.Length;
				for (int i = 0; i < argc; i++) {
					arg =  args[i];
					if (i+1 < argc)
						nextArg = args[i+1];
					else
						nextArg = null;

					OptionWasProcessed = false;

					if (arg.Length > 1 && (arg.StartsWith("-") || arg.StartsWith("/"))) {
						foreach(OptionDetails option in list) {
							OptionProcessingResult result = option.ProcessArgument(arg, nextArg);
							if (result != OptionProcessingResult.NotThisOption) {
								OptionWasProcessed = true;
								if (result == OptionProcessingResult.OptionConsumedParameter)
									i++;
								break;
							}
						}
					}

					if (!OptionWasProcessed)
						ProcessNonOption(arg);
				}

				foreach(OptionDetails option in list)
					option.TransferValues(); 

				foreach(string argument in argumentsTail)
					ProcessNonOption(argument);

				return (string[])arguments.ToArray(typeof(string));
				
			} catch (Exception ex) {
				System.Console.WriteLine(ex.ToString());
				System.Environment.Exit(1);
			}

			return null;
		}
		
		private void ProcessNonOption(string argument)
		{
			if (optionBundle.VerboseParsingOfOptions)
					Console.WriteLine("argument [" + argument + "]");							
			if (argumentProcessor == null)
				arguments.Add(argument);
			else
				argumentProcessor.Invoke(optionBundle, new object[] { argument });  						
		}
		
		#endregion

	}
}
