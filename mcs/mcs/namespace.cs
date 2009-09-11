//
// namespace.cs: Tracks namespaces
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// Copyright 2001 Ximian, Inc.
// Copyright 2003-2008 Novell, Inc.
//
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace Mono.CSharp {

	public class RootNamespace : Namespace {
		//
		// Points to Mono's GetNamespaces method, an
		// optimization when running on Mono to fetch all the
		// namespaces in an assembly
		//
		static MethodInfo get_namespaces_method;

		protected readonly string alias_name;
		protected Assembly [] referenced_assemblies;

		Hashtable all_namespaces;

		static RootNamespace ()
		{
			get_namespaces_method = typeof (Assembly).GetMethod ("GetNamespaces", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public RootNamespace (string alias_name)
			: base (null, String.Empty)
		{
			this.alias_name = alias_name;
			referenced_assemblies = new Assembly [0];

			all_namespaces = new Hashtable ();
			all_namespaces.Add ("", this);
		}

		public void AddAssemblyReference (Assembly a)
		{
			foreach (Assembly assembly in referenced_assemblies) {
				if (a == assembly)
					return;
			}

			int top = referenced_assemblies.Length;
			Assembly [] n = new Assembly [top + 1];
			referenced_assemblies.CopyTo (n, 0);
			n [top] = a;
			referenced_assemblies = n;
		}

		public void ComputeNamespace (CompilerContext ctx, Type extensionType)
		{
			foreach (Assembly a in referenced_assemblies) {
				try {
					ComputeNamespaces (a, extensionType);
				} catch (TypeLoadException e) {
					ctx.Report.Error (11, Location.Null, e.Message);
				} catch (System.IO.FileNotFoundException) {
					ctx.Report.Error (12, Location.Null, "An assembly `{0}' is used without being referenced",
						a.FullName);
				}
			}
		}

		public virtual Type LookupTypeReflection (CompilerContext ctx, string name, Location loc, bool must_be_unique)
		{
			Type found_type = null;

			foreach (Assembly a in referenced_assemblies) {
				Type t = GetTypeInAssembly (a, name);
				if (t == null)
					continue;

				if (!must_be_unique)
					return t;

				if (found_type == null) {
					found_type = t;
					continue;
				}

				// When type is forwarded
				if (t.Assembly == found_type.Assembly)
					continue;					

				ctx.Report.SymbolRelatedToPreviousError (found_type);
				ctx.Report.SymbolRelatedToPreviousError (t);
				if (loc.IsNull) {
					Error_AmbiguousPredefinedType (ctx, loc, name, found_type);
				} else {
					ctx.Report.Error (433, loc, "The imported type `{0}' is defined multiple times", name);
				}

				return found_type;
			}

			return found_type;
		}

		//
		// Returns the types starting with the given prefix
		//
		public ICollection CompletionGetTypesStartingWith (string prefix)
		{
			Hashtable result = null;

			foreach (Assembly a in referenced_assemblies){
				Type [] mtypes = a.GetTypes ();

				foreach (Type t in mtypes){
					string f = t.FullName;

					if (f.StartsWith (prefix) && (result == null || !result.Contains (f))){
						if (result == null)
							result = new Hashtable ();

						result [f] = f;
					}
				}
			}
			return result == null ? result : result.Keys;
		}
		
		protected static void Error_AmbiguousPredefinedType (CompilerContext ctx, Location loc, string name, Type type)
		{
			ctx.Report.Warning (1685, 1, loc,
				"The predefined type `{0}' is ambiguous. Using definition from `{1}'",
				name, type.Assembly.FullName);
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

		protected void RegisterNamespace (string dotted_name)
		{
			if (dotted_name != null && dotted_name.Length != 0 && ! IsNamespace (dotted_name))
				GetNamespace (dotted_name, true);
		}

		void RegisterExtensionMethodClass (Type t)
 		{
			string n = t.Namespace;
			Namespace ns = n == null ? GlobalRootNamespace.Instance : (Namespace) all_namespaces[n];
 			if (ns == null)
 				ns = GetNamespace (n, true);
 
 			ns.RegisterExternalExtensionMethodClass (t);
 		}

  		void ComputeNamespaces (Assembly assembly, Type extensionType)
  		{
			bool contains_extension_methods = extensionType != null && assembly.IsDefined (extensionType, false);
 
 			if (get_namespaces_method != null) {
  				string [] namespaces = (string []) get_namespaces_method.Invoke (assembly, null);
  				foreach (string ns in namespaces)
 					RegisterNamespace (ns);

				if (!contains_extension_methods)
					return;
  			}

 			foreach (Type t in assembly.GetTypes ()) {
 				if ((t.Attributes & Class.StaticClassAttribute) == Class.StaticClassAttribute &&
 					contains_extension_methods && t.IsDefined (extensionType, false))
 					RegisterExtensionMethodClass (t);

				if (get_namespaces_method == null)
					RegisterNamespace (t.Namespace);
 			}
  		}

		protected static Type GetTypeInAssembly (Assembly assembly, string name)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");
			if (name == null)
				throw new ArgumentNullException ("name");
			Type t = assembly.GetType (name);
			if (t == null)
				return null;

			if (t.IsPointer)
				throw new InternalErrorException ("Use GetPointerType() to get a pointer");

			TypeAttributes ta = t.Attributes & TypeAttributes.VisibilityMask;
			if (ta == TypeAttributes.NestedPrivate)
				return null;

			if ((ta == TypeAttributes.NotPublic ||
			     ta == TypeAttributes.NestedAssembly ||
			     ta == TypeAttributes.NestedFamANDAssem) &&
			    !TypeManager.IsThisOrFriendAssembly (t.Assembly))
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
		Module [] modules;
		ListDictionary root_namespaces;

		public static GlobalRootNamespace Instance = new GlobalRootNamespace ();

		GlobalRootNamespace ()
			: base ("global")
		{
			root_namespaces = new ListDictionary ();
			root_namespaces.Add (alias_name, this);
		}

		public static void Reset ()
		{
			Instance = new GlobalRootNamespace ();
		}

		public Assembly [] Assemblies {
		    get { return referenced_assemblies; }
		}

		public Module [] Modules {
			get { return modules; }
		}

		public void AddModuleReference (Module m)
		{
			int top = modules != null ? modules.Length : 0;
			Module [] n = new Module [top + 1];
			if (modules != null)
				modules.CopyTo (n, 0);
			n [top] = m;
			modules = n;

			if (m == RootContext.ToplevelTypes.Builder)
				return;

			foreach (Type t in m.GetTypes ())
				RegisterNamespace (t.Namespace);
		}

		public void ComputeNamespaces (CompilerContext ctx)
		{
			//
			// Do very early lookup because type is required when we cache
			// imported extension types in ComputeNamespaces
			//
			Type extension_attribute_type = TypeManager.CoreLookupType (ctx, "System.Runtime.CompilerServices", "ExtensionAttribute", Kind.Class, false);

			foreach (RootNamespace rn in root_namespaces.Values) {
				rn.ComputeNamespace (ctx, extension_attribute_type);
			}
		}

		public void DefineRootNamespace (string alias, Assembly assembly, CompilerContext ctx)
		{
			if (alias == alias_name) {
				NamespaceEntry.Error_GlobalNamespaceRedefined (Location.Null, ctx.Report);
				return;
			}

			RootNamespace retval = GetRootNamespace (alias);
			if (retval == null) {
				retval = new RootNamespace (alias);
				root_namespaces.Add (alias, retval);
			}

			retval.AddAssemblyReference (assembly);
		}

		public override void Error_NamespaceDoesNotExist (Location loc, string name, Report Report)
		{
			Report.Error (400, loc, "The type or namespace name `{0}' could not be found in the global namespace (are you missing an assembly reference?)",
				name);
		}

		public RootNamespace GetRootNamespace (string name)
		{
			return (RootNamespace) root_namespaces[name];
		}

		public override Type LookupTypeReflection (CompilerContext ctx, string name, Location loc, bool must_be_unique)
		{
			Type found_type = base.LookupTypeReflection (ctx, name, loc, must_be_unique);

			if (modules != null) {
				foreach (Module module in modules) {
					Type t = module.GetType (name);
					if (t == null)
						continue;

					if (found_type == null) {
						found_type = t;
						continue;
					}

					ctx.Report.SymbolRelatedToPreviousError (found_type);
					if (loc.IsNull) {
						DeclSpace ds = TypeManager.LookupDeclSpace (t);
						Error_AmbiguousPredefinedType (ctx, ds.Location, name, found_type);
						return found_type;
					}
					ctx.Report.SymbolRelatedToPreviousError (t);
					ctx.Report.Warning (436, 2, loc, "The type `{0}' conflicts with the imported type `{1}'. Ignoring the imported type definition",
						TypeManager.CSharpName (t), TypeManager.CSharpName (found_type));
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
		IDictionary namespaces;
		IDictionary declspaces;
		Hashtable cached_types;
		RootNamespace root;
		ArrayList external_exmethod_classes;

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
			this.Type = typeof (Namespace);
			this.loc = Location.Null;

			this.parent = parent;

			if (parent != null)
				this.root = parent.root;
			else
				this.root = this as RootNamespace;

			if (this.root == null)
				throw new InternalErrorException ("Root namespaces must be created using RootNamespace");
			
			string pname = parent != null ? parent.fullname : "";
				
			if (pname == "")
				fullname = name;
			else
				fullname = parent.fullname + "." + name;

			if (fullname == null)
				throw new InternalErrorException ("Namespace has a null fullname");

			if (parent != null && parent.MemberName != MemberName.Null)
				MemberName = new MemberName (parent.MemberName, name);
			else if (name.Length == 0)
				MemberName = MemberName.Null;
			else
				MemberName = new MemberName (name);

			namespaces = new HybridDictionary ();
			cached_types = new Hashtable ();

			root.RegisterNamespace (this);
		}

		public override Expression DoResolve (ResolveContext ec)
		{
			return this;
		}

		public virtual void Error_NamespaceDoesNotExist (Location loc, string name, Report Report)
		{
			if (name.IndexOf ('`') > 0) {
				FullNamedExpression retval = Lookup (RootContext.ToplevelTypes.Compiler, SimpleName.RemoveGenericArity (name), loc);
				if (retval != null) {
					Error_TypeArgumentsCannotBeUsed (retval, loc);
					return;
				}
			} else {
				Type t = LookForAnyGenericType (name);
				if (t != null) {
					Error_InvalidNumberOfTypeArguments (t, loc);
					return;
				}
			}

			Report.Error (234, loc, "The type or namespace name `{0}' does not exist in the namespace `{1}'. Are you missing an assembly reference?",
				name, GetSignatureForError ());
		}

		public static void Error_InvalidNumberOfTypeArguments (Type t, Location loc)
		{
			RootContext.ToplevelTypes.Compiler.Report.SymbolRelatedToPreviousError (t);
			RootContext.ToplevelTypes.Compiler.Report.Error (305, loc, "Using the generic type `{0}' requires `{1}' type argument(s)",
				TypeManager.CSharpName(t), TypeManager.GetNumberOfTypeArguments(t).ToString());
		}

		public static void Error_TypeArgumentsCannotBeUsed (FullNamedExpression expr, Location loc)
		{
			if (expr is TypeExpr) {
				RootContext.ToplevelTypes.Compiler.Report.SymbolRelatedToPreviousError (expr.Type);
				Error_TypeArgumentsCannotBeUsed (loc, "type", expr.GetSignatureForError ());
			} else {
				RootContext.ToplevelTypes.Compiler.Report.Error (307, loc, "The {0} `{1}' cannot be used with type arguments",
					expr.ExprClassName, expr.GetSignatureForError ());
			}
		}

		public static void Error_TypeArgumentsCannotBeUsed (MethodBase mi, Location loc)
		{
			RootContext.ToplevelTypes.Compiler.Report.SymbolRelatedToPreviousError (mi);
			Error_TypeArgumentsCannotBeUsed (loc, "method", TypeManager.CSharpSignature (mi));
		}

		static void Error_TypeArgumentsCannotBeUsed (Location loc, string type, string name)
		{
			RootContext.ToplevelTypes.Compiler.Report.Error(308, loc, "The non-generic {0} `{1}' cannot be used with the type arguments",
				type, name);
		}

		public override string GetSignatureForError ()
		{
			return fullname;
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

		public bool HasDefinition (string name)
		{
			return declspaces != null && declspaces [name] != null;
		}

		TypeExpr LookupType (CompilerContext ctx, string name, Location loc)
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

					if (RootContext.EvalMode){
						// Replace the TypeBuilder with a System.Type, as
						// Reflection.Emit fails otherwise (we end up pretty
						// much with Random type definitions later on).
						Type tt = t.Assembly.GetType (t.Name);
						if (tt != null)
							t = tt;
					}
				}
			}
			string lookup = t != null ? t.FullName : (fullname.Length == 0 ? name : fullname + "." + name);
			Type rt = root.LookupTypeReflection (ctx, lookup, loc, t == null);

			// HACK: loc.IsNull when the type is core type
			if (t == null || (rt != null && loc.IsNull))
				t = rt;

			TypeExpr te = t == null ? null : new TypeExpression (t, Location.Null);
			cached_types [name] = te;
			return te;
		}

		///
		/// Used for better error reporting only
		/// 
		public Type LookForAnyGenericType (string typeName)
		{
			if (declspaces == null)
				return null;

			typeName = SimpleName.RemoveGenericArity (typeName);

			foreach (DictionaryEntry de in declspaces) {
				string type_item = (string) de.Key;
				int pos = type_item.LastIndexOf ('`');
				if (pos == typeName.Length && String.Compare (typeName, 0, type_item, 0, pos) == 0)
					return ((DeclSpace) de.Value).TypeBuilder;
			}
			return null;
		}

		public FullNamedExpression Lookup (CompilerContext ctx, string name, Location loc)
		{
			if (namespaces.Contains (name))
				return (Namespace) namespaces [name];

			return LookupType (ctx, name, loc);
		}

		//
		// Completes types with the given `prefix' and stores the results in `result'
		//
		public void CompletionGetTypesStartingWith (string prefix, Hashtable result)
		{
			int l = fullname.Length + 1;
			ICollection res = root.CompletionGetTypesStartingWith (fullname + "." + prefix);

			if (res == null)
				return;
			
			foreach (string match in res){
				string x = match.Substring (l);

				// Turn reflection nested classes foo+bar into foo.bar
				x = x.Replace ('+', '.');

				// Only get the first name element, no point in adding anything beyond the first dot.
				int p = x.IndexOf ('.');
				if (p != -1)
					x = x.Substring (0, p);

				// Turn Foo`N into Foo<
				p = x.IndexOf ('`');
				if (p != -1)
					x = x.Substring (0, p) + "<";

				if (!result.Contains (x))
					result [x] = x;
			}
		}

		public void RegisterExternalExtensionMethodClass (Type type)
		{
			// Ignore, extension methods cannot be nested
			if (type.DeclaringType != null)
				return;

			if (type.IsNotPublic && !TypeManager.IsThisOrFriendAssembly (type.Assembly))
				return;

			if (external_exmethod_classes == null)
				external_exmethod_classes = new ArrayList ();

			external_exmethod_classes.Add (type);
		}

		/// 
		/// Looks for extension method in this namespace
		/// 
		public ArrayList LookupExtensionMethod (Type extensionType, ClassOrStruct currentClass, string name)
		{
			ArrayList found = null;

			if (declspaces != null) {
				IEnumerator e = declspaces.Values.GetEnumerator ();
				e.Reset ();
				while (e.MoveNext ()) {
					Class c = e.Current as Class;
					if (c == null)
						continue;

					if ((c.ModFlags & Modifiers.METHOD_EXTENSION) == 0)
						continue;

					ArrayList res = c.MemberCache.FindExtensionMethods (extensionType, name, c != currentClass);
					if (res == null)
						continue;

					if (found == null)
						found = res;
					else
						found.AddRange (res);
				}
			}

			if (external_exmethod_classes == null)
				return found;

			foreach (Type t in external_exmethod_classes) {
				MemberCache m = TypeHandle.GetMemberCache (t);
				ArrayList res = m.FindExtensionMethods (extensionType, name, true);
				if (res == null)
					continue;

				if (found == null)
					found = res;
				else
					found.AddRange (res);
			}

			return found;
		}

		public void AddDeclSpace (string name, DeclSpace ds)
		{
			if (declspaces == null)
				declspaces = new HybridDictionary ();
			declspaces.Add (name, ds);
		}

		public void RemoveDeclSpace (string name)
		{
			declspaces.Remove (name);
		}
		
		/// <summary>
		///   The qualified name of the current namespace
		/// </summary>
		public string Name {
			get { return fullname; }
		}

		/// <summary>
		///   The parent of this namespace, used by the parser to "Pop"
		///   the current namespace declaration
		/// </summary>
		public Namespace Parent {
			get { return parent; }
		}
	}

	//
	// Namespace container as created by the parser
	//
	public class NamespaceEntry : IMemberContext {

		class UsingEntry {
			readonly MemberName name;
			Namespace resolved;
			
			public UsingEntry (MemberName name)
			{
				this.name = name;
			}

			public string GetSignatureForError ()
			{
				return name.GetSignatureForError ();
			}

			public Location Location {
				get { return name.Location; }
			}

			public MemberName MemberName {
				get { return name; }
			}
			
			public string Name {
				get { return GetSignatureForError (); }
			}

			public Namespace Resolve (IMemberContext rc)
			{
				if (resolved != null)
					return resolved;

				FullNamedExpression fne = name.GetTypeExpression ().ResolveAsTypeStep (rc, false);
				if (fne == null)
					return null;

				resolved = fne as Namespace;
				if (resolved == null) {
					rc.Compiler.Report.SymbolRelatedToPreviousError (fne.Type);
					rc.Compiler.Report.Error (138, Location,
						"`{0}' is a type not a namespace. A using namespace directive can only be applied to namespaces",
						GetSignatureForError ());
				}
				return resolved;
			}

			public override string ToString ()
			{
				return Name;
			}
		}

		class UsingAliasEntry {
			public readonly string Alias;
			public Location Location;

			public UsingAliasEntry (string alias, Location loc)
			{
				this.Alias = alias;
				this.Location = loc;
			}

			public virtual FullNamedExpression Resolve (IMemberContext rc)
			{
				FullNamedExpression fne = GlobalRootNamespace.Instance.GetRootNamespace (Alias);
				if (fne == null) {
					rc.Compiler.Report.Error (430, Location,
						"The extern alias `{0}' was not specified in -reference option",
						Alias);
				}

				return fne;
			}

			public override string ToString ()
			{
				return Alias;
			}
			
		}

		class LocalUsingAliasEntry : UsingAliasEntry {
			FullNamedExpression resolved;
			MemberName value;

			public LocalUsingAliasEntry (string alias, MemberName name, Location loc)
				: base (alias, loc)
			{
				this.value = name;
			}

			public override FullNamedExpression Resolve (IMemberContext rc)
			{
				if (resolved != null || value == null)
					return resolved;

				if (rc == null)
					return null;

				resolved = value.GetTypeExpression ().ResolveAsTypeStep (rc, false);
				if (resolved == null) {
					value = null;
					return null;
				}

				if (resolved is TypeExpr)
					resolved = resolved.ResolveAsBaseTerminal (rc, false);

				return resolved;
			}

			public override string ToString ()
			{
				return String.Format ("{0} = {1}", Alias, value.GetSignatureForError ());
			}
		}

		Namespace ns;
		NamespaceEntry parent, implicit_parent;
		CompilationUnit file;
		int symfile_id;

		// Namespace using import block
		ArrayList using_aliases;
		ArrayList using_clauses;
		public bool DeclarationFound = false;
		// End

		public readonly bool IsImplicit;
		public readonly DeclSpace SlaveDeclSpace;
		static readonly Namespace [] empty_namespaces = new Namespace [0];
		Namespace [] namespace_using_table;

		static ArrayList entries = new ArrayList ();

		public static void Reset ()
		{
			entries = new ArrayList ();
		}

		public NamespaceEntry (NamespaceEntry parent, CompilationUnit file, string name)
		{
			this.parent = parent;
			this.file = file;
			entries.Add (this);

			if (parent != null)
				ns = parent.NS.GetNamespace (name, true);
			else if (name != null)
				ns = GlobalRootNamespace.Instance.GetNamespace (name, true);
			else
				ns = GlobalRootNamespace.Instance;
			SlaveDeclSpace = new RootDeclSpace (this);
		}

		private NamespaceEntry (NamespaceEntry parent, CompilationUnit file, Namespace ns, bool slave)
		{
			this.parent = parent;
			this.file = file;
			this.IsImplicit = true;
			this.ns = ns;
			this.SlaveDeclSpace = slave ? new RootDeclSpace (this) : null;
		}

		//
		// Populates the Namespace with some using declarations, used by the
		// eval mode. 
		//
		public void Populate (ArrayList source_using_aliases, ArrayList source_using_clauses)
		{
			foreach (UsingAliasEntry uae in source_using_aliases){
				if (using_aliases == null)
					using_aliases = new ArrayList ();
				
				using_aliases.Add (uae);
			}

			foreach (UsingEntry ue in source_using_clauses){
				if (using_clauses == null)
					using_clauses = new ArrayList ();
				
				using_clauses.Add (ue);
			}
		}

		//
		// Extracts the using alises and using clauses into a couple of
		// arrays that might already have the same information;  Used by the
		// C# Eval mode.
		//
		public void Extract (ArrayList out_using_aliases, ArrayList out_using_clauses)
		{
			if (using_aliases != null){
				foreach (UsingAliasEntry uae in using_aliases){
					bool replaced = false;
					
					for (int i = 0; i < out_using_aliases.Count; i++){
						UsingAliasEntry out_uea = (UsingAliasEntry) out_using_aliases [i];
						
						if (out_uea.Alias == uae.Alias){
							out_using_aliases [i] = uae;
							replaced = true;
							break;
						}
					}
					if (!replaced)
						out_using_aliases.Add (uae);
				}
			}

			if (using_clauses != null){
				foreach (UsingEntry ue in using_clauses){
					bool found = false;
					
					foreach (UsingEntry out_ue in out_using_clauses)
						if (out_ue.Name == ue.Name){
							found = true;
							break;
						}
					if (!found)
						out_using_clauses.Add (ue);
				}
			}
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
				if (!IsImplicit && doppelganger == null) {
					doppelganger = new NamespaceEntry (ImplicitParent, file, ns, true);
					doppelganger.using_aliases = using_aliases;
				}
				return doppelganger;
			}
		}

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
						: new NamespaceEntry (parent, file, ns.Parent, false);
				}
				return implicit_parent;
			}
		}

		/// <summary>
		///   Records a new namespace for resolving name references
		/// </summary>
		public void AddUsing (MemberName name, Location loc)
		{
			if (DeclarationFound){
				Compiler.Report.Error (1529, loc, "A using clause must precede all other namespace elements except extern alias declarations");
			}

			if (using_clauses == null) {
				using_clauses = new ArrayList ();
			} else {
				foreach (UsingEntry old_entry in using_clauses) {
					if (name.Equals (old_entry.MemberName)) {
						Compiler.Report.SymbolRelatedToPreviousError (old_entry.Location, old_entry.GetSignatureForError ());
						Compiler.Report.Warning (105, 3, loc, "The using directive for `{0}' appeared previously in this namespace", name.GetSignatureForError ());
						return;
					}
				}
			}

			using_clauses.Add (new UsingEntry (name));
		}

		public void AddUsingAlias (string alias, MemberName name, Location loc)
		{
			// TODO: This is parser bussines
			if (DeclarationFound){
				Compiler.Report.Error (1529, loc, "A using clause must precede all other namespace elements except extern alias declarations");
			}

			if (RootContext.Version != LanguageVersion.ISO_1 && alias == "global")
				Compiler.Report.Warning (440, 2, loc, "An alias named `global' will not be used when resolving 'global::';" +
					" the global namespace will be used instead");

			AddUsingAlias (new LocalUsingAliasEntry (alias, name, loc));
		}

		public void AddUsingExternalAlias (string alias, Location loc, Report Report)
		{
			// TODO: Do this in parser
			bool not_first = using_clauses != null || DeclarationFound;
			if (using_aliases != null && !not_first) {
				foreach (UsingAliasEntry uae in using_aliases) {
					if (uae is LocalUsingAliasEntry) {
						not_first = true;
						break;
					}
				}
			}

			if (not_first)
				Report.Error (439, loc, "An extern alias declaration must precede all other elements");

			if (alias == "global") {
				Error_GlobalNamespaceRedefined (loc, Report);
				return;
			}

			AddUsingAlias (new UsingAliasEntry (alias, loc));
		}

		void AddUsingAlias (UsingAliasEntry uae)
		{
			if (using_aliases == null) {
				using_aliases = new ArrayList ();
			} else {
				foreach (UsingAliasEntry entry in using_aliases) {
					if (uae.Alias == entry.Alias) {
						Compiler.Report.SymbolRelatedToPreviousError (uae.Location, uae.Alias);
						Compiler.Report.Error (1537, entry.Location, "The using alias `{0}' appeared previously in this namespace",
							entry.Alias);
						return;
					}
				}
			}

			using_aliases.Add (uae);
		}

		///
		/// Does extension methods look up to find a method which matches name and extensionType.
		/// Search starts from this namespace and continues hierarchically up to top level.
		///
		public ExtensionMethodGroupExpr LookupExtensionMethod (Type extensionType, string name, Location loc)
		{
			ArrayList candidates = null;
			foreach (Namespace n in GetUsingTable ()) {
				ArrayList a = n.LookupExtensionMethod (extensionType, null, name);
				if (a == null)
					continue;

				if (candidates == null)
					candidates = a;
				else
					candidates.AddRange (a);
			}

			if (candidates != null)
				return new ExtensionMethodGroupExpr (candidates, parent, extensionType, loc);

			if (parent == null)
				return null;

			//
			// Inspect parent namespaces in namespace expression
			//
			Namespace parent_ns = ns.Parent;
			do {
				candidates = parent_ns.LookupExtensionMethod (extensionType, null, name);
				if (candidates != null)
					return new ExtensionMethodGroupExpr (candidates, parent, extensionType, loc);

				parent_ns = parent_ns.Parent;
			} while (parent_ns != null);

			//
			// Continue in parent scope
			//
			return parent.LookupExtensionMethod (extensionType, name, loc);
		}

		public FullNamedExpression LookupNamespaceOrType (string name, Location loc, bool ignore_cs0104)
		{
			// Precondition: Only simple names (no dots) will be looked up with this function.
			FullNamedExpression resolved = null;
			for (NamespaceEntry curr_ns = this; curr_ns != null; curr_ns = curr_ns.ImplicitParent) {
				if ((resolved = curr_ns.Lookup (name, loc, ignore_cs0104)) != null)
					break;
			}
			return resolved;
		}

		public ICollection CompletionGetTypesStartingWith (string prefix)
		{
			Hashtable result = new Hashtable ();
			
			for (NamespaceEntry curr_ns = this; curr_ns != null; curr_ns = curr_ns.ImplicitParent){
				foreach (Namespace using_ns in GetUsingTable ()){
					if (prefix.StartsWith (using_ns.Name)){
						int ld = prefix.LastIndexOf ('.');
						if (ld != -1){
							string rest = prefix.Substring (ld+1);

							using_ns.CompletionGetTypesStartingWith (rest, result);
						}
					}
					using_ns.CompletionGetTypesStartingWith (prefix, result);
				}
			}

			return result.Keys;
		}
		
		void Error_AmbiguousTypeReference (Location loc, string name, FullNamedExpression t1, FullNamedExpression t2)
		{
			Compiler.Report.SymbolRelatedToPreviousError (t1.Type);
			Compiler.Report.SymbolRelatedToPreviousError (t2.Type);
			Compiler.Report.Error (104, loc, "`{0}' is an ambiguous reference between `{1}' and `{2}'",
				name, t1.GetSignatureForError (), t2.GetSignatureForError ());
		}

		// Looks-up a alias named @name in this and surrounding namespace declarations
		public FullNamedExpression LookupNamespaceAlias (string name)
		{
			for (NamespaceEntry n = this; n != null; n = n.ImplicitParent) {
				if (n.using_aliases == null)
					continue;

				foreach (UsingAliasEntry ue in n.using_aliases) {
					if (ue.Alias == name)
						return ue.Resolve (Doppelganger);
				}
			}

			return null;
		}

		private FullNamedExpression Lookup (string name, Location loc, bool ignore_cs0104)
		{
			//
			// Check whether it's in the namespace.
			//
			FullNamedExpression fne = ns.Lookup (Compiler, name, loc);

			//
			// Check aliases. 
			//
			if (using_aliases != null) {
				foreach (UsingAliasEntry ue in using_aliases) {
					if (ue.Alias == name) {
						if (fne != null) {
							if (Doppelganger != null) {
								// TODO: Namespace has broken location
								//Report.SymbolRelatedToPreviousError (fne.Location, null);
								Compiler.Report.SymbolRelatedToPreviousError (ue.Location, null);
								Compiler.Report.Error (576, loc,
									"Namespace `{0}' contains a definition with same name as alias `{1}'",
									GetSignatureForError (), name);
							} else {
								return fne;
							}
						}

						return ue.Resolve (Doppelganger);
					}
				}
			}

			if (fne != null)
				return fne;

			if (IsImplicit)
				return null;

			//
			// Check using entries.
			//
			FullNamedExpression match = null;
			foreach (Namespace using_ns in GetUsingTable ()) {
				match = using_ns.Lookup (Compiler, name, loc);
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
				Namespace using_ns = ue.Resolve (Doppelganger);
				if (using_ns == null)
					continue;

				list.Add (using_ns);
			}

			namespace_using_table = (Namespace[])list.ToArray (typeof (Namespace));
			return namespace_using_table;
		}

		static readonly string [] empty_using_list = new string [0];

		public int SymbolFileID {
			get {
				if (symfile_id == 0 && file.SourceFileEntry != null) {
					int parent_id = parent == null ? 0 : parent.SymbolFileID;

					string [] using_list = empty_using_list;
					if (using_clauses != null) {
						using_list = new string [using_clauses.Count];
						for (int i = 0; i < using_clauses.Count; i++)
							using_list [i] = ((UsingEntry) using_clauses [i]).MemberName.GetName ();
					}

					symfile_id = SymbolWriter.DefineNamespace (ns.Name, file.CompileUnitEntry, using_list, parent_id);
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

		public static void Error_GlobalNamespaceRedefined (Location loc, Report Report)
		{
			Report.Error (1681, loc, "You cannot redefine the global extern alias");
		}

		public static void Error_NamespaceNotFound (Location loc, string name, Report Report)
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
			if (using_aliases != null) {
				foreach (UsingAliasEntry ue in using_aliases)
					ue.Resolve (Doppelganger);
			}

			if (using_clauses != null) {
				foreach (UsingEntry ue in using_clauses)
					ue.Resolve (Doppelganger);
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

		#region IMemberContext Members

		public CompilerContext Compiler {
			get { return RootContext.ToplevelTypes.Compiler; }
		}

		public Type CurrentType {
			get { return SlaveDeclSpace.CurrentType; }
		}

		public TypeContainer CurrentTypeDefinition {
			get { return SlaveDeclSpace.CurrentTypeDefinition; }
		}

		public TypeParameter[] CurrentTypeParameters {
			get { return SlaveDeclSpace.CurrentTypeParameters; }
		}

		public bool IsObsolete {
			get { return SlaveDeclSpace.IsObsolete; }
		}

		public bool IsUnsafe {
			get { return SlaveDeclSpace.IsUnsafe; }
		}

		public bool IsStatic {
			get { return SlaveDeclSpace.IsStatic; }
		}

		#endregion
	}
}
