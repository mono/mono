//
// System.CodeDom.Compiler CodeGeneratorOptions class
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
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
#if NET_2_0
			properties.Add( "VerbatimOrder", false );
#endif
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

#if NET_2_0
		public bool VerbatimOrder {
			get {
				return (bool)properties["VerbatimOrder"];
			}
			set {
				properties["VerbatimOrder"] = value;
			}
		}
#endif
	}
}
