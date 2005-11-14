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
using System.Collections.Specialized;
using System.Reflection;

namespace Mono.CSharp {

	public class RootNamespace : Namespace {
		static MethodInfo get_namespaces_method;

		string alias_name;
		Assembly referenced_assembly;

		Hashtable all_namespaces;

		static Hashtable root_namespaces;
		public static GlobalRootNamespace Global;
		
		static RootNamespace ()
		{
			get_namespaces_method = typeof (Assembly).GetMethod ("GetNamespaces", BindingFlags.Instance | BindingFlags.NonPublic);

			Reset ();
		}

		public static void Reset ()
		{
			root_namespaces = new Hashtable ();
			Global = new GlobalRootNamespace ();
			root_namespaces ["global"] = Global;
		}

		protected RootNamespace (string alias_name, Assembly assembly)
			: base (null, String.Empty)
		{
			this.alias_name = alias_name;
			referenced_assembly = assembly;

			all_namespaces = new Hashtable ();
			all_namespaces.Add ("", this);

			if (referenced_assembly != null)
				ComputeNamespaces (this.referenced_assembly);
		}

		public static void DefineRootNamespace (string name, Assembly assembly)
		{
			if (name == "global") {
				// FIXME: Add proper error number
				Report.Error (-42, "Cannot define an external alias named `global'");
				return;
			}
			RootNamespace retval = GetRootNamespace (name);
			if (retval == null || retval.referenced_assembly != assembly)
				root_namespaces [name] = new RootNamespace (name, assembly);
		}

		public static RootNamespace GetRootNamespace (string name)
		{
			return (RootNamespace) root_namespaces [name];
		}

		public virtual Type LookupTypeReflection (string name, Location loc)
		{
			return GetTypeInAssembly (referenced_assembly, name);
		}

		public void RegisterNamespace (Namespace child)
		{
			if (child != this)
				all_namespaces.Add (child.Name, child);
		}

		public bool IsNamespace (string name)
		{
			return all_namespaces.Contains (name);
		}

		protected void EnsureNamespace (string dotted_name)
		{
			if (dotted_name != null && dotted_name != "" && ! IsNamespace (dotted_name))
				GetNamespace (dotted_name, true);
		}

		protected void ComputeNamespaces (Assembly assembly)
		{
			if (get_namespaces_method != null) {
				string [] namespaces = (string []) get_namespaces_method.Invoke (assembly, null);
				foreach (string ns in namespaces)
					EnsureNamespace (ns);
				return;
			}

			foreach (Type t in assembly.GetExportedTypes ())
				EnsureNamespace (t.Namespace);
		}
		
		protected static Type GetTypeInAssembly (Assembly assembly, string name)
		{
			Type t = assembly.GetType (name);
			if (t == null)
				return null;

			if (t.IsPointer)
				throw new InternalErrorException ("Use GetPointerType() to get a pointer");
			
			
			TypeAttributes ta = t.Attributes & TypeAttributes.VisibilityMask;
			if (ta == TypeAttributes.NestedPrivate)
				return null;
			
			if (ta == TypeAttributes.NotPublic ||
					ta == TypeAttributes.NestedAssembly ||
					ta == TypeAttributes.NestedFamANDAssem)
				if (!TypeManager.IsFriendAssembly (t.Assembly))
					return null;

			return t;
		}

		public override string ToString ()
		{
			return String.Format ("RootNamespace ({0}::)", alias_name);
		}

		public override string GetSignatureForError ()
		{
			return alias_name + "::";
		}
	}

	public class GlobalRootNamespace : RootNamespace {
		Assembly [] assemblies;
		Module [] modules;

		public GlobalRootNamespace ()
			: base ("global", null)
		{
			assemblies = new Assembly [0];
		}

		public Assembly [] Assemblies {
			get { return assemblies; }
		}

		public Module [] Modules {
			get { return modules; }
		}

		public void AddAssemblyReference (Assembly a)
		{
			foreach (Assembly assembly in assemblies) {
				if (a == assembly)
					return;
			}

			int top = assemblies.Length;
			Assembly [] n = new Assembly [top + 1];
			assemblies.CopyTo (n, 0);
			n [top] = a;
			assemblies = n;

			ComputeNamespaces (a);
		}

		public void AddModuleReference (Module m)
		{
			int top = modules != null ? modules.Length : 0;
			Module [] n = new Module [top + 1];
			if (modules != null)
				modules.CopyTo (n, 0);
			n [top] = m;
			modules = n;

			if (m == CodeGen.Module.Builder)
				return;

			foreach (Type t in m.GetTypes ())
				EnsureNamespace (t.Namespace);
		}

		public override Type LookupTypeReflection (string name, Location loc)
		{
			Type found_type = null;
		
			foreach (Assembly a in assemblies) {
				Type t = GetTypeInAssembly (a, name);
				if (t == null)
					continue;
					
				if (found_type == null) {
					found_type = t;
					continue;
				}

				Report.SymbolRelatedToPreviousError (found_type);
				Report.SymbolRelatedToPreviousError (t);
				Report.Error (433, loc, "The imported type `{0}' is defined multiple times", name);
					
				return found_type;
			}

			if (modules != null) {
				foreach (Module module in modules) {
					Type t = module.GetType (name);
					if (t == null)
						continue;

					if (found_type == null) {
						found_type = t;
						continue;
					}
					
					Report.SymbolRelatedToPreviousError (t);
					Report.SymbolRelatedToPreviousError (found_type);
					Report.Warning (436, 2, loc, "Ignoring imported type `{0}' since the current assembly already has a declaration with the same name",
						TypeManager.CSharpName (t));
					return t;
				}
			}

			return found_type;
		}
	}

	/// <summary>
	///   Keeps track of the namespaces defined in the C# code.
	///
	///   This is an Expression to allow it to be referenced in the
	///   compiler parse/intermediate tree during name resolution.
	/// </summary>
	public class Namespace : FullNamedExpression {
		
		Namespace parent;
		string fullname;
		Hashtable namespaces;
		IDictionary declspaces;
		Hashtable cached_types;
		RootNamespace root;

		public readonly MemberName MemberName;

		/// <summary>
		///   Constructor Takes the current namespace and the
		///   name.  This is bootstrapped with parent == null
		///   and name = ""
		/// </summary>
		public Namespace (Namespace parent, string name)
		{
			// Expression members.
			this.eclass = ExprClass.Namespace;
			this.Type = null;
			this.loc = Location.Null;

			this.parent = parent;

			if (parent != null)
				this.root = parent.root;
			else
				this.root = this as RootNamespace;

			if (this.root == null)
				throw new InternalErrorException ("Root namespaces must be created using RootNamespace");
			
			string pname = parent != null ? parent.Name : "";
				
			if (pname == "")
				fullname = name;
			else
				fullname = parent.Name + "." + name;

			if (fullname == null)
				throw new InternalErrorException ("Namespace has a null fullname");

			if (parent != null && parent.MemberName != MemberName.Null)
				MemberName = new MemberName (
					parent.MemberName, name, parent.MemberName.Location);
			else if (name == "")
				MemberName = MemberName.Null;
			else
				MemberName = new MemberName (name, Location.Null);

			namespaces = new Hashtable ();
			cached_types = new Hashtable ();

			root.RegisterNamespace (this);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			throw new InternalErrorException ("Expression tree referenced namespace " + fullname + " during Emit ()");
		}

		public override string GetSignatureForError ()
		{
			return Name;
		}
		
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

		TypeExpr LookupType (string name, Location loc)
		{
			if (cached_types.Contains (name))
				return cached_types [name] as TypeExpr;

			Type t = null;
			if (declspaces != null) {
				DeclSpace tdecl = declspaces [name] as DeclSpace;
				if (tdecl != null) {
					//
					// Note that this is not:
					//
					//   t = tdecl.DefineType ()
					//
					// This is to make it somewhat more useful when a DefineType
					// fails due to problems in nested types (more useful in the sense
					// of fewer misleading error messages)
					//
					tdecl.DefineType ();
					t = tdecl.TypeBuilder;
				}
			}
			string lookup = t != null ? t.FullName : (fullname == "" ? name : fullname + "." + name);
			Type rt = root.LookupTypeReflection (lookup, loc);
			if (t == null)
				t = rt;

			TypeExpr te = t == null ? null : new TypeExpression (t, Location.Null);
			cached_types [name] = te;
			return te;
		}

		public FullNamedExpression Lookup (DeclSpace ds, string name, Location loc)
		{
			if (namespaces.Contains (name))
				return (Namespace) namespaces [name];

			TypeExpr te = LookupType (name, loc);
			if (te == null || !ds.CheckAccessLevel (te.Type))
				return null;

			return te;
		}

		public void AddDeclSpace (string name, DeclSpace ds)
		{
			if (declspaces == null)
				declspaces = new HybridDictionary ();
			declspaces.Add (name, ds);
		}

		/// <summary>
		///   The qualified name of the current namespace
		/// </summary>
		public string Name {
			get { return fullname; }
		}

		public override string FullName {
			get { return fullname; }
		}

		/// <summary>
		///   The parent of this namespace, used by the parser to "Pop"
		///   the current namespace declaration
		/// </summary>
		public Namespace Parent {
			get { return parent; }
		}

		public override string ToString ()
		{
			return String.Format ("Namespace ({0})", Name);
		}
	}

	public class NamespaceEntry {
		Namespace ns;
		NamespaceEntry parent, implicit_parent;
		SourceFile file;
		int symfile_id;
		Hashtable aliases;
		ArrayList using_clauses;
		public bool DeclarationFound = false;
		public bool UsingFound = false;

		static ArrayList entries = new ArrayList ();

		public static void Reset ()
		{
			entries = new ArrayList ();
		}

		//
		// This class holds the location where a using definition is
		// done, and whether it has been used by the program or not.
		//
		// We use this to flag using clauses for namespaces that do not
		// exist.
		//
		public class UsingEntry {
			public readonly MemberName Name;
			readonly Expression Expr;
			readonly NamespaceEntry NamespaceEntry;
			readonly Location Location;
			
			public UsingEntry (NamespaceEntry entry, MemberName name, Location loc)
			{
				Name = name;
				Expr = name.GetTypeExpression ();
				NamespaceEntry = entry;
				Location = loc;
			}

			internal Namespace resolved;

			public Namespace Resolve ()
			{
				if (resolved != null)
					return resolved;

				DeclSpace root = RootContext.Tree.Types;
				root.NamespaceEntry = NamespaceEntry;
				FullNamedExpression fne = Expr.ResolveAsTypeStep (root.EmitContext, false);
				root.NamespaceEntry = null;

				if (fne == null) {
					Error_NamespaceNotFound (Location, Name.ToString ());
					return null;
				}

				resolved = fne as Namespace;
				if (resolved == null) {
					Report.Error (138, Location,
						"`{0} is a type not a namespace. A using namespace directive can only be applied to namespaces", Name.ToString ());
				}
				return resolved;
			}
		}

		public abstract class AliasEntry {
			public readonly string Name;
			public readonly NamespaceEntry NamespaceEntry;
			public readonly Location Location;
			
			protected AliasEntry (NamespaceEntry entry, string name, Location loc)
			{
				Name = name;
				NamespaceEntry = entry;
				Location = loc;
			}
			
			protected FullNamedExpression resolved;
			bool error;

			public FullNamedExpression Resolve ()
			{
				if (resolved != null || error)
					return resolved;
				resolved = DoResolve ();
				if (resolved == null)
					error = true;
				return resolved;
			}

			protected abstract FullNamedExpression DoResolve ();
		}

		public class LocalAliasEntry : AliasEntry
		{
			public readonly Expression Alias;
			
			public LocalAliasEntry (NamespaceEntry entry, string name, MemberName alias, Location loc) :
				base (entry, name, loc)
			{
				Alias = alias.GetTypeExpression ();
			}

			protected override FullNamedExpression DoResolve ()
			{
				DeclSpace root = RootContext.Tree.Types;
				root.NamespaceEntry = NamespaceEntry;
				resolved = Alias.ResolveAsTypeStep (root.EmitContext, false);
				root.NamespaceEntry = null;

				if (resolved == null)
					Error_NamespaceNotFound (Location, Alias.ToString ());
				return resolved;
			}
		}

		public class ExternAliasEntry : AliasEntry 
		{
			public ExternAliasEntry (NamespaceEntry entry, string name, Location loc) :
				base (entry, name, loc)
			{
			}

			protected override FullNamedExpression DoResolve ()
			{
				resolved = RootNamespace.GetRootNamespace (Name);
				if (resolved == null)
					Report.Error (430, Location, "The extern alias '" + Name +
									"' was not specified in a /reference option");

				return resolved;
			}
		}

		public NamespaceEntry (NamespaceEntry parent, SourceFile file, string name, Location loc)
		{
			this.parent = parent;
			this.file = file;
			this.IsImplicit = false;
			entries.Add (this);
			this.ID = entries.Count;

			if (parent != null)
				ns = parent.NS.GetNamespace (name, true);
			else if (name != null)
				ns = RootNamespace.Global.GetNamespace (name, true);
			else
				ns = RootNamespace.Global;
		}

		private NamespaceEntry (NamespaceEntry parent, SourceFile file, Namespace ns)
		{
			this.parent = parent;
			this.file = file;
			// no need to add self to 'entries', since we don't have any aliases or using entries.
			this.ID = -1;
			this.IsImplicit = true;
			this.ns = ns;
		}

		//
		// According to section 16.3.1 (using-alias-directive), the namespace-or-type-name is
		// resolved as if the immediately containing namespace body has no using-directives.
		//
		// Section 16.3.2 says that the same rule is applied when resolving the namespace-name
		// in the using-namespace-directive.
		//
		// To implement these rules, the expressions in the using directives are resolved using 
		// the "doppelganger" (ghostly bodiless duplicate).
		//
		NamespaceEntry doppelganger;
		NamespaceEntry Doppelganger {
			get {
				if (!IsImplicit && doppelganger == null)
					doppelganger = new NamespaceEntry (ImplicitParent, file, ns);
				return doppelganger;
			}
		}

		public readonly int ID;
		public readonly bool IsImplicit;

		public Namespace NS {
			get { return ns; }
		}

		public NamespaceEntry Parent {
			get { return parent; }
		}

		public NamespaceEntry ImplicitParent {
			get {
				if (parent == null)
					return null;
				if (implicit_parent == null) {
					implicit_parent = (parent.NS == ns.Parent)
						? parent
						: new NamespaceEntry (parent, file, ns.Parent);
				}
				return implicit_parent;
			}
		}

		/// <summary>
		///   Records a new namespace for resolving name references
		/// </summary>
		public void Using (MemberName name, Location loc)
		{
			if (DeclarationFound){
				Report.Error (1529, loc, "A using clause must precede all other namespace elements except extern alias declarations");
				return;
			}

			if (name.Equals (ns.MemberName))
				return;
			
			if (using_clauses == null)
				using_clauses = new ArrayList ();

			foreach (UsingEntry old_entry in using_clauses) {
				if (name.Equals (old_entry.Name)) {
					Report.Warning (105, 3, loc, "The using directive for `{0}' appeared previously in this namespace", name.GetName ());
						return;
					}
				}

			UsingEntry ue = new UsingEntry (Doppelganger, name, loc);
			using_clauses.Add (ue);
		}

		public void UsingAlias (string name, MemberName alias, Location loc)
		{
			if (DeclarationFound){
				Report.Error (1529, loc, "A using clause must precede all other namespace elements except extern alias declarations");
				return;
			}

			if (aliases == null)
				aliases = new Hashtable ();

			if (aliases.Contains (name)) {
				AliasEntry ae = (AliasEntry)aliases [name];
				Report.SymbolRelatedToPreviousError (ae.Location, ae.Name);
				Report.Error (1537, loc, "The using alias `" + name +
					      "' appeared previously in this namespace");
				return;
			}

			if (RootContext.Version == LanguageVersion.Default &&
			    name == "global" && RootContext.WarningLevel >= 2)
				Report.Warning (440, 2, loc, "An alias named `global' will not be used when resolving 'global::';" +
					" the global namespace will be used instead");

			aliases [name] = new LocalAliasEntry (Doppelganger, name, alias, loc);
		}

		public void UsingExternalAlias (string name, Location loc)
		{
			if (UsingFound || DeclarationFound) {
				Report.Error (439, loc, "An extern alias declaration must precede all other elements");
				return;
			}
			
			if (aliases == null)
				aliases = new Hashtable ();
			
			if (aliases.Contains (name)) {
				AliasEntry ae = (AliasEntry) aliases [name];
				Report.SymbolRelatedToPreviousError (ae.Location, ae.Name);
				Report.Error (1537, loc, "The using alias `" + name +
					      "' appeared previously in this namespace");
				return;
			}

			if (name == "global") {
				Report.Error (1681, loc, "You cannot redefine the global extern alias");
				return;
			}

			aliases [name] = new ExternAliasEntry (Doppelganger, name, loc);
		}

		public FullNamedExpression LookupNamespaceOrType (DeclSpace ds, string name, Location loc, bool ignore_cs0104)
		{
			// Precondition: Only simple names (no dots) will be looked up with this function.
			FullNamedExpression resolved = null;
			for (NamespaceEntry curr_ns = this; curr_ns != null; curr_ns = curr_ns.ImplicitParent) {
				if ((resolved = curr_ns.Lookup (ds, name, loc, ignore_cs0104)) != null)
					break;
			}
			return resolved;
		}

		static void Error_AmbiguousTypeReference (Location loc, string name, FullNamedExpression t1, FullNamedExpression t2)
		{
			Report.Error (104, loc, "`{0}' is an ambiguous reference between `{1}' and `{2}'",
				name, t1.FullName, t2.FullName);
		}

		// Looks-up a alias named @name in this and surrounding namespace declarations
		public FullNamedExpression LookupAlias (string name)
		{
			AliasEntry entry = null;
			// We use Parent rather than ImplicitParent since we know implicit namespace declarations
			// cannot have using entries.
			for (NamespaceEntry n = this; n != null; n = n.Parent) {
				if (n.aliases == null)
					continue;
				entry = n.aliases [name] as AliasEntry;
				if (entry != null)
					return entry.Resolve ();
			}
			return null;
		}

		private FullNamedExpression Lookup (DeclSpace ds, string name, Location loc, bool ignore_cs0104)
		{
			//
			// Check whether it's in the namespace.
			//
			FullNamedExpression fne = NS.Lookup (ds, name, loc);
			if (fne != null)
				return fne;

			if (IsImplicit)
				return null;

			//
			// Check aliases.
			//
			if (aliases != null) {
				AliasEntry entry = aliases [name] as AliasEntry;
				if (entry != null)
					return entry.Resolve ();
			}

			//
			// Check using entries.
			//
			FullNamedExpression match = null;
			foreach (Namespace using_ns in GetUsingTable ()) {
				match = using_ns.Lookup (ds, name, loc);
				if (match == null || !(match is TypeExpr))
					continue;
				if (fne != null) {
					if (!ignore_cs0104)
						Error_AmbiguousTypeReference (loc, name, fne, match);
					return null;
				}
				fne = match;
			}

			return fne;
		}

		// Our cached computation.
		readonly Namespace [] empty_namespaces = new Namespace [0];
		Namespace [] namespace_using_table;
		Namespace [] GetUsingTable ()
		{
			if (namespace_using_table != null)
				return namespace_using_table;

			if (using_clauses == null) {
				namespace_using_table = empty_namespaces;
				return namespace_using_table;
			}

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

		readonly string [] empty_using_list = new string [0];

		public int SymbolFileID {
			get {
				if (symfile_id == 0 && file.SourceFileEntry != null) {
					int parent_id = parent == null ? 0 : parent.SymbolFileID;

					string [] using_list = empty_using_list;
					if (using_clauses != null) {
						using_list = new string [using_clauses.Count];
						for (int i = 0; i < using_clauses.Count; i++)
							using_list [i] = ((UsingEntry) using_clauses [i]).Name.ToString ();
					}

					symfile_id = CodeGen.SymbolWriter.DefineNamespace (ns.Name, file.SourceFileEntry, using_list, parent_id);
				}
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

		public static void Error_NamespaceNotFound (Location loc, string name)
		{
			Report.Error (246, loc, "The type or namespace name `{0}' could not be found. Are you missing a using directive or an assembly reference?",
				name);

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
		void VerifyUsing ()
		{
			if (using_clauses != null) {
				foreach (UsingEntry ue in using_clauses)
					ue.Resolve ();
			}

			if (aliases != null) {
				foreach (DictionaryEntry de in aliases)
					((AliasEntry) de.Value).Resolve ();
			}
		}

		/// <summary>
		///   Used to validate that all the using clauses are correct
		///   after we are finished parsing all the files.  
		/// </summary>
		static public void VerifyAllUsing ()
		{
			foreach (NamespaceEntry entry in entries)
				entry.VerifyUsing ();
		}

		public string GetSignatureForError ()
		{
			return ns.GetSignatureForError ();
		}

		public override string ToString ()
		{
			return ns.ToString ();
		}
	}
}
