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
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
	public class AuthorAttribute : System.Attribute
	{
		public string Name;
		public string SubProject;

		public AuthorAttribute(string Name)
		{
			this.Name = Name;
			this.SubProject = null;
		}

		public AuthorAttribute(string Name, string SubProject)
		{
			this.Name = Name;
			this.SubProject = SubProject;
		}

		public override string ToString()
		{
			if (SubProject == null)
				return Name;
			else
				return Name + " (" + SubProject + ")"; 
		}
	}

	enum OptionParameterType
	{
		None,
		Integer,
		Decimal, // look XML Schemas for better names
		String,
		Symbol,
		FilePath,
		FileMask,
		AssemblyName,
		AssemblyFileName,
		AssemblyNameOrFileName
	}
		
	public delegate bool OptionFound(object Value);

	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class OptionList
	{
		struct OptionDetails
		{
			public char ShortForm;
			public string LongForm;
			public string ShortDescription;
			public OptionParameterType ParameterType;
			public int MinOccurs;
			public int MaxOccurs; // negative means there is no limit
			public object DefaultValue;
			public OptionFound Dispatcher;

		}
	
		private string appTitle = "Add a [assembly: AssemblyTitle(\"Here goes the application name\")] to your assembly";
		private string appCopyright = "Add a [assembly: AssemblyCopyright(\"(c)200n Here goes the copyright holder name\")] to your assembly";
		private string appDescription = "Add a [assembly: AssemblyDescription(\"Here goes the short description\")] to your assembly";
		private string[] appAuthors;
 
		public readonly string usageFormat;
		public readonly string aboutDetails;

		private SortedList list = new SortedList();

		private object[] GetAssemblyAttributes(Type type)
		{
			Assembly entry = Assembly.GetEntryAssembly();
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
				var = (string)(string)type.InvokeMember(propertyName, BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance, null, result[0], new object [] {}); ;
		}

		public OptionList(string aboutDetails, string usageFormat)
		{
			this.aboutDetails = aboutDetails;
			this.usageFormat = usageFormat;

			GetAssemblyAttributeValue(typeof(AssemblyTitleAttribute), "Title", ref appTitle);
			GetAssemblyAttributeValue(typeof(AssemblyCopyrightAttribute), "Copyright", ref appCopyright);
			GetAssemblyAttributeValue(typeof(AssemblyDescriptionAttribute), "Description", ref appDescription);
			appAuthors = GetAssemblyAttributeStrings(typeof(AuthorAttribute));
			if (appAuthors.Length == 0)
			{
				appAuthors = new String[1];
				appAuthors[0] = "Add one or more [assembly: Mono.GetOptions.Author(\"Here goes the author name\")] to your assembly";
			}
		}

		private void AddGenericOption(
			char shortForm, 
			string longForm, 
			string shortDescription,
			OptionParameterType parameterType, 
			int minOccurs, 
			int maxOccurs, 
			object defaultValue, 
			OptionFound dispatcher)
		{
			OptionDetails option = new OptionDetails();

			option.ShortForm = shortForm;
			option.LongForm = longForm;
			option.ShortDescription = shortDescription;
			option.ParameterType = parameterType;
			option.MinOccurs = minOccurs;
			option.MaxOccurs = maxOccurs;
			option.DefaultValue = defaultValue;
			option.Dispatcher = dispatcher;

			if (shortForm == ' ')
				list.Add(longForm, option);
			else
				list.Add(shortForm.ToString(), option);
		}

		public void AddAbout(char shortForm, string longForm, string shortDescription)
		{
			AddGenericOption(shortForm, longForm, shortDescription, OptionParameterType.None, 0, 1, null, new OptionFound(DoAbout));
		}

		public void AddBooleanSwitch(char shortForm, string longForm, string shortDescription, bool defaultValue, OptionFound switcher)
		{
			AddGenericOption(shortForm, longForm, shortDescription, OptionParameterType.None, 0, 1, defaultValue, switcher);
		}

		public void ShowAbout()
		{
			Console.WriteLine(appTitle + " - " + appCopyright); 
			Console.WriteLine(appDescription); 
			Console.WriteLine();
			Console.WriteLine(aboutDetails); 
			Console.WriteLine();
			Console.WriteLine("Authors:");
			foreach(string s in appAuthors)
				Console.WriteLine ("\t" + s);
		}

		private bool DoAbout(object nothing)
		{
			ShowAbout();
			return true;
		}

		public void ShowUsage()
		{
			Console.WriteLine(appTitle + " - " + appCopyright); 
			Console.Write("Usage: ");
			Console.WriteLine(usageFormat);
			// TODO: list registered options here
			foreach (DictionaryEntry option in list)
				Console.WriteLine(option.Value.ToString());
		}

		public void ShowUsage(string errorMessage)
		{
			Console.WriteLine(errorMessage);
			ShowUsage();
		}

		public void ProcessArgs(string[] args)
		{
			ShowAbout();
		}
	}
}
