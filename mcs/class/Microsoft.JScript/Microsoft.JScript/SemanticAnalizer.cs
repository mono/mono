//
// SemanticAnalyser.cs: Initiate the type check and identification phases.
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, 2004 Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
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
using System.Reflection;
using System.Collections;

namespace Microsoft.JScript {

	public class SemanticAnalyser {

		internal static bool print = true;
		public static bool allow_member_expr_as_function_name;
		static Hashtable global_env = new Hashtable ();
		static IdentificationTable context;
		static IdentificationTable label_set;

		static SemanticAnalyser ()
		{
			label_set = new IdentificationTable ();
			BuildGlobalEnv ();
		}

		static void BuildGlobalEnv ()
		{
			/* value properties of the Global Object */
			global_env.Add ("NaN", new BuiltIn ("NaN", false, false));
			global_env.Add ("Infinity", new BuiltIn ("Infinity", false, false));
			global_env.Add ("undefined", new BuiltIn ("undefined", false, false));
			
			/* function properties of the Global Object */
			object [] custom_attrs;
			Type global_object = typeof (GlobalObject);
			MethodInfo [] methods = global_object.GetMethods (BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
			foreach (MethodInfo mi in methods) {
				custom_attrs = mi.GetCustomAttributes (typeof (JSFunctionAttribute), false);
				foreach (JSFunctionAttribute attr in custom_attrs)
					if (attr.IsBuiltIn)
						global_env.Add (mi.Name, new BuiltIn (ImplementationName (attr.BuiltIn.ToString ()), false, true));
			}

			/* built in objects */
			global_env.Add ("Object", new BuiltIn ("Object", true, true));
			global_env.Add ("Function", new BuiltIn ("Function", true, true));
			global_env.Add ("Array", new BuiltIn ("Array", true, true));
			global_env.Add ("String", new BuiltIn ("String", true, true));
			global_env.Add ("Boolean", new BuiltIn ("Boolean", true, true));
			global_env.Add ("Number", new BuiltIn ("Number", true, true));
			global_env.Add ("Math", new BuiltIn ("Math", false, false));
			global_env.Add ("Date", new BuiltIn ("Date", true, true));
			global_env.Add ("RegExp", new BuiltIn ("RegExp", true, true));

			/* built in Error objects */
			global_env.Add ("Error", new BuiltIn ("Error", true, true));
			global_env.Add ("EvalError", new BuiltIn ("EvalError", true, true));
			global_env.Add ("RangeError", new BuiltIn ("RangeError", true, true));
			global_env.Add ("ReferenceError", new BuiltIn ("ReferenceError", true, true));
			global_env.Add ("SyntaxError", new BuiltIn ("SyntaxError", true, true));
			global_env.Add ("TypeError", new BuiltIn ("TypeError", true, true));
			global_env.Add ("URIError", new BuiltIn ("URIError", true, true));
		}

		static string ImplementationName (string name)
		{
			int i = name.LastIndexOf ('_');
			return name.Substring (i + 1);
		}

		internal static object ObjectSystemContains (string x)
		{
			return global_env [x];
		}

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

		internal static void AddLabel (string name, AST binding)
		{
			label_set.Enter (name, binding);
		}
		
		internal static bool ContainsLabel (string name)
		{
			object r = label_set.Contains (name);
			return r != null;
		}

		internal static object GetLabel (string name) 
		{
			return label_set.Contains (name);
		}

		internal static void RemoveLabel (string name)
		{
			label_set.Remove (name);
		}
	}
}
