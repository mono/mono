//
// SemanticAnalyser.cs: Initiate the type check and identification phases.
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
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

namespace Microsoft.JScript {

	public class SemanticAnalyser {

		internal static bool print = true;
		public static bool allow_member_expr_as_function_name;
		//
		// We must include the default 'Global Object',
		// which contains the built-in objects: Math, String, ...
		// static void init_default_global_object ()
		//

		static IdentificationTable context;

		public static bool Run (ScriptBlock prog)
		{
			context = new IdentificationTable ();

			return prog.Resolve (context);
		}

		public static void Dump ()
		{
			Console.WriteLine (context.ToString ());
		}

		static int anon_method_counter = -1;
		internal static string NextAnonymousMethod {
			get { 
				anon_method_counter++;
				return "anonymous " + anon_method_counter; 
			}
		}

		internal static string CurrentAnonymousMethod {
			get { return "anonymous " + anon_method_counter; }
		}
	}
}
