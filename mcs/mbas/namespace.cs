//
// namespace.cs: Tracks namespaces
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
using System;
using System.Collections;
using Mono.Languages;

namespace Mono.CSharp {

	/// <summary>
	///   Keeps track of the namespaces defined in the C# code.
	/// </summary>
	public class Namespace {
		Namespace parent;
		string name;
		ArrayList using_clauses;
		Hashtable aliases;
		bool decl_found = false;
		
		/// <summary>
		///   Constructor Takes the current namespace and the
		///   name.  This is bootstrapped with parent == null
		///   and name = ""
		/// </summary>
		public Namespace (Namespace parent, string name)
		{
			this.name = name;
			this.parent = parent;
		}

		/// <summary>
		///   The qualified name of the current namespace
		/// </summary>
		public string Name {
			get {
				string pname = parent != null ? parent.Name : "";
				
				if (pname == "")
					return name;
				else
					return parent.Name + "." + name;
			}
		}

		/// <summary>
		///   The parent of this namespace, used by the parser to "Pop"
		///   the current namespace declaration
		/// </summary>
		public Namespace Parent {
			get {
				return parent;
			}
		}

		/// <summary>
		///   When a declaration is found in a namespace,
		///   we call this function, to emit an error if the
		///   program attempts to use a using clause afterwards
		/// </summary>
		public void DeclarationFound ()
		{
			decl_found = true;
		}

		/// <summary>
		///   Records a new namespace for resolving name references
		/// </summary>
		public void Using (string ns)
		{
			if (decl_found){
				Report.Error (1529, "A using clause must precede all other namespace elements");
				return;
			}

			if (using_clauses == null)
				using_clauses = new ArrayList ();

			using_clauses.Add (ns);
		}

		public ArrayList UsingTable {
			get {
				return using_clauses;
			}
		}

		public void UsingAlias (string alias, string namespace_or_type, Location loc)
		{
			if (aliases == null)
				aliases = new Hashtable ();
			
			if (aliases.Contains (alias)){
				Report.Error (1537, loc, "The using alias `" + alias +
					      "' appeared previously in this namespace");
				return;
			}
					
			aliases [alias] = namespace_or_type;
		}

		public string LookupAlias (string alias)
		{
			string value = null;

			// System.Console.WriteLine ("Lookup " + alias + " in " + name);

			if (aliases != null)
				value = (string) (aliases [alias]);
			if (value == null && Parent != null)
				value = Parent.LookupAlias (alias);

			return value;
		}

		/// <summary>
		///   Used to validate that all the using clauses are correct
		///   after we are finished parsing all the files
		/// </summary>
		public void VerifyUsing ()
		{
			foreach (DictionaryEntry de in using_clauses){
				if (de.Value == null){
					string name = (string) de.Key;
					
					Report.Error (234, "The type or namespace `" +
							    name + "' does not exist in the " +
							    "class or namespace `" + name + "'");
				}
			}
		}

	}
}

