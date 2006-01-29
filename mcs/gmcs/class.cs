//
// class.cs: Class and Struct handlers
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Martin Baulig (martin@ximian.com)
//          Marek Safar (marek.safar@seznam.cz)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2004 Novell, Inc
//
//
//  2002-10-11  Miguel de Icaza  <miguel@ximian.com>
//
//	* class.cs: Following the comment from 2002-09-26 to AddMethod, I
//	have fixed a remaining problem: not every AddXXXX was adding a
//	fully qualified name.  
//
//	Now everyone registers a fully qualified name in the DeclSpace as
//	being defined instead of the partial name.  
//
//	Downsides: we are slower than we need to be due to the excess
//	copies and the names being registered this way.  
//
//	The reason for this is that we currently depend (on the corlib
//	bootstrap for instance) that types are fully qualified, because
//	we dump all the types in the namespace, and we should really have
//	types inserted into the proper namespace, so we can only store the
//	basenames in the defined_names array.
//
//
#define CACHE
using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Xml;

using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp {

	public enum Kind {
		Root,
		Struct,
		Class,
		Interface
	}

	/// <summary>
	///   This is the base class for structs and classes.  
	/// </summary>
	public abstract class TypeContainer : DeclSpace, IMemberContainer {

 		public class MemberCoreArrayList: ArrayList
 		{
			/// <summary>
			///   Defines the MemberCore objects that are in this array
			/// </summary>
			public virtual void DefineContainerMembers ()
			{
				foreach (MemberCore mc in this) {
					mc.Define ();
				}
			}

			public virtual void Emit ()
			{
				foreach (MemberCore mc in this)
					mc.Emit ();
			}
 		}

 		public class MethodArrayList : MemberCoreArrayList
 		{
 			[Flags]
 			enum CachedMethods {
 				Equals			= 1,
 				GetHashCode		= 1 << 1
 			}
 
 			CachedMethods cached_method;
			TypeContainer container;

			public MethodArrayList (TypeContainer container)
			{
				this.container = container;
			}
 
 			/// <summary>
 			/// Method container contains Equals method
 			/// </summary>
 			public bool HasEquals {
 				set {
 					cached_method |= CachedMethods.Equals;
 				}
 
 				get {
 					return (cached_method & CachedMethods.Equals) != 0;
 				}
 			}
 
 			/// <summary>
 			/// Method container contains GetHashCode method
 			/// </summary>
 			public bool HasGetHashCode {
 				set {
 					cached_method |= CachedMethods.GetHashCode;
 				}
 
 				get {
 					return (cached_method & CachedMethods.GetHashCode) != 0;
 				}
 			}
 
 			public override void DefineContainerMembers ()
 			{
 				base.DefineContainerMembers ();
 
 				if (HasEquals && !HasGetHashCode) {
 					Report.Warning (659, 3, container.Location, "`{0}' overrides Object.Equals(object) but does not override Object.GetHashCode()", container.GetSignatureForError ());
 				}
 			}
 
 		}

		public sealed class IndexerArrayList : MemberCoreArrayList
		{
			/// <summary>
			/// The indexer name for this container
			/// </summary>
 			public string IndexerName = DefaultIndexerName;

			bool seen_normal_indexers = false;

			TypeContainer container;

			public IndexerArrayList (TypeContainer container)
			{
				this.container = container;
			}

			/// <summary>
			/// Defines the indexers, and also verifies that the IndexerNameAttribute in the
			/// class is consistent.  Either it is `Item' or it is the name defined by all the
			/// indexers with the `IndexerName' attribute.
			///
			/// Turns out that the IndexerNameAttribute is applied to each indexer,
			/// but it is never emitted, instead a DefaultMember attribute is attached
			/// to the class.
			/// </summary>
			public override void DefineContainerMembers()
			{
				base.DefineContainerMembers ();

				string class_indexer_name = null;

				//
				// If there's both an explicit and an implicit interface implementation, the
				// explicit one actually implements the interface while the other one is just
				// a normal indexer.  See bug #37714.
				//

				// Invariant maintained by AddIndexer(): All explicit interface indexers precede normal indexers
				foreach (Indexer i in this) {
					if (i.InterfaceType != null) {
						if (seen_normal_indexers)
							throw new Exception ("Internal Error: 'Indexers' array not sorted properly.");
						continue;
					}

					seen_normal_indexers = true;

					if (class_indexer_name == null) {
						class_indexer_name = i.ShortName;
						continue;
					}

					if (i.ShortName != class_indexer_name)
						Report.Error (668, i.Location, "Two indexers have different names; the IndexerName attribute must be used with the same name on every indexer within a type");
				}

				if (class_indexer_name != null)
					IndexerName = class_indexer_name;
			}

			public override void Emit ()
			{
				base.Emit ();

				if (!seen_normal_indexers)
					return;

				CustomAttributeBuilder cb = new CustomAttributeBuilder (TypeManager.default_member_ctor, new string [] { IndexerName });
				container.TypeBuilder.SetCustomAttribute (cb);
			}
		}

 		public class OperatorArrayList: MemberCoreArrayList
		{
			TypeContainer container;

			public OperatorArrayList (TypeContainer container)
			{
				this.container = container;
			}

			//
			// Operator pair checking
			//
			class OperatorEntry
			{
				public int flags;
				public Type ret_type;
				public Type type1, type2;
				public Operator op;
				public Operator.OpType ot;
				
				public OperatorEntry (int f, Operator o)
				{
					flags = f;

					ret_type = o.OperatorMethod.ReturnType;
					Type [] pt = o.OperatorMethod.ParameterTypes;
					type1 = pt [0];
					type2 = pt [1];
					op = o;
					ot = o.OperatorType;
				}

				public override int GetHashCode ()
				{	
					return ret_type.GetHashCode ();
				}

				public override bool Equals (object o)
				{
					OperatorEntry other = (OperatorEntry) o;

					if (other.ret_type != ret_type)
						return false;
					if (other.type1 != type1)
						return false;
					if (other.type2 != type2)
						return false;
					return true;
				}
			}
				
			//
			// Checks that some operators come in pairs:
			//  == and !=
			// > and <
			// >= and <=
			// true and false
			//
			// They are matched based on the return type and the argument types
			//
			void CheckPairedOperators ()
			{
				IDictionary pairs = new HybridDictionary ();
				Operator true_op = null;
				Operator false_op = null;
				bool has_equality_or_inequality = false;
				
				// Register all the operators we care about.
				foreach (Operator op in this){
					int reg = 0;

					// Skip erroneous code.
					if (op.OperatorMethod == null)
						continue;

					switch (op.OperatorType){
					case Operator.OpType.Equality:
						reg = 1;
						has_equality_or_inequality = true;
						break;
					case Operator.OpType.Inequality:
						reg = 2;
						has_equality_or_inequality = true;
						break;

					case Operator.OpType.True:
						true_op = op;
						break;
					case Operator.OpType.False:
						false_op = op;
						break;
						
					case Operator.OpType.GreaterThan:
						reg = 1; break;
					case Operator.OpType.LessThan:
						reg = 2; break;
						
					case Operator.OpType.GreaterThanOrEqual:
						reg = 1; break;
					case Operator.OpType.LessThanOrEqual:
						reg = 2; break;
					}
					if (reg == 0)
						continue;

					OperatorEntry oe = new OperatorEntry (reg, op);

					object o = pairs [oe];
					if (o == null)
						pairs [oe] = oe;
					else {
						oe = (OperatorEntry) o;
						oe.flags |= reg;
					}
				}

				if (true_op != null){
					if (false_op == null)
						Report.Error (216, true_op.Location, "The operator `{0}' requires a matching operator `false' to also be defined",
							true_op.GetSignatureForError ());
				} else if (false_op != null)
					Report.Error (216, false_op.Location, "The operator `{0}' requires a matching operator `true' to also be defined",
						false_op.GetSignatureForError ());
				
				//
				// Look for the mistakes.
				//
				foreach (DictionaryEntry de in pairs){
					OperatorEntry oe = (OperatorEntry) de.Key;

					if (oe.flags == 3)
						continue;

					string s = "";
					switch (oe.ot){
					case Operator.OpType.Equality:
						s = "!=";
						break;
					case Operator.OpType.Inequality: 
						s = "==";
						break;
					case Operator.OpType.GreaterThan: 
						s = "<";
						break;
					case Operator.OpType.LessThan:
						s = ">";
						break;
					case Operator.OpType.GreaterThanOrEqual:
						s = "<=";
						break;
					case Operator.OpType.LessThanOrEqual:
						s = ">=";
						break;
					}
					Report.Error (216, oe.op.Location,
						"The operator `{0}' requires a matching operator `{1}' to also be defined",
						oe.op.GetSignatureForError (), s);
				}

 				if (has_equality_or_inequality && (RootContext.WarningLevel > 2)) {
 					if (container.Methods == null || !container.Methods.HasEquals)
 						Report.Warning (660, 2, container.Location, "`{0}' defines operator == or operator != but does not override Object.Equals(object o)", container.GetSignatureForError ());
 
 					if (container.Methods == null || !container.Methods.HasGetHashCode)
 						Report.Warning (661, 2, container.Location, "`{0}' defines operator == or operator != but does not override Object.GetHashCode()", container.GetSignatureForError ());
 				}
			}

	 		public override void DefineContainerMembers ()
	 		{
	 			base.DefineContainerMembers ();
	 			CheckPairedOperators ();
			}
		}


		// Whether this is a struct, class or interface
		public readonly Kind Kind;

		// Holds a list of classes and structures
		ArrayList types;

		// Holds the list of properties
		MemberCoreArrayList properties;

		// Holds the list of enumerations
		MemberCoreArrayList enums;

		// Holds the list of delegates
		MemberCoreArrayList delegates;
		
		// Holds the list of constructors
		protected MemberCoreArrayList instance_constructors;

		// Holds the list of fields
		MemberCoreArrayList fields;

		// Holds a list of fields that have initializers
		protected ArrayList initialized_fields;

		// Holds a list of static fields that have initializers
		protected ArrayList initialized_static_fields;

		// Holds the list of constants
		MemberCoreArrayList constants;

		// Holds the list of
		MemberCoreArrayList interfaces;

		// Holds the methods.
		MethodArrayList methods;

		// Holds the events
		protected MemberCoreArrayList events;

		// Holds the indexers
		IndexerArrayList indexers;

		// Holds the operators
		MemberCoreArrayList operators;

		// Holds the iterators
		ArrayList iterators;

		// Holds the parts of a partial class;
		ArrayList parts;

		//
		// Pointers to the default constructor and the default static constructor
		//
		protected Constructor default_constructor;
		protected Constructor default_static_constructor;

		//
		// Points to the first non-static field added to the container.
		//
		// This is an arbitrary choice.  We are interested in looking at _some_ non-static field,
		// and the first one's as good as any.
		//
		FieldBase first_nonstatic_field = null;

		//
		// This one is computed after we can distinguish interfaces
		// from classes from the arraylist `type_bases' 
		//
		string base_class_name;
		TypeExpr base_type;
		TypeExpr[] iface_exprs;

		ArrayList type_bases;

		bool members_defined;
		bool members_defined_ok;

		// The interfaces we implement.
		protected Type[] ifaces;
		protected Type ptype;

		// The base member cache and our member cache
		MemberCache base_cache;
		MemberCache member_cache;

		public const string DefaultIndexerName = "Item";

		Type GenericType;
		GenericTypeParameterBuilder[] gen_params;

		public TypeContainer (NamespaceEntry ns, TypeContainer parent, MemberName name,
				      Attributes attrs, Kind kind)
			: base (ns, parent, name, attrs)
		{
			if (parent != null && parent != RootContext.Tree.Types && parent.NamespaceEntry != ns)
				throw new InternalErrorException ("A nested type should be in the same NamespaceEntry as its enclosing class");

			this.Kind = kind;

			types = new ArrayList ();

			base_class_name = null;
		}

		public bool AddToMemberContainer (MemberCore symbol)
		{
			return AddToContainer (symbol, symbol.MemberName.MethodName);
		}

		protected virtual bool AddToTypeContainer (DeclSpace ds)
		{
			return AddToContainer (ds, ds.Basename);
		}

		public void AddConstant (Const constant)
		{
			if (!AddToMemberContainer (constant))
				return;

			if (constants == null)
				constants = new MemberCoreArrayList ();

			constants.Add (constant);
		}

		public void AddEnum (Mono.CSharp.Enum e)
		{
			if (!AddToTypeContainer (e))
				return;

			if (enums == null)
				enums = new MemberCoreArrayList ();

			enums.Add (e);
		}
		
		public bool AddClassOrStruct (TypeContainer c)
		{
			if (!AddToTypeContainer (c))
				return false;

			types.Add (c);
			return true;
		}

		public void AddDelegate (Delegate d)
		{
			if (!AddToTypeContainer (d))
				return;

			if (delegates == null)
				delegates = new MemberCoreArrayList ();
			
			delegates.Add (d);
		}

		public void AddMethod (Method method)
		{
			if (!AddToMemberContainer (method))
				return;

			if (methods == null)
				methods = new MethodArrayList (this);

			if (method.MemberName.Left != null)
				methods.Insert (0, method);
			else 
				methods.Add (method);
		}

		//
		// Do not use this method: use AddMethod.
		//
		// This is only used by iterators.
		//
		public void AppendMethod (Method method)
		{
			if (!AddToMemberContainer (method))
				return;

			if (methods == null)
				methods = new MethodArrayList (this);

			methods.Add (method);
		}

		public void AddConstructor (Constructor c)
		{
			if (c.Name != MemberName.Name) {
				Report.Error (1520, c.Location, "Class, struct, or interface method must have a return type");
			}

			bool is_static = (c.ModFlags & Modifiers.STATIC) != 0;
			
			if (is_static){
				if (default_static_constructor != null) {
					Report.SymbolRelatedToPreviousError (default_static_constructor);
					Report.Error (111, c.Location, Error111, c.GetSignatureForError ());
					return;
				}

				default_static_constructor = c;
			} else {
				if (c.IsDefault ()){
					if (default_constructor != null) {
						Report.SymbolRelatedToPreviousError (default_constructor);
						Report.Error (111, c.Location, Error111, c.GetSignatureForError ());
						return;
					}
					default_constructor = c;
				}
				
				if (instance_constructors == null)
					instance_constructors = new MemberCoreArrayList ();
				
				instance_constructors.Add (c);
			}
		}

		internal static string Error111 {
			get {
				return "`{0}' is already defined. Rename this member or use different parameter types";
			}
		}
		
		public bool AddInterface (TypeContainer iface)
		{
			if (!AddToTypeContainer (iface))
				return false;

			if (interfaces == null) {
				interfaces = new MemberCoreArrayList ();
			}

			interfaces.Add (iface);
			return true;
		}

		public void AddField (FieldMember field)
		{
			if (!AddToMemberContainer (field))
				return;

			if (fields == null)
				fields = new MemberCoreArrayList ();

			fields.Add (field);
			
			if ((field.ModFlags & Modifiers.STATIC) != 0)
				return;

			if (first_nonstatic_field == null) {
				first_nonstatic_field = field;
				return;
			}

			if (Kind == Kind.Struct &&
			    first_nonstatic_field.Parent != field.Parent &&
			    RootContext.WarningLevel >= 3) {
				Report.SymbolRelatedToPreviousError (first_nonstatic_field.Parent);
				Report.Warning (282, 3, field.Location,
					"struct instance field `{0}' found in different declaration from instance field `{1}'",
					field.GetSignatureForError (), first_nonstatic_field.GetSignatureForError ());
			}
		}

		public void AddProperty (Property prop)
		{
			if (!AddToMemberContainer (prop) || 
				!AddToMemberContainer (prop.Get) || !AddToMemberContainer (prop.Set))
				return;

			if (properties == null)
				properties = new MemberCoreArrayList ();

			if (prop.MemberName.Left != null)
				properties.Insert (0, prop);
			else
				properties.Add (prop);
		}

		public void AddEvent (Event e)
		{
			if (!AddToMemberContainer (e))
				return;

			if (e is EventProperty) {
				if (!AddToMemberContainer (e.Add))
					return;

				if (!AddToMemberContainer (e.Remove))
					return;
			}

			if (events == null)
				events = new MemberCoreArrayList ();

			events.Add (e);
		}

		/// <summary>
		/// Indexer has special handling in constrast to other AddXXX because the name can be driven by IndexerNameAttribute
		/// </summary>
		public void AddIndexer (Indexer i)
		{
			if (indexers == null)
				indexers = new IndexerArrayList (this);

			if (i.IsExplicitImpl)
				indexers.Insert (0, i);
			else
				indexers.Add (i);
		}

		public void AddOperator (Operator op)
		{
			if (!AddToMemberContainer (op))
				return;

			if (operators == null)
				operators = new OperatorArrayList (this);

			operators.Add (op);
		}

		public void AddIterator (Iterator i)
		{
			if (iterators == null)
				iterators = new ArrayList ();

			iterators.Add (i);
		}

		public void AddType (TypeContainer tc)
		{
			types.Add (tc);
		}

		public void AddPart (ClassPart part)
		{
			if (parts == null)
				parts = new ArrayList ();

			parts.Add (part);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.default_member_type) {
				if (Indexers != null) {
					Report.Error (646, a.Location, "Cannot specify the `DefaultMember' attribute on type containing an indexer");
					return;
				}
			}
			
			base.ApplyAttributeBuilder (a, cb);
               } 

		public override AttributeTargets AttributeTargets {
			get {
				switch (Kind) {
				case Kind.Class:
					return AttributeTargets.Class;
				case Kind.Struct:
					return AttributeTargets.Struct;
				case Kind.Interface:
					return AttributeTargets.Interface;
				default:
					throw new NotSupportedException ();
				}
			}
		}

		public ArrayList Types {
			get {
				return types;
			}
		}

		public MethodArrayList Methods {
			get {
				return methods;
			}
		}

		public ArrayList Constants {
			get {
				return constants;
			}
		}

		public ArrayList Interfaces {
			get {
				return interfaces;
			}
		}

		public ArrayList Iterators {
			get {
				return iterators;
			}
		}
		
		public string Base {
			get {
				return base_class_name;
			}
		}
		
		public ArrayList Bases {
			get {
				return type_bases;
			}

			set {
				type_bases = value;
			}
		}

		public ArrayList Fields {
			get {
				return fields;
			}
		}

		public ArrayList InstanceConstructors {
			get {
				return instance_constructors;
			}
		}

		public ArrayList Properties {
			get {
				return properties;
			}
		}

		public ArrayList Events {
			get {
				return events;
			}
		}
		
		public ArrayList Enums {
			get {
				return enums;
			}
		}

		public ArrayList Indexers {
			get {
				return indexers;
			}
		}

		public ArrayList Operators {
			get {
				return operators;
			}
		}

		public ArrayList Delegates {
			get {
				return delegates;
			}
		}
		
		public ArrayList Parts {
			get {
				return parts;
			}
		}

		protected override TypeAttributes TypeAttr {
			get {
				return Modifiers.TypeAttr (ModFlags, this) | base.TypeAttr;
			}
		}

		public string IndexerName {
			get {
				return indexers == null ? DefaultIndexerName : indexers.IndexerName;
			}
		}

		public bool IsComImport {
			get {
				if (OptAttributes == null)
					return false;

				return OptAttributes.Contains (TypeManager.comimport_attr_type, EmitContext);
			}
		}

		public virtual void RegisterFieldForInitialization (FieldBase field)
		{
			if ((field.ModFlags & Modifiers.STATIC) != 0){
				if (initialized_static_fields == null)
					initialized_static_fields = new ArrayList (4);

				initialized_static_fields.Add (field);
			} else {
				if (initialized_fields == null)
					initialized_fields = new ArrayList (4);

				initialized_fields.Add (field);
			}
		}

		//
		// Emits the instance field initializers
		//
		public virtual bool EmitFieldInitializers (EmitContext ec)
		{
			ArrayList fields;
			
			if (ec.IsStatic){
				fields = initialized_static_fields;
			} else {
				fields = initialized_fields;
			}

			if (fields == null)
				return true;

			foreach (FieldBase f in fields) {
				f.EmitInitializer (ec);
			}
			return true;
		}
		
		//
		// Defines the default constructors
		//
		protected void DefineDefaultConstructor (bool is_static)
		{
			Constructor c;

			// The default constructor is public
			// If the class is abstract, the default constructor is protected
			// The default static constructor is private

			int mods = Modifiers.PUBLIC;
			if (is_static)
				mods = Modifiers.STATIC | Modifiers.PRIVATE;
			else if ((ModFlags & Modifiers.ABSTRACT) != 0)
				mods = Modifiers.PROTECTED;

			TypeContainer constructor_parent = this;
			if (Parts != null)
				constructor_parent = (TypeContainer) Parts [0];

			c = new Constructor (constructor_parent, MemberName.Name, mods,
					     Parameters.EmptyReadOnlyParameters,
					     new GeneratedBaseInitializer (Location),
					     Location);
			
			AddConstructor (c);
			
			c.Block = new ToplevelBlock (null, Location);
			
		}

		/// <remarks>
		///  The pending methods that need to be implemented
		//   (interfaces or abstract methods)
		/// </remarks>
		public PendingImplementation Pending;

		public abstract PendingImplementation GetPendingImplementations ();

		TypeExpr[] GetPartialBases (out TypeExpr base_class)
		{
			ArrayList ifaces = new ArrayList ();

			base_class = null;

			foreach (ClassPart part in parts) {
				TypeExpr new_base_class;
				TypeExpr[] new_ifaces;

				new_ifaces = part.GetClassBases (out new_base_class);
				if (new_ifaces == null && new_base_class != null)
					return null;

				if ((base_class != null) && (new_base_class != null) &&
				    !base_class.Equals (new_base_class)) {
					Report.SymbolRelatedToPreviousError (base_class.Location, "");
					Report.Error (263, part.Location,
						      "Partial declarations of `{0}' must " +
						      "not specify different base classes",
						      Name);

					return null;
				}

				if ((base_class == null) && (new_base_class != null)) {
					base_class = new_base_class;
				}

				if (new_ifaces == null)
					continue;

				foreach (TypeExpr iface in new_ifaces) {
					bool found = false;
					foreach (TypeExpr old_iface in ifaces) {
						if (old_iface.Equals (iface)) {
							found = true;
							break;
						}
					}

					if (!found)
						ifaces.Add (iface);
				}
			}

			TypeExpr[] retval = new TypeExpr [ifaces.Count];
			ifaces.CopyTo (retval, 0);
			return retval;
		}

		TypeExpr[] GetNormalBases (out TypeExpr base_class)
		{
			base_class = null;

			int count = Bases.Count;
			int start = 0, i, j;

			if (Kind == Kind.Class){
				TypeExpr name = ResolveBaseTypeExpr (
					(Expression) Bases [0], false, Location);

				if (name == null){
					return null;
				}

				if (!name.IsInterface) {
					// base_class could be a class, struct, enum, delegate.
					// This is validated in GetClassBases.
					base_class = name;
					start = 1;
				}
			}

			TypeExpr [] ifaces = new TypeExpr [count-start];
			
			for (i = start, j = 0; i < count; i++, j++){
				TypeExpr resolved = ResolveBaseTypeExpr ((Expression) Bases [i], false, Location);
				if (resolved == null) {
					return null;
				}
				
				ifaces [j] = resolved;
			}

			return ifaces;
		}

		/// <summary>
		///   This function computes the Base class and also the
		///   list of interfaces that the class or struct @c implements.
		///   
		///   The return value is an array (might be null) of
		///   interfaces implemented (as Types).
		///   
		///   The @base_class argument is set to the base object or null
		///   if this is `System.Object'. 
		/// </summary>
		protected virtual TypeExpr [] GetClassBases (out TypeExpr base_class)
		{
			int i;

			TypeExpr[] ifaces;

			if (parts != null)
				ifaces = GetPartialBases (out base_class);
			else if (Bases == null){
				base_class = null;
				return null;
			} else
				ifaces = GetNormalBases (out base_class);

			if (ifaces == null)
				return null;

			if ((base_class != null) && (Kind == Kind.Class)){
				if (base_class is TypeParameterExpr){
					Report.Error (
						689, base_class.Location,
						"Cannot derive from `{0}' because it is a type parameter",
						base_class.GetSignatureForError ());
					error = true;
					return null;
				}

				if (base_class.Type.IsArray || base_class.Type.IsPointer) {
					Report.Error (1521, base_class.Location, "Invalid base type");
					return null;
				}

				if (base_class.IsSealed){
					Report.SymbolRelatedToPreviousError (base_class.Type);
					if (base_class.Type.IsAbstract) {
						Report.Error (709, Location, "`{0}': Cannot derive from static class `{1}'",
							GetSignatureForError (), TypeManager.CSharpName (base_class.Type));
					} else {
						Report.Error (509, Location, "`{0}': cannot derive from sealed class `{1}'",
							GetSignatureForError (), TypeManager.CSharpName (base_class.Type));
					}
					return null;
				}

				if (!base_class.CanInheritFrom ()){
					Report.Error (644, Location, "`{0}' cannot derive from special class `{1}'",
						      GetSignatureForError (), base_class.GetSignatureForError ());
					return null;
				}

				if (!base_class.AsAccessible (this, ModFlags)) {
					Report.SymbolRelatedToPreviousError (base_class.Type);
					Report.Error (60, Location, "Inconsistent accessibility: base class `{0}' is less accessible than class `{1}'", 
						TypeManager.CSharpName (base_class.Type), GetSignatureForError ());
				}
			}

			if (base_class != null)
				base_class_name = base_class.Name;

			if (ifaces == null)
				return null;

			int count = ifaces != null ? ifaces.Length : 0;

			for (i = 0; i < count; i++) {
				TypeExpr iface = (TypeExpr) ifaces [i];

				if (!iface.IsInterface) {
					if (Kind != Kind.Class) {
						// TODO: location of symbol related ....
						Error_TypeInListIsNotInterface (Location, iface.FullName);
					}
					else if (base_class != null)
						Report.Error (1721, Location, "`{0}': Classes cannot have multiple base classes (`{1}' and `{2}')",
							GetSignatureForError (), base_class.GetSignatureForError (), iface.GetSignatureForError ());
					else {
						Report.Error (1722, Location, "`{0}': Base class `{1}' must be specified as first",
							GetSignatureForError (), iface.GetSignatureForError ());
					}
					return null;
				}
  
				for (int x = 0; x < i; x++) {
					if (iface.Equals (ifaces [x])) {
						Report.Error (528, Location,
							      "`{0}' is already listed in " +
							      "interface list", iface.Name);
						return null;
					}
				}

				if ((Kind == Kind.Interface) &&
				    !iface.AsAccessible (Parent, ModFlags)) {
					Report.Error (61, Location,
						      "Inconsistent accessibility: base " +
						      "interface `{0}' is less accessible " +
						      "than interface `{1}'", iface.Name,
						      Name);
					return null;
				}
			}
			return ifaces;
		}

		bool CheckGenericInterfaces (Type[] ifaces)
		{
			ArrayList already_checked = new ArrayList ();

			for (int i = 0; i < ifaces.Length; i++) {
				Type iface = ifaces [i];
				foreach (Type t in already_checked) {
					if (iface == t)
						continue;

					Type[] infered = new Type [CountTypeParameters];
					if (!TypeManager.MayBecomeEqualGenericInstances (iface, t, infered, null))
						continue;

					Report.Error (695, Location,
						"`{0}' cannot implement both `{1}' and `{2}' " +
						"because they may unify for some type parameter substitutions",
						TypeManager.CSharpName (TypeBuilder), TypeManager.CSharpName (iface),
						TypeManager.CSharpName (t));
					return false;
				}

				already_checked.Add (iface);
			}

			return true;
		}

		bool error = false;
		
		protected void Error_TypeInListIsNotInterface (Location loc, string type)
		{
			Report.Error (527, loc, "Type `{0}' in interface list is not an interface", type);
		}

		//
		// Defines the type in the appropriate ModuleBuilder or TypeBuilder.
		//
		public override TypeBuilder DefineType ()
		{
			if (error)
				return null;

			if (TypeBuilder != null)
				return TypeBuilder;

			TypeAttributes type_attributes = TypeAttr;

			try {
				if (IsTopLevel){
					if (TypeManager.NamespaceClash (Name, Location)) {
						error = true;
						return null;
					}

					ModuleBuilder builder = CodeGen.Module.Builder;
					TypeBuilder = builder.DefineType (
						Name, type_attributes, null, null);
				} else {
					TypeBuilder builder = Parent.TypeBuilder;
					if (builder == null) {
						error = true;
						return null;
					}

					TypeBuilder = builder.DefineNestedType (
						Basename, type_attributes, ptype, null);
				}
			} catch (ArgumentException) {
				Report.RuntimeMissingSupport (Location, "static classes");
				error = true;
				return null;
			}

			TypeManager.AddUserType (this);

			if (Parts != null) {
				ec = null;
				foreach (ClassPart part in Parts) {
					part.TypeBuilder = TypeBuilder;
					part.ec = new EmitContext (part, Mono.CSharp.Location.Null, null, null, ModFlags);
					part.ec.ContainerType = TypeBuilder;
				}
			} else {
				//
				// Normally, we create the EmitContext here.
				// The only exception is if we're an Iterator - in this case,
				// we already have the `ec', so we don't want to create a new one.
				//
				if (ec == null)
					ec = new EmitContext (this, Mono.CSharp.Location.Null, null, null, ModFlags);
				ec.ContainerType = TypeBuilder;
			}

			if (IsGeneric) {
				string[] param_names = new string [TypeParameters.Length];
				for (int i = 0; i < TypeParameters.Length; i++)
					param_names [i] = TypeParameters [i].Name;

				gen_params = TypeBuilder.DefineGenericParameters (param_names);

				int offset = CountTypeParameters - CurrentTypeParameters.Length;
				for (int i = offset; i < gen_params.Length; i++)
					CurrentTypeParameters [i - offset].Define (gen_params [i]);
			}

			iface_exprs = GetClassBases (out base_type);
			if (iface_exprs == null && base_type != null) {
				error = true;
				return null;
			}

			//
			// GetClassBases calls ResolveBaseTypeExpr() on the various type expressions involved,
			// which in turn should have called DefineType()s on base types if necessary.
			//
			// None of the code below should trigger DefineType()s on classes that we depend on.
			// Thus, we are eligible to be on the topological sort `type_container_resolve_order'.
			//
			// Let's do it as soon as possible, since code below can call DefineType() on classes
			// that depend on us to be populated before they are.
			//
			if (!(this is Iterator))
				RootContext.RegisterOrder (this); 

			if (base_type == null) {
				if (Kind == Kind.Class){
					if (RootContext.StdLib)
						base_type = TypeManager.system_object_expr;
					else if (Name != "System.Object")
						base_type = TypeManager.system_object_expr;
				} else if (Kind == Kind.Struct){
					//
					// If we are compiling our runtime,
					// and we are defining ValueType, then our
					// base is `System.Object'.
					//
					if (!RootContext.StdLib && Name == "System.ValueType")
						base_type = TypeManager.system_object_expr;
					else if (Kind == Kind.Struct)
						base_type = TypeManager.system_valuetype_expr;
				}
			}

			// Avoid attributes check when parent is not set
			TypeResolveEmitContext.TestObsoleteMethodUsage = false;

			if (base_type != null) {
				// FIXME: I think this should be ...ResolveType (Parent.EmitContext).
				//        However, if Parent == RootContext.Tree.Types, its NamespaceEntry will be null.
				FullNamedExpression fne = base_type.ResolveAsTypeStep (TypeResolveEmitContext);
				if ((fne == null) || (fne.Type == null)) {
					error = true;
					return null;
				}

				ptype = fne.Type;

				if (IsGeneric && TypeManager.IsAttributeType (ptype)) {
					Report.Error (698, base_type.Location,
						      "A generic type cannot derive from `{0}' " +
						      "because it is an attribute class",
						      base_type.Name);
					error = true;
					return null;
				}
			}

			if (!CheckRecursiveDefinition (this)) {
				error = true;
				return null;
			}

			if (ptype != null) {
				TypeBuilder.SetParent (ptype);
			}

			// Attribute is undefined at the begining of corlib compilation
			if (TypeManager.obsolete_attribute_type != null) {
				TypeResolveEmitContext.TestObsoleteMethodUsage = GetObsoleteAttribute () == null;
				if (ptype != null && TypeResolveEmitContext.TestObsoleteMethodUsage) {
					CheckObsoleteType (base_type);
				}
			}

			// add interfaces that were not added at type creation
			if (iface_exprs != null) {
				// FIXME: I think this should be ...ExpandInterfaces (Parent.EmitContext, ...).
				//        However, if Parent == RootContext.Tree.Types, its NamespaceEntry will be null.
				TypeResolveEmitContext.ContainerType = TypeBuilder;
				ifaces = TypeManager.ExpandInterfaces (TypeResolveEmitContext, iface_exprs);
				if (ifaces == null) {
					error = true;
					return null;
				}

				foreach (Type itype in ifaces)
 					TypeBuilder.AddInterfaceImplementation (itype);

				if (!CheckGenericInterfaces (ifaces)) {
					error = true;
					return null;
				}

				TypeManager.RegisterBuilder (TypeBuilder, ifaces);
			}

			if (this is Iterator && !ResolveType ()) {
				error = true;
				return null;
			}

			if (!DefineNestedTypes ()) {
				error = true;
				return null;
			}

			return TypeBuilder;
		}

		public bool ResolveType ()
		{
			if ((base_type != null) &&
			    (base_type.ResolveType (TypeResolveEmitContext) == null)) {
				error = true;
				return false;
			}

			if (!IsGeneric)
				return true;

			TypeExpr current_type = null;
			if (Parts != null) {
				foreach (ClassPart part in Parts) {
					if (!part.DefineTypeParameters ()) {
						error = true;
						return false;
					}
				}
			} else {
				foreach (TypeParameter type_param in CurrentTypeParameters) {
					if (!type_param.Resolve (this)) {
						error = true;
						return false;
					}
				}

				foreach (TypeParameter type_param in TypeParameters) {
					if (!type_param.DefineType (ec)) {
						error = true;
						return false;
					}
				}

				current_type = new ConstructedType (
					TypeBuilder, TypeParameters, Location);
			}

			foreach (TypeParameter type_param in TypeParameters)
				if (!type_param.CheckDependencies (ec)) {
					error = true;
					return false;
				}

			if (current_type != null) {
				current_type = current_type.ResolveAsTypeTerminal (ec);
				if (current_type == null) {
					error = true;
					return false;
				}

				CurrentType = current_type.ResolveType (ec);
			}

			return true;
		}

		protected virtual bool DefineNestedTypes ()
		{
			if (Interfaces != null) {
				foreach (TypeContainer iface in Interfaces)
					if (iface.DefineType () == null)
						return false;
			}
			
			if (Types != null) {
				foreach (TypeContainer tc in Types)
					if (tc.DefineType () == null)
						return false;
			}

			if (Delegates != null) {
				foreach (Delegate d in Delegates)
					if (d.DefineType () == null)
						return false;
			}

			if (Enums != null) {
				foreach (Enum en in Enums)
					if (en.DefineType () == null)
						return false;
			}

			return true;
		}

		TypeContainer InTransit;

		protected bool CheckRecursiveDefinition (TypeContainer tc)
		{
			if (InTransit != null) {
				Report.SymbolRelatedToPreviousError (this);
				if (this is Interface)
					Report.Error (
						529, tc.Location, "Inherited interface `{0}' causes a " +
						"cycle in the interface hierarchy of `{1}'",
						GetSignatureForError (), tc.GetSignatureForError ());
				else
					Report.Error (
						146, tc.Location, "Circular base class dependency " +
						"involving `{0}' and `{1}'",
						tc.GetSignatureForError (), GetSignatureForError ());
				return false;
			}

			InTransit = tc;

			Type parent = ptype;
			if (parent != null) {
				parent = TypeManager.DropGenericTypeArguments (parent);
				TypeContainer ptc = TypeManager.LookupTypeContainer (parent);
				if ((ptc != null) && !ptc.CheckRecursiveDefinition (this))
					return false;
			}

			if (iface_exprs != null) {
				foreach (TypeExpr iface in iface_exprs) {
					Type itype = TypeManager.DropGenericTypeArguments (iface.Type);
					TypeContainer ptc = TypeManager.LookupTypeContainer (itype);
					if ((ptc != null) && !ptc.CheckRecursiveDefinition (this))
						return false;
				}
			}

			InTransit = null;
			return true;
		}

		public static void Error_KeywordNotAllowed (Location loc)
		{
			Report.Error (1530, loc, "Keyword `new' is not allowed on namespace elements");
		}

		/// <summary>
		///   Populates our TypeBuilder with fields and methods
		/// </summary>
		public override bool DefineMembers ()
		{
			if (members_defined)
				return members_defined_ok;

			if (!base.DefineMembers ())
				return false;

			members_defined_ok = DoDefineMembers ();
			members_defined = true;

			return members_defined_ok;
		}

		protected virtual bool DoDefineMembers ()
		{
			if (iface_exprs != null) {
				foreach (TypeExpr iface in iface_exprs) {
					ConstructedType ct = iface as ConstructedType;
					if ((ct != null) && !ct.CheckConstraints (ec))
						return false;
				}
			}

			if (base_type != null) {
				ConstructedType ct = base_type as ConstructedType;
				if ((ct != null) && !ct.CheckConstraints (ec))
					return false;
			}

			if (!IsTopLevel) {
				MemberInfo conflict_symbol = Parent.MemberCache.FindMemberWithSameName (Basename, false, TypeBuilder);
				if (conflict_symbol == null) {
					if ((RootContext.WarningLevel >= 4) && ((ModFlags & Modifiers.NEW) != 0))
						Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
				} else {
					if ((ModFlags & Modifiers.NEW) == 0) {
						Report.SymbolRelatedToPreviousError (conflict_symbol);
						Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
							GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
					}
				}
			}

			DefineContainerMembers (constants);
			DefineContainerMembers (fields);

			if ((Kind == Kind.Class) && !(this is ClassPart)){
				if ((instance_constructors == null) &&
				    !(this is StaticClass)) {
					if (default_constructor == null)
						DefineDefaultConstructor (false);
				}

				if (initialized_static_fields != null &&
				    default_static_constructor == null)
					DefineDefaultConstructor (true);
			}

			if (Kind == Kind.Struct){
				//
				// Structs can not have initialized instance
				// fields
				//
				if (initialized_static_fields != null &&
				    default_static_constructor == null)
					DefineDefaultConstructor (true);

				if (initialized_fields != null)
					ReportStructInitializedInstanceError ();
			}

			Pending = GetPendingImplementations ();

			if (parts != null) {
				foreach (ClassPart part in parts) {
					if (!part.DefineMembers ())
						return false;
				}
			}
			
			//
			// Constructors are not in the defined_names array
			//
			DefineContainerMembers (instance_constructors);

			if (default_static_constructor != null)
				default_static_constructor.Define ();

			DefineContainerMembers (properties);
			DefineContainerMembers (events);
			DefineContainerMembers (indexers);
			DefineContainerMembers (methods);
			DefineContainerMembers (operators);
			DefineContainerMembers (enums);
			DefineContainerMembers (delegates);

			if (CurrentType != null) {
				GenericType = CurrentType;

				ec.ContainerType = GenericType;
			}


#if CACHE
			if (!(this is ClassPart))
				member_cache = new MemberCache (this);
#endif

			if (parts != null) {
				foreach (ClassPart part in parts)
					part.member_cache = member_cache;
			}

			if (iterators != null) {
				foreach (Iterator iterator in iterators) {
					if (iterator.DefineType () == null)
						return false;
				}

				foreach (Iterator iterator in iterators) {
					if (!iterator.DefineMembers ())
						return false;
				}
			}

			return true;
		}

		void ReportStructInitializedInstanceError ()
		{
			foreach (Field f in initialized_fields){
				Report.Error (573, Location,
					"`{0}': Structs cannot have instance field initializers",
					f.GetSignatureForError ());
			}
		}

		protected virtual void DefineContainerMembers (MemberCoreArrayList mcal)
		{
			if (mcal != null)
				mcal.DefineContainerMembers ();
		}

		public override bool Define ()
		{
			if (parts != null) {
				foreach (ClassPart part in parts) {
					if (!part.Define ())
						return false;
				}
			}

			if (iterators != null) {
				foreach (Iterator iterator in iterators) {
					if (!iterator.Define ())
						return false;
				}
			}

			return true;
		}

		public MemberInfo FindBaseMemberWithSameName (string name, bool ignore_methods)
		{
			return BaseCache.FindMemberWithSameName (name, ignore_methods, null);
		}

		/// <summary>
		///   This function is based by a delegate to the FindMembers routine
		/// </summary>
		static bool AlwaysAccept (MemberInfo m, object filterCriteria)
		{
			return true;
		}

		/// <summary>
		///   This filter is used by FindMembers, and we just keep
		///   a global for the filter to `AlwaysAccept'
		/// </summary>
		static MemberFilter accepting_filter;

		
		static TypeContainer ()
		{
			accepting_filter = new MemberFilter (AlwaysAccept);
		}
		
		public MethodInfo[] GetMethods ()
		{
			ArrayList members = new ArrayList ();

			DefineMembers ();

			if (methods != null) {
				int len = methods.Count;
				for (int i = 0; i < len; i++) {
					Method m = (Method) methods [i];

					members.Add (m.MethodBuilder);
				}
			}

			if (operators != null) {
				int len = operators.Count;
				for (int i = 0; i < len; i++) {
					Operator o = (Operator) operators [i];

					members.Add (o.OperatorMethodBuilder);
				}
			}

			if (properties != null) {
				int len = properties.Count;
				for (int i = 0; i < len; i++) {
					Property p = (Property) properties [i];

					if (p.GetBuilder != null)
						members.Add (p.GetBuilder);
					if (p.SetBuilder != null)
						members.Add (p.SetBuilder);
				}
			}
				
			if (indexers != null) {
				int len = indexers.Count;
				for (int i = 0; i < len; i++) {
					Indexer ix = (Indexer) indexers [i];

					if (ix.GetBuilder != null)
						members.Add (ix.GetBuilder);
					if (ix.SetBuilder != null)
						members.Add (ix.SetBuilder);
				}
			}

			if (events != null) {
				int len = events.Count;
				for (int i = 0; i < len; i++) {
					Event e = (Event) events [i];

					if (e.AddBuilder != null)
						members.Add (e.AddBuilder);
					if (e.RemoveBuilder != null)
						members.Add (e.RemoveBuilder);
				}
			}

			MethodInfo[] retMethods = new MethodInfo [members.Count];
			members.CopyTo (retMethods, 0);
			return retMethods;
		}
		
		// Indicated whether container has StructLayout attribute set Explicit
		public virtual bool HasExplicitLayout {
			get {
				return false;
			}
		}

		public override Type FindNestedType (string name)
		{
			ArrayList [] lists = { types, enums, delegates, interfaces };

			for (int j = 0; j < lists.Length; ++j) {
				ArrayList list = lists [j];
				if (list == null)
					continue;
				
				int len = list.Count;
				for (int i = 0; i < len; ++i) {
					DeclSpace ds = (DeclSpace) list [i];
					if (ds.Basename == name) {
						ds.DefineType ();
						return ds.TypeBuilder;
					}
				}
			}

			return null;
		}

		private void FindMembers_NestedTypes (int modflags,
						      BindingFlags bf, MemberFilter filter, object criteria,
						      ref ArrayList members)
		{
			ArrayList [] lists = { types, enums, delegates, interfaces };

			for (int j = 0; j < lists.Length; ++j) {
				ArrayList list = lists [j];
				if (list == null)
					continue;
			
				int len = list.Count;
				for (int i = 0; i < len; i++) {
					DeclSpace ds = (DeclSpace) list [i];
					
					if ((ds.ModFlags & modflags) == 0)
						continue;
					
					TypeBuilder tb = ds.TypeBuilder;
					if (tb == null) {
						if (!(criteria is string) || ds.Basename.Equals (criteria))
							tb = ds.DefineType ();
					}
					
					if (tb != null && (filter (tb, criteria) == true)) {
						if (members == null)
							members = new ArrayList ();
						
						members.Add (tb);
					}
				}
			}
		}
		
		/// <summary>
		///   This method returns the members of this type just like Type.FindMembers would
		///   Only, we need to use this for types which are _being_ defined because MS' 
		///   implementation can't take care of that.
		/// </summary>
		//
		// FIXME: return an empty static array instead of null, that cleans up
		// some code and is consistent with some coding conventions I just found
		// out existed ;-)
		//
		//
		// Notice that in various cases we check if our field is non-null,
		// something that would normally mean that there was a bug elsewhere.
		//
		// The problem happens while we are defining p-invoke methods, as those
		// will trigger a FindMembers, but this happens before things are defined
		//
		// Since the whole process is a no-op, it is fine to check for null here.
		//
		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria)
		{
			ArrayList members = null;

			int modflags = 0;
			if ((bf & BindingFlags.Public) != 0)
				modflags |= Modifiers.PUBLIC | Modifiers.PROTECTED |
					Modifiers.INTERNAL;
			if ((bf & BindingFlags.NonPublic) != 0)
				modflags |= Modifiers.PRIVATE;

			int static_mask = 0, static_flags = 0;
			switch (bf & (BindingFlags.Static | BindingFlags.Instance)) {
			case BindingFlags.Static:
				static_mask = static_flags = Modifiers.STATIC;
				break;

			case BindingFlags.Instance:
				static_mask = Modifiers.STATIC;
				static_flags = 0;
				break;

			default:
				static_mask = static_flags = 0;
				break;
			}

			Timer.StartTimer (TimerType.TcFindMembers);

			if (filter == null)
				filter = accepting_filter; 

			if ((mt & MemberTypes.Field) != 0) {
				if (fields != null) {
					int len = fields.Count;
					for (int i = 0; i < len; i++) {
						FieldMember f = (FieldMember) fields [i];
						
						if ((f.ModFlags & modflags) == 0)
							continue;
						if ((f.ModFlags & static_mask) != static_flags)
							continue;

						FieldBuilder fb = f.FieldBuilder;
						if (fb != null && filter (fb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (fb);
					}
				}
				}

				if (constants != null) {
					int len = constants.Count;
					for (int i = 0; i < len; i++) {
						Const con = (Const) constants [i];
						
						if ((con.ModFlags & modflags) == 0)
							continue;
						if ((con.ModFlags & static_mask) != static_flags)
							continue;

						FieldBuilder fb = con.FieldBuilder;
						if (fb == null) {
							if (con.Define ())
								fb = con.FieldBuilder;
						}
						if (fb != null && filter (fb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (fb);
					}
				}
			}
			}

			if ((mt & MemberTypes.Method) != 0) {
				if (methods != null) {
					int len = methods.Count;
					for (int i = 0; i < len; i++) {
						Method m = (Method) methods [i];
						
						if ((m.ModFlags & modflags) == 0)
							continue;
						if ((m.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder mb = m.MethodBuilder;

						if (mb != null && filter (mb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (mb);
					}
				}
				}

				if (operators != null) {
					int len = operators.Count;
					for (int i = 0; i < len; i++) {
						Operator o = (Operator) operators [i];

						if ((o.ModFlags & modflags) == 0)
							continue;
						if ((o.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder ob = o.OperatorMethodBuilder;
						if (ob != null && filter (ob, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (ob);
					}
				}
				}

				if (properties != null) {
					int len = properties.Count;
					for (int i = 0; i < len; i++) {
						Property p = (Property) properties [i];

						if ((p.ModFlags & modflags) == 0)
							continue;
						if ((p.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder b;

						b = p.GetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
						}

						b = p.SetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
					}
				}
				}
				
				if (indexers != null) {
					int len = indexers.Count;
					for (int i = 0; i < len; i++) {
						Indexer ix = (Indexer) indexers [i];
				
						if ((ix.ModFlags & modflags) == 0)
							continue;
						if ((ix.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder b;

						b = ix.GetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
						}

						b = ix.SetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
					}
				}
			}
			}

			if ((mt & MemberTypes.Event) != 0) {
				if (events != null) {
					int len = events.Count;
					for (int i = 0; i < len; i++) {
						Event e = (Event) events [i];
						
						if ((e.ModFlags & modflags) == 0)
							continue;
						if ((e.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo eb = e.EventBuilder;
						if (eb != null && filter (eb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
						        members.Add (e.EventBuilder);
					}
			}
				}
			}
			
			if ((mt & MemberTypes.Property) != 0){
				if (properties != null) {
					int len = properties.Count;
					for (int i = 0; i < len; i++) {
						Property p = (Property) properties [i];
						
						if ((p.ModFlags & modflags) == 0)
							continue;
						if ((p.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo pb = p.PropertyBuilder;
						if (pb != null && filter (pb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (p.PropertyBuilder);
						}
					}
				}

				if (indexers != null) {
					int len = indexers.Count;
					for (int i = 0; i < len; i++) {
						Indexer ix = (Indexer) indexers [i];

						if ((ix.ModFlags & modflags) == 0)
							continue;
						if ((ix.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo ib = ix.PropertyBuilder;
						if (ib != null && filter (ib, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (ix.PropertyBuilder);
						}
					}
			}
			}
			
			if ((mt & MemberTypes.NestedType) != 0)
				FindMembers_NestedTypes (modflags, bf, filter, criteria, ref members);

			if ((mt & MemberTypes.Constructor) != 0){
				if (((bf & BindingFlags.Instance) != 0) && (instance_constructors != null)){
					int len = instance_constructors.Count;
					for (int i = 0; i < len; i++) {
						Constructor c = (Constructor) instance_constructors [i];
						
						ConstructorBuilder cb = c.ConstructorBuilder;
						if (cb != null && filter (cb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
								members.Add (cb);
					}
				}
				}

				if (((bf & BindingFlags.Static) != 0) && (default_static_constructor != null)){
					ConstructorBuilder cb =
						default_static_constructor.ConstructorBuilder;
					
					if (cb != null && filter (cb, criteria) == true) {
						if (members == null)
							members = new ArrayList ();
						
						members.Add (cb);
				}
			}
			}

			//
			// Lookup members in base if requested.
			//
			if ((bf & BindingFlags.DeclaredOnly) == 0) {
				if (TypeBuilder.BaseType != null) {
				MemberList list = FindMembers (TypeBuilder.BaseType, mt, bf, filter, criteria);
				if (list.Count > 0) {
					if (members == null)
						members = new ArrayList ();
					
				members.AddRange (list);
			}
			}
				
 			}

			Timer.StopTimer (TimerType.TcFindMembers);

			if (members == null)
				return MemberList.Empty;
			else
				return new MemberList (members);
		}

		public override MemberCache MemberCache {
			get {
				return member_cache;
			}
		}

		public static MemberList FindMembers (Type t, MemberTypes mt, BindingFlags bf,
						      MemberFilter filter, object criteria)
		{
			DeclSpace ds = TypeManager.LookupDeclSpace (t);

			if (ds != null)
				return ds.FindMembers (mt, bf, filter, criteria);
			else
				return new MemberList (t.FindMembers (mt, bf, filter, criteria));
		}

		//
		// FindMethods will look for methods not only in the type `t', but in
		// any interfaces implemented by the type.
		//
		public static MethodInfo [] FindMethods (Type t, BindingFlags bf,
							 MemberFilter filter, object criteria)
		{
			return null;
		}

		/// <summary>
		///   Emits the values for the constants
		/// </summary>
		public void EmitConstants ()
		{
			if (constants != null)
				foreach (Const con in constants)
					con.Emit ();
			return;
		}

		void CheckMemberUsage (MemberCoreArrayList al, string member_type)
		{
			if (al == null)
				return;

			foreach (MemberCore mc in al) {
				if ((mc.ModFlags & Modifiers.Accessibility) != Modifiers.PRIVATE)
					continue;

				if (!mc.IsUsed) {
					Report.Warning (169, 3, mc.Location, "The private {0} `{1}' is never used", member_type, mc.GetSignatureForError ());
				}
			}
		}

		public virtual void VerifyMembers ()
		{
			//
			// Check for internal or private fields that were never assigned
			//
			if (RootContext.WarningLevel >= 3) {
				CheckMemberUsage (properties, "property");
				CheckMemberUsage (methods, "method");
				CheckMemberUsage (constants, "constant");

				if (fields != null){
					foreach (FieldMember f in fields) {
						if ((f.ModFlags & Modifiers.Accessibility) != Modifiers.PRIVATE)
							continue;
						
						if (!f.IsUsed){
							if ((f.caching_flags & Flags.IsAssigned) == 0)
								Report.Warning (169, 3, f.Location, "The private field `{0}' is never used", f.GetSignatureForError ());
							else {
#if NET_2_0
								const int error_code = 414;
#else
								const int error_code = 169;
#endif
								Report.Warning (error_code, 3, f.Location, "The private field `{0}' is assigned but its value is never used",
									f.GetSignatureForError ());
							}
							continue;
						}
						
						//
						// Only report 649 on level 4
						//
						if (RootContext.WarningLevel < 4)
							continue;
						
						if ((f.caching_flags & Flags.IsAssigned) != 0)
							continue;
						
						Report.Warning (649, 4, f.Location, "Field `{0}' is never assigned to, and will always have its default value `{1}'",
							f.GetSignatureForError (), f.Type.Type.IsValueType ? Activator.CreateInstance (f.Type.Type).ToString() : "null");
					}
				}
			}
		}

		/// <summary>
		///   Emits the code, this step is performed after all
		///   the types, enumerations, constructors
		/// </summary>
		public virtual void EmitType ()
		{
			if (OptAttributes != null)
				OptAttributes.Emit (ec, this);

			if (IsGeneric && !(this is ClassPart)) {
				int offset = CountTypeParameters - CurrentTypeParameters.Length;
				for (int i = offset; i < gen_params.Length; i++)
					CurrentTypeParameters [i - offset].EmitAttributes (ec);
			}

			//
			// Structs with no fields need to have at least one byte.
			// The right thing would be to set the PackingSize in a DefineType
			// but there are no functions that allow interfaces *and* the size to
			// be specified.
			//

			if (Kind == Kind.Struct && first_nonstatic_field == null){
				FieldBuilder fb = TypeBuilder.DefineField ("$PRIVATE$", TypeManager.byte_type,
									   FieldAttributes.Private);

				if (HasExplicitLayout){
					object [] ctor_args = new object [1];
					ctor_args [0] = 0;
				
					CustomAttributeBuilder cba = new CustomAttributeBuilder (
						TypeManager.field_offset_attribute_ctor, ctor_args);
					fb.SetCustomAttribute (cba);
				}
			}

			Emit ();

			if (instance_constructors != null) {
				if (TypeBuilder.IsSubclassOf (TypeManager.attribute_type) && RootContext.VerifyClsCompliance && IsClsComplianceRequired (this)) {
					bool has_compliant_args = false;

					foreach (Constructor c in instance_constructors) {
						c.Emit ();

						if (has_compliant_args)
							continue;

						has_compliant_args = c.HasCompliantArgs;
					}
					if (!has_compliant_args)
						Report.Error (3015, Location, "`{0}' has no accessible constructors which use only CLS-compliant types", GetSignatureForError ());
				} else {
				foreach (Constructor c in instance_constructors)
						c.Emit ();
				}
			}

			// Can not continue if constants are broken
			EmitConstants ();
			if (Report.Errors > 0)
				return;

			if (default_static_constructor != null)
				default_static_constructor.Emit ();
			
			if (methods != null){
				foreach (Method m in methods)
					m.Emit ();
			}

			if (operators != null)
				foreach (Operator o in operators)
					o.Emit ();

			if (properties != null)
				foreach (Property p in properties)
					p.Emit ();

			if (indexers != null){
				indexers.Emit ();
			}
			
			if (fields != null)
				foreach (FieldMember f in fields)
					f.Emit ();

			if (events != null){
				foreach (Event e in Events)
					e.Emit ();
			}

			if (delegates != null) {
				foreach (Delegate d in Delegates) {
					d.Emit ();
				}
			}

			if (enums != null) {
				foreach (Enum e in enums) {
					e.Emit ();
				}
			}

			if (parts != null) {
				foreach (ClassPart part in parts)
					part.EmitType ();
			}

			if ((Pending != null) && !(this is ClassPart))
				if (Pending.VerifyPendingMethods ())
					return;

			if (iterators != null)
				foreach (Iterator iterator in iterators)
					iterator.EmitType ();
			
//			if (types != null)
//				foreach (TypeContainer tc in types)
//					tc.Emit ();
		}
		
		public override void CloseType ()
		{
			if ((caching_flags & Flags.CloseTypeCreated) != 0)
				return;

			try {
				caching_flags |= Flags.CloseTypeCreated;
					TypeBuilder.CreateType ();
			} catch (TypeLoadException){
				//
				// This is fine, the code still created the type
				//
//				Report.Warning (-20, "Exception while creating class: " + TypeBuilder.Name);
//				Console.WriteLine (e.Message);
			} catch {
				Console.WriteLine ("In type: " + Name);
				throw;
			}
			
			if (Enums != null)
				foreach (Enum en in Enums)
					en.CloseType ();

			if (Types != null){
				foreach (TypeContainer tc in Types)
					if (tc.Kind == Kind.Struct)
						tc.CloseType ();

				foreach (TypeContainer tc in Types)
					if (tc.Kind != Kind.Struct)
						tc.CloseType ();
			}

			if (Delegates != null)
				foreach (Delegate d in Delegates)
					d.CloseType ();

			if (Iterators != null)
				foreach (Iterator i in Iterators)
					i.CloseType ();
			
			types = null;
			properties = null;
			enums = null;
			delegates = null;
			fields = null;
			initialized_fields = null;
			initialized_static_fields = null;
			constants = null;
			interfaces = null;
			methods = null;
			events = null;
			indexers = null;
			operators = null;
			iterators = null;
			ec = null;
			default_constructor = null;
			default_static_constructor = null;
			type_bases = null;
			OptAttributes = null;
			ifaces = null;
			base_cache = null;
			member_cache = null;
		}

		//
		// Performs the validation on a Method's modifiers (properties have
		// the same properties).
		//
		public bool MethodModifiersValid (MemberCore mc)
		{
			const int vao = (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE);
			const int va = (Modifiers.VIRTUAL | Modifiers.ABSTRACT);
			const int nv = (Modifiers.NEW | Modifiers.VIRTUAL);
			bool ok = true;
			int flags = mc.ModFlags;
			
			//
			// At most one of static, virtual or override
			//
			if ((flags & Modifiers.STATIC) != 0){
				if ((flags & vao) != 0){
					Report.Error (112, mc.Location, "A static member `{0}' cannot be marked as override, virtual or abstract",
						mc.GetSignatureForError ());
					ok = false;
				}
			}

			if (Kind == Kind.Struct){
				if ((flags & va) != 0){
					Modifiers.Error_InvalidModifier (mc.Location, "virtual or abstract");
					ok = false;
				}
			}

			if ((flags & Modifiers.OVERRIDE) != 0 && (flags & nv) != 0){
				Report.Error (113, mc.Location, "A member `{0}' marked as override cannot be marked as new or virtual",
					mc.GetSignatureForError ());
				ok = false;
			}

			//
			// If the declaration includes the abstract modifier, then the
			// declaration does not include static, virtual or extern
			//
			if ((flags & Modifiers.ABSTRACT) != 0){
				if ((flags & Modifiers.EXTERN) != 0){
					Report.Error (
						180, mc.Location, "`{0}' cannot be both extern and abstract", mc.GetSignatureForError ());
					ok = false;
				}

				if ((flags & Modifiers.SEALED) != 0) {
					Report.Error (502, mc.Location, "`{0}' cannot be both abstract and sealed", mc.GetSignatureForError ());
					ok = false;
				}

				if ((flags & Modifiers.VIRTUAL) != 0){
					Report.Error (503, mc.Location, "The abstract method `{0}' cannot be marked virtual", mc.GetSignatureForError ());
					ok = false;
				}

				if ((ModFlags & Modifiers.ABSTRACT) == 0){
					Report.Error (513, mc.Location, "`{0}' is abstract but it is contained in nonabstract class", mc.GetSignatureForError ());
					ok = false;
				}
			}

			if ((flags & Modifiers.PRIVATE) != 0){
				if ((flags & vao) != 0){
					Report.Error (621, mc.Location, "`{0}': virtual or abstract members cannot be private", mc.GetSignatureForError ());
					ok = false;
				}
			}

			if ((flags & Modifiers.SEALED) != 0){
				if ((flags & Modifiers.OVERRIDE) == 0){
					Report.Error (238, mc.Location, "`{0}' cannot be sealed because it is not an override", mc.GetSignatureForError ());
					ok = false;
				}
			}

			return ok;
		}

		public bool UserDefinedStaticConstructor {
			get {
				return default_static_constructor != null;
			}
		}

		public Constructor DefaultStaticConstructor {
			get { return default_static_constructor; }
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			VerifyClsName ();

			Type base_type = TypeBuilder.BaseType;
			if (base_type != null && !AttributeTester.IsClsCompliant (base_type)) {
				Report.Error (3009, Location, "`{0}': base type `{1}' is not CLS-compliant", GetSignatureForError (), TypeManager.CSharpName (base_type));
			}

			if (!Parent.IsClsComplianceRequired (ds)) {
				Report.Error (3018, Location, "`{0}' cannot be marked as CLS-compliant because it is a member of non CLS-compliant type `{1}'", 
					GetSignatureForError (), Parent.GetSignatureForError ());
			}
			return true;
		}


		/// <summary>
		/// Checks whether container name is CLS Compliant
		/// </summary>
		void VerifyClsName ()
		{
			Hashtable base_members = base_cache == null ? 
				new Hashtable () :
				base_cache.GetPublicMembers ();
			Hashtable this_members = new Hashtable ();

			foreach (DictionaryEntry entry in defined_names) {
				MemberCore mc = (MemberCore)entry.Value;
				if (!mc.IsClsComplianceRequired (mc.Parent))
					continue;

				string name = (string) entry.Key;
				string basename = name.Substring (name.LastIndexOf ('.') + 1);

				string lcase = basename.ToLower (System.Globalization.CultureInfo.InvariantCulture);
				object found = base_members [lcase];
				if (found == null) {
					found = this_members [lcase];
					if (found == null) {
						this_members.Add (lcase, mc);
						continue;
					}
				}

				if ((mc.ModFlags & Modifiers.OVERRIDE) != 0)
					continue;					

				if (found is MemberInfo) {
					if (basename == ((MemberInfo) found).Name)
						continue;
					Report.SymbolRelatedToPreviousError ((MemberInfo) found);
				} else {
					Report.SymbolRelatedToPreviousError ((MemberCore) found);
				}
				Report.Warning (3005, 1, mc.Location, "Identifier `{0}' differing only in case is not CLS-compliant", mc.GetSignatureForError ());
			}
		}


		/// <summary>
		///   Performs checks for an explicit interface implementation.  First it
		///   checks whether the `interface_type' is a base inteface implementation.
		///   Then it checks whether `name' exists in the interface type.
		/// </summary>
		public virtual bool VerifyImplements (MemberBase mb)
		{
			if (ifaces != null) {
				foreach (Type t in ifaces){
					if (t == mb.InterfaceType)
						return true;
				}
			}
			
			Report.Error (540, mb.Location, "`{0}': containing type does not implement interface `{1}'",
				mb.GetSignatureForError (), TypeManager.CSharpName (mb.InterfaceType));
			return false;
		}

		public virtual void Mark_HasEquals ()
		{
			Methods.HasEquals = true;
		}

		public virtual void Mark_HasGetHashCode ()
		{
			Methods.HasGetHashCode = true;
		}

		//
		// IMemberContainer
		//

		string IMemberContainer.Name {
			get {
				return Name;
			}
		}

		Type IMemberContainer.Type {
			get {
				return TypeBuilder;
			}
		}

		MemberCache IMemberContainer.MemberCache {
			get {
				return member_cache;
			}
		}

		bool IMemberContainer.IsInterface {
			get {
				return Kind == Kind.Interface;
			}
		}

		MemberList IMemberContainer.GetMembers (MemberTypes mt, BindingFlags bf)
		{
			BindingFlags new_bf = bf | BindingFlags.DeclaredOnly;

			if (GenericType != null)
				return TypeManager.FindMembers (GenericType, mt, new_bf,
								null, null);
			else
				return FindMembers (mt, new_bf, null, null);
		}

		//
		// Generates xml doc comments (if any), and if required,
		// handle warning report.
		//
		internal override void GenerateDocComment (DeclSpace ds)
		{
			DocUtil.GenerateTypeDocComment (this, ds);
		}

		public override string DocCommentHeader {
			get { return "T:"; }
		}

		public virtual MemberCache BaseCache {
			get {
				if (base_cache != null)
					return base_cache;
				if (TypeBuilder.BaseType != null)
					base_cache = TypeManager.LookupMemberCache (TypeBuilder.BaseType);
				if (TypeBuilder.IsInterface)
					base_cache = TypeManager.LookupBaseInterfacesCache (TypeBuilder);
				return base_cache;
			}
		}
		
	}

	public class PartialContainer : TypeContainer {

		public readonly Namespace Namespace;
		public readonly int OriginalModFlags;
		public readonly int AllowedModifiers;
		public readonly TypeAttributes DefaultTypeAttributes;
		public ListDictionary DeclarativeSecurity;

		static PartialContainer Create (NamespaceEntry ns, TypeContainer parent,
						MemberName member_name, int mod_flags, Kind kind)
		{

			if (!CheckModFlags (0, mod_flags, member_name))
				return null;

			PartialContainer pc = RootContext.Tree.GetDecl (member_name) as PartialContainer;
			if (pc != null) {
				if (pc.Kind != kind) {
					Report.Error (
						261, member_name.Location,
						"Partial declarations of `{0}' must be all classes, " +
						"all structs or all interfaces",
						member_name.GetTypeName ());
					return null;
				}

				if (!CheckModFlags (pc.OriginalModFlags, mod_flags, member_name))
					return null;
				pc.ModFlags |= (mod_flags & pc.AllowedModifiers);

				if (pc.IsGeneric) {
					if (pc.CountTypeParameters != member_name.CountTypeArguments) {
						Report.Error (
							264, member_name.Location,
							"Partial declarations of `{0}' must have the " +
							"same type parameter names in the same order",
							member_name.GetTypeName ());
						return null;
					}

					TypeParameterName[] pc_names = pc.MemberName.TypeArguments.GetDeclarations ();
					TypeParameterName[] names = member_name.TypeArguments.GetDeclarations ();

					for (int i = 0; i < pc.CountTypeParameters; i++) {
						if (pc_names [i].Name == names [i].Name)
							continue;

						Report.Error (
							264, member_name.Location,
							"Partial declarations of `{0}' must have the " +
							"same type parameter names in the same order",
							member_name.GetTypeName ());
						return null;
					}
				}

				return pc;
			}

			if (parent is ClassPart)
				parent = ((ClassPart) parent).PartialContainer;

			pc = new PartialContainer (ns.NS, parent, member_name, mod_flags, kind);

			if (kind == Kind.Interface) {
				if (!parent.AddInterface (pc))
					return null;
			} else if (kind == Kind.Class || kind == Kind.Struct) {
				if (!parent.AddClassOrStruct (pc))
					return null;
			} else {
				throw new InvalidOperationException ();
			}
			RootContext.Tree.RecordDecl (ns.NS, member_name, pc);
			// This is needed to define our type parameters; we define the constraints later.
			pc.SetParameterInfo (null);
			return pc;
		}

		static bool CheckModFlags (int flags_org, int flags, MemberName member_name)
		{
			// Check (abstract|static|sealed) sanity.
			int tmp = (flags_org | flags) & (Modifiers.ABSTRACT | Modifiers.SEALED | Modifiers.STATIC);
			if ((tmp & Modifiers.ABSTRACT) != 0) {
				if ((tmp & (Modifiers.STATIC | Modifiers.SEALED)) != 0) {
					Report.Error (
						418, member_name.Location, 
						"`{0}': an abstract class cannot be sealed or static", member_name.ToString ());
					return false;
				}
			} else if (tmp == (Modifiers.SEALED | Modifiers.STATIC)) {
				Report.Error (441, member_name.Location, "`{0}': a class cannot be both static and sealed", member_name.ToString ());
				return false;
			}

			if (flags_org == 0)
				return true;

			// Check conflicts.
			if (0 != ((flags_org ^ flags) & (0xFFFFFFFF ^ (Modifiers.SEALED | Modifiers.ABSTRACT)))) {
				Report.Error (
					262, member_name.Location, "Partial declarations of `{0}' " +
					"have conflicting accessibility modifiers",
					member_name.GetName ());
				return false;
			}
			return true;
		}

		public static ClassPart CreatePart (NamespaceEntry ns, TypeContainer parent,
						    MemberName name, int mod, Attributes attrs,
						    Kind kind, Location loc)
		{
			PartialContainer pc = Create (ns, parent, name, mod, kind);
			if (pc == null) {
				// An error occured; create a dummy container, but don't
				// register it.
				pc = new PartialContainer (ns.NS, parent, name, mod, kind);
			}

			ClassPart part = new ClassPart (ns, pc, parent, mod, attrs, kind);
			pc.AddPart (part);
			return part;
		}

		protected PartialContainer (Namespace ns, TypeContainer parent,
					    MemberName name, int mod, Kind kind)
			: base (null, parent, name, null, kind)
		{
			this.Namespace = ns;

			switch (kind) {
			case Kind.Class:
				AllowedModifiers = Class.AllowedModifiers;
				DefaultTypeAttributes = Class.DefaultTypeAttributes;
				break;

			case Kind.Struct:
				AllowedModifiers = Struct.AllowedModifiers;
				DefaultTypeAttributes = Struct.DefaultTypeAttributes;
				break;

			case Kind.Interface:
				AllowedModifiers = Interface.AllowedModifiers;
				DefaultTypeAttributes = Interface.DefaultTypeAttributes;
				break;

			default:
				throw new InvalidOperationException ();
			}

			int accmods;
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			// FIXME: remove this nasty fix for bug #77370 when
			// we get good AllowModifiersProp implementation.
			if ((mod & Modifiers.STATIC) != 0) {
				AllowedModifiers |= Modifiers.STATIC;
				AllowedModifiers &= ~ (Modifiers.ABSTRACT | Modifiers.SEALED);
			}

			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, Location);
			this.OriginalModFlags = mod;
		}

		public override void EmitType ()
		{
			base.EmitType ();

			if (DeclarativeSecurity != null) {
				foreach (DictionaryEntry de in DeclarativeSecurity) {
					TypeBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}
		}

		public override PendingImplementation GetPendingImplementations ()
		{
			return PendingImplementation.GetPendingImplementations (this);
		}

		public override bool MarkForDuplicationCheck ()
		{
			return true;
		}

		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}
	}

	public class ClassPart : TypeContainer, IMemberContainer {
		public readonly PartialContainer PartialContainer;
		public readonly bool IsPartial;

		Constraints[] constraints;

		public ClassPart (NamespaceEntry ns, PartialContainer pc, TypeContainer parent,
				  int mod, Attributes attrs, Kind kind)
			: base (ns, parent, pc.MemberName, attrs, kind)
		{
			this.PartialContainer = pc;
			this.IsPartial = true;

			int accmods;
			if (parent == null || parent == RootContext.Tree.Types)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (pc.AllowedModifiers, mod, accmods, pc.MemberName.Location);

			if (pc.IsGeneric)
				constraints = new Constraints [pc.CountCurrentTypeParameters];
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (PartialContainer.DeclarativeSecurity == null)
					PartialContainer.DeclarativeSecurity = new ListDictionary ();

				a.ExtractSecurityPermissionSet (PartialContainer.DeclarativeSecurity);
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}

		public override PendingImplementation GetPendingImplementations ()
		{
			return PartialContainer.Pending;
		}

		public override bool VerifyImplements (MemberBase mb)
		{
			return PartialContainer.VerifyImplements (mb);
		}

		public override void SetParameterInfo (ArrayList constraints_list)
		{
			if (constraints_list == null)
				return;

			TypeParameter[] current_params = PartialContainer.CurrentTypeParameters;
			for (int i = 0; i < constraints.Length; i++) {
				foreach (Constraints constraint in constraints_list) {
					if (constraint.TypeParameter == current_params [i].Name) {
						constraints [i] = constraint;
						break;
					}
				}
			}
		}

		public bool DefineTypeParameters ()
		{
			TypeParameter[] current_params = PartialContainer.CurrentTypeParameters;

			for (int i = 0; i < current_params.Length; i++) {
				Constraints new_constraints = constraints [i];
				if (new_constraints == null)
					continue;

				if (!current_params [i].UpdateConstraints (ec, new_constraints)) {
					Report.Error (265, Location, "Partial declarations of `{0}' have " +
						      "inconsistent constraints for type parameter `{1}'.",
						      MemberName.GetTypeName (), current_params [i].Name);
					return false;
				}
			}

			for (int i = 0; i < current_params.Length; i++) {
				if (!current_params [i].Resolve (this))
					return false;
			}

			foreach (TypeParameter type_param in PartialContainer.TypeParameters) {
				if (!type_param.DefineType (ec))
					return false;
			}

			return true;
		}

		public override void RegisterFieldForInitialization (FieldBase field)
		{
			PartialContainer.RegisterFieldForInitialization (field);
		}

		public override bool EmitFieldInitializers (EmitContext ec)
		{
			return PartialContainer.EmitFieldInitializers (ec);
		}

		public override Type FindNestedType (string name)
		{
			return PartialContainer.FindNestedType (name);
		}

		public override MemberCache BaseCache {
			get {
				return PartialContainer.BaseCache;
			}
		}

		public override TypeBuilder DefineType ()
		{
			throw new InternalErrorException ("Should not get here");
		}

		public override void Mark_HasEquals ()
		{
			PartialContainer.Mark_HasEquals ();
		}

		public override void Mark_HasGetHashCode ()
		{
			PartialContainer.Mark_HasGetHashCode ();
		}
	}

	public abstract class ClassOrStruct : TypeContainer {
		bool has_explicit_layout = false;
		ListDictionary declarative_security;

		public ClassOrStruct (NamespaceEntry ns, TypeContainer parent,
				      MemberName name, Attributes attrs, Kind kind)
			: base (ns, parent, name, attrs, kind)
		{
		}

		public override PendingImplementation GetPendingImplementations ()
		{
			return PendingImplementation.GetPendingImplementations (this);
		}

		public override bool HasExplicitLayout {
			get {
				return has_explicit_layout;
				}
			}

		public override void VerifyMembers ()
		{
			base.VerifyMembers ();

			if ((events != null) && (RootContext.WarningLevel >= 3)) {
				foreach (Event e in events){
					if ((e.caching_flags & Flags.IsAssigned) == 0)
						Report.Warning (67, 3, e.Location, "The event `{0}' is never used", e.GetSignatureForError ());
				}
			}
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();

				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Type == TypeManager.struct_layout_attribute_type && a.GetLayoutKindValue () == LayoutKind.Explicit)
				has_explicit_layout = true;

			base.ApplyAttributeBuilder (a, cb);
		}

		public override void Emit()
		{
			base.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					TypeBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}
		}
	}

	/// <summary>
	/// Class handles static classes declaration
	/// </summary>
	public sealed class StaticClass: Class {
		public StaticClass (NamespaceEntry ns, TypeContainer parent, MemberName name, int mod,
				    Attributes attrs)
			: base (ns, parent, name, mod, attrs)
		{
			if (RootContext.Version == LanguageVersion.ISO_1) {
				Report.FeatureIsNotStandardized (Location, "static classes");
			}
		}

		protected override int AllowedModifiersProp {
			get {
				return Modifiers.NEW | Modifiers.PUBLIC | Modifiers.PROTECTED | Modifiers.INTERNAL | Modifiers.PRIVATE |
					Modifiers.STATIC | Modifiers.UNSAFE;
			}
		}

		protected override void DefineContainerMembers (MemberCoreArrayList list)
		{
			if (list == null)
				return;

			foreach (MemberCore m in list) {
				if (m is Operator) {
					Report.Error (715, m.Location, "`{0}': Static classes cannot contain user-defined operators", m.GetSignatureForError ());
					continue;
				}

				if (m is Destructor) {
					Report.Error (711, m.Location, "`{0}': Static classes cannot contain destructor", GetSignatureForError ());
					continue;
				}

				if ((m.ModFlags & Modifiers.PROTECTED) != 0) {
					Report.Error (1057, m.Location, "`{0}': Static classes cannot contain protected members", m.GetSignatureForError ());
					continue;
				}

				if (m is Indexer) {
					Report.Error (720, m.Location, "`{0}': cannot declare indexers in a static class", m.GetSignatureForError ());
					continue;
				}

				if ((m.ModFlags & Modifiers.STATIC) != 0 || m is Enum || m is Delegate)
					continue;

				if (m is Constructor) {
					Report.Error (710, m.Location, "`{0}': Static classes cannot have instance constructors", GetSignatureForError ());
					continue;
				}

				Report.Error (708, m.Location, "`{0}': cannot declare instance members in a static class", m.GetSignatureForError ());
			}

			base.DefineContainerMembers (list);
		}

		public override TypeBuilder DefineType()
		{
			if ((ModFlags & (Modifiers.SEALED | Modifiers.STATIC)) == (Modifiers.SEALED | Modifiers.STATIC)) {
				Report.Error (441, Location, "`{0}': a class cannot be both static and sealed", GetSignatureForError ());
				return null;
			}

			TypeBuilder tb = base.DefineType ();
			if (tb == null)
				return null;

			if (ptype != TypeManager.object_type) {
				Report.Error (713, Location, "Static class `{0}' cannot derive from type `{1}'. Static classes must derive from object", GetSignatureForError (), TypeManager.CSharpName (ptype));
				return null;
			}

			if (ifaces != null) {
				foreach (Type t in ifaces)
					Report.SymbolRelatedToPreviousError (t);
				Report.Error (714, Location, "`{0}': static classes cannot implement interfaces", GetSignatureForError ());
			}
			return tb;
		}

		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | TypeAttributes.Abstract | TypeAttributes.Sealed;
			}
		}
	}

	public class Class : ClassOrStruct {
		// TODO: remove this and use only AllowedModifiersProp to fix partial classes bugs
		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.ABSTRACT |
			Modifiers.SEALED |
			Modifiers.UNSAFE;

		public Class (NamespaceEntry ns, TypeContainer parent, MemberName name, int mod,
			      Attributes attrs)
			: base (ns, parent, name, attrs, Kind.Class)
		{
			this.ModFlags = mod;
		}

		virtual protected int AllowedModifiersProp {
			get {
				return AllowedModifiers;
			}
		}

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.attribute_usage_type) {
				if (ptype != TypeManager.attribute_type &&
				    !ptype.IsSubclassOf (TypeManager.attribute_type) &&
				    TypeBuilder.FullName != "System.Attribute") {
					Report.Error (641, a.Location, "Attribute `{0}' is only valid on classes derived from System.Attribute", a.GetSignatureForError ());
				}
			}

			if (a.Type == TypeManager.conditional_attribute_type &&
				!(ptype == TypeManager.attribute_type || ptype.IsSubclassOf (TypeManager.attribute_type))) {
				Report.Error (1689, a.Location, "Attribute 'System.Diagnostics.ConditionalAttribute' is only valid on methods or attribute classes");
				return;
			}

			if (AttributeTester.IsAttributeExcluded (a.Type))
				return;

			base.ApplyAttributeBuilder (a, cb);
		}

		public const TypeAttributes DefaultTypeAttributes =
			TypeAttributes.AutoLayout | TypeAttributes.Class;

		public override TypeBuilder DefineType()
		{
			if ((ModFlags & Modifiers.ABSTRACT) == Modifiers.ABSTRACT && (ModFlags & (Modifiers.SEALED | Modifiers.STATIC)) != 0) {
				Report.Error (418, Location, "`{0}': an abstract class cannot be sealed or static", GetSignatureForError ());
				return null;
			}

			int accmods = Parent.Parent == null ? Modifiers.INTERNAL : Modifiers.PRIVATE;
			ModFlags = Modifiers.Check (AllowedModifiersProp, ModFlags, accmods, Location);

			return base.DefineType ();
		}

		/// Search for at least one defined condition in ConditionalAttribute of attribute class
		/// Valid only for attribute classes.
		public bool IsExcluded ()
		{
			if ((caching_flags & Flags.Excluded_Undetected) == 0)
				return (caching_flags & Flags.Excluded) != 0;

			caching_flags &= ~Flags.Excluded_Undetected;

			if (OptAttributes == null)
				return false;

			Attribute[] attrs = OptAttributes.SearchMulti (TypeManager.conditional_attribute_type, ec);

			if (attrs == null)
				return false;

			foreach (Attribute a in attrs) {
				string condition = a.GetConditionalAttributeValue (Parent.EmitContext);
				if (RootContext.AllDefines.Contains (condition))
					return false;
			}

			caching_flags |= Flags.Excluded;
			return true;
		}

		//
		// FIXME: How do we deal with the user specifying a different
		// layout?
		//
		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}
	}

	public class Struct : ClassOrStruct {
		// <summary>
		//   Modifiers allowed in a struct declaration
		// </summary>
		public const int AllowedModifiers =
			Modifiers.NEW       |
			Modifiers.PUBLIC    |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL  |
			Modifiers.UNSAFE    |
			Modifiers.PRIVATE;

		public Struct (NamespaceEntry ns, TypeContainer parent, MemberName name,
			       int mod, Attributes attrs)
			: base (ns, parent, name, attrs, Kind.Struct)
		{
			int accmods;
			
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, Location);

			this.ModFlags |= Modifiers.SEALED;
		}

		public const TypeAttributes DefaultTypeAttributes =
			TypeAttributes.SequentialLayout |
			TypeAttributes.Sealed |
			TypeAttributes.BeforeFieldInit;

		//
		// FIXME: Allow the user to specify a different set of attributes
		// in some cases (Sealed for example is mandatory for a class,
		// but what SequentialLayout can be changed
		//
		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}
	}

	/// <summary>
	///   Interfaces
	/// </summary>
	public class Interface : TypeContainer, IMemberContainer {

		/// <summary>
		///   Modifiers allowed in a class declaration
		/// </summary>
		public const int AllowedModifiers =
			Modifiers.NEW       |
			Modifiers.PUBLIC    |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL  |
		 	Modifiers.UNSAFE    |
			Modifiers.PRIVATE;

		public Interface (NamespaceEntry ns, TypeContainer parent, MemberName name, int mod,
				  Attributes attrs)
			: base (ns, parent, name, attrs, Kind.Interface)
		{
			int accmods;

			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, name.Location);
		}

		public override PendingImplementation GetPendingImplementations ()
		{
			return null;
		}

		public const TypeAttributes DefaultTypeAttributes =
					TypeAttributes.AutoLayout |
					TypeAttributes.Abstract |
					TypeAttributes.Interface;

		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			if (ifaces != null) {
				foreach (Type t in ifaces) {
					if (AttributeTester.IsClsCompliant (t))
						continue;

					Report.SymbolRelatedToPreviousError (t);
					Report.Warning (3027, 1, Location, "`{0}' is not CLS-compliant because base interface `{1}' is not CLS-compliant",
						GetSignatureForError (), TypeManager.CSharpName (t));
				}
			}

			return true;
		}
	}

	public abstract class MethodCore : MemberBase {
		public readonly Parameters Parameters;
		protected ToplevelBlock block;
		
		// Whether this is an operator method.
		public Operator IsOperator;

		//
		// The method we're overriding if this is an override method.
		//
		protected MethodInfo base_method = null;

		static string[] attribute_targets = new string [] { "method", "return" };

		public MethodCore (TypeContainer parent, GenericMethod generic,
				   Expression type, int mod, int allowed_mod, bool is_iface,
				   MemberName name, Attributes attrs, Parameters parameters)
			: base (parent, generic, type, mod, allowed_mod, Modifiers.PRIVATE,
				name, attrs)
		{
			Parameters = parameters;
			IsInterface = is_iface;
		}
		
		//
		//  Returns the System.Type array for the parameters of this method
		//
		public Type [] ParameterTypes {
			get {
				return Parameters.Types;
			}
		}

		public Parameters ParameterInfo
		{
			get {
				return Parameters;
			}
		}

		public override EmitContext EmitContext {
			get { return ds.EmitContext; }
		}
		
		public ToplevelBlock Block {
			get {
				return block;
			}

			set {
				block = value;
			}
		}

		public void SetYields ()
		{
			ModFlags |= Modifiers.METHOD_YIELDS;
		}

		protected override bool CheckBase ()
		{
			if (!base.CheckBase ())
				return false;
			
			// Check whether arguments were correct.
			if (!DoDefineParameters ())
				return false;

			if ((caching_flags & Flags.TestMethodDuplication) != 0 && !CheckForDuplications ())
				return false;

			if (IsExplicitImpl)
				return true;

			// Is null for System.Object while compiling corlib and base interfaces
			if (Parent.BaseCache == null) {
				if ((RootContext.WarningLevel >= 4) && ((ModFlags & Modifiers.NEW) != 0)) {
					Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
				}
				return true;
			}

			Type base_ret_type = null;
			base_method = FindOutBaseMethod (Parent, ref base_ret_type);

			// method is override
			if (base_method != null) {

				if (!CheckMethodAgainstBase ())
					return false;

				if ((ModFlags & Modifiers.NEW) == 0) {
					if (!TypeManager.IsEqual (MemberType, TypeManager.TypeToCoreType (base_ret_type))) {
						Report.SymbolRelatedToPreviousError (base_method);
						if (this is PropertyBase) {
							Report.Error (1715, Location, "`{0}': type must be `{1}' to match overridden member `{2}'", 
								GetSignatureForError (), TypeManager.CSharpName (base_ret_type), TypeManager.CSharpSignature (base_method));
						}
						else {
							Report.Error (508, Location, "`{0}': return type must be `{1}' to match overridden member `{2}'",
								GetSignatureForError (), TypeManager.CSharpName (base_ret_type), TypeManager.CSharpSignature (base_method));
						}
						return false;
					}
				} else {
					if (base_method.IsAbstract && !IsInterface) {
						Report.SymbolRelatedToPreviousError (base_method);
						Report.Error (533, Location, "`{0}' hides inherited abstract member `{1}'",
							GetSignatureForError (), TypeManager.CSharpSignature (base_method));
						return false;
					}
				}

				if (base_method.IsSpecialName && !(this is PropertyBase)) {
					Report.Error (115, Location, "`{0}': no suitable method found to override", GetSignatureForError ());
					return false;
				}

				if (Name == "Equals" && Parameters.Count == 1 && ParameterTypes [0] == TypeManager.object_type)
					Parent.Mark_HasEquals ();
				else if (Name == "GetHashCode" && Parameters.Empty)
					Parent.Mark_HasGetHashCode ();

				if ((ModFlags & Modifiers.OVERRIDE) != 0) {
					ObsoleteAttribute oa = AttributeTester.GetMethodObsoleteAttribute (base_method);
					if (oa != null) {
						EmitContext ec = new EmitContext (this.Parent, this.Parent, Location, null, null, ModFlags, false);
						if (OptAttributes == null || !OptAttributes.Contains (TypeManager.obsolete_attribute_type, ec)) {
							Report.SymbolRelatedToPreviousError (base_method);
							Report.Warning (672, 1, Location, "Member `{0}' overrides obsolete member `{1}'. Add the Obsolete attribute to `{0}'",
								GetSignatureForError (), TypeManager.CSharpSignature (base_method) );
						}
					}
				}
				return true;
			}

			MemberInfo conflict_symbol = Parent.FindBaseMemberWithSameName (Name, !(this is Property));
			if ((ModFlags & Modifiers.OVERRIDE) != 0) {
				if (conflict_symbol != null) {
					Report.SymbolRelatedToPreviousError (conflict_symbol);
					if (this is PropertyBase)
						Report.Error (544, Location, "`{0}': cannot override because `{1}' is not a property", GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
					else
						Report.Error (505, Location, "`{0}': cannot override because `{1}' is not a method", GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
				} else
					Report.Error (115, Location, "`{0}': no suitable method found to override", GetSignatureForError ());
				return false;
			}

			if (conflict_symbol == null) {
				if ((RootContext.WarningLevel >= 4) && ((ModFlags & Modifiers.NEW) != 0)) {
					Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
				}
				return true;
			}

			if ((ModFlags & Modifiers.NEW) == 0) {
				if (this is Method && conflict_symbol is MethodBase)
					return true;

				Report.SymbolRelatedToPreviousError (conflict_symbol);
				Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
					GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
			}

			return true;
		}

		//
		// Performs various checks on the MethodInfo `mb' regarding the modifier flags
		// that have been defined.
		//
		// `name' is the user visible name for reporting errors (this is used to
		// provide the right name regarding method names and properties)
		//
		bool CheckMethodAgainstBase ()
		{
			bool ok = true;

			if ((ModFlags & Modifiers.OVERRIDE) != 0){
				if (!(base_method.IsAbstract || base_method.IsVirtual)){
					Report.Error (506, Location,
						"`{0}': cannot override inherited member `{1}' because it is not marked virtual, abstract or override",
						 GetSignatureForError (), TypeManager.CSharpSignature (base_method));
					ok = false;
				}
				
				// Now we check that the overriden method is not final
				
				if (base_method.IsFinal) {
					Report.SymbolRelatedToPreviousError (base_method);
					Report.Error (239, Location, "`{0}': cannot override inherited member `{1}' because it is sealed",
							      GetSignatureForError (), TypeManager.CSharpSignature (base_method));
					ok = false;
				}
				//
				// Check that the permissions are not being changed
				//
				MethodAttributes thisp = flags & MethodAttributes.MemberAccessMask;
				MethodAttributes base_classp = base_method.Attributes & MethodAttributes.MemberAccessMask;

				if (!CheckAccessModifiers (thisp, base_classp, base_method)) {
					Error_CannotChangeAccessModifiers (base_method, base_classp, null);
					ok = false;
				}
			}

			if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE)) == 0 && Name != "Finalize") {
				ModFlags |= Modifiers.NEW;
				Report.SymbolRelatedToPreviousError (base_method);
				if (!IsInterface && (base_method.IsVirtual || base_method.IsAbstract)) {
					Report.Warning (114, 2, Location, "`{0}' hides inherited member `{1}'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword",
						GetSignatureForError (), TypeManager.CSharpSignature (base_method));
				} else {
					Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
						GetSignatureForError (), TypeManager.CSharpSignature (base_method));
				}
			}

			return ok;
		}
		
		protected bool CheckAccessModifiers (MethodAttributes thisp, MethodAttributes base_classp, MethodInfo base_method)
		{
			if ((base_classp & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem){
				//
				// when overriding protected internal, the method can be declared
				// protected internal only within the same assembly
				//

				if ((thisp & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem){
					if (Parent.TypeBuilder.Assembly != base_method.DeclaringType.Assembly){
						//
						// assemblies differ - report an error
						//
						
						return false;
					} else if (thisp != base_classp) {
						//
						// same assembly, but other attributes differ - report an error
						//
						
						return false;
					};
				} else if ((thisp & MethodAttributes.Family) != MethodAttributes.Family) {
					//
					// if it's not "protected internal", it must be "protected"
					//

					return false;
				} else if (Parent.TypeBuilder.Assembly == base_method.DeclaringType.Assembly) {
					//
					// protected within the same assembly - an error
					//
					return false;
				} else if ((thisp & ~(MethodAttributes.Family | MethodAttributes.FamORAssem)) != 
					   (base_classp & ~(MethodAttributes.Family | MethodAttributes.FamORAssem))) {
					//
					// protected ok, but other attributes differ - report an error
					//
					return false;
				}
				return true;
			} else {
				return (thisp == base_classp);
			}
		}

		public bool CheckAbstractAndExtern (bool has_block)
		{
			if (Parent.Kind == Kind.Interface)
				return true;

			if (has_block) {
				if ((ModFlags & Modifiers.EXTERN) != 0) {
					Report.Error (179, Location, "`{0}' cannot declare a body because it is marked extern",
						GetSignatureForError ());
					return false;
				}

				if ((ModFlags & Modifiers.ABSTRACT) != 0) {
					Report.Error (500, Location, "`{0}' cannot declare a body because it is marked abstract",
						GetSignatureForError ());
					return false;
				}
			} else {
				if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) == 0) {
					Report.Error (501, Location, "`{0}' must declare a body because it is not marked abstract or extern",
						GetSignatureForError ());
					return false;
				}
			}

			return true;
		}

		protected void Error_CannotChangeAccessModifiers (MemberInfo base_method, MethodAttributes ma, string suffix)
		{
			Report.SymbolRelatedToPreviousError (base_method);
			string base_name = TypeManager.GetFullNameSignature (base_method);
			string this_name = GetSignatureForError ();
			if (suffix != null) {
				base_name += suffix;
				this_name += suffix;
			}

			Report.Error (507, Location, "`{0}': cannot change access modifiers when overriding `{1}' inherited member `{2}'",
				this_name, Modifiers.GetDescription (ma), base_name);
		}

		protected static string Error722 {
			get {
				return "`{0}': static types cannot be used as return types";
			}
		}

		/// <summary>
		/// For custom member duplication search in a container
		/// </summary>
		protected abstract bool CheckForDuplications ();

		/// <summary>
		/// Gets base method and its return type
		/// </summary>
		protected abstract MethodInfo FindOutBaseMethod (TypeContainer container, ref Type base_ret_type);

		protected bool DoDefineParameters ()
		{
			EmitContext ec = ds.EmitContext;
			if (ec == null)
				throw new InternalErrorException ("DoDefineParameters invoked too early");

			bool old_unsafe = ec.InUnsafe;
			ec.InUnsafe = InUnsafe;
			ec.ResolvingGenericMethod = GenericMethod != null;

			bool old_obsolete = ec.TestObsoleteMethodUsage;
			if (GetObsoleteAttribute () != null || Parent.GetObsoleteAttribute () != null)
				ec.TestObsoleteMethodUsage = false;

			// Check if arguments were correct
			if (!Parameters.Resolve (ec))
				return false;

			ec.ResolvingGenericMethod = false;
			ec.InUnsafe = old_unsafe;
			ec.TestObsoleteMethodUsage = old_obsolete;

			return CheckParameters (ParameterTypes);
		}

		bool CheckParameters (Type [] parameters)
		{
			bool error = false;

			foreach (Type partype in parameters){
				if (partype == TypeManager.void_type) {
					Report.Error (
						1547, Location, "Keyword 'void' cannot " +
						"be used in this context");
					return false;
				}

				if (partype.IsPointer){
					if (!UnsafeOK (ds))
						error = true;
					if (!TypeManager.VerifyUnManaged (TypeManager.GetElementType (partype), Location))
						error = true;
				}

				if (ds.AsAccessible (partype, ModFlags))
					continue;

				if (this is Indexer)
					Report.Error (55, Location,
						"Inconsistent accessibility: parameter type `" +
						TypeManager.CSharpName (partype) + "' is less " +
						"accessible than indexer `" + GetSignatureForError () + "'");
				else if ((this is Method) && ((Method) this).IsOperator != null)
					Report.Error (57, Location,
						"Inconsistent accessibility: parameter type `" +
						TypeManager.CSharpName (partype) + "' is less " +
						"accessible than operator `" + GetSignatureForError () + "'");
				else
					Report.Error (51, Location,
						"Inconsistent accessibility: parameter type `" +
						TypeManager.CSharpName (partype) + "' is less " +
						"accessible than method `" + GetSignatureForError () + "'");
				error = true;
			}

			return !error;
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds)) {
				if ((ModFlags & Modifiers.ABSTRACT) != 0 && IsExposedFromAssembly (ds) && ds.IsClsComplianceRequired (ds)) {
					Report.Error (3011, Location, "`{0}': only CLS-compliant members can be abstract", GetSignatureForError ());
				}
				return false;
			}

			if (Parameters.HasArglist) {
				Report.Error (3000, Location, "Methods with variable arguments are not CLS-compliant");
			}

			if (!AttributeTester.IsClsCompliant (MemberType)) {
				if (this is PropertyBase)
					Report.Error (3003, Location, "Type of `{0}' is not CLS-compliant",
						      GetSignatureForError ());
				else
					Report.Error (3002, Location, "Return type of `{0}' is not CLS-compliant",
						      GetSignatureForError ());
			}

			Parameters.VerifyClsCompliance ();

			return true;
		}

		protected bool IsDuplicateImplementation (MethodCore method)
		{
			if (method == this || !(method.MemberName.Equals (MemberName)))
				return false;

			Type[] param_types = method.ParameterTypes;
			// This never happen. Rewrite this as Equal
			if (param_types == null && ParameterTypes == null)
				return true;
			if (param_types == null || ParameterTypes == null)
				return false;

			if (param_types.Length != ParameterTypes.Length)
				return false;

			if (method.Parameters.HasArglist != Parameters.HasArglist)
				return false;
			
			bool equal = true;

			for (int i = 0; i < param_types.Length; i++) {
				if (param_types [i] != ParameterTypes [i])
					equal = false;
			}

			if (IsExplicitImpl && (method.InterfaceType != InterfaceType))
				equal = false;

			// TODO: make operator compatible with MethodCore to avoid this
			if (this is Operator && method is Operator) {
				if (MemberType != method.MemberType)
					equal = false;
			}

			if (equal) {
				//
				// Try to report 663: method only differs on out/ref
				//
				Parameters info = ParameterInfo;
				Parameters other_info = method.ParameterInfo;
				for (int i = 0; i < info.Count; i++){
					try {
					if (info.ParameterModifier (i) != other_info.ParameterModifier (i)){
						Report.SymbolRelatedToPreviousError (method);
						Report.Error (663, Location, "`{0}': Methods cannot differ only on their use of ref and out on a parameters",
							      GetSignatureForError ());
						return false;
					}} catch {
						Console.WriteLine ("Method is: {0} {1}", method.Location, method);
						Console.WriteLine ("this is: {0} {1}", Location, this);
					}
				}

				Report.SymbolRelatedToPreviousError (method);
				if (this is Operator && method is Operator)
					Report.Error (557, Location, "Duplicate user-defined conversion in type `{0}'", Parent.Name);
				else
					Report.Error (111, Location, TypeContainer.Error111, GetSignatureForError ());

				return true;
			}

			return false;
		}

		public override bool IsUsed {
			get { return IsExplicitImpl || base.IsUsed; }
		}

		//
		// Returns a string that represents the signature for this 
		// member which should be used in XML documentation.
		//
		public override string GetDocCommentName (DeclSpace ds)
		{
			return DocUtil.GetMethodDocCommentName (this, ds);
		}

		//
		// Raised (and passed an XmlElement that contains the comment)
		// when GenerateDocComment is writing documentation expectedly.
		//
		// FIXME: with a few effort, it could be done with XmlReader,
		// that means removal of DOM use.
		//
		internal override void OnGenerateDocComment (DeclSpace ds, XmlElement el)
		{
			DocUtil.OnMethodGenerateDocComment (this, ds, el);
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "M:"; }
		}

	}

	public class SourceMethod : ISourceMethod
	{
		TypeContainer container;
		MethodBase builder;

		protected SourceMethod (TypeContainer container, MethodBase builder,
					ISourceFile file, Location start, Location end)
		{
			this.container = container;
			this.builder = builder;
			
			CodeGen.SymbolWriter.OpenMethod (
				file, this, start.Row, start.Column, end.Row, start.Column);
		}

		public string Name {
			get { return builder.Name; }
		}

		public int NamespaceID {
			get { return container.NamespaceEntry.SymbolFileID; }
		}

		public int Token {
			get {
				if (builder is MethodBuilder)
					return ((MethodBuilder) builder).GetToken ().Token;
				else if (builder is ConstructorBuilder)
					return ((ConstructorBuilder) builder).GetToken ().Token;
				else
					throw new NotSupportedException ();
			}
		}

		public void CloseMethod ()
		{
			if (CodeGen.SymbolWriter != null)
				CodeGen.SymbolWriter.CloseMethod ();
		}

		public static SourceMethod Create (TypeContainer parent,
						   MethodBase builder, Block block)
		{
			if (CodeGen.SymbolWriter == null)
				return null;
			if (block == null)
				return null;

			Location start_loc = block.StartLocation;
			if (start_loc.IsNull)
				return null;

			Location end_loc = block.EndLocation;
			if (end_loc.IsNull)
				return null;

			ISourceFile file = start_loc.SourceFile;
			if (file == null)
				return null;

			return new SourceMethod (
				parent, builder, file, start_loc, end_loc);
		}
	}

	public class Method : MethodCore, IIteratorContainer, IMethodData {
		public MethodBuilder MethodBuilder;
		public MethodData MethodData;
		ReturnParameter return_attributes;
		ListDictionary declarative_security;

		/// <summary>
		///   Modifiers allowed in a class declaration
		/// </summary>
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.ABSTRACT |
		        Modifiers.UNSAFE |
			Modifiers.METHOD_YIELDS | 
			Modifiers.EXTERN;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW | Modifiers.UNSAFE;

		//
		// return_type can be "null" for VOID values.
		//
		public Method (TypeContainer parent, GenericMethod generic,
			       Expression return_type, int mod, bool is_iface,
			       MemberName name, Parameters parameters, Attributes attrs)
			: base (parent, generic, return_type, mod,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				is_iface, name, attrs, parameters)
		{
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Method;
			}
		}
		
		public override string GetSignatureForError()
		{
			if (IsOperator != null)
				return IsOperator.GetSignatureForError ();

			return base.GetSignatureForError () + Parameters.GetSignatureForError ();
		}

                void DuplicateEntryPoint (MethodInfo b, Location location)
                {
                        Report.Error (
                                17, location,
                                "Program `" + CodeGen.FileName +
                                "' has more than one entry point defined: `" +
                                TypeManager.CSharpSignature(b) + "'");
                }

                bool IsEntryPoint (MethodBuilder b, Parameters pinfo)
                {
                        if (b.ReturnType != TypeManager.void_type &&
                            b.ReturnType != TypeManager.int32_type)
                                return false;

                        if (pinfo.Count == 0)
                                return true;

                        if (pinfo.Count > 1)
                                return false;

                        Type t = pinfo.ParameterType(0);
                        if (t.IsArray &&
                            (t.GetArrayRank() == 1) &&
                            (TypeManager.GetElementType(t) == TypeManager.string_type) &&
                            (pinfo.ParameterModifier(0) == Parameter.Modifier.NONE))
                                return true;
                        else
                                return false;
                }

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb);
				return;
			}

			if (a.Type == TypeManager.methodimpl_attr_type &&
				(a.GetMethodImplOptions () & MethodImplOptions.InternalCall) != 0) {
				MethodBuilder.SetImplementationFlags (MethodImplAttributes.InternalCall | MethodImplAttributes.Runtime);
			}

			if (a.Type == TypeManager.dllimport_type) {
				const int extern_static = Modifiers.EXTERN | Modifiers.STATIC;
				if ((ModFlags & extern_static) != extern_static) {
					Report.Error (601, a.Location, "The DllImport attribute must be specified on a method marked `static' and `extern'");
				}

				return;
			}

			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Type == TypeManager.conditional_attribute_type) {
				if (IsOperator != null || IsExplicitImpl) {
					Report.Error (577, Location, "Conditional not valid on `{0}' because it is a constructor, destructor, operator or explicit interface implementation",
						GetSignatureForError ());
					return;
				}

				if (ReturnType != TypeManager.void_type) {
					Report.Error (578, Location, "Conditional not valid on `{0}' because its return type is not void", GetSignatureForError ());
					return;
				}

				if ((ModFlags & Modifiers.OVERRIDE) != 0) {
					Report.Error (243, Location, "Conditional not valid on `{0}' because it is an override method", GetSignatureForError ());
					return;
				}

				if (IsInterface) {
					Report.Error (582, Location, "Conditional not valid on interface members");
					return;
				}

				if (MethodData.implementing != null) {
					Report.Error (629, Location, "Conditional member `{0}' cannot implement interface member `{1}'",
						GetSignatureForError (), TypeManager.CSharpSignature (MethodData.implementing));
					return;
				}

				for (int i = 0; i < ParameterInfo.Count; ++i) {
					if ((ParameterInfo.ParameterModifier (i) & Parameter.Modifier.OUTMASK) != 0) {
						Report.Error (685, Location, "Conditional method `{0}' cannot have an out parameter", GetSignatureForError ());
						return;
					}
				}
			}

			MethodBuilder.SetCustomAttribute (cb);
		}

  		protected override bool CheckForDuplications ()
		{
			ArrayList ar = Parent.Methods;
  			if (ar != null) {
  				int arLen = ar.Count;
   					
  				for (int i = 0; i < arLen; i++) {
  					Method m = (Method) ar [i];
  					if (IsDuplicateImplementation (m))
  						return false;
   				}
  			}

			ar = Parent.Properties;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					PropertyBase pb = (PropertyBase) ar [i];
					if (pb.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			ar = Parent.Indexers;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					PropertyBase pb = (PropertyBase) ar [i];
					if (pb.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			ar = Parent.Events;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					Event ev = (Event) ar [i];
					if (ev.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			return true;
		}

		//
		// Creates the type
		//
		public override bool Define ()
		{
			if (!DoDefineBase ())
				return false;

			MethodBuilder mb = null;
			if (GenericMethod != null) {
				string method_name = MemberName.Name;

				if (IsExplicitImpl) {
					method_name = TypeManager.CSharpName (InterfaceType) +
						'.' + method_name;
				}

				mb = Parent.TypeBuilder.DefineGenericMethod (method_name, flags);
				if (!GenericMethod.Define (mb))
					return false;
			}

			if (!DoDefine ())
				return false;

			if (!CheckAbstractAndExtern (block != null))
				return false;

			if (RootContext.StdLib && (ReturnType == TypeManager.arg_iterator_type || ReturnType == TypeManager.typed_reference_type)) {
				Error1599 (Location, ReturnType);
				return false;
			}

			if (!CheckBase ())
				return false;

			if (IsOperator != null)
				flags |= MethodAttributes.SpecialName | MethodAttributes.HideBySig;

			MethodData = new MethodData (this, ModFlags, flags, this, mb, GenericMethod, base_method);

			if (!MethodData.Define (Parent))
				return false;

			if (ReturnType == TypeManager.void_type && ParameterTypes.Length == 0 && 
				Name == "Finalize" && !(this is Destructor)) {
				Report.Warning (465, 1, Location, "Introducing a 'Finalize' method can interfere with destructor invocation. Did you intend to declare a destructor?");
			}

			//
			// Setup iterator if we are one
			//
			if ((ModFlags & Modifiers.METHOD_YIELDS) != 0){
				Iterator iterator = new Iterator (
					this, Parent, GenericMethod, ModFlags);

				if (!iterator.DefineIterator ())
					return false;
			}

			MethodBuilder = MethodData.MethodBuilder;

			//
			// This is used to track the Entry Point,
			//
			if (Name == "Main" &&
			    ((ModFlags & Modifiers.STATIC) != 0) && RootContext.NeedsEntryPoint && 
			    (RootContext.MainClass == null ||
			     RootContext.MainClass == Parent.TypeBuilder.FullName)){
                                if (IsEntryPoint (MethodBuilder, ParameterInfo)) {
                                        IMethodData md = TypeManager.GetMethod (MethodBuilder);
                                        md.SetMemberIsUsed ();

                                        if (RootContext.EntryPoint == null) {
						if (Parent.IsGeneric){
							Report.Error (-201, Location,
								      "Entry point can not be defined in a generic class");
						}
						
                                                RootContext.EntryPoint = MethodBuilder;
                                                RootContext.EntryPointLocation = Location;
                                        } else {
                                                DuplicateEntryPoint (RootContext.EntryPoint, RootContext.EntryPointLocation);
                                                DuplicateEntryPoint (MethodBuilder, Location);
                                        }
                                } else {
					if (RootContext.WarningLevel >= 4)
						Report.Warning (28, 4, Location, "`{0}' has the wrong signature to be an entry point", TypeManager.CSharpSignature(MethodBuilder));
				}
			}

			if (MemberType.IsAbstract && MemberType.IsSealed) {
				Report.Error (722, Location, Error722, TypeManager.CSharpName (MemberType));
				return false;
			}

			return true;
		}

		//
		// Emits the code
		// 
		public override void Emit ()
		{
			MethodData.Emit (Parent, this);
			base.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					MethodBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}

			Block = null;
			MethodData = null;
		}

		public static void Error1599 (Location loc, Type t)
		{
			Report.Error (1599, loc, "Method or delegate cannot return type `{0}'", TypeManager.CSharpName (t));
		}

		protected override MethodInfo FindOutBaseMethod (TypeContainer container, ref Type base_ret_type)
		{
			MethodInfo mi = (MethodInfo) container.BaseCache.FindMemberToOverride (
				container.TypeBuilder, Name, ParameterTypes, GenericMethod, false);

			if (mi == null)
				return null;

			base_ret_type = mi.ReturnType;
			return mi;
		}
	
		public override bool MarkForDuplicationCheck ()
		{
			caching_flags |= Flags.TestMethodDuplication;
			return true;
		}

		protected override bool VerifyClsCompliance(DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			if (ParameterInfo.Count > 0) {
				ArrayList al = (ArrayList)ds.MemberCache.Members [Name];
				if (al.Count > 1)
					ds.MemberCache.VerifyClsParameterConflict (al, this, MethodBuilder);
			}

			return true;
		}

		#region IMethodData Members

		public CallingConventions CallingConventions {
			get {
				CallingConventions cc = Parameters.CallingConvention;
				if (Parameters.HasArglist)
					block.HasVarargs = true;

				if (!IsInterface)
					if ((ModFlags & Modifiers.STATIC) == 0)
						cc |= CallingConventions.HasThis;

				// FIXME: How is `ExplicitThis' used in C#?
			
				return cc;
			}
		}

		public Type ReturnType {
			get {
				return MemberType;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		public new Location Location {
			get {
				return base.Location;
			}
		}

		protected override bool CheckBase() {
			if (!base.CheckBase ())
				return false;

			// TODO: Destructor should derive from MethodCore
			if (base_method != null && (ModFlags & Modifiers.OVERRIDE) != 0 && Name == "Finalize" &&
				base_method.DeclaringType == TypeManager.object_type && !(this is Destructor)) {
				Report.Error (249, Location, "Do not override object.Finalize. Instead, provide a destructor");
				return false;
			}

			return true;
		}

		public EmitContext CreateEmitContext (TypeContainer tc, ILGenerator ig)
		{
			EmitContext ec = new EmitContext (
				tc, ds, Location, ig, ReturnType, ModFlags, false);

			Iterator iterator = tc as Iterator;
			if (iterator != null)
				ec.CurrentAnonymousMethod = iterator.Host;

			return ec;
		}

		/// <summary>
		/// Returns true if method has conditional attribute and the conditions is not defined (method is excluded).
		/// </summary>
		public bool IsExcluded (EmitContext ec)
		{
			if ((caching_flags & Flags.Excluded_Undetected) == 0)
				return (caching_flags & Flags.Excluded) != 0;

			caching_flags &= ~Flags.Excluded_Undetected;

			if (base_method == null) {
				if (OptAttributes == null)
					return false;

				Attribute[] attrs = OptAttributes.SearchMulti (TypeManager.conditional_attribute_type, ec);

				if (attrs == null)
					return false;

				foreach (Attribute a in attrs) {
					string condition = a.GetConditionalAttributeValue (Parent.EmitContext);
					if (RootContext.AllDefines.Contains (condition))
						return false;
				}

				caching_flags |= Flags.Excluded;
				return true;
			}

			IMethodData md = TypeManager.GetMethod (base_method);
			if (md == null) {
				if (AttributeTester.IsConditionalMethodExcluded (base_method)) {
					caching_flags |= Flags.Excluded;
					return true;
				}
				return false;
			}

			if (md.IsExcluded (ec)) {
				caching_flags |= Flags.Excluded;
				return true;
			}
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return GenericMethod;
			}
		}

		#endregion
	}

	public abstract class ConstructorInitializer {
		ArrayList argument_list;
		protected ConstructorInfo base_constructor;
		Location loc;
		
		public ConstructorInitializer (ArrayList argument_list, Location loc)
		{
			this.argument_list = argument_list;
			this.loc = loc;
		}

		public ArrayList Arguments {
			get {
				return argument_list;
			}
		}

		public bool Resolve (ConstructorBuilder caller_builder, Block block, EmitContext ec)
		{
			Expression base_constructor_group;
			Type t;
			bool error = false;

			ec.CurrentBlock = block;

			if (argument_list != null){
				foreach (Argument a in argument_list){
					if (!a.Resolve (ec, loc))
						return false;
				}
			}
			ec.CurrentBlock = null;

			if (this is ConstructorBaseInitializer) {
				if (ec.ContainerType.BaseType == null)
					return true;

				t = ec.ContainerType.BaseType;
				if (ec.ContainerType.IsValueType) {
					Report.Error (522, loc,
						"`{0}': Struct constructors cannot call base constructors", TypeManager.CSharpSignature (caller_builder));
					return false;
				}
			} else
				t = ec.ContainerType;

			base_constructor_group = Expression.MemberLookup (
				ec, t, ".ctor", MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				loc);
			
			if (base_constructor_group == null){
				error = true;
				base_constructor_group = Expression.MemberLookup (
					ec, t, null, t, ".ctor", MemberTypes.Constructor,
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
					loc);
			}

			int errors = Report.Errors;
			if (base_constructor_group != null)
				base_constructor = (ConstructorInfo) Invocation.OverloadResolve (
					ec, (MethodGroupExpr) base_constructor_group, argument_list,
					false, loc);
			
			if (base_constructor == null) {
				if (errors == Report.Errors)
					Invocation.Error_WrongNumArguments (loc, TypeManager.CSharpSignature (caller_builder),
						argument_list.Count);
				return false;
			}

			if (error) {
				Expression.ErrorIsInaccesible (loc, TypeManager.CSharpSignature (base_constructor));
				base_constructor = null;
				return false;
			}
			
			if (base_constructor == caller_builder){
				Report.Error (516, loc, "Constructor `{0}' cannot call itself", TypeManager.CSharpSignature (caller_builder));
				return false;
			}
			
			return true;
		}

		public virtual void Emit (EmitContext ec)
		{
			if (base_constructor != null){
				ec.Mark (loc, false);
				if (ec.IsStatic)
					Invocation.EmitCall (ec, true, true, null, base_constructor, argument_list, loc);
				else
					Invocation.EmitCall (ec, true, false, ec.GetThis (loc), base_constructor, argument_list, loc);
			}
		}
	}

	public class ConstructorBaseInitializer : ConstructorInitializer {
		public ConstructorBaseInitializer (ArrayList argument_list, Location l) :
			base (argument_list, l)
		{
		}
	}

	class GeneratedBaseInitializer: ConstructorBaseInitializer {
		public GeneratedBaseInitializer (Location loc):
			base (null, loc)
		{
		}

		public override void Emit(EmitContext ec)
		{
			bool old = ec.TestObsoleteMethodUsage;
			ec.TestObsoleteMethodUsage = false;
			base.Emit (ec);
			ec.TestObsoleteMethodUsage = old;
		}
	}

	public class ConstructorThisInitializer : ConstructorInitializer {
		public ConstructorThisInitializer (ArrayList argument_list, Location l) :
			base (argument_list, l)
		{
		}
	}
	
	public class Constructor : MethodCore, IMethodData {
		public ConstructorBuilder ConstructorBuilder;
		public ConstructorInitializer Initializer;
		ListDictionary declarative_security;

		// <summary>
		//   Modifiers allowed for a constructor.
		// </summary>
		public const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.STATIC |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |		
			Modifiers.PRIVATE;

		bool has_compliant_args = false;
		//
		// The spec claims that static is not permitted, but
		// my very own code has static constructors.
		//
		public Constructor (TypeContainer ds, string name, int mod, Parameters args,
				    ConstructorInitializer init, Location loc)
			: base (ds, null, null, mod, AllowedModifiers, false,
				new MemberName (name, loc), null, args)
		{
			Initializer = init;
		}

		public bool HasCompliantArgs {
			get {
				return has_compliant_args;
			}
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Constructor;
			}
		}


		//
		// Returns true if this is a default constructor
		//
		public bool IsDefault ()
		{
			if ((ModFlags & Modifiers.STATIC) != 0)
				return Parameters.Empty;
			
			return Parameters.Empty &&
					(Initializer is ConstructorBaseInitializer) &&
					(Initializer.Arguments == null);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (declarative_security == null) {
					declarative_security = new ListDictionary ();
				}
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			ConstructorBuilder.SetCustomAttribute (cb);
		}

 		protected override bool CheckForDuplications ()
		{
			ArrayList ar = Parent.InstanceConstructors;
			if (ar != null) {
				int arLen = ar.Count;
					
				for (int i = 0; i < arLen; i++) {
					Constructor m = (Constructor) ar [i];
					if (IsDuplicateImplementation (m))
						return false;
				}
			}
			return true;
		}
			
		protected override bool CheckBase ()
		{
			// Check whether arguments were correct.
			if (!DoDefineParameters ())
				return false;
			
			// TODO: skip the rest for generated ctor
			if ((ModFlags & Modifiers.STATIC) != 0)
				return true;
			
			if (!CheckForDuplications ())
				return false;

			if (Parent.Kind == Kind.Struct) {
				if (ParameterTypes.Length == 0) {
					Report.Error (568, Location, 
						"Structs cannot contain explicit parameterless constructors");
					return false;
				}

				if ((ModFlags & Modifiers.PROTECTED) != 0) {
					Report.Error (666, Location, "`{0}': new protected member declared in struct", GetSignatureForError ());
						return false;
				}
			}
			
			if ((RootContext.WarningLevel >= 4) && ((Parent.ModFlags & Modifiers.SEALED) != 0 && (ModFlags & Modifiers.PROTECTED) != 0)) {
				Report.Warning (628, 4, Location, "`{0}': new protected member declared in sealed class", GetSignatureForError ());
			}
			
			return true;
		}
		
		//
		// Creates the ConstructorBuilder
		//
		public override bool Define ()
		{
			if (ConstructorBuilder != null)
				return true;

			MethodAttributes ca = (MethodAttributes.RTSpecialName |
					       MethodAttributes.SpecialName);

			if ((ModFlags & Modifiers.STATIC) != 0){
				ca |= MethodAttributes.Static | MethodAttributes.Private;
			} else {
				ca |= MethodAttributes.HideBySig;

				if ((ModFlags & Modifiers.PUBLIC) != 0)
					ca |= MethodAttributes.Public;
				else if ((ModFlags & Modifiers.PROTECTED) != 0){
					if ((ModFlags & Modifiers.INTERNAL) != 0)
						ca |= MethodAttributes.FamORAssem;
					else 
						ca |= MethodAttributes.Family;
				} else if ((ModFlags & Modifiers.INTERNAL) != 0)
					ca |= MethodAttributes.Assembly;
				else if (IsDefault ())
					ca |= MethodAttributes.Public;
				else
					ca |= MethodAttributes.Private;
			}

			if (!CheckAbstractAndExtern (block != null))
				return false;
			
			// Check if arguments were correct.
			if (!CheckBase ())
				return false;

			ConstructorBuilder = Parent.TypeBuilder.DefineConstructor (
				ca, CallingConventions,
				ParameterTypes);

			if ((ModFlags & Modifiers.UNSAFE) != 0)
				ConstructorBuilder.InitLocals = false;

			if (Parent.IsComImport) {
				if (!IsDefault ()) {
					Report.Error (669, Location, "`{0}': A class with the ComImport attribute cannot have a user-defined constructor",
						Parent.GetSignatureForError ());
					return false;
				}
				ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.InternalCall);
			}
			
			TypeManager.AddMethod (ConstructorBuilder, this);

			return true;
		}

		//
		// Emits the code
		//
		public override void Emit ()
		{
			EmitContext ec = CreateEmitContext (null, null);
			if (GetObsoleteAttribute () != null || Parent.GetObsoleteAttribute () != null)
				ec.TestObsoleteMethodUsage = false;

			// If this is a non-static `struct' constructor and doesn't have any
			// initializer, it must initialize all of the struct's fields.
			if ((Parent.Kind == Kind.Struct) &&
			    ((ModFlags & Modifiers.STATIC) == 0) && (Initializer == null))
				Block.AddThisVariable (Parent, Location);

			if (block != null) {
				if (!block.ResolveMeta (ec, ParameterInfo))
					block = null;
			}

			if ((ModFlags & Modifiers.STATIC) == 0){
				if (Parent.Kind == Kind.Class && Initializer == null)
					Initializer = new GeneratedBaseInitializer (Location);


				//
				// Spec mandates that Initializers will not have
				// `this' access
				//
				ec.IsStatic = true;
				if ((Initializer != null) &&
				    !Initializer.Resolve (ConstructorBuilder, block, ec))
					return;
				ec.IsStatic = false;
			}

			Parameters.ApplyAttributes (ec, ConstructorBuilder);
			
			SourceMethod source = SourceMethod.Create (
				Parent, ConstructorBuilder, block);

			//
			// Classes can have base initializers and instance field initializers.
			//
			if (Parent.Kind == Kind.Class){
				if ((ModFlags & Modifiers.STATIC) == 0){

					//
					// If we use a "this (...)" constructor initializer, then
					// do not emit field initializers, they are initialized in the other constructor
					//
					if (!(Initializer != null && Initializer is ConstructorThisInitializer))
						Parent.EmitFieldInitializers (ec);
				}
			}
			if (Initializer != null) {
				Initializer.Emit (ec);
			}
			
			if ((ModFlags & Modifiers.STATIC) != 0)
				Parent.EmitFieldInitializers (ec);

			if (OptAttributes != null) 
				OptAttributes.Emit (ec, this);

			ec.EmitTopBlock (this, block);

			if (source != null)
				source.CloseMethod ();

			base.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					ConstructorBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}

			block = null;
		}

		// Is never override
		protected override MethodInfo FindOutBaseMethod (TypeContainer container, ref Type base_ret_type)
		{
			return null;
		}
						
		public override string GetSignatureForError()
		{
			return base.GetSignatureForError () + Parameters.GetSignatureForError ();
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds) || !IsExposedFromAssembly (ds)) {
				return false;
			}

 			if (ParameterInfo.Count > 0) {
 				ArrayList al = (ArrayList)ds.MemberCache.Members [".ctor"];
 				if (al.Count > 3)
 					ds.MemberCache.VerifyClsParameterConflict (al, this, ConstructorBuilder);
				
				if (ds.TypeBuilder.IsSubclassOf (TypeManager.attribute_type)) {
					foreach (Type param in ParameterTypes) {
						if (param.IsArray) {
							return true;
				}
			}
				}
			}
			has_compliant_args = true;
				return true;
		}

		#region IMethodData Members

		public System.Reflection.CallingConventions CallingConventions {
			get {
				CallingConventions cc = Parameters.CallingConvention;

				if (Parent.Kind == Kind.Class)
					if ((ModFlags & Modifiers.STATIC) == 0)
						cc |= CallingConventions.HasThis;

				// FIXME: How is `ExplicitThis' used in C#?
			
				return cc;
				}
			}
			
		public new Location Location {
			get {
				return base.Location;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}
			
		public Type ReturnType {
			get {
				return MemberType;
			}
		}

		public EmitContext CreateEmitContext (TypeContainer tc, ILGenerator ig)
		{
			ILGenerator ig_ = ConstructorBuilder.GetILGenerator ();
			return new EmitContext (Parent, Location, ig_, null, ModFlags, true);
		}

		public bool IsExcluded(EmitContext ec)
		{
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return null;
			}
		}

		#endregion
	}

	/// <summary>
	/// Interface for MethodData class. Holds links to parent members to avoid member duplication.
	/// </summary>
	public interface IMethodData
	{
		CallingConventions CallingConventions { get; }
		Location Location { get; }
		MemberName MethodName { get; }
		Type ReturnType { get; }
		GenericMethod GenericMethod { get; }
		Parameters ParameterInfo { get; }

		Attributes OptAttributes { get; }
		ToplevelBlock Block { get; set; }

		EmitContext CreateEmitContext (TypeContainer tc, ILGenerator ig);
		ObsoleteAttribute GetObsoleteAttribute ();
		string GetSignatureForError ();
		bool IsExcluded (EmitContext ec);
		bool IsClsComplianceRequired (DeclSpace ds);
		void SetMemberIsUsed ();
	}

	//
	// Encapsulates most of the Method's state
	//
	public class MethodData {

		readonly IMethodData method;

		public readonly GenericMethod GenericMethod;

		//
		// Are we implementing an interface ?
		//
		public MethodInfo implementing;

		//
		// Protected data.
		//
		protected MemberBase member;
		protected int modifiers;
		protected MethodAttributes flags;
		protected Type declaring_type;
		protected MethodInfo parent_method;

		MethodBuilder builder = null;
		public MethodBuilder MethodBuilder {
			get {
				return builder;
			}
		}

		public Type DeclaringType {
			get {
				return declaring_type;
			}
		}

		public MethodData (MemberBase member,
				   int modifiers, MethodAttributes flags, IMethodData method)
		{
			this.member = member;
			this.modifiers = modifiers;
			this.flags = flags;

			this.method = method;
		}

		public MethodData (MemberBase member, 
				   int modifiers, MethodAttributes flags, 
				   IMethodData method, MethodBuilder builder,
				   GenericMethod generic, MethodInfo parent_method)
			: this (member, modifiers, flags, method)
		{
			this.builder = builder;
			this.GenericMethod = generic;
			this.parent_method = parent_method;
		}

		public bool Define (TypeContainer container)
		{
			string name = method.MethodName.Basename;
			string method_name = method.MethodName.FullName;

			if (container.Pending != null){
				if (member is Indexer) // TODO: test it, but it should work without this IF
					implementing = container.Pending.IsInterfaceIndexer (
						member.InterfaceType, method.ReturnType, method.ParameterInfo);
				else
					implementing = container.Pending.IsInterfaceMethod (
						member.InterfaceType, name, method.ReturnType, method.ParameterInfo);

				if (member.InterfaceType != null){
					if (implementing == null){
						if (member is PropertyBase) {
							Report.Error (550, method.Location, "`{0}' is an accessor not found in interface member `{1}{2}'",
								      method.GetSignatureForError (), TypeManager.CSharpName (member.InterfaceType),
								      member.GetSignatureForError ().Substring (member.GetSignatureForError ().LastIndexOf ('.')));

						} else {
							Report.Error (539, method.Location,
								      "`{0}.{1}' in explicit interface declaration is not a member of interface",
								      TypeManager.CSharpName (member.InterfaceType), member.ShortName);
						}
						return false;
					}
					if (implementing.IsSpecialName && !(method is AbstractPropertyEventMethod)) {
						Report.SymbolRelatedToPreviousError (implementing);
						Report.Error (683, method.Location, "`{0}' explicit method implementation cannot implement `{1}' because it is an accessor",
							member.GetSignatureForError (), TypeManager.CSharpSignature (implementing));
						return false;
					}

					method_name = TypeManager.GetFullName (member.InterfaceType) +
						'.' + method_name;
				} else {
					if (implementing != null) {
						AbstractPropertyEventMethod prop_method = method as AbstractPropertyEventMethod;
						if (prop_method != null) {
							if (!implementing.IsSpecialName) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (686, method.Location, "Accessor `{0}' cannot implement interface member `{1}' for type `{2}'. Use an explicit interface implementation",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing), container.GetSignatureForError ());
								return false;
							}
							PropertyBase.PropertyMethod pm = prop_method as PropertyBase.PropertyMethod;
							if (pm != null && pm.HasCustomAccessModifier && (pm.ModFlags & Modifiers.PUBLIC) == 0) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (277, method.Location, "Accessor `{0}' must be declared public to implement interface member `{1}'",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing, true));
								return false;
							}
						}
					}
				}
			}

			//
			// For implicit implementations, make sure we are public, for
			// explicit implementations, make sure we are private.
			//
			if (implementing != null){
				//
				// Setting null inside this block will trigger a more
				// verbose error reporting for missing interface implementations
				//
				// The "candidate" function has been flagged already
				// but it wont get cleared
				//
				if (member.IsExplicitImpl){
					if ((modifiers & (Modifiers.PUBLIC | Modifiers.ABSTRACT | Modifiers.VIRTUAL)) != 0){
						Modifiers.Error_InvalidModifier (method.Location, "public, virtual or abstract");
						implementing = null;
					}
				} else if ((flags & MethodAttributes.MemberAccessMask) != MethodAttributes.Public){
					if (TypeManager.IsInterfaceType (implementing.DeclaringType)){
						//
						// If this is an interface method implementation,
						// check for public accessibility
						//
						implementing = null;
					} else if ((flags & MethodAttributes.MemberAccessMask) == MethodAttributes.Private){
						// We may never be private.
						implementing = null;
					} else if ((modifiers & Modifiers.OVERRIDE) == 0){
						//
						// We may be protected if we're overriding something.
						//
						implementing = null;
					}
				} 
					
				//
				// Static is not allowed
				//
				if ((modifiers & Modifiers.STATIC) != 0){
					implementing = null;
				}
			}
			
			//
			// If implementing is still valid, set flags
			//
			if (implementing != null){
				//
				// When implementing interface methods, set NewSlot
				// unless, we are overwriting a method.
				//
				if (implementing.DeclaringType.IsInterface){
					if ((modifiers & Modifiers.OVERRIDE) == 0)
						flags |= MethodAttributes.NewSlot;
				}
				flags |=
					MethodAttributes.Virtual |
					MethodAttributes.HideBySig;

				// Set Final unless we're virtual, abstract or already overriding a method.
				if ((modifiers & (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE)) == 0)
					flags |= MethodAttributes.Final;
			}

			EmitContext ec = method.CreateEmitContext (container, null);
			if (method.GetObsoleteAttribute () != null || container.GetObsoleteAttribute () != null)
				ec.TestObsoleteMethodUsage = false;

			DefineMethodBuilder (ec, container, method_name, method.ParameterInfo.Types);

			if (builder == null)
				return false;

			if (container.CurrentType != null)
				declaring_type = container.CurrentType;
			else
				declaring_type = container.TypeBuilder;

			if ((modifiers & Modifiers.UNSAFE) != 0)
				builder.InitLocals = false;

			if (implementing != null){
				//
				// clear the pending implemntation flag
				//
				if (member is Indexer) {
					container.Pending.ImplementIndexer (
						member.InterfaceType, builder, method.ReturnType,
						method.ParameterInfo, member.IsExplicitImpl);
				} else
					container.Pending.ImplementMethod (
						member.InterfaceType, name, method.ReturnType,
						method.ParameterInfo, member.IsExplicitImpl);

				if (member.IsExplicitImpl)
					container.TypeBuilder.DefineMethodOverride (
						builder, implementing);
			}

			TypeManager.AddMethod (builder, method);

			if (GenericMethod != null) {
				bool is_override = member.IsExplicitImpl |
					((modifiers & Modifiers.OVERRIDE) != 0);

				if (implementing != null)
					parent_method = implementing;

				if (!GenericMethod.DefineType (ec, builder, parent_method, is_override))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Create the MethodBuilder for the method 
		/// </summary>
		void DefineMethodBuilder (EmitContext ec, TypeContainer container, string method_name, Type[] ParameterTypes)
		{
			const int extern_static = Modifiers.EXTERN | Modifiers.STATIC;

			if ((modifiers & extern_static) == extern_static) {

				if (method.OptAttributes != null) {
					Attribute dllimport_attribute = method.OptAttributes.Search (TypeManager.dllimport_type, ec);
					if (dllimport_attribute != null) {
						flags |= MethodAttributes.PinvokeImpl;
						builder = dllimport_attribute.DefinePInvokeMethod (
							ec, container.TypeBuilder, method_name, flags,
							method.ReturnType, ParameterTypes);

						return;
					}
				}

				// for extern static method must be specified either DllImport attribute or MethodImplAttribute.
				// We are more strict than Microsoft and report CS0626 like error
				if (method.OptAttributes == null ||
					!method.OptAttributes.Contains (TypeManager.methodimpl_attr_type, ec)) {
					Report.Error (626, method.Location, "Method, operator, or accessor `{0}' is marked external and has no attributes on it. Consider adding a DllImport attribute to specify the external implementation",
						method.GetSignatureForError ());
					return;
				}
			}

			if (builder == null)
				builder = container.TypeBuilder.DefineMethod (
					method_name, flags, method.CallingConventions,
					method.ReturnType, ParameterTypes);
			else
				builder.SetGenericMethodSignature (
					flags, method.CallingConventions,
					method.ReturnType, ParameterTypes);
		}

		//
		// Emits the code
		// 
		public void Emit (TypeContainer container, Attributable kind)
		{
			EmitContext ec;
			if ((flags & MethodAttributes.PinvokeImpl) == 0)
				ec = method.CreateEmitContext (container, builder.GetILGenerator ());
			else
				ec = method.CreateEmitContext (container, null);

			if (method.GetObsoleteAttribute () != null || container.GetObsoleteAttribute () != null)
				ec.TestObsoleteMethodUsage = false;

			method.ParameterInfo.ApplyAttributes (ec, MethodBuilder);

			Attributes OptAttributes = method.OptAttributes;

			if (OptAttributes != null)
				OptAttributes.Emit (ec, kind);

			if (GenericMethod != null)
				GenericMethod.EmitAttributes (ec);

			ToplevelBlock block = method.Block;
			
			SourceMethod source = SourceMethod.Create (
				container, MethodBuilder, method.Block);

			//
			// Handle destructors specially
			//
			// FIXME: This code generates buggy code
			//
			if (member is Destructor)
				EmitDestructor (ec, block);
			else
				ec.EmitTopBlock (method, block);

			if (source != null)
				source.CloseMethod ();
		}

		void EmitDestructor (EmitContext ec, ToplevelBlock block)
		{
			ILGenerator ig = ec.ig;
			
			Label finish = ig.DefineLabel ();

			block.SetDestructor ();
			
			ig.BeginExceptionBlock ();
			ec.ReturnLabel = finish;
			ec.HasReturnLabel = true;
			ec.EmitTopBlock (method, block);
			
			// ig.MarkLabel (finish);
			ig.BeginFinallyBlock ();
			
			if (ec.ContainerType.BaseType != null) {
				Expression member_lookup = Expression.MemberLookup (
					ec, ec.ContainerType.BaseType, null, ec.ContainerType.BaseType,
					"Finalize", MemberTypes.Method, Expression.AllBindingFlags, method.Location);

				if (member_lookup != null){
					MethodGroupExpr base_destructor = ((MethodGroupExpr) member_lookup);
				
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Call, (MethodInfo) base_destructor.Methods [0]);
				}
			}
			
			ig.EndExceptionBlock ();
			//ig.MarkLabel (ec.ReturnLabel);
			ig.Emit (OpCodes.Ret);
		}
	}

	// TODO: Should derive from MethodCore
	public class Destructor : Method {

		public Destructor (TypeContainer ds, Expression return_type, int mod,
				   string name, Parameters parameters, Attributes attrs,
				   Location l)
			: base (ds, null, return_type, mod, false, new MemberName (name, l),
				parameters, attrs)
		{ }

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.conditional_attribute_type) {
				Report.Error (577, Location, "Conditional not valid on `{0}' because it is a constructor, destructor, operator or explicit interface implementation",
					GetSignatureForError ());
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}

		public override string GetSignatureForError ()
		{
			return Parent.GetSignatureForError () + ".~" + Parent.MemberName.Name + "()";
		}

	}
	
	abstract public class MemberBase : MemberCore {
		public Expression Type;

		public MethodAttributes flags;
		public readonly DeclSpace ds;
		public readonly GenericMethod GenericMethod;

		protected readonly int explicit_mod_flags;

		//
		// The "short" name of this property / indexer / event.  This is the
		// name without the explicit interface.
		//
		public string ShortName {
			get { return MemberName.Name; }
			set { SetMemberName (new MemberName (MemberName.Left, value, Location)); }
		}

		public new TypeContainer Parent {
			get { return (TypeContainer) base.Parent; }
		}

		//
		// The type of this property / indexer / event
		//
		protected Type member_type;
		public Type MemberType {
			get {
				if (member_type == null && Type != null) {
					EmitContext ec = ds.EmitContext;
					bool old_unsafe = ec.InUnsafe;
					ec.InUnsafe = InUnsafe;
					ec.ResolvingGenericMethod = GenericMethod != null;
					Type = Type.ResolveAsTypeTerminal (ec);
					ec.ResolvingGenericMethod = false;
					ec.InUnsafe = old_unsafe;
					if (Type != null) {
						member_type = Type.Type;
					}
				}
				return member_type;
			}
		}

		//
		// Whether this is an interface member.
		//
		public bool IsInterface;

		//
		// If true, this is an explicit interface implementation
		//
		public bool IsExplicitImpl;

		//
		// The interface type we are explicitly implementing
		//
		public Type InterfaceType = null;

		//
		// The constructor is only exposed to our children
		//
		protected MemberBase (TypeContainer parent, GenericMethod generic,
				      Expression type, int mod, int allowed_mod, int def_mod,
				      MemberName name, Attributes attrs)
			: base (parent, name, attrs)
		{
			this.ds = generic != null ? generic : (DeclSpace) parent;
			explicit_mod_flags = mod;
			Type = type;
			ModFlags = Modifiers.Check (allowed_mod, mod, def_mod, Location);
			IsExplicitImpl = (MemberName.Left != null);
			GenericMethod = generic;
		}

		protected virtual bool CheckBase ()
		{
  			if ((ModFlags & Modifiers.PROTECTED) != 0 && Parent.Kind == Kind.Struct) {
  				Report.Error (666, Location, "`{0}': new protected member declared in struct", GetSignatureForError ());
  				return false;
   			}
   
  			if ((RootContext.WarningLevel >= 4) &&
			    ((Parent.ModFlags & Modifiers.SEALED) != 0) &&
			    ((ModFlags & Modifiers.PROTECTED) != 0) &&
			    ((ModFlags & Modifiers.OVERRIDE) == 0) && (Name != "Finalize")) {
  				Report.Warning (628, 4, Location, "`{0}': new protected member declared in sealed class", GetSignatureForError ());
   			}
  			return true;
		}

		protected virtual bool DoDefineBase ()
		{
			EmitContext ec = Parent.EmitContext;
			if (ec == null)
				throw new InternalErrorException ("MemberBase.DoDefine called too early");

			if (Name == null)
				throw new InternalErrorException ();

			if (IsInterface) {
				ModFlags = Modifiers.PUBLIC |
					Modifiers.ABSTRACT |
					Modifiers.VIRTUAL | (ModFlags & Modifiers.UNSAFE) | (ModFlags & Modifiers.NEW);

				flags = MethodAttributes.Public |
					MethodAttributes.Abstract |
					MethodAttributes.HideBySig |
					MethodAttributes.NewSlot |
					MethodAttributes.Virtual;
			} else {
				if (!Parent.MethodModifiersValid (this))
					return false;

				flags = Modifiers.MethodAttr (ModFlags);
			}

			if (IsExplicitImpl) {
				Expression expr = MemberName.Left.GetTypeExpression ();
				TypeExpr iface_texpr = expr.ResolveAsTypeTerminal (ec);
				if (iface_texpr == null)
					return false;

				InterfaceType = iface_texpr.ResolveType (ec);

				if (!InterfaceType.IsInterface) {
					Report.Error (538, Location, "'{0}' in explicit interface declaration is not an interface", TypeManager.CSharpName (InterfaceType));
					return false;
				}

				if (!Parent.VerifyImplements (this))
					return false;
				
				Modifiers.Check (Modifiers.AllowedExplicitImplFlags, explicit_mod_flags, 0, Location);
			}

			return true;
		}

		protected virtual bool DoDefine ()
		{
			EmitContext ec = ds.EmitContext;
			if (ec == null)
				throw new InternalErrorException ("MemberBase.DoDefine called too early");

			ec.InUnsafe = InUnsafe;

			if (MemberType == null)
				return false;

			CheckObsoleteType (Type);

			if ((Parent.ModFlags & Modifiers.SEALED) != 0 && 
				(ModFlags & (Modifiers.VIRTUAL|Modifiers.ABSTRACT)) != 0) {
					Report.Error (549, Location, "New virtual member `{0}' is declared in a sealed class `{1}'",
						GetSignatureForError (), Parent.GetSignatureForError ());
					return false;
			}
			
			// verify accessibility
			if (!Parent.AsAccessible (MemberType, ModFlags)) {
				Report.SymbolRelatedToPreviousError (MemberType);
				if (this is Property)
					Report.Error (53, Location,
						      "Inconsistent accessibility: property type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than property `" + GetSignatureForError () + "'");
				else if (this is Indexer)
					Report.Error (54, Location,
						      "Inconsistent accessibility: indexer return type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than indexer `" + GetSignatureForError () + "'");
				else if (this is MethodCore) {
					if (this is Operator)
						Report.Error (56, Location,
							      "Inconsistent accessibility: return type `" +
							      TypeManager.CSharpName (MemberType) + "' is less " +
							      "accessible than operator `" + GetSignatureForError () + "'");
					else
						Report.Error (50, Location,
							      "Inconsistent accessibility: return type `" +
							      TypeManager.CSharpName (MemberType) + "' is less " +
							      "accessible than method `" + GetSignatureForError () + "'");
				} else {
					Report.Error (52, Location,
						      "Inconsistent accessibility: field type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than field `" + GetSignatureForError () + "'");
				}
				return false;
			}

			if (MemberType.IsPointer && !UnsafeOK (Parent))
				return false;

			if (IsExplicitImpl) {
				Expression expr = MemberName.Left.GetTypeExpression ();
				TypeExpr texpr = expr.ResolveAsTypeTerminal (ec);
				if (texpr == null)
					return false;

				InterfaceType = texpr.ResolveType (ec);

				if (!InterfaceType.IsInterface) {
					Report.Error (538, Location, "`{0}' in explicit interface declaration is not an interface", TypeManager.CSharpName (InterfaceType));
					return false;
				}
				
				if (!Parent.VerifyImplements (this))
					return false;
				
				Modifiers.Check (Modifiers.AllowedExplicitImplFlags, explicit_mod_flags, 0, Location);
				
			}
			return true;
		}

		protected bool IsTypePermitted ()
		{
			if (MemberType == TypeManager.arg_iterator_type || MemberType == TypeManager.typed_reference_type) {
				Report.Error (610, Location, "Field or property cannot be of type `{0}'", TypeManager.CSharpName (MemberType));
				return false;
			}
			return true;
		}

		protected override bool VerifyClsCompliance(DeclSpace ds)
		{
			if (base.VerifyClsCompliance (ds)) {
				return true;
			}

			if (IsInterface && HasClsCompliantAttribute && ds.IsClsComplianceRequired (ds)) {
				Report.Error (3010, Location, "`{0}': CLS-compliant interfaces must have only CLS-compliant members", GetSignatureForError ());
			}
			return false;
		}

	}

	//
	// Fields and Events both generate FieldBuilders, we use this to share 
	// their common bits.  This is also used to flag usage of the field
	//
	abstract public class FieldBase : MemberBase {
		public FieldBuilder  FieldBuilder;
		public Status status;
		protected Expression initializer;

		[Flags]
		public enum Status : byte {
			HAS_OFFSET = 4		// Used by FieldMember.
		}

		static string[] attribute_targets = new string [] { "field" };

		/// <summary>
		///  Symbol with same name in base class/struct
		/// </summary>
		public MemberInfo conflict_symbol;

		protected FieldBase (TypeContainer parent, Expression type, int mod,
				     int allowed_mod, MemberName name, Attributes attrs)
			: base (parent, null, type, mod, allowed_mod, Modifiers.PRIVATE,
				name, attrs)
		{
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Field;
			}
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.marshal_as_attr_type) {
				UnmanagedMarshal marshal = a.GetMarshal (this);
				if (marshal != null) {
					FieldBuilder.SetMarshal (marshal);
				}
					return;
				}

			if (a.Type.IsSubclassOf (TypeManager.security_attr_type)) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			FieldBuilder.SetCustomAttribute (cb);
		}

		public void EmitInitializer (EmitContext ec)
		{
			// Replace DeclSpace because of partial classes
			ec.DeclSpace = EmitContext.DeclSpace;

			ec.IsFieldInitializer = true;
			initializer = initializer.Resolve (ec);
			ec.IsFieldInitializer = false;
			if (initializer == null)
				return;
			FieldExpr fe = new FieldExpr (FieldBuilder, Location, true);
			if ((ModFlags & Modifiers.STATIC) == 0)
				fe.InstanceExpression = new This (Location).Resolve (ec);

			ExpressionStatement a = new Assign (fe, initializer, Location);

			a = a.ResolveStatement (ec);
			if (a == null)
				return;

			Constant c = initializer as Constant;
			if (c != null && CanElideInitializer (c))
				return;

			a.EmitStatement (ec);
		}

		bool CanElideInitializer (Constant c)
		{
			if (MemberType == c.Type)
				return c.IsDefaultValue;

			if (c.Type == TypeManager.null_type)
				return true;

			return false;
		}

 		protected override bool CheckBase ()
		{
 			if (!base.CheckBase ())
 				return false;

 			// TODO: Implement
 			if (IsInterface)
 				return true;

 			conflict_symbol = Parent.FindBaseMemberWithSameName (Name, false);
 			if (conflict_symbol == null) {
 				if ((RootContext.WarningLevel >= 4) && ((ModFlags & Modifiers.NEW) != 0)) {
 					Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
 				}
 				return true;
 			}

 			if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE)) == 0) {
				Report.SymbolRelatedToPreviousError (conflict_symbol);
				Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
					GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
			}

			return true;
 		}

		public Expression Initializer {
			set {
				if (value != null) {
					this.initializer = value;
					Parent.RegisterFieldForInitialization (this);
				}
			}
		}

		protected virtual bool IsFieldClsCompliant {
			get {
				if (FieldBuilder == null)
					return true;

				return AttributeTester.IsClsCompliant (FieldBuilder.FieldType);
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			if (!IsFieldClsCompliant) {
				Report.Error (3003, Location, "Type of `{0}' is not CLS-compliant", GetSignatureForError ());
			}
			return true;
		}


		public void SetAssigned ()
		{
			caching_flags |= Flags.IsAssigned;
		}
	}

	public abstract class FieldMember : FieldBase
	{
		protected FieldMember (TypeContainer parent, Expression type, int mod,
			int allowed_mod, MemberName name, Attributes attrs)
			: base (parent, type, mod, allowed_mod | Modifiers.ABSTRACT, name, attrs)
		{
			if ((mod & Modifiers.ABSTRACT) != 0)
				Report.Error (681, Location, "The modifier 'abstract' is not valid on fields. Try using a property instead");
		}

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.field_offset_attribute_type)
			{
				status |= Status.HAS_OFFSET;

				if (!Parent.HasExplicitLayout) {
					Report.Error (636, Location, "The FieldOffset attribute can only be placed on members of types marked with the StructLayout(LayoutKind.Explicit)");
					return;
				}

				if ((ModFlags & Modifiers.STATIC) != 0 || this is Const) {
					Report.Error (637, Location, "The FieldOffset attribute is not allowed on static or const fields");
					return;
				}
			}

			if (a.Type == TypeManager.fixed_buffer_attr_type) {
				Report.Error (1716, Location, "Do not use 'System.Runtime.CompilerServices.FixedBuffer' attribute. Use the 'fixed' field modifier instead");
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}


		public override bool Define()
		{
			EmitContext ec = Parent.EmitContext;
			if (ec == null)
				throw new InternalErrorException ("FieldMember.Define called too early");

			if (MemberType == null || Type == null)
				return false;
			
			CheckObsoleteType (Type);

			if (MemberType == TypeManager.void_type) {
				Report.Error (1547, Location, "Keyword 'void' cannot be used in this context");
				return false;
			}

			if (!CheckBase ())
				return false;
			
			if (!Parent.AsAccessible (MemberType, ModFlags)) {
				Report.Error (52, Location,
					"Inconsistent accessibility: field type `" +
					TypeManager.CSharpName (MemberType) + "' is less " +
					"accessible than field `" + GetSignatureForError () + "'");
				return false;
			}

			if (!IsTypePermitted ())
				return false;

			if (MemberType.IsPointer && !UnsafeOK (Parent))
				return false;

			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				EmitContext ec = new EmitContext (Parent, Location, null, FieldBuilder.FieldType, ModFlags);
				OptAttributes.Emit (ec, this);
			}

			if (Parent.HasExplicitLayout && ((status & Status.HAS_OFFSET) == 0) && (ModFlags & Modifiers.STATIC) == 0) {
				Report.Error (625, Location, "`{0}': Instance field types marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute.", GetSignatureForError ());
			}

			base.Emit ();
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "F:"; }
		}
	}

	interface IFixedBuffer
	{
		FieldInfo Element { get; }
		Type ElementType { get; }
	}

	public class FixedFieldExternal: IFixedBuffer
	{
		FieldInfo element_field;

		public FixedFieldExternal (FieldInfo fi)
		{
			element_field = fi.FieldType.GetField (FixedField.FixedElementName);
		}

		#region IFixedField Members

		public FieldInfo Element {
			get {
				return element_field;
			}
		}

		public Type ElementType {
			get {
				return element_field.FieldType;
			}
		}

		#endregion
	}

	/// <summary>
	/// Fixed buffer implementation
	/// </summary>
	public class FixedField : FieldMember, IFixedBuffer
	{
		public const string FixedElementName = "FixedElementField";
		static int GlobalCounter = 0;
		static object[] ctor_args = new object[] { (short)LayoutKind.Sequential };
		static FieldInfo[] fi;

		TypeBuilder fixed_buffer_type;
		FieldBuilder element;
		Expression size_expr;
		int buffer_size;

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public FixedField (TypeContainer parent, Expression type, int mod, string name,
			Expression size_expr, Attributes attrs, Location loc):
			base (parent, type, mod, AllowedModifiers, new MemberName (name, loc), attrs)
		{
			if (RootContext.Version == LanguageVersion.ISO_1)
				Report.FeatureIsNotStandardized (loc, "fixed size buffers");

			this.size_expr = size_expr;
		}

		public override bool Define()
		{
#if !NET_2_0
			if ((ModFlags & (Modifiers.PUBLIC | Modifiers.PROTECTED)) != 0)
				Report.Warning (-23, 1, Location, "Only private or internal fixed sized buffers are supported by .NET 1.x");
#endif

			if (Parent.Kind != Kind.Struct) {
				Report.Error (1642, Location, "`{0}': Fixed size buffer fields may only be members of structs",
					GetSignatureForError ());
				return false;
			}

			if (!base.Define ())
				return false;

			if (!TypeManager.IsPrimitiveType (MemberType)) {
				Report.Error (1663, Location, "`{0}': Fixed size buffers type must be one of the following: bool, byte, short, int, long, char, sbyte, ushort, uint, ulong, float or double",
					GetSignatureForError ());
				return false;
			}

			Constant c = size_expr.ResolveAsConstant (Parent.EmitContext, this);
			if (c == null)
				return false;

			IntConstant buffer_size_const = c.ToInt (Location);
			if (buffer_size_const == null)
				return false;

			buffer_size = buffer_size_const.Value;

			if (buffer_size <= 0) {
				Report.Error (1665, Location, "`{0}': Fixed size buffers must have a length greater than zero", GetSignatureForError ());
				return false;
			}

			int type_size = Expression.GetTypeSize (MemberType);

			if (buffer_size > int.MaxValue / type_size) {
				Report.Error (1664, Location, "Fixed size buffer `{0}' of length `{1}' and type `{2}' exceeded 2^31 limit",
					GetSignatureForError (), buffer_size.ToString (), TypeManager.CSharpName (MemberType));
				return false;
			}

			buffer_size *= type_size;

			// Define nested
			string name = String.Format ("<{0}>__FixedBuffer{1}", Name, GlobalCounter++);

			fixed_buffer_type = Parent.TypeBuilder.DefineNestedType (name,
				TypeAttributes.NestedPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, TypeManager.value_type);
			element = fixed_buffer_type.DefineField (FixedElementName, MemberType, FieldAttributes.Public);
			RootContext.RegisterCompilerGeneratedType (fixed_buffer_type);

			FieldBuilder = Parent.TypeBuilder.DefineField (Name, fixed_buffer_type, Modifiers.FieldAttr (ModFlags));
			TypeManager.RegisterFieldBase (FieldBuilder, this);

			return true;
		}

		public override void Emit()
		{
			if (fi == null)
				fi = new FieldInfo [] { TypeManager.struct_layout_attribute_type.GetField ("Size") };

			object[] fi_val = new object[1];
			fi_val [0] = buffer_size;

			CustomAttributeBuilder cab = new CustomAttributeBuilder (TypeManager.struct_layout_attribute_ctor, 
				ctor_args, fi, fi_val);
			fixed_buffer_type.SetCustomAttribute (cab);

			cab = new CustomAttributeBuilder (TypeManager.fixed_buffer_attr_ctor, new object[] { MemberType, buffer_size } );
			FieldBuilder.SetCustomAttribute (cab);
			base.Emit ();
		}

		protected override bool IsFieldClsCompliant {
			get {
				return false;
			}
		}

		#region IFixedField Members

		public FieldInfo Element {
			get {
				return element;
			}
		}

		public Type ElementType {
			get {
				return MemberType;
			}
		}

		#endregion
	}

	//
	// The Field class is used to represents class/struct fields during parsing.
	//
	public class Field : FieldMember {
		// <summary>
		//   Modifiers allowed in a class declaration
		// </summary>
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
		        Modifiers.VOLATILE |
		        Modifiers.UNSAFE |
			Modifiers.READONLY;

		public Field (TypeContainer parent, Expression type, int mod, string name,
			      Attributes attrs, Location loc)
			: base (parent, type, mod, AllowedModifiers, new MemberName (name, loc),
				attrs)
		{
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (RootContext.WarningLevel > 1){
				Type ptype = Parent.TypeBuilder.BaseType;

				// ptype is only null for System.Object while compiling corlib.
				if (ptype != null){
					TypeContainer.FindMembers (
						ptype, MemberTypes.Method,
						BindingFlags.Public |
						BindingFlags.Static | BindingFlags.Instance,
						System.Type.FilterName, Name);
				}
			}

			if ((ModFlags & Modifiers.VOLATILE) != 0){
				if (!MemberType.IsClass){
					Type vt = MemberType;
					
					if (TypeManager.IsEnumType (vt))
						vt = TypeManager.EnumToUnderlying (MemberType);

					if (!((vt == TypeManager.bool_type) ||
					      (vt == TypeManager.sbyte_type) ||
					      (vt == TypeManager.byte_type) ||
					      (vt == TypeManager.short_type) ||
					      (vt == TypeManager.ushort_type) ||
					      (vt == TypeManager.int32_type) ||
					      (vt == TypeManager.uint32_type) ||    
					      (vt == TypeManager.char_type) ||
					      (vt == TypeManager.float_type) ||
					      (!vt.IsValueType))){
						Report.Error (677, Location, "`{0}': A volatile field cannot be of the type `{1}'",
							GetSignatureForError (), TypeManager.CSharpName (vt));
						return false;
					}
				}

				if ((ModFlags & Modifiers.READONLY) != 0){
					Report.Error (678, Location, "`{0}': A field cannot be both volatile and readonly",
						GetSignatureForError ());
					return false;
				}
			}

			FieldAttributes fa = Modifiers.FieldAttr (ModFlags);

			if (Parent.Kind == Kind.Struct && 
			    ((fa & FieldAttributes.Static) == 0) &&
			    MemberType == Parent.TypeBuilder &&
			    !TypeManager.IsBuiltinType (MemberType)){
				Report.Error (523, Location, "Struct member `" + Parent.Name + "." + Name + 
					      "' causes a cycle in the structure layout");
				return false;
			}

			try {
				FieldBuilder = Parent.TypeBuilder.DefineField (
					Name, MemberType, Modifiers.FieldAttr (ModFlags));

			TypeManager.RegisterFieldBase (FieldBuilder, this);
			}
			catch (ArgumentException) {
				Report.Warning (-24, 1, Location, "The Microsoft runtime is unable to use [void|void*] as a field type, try using the Mono runtime.");
				return false;
			}

			return true;
		}

		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds))
				return false;

			if ((ModFlags & Modifiers.VOLATILE) != 0) {
				Report.Warning (3026, 1, Location, "CLS-compliant field `{0}' cannot be volatile", GetSignatureForError ());
			}

			return true;
		}
	}

	//
	// `set' and `get' accessors are represented with an Accessor.
	// 
	public class Accessor : IIteratorContainer {
		//
		// Null if the accessor is empty, or a Block if not
		//
		public const int AllowedModifiers = 
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;
		
		public ToplevelBlock Block;
		public Attributes Attributes;
		public Location Location;
		public int ModFlags;
		public bool Yields;
		
		public Accessor (ToplevelBlock b, int mod, Attributes attrs, Location loc)
		{
			Block = b;
			Attributes = attrs;
			Location = loc;
			ModFlags = Modifiers.Check (AllowedModifiers, mod, 0, loc);
		}

		public void SetYields ()
		{
			Yields = true;
		}
	}

	// Ooouh Martin, templates are missing here.
	// When it will be possible move here a lot of child code and template method type.
	public abstract class AbstractPropertyEventMethod : MemberCore, IMethodData {
		protected MethodData method_data;
		protected ToplevelBlock block;
		protected ListDictionary declarative_security;

		// The accessor are created event if they are not wanted.
		// But we need them because their names are reserved.
		// Field says whether accessor will be emited or not
		public readonly bool IsDummy;

		protected readonly string prefix;

		ReturnParameter return_attributes;

		public AbstractPropertyEventMethod (MemberBase member, string prefix)
			: base (member.Parent, SetupName (prefix, member, member.Location), null)
		{
			this.prefix = prefix;
			IsDummy = true;
		}

		public AbstractPropertyEventMethod (MemberBase member, Accessor accessor,
						    string prefix)
			: base (member.Parent, SetupName (prefix, member, accessor.Location),
				accessor.Attributes)
		{
			this.prefix = prefix;
			this.block = accessor.Block;
		}

		static MemberName SetupName (string prefix, MemberBase member, Location loc)
		{
			return new MemberName (member.MemberName.Left, prefix + member.ShortName, loc);
		}

		public void UpdateName (MemberBase member)
		{
			SetMemberName (SetupName (prefix, member, Location));
		}

		#region IMethodData Members

		public ToplevelBlock Block {
			get {
				return block;
			}

			set {
				block = value;
			}
		}

		public CallingConventions CallingConventions {
			get {
				return CallingConventions.Standard;
			}
		}

		public bool IsExcluded (EmitContext ec)
		{
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return null;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		public Type[] ParameterTypes { 
			get {
				return ParameterInfo.Types;
			}
		}

		public abstract Parameters ParameterInfo { get ; }
		public abstract Type ReturnType { get; }
		public abstract EmitContext CreateEmitContext(TypeContainer tc, ILGenerator ig);

		#endregion

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.cls_compliant_attribute_type || a.Type == TypeManager.obsolete_attribute_type ||
					a.Type == TypeManager.conditional_attribute_type) {
				Report.Error (1667, a.Location,
					"Attribute `{0}' is not valid on property or event accessors. It is valid on `{1}' declarations only",
					TypeManager.CSharpName (a.Type), a.GetValidTargets ());
				return;
			}

			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Target == AttributeTargets.Method) {
				method_data.MethodBuilder.SetCustomAttribute (cb);
				return;
			}

			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (method_data.MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb);
				return;
			}

			ApplyToExtraTarget (a, cb);
		}

		virtual protected void ApplyToExtraTarget (Attribute a, CustomAttributeBuilder cb)
		{
			System.Diagnostics.Debug.Fail ("You forgot to define special attribute target handling");
		}

		public override bool Define()
		{
			throw new NotSupportedException ();
		}

		public virtual void Emit (TypeContainer container)
		{
			EmitMethod (container);

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					method_data.MethodBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}

			block = null;
		}

		protected virtual void EmitMethod (TypeContainer container)
		{
			method_data.Emit (container, this);
		}

		public override bool IsClsComplianceRequired(DeclSpace ds)
		{
			return false;
		}

		public bool IsDuplicateImplementation (MethodCore method)
		{
			if (!MemberName.Equals (method.MemberName))
				return false;

			Type[] param_types = method.ParameterTypes;

			if (param_types.Length != ParameterTypes.Length)
				return false;

			for (int i = 0; i < param_types.Length; i++)
				if (param_types [i] != ParameterTypes [i])
					return false;

			Report.SymbolRelatedToPreviousError (method);
			Report.Error (111, Location, TypeContainer.Error111, method.GetSignatureForError ());
			return true;
		}

		public override bool IsUsed
		{
			get {
				if (IsDummy)
					return false;

				return base.IsUsed;
			}
		}

		public new Location Location { 
			get {
				return base.Location;
			}
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { throw new InvalidOperationException ("Unexpected attempt to get doc comment from " + this.GetType () + "."); }
		}

	}

	//
	// Properties and Indexers both generate PropertyBuilders, we use this to share 
	// their common bits.
	//
	abstract public class PropertyBase : MethodCore {

		public class GetMethod : PropertyMethod
		{
			static string[] attribute_targets = new string [] { "method", "return" };

			public GetMethod (MethodCore method):
				base (method, "get_")
			{
			}

			public GetMethod (MethodCore method, Accessor accessor):
				base (method, accessor, "get_")
			{
			}

			public override MethodBuilder Define(TypeContainer container)
			{
				base.Define (container);
				
				method_data = new MethodData (method, ModFlags, flags, this);

				if (!method_data.Define (container))
					return null;

				return method_data.MethodBuilder;
			}

			public override Type ReturnType {
				get {
					return method.MemberType;
				}
			}

			public override Parameters ParameterInfo {
				get {
					return Parameters.EmptyReadOnlyParameters;
				}
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}

		public class SetMethod : PropertyMethod {

			static string[] attribute_targets = new string [] { "method", "param", "return" };
                       ImplicitParameter param_attr;
			protected Parameters parameters;

			public SetMethod (MethodCore method):
				base (method, "set_")
			{
			}

			public SetMethod (MethodCore method, Accessor accessor):
				base (method, accessor, "set_")
			{
			}

			protected override void ApplyToExtraTarget(Attribute a, CustomAttributeBuilder cb)
			{
				if (a.Target == AttributeTargets.Parameter) {
					if (param_attr == null)
						param_attr = new ImplicitParameter (method_data.MethodBuilder, method.Location);

					param_attr.ApplyAttributeBuilder (a, cb);
					return;
				}

				base.ApplyAttributeBuilder (a, cb);
			}

			public override Parameters ParameterInfo {
				get {
					return parameters;
				}
			}

			protected virtual void DefineParameters ()
			{
				Parameter [] parms = new Parameter [1];
				parms [0] = new Parameter (method.MemberType, "value", Parameter.Modifier.NONE, null, Location);
				parameters = new Parameters (parms);
				parameters.Resolve (null);
			}

			public override MethodBuilder Define (TypeContainer container)
			{
				if (container.EmitContext == null)
					throw new InternalErrorException ("SetMethod.Define called too early");

				DefineParameters ();
				if (IsDummy)
					return null;

				base.Define (container);

				method_data = new MethodData (method, ModFlags, flags, this);

				if (!method_data.Define (container))
					return null;

				return method_data.MethodBuilder;
			}

			public override Type ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}

		static string[] attribute_targets = new string [] { "property" };

		public abstract class PropertyMethod : AbstractPropertyEventMethod {
			protected readonly MethodCore method;
			protected MethodAttributes flags;
			bool yields;

			public PropertyMethod (MethodCore method, string prefix)
				: base (method, prefix)
			{
				this.method = method;
			}

			public PropertyMethod (MethodCore method, Accessor accessor, string prefix)
				: base (method, accessor, prefix)
			{
				this.method = method;
				this.ModFlags = accessor.ModFlags;
				yields = accessor.Yields;

				if (accessor.ModFlags != 0 && RootContext.Version == LanguageVersion.ISO_1) {
					Report.FeatureIsNotStandardized (Location, "access modifiers on properties");
				}
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsComplianceRequired(DeclSpace ds)
			{
				return method.IsClsComplianceRequired (ds);
			}

			public virtual MethodBuilder Define (TypeContainer container)
			{
				if (!method.CheckAbstractAndExtern (block != null))
					return null;

				//
				// Check for custom access modifier
				//
				if (ModFlags == 0) {
					ModFlags = method.ModFlags;
					flags = method.flags;
				} else {
					if (container.Kind == Kind.Interface)
						Report.Error (275, Location, "`{0}': accessibility modifiers may not be used on accessors in an interface",
							GetSignatureForError ());

					if ((method.ModFlags & Modifiers.ABSTRACT) != 0 && (ModFlags & Modifiers.PRIVATE) != 0) {
						Report.Error (442, Location, "`{0}': abstract properties cannot have private accessors", GetSignatureForError ());
					}

					CheckModifiers (container, ModFlags);
					ModFlags |= (method.ModFlags & (~Modifiers.Accessibility));
					ModFlags |= Modifiers.PROPERTY_CUSTOM;
					flags = Modifiers.MethodAttr (ModFlags);
					flags |= (method.flags & (~MethodAttributes.MemberAccessMask));
				}

				//
				// Setup iterator if we are one
				//
				if (yields) {
					Iterator iterator = new Iterator (this, Parent as TypeContainer, null, ModFlags);
					
					if (!iterator.DefineIterator ())
						return null;
				}

				return null;
			}

			public bool HasCustomAccessModifier
			{
				get {
					return (ModFlags & Modifiers.PROPERTY_CUSTOM) != 0;
				}
			}

			public override EmitContext CreateEmitContext (TypeContainer tc,
								       ILGenerator ig)
			{
				return new EmitContext (
					tc, method.ds, method.Location, ig, ReturnType,
					method.ModFlags, false);
			}

			public override ObsoleteAttribute GetObsoleteAttribute ()
			{
				return method.GetObsoleteAttribute ();
			}

			public override string GetSignatureForError()
  			{
				return method.GetSignatureForError () + '.' + prefix.Substring (0, 3);
  			}

			void CheckModifiers (TypeContainer container, int modflags)
			{
				int flags = 0;
				int mflags = method.ModFlags & Modifiers.Accessibility;

				if ((mflags & Modifiers.PUBLIC) != 0) {
					flags |= Modifiers.PROTECTED | Modifiers.INTERNAL | Modifiers.PRIVATE;
				}
				else if ((mflags & Modifiers.PROTECTED) != 0) {
					if ((mflags & Modifiers.INTERNAL) != 0)
						flags |= Modifiers.PROTECTED | Modifiers.INTERNAL;

					flags |= Modifiers.PRIVATE;
				}
				else if ((mflags & Modifiers.INTERNAL) != 0)
					flags |= Modifiers.PRIVATE;

				if ((mflags == modflags) || (modflags & (~flags)) != 0) {
					Report.Error (273, Location,
						"The accessibility modifier of the `{0}' accessor must be more restrictive than the modifier of the property or indexer `{1}'",
						GetSignatureForError (), method.GetSignatureForError ());
				}
			}

			public override bool MarkForDuplicationCheck ()
			{
				caching_flags |= Flags.TestMethodDuplication;
				return true;
			}
		}

		public PropertyMethod Get, Set;
		public PropertyBuilder PropertyBuilder;
		public MethodBuilder GetBuilder, SetBuilder;

		protected EmitContext ec;

		public PropertyBase (TypeContainer parent, Expression type, int mod_flags,
				     int allowed_mod, bool is_iface, MemberName name,
				     Parameters parameters, Attributes attrs)
			: base (parent, null, type, mod_flags, allowed_mod, is_iface, name,
				attrs, parameters)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type)) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			PropertyBuilder.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Property;
			}
		}

		public override bool Define ()
		{
			if (!DoDefine ())
				return false;

			if (!IsTypePermitted ())
				return false;

			return true;
		}

		protected override bool DoDefine ()
		{
			if (!base.DoDefine ())
				return false;

			//
			// Accessors modifiers check
			//
			if (Get.ModFlags != 0 && Set.ModFlags != 0) {
				Report.Error (274, Location, "`{0}': Cannot specify accessibility modifiers for both accessors of the property or indexer",
						GetSignatureForError ());
				return false;
			}

			if ((Get.IsDummy || Set.IsDummy)
					&& (Get.ModFlags != 0 || Set.ModFlags != 0) && (ModFlags & Modifiers.OVERRIDE) == 0) {
				Report.Error (276, Location, 
					"`{0}': accessibility modifiers on accessors may only be used if the property or indexer has both a get and a set accessor",
					GetSignatureForError ());
				return false;
			}

			if (MemberType.IsAbstract && MemberType.IsSealed) {
				Report.Error (722, Location, Error722, TypeManager.CSharpName (MemberType));
				return false;
			}

			ec = new EmitContext (Parent, Location, null, MemberType, ModFlags);
			return true;
		}

		protected override bool CheckForDuplications ()
		{
			ArrayList ar = Parent.Indexers;
			if (ar != null) {
				int arLen = ar.Count;
					
				for (int i = 0; i < arLen; i++) {
					Indexer m = (Indexer) ar [i];
					if (IsDuplicateImplementation (m))
						return false;
				}
			}

			ar = Parent.Properties;
			if (ar != null) {
				int arLen = ar.Count;
					
				for (int i = 0; i < arLen; i++) {
					Property m = (Property) ar [i];
					if (IsDuplicateImplementation (m))
						return false;
				}
			}

			return true;
		}

		// TODO: rename to Resolve......
 		protected override MethodInfo FindOutBaseMethod (TypeContainer container, ref Type base_ret_type)
 		{
 			PropertyInfo base_property = container.BaseCache.FindMemberToOverride (
 				container.TypeBuilder, Name, ParameterTypes, null, true) as PropertyInfo;

 			if (base_property == null)
 				return null;

 			base_ret_type = base_property.PropertyType;
			MethodInfo get_accessor = base_property.GetGetMethod (true);
			MethodInfo set_accessor = base_property.GetSetMethod (true);
			MethodAttributes get_accessor_access, set_accessor_access;

			if ((ModFlags & Modifiers.OVERRIDE) != 0) {
				if (Get != null && !Get.IsDummy && get_accessor == null) {
					Report.SymbolRelatedToPreviousError (base_property);
					Report.Error (545, Location, "`{0}.get': cannot override because `{1}' does not have an overridable get accessor", GetSignatureForError (), TypeManager.GetFullNameSignature (base_property));
				}

				if (Set != null && !Set.IsDummy && set_accessor == null) {
					Report.SymbolRelatedToPreviousError (base_property);
					Report.Error (546, Location, "`{0}.set': cannot override because `{1}' does not have an overridable set accessor", GetSignatureForError (), TypeManager.GetFullNameSignature (base_property));
				}
			}
			
			//
			// Check base class accessors access
			//

			// TODO: rewrite to reuse Get|Set.CheckAccessModifiers and share code there
			get_accessor_access = set_accessor_access = 0;
			if ((ModFlags & Modifiers.NEW) == 0) {
				if (get_accessor != null) {
					MethodAttributes get_flags = Modifiers.MethodAttr (Get.ModFlags != 0 ? Get.ModFlags : ModFlags);
					get_accessor_access = (get_accessor.Attributes & MethodAttributes.MemberAccessMask);

					if (!Get.IsDummy && !CheckAccessModifiers (get_flags & MethodAttributes.MemberAccessMask, get_accessor_access, get_accessor))
						Error_CannotChangeAccessModifiers (get_accessor, get_accessor_access,  ".get");
				}

				if (set_accessor != null)  {
					MethodAttributes set_flags = Modifiers.MethodAttr (Set.ModFlags != 0 ? Set.ModFlags : ModFlags);
					set_accessor_access = (set_accessor.Attributes & MethodAttributes.MemberAccessMask);

					if (!Set.IsDummy && !CheckAccessModifiers (set_flags & MethodAttributes.MemberAccessMask, set_accessor_access, set_accessor))
						Error_CannotChangeAccessModifiers (set_accessor, set_accessor_access, ".set");
				}
			}

			//
			// Get the less restrictive access
			//
			return get_accessor_access > set_accessor_access ? get_accessor : set_accessor;
		}

		public override void Emit ()
		{
			//
			// The PropertyBuilder can be null for explicit implementations, in that
			// case, we do not actually emit the ".property", so there is nowhere to
			// put the attribute
			//
			if (PropertyBuilder != null && OptAttributes != null)
				OptAttributes.Emit (ec, this);

			if (!Get.IsDummy)
				Get.Emit (Parent);

			if (!Set.IsDummy)
				Set.Emit (Parent);

			base.Emit ();
		}

		/// <summary>
		/// Tests whether accessors are not in collision with some method (CS0111)
		/// </summary>
		public bool AreAccessorsDuplicateImplementation (MethodCore mc)
		{
			return Get.IsDuplicateImplementation (mc) || Set.IsDuplicateImplementation (mc);
		}

		public override bool IsUsed
		{
			get {
				if (IsExplicitImpl)
					return true;

				return Get.IsUsed | Set.IsUsed;
			}
		}

		protected override void SetMemberName (MemberName new_name)
		{
			base.SetMemberName (new_name);

			Get.UpdateName (this);
			Set.UpdateName (this);
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "P:"; }
		}
	}
			
	public class Property : PropertyBase {
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.ABSTRACT |
		        Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.METHOD_YIELDS |
			Modifiers.VIRTUAL;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		public Property (TypeContainer ds, Expression type, int mod, bool is_iface,
				 MemberName name, Attributes attrs, Accessor get_block,
				 Accessor set_block)
			: base (ds, type, mod,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				is_iface, name, Parameters.EmptyReadOnlyParameters, attrs)
		{
			if (get_block == null)
				Get = new GetMethod (this);
			else
				Get = new GetMethod (this, get_block);

			if (set_block == null)
				Set = new SetMethod (this);
			else
				Set = new SetMethod (this, set_block);
		}

		public override bool Define ()
		{
			if (!DoDefineBase ())
				return false;

			if (!base.Define ())
				return false;

			if (!CheckBase ())
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;

			if (!Get.IsDummy) {
				GetBuilder = Get.Define (Parent);
				if (GetBuilder == null)
					return false;
			}

			SetBuilder = Set.Define (Parent);
			if (!Set.IsDummy) {
				if (SetBuilder == null)
					return false;
			}

			// FIXME - PropertyAttributes.HasDefault ?

			PropertyBuilder = Parent.TypeBuilder.DefineProperty (
				MemberName.ToString (), PropertyAttributes.None, MemberType, null);

			if (!Get.IsDummy)
				PropertyBuilder.SetGetMethod (GetBuilder);

			if (!Set.IsDummy)
				PropertyBuilder.SetSetMethod (SetBuilder);

			return true;
		}
	}

	/// </summary>
	///  Gigantic workaround  for lameness in SRE follows :
	///  This class derives from EventInfo and attempts to basically
	///  wrap around the EventBuilder so that FindMembers can quickly
	///  return this in it search for members
	/// </summary>
	public class MyEventBuilder : EventInfo {
		
		//
		// We use this to "point" to our Builder which is
		// not really a MemberInfo
		//
		EventBuilder MyBuilder;
		
		//
		// We "catch" and wrap these methods
		//
		MethodInfo raise, remove, add;

		EventAttributes attributes;
		Type declaring_type, reflected_type, event_type;
		string name;

		Event my_event;

		public MyEventBuilder (Event ev, TypeBuilder type_builder, string name, EventAttributes event_attr, Type event_type)
		{
			MyBuilder = type_builder.DefineEvent (name, event_attr, event_type);

			// And now store the values in our own fields.
			
			declaring_type = type_builder;

			reflected_type = type_builder;
			
			attributes = event_attr;
			this.name = name;
			my_event = ev;
			this.event_type = event_type;
		}
		
		//
		// Methods that you have to override.  Note that you only need 
		// to "implement" the variants that take the argument (those are
		// the "abstract" methods, the others (GetAddMethod()) are 
		// regular.
		//
		public override MethodInfo GetAddMethod (bool nonPublic)
		{
			return add;
		}
		
		public override MethodInfo GetRemoveMethod (bool nonPublic)
		{
			return remove;
		}
		
		public override MethodInfo GetRaiseMethod (bool nonPublic)
		{
			return raise;
		}
		
		//
		// These methods make "MyEventInfo" look like a Builder
		//
		public void SetRaiseMethod (MethodBuilder raiseMethod)
		{
			raise = raiseMethod;
			MyBuilder.SetRaiseMethod (raiseMethod);
		}

		public void SetRemoveOnMethod (MethodBuilder removeMethod)
		{
			remove = removeMethod;
			MyBuilder.SetRemoveOnMethod (removeMethod);
		}

		public void SetAddOnMethod (MethodBuilder addMethod)
		{
			add = addMethod;
			MyBuilder.SetAddOnMethod (addMethod);
		}

		public void SetCustomAttribute (CustomAttributeBuilder cb)
		{
			MyBuilder.SetCustomAttribute (cb);
		}
		
		public override object [] GetCustomAttributes (bool inherit)
		{
			// FIXME : There's nothing which can be seemingly done here because
			// we have no way of getting at the custom attribute objects of the
			// EventBuilder !
			return null;
		}

		public override object [] GetCustomAttributes (Type t, bool inherit)
		{
			// FIXME : Same here !
			return null;
		}

		public override bool IsDefined (Type t, bool b)
		{
			return true;
		}

		public override EventAttributes Attributes {
			get {
				return attributes;
			}
		}

		public override string Name {
			get {
				return name;
			}
		}

		public override Type DeclaringType {
			get {
				return declaring_type;
			}
		}

		public override Type ReflectedType {
			get {
				return reflected_type;
			}
		}

		public Type EventType {
			get {
				return event_type;
			}
		}
		
		public void SetUsed ()
		{
			if (my_event != null) {
				my_event.SetAssigned ();
				my_event.SetMemberIsUsed ();
			}
		}
	}
	
	/// <summary>
	/// For case when event is declared like property (with add and remove accessors).
	/// </summary>
	public class EventProperty: Event {

		static string[] attribute_targets = new string [] { "event" }; // "property" target was disabled for 2.0 version

		public EventProperty (TypeContainer parent, Expression type, int mod_flags,
				      bool is_iface, MemberName name,
				      Attributes attrs, Accessor add, Accessor remove)
			: base (parent, type, mod_flags, is_iface, name, attrs)
		{
			Add = new AddDelegateMethod (this, add);
			Remove = new RemoveDelegateMethod (this, remove);

			// For this event syntax we don't report error CS0067
			// because it is hard to do it.
			SetAssigned ();
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}

	/// <summary>
	/// Event is declared like field.
	/// </summary>
	public class EventField : Event {

		static string[] attribute_targets = new string [] { "event", "field", "method" };
		static string[] attribute_targets_interface = new string[] { "event", "method" };

		public EventField (TypeContainer parent, Expression type, int mod_flags,
				   bool is_iface, MemberName name,
				   Attributes attrs)
			: base (parent, type, mod_flags, is_iface, name, attrs)
		{
			Add = new AddDelegateMethod (this);
			Remove = new RemoveDelegateMethod (this);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Target == AttributeTargets.Field) {
				FieldBuilder.SetCustomAttribute (cb);
				return;
			}

			if (a.Target == AttributeTargets.Method) {
				Add.ApplyAttributeBuilder (a, cb);
				Remove.ApplyAttributeBuilder (a, cb);
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}

		public override bool Define()
		{
			if (!base.Define ())
				return false;

			if (initializer != null) {
				if (((ModFlags & Modifiers.ABSTRACT) != 0)) {
					Report.Error (74, Location, "`{0}': abstract event cannot have an initializer",
						GetSignatureForError ());
					return false;
				}
			}

			return true;
		}

		public override string[] ValidAttributeTargets {
			get {
				return IsInterface ? attribute_targets_interface : attribute_targets;
			}
		}
	}

	public abstract class Event : FieldBase {

		protected sealed class AddDelegateMethod: DelegateMethod
		{

			public AddDelegateMethod (Event method):
				base (method, "add_")
			{
			}

			public AddDelegateMethod (Event method, Accessor accessor):
				base (method, accessor, "add_")
			{
			}

			protected override MethodInfo DelegateMethodInfo {
				get {
					return TypeManager.delegate_combine_delegate_delegate;
				}
			}

		}

		protected sealed class RemoveDelegateMethod: DelegateMethod
		{
			public RemoveDelegateMethod (Event method):
				base (method, "remove_")
			{
			}

			public RemoveDelegateMethod (Event method, Accessor accessor):
				base (method, accessor, "remove_")
			{
			}

			protected override MethodInfo DelegateMethodInfo {
				get {
					return TypeManager.delegate_remove_delegate_delegate;
				}
			}

		}

		public abstract class DelegateMethod: AbstractPropertyEventMethod
		{
			protected readonly Event method;
                       ImplicitParameter param_attr;

			static string[] attribute_targets = new string [] { "method", "param", "return" };

			public DelegateMethod (Event method, string prefix)
				: base (method, prefix)
			{
				this.method = method;
			}

			public DelegateMethod (Event method, Accessor accessor, string prefix)
				: base (method, accessor, prefix)
			{
				this.method = method;
			}

			protected override void ApplyToExtraTarget(Attribute a, CustomAttributeBuilder cb)
			{
				if (a.Target == AttributeTargets.Parameter) {
					if (param_attr == null)
						param_attr = new ImplicitParameter (method_data.MethodBuilder, method.Location);

					param_attr.ApplyAttributeBuilder (a, cb);
					return;
				}

				base.ApplyAttributeBuilder (a, cb);
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsComplianceRequired(DeclSpace ds)
			{
				return method.IsClsComplianceRequired (ds);
			}

			public MethodBuilder Define (TypeContainer container)
			{
				method_data = new MethodData (method, method.ModFlags,
					method.flags | MethodAttributes.HideBySig | MethodAttributes.SpecialName, this);

				if (!method_data.Define (container))
					return null;

				MethodBuilder mb = method_data.MethodBuilder;
				ParameterInfo.ApplyAttributes (Parent.EmitContext, mb);
				return mb;
			}


			protected override void EmitMethod (TypeContainer tc)
			{
				if (block != null) {
					base.EmitMethod (tc);
					return;
				}

				if ((method.ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
					return;

				ILGenerator ig = method_data.MethodBuilder.GetILGenerator ();
				FieldInfo field_info = (FieldInfo)method.FieldBuilder;

				method_data.MethodBuilder.SetImplementationFlags (MethodImplAttributes.Synchronized);
				if ((method.ModFlags & Modifiers.STATIC) != 0) {
					ig.Emit (OpCodes.Ldsfld, field_info);
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Call, DelegateMethodInfo);
					ig.Emit (OpCodes.Castclass, method.MemberType);
					ig.Emit (OpCodes.Stsfld, field_info);
				} else {
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldfld, field_info);
					ig.Emit (OpCodes.Ldarg_1);
					ig.Emit (OpCodes.Call, DelegateMethodInfo);
					ig.Emit (OpCodes.Castclass, method.MemberType);
					ig.Emit (OpCodes.Stfld, field_info);
				}
				ig.Emit (OpCodes.Ret);
			}

			protected abstract MethodInfo DelegateMethodInfo { get; }

			public override Type ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			public override EmitContext CreateEmitContext (TypeContainer tc,
								       ILGenerator ig)
			{
				return new EmitContext (
					tc, method.Parent, Location, ig, ReturnType,
					method.ModFlags, false);
			}

			public override ObsoleteAttribute GetObsoleteAttribute ()
			{
				return method.GetObsoleteAttribute ();
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}

			public override Parameters ParameterInfo {
				get {
					return method.parameters;
				}
			}

		}


		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.UNSAFE |
			Modifiers.ABSTRACT;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		public DelegateMethod Add, Remove;
		public MyEventBuilder     EventBuilder;
		public MethodBuilder AddBuilder, RemoveBuilder;
		Parameters parameters;

		protected Event (TypeContainer parent, Expression type, int mod_flags,
			      bool is_iface, MemberName name, Attributes attrs)
			: base (parent, type, mod_flags,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, attrs)
		{
			IsInterface = is_iface;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type)) {
				a.Error_InvalidSecurityParent ();
				return;
			}
			
			EventBuilder.SetCustomAttribute (cb);
		}

		public bool AreAccessorsDuplicateImplementation (MethodCore mc)
		{
			return Add.IsDuplicateImplementation (mc) || Remove.IsDuplicateImplementation (mc);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Event;
			}
		}
  
		public override bool Define ()
		{
			EventAttributes e_attr;
			e_attr = EventAttributes.None;

			if (!DoDefineBase ())
				return false;

			if (!DoDefine ())
				return false;

			if (!TypeManager.IsDelegateType (MemberType)) {
				Report.Error (66, Location, "`{0}': event must be of a delegate type", GetSignatureForError ());
				return false;
			}

			EmitContext ec = Parent.EmitContext;
			if (ec == null)
				throw new InternalErrorException ("Event.Define called too early?");
			bool old_unsafe = ec.InUnsafe;
			ec.InUnsafe = InUnsafe;

			Parameter [] parms = new Parameter [1];
			parms [0] = new Parameter (MemberType, "value", Parameter.Modifier.NONE, null, Location);
			parameters = new Parameters (parms);
			parameters.Resolve (null);

			ec.InUnsafe = old_unsafe;

			if (!CheckBase ())
				return false;

			//
			// Now define the accessors
			//

			AddBuilder = Add.Define (Parent);
			if (AddBuilder == null)
				return false;

			RemoveBuilder = Remove.Define (Parent);
			if (RemoveBuilder == null)
				return false;

			EventBuilder = new MyEventBuilder (this, Parent.TypeBuilder, Name, e_attr, MemberType);
					
			if (Add.Block == null && Remove.Block == null && !IsInterface) {
					FieldBuilder = Parent.TypeBuilder.DefineField (
						Name, MemberType,
						FieldAttributes.Private | ((ModFlags & Modifiers.STATIC) != 0 ? FieldAttributes.Static : 0));
					TypeManager.RegisterPrivateFieldOfEvent (
						(EventInfo) EventBuilder, FieldBuilder);
					TypeManager.RegisterFieldBase (FieldBuilder, this);
				}
			
				EventBuilder.SetAddOnMethod (AddBuilder);
				EventBuilder.SetRemoveOnMethod (RemoveBuilder);

				TypeManager.RegisterEvent (EventBuilder, AddBuilder, RemoveBuilder);
			return true;
		}

 		protected override bool CheckBase ()
 		{
 			if (!base.CheckBase ())
 				return false;
 
 			if (conflict_symbol != null && (ModFlags & Modifiers.NEW) == 0) {
 				if (!(conflict_symbol is EventInfo)) {
 					Report.SymbolRelatedToPreviousError (conflict_symbol);
 					Report.Error (72, Location, "Event `{0}' can override only event", GetSignatureForError ());
 					return false;
 				}
 			}
 
 			return true;
 		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				EmitContext ec = new EmitContext (
					Parent, Location, null, MemberType, ModFlags);
				OptAttributes.Emit (ec, this);
			}

			Add.Emit (Parent);
			Remove.Emit (Parent);

			base.Emit ();
		}

		public override string GetSignatureForError ()
		{
			return base.GetSignatureForError ();
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "E:"; }
		}
	}

 
	public class Indexer : PropertyBase, IIteratorContainer {

		class GetIndexerMethod : GetMethod
		{
			public GetIndexerMethod (MethodCore method):
				base (method)
			{
			}

			public GetIndexerMethod (MethodCore method, Accessor accessor):
				base (method, accessor)
			{
			}

			public override Parameters ParameterInfo {
				get {
					return method.ParameterInfo;
				}
			}
		}

		class SetIndexerMethod: SetMethod
		{
			public SetIndexerMethod (MethodCore method):
				base (method)
			{
			}

			public SetIndexerMethod (MethodCore method, Accessor accessor):
				base (method, accessor)
			{
			}

			protected override void DefineParameters ()
			{
				Parameter [] fixed_parms = method.Parameters.FixedParameters;
				Parameter [] tmp = new Parameter [fixed_parms.Length + 1];

				fixed_parms.CopyTo (tmp, 0);
				tmp [fixed_parms.Length] = new Parameter (
					method.MemberType, "value", Parameter.Modifier.NONE, null, method.Location);

				parameters = new Parameters (tmp);
				parameters.Resolve (null);
			}
		}

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.ABSTRACT;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		//
		// Are we implementing an interface ?
		//
		public Indexer (TypeContainer parent, Expression type, MemberName name, int mod,
				bool is_iface, Parameters parameters, Attributes attrs,
				Accessor get_block, Accessor set_block)
			: base (parent, type, mod,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				is_iface, name, parameters, attrs)
		{
			if (get_block == null)
				Get = new GetIndexerMethod (this);
			else
				Get = new GetIndexerMethod (this, get_block);

			if (set_block == null)
				Set = new SetIndexerMethod (this);
			else
				Set = new SetIndexerMethod (this, set_block);
		}

		public override bool Define ()
		{
			if (!DoDefineBase ())
				return false;

			if (!base.Define ())
				return false;

			if (MemberType == TypeManager.void_type) {
				Report.Error (620, Location, "Indexers cannot have void type");
				return false;
			}

			if (OptAttributes != null) {
				Attribute indexer_attr = OptAttributes.Search (TypeManager.indexer_name_type, ec);
				if (indexer_attr != null) {
					// Remove the attribute from the list because it is not emitted
					OptAttributes.Attrs.Remove (indexer_attr);

					ShortName = indexer_attr.GetIndexerAttributeValue (ec);

					if (IsExplicitImpl) {
						Report.Error (415, indexer_attr.Location,
							      "The `IndexerName' attribute is valid only on an " +
							      "indexer that is not an explicit interface member declaration");
						return false;
					}
				
					if ((ModFlags & Modifiers.OVERRIDE) != 0) {
						Report.Error (609, indexer_attr.Location,
							      "Cannot set the `IndexerName' attribute on an indexer marked override");
						return false;
					}

					if (!Tokenizer.IsValidIdentifier (ShortName)) {
						Report.Error (633, indexer_attr.Location,
							      "The argument to the `IndexerName' attribute must be a valid identifier");
						return false;
					}
				}
			}

			if (InterfaceType != null) {
				string base_IndexerName = TypeManager.IndexerPropertyName (InterfaceType);
				if (base_IndexerName != Name)
					ShortName = base_IndexerName;
			}

			if (!Parent.AddToMemberContainer (this) ||
				!Parent.AddToMemberContainer (Get) || !Parent.AddToMemberContainer (Set))
				return false;

			if (!CheckBase ())
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			if (!Get.IsDummy){
				GetBuilder = Get.Define (Parent);
				if (GetBuilder == null)
					return false;

				//
				// Setup iterator if we are one
				//
				if ((ModFlags & Modifiers.METHOD_YIELDS) != 0){
					Iterator iterator = new Iterator (
						Get, Parent, null, ModFlags);

					if (!iterator.DefineIterator ())
						return false;
				}
			}
			
			SetBuilder = Set.Define (Parent);
			if (!Set.IsDummy){
				if (SetBuilder == null)
					return false;
			}

			//
			// Now name the parameters
			//
			Parameter [] p = Parameters.FixedParameters;
			if (p != null) {
				// TODO: should be done in parser and it needs to do cycle
				if ((p [0].ModFlags & Parameter.Modifier.ISBYREF) != 0) {
					Report.Error (631, Location, "ref and out are not valid in this context");
					return false;
				}
			}

				PropertyBuilder = Parent.TypeBuilder.DefineProperty (
				Name, PropertyAttributes.None, MemberType, ParameterTypes);

				if (!Get.IsDummy)
					PropertyBuilder.SetGetMethod (GetBuilder);

				if (!Set.IsDummy)
					PropertyBuilder.SetSetMethod (SetBuilder);
				
			TypeManager.RegisterIndexer (PropertyBuilder, GetBuilder, SetBuilder, ParameterTypes);

			return true;
		}

		public override string GetSignatureForError ()
		{
			StringBuilder sb = new StringBuilder (Parent.GetSignatureForError ());
			if (MemberName.Left != null) {
				sb.Append ('.');
				sb.Append (MemberName.Left);
			}

			sb.Append (".this");
			sb.Append (Parameters.GetSignatureForError ().Replace ('(', '[').Replace (')', ']'));
			return sb.ToString ();
		}

		public override bool MarkForDuplicationCheck ()
		{
			caching_flags |= Flags.TestMethodDuplication;
			return true;
		}
	}

	public class Operator : MethodCore, IIteratorContainer {

		const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.STATIC;

		public enum OpType : byte {

			// Unary operators
			LogicalNot,
			OnesComplement,
			Increment,
			Decrement,
			True,
			False,

			// Unary and Binary operators
			Addition,
			Subtraction,

			UnaryPlus,
			UnaryNegation,
			
			// Binary operators
			Multiply,
			Division,
			Modulus,
			BitwiseAnd,
			BitwiseOr,
			ExclusiveOr,
			LeftShift,
			RightShift,
			Equality,
			Inequality,
			GreaterThan,
			LessThan,
			GreaterThanOrEqual,
			LessThanOrEqual,

			// Implicit and Explicit
			Implicit,
			Explicit,

			// Just because of enum
			TOP
		};

		public readonly OpType OperatorType;
		public MethodBuilder   OperatorMethodBuilder;
		
		public Method OperatorMethod;

		static string[] attribute_targets = new string [] { "method", "return" };

		public Operator (TypeContainer parent, OpType type, Expression ret_type,
				 int mod_flags, Parameters parameters,
				 ToplevelBlock block, Attributes attrs, Location loc)
			: base (parent, null, ret_type, mod_flags, AllowedModifiers, false,
				new MemberName ("op_" + type, loc), attrs, parameters)
		{
			OperatorType = type;
			Block = block;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb) 
		{
			OperatorMethod.ApplyAttributeBuilder (a, cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Method; 
			}
		}
		
		protected override bool CheckForDuplications()
		{
			ArrayList ar = Parent.Operators;
			if (ar != null) {
				int arLen = ar.Count;

				for (int i = 0; i < arLen; i++) {
					Operator o = (Operator) ar [i];
					if (IsDuplicateImplementation (o))
						return false;
				}
			}

			ar = Parent.Methods;
			if (ar != null) {
				int arLen = ar.Count;

				for (int i = 0; i < arLen; i++) {
					Method m = (Method) ar [i];
					if (IsDuplicateImplementation (m))
						return false;
				}
			}

			return true;
		}

		public override bool Define ()
		{
			const int RequiredModifiers = Modifiers.PUBLIC | Modifiers.STATIC;
			if ((ModFlags & RequiredModifiers) != RequiredModifiers){
				Report.Error (558, Location, "User-defined operator `{0}' must be declared static and public", GetSignatureForError ());
				return false;
			}

			if (!DoDefine ())
				return false;

			if (MemberType == TypeManager.void_type) {
				Report.Error (590, Location, "User-defined operators cannot return void");
				return false;
			}

			OperatorMethod = new Method (
				Parent, null, Type, ModFlags, false, MemberName,
				Parameters, OptAttributes);

			OperatorMethod.Block = Block;
			OperatorMethod.IsOperator = this;			
			OperatorMethod.flags |= MethodAttributes.SpecialName | MethodAttributes.HideBySig;
			OperatorMethod.Define ();

			if (OperatorMethod.MethodBuilder == null)
				return false;
			
			OperatorMethodBuilder = OperatorMethod.MethodBuilder;

			Type[] parameter_types = OperatorMethod.ParameterTypes;
			Type declaring_type = OperatorMethod.MethodData.DeclaringType;
			Type return_type = OperatorMethod.ReturnType;
			Type first_arg_type = parameter_types [0];

			if (!CheckBase ())
				return false;

			// Rules for conversion operators
			
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit) {
				if (first_arg_type == return_type && first_arg_type == declaring_type){
					Report.Error (555, Location,
						"User-defined operator cannot take an object of the enclosing type and convert to an object of the enclosing type");
					return false;
				}
				
				if (first_arg_type != declaring_type && return_type != declaring_type){
					Report.Error (
						556, Location, 
						"User-defined conversion must convert to or from the " +
						"enclosing type");
					return false;
				}
				
				if (first_arg_type == TypeManager.object_type ||
				    return_type == TypeManager.object_type){
					Report.Error (
						-8, Location,
						"User-defined conversion cannot convert to or from " +
						"object type");
					return false;
				}

				if (first_arg_type.IsInterface || return_type.IsInterface){
					Report.Error (552, Location, "User-defined conversion `{0}' cannot convert to or from an interface type",
						GetSignatureForError ());
					return false;
				}
				
				if (first_arg_type.IsSubclassOf (return_type)
					|| return_type.IsSubclassOf (first_arg_type)){
					if (declaring_type.IsSubclassOf (return_type)) {
						Report.Error (553, Location, "User-defined conversion `{0}' cannot convert to or from base class",
							GetSignatureForError ());
						return false;
					}
					Report.Error (554, Location, "User-defined conversion `{0}' cannot convert to or from derived class",
						GetSignatureForError ());
					return false;
				}
			} else if (OperatorType == OpType.LeftShift || OperatorType == OpType.RightShift) {
				if (first_arg_type != declaring_type || parameter_types [1] != TypeManager.int32_type) {
					Report.Error (564, Location, "Overloaded shift operator must have the type of the first operand be the containing type, and the type of the second operand must be int");
					return false;
				}
			} else if (Parameters.Count == 1) {
				// Checks for Unary operators
				
				if (OperatorType == OpType.Increment || OperatorType == OpType.Decrement) {
					if (return_type != declaring_type && !return_type.IsSubclassOf (declaring_type)) {
						Report.Error (448, Location,
							"The return type for ++ or -- operator must be the containing type or derived from the containing type");
						return false;
					}
				if (first_arg_type != declaring_type){
					Report.Error (
							559, Location, "The parameter type for ++ or -- operator must be the containing type");
					return false;
				}
				}
				
				if (first_arg_type != declaring_type){
						Report.Error (
						562, Location,
						"The parameter of a unary operator must be the " +
						"containing type");
						return false;
				}
				
				if (OperatorType == OpType.True || OperatorType == OpType.False) {
					if (return_type != TypeManager.bool_type){
						Report.Error (
							215, Location,
							"The return type of operator True or False " +
							"must be bool");
						return false;
					}
				}
				
			} else {
				// Checks for Binary operators
				
				if (first_arg_type != declaring_type &&
				    parameter_types [1] != declaring_type){
					Report.Error (
						563, Location,
						"One of the parameters of a binary operator must " +
						"be the containing type");
					return false;
				}
			}

			return true;
		}
		
		public override void Emit ()
		{
			//
			// abstract or extern methods have no bodies
			//
			if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
				return;
			
			OperatorMethod.Emit ();
			Block = null;
		}

		// Operator cannot be override
		protected override MethodInfo FindOutBaseMethod (TypeContainer container, ref Type base_ret_type)
		{
			return null;
		}

		public static string GetName (OpType ot)
		{
			switch (ot){
			case OpType.LogicalNot:
				return "!";
			case OpType.OnesComplement:
				return "~";
			case OpType.Increment:
				return "++";
			case OpType.Decrement:
				return "--";
			case OpType.True:
				return "true";
			case OpType.False:
				return "false";
			case OpType.Addition:
				return "+";
			case OpType.Subtraction:
				return "-";
			case OpType.UnaryPlus:
				return "+";
			case OpType.UnaryNegation:
				return "-";
			case OpType.Multiply:
				return "*";
			case OpType.Division:
				return "/";
			case OpType.Modulus:
				return "%";
			case OpType.BitwiseAnd:
				return "&";
			case OpType.BitwiseOr:
				return "|";
			case OpType.ExclusiveOr:
				return "^";
			case OpType.LeftShift:
				return "<<";
			case OpType.RightShift:
				return ">>";
			case OpType.Equality:
				return "==";
			case OpType.Inequality:
				return "!=";
			case OpType.GreaterThan:
				return ">";
			case OpType.LessThan:
				return "<";
			case OpType.GreaterThanOrEqual:
				return ">=";
			case OpType.LessThanOrEqual:
				return "<=";
			case OpType.Implicit:
				return "implicit";
			case OpType.Explicit:
				return "explicit";
			default: return "";
			}
		}

		public static OpType GetOperatorType (string name)
		{
			if (name.StartsWith ("op_")){
				for (int i = 0; i < Unary.oper_names.Length; ++i) {
					if (Unary.oper_names [i] == name)
						return (OpType)i;
				}

				for (int i = 0; i < Binary.oper_names.Length; ++i) {
					if (Binary.oper_names [i] == name)
						return (OpType)i;
				}
			}
			return OpType.TOP;
		}

		public override string GetSignatureForError ()
		{
			StringBuilder sb = new StringBuilder ();
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit) {
				sb.AppendFormat ("{0}.{1} operator {2}", Parent.GetSignatureForError (), GetName (OperatorType), Type.Type == null ? Type.ToString () : TypeManager.CSharpName (Type.Type));
			}
			else {
				sb.AppendFormat ("{0}.operator {1}", Parent.GetSignatureForError (), GetName (OperatorType));
			}

			sb.Append (Parameters.GetSignatureForError ());
			return sb.ToString ();
		}
		
		public override bool MarkForDuplicationCheck ()
		{
			caching_flags |= Flags.TestMethodDuplication;
			return true;
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}

	//
	// This is used to compare method signatures
	//
	struct MethodSignature {
		public string Name;
		public Type RetType;
		public Type [] Parameters;
		
		/// <summary>
		///    This delegate is used to extract methods which have the
		///    same signature as the argument
		/// </summary>
		public static MemberFilter method_signature_filter = new MemberFilter (MemberSignatureCompare);
		
		public MethodSignature (string name, Type ret_type, Type [] parameters)
		{
			Name = name;
			RetType = ret_type;

			if (parameters == null)
				Parameters = TypeManager.NoTypes;
			else
				Parameters = parameters;
		}

		public override string ToString ()
		{
			string pars = "";
			if (Parameters.Length != 0){
				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				for (int i = 0; i < Parameters.Length; i++){
					sb.Append (Parameters [i]);
					if (i+1 < Parameters.Length)
						sb.Append (", ");
				}
				pars = sb.ToString ();
			}

			return String.Format ("{0} {1} ({2})", RetType, Name, pars);
		}
		
		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public override bool Equals (Object o)
		{
			MethodSignature other = (MethodSignature) o;

			if (other.Name != Name)
				return false;

			if (other.RetType != RetType)
				return false;
			
			if (Parameters == null){
				if (other.Parameters == null)
					return true;
				return false;
			}

			if (other.Parameters == null)
				return false;
			
			int c = Parameters.Length;
			if (other.Parameters.Length != c)
				return false;

			for (int i = 0; i < c; i++)
				if (other.Parameters [i] != Parameters [i])
					return false;

			return true;
		}

		static bool MemberSignatureCompare (MemberInfo m, object filter_criteria)
		{
			MethodSignature sig = (MethodSignature) filter_criteria;

			if (m.Name != sig.Name)
				return false;

			Type ReturnType;
			MethodInfo mi = m as MethodInfo;
			PropertyInfo pi = m as PropertyInfo;

			if (mi != null)
				ReturnType = mi.ReturnType;
			else if (pi != null)
				ReturnType = pi.PropertyType;
			else
				return false;
			
			//
			// we use sig.RetType == null to mean `do not check the
			// method return value.  
			//
			if (sig.RetType != null)
				if (ReturnType != sig.RetType)
					return false;

			Type [] args;
			if (mi != null)
				args = TypeManager.GetParameterData (mi).Types;
			else
				args = TypeManager.GetArgumentTypes (pi);
			Type [] sigp = sig.Parameters;

			if (args.Length != sigp.Length)
				return false;

			for (int i = args.Length; i > 0; ){
				i--;
				if (args [i] != sigp [i])
					return false;
			}
			return true;
		}
	}
}
