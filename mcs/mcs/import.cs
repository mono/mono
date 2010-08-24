//
// import.cs: System.Reflection conversions
//
// Authors: Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2009, 2010 Novell, Inc
//

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mono.CSharp
{
	public static class Import
	{
		static Dictionary<Type, TypeSpec> import_cache;
		static Dictionary<Type, PredefinedTypeSpec> type_2_predefined;

		public static void Initialize ()
		{
			import_cache = new Dictionary<Type, TypeSpec> (1024, ReferenceEquality<Type>.Default);

			// Setup mapping for predefined types
			type_2_predefined = new Dictionary<Type, PredefinedTypeSpec> () {
				{ typeof (object), TypeManager.object_type },
				{ typeof (System.ValueType), TypeManager.value_type },
				{ typeof (System.Attribute), TypeManager.attribute_type },

				{ typeof (int), TypeManager.int32_type },
				{ typeof (long), TypeManager.int64_type },
				{ typeof (uint), TypeManager.uint32_type },
				{ typeof (ulong), TypeManager.uint64_type },
				{ typeof (byte), TypeManager.byte_type },
				{ typeof (sbyte), TypeManager.sbyte_type },
				{ typeof (short), TypeManager.short_type },
				{ typeof (ushort), TypeManager.ushort_type },

				{ typeof (System.Collections.IEnumerator), TypeManager.ienumerator_type },
				{ typeof (System.Collections.IEnumerable), TypeManager.ienumerable_type },
				{ typeof (System.IDisposable), TypeManager.idisposable_type },

				{ typeof (char), TypeManager.char_type },
				{ typeof (string), TypeManager.string_type },
				{ typeof (float), TypeManager.float_type },
				{ typeof (double), TypeManager.double_type },
				{ typeof (decimal), TypeManager.decimal_type },
				{ typeof (bool), TypeManager.bool_type },
				{ typeof (System.IntPtr), TypeManager.intptr_type },
				{ typeof (System.UIntPtr), TypeManager.uintptr_type },

				{ typeof (System.MulticastDelegate), TypeManager.multicast_delegate_type },
				{ typeof (System.Delegate), TypeManager.delegate_type },
				{ typeof (System.Enum), TypeManager.enum_type },
				{ typeof (System.Array), TypeManager.array_type },
				{ typeof (void), TypeManager.void_type },
				{ typeof (System.Type), TypeManager.type_type },
				{ typeof (System.Exception), TypeManager.exception_type },
				{ typeof (System.RuntimeFieldHandle), TypeManager.runtime_field_handle_type },
				{ typeof (System.RuntimeTypeHandle), TypeManager.runtime_handle_type }
			};
		}

		public static FieldSpec CreateField (FieldInfo fi, TypeSpec declaringType)
		{
			Modifiers mod = 0;
			var fa = fi.Attributes;
			switch (fa & FieldAttributes.FieldAccessMask) {
				case FieldAttributes.Public:
					mod = Modifiers.PUBLIC;
					break;
				case FieldAttributes.Assembly:
					mod = Modifiers.INTERNAL;
					break;
				case FieldAttributes.Family:
					mod = Modifiers.PROTECTED;
					break;
				case FieldAttributes.FamORAssem:
					mod = Modifiers.PROTECTED | Modifiers.INTERNAL;
					break;
				default:
					mod = Modifiers.PRIVATE;
					break;
			}

			// Ignore private fields (even for error reporting) to not require extra dependencies
			if (mod == Modifiers.PRIVATE)
				return null;

			var definition = new ImportedMemberDefinition (fi);
			TypeSpec field_type;

			try {
				field_type = ImportType (fi.FieldType);
			} catch (Exception e) {
				// TODO: I should construct fake TypeSpec based on TypeRef signature
				// but there is no way to do it with System.Reflection
				throw new InternalErrorException (e, "Cannot import field `{0}.{1}' in assembly `{2}'",
					declaringType.GetSignatureForError (), fi.Name, declaringType.Assembly);
			}

			if ((fa & FieldAttributes.Literal) != 0) {
				var c = Constant.CreateConstantFromValue (field_type, fi.GetValue (fi), Location.Null);
				return new ConstSpec (declaringType, definition, field_type, fi, mod, c);
			}

			if ((fa & FieldAttributes.InitOnly) != 0) {
				if (field_type == TypeManager.decimal_type) {
					var dc = ReadDecimalConstant (fi);
					if (dc != null)
						return new ConstSpec (declaringType, definition, field_type, fi, mod, dc);
				}

				mod |= Modifiers.READONLY;
			} else {
				var reqs = fi.GetRequiredCustomModifiers ();
				if (reqs.Length > 0) {
					foreach (Type t in reqs) {
						if (t == typeof (System.Runtime.CompilerServices.IsVolatile)) {
							mod |= Modifiers.VOLATILE;
							break;
						}
					}
				}
			}

			if ((fa & FieldAttributes.Static) != 0)
				mod |= Modifiers.STATIC;

			if (field_type.IsStruct) {
				 if (fi.IsDefined (typeof (FixedBufferAttribute), false)) {
					var element_field = CreateField (fi.FieldType.GetField (FixedField.FixedElementName), declaringType);
					return new FixedFieldSpec (declaringType, definition, fi, element_field, mod);
				}
			}

			return new FieldSpec (declaringType, definition, field_type, fi, mod);
		}

		public static EventSpec CreateEvent (EventInfo ei, TypeSpec declaringType, MethodSpec add, MethodSpec remove)
		{
			add.IsAccessor = true;
			remove.IsAccessor = true;

			if (add.Modifiers != remove.Modifiers)
				throw new NotImplementedException ("Different accessor modifiers " + ei.Name);

			var definition = new ImportedMemberDefinition (ei);
			return new EventSpec (declaringType, definition, ImportType (ei.EventHandlerType), add.Modifiers, add, remove);
		}

		static T[] CreateGenericParameters<T> (Type type, TypeSpec declaringType) where T : TypeSpec
		{
			Type[] tparams = type.GetGenericArguments ();

			int parent_owned_count;
			if (type.IsNested) {
				parent_owned_count = type.DeclaringType.GetGenericArguments ().Length;

				//
				// System.Reflection duplicates parent type parameters for each
				// nested type with slightly modified properties (eg. different owner)
				// This just makes things more complicated (think of cloned constraints)
				// therefore we remap any nested type owned by parent using `type_cache'
				// to the single TypeParameterSpec
				//
				if (declaringType != null && parent_owned_count > 0) {
					int read_count = 0;
					while (read_count != parent_owned_count) {
						var tparams_count = declaringType.Arity;
						if (tparams_count != 0) {
							var parent_tp = declaringType.MemberDefinition.TypeParameters;
							read_count += tparams_count;
							for (int i = 0; i < tparams_count; i++) {
								import_cache.Add (tparams[parent_owned_count - read_count + i], parent_tp[i]);
							}
						}

						declaringType = declaringType.DeclaringType;
					}
				}			
			} else {
				parent_owned_count = 0;
			}

			if (tparams.Length - parent_owned_count == 0)
				return null;

			return CreateGenericParameters<T> (parent_owned_count, tparams);
		}

		static T[] CreateGenericParameters<T> (int first, Type[] tparams) where T : TypeSpec
		{
			var tspec = new T [tparams.Length - first];
			for (int pos = first; pos < tparams.Length; ++pos) {
				var type = tparams[pos];
				if (type.HasElementType) {
					var element = type.GetElementType ();
					var spec = CreateType (element);

					if (type.IsArray) {
						tspec[pos - first] = (T) (TypeSpec) ArrayContainer.MakeType (spec, type.GetArrayRank ());
						continue;
					}

					throw new NotImplementedException ("Unknown element type " + type.ToString ());
				}

				tspec [pos - first] = (T) CreateType (type);
			}

			return tspec;
		}

		public static MethodSpec CreateMethod (MethodBase mb, TypeSpec declaringType)
		{
			Modifiers mod = ReadMethodModifiers (mb, declaringType);
			TypeParameterSpec[] tparams;
			ImportedMethodDefinition definition;

			var parameters = ParametersImported.Create (declaringType, mb);

			if (mb.IsGenericMethod) {
				if (!mb.IsGenericMethodDefinition)
					throw new NotSupportedException ("assert");

				tparams = CreateGenericParameters<TypeParameterSpec>(0, mb.GetGenericArguments ());
				definition = new ImportedGenericMethodDefinition ((MethodInfo) mb, parameters, tparams);
			} else {
				definition = new ImportedMethodDefinition (mb, parameters);
				tparams = null;
			}

			MemberKind kind;
			TypeSpec returnType;
			if (mb.MemberType == MemberTypes.Constructor) {
				kind = MemberKind.Constructor;
				returnType = TypeManager.void_type;
			} else {
				//
				// Detect operators and destructors
				//
				string name = mb.Name;
				kind = MemberKind.Method;
				if (tparams == null && !mb.DeclaringType.IsInterface && name.Length > 6) {
					if ((mod & (Modifiers.STATIC | Modifiers.PUBLIC)) == (Modifiers.STATIC | Modifiers.PUBLIC)) {
						if (name[2] == '_' && name[1] == 'p' && name[0] == 'o') {
							var op_type = Operator.GetType (name);
							if (op_type.HasValue && parameters.Count > 0 && parameters.Count < 3) {
								kind = MemberKind.Operator;
							}
						}
					} else if (parameters.IsEmpty && name == Destructor.MetadataName) {
						kind = MemberKind.Destructor;
						if (declaringType == TypeManager.object_type) {
							mod &= ~Modifiers.OVERRIDE;
							mod |= Modifiers.VIRTUAL;
						}
					}
				}

				returnType = ImportType (((MethodInfo)mb).ReturnType);

				// Cannot set to OVERRIDE without full hierarchy checks
				// this flag indicates that the method could be override
				// but further validation is needed
				if ((mod & Modifiers.OVERRIDE) != 0 && kind == MemberKind.Method && declaringType.BaseType != null) {
					var filter = MemberFilter.Method (name, tparams != null ? tparams.Length : 0, parameters, null);
					var candidate = MemberCache.FindMember (declaringType.BaseType, filter, BindingRestriction.None);

					//
					// For imported class method do additional validation to be sure that metadata
					// override flag was correct
					// 
					// Difference between protected internal and protected is ok
					//
					const Modifiers conflict_mask = Modifiers.AccessibilityMask & ~Modifiers.INTERNAL;
					if (candidate == null || (candidate.Modifiers & conflict_mask) != (mod & conflict_mask) || candidate.IsStatic) {
						mod &= ~Modifiers.OVERRIDE;
					}
				}
			}

			MethodSpec ms = new MethodSpec (kind, declaringType, definition, returnType, mb, parameters, mod);
			if (tparams != null)
				ms.IsGeneric = true;

			return ms;
		}

		//
		// Returns null when the property is not valid C# property
		//
		public static PropertySpec CreateProperty (PropertyInfo pi, TypeSpec declaringType, MethodSpec get, MethodSpec set)
		{
			var definition = new ImportedMemberDefinition (pi);

			Modifiers mod = 0;
			AParametersCollection param = null;
			TypeSpec type = null;
			if (get != null) {
				mod = get.Modifiers;
				param = get.Parameters;
				type = get.ReturnType;
			}

			bool is_valid_property = true;
			if (set != null) {
				if (set.ReturnType != TypeManager.void_type)
					is_valid_property = false;

				var set_param_count = set.Parameters.Count - 1;

				if (set_param_count < 0) {
					set_param_count = 0;
					is_valid_property = false;
				}

				var set_type = set.Parameters.Types[set_param_count];

				if (mod == 0) {
					AParametersCollection set_based_param;

					if (set_param_count == 0) {
						set_based_param = ParametersCompiled.EmptyReadOnlyParameters;
					} else {
						//
						// Create indexer parameters based on setter method parameters (the last parameter has to be removed)
						//
						var data = new IParameterData[set_param_count];
						var types = new TypeSpec[set_param_count];
						Array.Copy (set.Parameters.FixedParameters, data, set_param_count);
						Array.Copy (set.Parameters.Types, types, set_param_count);
						set_based_param = new ParametersImported (data, types, set.Parameters.HasParams);
					}

					mod = set.Modifiers;
					param = set_based_param;
					type = set_type;
				} else {
					if (set_param_count != get.Parameters.Count)
						is_valid_property = false;

					if (get.ReturnType != set_type)
						is_valid_property = false;

					// Possible custom accessor modifiers
					if ((mod & Modifiers.AccessibilityMask) != (set.Modifiers & Modifiers.AccessibilityMask)) {
						var get_acc = mod & Modifiers.AccessibilityMask;
						if (get_acc != Modifiers.PUBLIC) {
							var set_acc = set.Modifiers & Modifiers.AccessibilityMask;
							// If the accessor modifiers are not same, do extra restriction checks
							if (get_acc != set_acc) {
								var get_restr = ModifiersExtensions.IsRestrictedModifier (get_acc, set_acc);
								var set_restr = ModifiersExtensions.IsRestrictedModifier (set_acc, get_acc);
								if (get_restr && set_restr) {
									is_valid_property = false; // Neither is more restrictive
								}

								if (get_restr) {
									mod &= ~Modifiers.AccessibilityMask;
									mod |= set_acc;
								}
							}
						}
					}
				}
			}

			PropertySpec spec = null;
			if (!param.IsEmpty) {
				var index_name = declaringType.MemberDefinition.GetAttributeDefaultMember ();
				if (index_name == null) {
					is_valid_property = false;
				} else {
					if (get != null) {
						if (get.IsStatic)
							is_valid_property = false;
						if (get.Name.IndexOf (index_name, StringComparison.Ordinal) != 4)
							is_valid_property = false;
					}
					if (set != null) {
						if (set.IsStatic)
							is_valid_property = false;
						if (set.Name.IndexOf (index_name, StringComparison.Ordinal) != 4)
							is_valid_property = false;
					}
				}

				if (is_valid_property)
					spec = new IndexerSpec (declaringType, definition, type, param, pi, mod);
			}

			if (spec == null)
				spec = new PropertySpec (MemberKind.Property, declaringType, definition, type, pi, mod);

			if (!is_valid_property) {
				spec.IsNotRealProperty = true;
				return spec;
			}

			if (set != null)
				spec.Set = set;
			if (get != null)
				spec.Get = get;

			return spec;
		}

		public static TypeSpec CreateType (Type type)
		{
			TypeSpec declaring_type;
			if (type.IsNested && !type.IsGenericParameter)
				declaring_type = CreateType (type.DeclaringType);
			else
				declaring_type = null;

			return CreateType (type, declaring_type);
		}

		public static TypeSpec CreateType (Type type, TypeSpec declaringType)
		{
			TypeSpec spec;
			if (import_cache.TryGetValue (type, out spec))
				return spec;

			if (type.IsGenericType && !type.IsGenericTypeDefinition) {	
				var type_def = type.GetGenericTypeDefinition ();
				spec = CreateType (type_def, declaringType);

				var targs = CreateGenericParameters<TypeSpec> (type, null);

				InflatedTypeSpec inflated;
				if (targs == null) {
					// Inflating nested non-generic type, same in TypeSpec::InflateMember
					inflated = new InflatedTypeSpec (spec, declaringType, TypeSpec.EmptyTypes);
				} else {
					// CreateGenericParameters constraint could inflate type
					if (import_cache.ContainsKey (type))
						return import_cache[type];

					inflated = spec.MakeGenericType (targs);

					// Use of reading cache to speed up reading only
					import_cache.Add (type, inflated);
				}

				return inflated;
			}

			Modifiers mod;
			MemberKind kind;

			var ma = type.Attributes;
			switch (ma & TypeAttributes.VisibilityMask) {
			case TypeAttributes.Public:
			case TypeAttributes.NestedPublic:
				mod = Modifiers.PUBLIC;
				break;
			case TypeAttributes.NestedPrivate:
				mod = Modifiers.PRIVATE;
				break;
			case TypeAttributes.NestedFamily:
				mod = Modifiers.PROTECTED;
				break;
			case TypeAttributes.NestedFamORAssem:
				mod = Modifiers.PROTECTED | Modifiers.INTERNAL;
				break;
			default:
				mod = Modifiers.INTERNAL;
				break;
			}

			if ((ma & TypeAttributes.Interface) != 0) {
				kind = MemberKind.Interface;
			} else if (type.IsGenericParameter) {
				kind = MemberKind.TypeParameter;
			} else if (type.IsClass || type.IsAbstract) {  				// SRE: System.Enum returns false for IsClass
				if ((ma & TypeAttributes.Sealed) != 0 && type.IsSubclassOf (typeof (MulticastDelegate))) {
					kind = MemberKind.Delegate;
					mod |= Modifiers.SEALED;
				} else {
					kind = MemberKind.Class;

					if (type == typeof (object)) {
#if NET_4_0
						var pa = PredefinedAttributes.Get.Dynamic.Type;
						if (pa != null && type.IsDefined (typeof (DynamicAttribute), false))
							return InternalType.Dynamic;
#endif
					}

					if ((ma & TypeAttributes.Sealed) != 0) {
						mod |= Modifiers.SEALED;
						if ((ma & TypeAttributes.Abstract) != 0)
							mod |= Modifiers.STATIC;
					} else if ((ma & TypeAttributes.Abstract) != 0) {
						mod |= Modifiers.ABSTRACT;
					}
				}
			} else if (type.IsEnum) {
				kind = MemberKind.Enum;
			} else {
				kind = MemberKind.Struct;
				mod |= Modifiers.SEALED;
			}

			var definition = new ImportedTypeDefinition (type);
			PredefinedTypeSpec pt;

			if (kind == MemberKind.Enum) {
				const BindingFlags underlying_member = BindingFlags.DeclaredOnly |
					BindingFlags.Instance |
					BindingFlags.Public | BindingFlags.NonPublic;

				var type_members = type.GetFields (underlying_member);
				foreach (var type_member in type_members) {
					spec = new EnumSpec (declaringType, definition, Import.CreateType (type_member.FieldType), type, mod);
					break;
				}

				if (spec == null)
					kind = MemberKind.Class;

			} else if (kind == MemberKind.TypeParameter) {
				// Return as type_cache was updated
				return CreateTypeParameter (type, declaringType);
			} else if (type.IsGenericTypeDefinition) {
				definition.TypeParameters = CreateGenericParameters<TypeParameterSpec>(type, declaringType);

				// Constraints are not loaded on demand and can reference this type
				if (import_cache.TryGetValue (type, out spec))
					return spec;

			} else if (type_2_predefined.TryGetValue (type, out pt)) {
				spec = pt;
				pt.SetDefinition (definition, type);
			}

			if (spec == null)
				spec = new TypeSpec (kind, declaringType, definition, type, mod);

			import_cache.Add (type, spec);

			if (kind == MemberKind.Interface)
				spec.BaseType = TypeManager.object_type;
			else if (type.BaseType != null)
				spec.BaseType = CreateType (type.BaseType);

			var ifaces = type.GetInterfaces ();
			if (ifaces.Length > 0) {
				foreach (Type iface in ifaces) {
					spec.AddInterface (Import.CreateType (iface));
				}
			}

			return spec;
		}

		static TypeParameterSpec CreateTypeParameter (Type type, TypeSpec declaringType)
		{
			Variance variance;
			switch (type.GenericParameterAttributes & GenericParameterAttributes.VarianceMask) {
			case GenericParameterAttributes.Covariant:
				variance = Variance.Covariant;
				break;
			case GenericParameterAttributes.Contravariant:
				variance = Variance.Contravariant;
				break;
			default:
				variance = Variance.None;
				break;
			}

			SpecialConstraint special = SpecialConstraint.None;
			var import_special = type.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;

			if ((import_special & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0) {
				special |= SpecialConstraint.Struct;
			} else if ((import_special & GenericParameterAttributes.DefaultConstructorConstraint) != 0) {
				special = SpecialConstraint.Constructor;
			}

			if ((import_special & GenericParameterAttributes.ReferenceTypeConstraint) != 0) {
				special |= SpecialConstraint.Class;
			}

			TypeParameterSpec spec;
			var def = new ImportedTypeParameterDefinition (type);
			if (type.DeclaringMethod != null)
				spec = new TypeParameterSpec (type.GenericParameterPosition, def, special, variance, type);
			else
				spec = new TypeParameterSpec (declaringType, type.GenericParameterPosition, def, special, variance, type);

			// Add it now, so any constraint can reference it and get same instance
			import_cache.Add (type, spec);

			var constraints = type.GetGenericParameterConstraints ();
			List<TypeSpec> tparams = null;
			foreach (var ct in constraints) {
				if (ct.IsGenericParameter) {
					if (tparams == null)
						tparams = new List<TypeSpec> ();

					tparams.Add (CreateType (ct));
					continue;
				}

				if (ct.IsClass) {
					if (ct == typeof (ValueType)) {
						spec.BaseType = TypeManager.value_type;
					} else {
						spec.BaseType = CreateType (ct);
					}

					continue;
				}

				spec.AddInterface (CreateType (ct));
			}

			if (spec.BaseType == null)
				spec.BaseType = TypeManager.object_type;

			if (tparams != null)
				spec.TypeArguments = tparams.ToArray ();

			return spec;
		}

		public static TypeSpec ImportType (Type type)
		{
			if (type.HasElementType) {
				var element = type.GetElementType ();
				var spec = ImportType (element);

				if (type.IsArray)
					return ArrayContainer.MakeType (spec, type.GetArrayRank ());
				if (type.IsByRef)
					return ReferenceContainer.MakeType (spec);
				if (type.IsPointer)
					return PointerContainer.MakeType (spec);

				throw new NotImplementedException ("Unknown element type " + type.ToString ());
			}

			TypeSpec dtype;
			if (type.IsNested)
				dtype = ImportType (type.DeclaringType);
			else
				dtype = null;

			return CreateType (type, dtype);
		}

		//
		// Decimal constants cannot be encoded in the constant blob, and thus are marked
		// as IsInitOnly ('readonly' in C# parlance).  We get its value from the 
		// DecimalConstantAttribute metadata.
		//
		static Constant ReadDecimalConstant (FieldInfo fi)
		{
			object[] attrs = fi.GetCustomAttributes (typeof (DecimalConstantAttribute), false);
			if (attrs.Length != 1)
				return null;

			return new DecimalConstant (((DecimalConstantAttribute) attrs [0]).Value, Location.Null);
		}

		static Modifiers ReadMethodModifiers (MethodBase mb, TypeSpec declaringType)
		{
			Modifiers mod;
			var ma = mb.Attributes;
			switch (ma & MethodAttributes.MemberAccessMask) {
			case MethodAttributes.Public:
				mod = Modifiers.PUBLIC;
				break;
			case MethodAttributes.Assembly:
				mod = Modifiers.INTERNAL;
				break;
			case MethodAttributes.Family:
				mod = Modifiers.PROTECTED;
				break;
			case MethodAttributes.FamORAssem:
				mod = Modifiers.PROTECTED | Modifiers.INTERNAL;
				break;
			default:
				mod = Modifiers.PRIVATE;
				break;
			}

			if ((ma & MethodAttributes.Static) != 0) {
				mod |= Modifiers.STATIC;
				return mod;
			}
			if ((ma & MethodAttributes.Abstract) != 0 && declaringType.IsClass) {
				mod |= Modifiers.ABSTRACT;
				return mod;
			}

			if ((ma & MethodAttributes.Final) != 0)
				mod |= Modifiers.SEALED;

			// It can be sealed and override
			if ((ma & MethodAttributes.Virtual) != 0) {
				if ((ma & MethodAttributes.NewSlot) != 0 || !declaringType.IsClass) {
					// No private virtual or sealed virtual
					if ((mod & (Modifiers.PRIVATE | Modifiers.SEALED)) == 0)
						mod |= Modifiers.VIRTUAL;
				} else {
					mod |= Modifiers.OVERRIDE;
				}
			}

			return mod;
		}
	}

	class ImportedMemberDefinition : IMemberDefinition
	{
		protected class AttributesBag
		{
			public static readonly AttributesBag Default = new AttributesBag ();

			public AttributeUsageAttribute AttributeUsage;
			public ObsoleteAttribute Obsolete;
			public string[] Conditionals;
			public string DefaultIndexerName;
			public bool IsNotCLSCompliant;

			public static AttributesBag Read (MemberInfo mi)
			{
				AttributesBag bag = null;
				List<string> conditionals = null;

				// It should not throw any loading exception
				IList<CustomAttributeData> attrs = CustomAttributeData.GetCustomAttributes (mi);

				foreach (var a in attrs) {
					var type = a.Constructor.DeclaringType;
					if (type == typeof (ObsoleteAttribute)) {
						if (bag == null)
							bag = new AttributesBag ();

						var args = a.ConstructorArguments;

						if (args.Count == 1) {
							bag.Obsolete = new ObsoleteAttribute ((string) args[0].Value);
						} else if (args.Count == 2) {
							bag.Obsolete = new ObsoleteAttribute ((string) args[0].Value, (bool) args[1].Value);
						} else {
							bag.Obsolete = new ObsoleteAttribute ();
						}

						continue;
					}

					if (type == typeof (ConditionalAttribute)) {
						if (bag == null)
							bag = new AttributesBag ();

						if (conditionals == null)
							conditionals = new List<string> (2);

						conditionals.Add ((string) a.ConstructorArguments[0].Value);
						continue;
					}

					if (type == typeof (CLSCompliantAttribute)) {
						if (bag == null)
							bag = new AttributesBag ();

						bag.IsNotCLSCompliant = !(bool) a.ConstructorArguments[0].Value;
						continue;
					}

					// Type only attributes
					if (type == typeof (DefaultMemberAttribute)) {
						if (bag == null)
							bag = new AttributesBag ();

						bag.DefaultIndexerName = (string) a.ConstructorArguments[0].Value;
						continue;
					}

					if (type == typeof (AttributeUsageAttribute)) {
						if (bag == null)
							bag = new AttributesBag ();

						bag.AttributeUsage = new AttributeUsageAttribute ((AttributeTargets) a.ConstructorArguments[0].Value);
						foreach (var named in a.NamedArguments) {
							if (named.MemberInfo.Name == "AllowMultiple")
								bag.AttributeUsage.AllowMultiple = (bool) named.TypedValue.Value;
							else if (named.MemberInfo.Name == "Inherited")
								bag.AttributeUsage.Inherited = (bool) named.TypedValue.Value;
						}
						continue;
					}
				}

				if (bag == null)
					return Default;

				if (conditionals != null)
					bag.Conditionals = conditionals.ToArray ();

				return bag;
			}
		}

		protected readonly MemberInfo provider;
		protected AttributesBag cattrs;

		public ImportedMemberDefinition (MemberInfo provider)
		{
			this.provider = provider;
		}

		#region Properties

		public Assembly Assembly {
			get { 
				return provider.Module.Assembly;
			}
		}

		public bool IsImported {
			get {
				return true;
			}
		}

		public virtual string Name {
			get {
				return provider.Name;
			}
		}

		#endregion

		public string[] ConditionalConditions ()
		{
			if (cattrs == null)
				ReadAttributes ();

			return cattrs.Conditionals;
		}

		public ObsoleteAttribute GetAttributeObsolete ()
		{
			if (cattrs == null)
				ReadAttributes ();

			return cattrs.Obsolete;
		}

		public bool IsNotCLSCompliant ()
		{
			if (cattrs == null)
				ReadAttributes ();

			return cattrs.IsNotCLSCompliant;
		}

		protected void ReadAttributes ()
		{
			cattrs = AttributesBag.Read (provider);
		}

		public void SetIsAssigned ()
		{
			// Unused for imported members
		}

		public void SetIsUsed ()
		{
			// Unused for imported members
		}
	}

	class ImportedMethodDefinition : ImportedMemberDefinition, IParametersMember
	{
		readonly AParametersCollection parameters;

		public ImportedMethodDefinition (MethodBase provider, AParametersCollection parameters)
			: base (provider)
		{
			this.parameters = parameters;
		}

		#region Properties

		public AParametersCollection Parameters {
			get {
				return parameters;
			}
		}

		public TypeSpec MemberType {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion
	}

	class ImportedGenericMethodDefinition : ImportedMethodDefinition, IGenericMethodDefinition
	{
		TypeParameterSpec[] tparams;

		public ImportedGenericMethodDefinition (MethodInfo provider, AParametersCollection parameters, TypeParameterSpec[] tparams)
			: base (provider, parameters)
		{
			this.tparams = tparams;
		}

		#region Properties

		public TypeParameterSpec[] TypeParameters {
			get {
				return tparams;
			}
		}

		public int TypeParametersCount {
			get {
				return tparams.Length;
			}
		}

		#endregion
	}

	class ImportedTypeDefinition : ImportedMemberDefinition, ITypeDefinition
	{
		TypeParameterSpec[] tparams;
		string name;

		public ImportedTypeDefinition (Type type)
			: base (type)
		{
		}

		#region Properties

		public override string Name {
			get {
				if (name == null) {
					name = base.Name;
					if (tparams != null)
						name = name.Substring (0, name.IndexOf ('`'));
				}

				return name;
			}
		}

		public string Namespace {
			get {
				return ((Type) provider).Namespace;
			}
		}

		public int TypeParametersCount {
			get {
				return tparams == null ? 0 : tparams.Length;
			}
		}

		public TypeParameterSpec[] TypeParameters {
			get {
				return tparams;
			}
			set {
				tparams = value;
			}
		}

		#endregion

		public TypeSpec GetAttributeCoClass ()
		{
			// TODO: Use ReadAttributes
			var attr =  provider.GetCustomAttributes (typeof (CoClassAttribute), false);
			if (attr.Length < 1)
				return null;

			return Import.CreateType (((CoClassAttribute) attr[0]).CoClass);
		}

		public string GetAttributeDefaultMember ()
		{
			if (cattrs == null)
				ReadAttributes ();

			return cattrs.DefaultIndexerName;
		}

		public AttributeUsageAttribute GetAttributeUsage (PredefinedAttribute pa)
		{
			if (cattrs == null)
				ReadAttributes ();

			return cattrs.AttributeUsage;
		}

		public MemberCache LoadMembers (TypeSpec declaringType)
		{
			var loading_type = (Type) provider;
			const BindingFlags all_members = BindingFlags.DeclaredOnly |
				BindingFlags.Static | BindingFlags.Instance |
				BindingFlags.Public | BindingFlags.NonPublic;

			const MethodAttributes explicit_impl = MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.HideBySig |
					MethodAttributes.Final;

			Dictionary<MethodBase, MethodSpec> possible_accessors = null;
			MemberSpec imported;
			MethodInfo m;

			//
			// This requires methods to be returned first which seems to work for both Mono and .NET
			//
			MemberInfo[] all;
			try {
				all = loading_type.GetMembers (all_members);
			} catch (Exception e) {
				throw new InternalErrorException (e, "Could not import type `{0}' from `{1}'",
					declaringType.GetSignatureForError (), declaringType.Assembly.Location);
			}

			var cache = new MemberCache (all.Length);
			foreach (var member in all) {
				switch (member.MemberType) {
				case MemberTypes.Constructor:
				case MemberTypes.Method:
					MethodBase mb = (MethodBase) member;

					// Ignore explicitly implemented members
					if ((mb.Attributes & explicit_impl) == explicit_impl && (mb.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private)
						continue;

					// Ignore compiler generated methods
					if (mb.IsPrivate && mb.IsDefined (typeof (CompilerGeneratedAttribute), false))
						continue;

					imported = Import.CreateMethod (mb, declaringType);
					if (imported.Kind == MemberKind.Method && !imported.IsGeneric) {
						if (possible_accessors == null)
							possible_accessors = new Dictionary<MethodBase, MethodSpec> (ReferenceEquality<MethodBase>.Default);

						// There are no metadata rules for accessors, we have to consider any method as possible candidate
						possible_accessors.Add (mb, (MethodSpec) imported);
					}

					break;
				case MemberTypes.Property:
					if (possible_accessors == null)
						continue;

					var p = (PropertyInfo) member;
					//
					// Links possible accessors with property
					//
					MethodSpec get, set;
					m = p.GetGetMethod (true);
					if (m == null || !possible_accessors.TryGetValue (m, out get))
						get = null;

					m = p.GetSetMethod (true);
					if (m == null || !possible_accessors.TryGetValue (m, out set))
						set = null;

					// No accessors registered (e.g. explicit implementation)
					if (get == null && set == null)
						continue;

					imported = Import.CreateProperty (p, declaringType, get, set);
					if (imported == null)
						continue;

					break;
				case MemberTypes.Event:
					if (possible_accessors == null)
						continue;

					var e = (EventInfo) member;
					//
					// Links accessors with event
					//
					MethodSpec add, remove;
					m = e.GetAddMethod (true);
					if (m == null || !possible_accessors.TryGetValue (m, out add))
						add = null;

					m = e.GetRemoveMethod (true);
					if (m == null || !possible_accessors.TryGetValue (m, out remove))
						remove = null;

					// Both accessors are required
					if (add == null || remove == null)
						continue;

					imported = Import.CreateEvent (e, declaringType, add, remove);
					break;
				case MemberTypes.Field:
					var fi = (FieldInfo) member;

					// Ignore compiler generated fields
					if (fi.IsPrivate && fi.IsDefined (typeof (CompilerGeneratedAttribute), false))
						continue;

					imported = Import.CreateField (fi, declaringType);
					if (imported == null)
						continue;

					break;
				case MemberTypes.NestedType:
					Type t = (Type) member;

					// Ignore compiler generated types, mostly lambda containers
					if (t.IsNotPublic && t.IsDefined (typeof (CompilerGeneratedAttribute), false))
						continue;

					imported = Import.CreateType (t, declaringType);
					break;
				default:
					throw new NotImplementedException (member.ToString ());
				}

				cache.AddMember (imported);
			}

			if (declaringType.IsInterface && declaringType.Interfaces != null) {
				foreach (var iface in declaringType.Interfaces) {
					cache.AddInterface (iface);
				}
			}

			return cache;
		}
	}

	class ImportedTypeParameterDefinition : ImportedMemberDefinition, ITypeDefinition
	{
		public ImportedTypeParameterDefinition (Type type)
			: base (type)
		{
		}

		#region Properties

		public string Namespace {
			get {
				return null;
			}
		}

		public int TypeParametersCount {
			get {
				return 0;
			}
		}

		public TypeParameterSpec[] TypeParameters {
			get {
				return null;
			}
		}

		#endregion

		public TypeSpec GetAttributeCoClass ()
		{
			return null;
		}

		public string GetAttributeDefaultMember ()
		{
			throw new NotSupportedException ();
		}

		public AttributeUsageAttribute GetAttributeUsage (PredefinedAttribute pa)
		{
			throw new NotSupportedException ();
		}

		public MemberCache LoadMembers (TypeSpec declaringType)
		{
			throw new NotImplementedException ();
		}
	}
}
