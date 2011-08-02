//
// System.CodeDom.Compiler CodeGeneratorOptions class
//
// Authors:
//	Daniel Stodden (stodden@in.tum.de)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc.
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.CodeDom.Compiler {

	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public class CodeGeneratorOptions {

		private IDictionary properties;
		
		//
		// Constructors
		//
		public CodeGeneratorOptions()
		{
			properties = new ListDictionary();
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
				object o = properties["BlankLinesBetweenMembers"];
				return ((o == null) ? true : (bool) o);
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
				object o = properties["BracingStyle"];
				return ((o == null) ? "Block" : (string) o);
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
				object o = properties["ElseOnClosing"];
				return ((o == null) ? false : (bool) o);
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
				object o = properties["IndentString"];
				return ((o == null) ? "    " : (string) o);
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

		[ComVisible (false)]
		public bool VerbatimOrder {
			get {
				object o = properties["VerbatimOrder"];
				return ((o == null) ? false : (bool) o);
			}
			set {
				properties["VerbatimOrder"] = value;
			}
		}
	}
}
