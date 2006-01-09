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

	internal class SemanticAnalyser {

		internal static bool print = true;
		internal static bool allow_member_expr_as_function_name;
		static Environment env;
		static IdentificationTable label_set;

		private static Hashtable obj_ctrs;
		private static Hashtable prototypes;
		internal static Hashtable methods_with_eval = new Hashtable ();
		internal static Hashtable methods_with_outter_scope_refs = new Hashtable ();
		internal static Hashtable methods_with_vars_used_nested = new Hashtable ();
		internal static Environment Ocurrences = new Environment ();

		//
		// Type to GlobalObject
		//
		private static Type global_obj = typeof (GlobalObject);

		internal static bool NoFast = true;
		private static Assembly [] assemblies;

		private static readonly string global_namespace = String.Empty;
		internal static string GlobalNamespace {
			get { return global_namespace; }
		}

		static SemanticAnalyser ()
		{
			label_set = new IdentificationTable ();
			
			obj_ctrs = new Hashtable ();
			obj_ctrs.Add ("Array", typeof (ArrayConstructor));
			obj_ctrs.Add ("Boolean", typeof (BooleanConstructor));
			obj_ctrs.Add ("Date", typeof (DateConstructor));
			obj_ctrs.Add ("Function", typeof (FunctionConstructor));
			obj_ctrs.Add ("Math", typeof (MathObject));
			obj_ctrs.Add ("Number", typeof (NumberConstructor));
			obj_ctrs.Add ("Object", typeof (ObjectConstructor));
			obj_ctrs.Add ("String", typeof (StringConstructor));
			obj_ctrs.Add ("RegExp", typeof (RegExpConstructor));

			prototypes = new Hashtable ();
			// Constructors
			prototypes.Add (typeof (FunctionConstructor), typeof (FunctionPrototype));
			prototypes.Add (typeof (ArrayConstructor), typeof (FunctionPrototype));
			prototypes.Add (typeof (StringConstructor), typeof (FunctionPrototype));
			prototypes.Add (typeof (BooleanConstructor), typeof (FunctionPrototype));
			prototypes.Add (typeof (NumberConstructor), typeof (FunctionPrototype));
			prototypes.Add (typeof (DateConstructor), typeof (FunctionPrototype));
			prototypes.Add (typeof (RegExpConstructor), typeof (FunctionPrototype));
			prototypes.Add (typeof (ObjectConstructor), typeof (FunctionPrototype));
			prototypes.Add (typeof (ErrorConstructor), typeof (FunctionPrototype));
			// Prototypes
			prototypes.Add (typeof (FunctionPrototype), typeof (ObjectPrototype));
			prototypes.Add (typeof (ArrayPrototype), typeof (ObjectPrototype));
			prototypes.Add (typeof (StringPrototype), typeof (ObjectPrototype));
			prototypes.Add (typeof (BooleanPrototype), typeof (ObjectPrototype));
			prototypes.Add (typeof (NumberPrototype), typeof (ObjectPrototype));
			prototypes.Add (typeof (DatePrototype), typeof (ObjectPrototype));
			prototypes.Add (typeof (RegExpPrototype), typeof (ObjectPrototype));
			prototypes.Add (typeof (ErrorPrototype), typeof (ObjectPrototype));
			// Regular objects
			prototypes.Add (typeof (object), typeof (ObjectPrototype));
			prototypes.Add (typeof (FunctionObject), typeof (FunctionPrototype));
			prototypes.Add (typeof (ScriptFunction), typeof (FunctionPrototype));
			prototypes.Add (typeof (Closure), typeof (FunctionPrototype));
			prototypes.Add (typeof (ArrayObject), typeof (ArrayPrototype));
			prototypes.Add (typeof (StringObject), typeof (StringPrototype));
			prototypes.Add (typeof (BooleanObject), typeof (BooleanPrototype));
			prototypes.Add (typeof (NumberObject), typeof (NumberPrototype));
			prototypes.Add (typeof (DateObject), typeof (DatePrototype));
			prototypes.Add (typeof (RegExpObject), typeof (RegExpPrototype));
			prototypes.Add (typeof (RegExpMatch), typeof (ArrayPrototype));
			prototypes.Add (typeof (ObjectPrototype), typeof (ObjectPrototype));
			prototypes.Add (typeof (ErrorObject), typeof (ErrorPrototype));
			prototypes.Add (typeof (EvalErrorObject), typeof (ErrorPrototype));
			prototypes.Add (typeof (RangeErrorObject), typeof (ErrorPrototype));
			prototypes.Add (typeof (SyntaxErrorObject), typeof (ErrorPrototype));
			prototypes.Add (typeof (TypeErrorObject), typeof (ErrorPrototype));
			prototypes.Add (typeof (URIErrorObject), typeof (ErrorPrototype));


			// literals, used when accessing a method from the prototype
			// through the literal
			prototypes.Add (typeof (ArrayLiteral), typeof (ArrayPrototype));
			prototypes.Add (typeof (StringLiteral), typeof (StringPrototype));
			prototypes.Add (typeof (BooleanConstant), typeof (BooleanPrototype));

			Type number_prototype = typeof (NumberPrototype);
			prototypes.Add (typeof (ByteConstant), number_prototype);
			prototypes.Add (typeof (ShortConstant), number_prototype);
			prototypes.Add (typeof (IntConstant), number_prototype);
			prototypes.Add (typeof (LongConstant), number_prototype);
			prototypes.Add (typeof (FloatConstant), number_prototype);
			prototypes.Add (typeof (DoubleConstant), number_prototype);
		}

		internal static string ImplementationName (string name)
		{
			int i = name.LastIndexOf ('_');
			return name.Substring (i + 1);
		}

		internal static bool Run (ScriptBlock [] blocks, Assembly [] ref_items)
		{
			assemblies = ref_items;

			if (assemblies != null && assemblies.Length > 0)
				ComputeNamespaces ();

			env = new Environment (blocks);
			bool r = true;

			foreach (ScriptBlock script_block in blocks)
				((ICanModifyContext) script_block).PopulateContext (env, String.Empty);

			foreach (ScriptBlock script_block in blocks)
				r &= script_block.Resolve (env);

			return r;
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
			if (thisObj == null || (thisObj.GetType () != expType && !thisObj.GetType ().IsSubclassOf (expType)))
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
		internal static bool object_contains (Type type, string name)
		{
			return contains (type, name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
		}

		internal static Type map_to_ctr (string type_name)
		{
			return (Type) obj_ctrs [type_name];
		}

		internal static MemberInfo get_member (AST left, AST right)
		{
			bool right_is_identifier = false;
			
			if (left != null && right != null) {
				right_is_identifier = right is Identifier;

				Type target_type = null;
				string prop_name = string.Empty;
				string obj = string.Empty;
					
				if (left is Identifier && right_is_identifier) {
					obj =  ((Identifier) left).name.Value;
					prop_name = ((Identifier) right).name.Value;
					target_type = SemanticAnalyser.map_to_ctr (obj);
				} else if (left is ICanLookupPrototype && right_is_identifier) {
					prop_name = ((Identifier) right).name.Value;
					target_type = SemanticAnalyser.map_to_prototype (left);
				}
				if (target_type != null && prop_name != string.Empty)
					return Find (target_type, prop_name);
			}
			return null;
		}

		internal static MemberInfo Find (Type type, string propertyName)
		{
			MemberInfo [] members = type.GetMember (propertyName);
			if (members != null && members.Length > 0)
				return members [0];
			return null;
		}

		internal static Type map_to_prototype (object jsObj)
		{
			if (jsObj == null)
				throw new Exception ("jsObj can't be null");
			return (Type) prototypes [jsObj.GetType ()];
		}

		internal static void AddMethodWithEval (string name)
		{
			object contained = methods_with_eval [name];
			if (contained == null)
				methods_with_eval.Add (name, true);
		}

		internal static bool MethodContainsEval (string name)
		{
			object val = methods_with_eval [name];
			return val != null;
		}

		internal static void AddMethodReferenceOutterScopeVar (string name, VariableDeclaration decl)
		{
			object contained = methods_with_outter_scope_refs [name];
			if (contained == null)
				methods_with_outter_scope_refs.Add (name, decl);
		}

		internal static void AddMethodVarsUsedNested (string name, VariableDeclaration decl)
		{
			object contained = methods_with_vars_used_nested [name];
			if (contained == null)
				methods_with_vars_used_nested.Add (name, decl);
		}

		internal static bool MethodReferenceOutterScopeVar (string name)
		{
			return OutterScopeVar (name) != null;
		}

		internal static VariableDeclaration OutterScopeVar (string name)
		{
			return (VariableDeclaration) methods_with_outter_scope_refs [name];
		}

		internal static bool MethodVarsUsedNested (string name)
		{
			bool r = VarUsedNested (name) != null;
			return r;
		}

		internal static VariableDeclaration VarUsedNested (string name)
		{
			return (VariableDeclaration) methods_with_vars_used_nested [name];
		}

		internal static Type IsLiteral (AST ast)
		{
			if (ast != null) {
				Type type = ast.GetType ();
				// FIXME: Add test for other literals (exclude StringLiteral)
				if (type == typeof (ArrayLiteral))
					return type;
			}
			return null;
		}

		internal static bool IsNumericConstant (object o)
		{
			return o is ByteConstant || o is ShortConstant ||
				o is IntConstant || o is LongConstant ||
				o is FloatConstant || o is DoubleConstant;
		}

		internal static bool NeedsToBoolean (AST ast)
		{
			if (ast is BooleanConstant || ast is StrictEquality)
				return false;
			else if (ast is Expression)
				return NeedsToBoolean (((Expression) ast).Last);
			else if (ast is Unary) {
				Unary unary = (Unary) ast;
				if (unary.oper != JSToken.LogicalNot)
					return true;
				return false;
			} else if (ast is Call || ast is Identifier)
				return true;
			else {
				Console.WriteLine ("ast.LineNumber = {0}", ast.Location.LineNumber);
				throw new NotImplementedException ();
			}
		}

		internal static bool Needs (JSFunctionAttributeEnum targetAttr, MethodInfo method)
		{
			JSFunctionAttribute [] custom_attrs = (JSFunctionAttribute [])
				method.GetCustomAttributes (typeof (JSFunctionAttribute), true);

			foreach (JSFunctionAttribute attr in custom_attrs)
				if ((attr.GetAttributeValue () & targetAttr) != 0)
					return true;
			return false;
		}

		internal static bool IsDeletable (Identifier left, Identifier right, out bool isCtr)
		{
			Type ctr = SemanticAnalyser.map_to_ctr (left.name.Value);
			isCtr = ctr != null ? true : false;

			if (isCtr)
				return SemanticAnalyser.get_member (left, right) == null;

			Console.WriteLine ("ctr = {0}, left = {1} ({2}); right = {3} ({4})",
				   ctr, left, left.GetType (), right, right.GetType ());
			return false;
		}

		/// <summary>
		///   Computes the namespaces that we import from the assemblies we reference.
		/// </summary>
		public static void ComputeNamespaces ()
		{
			MethodInfo assembly_get_namespaces = typeof (Assembly).GetMethod ("GetNamespaces", BindingFlags.Instance|BindingFlags.NonPublic);

			Hashtable cache = null;

			//
			// First add the assembly namespaces
			//
			if (assembly_get_namespaces != null){
				int count = assemblies.Length;

				for (int i = 0; i < count; i++){
					Assembly a = assemblies [i];
					string [] namespaces = (string []) assembly_get_namespaces.Invoke (a, null);
					foreach (string ns in namespaces){
						if (ns == "")
							continue;
						Namespace.GetNamespace (ns, true);
					}
				}
			} else {
				cache = new Hashtable ();
				cache.Add ("", null);
				foreach (Assembly a in assemblies) {
					foreach (Type t in a.GetExportedTypes ()) {
						string ns = t.Namespace;
						if (ns == null || cache.Contains (ns))
							continue;
						Namespace.GetNamespace (ns, true);
						cache.Add (ns, null);
					}
				}
			}
		}
	}
}
