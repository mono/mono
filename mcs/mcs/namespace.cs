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

namespace Mono.CSharp {

	/// <summary>
	///   Keeps track of the namespaces defined in the C# code.
	/// </summary>
	public class Namespace {
		static ArrayList all_namespaces = new ArrayList ();
		static Hashtable namespaces_map = new Hashtable ();
		
		Namespace parent;
		string fullname;
		ArrayList entries;
		Hashtable namespaces;
		Hashtable defined_names;

		/// <summary>
		///   Constructor Takes the current namespace and the
		///   name.  This is bootstrapped with parent == null
		///   and name = ""
		/// </summary>
		public Namespace (Namespace parent, string name)
		{
			this.parent = parent;

			string pname = parent != null ? parent.Name : "";
				
			if (pname == "")
				fullname = name;
			else
				fullname = parent.Name + "." + name;

			entries = new ArrayList ();
			namespaces = new Hashtable ();
			defined_names = new Hashtable ();

			all_namespaces.Add (this);
			if (namespaces_map.Contains (fullname))
				return;
			namespaces_map [fullname] = true;
		}

		public static bool IsNamespace (string name)
		{
			return namespaces_map [name] != null;
		}
		
		public static Namespace Root = new Namespace (null, "");

		public Namespace GetNamespace (string name, bool create)
		{
			int pos = name.IndexOf ('.');

			Namespace ns;
			string first;
			if (pos >= 0)
				first = name.Substring (0, pos);
			else
				first = name;

			ns = (Namespace) namespaces [first];
			if (ns == null) {
				if (!create)
					return null;

				ns = new Namespace (this, first);
				namespaces.Add (first, ns);
			}

			if (pos >= 0)
				ns = ns.GetNamespace (name.Substring (pos + 1), create);

			return ns;
		}

		public static Namespace LookupNamespace (string name, bool create)
		{
			return Root.GetNamespace (name, create);
		}

		public object Lookup (DeclSpace ds, string name)
		{
			object o = Lookup (name);

			Type t;
			DeclSpace tdecl = o as DeclSpace;
			if (tdecl != null) {
				t = tdecl.DefineType ();

				if ((ds == null) || ds.CheckAccessLevel (t))
					return t;
			}

			Namespace ns = GetNamespace (name, false);
			if (ns != null)
				return ns;

			t = TypeManager.LookupType (DeclSpace.MakeFQN (fullname, name));
			if ((t == null) || ((ds != null) && !ds.CheckAccessLevel (t)))
				return null;

			return t;
		}

		public void AddNamespaceEntry (NamespaceEntry entry)
		{
			entries.Add (entry);
		}

		public void DefineName (string name, object o)
		{
			defined_names.Add (name, o);
		}

		public object Lookup (string name)
		{
			return defined_names [name];
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
				return fullname;
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

		public static void DefineNamespaces (SymbolWriter symwriter)
		{
			foreach (Namespace ns in all_namespaces) {
				foreach (NamespaceEntry entry in ns.entries)
					entry.DefineNamespace (symwriter);
			}
		}

		/// <summary>
		///   Used to validate that all the using clauses are correct
		///   after we are finished parsing all the files.  
		/// </summary>
		public static void VerifyUsing ()
		{
			foreach (Namespace ns in all_namespaces) {
				foreach (NamespaceEntry entry in ns.entries)
					entry.VerifyUsing ();
			}
		}

		public override string ToString ()
		{
			if (this == Root)
				return "Namespace (<root>)";
			else
				return String.Format ("Namespace ({0})", Name);
		}
	}

	public class NamespaceEntry
	{
		Namespace ns;
		NamespaceEntry parent, implicit_parent;
		SourceFile file;
		int symfile_id;
		Hashtable aliases;
		ArrayList using_clauses;
		public bool DeclarationFound = false;

		//
		// This class holds the location where a using definition is
		// done, and whether it has been used by the program or not.
		//
		// We use this to flag using clauses for namespaces that do not
		// exist.
		//
		public class UsingEntry {
			public readonly string Name;
			public readonly NamespaceEntry NamespaceEntry;
			public readonly Location Location;
			
			public UsingEntry (NamespaceEntry entry, string name, Location loc)
			{
				Name = name;
				NamespaceEntry = entry;
				Location = loc;
			}

			Namespace resolved_ns;

			public Namespace Resolve ()
			{
				if (resolved_ns != null)
					return resolved_ns;

				object resolved = NamespaceEntry.LookupForUsing (Name, Location);
				resolved_ns = resolved as Namespace;
				return resolved_ns;
			}
		}

		public class AliasEntry {
			public readonly string Name;
			public readonly MemberName Alias;
			public readonly NamespaceEntry NamespaceEntry;
			public readonly Location Location;
			
			public AliasEntry (NamespaceEntry entry, string name, MemberName alias, Location loc)
			{
				Name = name;
				Alias = alias;
				NamespaceEntry = entry;
				Location = loc;
			}

			object resolved;

			public object Resolve ()
			{
				if (resolved != null)
					return resolved;

				string alias = Alias.GetPartialName ();

				resolved = NamespaceEntry.LookupForUsing (alias, Location);
				return resolved;
			}
		}

		public NamespaceEntry (NamespaceEntry parent, SourceFile file, string name, Location loc)
			: this (parent, file, name, false, loc)
		{ }

		protected NamespaceEntry (NamespaceEntry parent, SourceFile file, string name, bool is_implicit, Location loc)
		{
			this.parent = parent;
			this.file = file;
			this.IsImplicit = is_implicit;
			this.ID = ++next_id;

			if (!is_implicit && (parent != null))
				ns = parent.NS.GetNamespace (name, true);
			else if (name != null)
				ns = Namespace.LookupNamespace (name, true);
			else
				ns = Namespace.Root;
			ns.AddNamespaceEntry (this);

			if ((parent != null) && (parent.NS != ns.Parent))
				implicit_parent = new NamespaceEntry (parent, file, ns.Parent.Name, true, loc);
			else
				implicit_parent = parent;

			this.FullName = ns.Name;
		}

		static int next_id = 0;
		public readonly string FullName;
		public readonly int ID;
		public readonly bool IsImplicit;

		public Namespace NS {
			get {
				return ns;
			}
		}

		public NamespaceEntry Parent {
			get {
				return parent;
			}
		}

		public NamespaceEntry ImplicitParent {
			get {
				return implicit_parent;
			}
		}

		public void DefineName (string name, object o)
		{
			ns.DefineName (name, o);
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

			if (ns == FullName)
				return;
			
			if (using_clauses == null)
				using_clauses = new ArrayList ();

			foreach (UsingEntry old_entry in using_clauses) {
				if (old_entry.Name == ns) {
					if (RootContext.WarningLevel >= 3)
						Report.Warning (105, loc, "The using directive for '{0}' appeared previously in this namespace", ns);
					return;
				}
			}
			
			UsingEntry ue = new UsingEntry (this, ns, loc);
			using_clauses.Add (ue);
		}

		public void UsingAlias (string name, MemberName alias, Location loc)
		{
			if (DeclarationFound){
				Report.Error (1529, loc, "A using clause must precede all other namespace elements");
				return;
			}

			if (aliases == null)
				aliases = new Hashtable ();
			
			if (aliases.Contains (name)){
				Report.Error (1537, loc, "The using alias `" + name +
					      "' appeared previously in this namespace");
				return;
			}

			aliases [name] = new AliasEntry (this, name, alias, loc);
		}

		protected AliasEntry GetAliasEntry (string alias)
		{
			AliasEntry entry = null;

			if (aliases != null)
				entry = (AliasEntry) aliases [alias];
			if (entry == null && Parent != null)
				entry = Parent.GetAliasEntry (alias);

			return entry;
		}

		public string LookupAlias (string alias)
		{
			AliasEntry entry = GetAliasEntry (alias);

			if (entry == null)
				return null;

			object resolved = entry.Resolve ();
			if (resolved == null)
				return null;
			else if (resolved is Namespace)
				return ((Namespace) resolved).Name;
			else
				return ((Type) resolved).FullName;
		}

		//
		// According to section 16.3.1 (using-alias-directive), the namespace-or-type-name is 
		// resolved as if the immediately containing namespace body has no using-directives.
		//
		// Section 16.3.2 says that the same rule is applied when resolving the namespace-name
		// in the using-namespace-directive.
		//
		public object LookupForUsing (string dotted_name, Location loc)
		{
			int pos = dotted_name.IndexOf ('.');
			string simple_name = dotted_name;
			string rest = null;
			if (pos >= 0) {
				simple_name = dotted_name.Substring (0, pos);
				rest = dotted_name.Substring (pos + 1);
			}

			object o = NS.Lookup (null, simple_name);
			if (o == null && ImplicitParent != null)
				o = ImplicitParent.LookupNamespaceOrType (null, simple_name, loc);

			if (o == null || rest == null)
				return o;

			Namespace ns = o as Namespace;
			if (ns != null)
				return ns.Lookup (null, rest);
			
			Type nested = TypeManager.LookupType ((((Type) o).Name + "." + rest));
			return nested;
		}

		public object LookupNamespaceOrType (DeclSpace ds, string name, Location loc)
		{
			object resolved = null;
			for (NamespaceEntry curr_ns = this; curr_ns != null; curr_ns = curr_ns.ImplicitParent) {
				if ((resolved = curr_ns.Lookup (ds, name, loc)) != null)
					break;
			}
			return resolved;
		}

		private object Lookup (DeclSpace ds, string name, Location loc)
		{
			object o;
			Namespace ns;

			//
			// If name is of the form `N.I', first lookup `N', then search a member `I' in it.
			//
			// FIXME: Remove this block.  Only simple names should come here.
			//        The bug: The loop in LookupNamespaceOrType continues if 
			//        the lookup for N succeeds but the nested lookup for I fails.
			//        This is one part of #52697.
			//
			int pos = name.IndexOf ('.');
			if (pos >= 0) {
				string first = name.Substring (0, pos);
				string last = name.Substring (pos + 1);

				o = Lookup (ds, first, loc);
				if (o == null)
					return null;

				ns = o as Namespace;
				if (ns != null)
					return ns.Lookup (ds, last);

				Type nested = TypeManager.LookupType ((((Type) o).Name + "." + last));
				if ((nested == null) || ((ds != null) && !ds.CheckAccessLevel (nested)))
					return null;

				return nested;
			}

			//
			// Check whether it's in the namespace.
			//
			o = NS.Lookup (ds, name);
			if (o != null)
				return o;

			//
			// Check aliases.
			//
			AliasEntry entry = GetAliasEntry (name);
			if (entry != null) {
				o = entry.Resolve ();
				if (o != null)
					return o;
			}

			if (name.IndexOf ('.') > 0)
				return null;

			//
			// Check using entries.
			//
			Type t = null, match = null;
			foreach (Namespace using_ns in GetUsingTable ()) {
				match = using_ns.Lookup (ds, name) as Type;
				if (match != null){
					if (t != null) {
						DeclSpace.Error_AmbiguousTypeReference (loc, name, t, match);
						return null;
					} else {
						t = match;
					}
				}
			}

			return t;
		}

		// Our cached computation.
		Namespace [] namespace_using_table;
		public Namespace[] GetUsingTable ()
		{
			if (namespace_using_table != null)
				return namespace_using_table;
			
			if (using_clauses == null)
				return new Namespace [0];

			ArrayList list = new ArrayList (using_clauses.Count);

			foreach (UsingEntry ue in using_clauses) {
				Namespace using_ns = ue.Resolve ();
				if (using_ns == null)
					continue;

				list.Add (using_ns);
			}

			namespace_using_table = new Namespace [list.Count];
			list.CopyTo (namespace_using_table, 0);
			return namespace_using_table;
		}

		public void DefineNamespace (SymbolWriter symwriter)
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
			if (file.SourceFileEntry == null)
				return;

			symfile_id = symwriter.DefineNamespace (
				ns.Name, file.SourceFileEntry, using_list, parent_id);
		}

		public int SymbolFileID {
			get {
				return symfile_id;
			}
		}

		static void MsgtryRef (string s)
		{
			Console.WriteLine ("    Try using -r:" + s);
		}
		
		static void MsgtryPkg (string s)
		{
			Console.WriteLine ("    Try using -pkg:" + s);
		}

		protected void error246 (Location loc, string name)
		{
			Report.Error (246, loc, "The namespace `" + name +
				      "' can not be found (missing assembly reference?)");

			switch (name) {
			case "Gtk": case "GtkSharp":
				MsgtryPkg ("gtk-sharp");
				break;

			case "Gdk": case "GdkSharp":
				MsgtryPkg ("gdk-sharp");
				break;

			case "Glade": case "GladeSharp":
				MsgtryPkg ("glade-sharp");
				break;

			case "System.Drawing":
			case "System.Web.Services":
			case "System.Web":
			case "System.Data":
			case "System.Windows.Forms":
				MsgtryRef (name);
				break;
			}
		}

		/// <summary>
		///   Used to validate that all the using clauses are correct
		///   after we are finished parsing all the files.  
		/// </summary>
		public void VerifyUsing ()
		{
			if (using_clauses != null){
				foreach (UsingEntry ue in using_clauses){
					if (ue.Resolve () != null)
						continue;

					if (LookupForUsing (ue.Name, ue.Location) == null)
						error246 (ue.Location, ue.Name);
					else
						Report.Error (138, ue.Location, "The using keyword only lets you specify a namespace, " +
							      "`" + ue.Name + "' is a class not a namespace.");

				}
			}

			if (aliases != null){
				foreach (DictionaryEntry de in aliases){
					AliasEntry alias = (AliasEntry) de.Value;

					if (alias.Resolve () != null)
						continue;

					error246 (alias.Location, alias.Alias.GetPartialName ());
				}
			}
		}

		public override string ToString ()
		{
			if (NS == Namespace.Root)
				return "NamespaceEntry (<root>)";
			else
				return String.Format ("NamespaceEntry ({0},{1},{2})", FullName, IsImplicit, ID);
		}
	}
}
