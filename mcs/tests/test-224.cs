// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version value="$version"/>
// </file>

using System;
using System.Reflection;

	/// <summary>
	/// Indicates that field should be treated as a xml attribute for the codon or condition.
	/// The field is treated as a array, separated by ',' example :
	/// fileextensions = ".cpp,.cc,.C"
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, Inherited=true)]
	public class XmlMemberArrayAttribute : Attribute
	{
		char[]   separator = new char[] { ',' };
		string name;
		bool   isRequired;
		
		/// <summary>
		/// Constructs a new instance.
		/// </summary>
		public XmlMemberArrayAttribute(string name)
		{
			this.name  = name;
			isRequired = false;
		}
		
		public char[] Separator {
			get {
				return separator;
			}
			set {
				separator = value;
			}
		}
		
		/// <summary>
		/// The name of the attribute.
		/// </summary>
		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}
		
		/// <summary>
		/// returns <code>true</code> if this attribute is required.
		/// </summary>
		public bool IsRequired {
			get {
				return isRequired;
			}
			set {
				isRequired = value;
			}
		}
	}

public class t
{

	[XmlMemberArrayAttribute("shortcut", Separator=new char[] { '|'})]
	string[] shortcut;

	public static void Main () { }

}
