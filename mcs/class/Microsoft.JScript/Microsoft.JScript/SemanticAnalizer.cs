//
// SemanticAnalyser.cs: Initiate the type check and identification phases.
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, 2004 Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
// (C) 2005, Novell Inc.
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
		static IdentificationTable context;
		static IdentificationTable label_set;

		private static Hashtable obj_ctrs;
		private static Hashtable prototypes;

		//
		// Type to GlobalObject
		//
		private static Type global_obj = typeof (GlobalObject);

		static SemanticAnalyser ()
		{
			label_set = new IdentificationTable ();
			
			obj_ctrs = new Hashtable ();
			obj_ctrs.Add ("Date", typeof (DateConstructor));
			obj_ctrs.Add ("Math", typeof (MathObject));
			obj_ctrs.Add ("Number", typeof (NumberConstructor));
			obj_ctrs.Add ("String", typeof (StringConstructor));

			prototypes = new Hashtable ();
			prototypes.Add (typeof (object), typeof (ObjectPrototype));
			prototypes.Add (typeof (FunctionObject), typeof (FunctionPrototype));
			prototypes.Add (typeof (ArrayObject), typeof (ArrayPrototype));
			prototypes.Add (typeof (StringObject), typeof (StringPrototype));
			prototypes.Add (typeof (BooleanObject), typeof (BooleanPrototype));
			prototypes.Add (typeof (NumberObject), typeof (NumberPrototype));
			prototypes.Add (typeof (DateObject), typeof (DatePrototype));
			prototypes.Add (typeof (RegExpObject), typeof (RegExpPrototype));
			// FIXME: Error objects missing
		}

		internal static string ImplementationName (string name)
		{
			int i = name.LastIndexOf ('_');
			return name.Substring (i + 1);
		}

		public static bool Run (ScriptBlock prog)
		{
			context = new IdentificationTable ();
			context.BuildGlobalEnv ();
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

		static int anon_regExp_counter = -1;
		internal static string NextAnonymousRegExpObj {
			get {
				anon_regExp_counter++;
				return "regexp " + anon_regExp_counter;
			}
		}
		
		internal static string CurrentAnonymousMethod {
			get { return "anonymous " + anon_method_counter; }
		}

		internal static void AddLabel (string name, AST binding)
		{
			label_set.Enter (Symbol.CreateSymbol (name), binding);
		}
		
		internal static bool ContainsLabel (string name)
		{
			object r = label_set.Get (Symbol.CreateSymbol (name));
			return r != null;
		}

		internal static object GetLabel (string name) 
		{
			return label_set.Get (Symbol.CreateSymbol (name));
		}

		internal static void RemoveLabel (string name)
		{
			label_set.Remove (Symbol.CreateSymbol (name));
		}


		internal static void assert_type (object thisObj, Type expType)
		{
			if (thisObj == null || thisObj.GetType () != expType)
				throw new Exception ("Type error");
		}

		internal static bool contains (Type target_type, string name, BindingFlags flags)
		{
			MemberInfo [] type_props = target_type.GetMembers (flags);
			foreach (MemberInfo mi in type_props)
				if (mi.Name == name)
					return true;
			return false;
		}

		internal static bool is_js_object (string name)
		{			
			return contains (global_obj, name, BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);
		}
		
		//
		// We assume type is a valid native object
		// type. Search for method name.
		//
		internal static bool object_contains_method (Type type, string name)
		{
			return contains (type, name, BindingFlags.Public | BindingFlags.Static);
		}

		internal static Type map_to_ctr (string type_name)
		{
			return (Type) obj_ctrs [type_name];
		}

		internal static MemberInfo get_member (AST left, AST right)
		{
			if (left != null && right != null && left is Identifier && right is Identifier) {
				string obj =  ((Identifier) left).name.Value;
				string prop_name = ((Identifier) right).name.Value;
				Type target_type = SemanticAnalyser.map_to_ctr (obj);

				if (target_type != null) {
					MemberInfo [] members = target_type.GetMember (prop_name);
					if (members != null && members.Length > 0)
						return members [0];
				}
			}
			return null;
		}

		internal static Type map_to_prototype (JSObject jsObj)
		{
			if (jsObj == null)
				throw new Exception ("jsObj can't be null");
			return (Type) prototypes [jsObj.GetType ()];
		}
	}
}
