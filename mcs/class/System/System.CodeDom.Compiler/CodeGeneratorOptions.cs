//
// System.CodeDom.Compiler CodeGeneratorOptions class
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System;
using System.Collections;
using System.Collections.Specialized;

namespace System.CodeDom.Compiler
{
	public class CodeGeneratorOptions
	{
		private IDictionary properties;
		
		//
		// Constructors
		//
		public CodeGeneratorOptions()
		{
			properties = new ListDictionary();
			properties.Add( "BlankLinesBetweenMembers", true );
			properties.Add( "BracingStyle", "Block" );
			properties.Add( "ElseOnClosing", false );
			properties.Add( "IndentString", "    " );
		}

		//
		// Properties
		//

		/// <summary>
		/// Whether to insert blank lines between individual members.
		/// Default is true.
		/// </summary>
		public bool BlankLinesBetweenMembers {
			get {
				return (bool)properties["BlankLinesBetweenMembers"];
			}
			set {
				properties["BlankLinesBetweenMembers"] = value;
			}
		}

		/// <summary>
		/// "Block" puts braces on the same line as the associated statement or declaration.
		/// "C" puts braces on the following line.
		/// Default is "C"
		/// </summary>
		public string BracingStyle {
			get {
				return (string)properties["BracingStyle"];
			}
			set {
				properties["BracingStyle"] = value;
			}
		}

		/// <summary>
		/// Whether to start <code>else</code>,
		/// <code>catch</code>, or <code>finally</code>
		/// blocks on the same line as the previous block.
		/// Default is false.
		/// </summary>
		public bool ElseOnClosing {
			get {
				return (bool)properties["ElseOnClosing"];
			}
			set {
				properties["ElseOnClosing"] = value;
			}
		}

		/// <summary>
		/// The string used for individual indentation levels. Default is four spaces.
		/// </summary>
		public string IndentString {
			get {
				return (string)properties["IndentString"];
			}
			set {
				properties["IndentString"] = value;
			}
		}

		public Object this[string index] {
			get {
				return properties[index];
			}
			set {
				properties[index] = value;
			}
		}
	}
}
