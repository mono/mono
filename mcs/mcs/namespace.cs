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
		static ArrayList all_namespaces = new ArrayList ();
		
		Namespace parent;
		string name;
		SourceFile file;
		int symfile_id;
		ArrayList using_clauses;
		Hashtable aliases;
		public bool DeclarationFound = false;

		//
		// This class holds the location where a using definition is
		// done, and whether it has been used by the program or not.
		//
		// We use this to flag using clauses for namespaces that do not
		// exist.
		//
		public class UsingEntry {
			public string Name;
			public Location Location;
			
			public UsingEntry (string name, Location loc)
			{
				Name = name;
				Location = loc;
			}
		}
		
		/// <summary>
		///   Constructor Takes the current namespace and the
		///   name.  This is bootstrapped with parent == null
		///   and name = ""
		/// </summary>
		public Namespace (Namespace parent, SourceFile file, string name)
		{
			this.name = name;
			this.file = file;
			this.parent = parent;

			all_namespaces.Add (this);
		}

		static public ArrayList UserDefinedNamespaces {
			get {
				return all_namespaces;
			}
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
					return String.Concat (parent.Name, ".", name);
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

		public int SymbolFileID {
			get {
				return symfile_id;
			}
		}

		/// <summary>
		///   Records a new namespace for resolving name references
		/// </summary>
		public void Using (string ns, Location loc)
		{
			if (DeclarationFound){
				Report.Error (1529, loc, "A using clause must precede all other namespace elements");
				return;
			}

			if (ns == Name)
				return;
			
			if (using_clauses == null)
				using_clauses = new ArrayList ();

			foreach (UsingEntry old_entry in using_clauses){
				if (old_entry.Name == ns){
					Report.Warning (105, loc, "The using directive for '" + ns +
							"' appeared previously in this namespace");
					return;
				}
			}
			
			UsingEntry ue = new UsingEntry (ns, loc);
			using_clauses.Add (ue);
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

		void DefineNamespace (SymbolWriter symwriter)
		{
			if (symfile_id != 0)
				return;
			if (parent != null)
				parent.DefineNamespace (symwriter);

			string[] using_list;
			if (using_clauses != null) {
				using_list = new string [using_clauses.Count];
				for (int i = 0; i < using_clauses.Count; i++)
					using_list [i] = ((UsingEntry) using_clauses [i]).Name;
			} else {
				using_list = new string [0];
			}				

			int parent_id = parent != null ? parent.symfile_id : 0;
			symfile_id = symwriter.DefineNamespace (name, file, using_list, parent_id);
		}

		public static void DefineNamespaces (SymbolWriter symwriter)
		{
			foreach (Namespace ns in all_namespaces)
				ns.DefineNamespace (symwriter);
		}

		static void Msgtry (string s)
		{
			Console.WriteLine ("    Try using -r:" + s);
		}
		
		/// <summary>
		///   Used to validate that all the using clauses are correct
		///   after we are finished parsing all the files.  
		/// </summary>
		public static void VerifyUsing ()
		{
			foreach (Namespace ns in all_namespaces){
				ArrayList uses = ns.UsingTable;

				if (uses != null){
					foreach (UsingEntry ue in uses){
						if (TypeManager.IsNamespace (ue.Name))
							continue;
						
						Report.Error (246, ue.Location, "The namespace `" + ue.Name +
							      "' can not be found (missing assembly reference?)");

						switch (ue.Name){
						case "Gtk": case "GtkSharp":
							Msgtry ("gtk-sharp");
							break;
							
						case "Gdk": case "GdkSharp":
							Msgtry ("gdk-sharp");
							break;

						case "Glade": case "GladeSharp":
							Msgtry ("glade-sharp");
							break;
							
						case "System.Drawing":
							Msgtry ("System.Drawing");
							break;
							
						case "System.Web.Services":
							Msgtry ("System.Web.Services");
							break;

						case "System.Web":
							Msgtry ("System.Web");
							break;
							
						case "System.Data":
							Msgtry ("System.Data");
							break;

						case "System.Windows.Forms":
							Msgtry ("System.Windows.Forms");
							break;
						}
					}
				}

				if (ns.aliases != null){
					foreach (DictionaryEntry de in ns.aliases){
						string value = (string) de.Value;
						
						if (TypeManager.IsNamespace (value))
							continue;
						if (TypeManager.LookupTypeDirect (value) != null)
							continue;
						
						Report.Error (246, String.Format (
								      "The type or namespace `{0}' could not be found (missing assembly reference?)",
								      value));
					}
				}
			}
		}
	}
}
