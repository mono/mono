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
	public class ReflectionMetaImporter
	{
		Dictionary<Type, TypeSpec> import_cache;
		Dictionary<Type, PredefinedTypeSpec> type_2_predefined;

		public ReflectionMetaImporter ()
		{
			import_cache = new Dictionary<Type, TypeSpec> (1024, ReferenceEquality<Type>.Default);
			IgnorePrivateMembers = true;
		}

		#region Properties

		public bool IgnorePrivateMembers { get; set; }

		#endregion

		public void Initialize ()
		{
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

		public FieldSpec CreateField (FieldInfo fi, TypeSpec declaringType)
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
					// Ignore private fields (even for error reporting) to not require extra dependencies
					if (IgnorePrivateMembers || fi.IsDefined (typeof (CompilerGeneratedAttribute), false))
						return null;

					mod = Modifiers.PRIVATE;
					break;
			}

			var definition = new ImportedMemberDefinition (fi);
			TypeSpec field_type;

			try {
				field_type = ImportType (fi.FieldType, fi, 0);
			} catch (Exception e) {
				// TODO: I should construct fake TypeSpec based on TypeRef signature
				// but there is no way to do it with System.Reflection
				throw new InternalErrorException (e, "Cannot import field `{0}.{1}' referenced in assembly `{2}'",
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

		public EventSpec CreateEvent (EventInfo ei, TypeSpec declaringType, MethodSpec add, MethodSpec remove)
		{
			add.IsAccessor = true;
			remove.IsAccessor = true;

			if (add.Modifiers != remove.Modifiers)
				throw new NotImplementedException ("Different accessor modifiers " + ei.Name);

			var definition = new ImportedMemberDefinition (ei);
			return new EventSpec (declaringType, definition, ImportType (ei.EventHandlerType, ei, 0), add.Modifiers, add, remove);
		}

		T[] CreateGenericParameters<T> (Type type, TypeSpec declaringType) where T : TypeSpec
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

			return CreateGenericParameters<T> (parent_owned_count, tparams, null, 0);
		}

		T[] CreateGenericParameters<T> (int first, Type[] tparams, ICustomAttributeProvider ca, int dynamicCursor) where T : TypeSpec
		{
			var tspec = new T [tparams.Length - first];
			for (int pos = first; pos < tparams.Length; ++pos) {
				var type = tparams[pos];
				int index = pos - first;

				if (type.HasElementType) {
					var element = type.GetElementType ();
					var spec = ImportType (element);

					if (type.IsArray) {
						tspec[index] = (T) (TypeSpec) ArrayContainer.MakeType (spec, type.GetArrayRank ());
						continue;
					}

					throw new NotImplementedException ("Unknown element type " + type.ToString ());
				}

				tspec [index] = (T) CreateType (type, ca, dynamicCursor + index + 1);
			}

			return tspec;
		}

		public MethodSpec CreateMethod (MethodBase mb, TypeSpec declaringType)
		{
			Modifiers mod = ReadMethodModifiers (mb, declaringType);
			TypeParameterSpec[] tparams;
			ImportedMethodDefinition definition;

			var parameters = CreateParameters (declaringType, mb.GetParameters (), mb);

			if (mb.IsGenericMethod) {
				if (!mb.IsGenericMethodDefinition)
					throw new NotSupportedException ("assert");

				tparams = CreateGenericParameters<TypeParameterSpec>(0, mb.GetGenericArguments (), null, 0);
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

				var mi = (MethodInfo) mb;
				returnType = ImportType (mi.ReturnType, mi.ReturnTypeCustomAttributes, 0);

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
		// Imports System.Reflection parameters
		//
		AParametersCollection CreateParameters (TypeSpec parent, ParameterInfo[] pi, MethodBase method)
		{
			int varargs = method != null && (method.CallingConvention & CallingConventions.VarArgs) != 0 ? 1 : 0;

			if (pi.Length == 0 && varargs == 0)
				return ParametersCompiled.EmptyReadOnlyParameters;

			TypeSpec[] types = new TypeSpec[pi.Length + varargs];
			IParameterData[] par = new IParameterData[pi.Length + varargs];
			bool is_params = false;
			for (int i = 0; i < pi.Length; i++) {
				ParameterInfo p = pi[i];
				Parameter.Modifier mod = 0;
				Expression default_value = null;
				if (p.ParameterType.IsByRef) {
					if ((p.Attributes & (ParameterAttributes.Out | ParameterAttributes.In)) == ParameterAttributes.Out)
						mod = Parameter.Modifier.OUT;
					else
						mod = Parameter.Modifier.REF;

					//
					// Strip reference wrapping
					//
					var el = p.ParameterType.GetElementType ();
					types[i] = ImportType (el, p, 0);	// TODO: 1 to be csc compatible
				} else if (i == 0 && method.IsStatic && parent.IsStatic && // TODO: parent.Assembly.IsExtension &&
					HasExtensionAttribute (CustomAttributeData.GetCustomAttributes (method)) != null) {
					mod = Parameter.Modifier.This;
					types[i] = ImportType (p.ParameterType);
				} else {
					types[i] = ImportType (p.ParameterType, p, 0);

					if (i >= pi.Length - 2 && types[i] is ArrayContainer) {
						var cattrs = CustomAttributeData.GetCustomAttributes (p);
						if (cattrs != null && cattrs.Any (l => l.Constructor.DeclaringType == typeof (ParamArrayAttribute))) {
							mod = Parameter.Modifier.PARAMS;
							is_params = true;
						}
					}

					if (!is_params && p.IsOptional) {
						object value = p.DefaultValue;
						var ptype = types[i];
						if (((p.Attributes & ParameterAttributes.HasDefault) != 0 && ptype.Kind != MemberKind.TypeParameter) || p.IsDefined (typeof (DecimalConstantAttribute), false)) {
							var dtype = value == null ? ptype : ImportType (value.GetType ());
							default_value = Constant.CreateConstant (null, dtype, value, Location.Null);
						} else if (value == Missing.Value) {
							default_value = EmptyExpression.MissingValue;
						} else {
							default_value = new DefaultValueExpression (new TypeExpression (ptype, Location.Null), Location.Null);
						}
					}
				}

				par[i] = new ParameterData (p.Name, mod, default_value);
			}

			if (varargs != 0) {
				par[par.Length - 1] = new ArglistParameter (Location.Null);
				types[types.Length - 1] = InternalType.Arglist;
			}

			return method != null ?
				new ParametersImported (par, types, varargs != 0, is_params) :
				new ParametersImported (par, types, is_params);
		}


		//
		// Returns null when the property is not valid C# property
		//
		public PropertySpec CreateProperty (PropertyInfo pi, TypeSpec declaringType, MethodSpec get, MethodSpec set)
		{
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
					spec = new IndexerSpec (declaringType, new ImportedIndexerDefinition (pi, param), type, param, pi, mod);
			}

			if (spec == null)
				spec = new PropertySpec (MemberKind.Property, declaringType, new ImportedMemberDefinition (pi), type, pi, mod);

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

		public TypeSpec CreateType (Type type)
		{
			return CreateType (type, null, 0);
		}

		TypeSpec CreateType (Type type, ICustomAttributeProvider ca, int dynamicCursor)
		{
			TypeSpec declaring_type;
			if (type.IsNested && !type.IsGenericParameter)
				declaring_type = CreateType (type.DeclaringType, type.DeclaringType, 0);
			else
				declaring_type = null;

			return CreateType (type, declaring_type, ca, dynamicCursor);
		}

		public TypeSpec CreateType (Type type, TypeSpec declaringType, ICustomAttributeProvider ca, int dynamicCursor)
		{
			TypeSpec spec;
			if (import_cache.TryGetValue (type, out spec)) {
				if (ca == null)
					return spec;

				if (type == typeof (object)) {
					if (IsDynamicType (ca, dynamicCursor))
						return InternalType.Dynamic;

					return spec;
				}

				if (!spec.IsGeneric)
					return spec;

#if NET_4_0
				if (!ca.IsDefined (typeof (DynamicAttribute), false))
#endif
					return spec;

				// We've found same object in the cache but this one has a dynamic custom attribute
				// and it's most likely dynamic version of same type IFoo<object> agains IFoo<dynamic>
				// Do resolve the type process again in that case
			}

			if (type.IsGenericType && !type.IsGenericTypeDefinition) {	
				var type_def = type.GetGenericTypeDefinition ();
				var targs = CreateGenericParameters<TypeSpec> (0, type.GetGenericArguments (), ca, dynamicCursor);
				if (declaringType == null) {
					// Simple case, no nesting
					spec = CreateType (type_def, null, null, 0);
					spec = spec.MakeGenericType (targs);
				} else {
					//
					// Nested type case, converting .NET types like
					// A`1.B`1.C`1<int, long, string> to typespec like
					// A<int>.B<long>.C<string>
					//
					var nested_hierarchy = new List<TypeSpec> ();
					while (declaringType.IsNested) {
						nested_hierarchy.Add (declaringType);
						declaringType = declaringType.DeclaringType;
					}

					int targs_pos = 0;
					if (declaringType.Arity > 0) {
						spec = declaringType.MakeGenericType (targs.Skip (targs_pos).Take (declaringType.Arity).ToArray ());
						targs_pos = spec.Arity;
					} else {
						spec = declaringType;
					}

					for (int i = nested_hierarchy.Count; i != 0; --i) {
						var t = nested_hierarchy [i - 1];
						spec = MemberCache.FindNestedType (spec, t.Name, t.Arity);
						if (t.Arity > 0) {
							spec = spec.MakeGenericType (targs.Skip (targs_pos).Take (spec.Arity).ToArray ());
							targs_pos += t.Arity;
						}
					}

					string name = type.Name;
					int index = name.IndexOf ('`');
					if (index > 0)
						name = name.Substring (0, index);

					spec = MemberCache.FindNestedType (spec, name, targs.Length - targs_pos);
					if (spec.Arity > 0) {
						spec = spec.MakeGenericType (targs.Skip (targs_pos).ToArray ());
					}
				}

				// Don't add generic type with dynamic arguments, they can interfere with same type
				// using object type arguments
				if (!spec.HasDynamicElement) {

					// Add to reading cache to speed up reading
					if (!import_cache.ContainsKey (type))
						import_cache.Add (type, spec);
				}

				return spec;
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
			} else if (type.IsClass || type.IsAbstract) {  				// System.Reflection: System.Enum returns false for IsClass
				if ((ma & TypeAttributes.Sealed) != 0 && type.IsSubclassOf (typeof (MulticastDelegate))) {
					kind = MemberKind.Delegate;
					mod |= Modifiers.SEALED;
				} else {
					kind = MemberKind.Class;
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

			var definition = new ImportedTypeDefinition (this, type);
			PredefinedTypeSpec pt;

			if (kind == MemberKind.Enum) {
				const BindingFlags underlying_member = BindingFlags.DeclaredOnly |
					BindingFlags.Instance |
					BindingFlags.Public | BindingFlags.NonPublic;

				var type_members = type.GetFields (underlying_member);
				foreach (var type_member in type_members) {
					spec = new EnumSpec (declaringType, definition, CreateType (type_member.FieldType), type, mod);
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

			//
			// Two stage setup as the base type can be inflated declaring type
			//
			if (declaringType == null || !IgnorePrivateMembers)
				ImportTypeBase (spec, type);

			return spec;
		}

		public void ImportTypeBase (Type type)
		{
			TypeSpec spec = import_cache[type];
			if (spec != null)
				ImportTypeBase (spec, type);
		}

		void ImportTypeBase (TypeSpec spec, Type type)
		{
			if (spec.Kind == MemberKind.Interface)
				spec.BaseType = TypeManager.object_type;
			else if (type.BaseType != null) {
				if (type.BaseType.IsGenericType)
					spec.BaseType = CreateType (type.BaseType, type, 0);
				else
					spec.BaseType = CreateType (type.BaseType);
			}

			var ifaces = type.GetInterfaces ();
			if (ifaces.Length > 0) {
				foreach (Type iface in ifaces) {
					spec.AddInterface (CreateType (iface));
				}
			}
		}

		TypeParameterSpec CreateTypeParameter (Type type, TypeSpec declaringType)
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

		static Type HasExtensionAttribute (IList<CustomAttributeData> attributes)
		{
			foreach (var attr in attributes) {
				var dt = attr.Constructor.DeclaringType;
				if (dt.Name == "ExtensionAttribute" && dt.Namespace == "System.Runtime.CompilerServices") {
					return dt;
				}
			}

			return null;
		}

		public void ImportAssembly (Assembly assembly, Namespace targetNamespace)
		{
			Type extension_type = HasExtensionAttribute (CustomAttributeData.GetCustomAttributes (assembly));

			//
			// This part tries to simulate loading of top-level
			// types only, any missing dependencies are ignores here.
			// Full error report is reported later when the type is
			// actually used
			//
			Type[] all_types;
			try {
				all_types = assembly.GetTypes ();
			} catch (ReflectionTypeLoadException e) {
				all_types = e.Types;
			}

			ImportTypes (all_types, targetNamespace, extension_type);
		}

		public void ImportModule (Module module, Namespace targetNamespace)
		{
			Type extension_type = HasExtensionAttribute (CustomAttributeData.GetCustomAttributes (module));

			Type[] all_types;
			try {
				all_types = module.GetTypes ();
			} catch (ReflectionTypeLoadException e) {
				all_types = e.Types;
				throw;
			}

			ImportTypes (all_types, targetNamespace, extension_type);
		}

		void ImportTypes (Type[] types, Namespace targetNamespace, Type extension_type)
		{
			Namespace ns = targetNamespace;
			string prev_namespace = null;
			foreach (var t in types) {
				if (t == null)
					continue;

				// Be careful not to trigger full parent type loading
				if (t.MemberType == MemberTypes.NestedType)
					continue;

				if (t.Name[0] == '<')
					continue;

				var it = CreateType (t, null, t, 0);
				if (it == null)
					continue;

				if (prev_namespace != t.Namespace) {
					ns = t.Namespace == null ? targetNamespace : targetNamespace.GetNamespace (t.Namespace, true);
					prev_namespace = t.Namespace;
				}

				ns.AddType (it);

				if (it.IsStatic && extension_type != null && t.IsDefined (extension_type, false)) {
					it.SetExtensionMethodContainer ();
				}
			}
		}

		public TypeSpec ImportType (Type type)
		{
			return ImportType (type, null, 0);
		}

		public TypeSpec ImportType (Type type, ICustomAttributeProvider ca, int dynamicCursor)
		{
			if (type.HasElementType) {
				var element = type.GetElementType ();
				var spec = ImportType (element, ca, dynamicCursor + 1);

				if (type.IsArray)
					return ArrayContainer.MakeType (spec, type.GetArrayRank ());
				if (type.IsByRef)
					return ReferenceContainer.MakeType (spec);
				if (type.IsPointer)
					return PointerContainer.MakeType (spec);

				throw new NotImplementedException ("Unknown element type " + type.ToString ());
			}

			return CreateType (type, ca, dynamicCursor);
		}

		static bool IsDynamicType (ICustomAttributeProvider ca, int index)
		{
#if NET_4_0
			if (ca.IsDefined (typeof (DynamicAttribute), false)) {
				if (index == 0)
					return true;

				var v = (DynamicAttribute) ca.GetCustomAttributes (typeof (DynamicAttribute), false)[0];
				return v.TransformFlags[index];
			}
#endif
			return false;
		}

		//
		// Decimal constants cannot be encoded in the constant blob, and thus are marked
		// as IsInitOnly ('readonly' in C# parlance).  We get its value from the 
		// DecimalConstantAttribute metadata.
		//
		static Constant ReadDecimalConstant (ICustomAttributeProvider fi)
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

	class ImportedIndexerDefinition : ImportedMemberDefinition, IParametersMember
	{
		readonly AParametersCollection parameters;

		public ImportedIndexerDefinition (PropertyInfo provider, AParametersCollection parameters)
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
		ReflectionMetaImporter meta_import;

		public ImportedTypeDefinition (ReflectionMetaImporter metaImport, Type type)
			: base (type)
		{
			this.meta_import = metaImport;
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

			return meta_import.CreateType (((CoClassAttribute) attr[0]).CoClass);
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

		public void LoadMembers (TypeSpec declaringType, bool onlyTypes, ref MemberCache cache)
		{
			//
			// Not interested in members of nested private types unless the importer needs them
			//
			if (declaringType.IsPrivate && meta_import.IgnorePrivateMembers) {
				cache = MemberCache.Empty;
				return;
			}

			var loading_type = (Type) provider;
			const BindingFlags all_members = BindingFlags.DeclaredOnly |
				BindingFlags.Static | BindingFlags.Instance |
				BindingFlags.Public | BindingFlags.NonPublic;

			const MethodAttributes explicit_impl = MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.HideBySig |
					MethodAttributes.Final;

			Dictionary<MethodBase, MethodSpec> possible_accessors = null;
			List<EventSpec> imported_events = null;
			EventSpec event_spec;
			MemberSpec imported;
			MethodInfo m;
			MemberInfo[] all;
			try {
				all = loading_type.GetMembers (all_members);
			} catch (Exception e) {
				throw new InternalErrorException (e, "Could not import type `{0}' from `{1}'",
					declaringType.GetSignatureForError (), declaringType.Assembly.Location);
			}

			if (cache == null) {
				cache = new MemberCache (all.Length);

				//
				// Do the types first as they can be referenced by the members before
				// they are found or inflated
				//
				foreach (var member in all) {
					if (member.MemberType != MemberTypes.NestedType)
						continue;

					Type t = (Type) member;

					// Ignore compiler generated types, mostly lambda containers
					if (t.IsNotPublic && t.IsDefined (typeof (CompilerGeneratedAttribute), false))
						continue;

					imported = meta_import.CreateType (t, declaringType, t, 0);
					cache.AddMember (imported);
				}

				foreach (var member in all) {
					if (member.MemberType != MemberTypes.NestedType)
						continue;

					meta_import.ImportTypeBase ((Type) member);
				}
			}

			if (!onlyTypes) {
				//
				// The logic here requires methods to be returned first which seems to work for both Mono and .NET
				//
				foreach (var member in all) {
					switch (member.MemberType) {
					case MemberTypes.Constructor:
					case MemberTypes.Method:
						MethodBase mb = (MethodBase) member;
						var attrs = mb.Attributes;

						if ((attrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Private) {
							if (meta_import.IgnorePrivateMembers)
								continue;

							// Ignore explicitly implemented members
							if ((attrs & explicit_impl) == explicit_impl)
								continue;

							// Ignore compiler generated methods
							if (mb.IsDefined (typeof (CompilerGeneratedAttribute), false))
								continue;
						}

						imported = meta_import.CreateMethod (mb, declaringType);
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

						imported = meta_import.CreateProperty (p, declaringType, get, set);
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

						event_spec = meta_import.CreateEvent (e, declaringType, add, remove);
						if (!meta_import.IgnorePrivateMembers) {
							if (imported_events == null)
								imported_events = new List<EventSpec> ();

							imported_events.Add (event_spec);
						}

						imported = event_spec;
						break;
					case MemberTypes.Field:
						var fi = (FieldInfo) member;

						imported = meta_import.CreateField (fi, declaringType);
						if (imported == null)
							continue;

						//
						// For dynamic binder event has to be fully restored to allow operations
						// within the type container to work correctly
						//
						if (imported_events != null) {
							// The backing event field should be private but it may not
							int index = imported_events.FindIndex (l => l.Name == fi.Name);
							if (index >= 0) {
								event_spec = imported_events[index];
								event_spec.BackingField = (FieldSpec) imported;
								imported_events.RemoveAt (index);
								continue;
							}
						}

						break;
					case MemberTypes.NestedType:
						// Already in the cache from the first pass
						continue;
					default:
						throw new NotImplementedException (member.ToString ());
					}

					cache.AddMember (imported);
				}
			}

			if (declaringType.IsInterface && declaringType.Interfaces != null) {
				foreach (var iface in declaringType.Interfaces) {
					cache.AddInterface (iface);
				}
			}
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

		public void LoadMembers (TypeSpec declaringType, bool onlyTypes, ref MemberCache cache)
		{
			throw new NotImplementedException ();
		}
	}
}
