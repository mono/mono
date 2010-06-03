//
// property.cs: Property based handlers
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Martin Baulig (martin@ximian.com)
//          Marek Safar (marek.safar@seznam.cz)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004-2008 Novell, Inc
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

#if NET_2_1
using XmlElement = System.Object;
#else
using System.Xml;
#endif

using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	// It is used as a base class for all property based members
	// This includes properties, indexers, and events
	public abstract class PropertyBasedMember : InterfaceMemberBase
	{
		public PropertyBasedMember (DeclSpace parent, GenericMethod generic,
			FullNamedExpression type, Modifiers mod, Modifiers allowed_mod,
			MemberName name, Attributes attrs)
			: base (parent, generic, type, mod, allowed_mod, name, attrs)
		{
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (!MemberType.IsCLSCompliant ()) {
				Report.Warning (3003, 1, Location, "Type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}
			return true;
		}

	}

	//
	// `set' and `get' accessors are represented with an Accessor.
	// 
	public class Accessor {
		//
		// Null if the accessor is empty, or a Block if not
		//
		public const Modifiers AllowedModifiers = 
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;
		
		public ToplevelBlock Block;
		public Attributes Attributes;
		public Location Location;
		public Modifiers ModFlags;
		public ParametersCompiled Parameters;
		
		public Accessor (ToplevelBlock b, Modifiers mod, Attributes attrs, ParametersCompiled p, Location loc)
		{
			Block = b;
			Attributes = attrs;
			Location = loc;
			Parameters = p;
			ModFlags = ModifiersExtensions.Check (AllowedModifiers, mod, 0, loc, RootContext.ToplevelTypes.Compiler.Report);
		}
	}

	public class PropertySpec : MemberSpec, IInterfaceMemberSpec
	{
		PropertyInfo info;
		TypeSpec memberType;
		MethodSpec set, get;

		public PropertySpec (MemberKind kind, TypeSpec declaringType, IMemberDefinition definition, TypeSpec memberType, PropertyInfo info, Modifiers modifiers)
			: base (kind, declaringType, definition, modifiers)
		{
			this.info = info;
			this.memberType = memberType;
		}

		#region Properties

		public MethodSpec Get {
			get {
				return get;
			}
			set {
				get = value;
				get.IsAccessor = true;
			}
		}

		public MethodSpec Set { 
			get {
				return set;
			}
			set {
				set = value;
				set.IsAccessor = true;
			}
		}

		public bool IsNotRealProperty {
			get {
				return (state & StateFlags.IsNotRealProperty) != 0;
			}
			set {
				state |= StateFlags.IsNotRealProperty;
			}
		}

		public bool HasDifferentAccessibility {
			get {
				return HasGet && HasSet && 
					(Get.Modifiers & Modifiers.AccessibilityMask) != (Set.Modifiers & Modifiers.AccessibilityMask);
			}
		}

		public bool HasGet {
			get {
				return Get != null && Get.Kind != MemberKind.FakeMethod;
			}
		}

		public bool HasSet {
			get {
				return Set != null && Set.Kind != MemberKind.FakeMethod;
			}
		}

		public PropertyInfo MetaInfo {
			get {
				if ((state & StateFlags.PendingMetaInflate) != 0)
					throw new NotSupportedException ();

				return info;
			}
		}

		public TypeSpec MemberType {
			get {
				return memberType;
			}
		}

		#endregion

		public override MemberSpec InflateMember (TypeParameterInflator inflator)
		{
			var ps = (PropertySpec) base.InflateMember (inflator);
			ps.memberType = inflator.Inflate (memberType);
			return ps;
		}
	}

	//
	// Properties and Indexers both generate PropertyBuilders, we use this to share 
	// their common bits.
	//
	abstract public class PropertyBase : PropertyBasedMember {

		public class GetMethod : PropertyMethod
		{
			static string[] attribute_targets = new string [] { "method", "return" };

			public GetMethod (PropertyBase method):
				base (method, "get_")
			{
			}

			public GetMethod (PropertyBase method, Accessor accessor):
				base (method, accessor, "get_")
			{
			}

			public override MethodBuilder Define (DeclSpace parent)
			{
				base.Define (parent);

				Spec = new MethodSpec (IsDummy ? MemberKind.FakeMethod : MemberKind.Method, parent.PartialContainer.Definition, this, ReturnType, null, ParameterInfo, ModFlags);

				if (IsDummy)
					return null;
				
				method_data = new MethodData (method, ModFlags, flags, this);

				if (!method_data.Define (parent, method.GetFullName (MemberName), Report))
					return null;

				Spec.SetMetaInfo (method_data.MethodBuilder);

				return method_data.MethodBuilder;
			}

			public override TypeSpec ReturnType {
				get {
					return method.MemberType;
				}
			}

			public override ParametersCompiled ParameterInfo {
				get {
					return ParametersCompiled.EmptyReadOnlyParameters;
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
			protected ParametersCompiled parameters;

			public SetMethod (PropertyBase method) :
				base (method, "set_")
			{
				parameters = ParametersCompiled.CreateImplicitParameter (method.type_expr, Location);
			}

			public SetMethod (PropertyBase method, Accessor accessor):
				base (method, accessor, "set_")
			{
				this.parameters = accessor.Parameters;
			}

			protected override void ApplyToExtraTarget (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
			{
				if (a.Target == AttributeTargets.Parameter) {
					if (param_attr == null)
						param_attr = new ImplicitParameter (method_data.MethodBuilder);

					param_attr.ApplyAttributeBuilder (a, ctor, cdata, pa);
					return;
				}

				base.ApplyAttributeBuilder (a, ctor, cdata, pa);
			}

			public override ParametersCompiled ParameterInfo {
			    get {
			        return parameters;
			    }
			}

			public override MethodBuilder Define (DeclSpace parent)
			{
				parameters.Resolve (this);
				
				base.Define (parent);

				Spec = new MethodSpec (IsDummy ? MemberKind.FakeMethod : MemberKind.Method, parent.PartialContainer.Definition, this, ReturnType, null, ParameterInfo, ModFlags);

				if (IsDummy)
					return null;

				method_data = new MethodData (method, ModFlags, flags, this);

				if (!method_data.Define (parent, method.GetFullName (MemberName), Report))
					return null;

				Spec.SetMetaInfo (method_data.MethodBuilder);

				return method_data.MethodBuilder;
			}

			public override TypeSpec ReturnType {
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

		public abstract class PropertyMethod : AbstractPropertyEventMethod
		{
			protected readonly PropertyBase method;
			protected MethodAttributes flags;

			public PropertyMethod (PropertyBase method, string prefix)
				: base (method, prefix)
			{
				this.method = method;
				this.ModFlags = method.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE);				
			}

			public PropertyMethod (PropertyBase method, Accessor accessor,
					       string prefix)
				: base (method, accessor, prefix)
			{
				this.method = method;
				this.ModFlags = accessor.ModFlags | (method.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE));

				if (accessor.ModFlags != 0 && RootContext.Version == LanguageVersion.ISO_1) {
					Report.FeatureIsNotAvailable (Location, "access modifiers on properties");
				}
			}

			public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
			{
				if (a.IsInternalMethodImplAttribute) {
					method.is_external_implementation = true;
				}

				base.ApplyAttributeBuilder (a, ctor, cdata, pa);
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsComplianceRequired ()
			{
				return method.IsClsComplianceRequired ();
			}

			public virtual MethodBuilder Define (DeclSpace parent)
			{
				CheckForDuplications ();

				if (IsDummy) {
					if (method.InterfaceType != null && parent.PartialContainer.PendingImplementations != null) {
						var mi = parent.PartialContainer.PendingImplementations.IsInterfaceMethod (
							MethodName, method.InterfaceType, new MethodData (method, ModFlags, flags, this));
						if (mi != null) {
							Report.SymbolRelatedToPreviousError (mi);
							Report.Error (551, Location, "Explicit interface implementation `{0}' is missing accessor `{1}'",
								method.GetSignatureForError (), mi.GetSignatureForError ());
						}
					}
					return null;
				}

				TypeContainer container = parent.PartialContainer;

				//
				// Check for custom access modifier
				//
				if ((ModFlags & Modifiers.AccessibilityMask) == 0) {
					ModFlags |= method.ModFlags;
					flags = method.flags;
				} else {
					if (container.Kind == MemberKind.Interface)
						Report.Error (275, Location, "`{0}': accessibility modifiers may not be used on accessors in an interface",
							GetSignatureForError ());

					if ((method.ModFlags & Modifiers.ABSTRACT) != 0 && (ModFlags & Modifiers.PRIVATE) != 0) {
						Report.Error (442, Location, "`{0}': abstract properties cannot have private accessors", GetSignatureForError ());
					}

					CheckModifiers (ModFlags);
					ModFlags |= (method.ModFlags & (~Modifiers.AccessibilityMask));
					ModFlags |= Modifiers.PROPERTY_CUSTOM;
					flags = ModifiersExtensions.MethodAttr (ModFlags);
					flags |= (method.flags & (~MethodAttributes.MemberAccessMask));
				}

				CheckAbstractAndExtern (block != null);
				CheckProtectedModifier ();

				if (block != null && block.IsIterator)
					Iterator.CreateIterator (this, Parent.PartialContainer, ModFlags, Compiler);

				return null;
			}

			public bool HasCustomAccessModifier {
				get {
					return (ModFlags & Modifiers.PROPERTY_CUSTOM) != 0;
				}
			}

			public PropertyBase Property {
				get {
					return method;
				}
			}

			public override ObsoleteAttribute GetAttributeObsolete ()
			{
				return method.GetAttributeObsolete ();
			}

			public override string GetSignatureForError()
			{
				return method.GetSignatureForError () + "." + prefix.Substring (0, 3);
			}

			void CheckModifiers (Modifiers modflags)
			{
				if (!ModifiersExtensions.IsRestrictedModifier (modflags & Modifiers.AccessibilityMask, method.ModFlags & Modifiers.AccessibilityMask)) {
					Report.Error (273, Location,
						"The accessibility modifier of the `{0}' accessor must be more restrictive than the modifier of the property or indexer `{1}'",
						GetSignatureForError (), method.GetSignatureForError ());
				}
			}

			protected bool CheckForDuplications ()
			{
				if ((caching_flags & Flags.MethodOverloadsExist) == 0)
					return true;

				return Parent.MemberCache.CheckExistingMembersOverloads (this, ParameterInfo);
			}
		}

		public PropertyMethod Get, Set;
		public PropertyBuilder PropertyBuilder;
		public MethodBuilder GetBuilder, SetBuilder;

		protected bool define_set_first = false;

		public PropertyBase (DeclSpace parent, FullNamedExpression type, Modifiers mod_flags,
				     Modifiers allowed_mod, MemberName name,
				     Attributes attrs, bool define_set_first)
			: base (parent, null, type, mod_flags, allowed_mod, name, attrs)
		{
			 this.define_set_first = define_set_first;
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.HasSecurityAttribute) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			if (a.Type == pa.Dynamic) {
				a.Error_MisusedDynamicAttribute ();
				return;
			}

			PropertyBuilder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), cdata);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Property;
			}
		}

		protected override bool CheckOverrideAgainstBase (MemberSpec base_member)
		{
			var ok = base.CheckOverrideAgainstBase (base_member);

			//
			// Check base property accessors conflict
			//
			var base_prop = (PropertySpec) base_member;
			if (!Get.IsDummy) {
				if (!base_prop.HasGet) {
					Report.SymbolRelatedToPreviousError (base_prop);
					Report.Error (545, Get.Location,
						"`{0}': cannot override because `{1}' does not have an overridable get accessor",
						Get.GetSignatureForError (), base_prop.GetSignatureForError ());
					ok = false;
				} else if (Get.HasCustomAccessModifier || base_prop.HasDifferentAccessibility) {
					if (!CheckAccessModifiers (Get, base_prop.Get)) {
						Error_CannotChangeAccessModifiers (Get, base_prop.Get);
						ok = false;
					}
				}
			}

			if (!Set.IsDummy) {
				if (!base_prop.HasSet) {
					Report.SymbolRelatedToPreviousError (base_prop);
					Report.Error (546, Set.Location,
						"`{0}': cannot override because `{1}' does not have an overridable set accessor",
						Set.GetSignatureForError (), base_prop.GetSignatureForError ());
					ok = false;
				} else if (Set.HasCustomAccessModifier || base_prop.HasDifferentAccessibility) {
					if (!CheckAccessModifiers (Set, base_prop.Set)) {
						Error_CannotChangeAccessModifiers (Set, base_prop.Set);
						ok = false;
					}
				}
			}

			if (!Set.HasCustomAccessModifier && !Get.HasCustomAccessModifier) {
				if (!CheckAccessModifiers (this, base_prop)) {
					Error_CannotChangeAccessModifiers (this, base_prop);
					ok = false;
				}
			}

			return ok;
		}

		protected override void DoMemberTypeDependentChecks ()
		{
			base.DoMemberTypeDependentChecks ();

			IsTypePermitted ();

			if (MemberType.IsStatic)
				Error_StaticReturnType ();
		}

		protected override void DoMemberTypeIndependentChecks ()
		{
			base.DoMemberTypeIndependentChecks ();

			//
			// Accessors modifiers check
			//
			if ((Get.ModFlags & Modifiers.AccessibilityMask) != 0 &&
				(Set.ModFlags & Modifiers.AccessibilityMask) != 0) {
				Report.Error (274, Location, "`{0}': Cannot specify accessibility modifiers for both accessors of the property or indexer",
						GetSignatureForError ());
			}

			if ((ModFlags & Modifiers.OVERRIDE) == 0 && 
				(Get.IsDummy && (Set.ModFlags & Modifiers.AccessibilityMask) != 0) ||
				(Set.IsDummy && (Get.ModFlags & Modifiers.AccessibilityMask) != 0)) {
				Report.Error (276, Location, 
					      "`{0}': accessibility modifiers on accessors may only be used if the property or indexer has both a get and a set accessor",
					      GetSignatureForError ());
			}
		}

		bool DefineGet ()
		{
			GetBuilder = Get.Define (Parent);
			return (Get.IsDummy) ? true : GetBuilder != null;
		}

		bool DefineSet (bool define)
		{
			if (!define)
				return true;

			SetBuilder = Set.Define (Parent);
			return (Set.IsDummy) ? true : SetBuilder != null;
		}

		protected bool DefineAccessors ()
		{
			return DefineSet (define_set_first) &&
				DefineGet () &&
				DefineSet (!define_set_first);
		}

		protected void DefineBuilders (MemberKind kind, ParametersCompiled parameters)
		{
			// FIXME - PropertyAttributes.HasDefault ?

			PropertyBuilder = Parent.TypeBuilder.DefineProperty (
				GetFullName (MemberName), PropertyAttributes.None, MemberType.GetMetaInfo (), parameters.GetMetaInfo ());

			PropertySpec spec;
			if (kind == MemberKind.Indexer)
				spec = new IndexerSpec (Parent.Definition, this, MemberType, parameters, PropertyBuilder, ModFlags);
			else
				spec = new PropertySpec (kind, Parent.Definition, this, MemberType, PropertyBuilder, ModFlags);

			spec.Get = Get.Spec;
			spec.Set = Set.Spec;

			if (!Get.IsDummy) {
				PropertyBuilder.SetGetMethod (GetBuilder);
			}

			if (!Set.IsDummy) {
				PropertyBuilder.SetSetMethod (SetBuilder);
			}

			Parent.MemberCache.AddMember (this, Get.IsDummy ? Get.Name : GetBuilder.Name, Get.Spec);
			Parent.MemberCache.AddMember (this, Set.IsDummy ? Set.Name : SetBuilder.Name, Set.Spec);
			Parent.MemberCache.AddMember (this, PropertyBuilder.Name, spec);
		}

		public override void Emit ()
		{
			//
			// The PropertyBuilder can be null for explicit implementations, in that
			// case, we do not actually emit the ".property", so there is nowhere to
			// put the attribute
			//
			if (PropertyBuilder != null) {
				if (OptAttributes != null)
					OptAttributes.Emit ();

				if (member_type == InternalType.Dynamic) {
					PredefinedAttributes.Get.Dynamic.EmitAttribute (PropertyBuilder);
				} else {
					var trans_flags = TypeManager.HasDynamicTypeUsed (member_type);
					if (trans_flags != null) {
						var pa = PredefinedAttributes.Get.DynamicTransform;
						if (pa.Constructor != null || pa.ResolveConstructor (Location, ArrayContainer.MakeType (TypeManager.bool_type))) {
							PropertyBuilder.SetCustomAttribute (
								new CustomAttributeBuilder (pa.Constructor, new object [] { trans_flags }));
						}
					}
				}
			}

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

		public override bool IsUsed {
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
			
	public class Property : PropertyBase
	{
		public sealed class BackingField : Field
		{
			readonly Property property;

			public BackingField (Property p)
				: base (p.Parent, p.type_expr,
				Modifiers.BACKING_FIELD | Modifiers.COMPILER_GENERATED | Modifiers.PRIVATE | (p.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)),
				new MemberName ("<" + p.GetFullName (p.MemberName) + ">k__BackingField", p.Location), null)
			{
				this.property = p;
			}

			public string OriginalName {
				get {
					return property.Name;
				}
			}

			public override string GetSignatureForError ()
			{
				return property.GetSignatureForError ();
			}
		}

		const Modifiers AllowedModifiers =
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
			Modifiers.VIRTUAL;

		const Modifiers AllowedInterfaceModifiers =
			Modifiers.NEW;

		public Property (DeclSpace parent, FullNamedExpression type, Modifiers mod,
				 MemberName name, Attributes attrs, Accessor get_block,
				 Accessor set_block, bool define_set_first)
			: this (parent, type, mod, name, attrs, get_block, set_block,
				define_set_first, null)
		{
		}
		
		public Property (DeclSpace parent, FullNamedExpression type, Modifiers mod,
				 MemberName name, Attributes attrs, Accessor get_block,
				 Accessor set_block, bool define_set_first, Block current_block)
			: base (parent, type, mod,
				parent.PartialContainer.Kind == MemberKind.Interface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, attrs, define_set_first)
		{
			if (get_block == null)
				Get = new GetMethod (this);
			else
				Get = new GetMethod (this, get_block);

			if (set_block == null)
				Set = new SetMethod (this);
			else
				Set = new SetMethod (this, set_block);

			if (!IsInterface && (mod & (Modifiers.ABSTRACT | Modifiers.EXTERN)) == 0 &&
				get_block != null && get_block.Block == null &&
				set_block != null && set_block.Block == null) {
				if (RootContext.Version <= LanguageVersion.ISO_2)
					Report.FeatureIsNotAvailable (Location, "automatically implemented properties");

				Get.ModFlags |= Modifiers.COMPILER_GENERATED;
				Set.ModFlags |= Modifiers.COMPILER_GENERATED;
			}
		}

		void CreateAutomaticProperty ()
		{
			// Create backing field
			Field field = new BackingField (this);
			if (!field.Define ())
				return;

			Parent.PartialContainer.AddField (field);

			FieldExpr fe = new FieldExpr (field, Location);
			if ((field.ModFlags & Modifiers.STATIC) == 0)
				fe.InstanceExpression = new CompilerGeneratedThis (fe.Type, Location);

			// Create get block
			Get.Block = new ToplevelBlock (Compiler, ParametersCompiled.EmptyReadOnlyParameters, Location);
			Return r = new Return (fe, Location);
			Get.Block.AddStatement (r);

			// Create set block
			Set.Block = new ToplevelBlock (Compiler, Set.ParameterInfo, Location);
			Assign a = new SimpleAssign (fe, new SimpleName ("value", Location));
			Set.Block.AddStatement (new StatementExpression (a));
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;

			if ((Get.ModFlags & Modifiers.COMPILER_GENERATED) != 0)
				CreateAutomaticProperty ();

			if (!DefineAccessors ())
				return false;

			if (!CheckBase ())
				return false;

			DefineBuilders (MemberKind.Property, ParametersCompiled.EmptyReadOnlyParameters);
			return true;
		}

		public override void Emit ()
		{
			if (((Set.ModFlags | Get.ModFlags) & (Modifiers.STATIC | Modifiers.COMPILER_GENERATED)) == Modifiers.COMPILER_GENERATED && Parent.PartialContainer.HasExplicitLayout) {
				Report.Error (842, Location,
					"Automatically implemented property `{0}' cannot be used inside a type with an explicit StructLayout attribute",
					GetSignatureForError ());
			}

			base.Emit ();
		}
	}

	/// <summary>
	/// For case when event is declared like property (with add and remove accessors).
	/// </summary>
	public class EventProperty: Event {
		abstract class AEventPropertyAccessor : AEventAccessor
		{
			protected AEventPropertyAccessor (EventProperty method, Accessor accessor, string prefix)
				: base (method, accessor, prefix)
			{
			}

			public override MethodBuilder Define (DeclSpace ds)
			{
				CheckAbstractAndExtern (block != null);
				return base.Define (ds);
			}
			
			public override string GetSignatureForError ()
			{
				return method.GetSignatureForError () + "." + prefix.Substring (0, prefix.Length - 1);
			}
		}

		sealed class AddDelegateMethod: AEventPropertyAccessor
		{
			public AddDelegateMethod (EventProperty method, Accessor accessor):
				base (method, accessor, AddPrefix)
			{
			}
		}

		sealed class RemoveDelegateMethod: AEventPropertyAccessor
		{
			public RemoveDelegateMethod (EventProperty method, Accessor accessor):
				base (method, accessor, RemovePrefix)
			{
			}
		}


		static readonly string[] attribute_targets = new string [] { "event" };

		public EventProperty (DeclSpace parent, FullNamedExpression type, Modifiers mod_flags,
				      MemberName name,
				      Attributes attrs, Accessor add, Accessor remove)
			: base (parent, type, mod_flags, name, attrs)
		{
			Add = new AddDelegateMethod (this, add);
			Remove = new RemoveDelegateMethod (this, remove);
		}

		public override bool Define()
		{
			if (!base.Define ())
				return false;

			SetIsUsed ();
			return true;
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
		abstract class EventFieldAccessor : AEventAccessor
		{
			protected EventFieldAccessor (EventField method, string prefix)
				: base (method, prefix)
			{
			}

			public override void Emit (DeclSpace parent)
			{
				if ((method.ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) == 0) {
					if (parent is Class) {
						MethodBuilder mb = method_data.MethodBuilder;
						mb.SetImplementationFlags (mb.GetMethodImplementationFlags () | MethodImplAttributes.Synchronized);
					}

					var field_info = ((EventField) method).BackingField;
					FieldExpr f_expr = new FieldExpr (field_info, Location);
					if ((method.ModFlags & Modifiers.STATIC) == 0)
						f_expr.InstanceExpression = new CompilerGeneratedThis (field_info.Spec.MemberType, Location);

					block = new ToplevelBlock (Compiler, ParameterInfo, Location);
					block.AddStatement (new StatementExpression (
						new CompoundAssign (Operation,
							f_expr,
							block.GetParameterReference (ParameterInfo[0].Name, Location))));
				}

				base.Emit (parent);
			}

			protected abstract Binary.Operator Operation { get; }
		}

		sealed class AddDelegateMethod: EventFieldAccessor
		{
			public AddDelegateMethod (EventField method):
				base (method, AddPrefix)
			{
			}

			protected override Binary.Operator Operation {
				get { return Binary.Operator.Addition; }
			}
		}

		sealed class RemoveDelegateMethod: EventFieldAccessor
		{
			public RemoveDelegateMethod (EventField method):
				base (method, RemovePrefix)
			{
			}

			protected override Binary.Operator Operation {
				get { return Binary.Operator.Subtraction; }
			}
		}


		static readonly string[] attribute_targets = new string [] { "event", "field", "method" };
		static readonly string[] attribute_targets_interface = new string[] { "event", "method" };

		public Field BackingField;
		public Expression Initializer;

		public EventField (DeclSpace parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, Attributes attrs)
			: base (parent, type, mod_flags, name, attrs)
		{
			Add = new AddDelegateMethod (this);
			Remove = new RemoveDelegateMethod (this);
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.Field) {
				BackingField.ApplyAttributeBuilder (a, ctor, cdata, pa);
				return;
			}

			if (a.Target == AttributeTargets.Method) {
				int errors = Report.Errors;
				Add.ApplyAttributeBuilder (a, ctor, cdata, pa);
				if (errors == Report.Errors)
					Remove.ApplyAttributeBuilder (a, ctor, cdata, pa);
				return;
			}

			base.ApplyAttributeBuilder (a, ctor, cdata, pa);
		}

		public override bool Define()
		{
			if (!base.Define ())
				return false;

			if (Initializer != null && (ModFlags & Modifiers.ABSTRACT) != 0) {
				Report.Error (74, Location, "`{0}': abstract event cannot have an initializer",
					GetSignatureForError ());
			}

			if (!HasBackingField) {
				SetIsUsed ();
				return true;
			}

			// FIXME: We are unable to detect whether generic event is used because
			// we are using FieldExpr instead of EventExpr for event access in that
			// case.  When this issue will be fixed this hack can be removed.
			if (TypeManager.IsGenericType (MemberType) || Parent.IsGeneric)
				SetIsUsed ();

			if (Add.IsInterfaceImplementation)
				SetIsUsed ();

			BackingField = new Field (Parent,
				new TypeExpression (MemberType, Location),
				Modifiers.BACKING_FIELD | Modifiers.COMPILER_GENERATED | Modifiers.PRIVATE | (ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)),
				MemberName, null);

			Parent.PartialContainer.AddField (BackingField);
			BackingField.Initializer = Initializer;
			BackingField.ModFlags &= ~Modifiers.COMPILER_GENERATED;

			// Call define because we passed fields definition
			return BackingField.Define ();
		}

		public bool HasBackingField {
			get {
				return !IsInterface && (ModFlags & Modifiers.ABSTRACT) == 0;
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return HasBackingField ? attribute_targets : attribute_targets_interface;
			}
		}
	}

	public abstract class Event : PropertyBasedMember {
		public abstract class AEventAccessor : AbstractPropertyEventMethod
		{
			protected readonly Event method;
			ImplicitParameter param_attr;
			ParametersCompiled parameters;

			static readonly string[] attribute_targets = new string [] { "method", "param", "return" };

			public const string AddPrefix = "add_";
			public const string RemovePrefix = "remove_";

			protected AEventAccessor (Event method, string prefix)
				: base (method, prefix)
			{
				this.method = method;
				this.ModFlags = method.ModFlags;
				this.parameters = ParametersCompiled.CreateImplicitParameter (method.type_expr, Location);
			}

			protected AEventAccessor (Event method, Accessor accessor, string prefix)
				: base (method, accessor, prefix)
			{
				this.method = method;
				this.ModFlags = method.ModFlags;
				this.parameters = accessor.Parameters;
			}

			public bool IsInterfaceImplementation {
				get { return method_data.implementing != null; }
			}

			public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
			{
				if (a.IsInternalMethodImplAttribute) {
					method.is_external_implementation = true;
				}

				base.ApplyAttributeBuilder (a, ctor, cdata, pa);
			}

			protected override void ApplyToExtraTarget (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
			{
				if (a.Target == AttributeTargets.Parameter) {
					if (param_attr == null)
						param_attr = new ImplicitParameter (method_data.MethodBuilder);

					param_attr.ApplyAttributeBuilder (a, ctor, cdata, pa);
					return;
				}

				base.ApplyAttributeBuilder (a, ctor, cdata, pa);
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsComplianceRequired ()
			{
				return method.IsClsComplianceRequired ();
			}

			public virtual MethodBuilder Define (DeclSpace parent)
			{
				parameters.Resolve (this);

				method_data = new MethodData (method, method.ModFlags,
					method.flags | MethodAttributes.HideBySig | MethodAttributes.SpecialName, this);

				if (!method_data.Define (parent, method.GetFullName (MemberName), Report))
					return null;

				MethodBuilder mb = method_data.MethodBuilder;
				ParameterInfo.ApplyAttributes (mb);
				Spec = new MethodSpec (MemberKind.Method, parent.PartialContainer.Definition, this, ReturnType, mb, ParameterInfo, method.ModFlags);
				Spec.IsAccessor = true;

				return mb;
			}

			public override TypeSpec ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			public override ObsoleteAttribute GetAttributeObsolete ()
			{
				return method.GetAttributeObsolete ();
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}

			public override ParametersCompiled ParameterInfo {
				get {
					return parameters;
				}
			}
		}


		const Modifiers AllowedModifiers =
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
			Modifiers.ABSTRACT |
			Modifiers.EXTERN;

		const Modifiers AllowedInterfaceModifiers =
			Modifiers.NEW;

		public AEventAccessor Add, Remove;
		public EventBuilder     EventBuilder;
		public MethodBuilder AddBuilder, RemoveBuilder;

		protected Event (DeclSpace parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, Attributes attrs)
			: base (parent, null, type, mod_flags,
				parent.PartialContainer.Kind == MemberKind.Interface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, attrs)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if ((a.HasSecurityAttribute)) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			EventBuilder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), cdata);
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

		protected override bool CheckOverrideAgainstBase (MemberSpec base_member)
		{
			var ok = base.CheckOverrideAgainstBase (base_member);

			if (!CheckAccessModifiers (this, base_member)) {
				Error_CannotChangeAccessModifiers (this, base_member);
				ok = false;
			}

			return ok;
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (!TypeManager.IsDelegateType (MemberType)) {
				Report.Error (66, Location, "`{0}': event must be of a delegate type", GetSignatureForError ());
			}

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

			EventBuilder = Parent.TypeBuilder.DefineEvent (Name, EventAttributes.None, MemberType.GetMetaInfo ());
			EventBuilder.SetAddOnMethod (AddBuilder);
			EventBuilder.SetRemoveOnMethod (RemoveBuilder);

			var spec = new EventSpec (Parent.Definition, this, MemberType, ModFlags, Add.Spec, Remove.Spec);

			Parent.MemberCache.AddMember (this, Name, spec);
			Parent.MemberCache.AddMember (this, AddBuilder.Name, Add.Spec);
			Parent.MemberCache.AddMember (this, RemoveBuilder.Name, Remove.Spec);

			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				OptAttributes.Emit ();
			}

			Add.Emit (Parent);
			Remove.Emit (Parent);

			base.Emit ();
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "E:"; }
		}
	}

	public class EventSpec : MemberSpec, IInterfaceMemberSpec
	{
		MethodSpec add, remove;

		public EventSpec (TypeSpec declaringType, IMemberDefinition definition, TypeSpec eventType, Modifiers modifiers, MethodSpec add, MethodSpec remove)
			: base (MemberKind.Event, declaringType, definition, modifiers)
		{
			this.AccessorAdd = add;
			this.AccessorRemove = remove;
			this.MemberType = eventType;
		}

		#region Properties

		public MethodSpec AccessorAdd { 
			get {
				return add;
			}
			set {
				add = value;
			}
		}

		public MethodSpec AccessorRemove {
			get {
				return remove;
			}
			set {
				remove = value;
			}
		}

		public TypeSpec MemberType { get; private set; }

		#endregion

		public override MemberSpec InflateMember (TypeParameterInflator inflator)
		{
			var es = (EventSpec) base.InflateMember (inflator);
			es.MemberType = inflator.Inflate (MemberType);
			return es;
		}
	}
 
	public class Indexer : PropertyBase, IParametersMember
	{
		public class GetIndexerMethod : GetMethod, IParametersMember
		{
			ParametersCompiled parameters;

			public GetIndexerMethod (Indexer method):
				base (method)
			{
				this.parameters = method.parameters;
			}

			public GetIndexerMethod (PropertyBase method, Accessor accessor):
				base (method, accessor)
			{
				parameters = accessor.Parameters;
			}

			public override MethodBuilder Define (DeclSpace parent)
			{
				parameters.Resolve (this);
				return base.Define (parent);
			}
			
			public override bool EnableOverloadChecks (MemberCore overload)
			{
				if (base.EnableOverloadChecks (overload)) {
					overload.caching_flags |= Flags.MethodOverloadsExist;
					return true;
				}

				return false;
			}			

			public override ParametersCompiled ParameterInfo {
				get {
					return parameters;
				}
			}

			#region IParametersMember Members

			AParametersCollection IParametersMember.Parameters {
				get {
					return parameters;
				}
			}

			TypeSpec IInterfaceMemberSpec.MemberType {
				get {
					return ReturnType;
				}
			}

			#endregion
		}

		public class SetIndexerMethod : SetMethod, IParametersMember
		{
			public SetIndexerMethod (Indexer method):
				base (method)
			{
				parameters = ParametersCompiled.MergeGenerated (Compiler, method.parameters, false, parameters [0], null);
			}

			public SetIndexerMethod (PropertyBase method, Accessor accessor):
				base (method, accessor)
			{
				parameters = method.Get.IsDummy ? accessor.Parameters : accessor.Parameters.Clone ();			
			}

			public override bool EnableOverloadChecks (MemberCore overload)
			{
				if (base.EnableOverloadChecks (overload)) {
					overload.caching_flags |= Flags.MethodOverloadsExist;
					return true;
				}

				return false;
			}

			#region IParametersMember Members

			AParametersCollection IParametersMember.Parameters {
				get {
					return parameters;
				}
			}

			TypeSpec IInterfaceMemberSpec.MemberType {
				get {
					return ReturnType;
				}
			}

			#endregion
		}

		const Modifiers AllowedModifiers =
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

		const Modifiers AllowedInterfaceModifiers =
			Modifiers.NEW;

		readonly ParametersCompiled parameters;

		public Indexer (DeclSpace parent, FullNamedExpression type, MemberName name, Modifiers mod,
				ParametersCompiled parameters, Attributes attrs,
				Accessor get_block, Accessor set_block, bool define_set_first)
			: base (parent, type, mod,
				parent.PartialContainer.Kind == MemberKind.Interface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, attrs, define_set_first)
		{
			this.parameters = parameters;

			if (get_block == null)
				Get = new GetIndexerMethod (this);
			else
				Get = new GetIndexerMethod (this, get_block);

			if (set_block == null)
				Set = new SetIndexerMethod (this);
			else
				Set = new SetIndexerMethod (this, set_block);
		}

		public override void ApplyAttributeBuilder (Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.IndexerName) {
				if (IsExplicitImpl) {
					Report.Error (415, a.Location,
						"The `{0}' attribute is valid only on an indexer that is not an explicit interface member declaration",
						TypeManager.CSharpName (a.Type));
				}

				// Attribute was copied to container
				return;
			}

			base.ApplyAttributeBuilder (a, ctor, cdata, pa);
		}

		protected override bool CheckForDuplications ()
		{
			return Parent.MemberCache.CheckExistingMembersOverloads (this, parameters);
		}
		
		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (!DefineParameters (parameters))
				return false;

			if (OptAttributes != null) {
				Attribute indexer_attr = OptAttributes.Search (PredefinedAttributes.Get.IndexerName);
				if (indexer_attr != null) {
					var compiling = indexer_attr.Type.MemberDefinition as TypeContainer;
					if (compiling != null)
						compiling.Define ();

					string name = indexer_attr.GetIndexerAttributeValue ();
					if ((ModFlags & Modifiers.OVERRIDE) != 0) {
						Report.Error (609, indexer_attr.Location,
							"Cannot set the `IndexerName' attribute on an indexer marked override");
					}

					if (!string.IsNullOrEmpty (name))
						ShortName = name;
				}
			}

			if (InterfaceType != null) {
				string base_IndexerName = InterfaceType.MemberDefinition.GetAttributeDefaultMember ();
				if (base_IndexerName != Name)
					ShortName = base_IndexerName;
			}

			if (!Parent.PartialContainer.AddMember (this) ||
				!Parent.PartialContainer.AddMember (Get) || !Parent.PartialContainer.AddMember (Set))
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			
			if (!DefineAccessors ())
				return false;

			if (!CheckBase ())
				return false;

			DefineBuilders (MemberKind.Indexer, parameters);
			return true;
		}

		public override bool EnableOverloadChecks (MemberCore overload)
		{
			if (overload is Indexer) {
				caching_flags |= Flags.MethodOverloadsExist;
				return true;
			}

			return base.EnableOverloadChecks (overload);
		}

		public override string GetDocCommentName (DeclSpace ds)
		{
			return DocUtil.GetMethodDocCommentName (this, parameters, ds);
		}

		public override string GetSignatureForError ()
		{
			StringBuilder sb = new StringBuilder (Parent.GetSignatureForError ());
			if (MemberName.Left != null) {
				sb.Append ('.');
				sb.Append (MemberName.Left.GetSignatureForError ());
			}

			sb.Append (".this");
			sb.Append (parameters.GetSignatureForError ().Replace ('(', '[').Replace (')', ']'));
			return sb.ToString ();
		}

		public AParametersCollection Parameters {
			get {
				return parameters;
			}
		}

		public ParametersCompiled ParameterInfo {
			get {
				return parameters;
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			parameters.VerifyClsCompliance (this);
			return true;
		}
	}

	public class IndexerSpec : PropertySpec, IParametersMember
	{
		AParametersCollection parameters;

		public IndexerSpec (TypeSpec declaringType, IMemberDefinition definition, TypeSpec memberType, AParametersCollection parameters, PropertyInfo info, Modifiers modifiers)
			: base (MemberKind.Indexer, declaringType, definition, memberType, info, modifiers)
		{
			this.parameters = parameters;
		}

		public override string GetSignatureForError ()
		{
			return DeclaringType.GetSignatureForError () + ".this" + parameters.GetSignatureForError ("[", "]", parameters.Count);
		}

		public AParametersCollection Parameters {
			get {
				return parameters;
			}
		}
	}
}
