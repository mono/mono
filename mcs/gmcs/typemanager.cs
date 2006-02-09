//
// typemanager.cs: C# type manager
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Ravi Pratap     (ravi@ximian.com)
//         Marek Safar     (marek.safar@seznam.cz)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

//
// We will eventually remove the SIMPLE_SPEEDUP, and should never change 
// the behavior of the compilation.  This can be removed if we rework
// the code to get a list of namespaces available.
//
#define SIMPLE_SPEEDUP

using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Mono.CSharp {

public partial class TypeManager {
	//
	// A list of core types that the compiler requires or uses
	//
	static public Type object_type;
	static public Type value_type;
	static public Type string_type;
	static public Type int32_type;
	static public Type uint32_type;
	static public Type int64_type;
	static public Type uint64_type;
	static public Type float_type;
	static public Type double_type;
	static public Type char_type;
	static public Type char_ptr_type;
	static public Type short_type;
	static public Type decimal_type;
	static public Type bool_type;
	static public Type sbyte_type;
	static public Type byte_type;
	static public Type ushort_type;
	static public Type enum_type;
	static public Type delegate_type;
	static public Type multicast_delegate_type;
	static public Type void_type;
	static public Type null_type;
	static public Type enumeration_type;
	static public Type array_type;
	static public Type runtime_handle_type;
	static public Type icloneable_type;
	static public Type type_type;
	static public Type ienumerator_type;
	static public Type ienumerable_type;
	static public Type idisposable_type;
	static public Type iconvertible_type;
	static public Type default_member_type;
	static public Type iasyncresult_type;
	static public Type asynccallback_type;
	static public Type intptr_type;
	static public Type monitor_type;
	static public Type runtime_field_handle_type;
	static public Type runtime_argument_handle_type;
	static public Type attribute_type;
	static public Type attribute_usage_type;
	static public Type decimal_constant_attribute_type;
	static public Type dllimport_type;
	static public Type unverifiable_code_type;
	static public Type methodimpl_attr_type;
	static public Type marshal_as_attr_type;
	static public Type param_array_type;
	static public Type void_ptr_type;
	static public Type indexer_name_type;
	static public Type exception_type;
	static public Type invalid_operation_exception_type;
	static public Type not_supported_exception_type;
	static public Type obsolete_attribute_type;
	static public Type conditional_attribute_type;
	static public Type in_attribute_type;
	static public Type out_attribute_type;
	static public Type anonymous_method_type;
	static public Type cls_compliant_attribute_type;
	static public Type typed_reference_type;
	static public Type arg_iterator_type;
	static public Type mbr_type;
	static public Type struct_layout_attribute_type;
	static public Type field_offset_attribute_type;
	static public Type security_attr_type;
	static public Type required_attr_type;
	static public Type guid_attr_type;
	static public Type assembly_culture_attribute_type;
	static public Type coclass_attr_type;
	static public Type comimport_attr_type;

	/// 
	/// .NET 2.0
	///
	static internal Type runtime_compatibility_attr_type;
	static internal Type compiler_generated_attr_type;
	static internal Type fixed_buffer_attr_type;
	static internal Type default_charset_type;
	static internal Type internals_visible_attr_type;

	// 
	// Expressions representing the internal types.  Used during declaration
	// definition.
	//
	static public TypeExpr system_object_expr, system_string_expr; 
	static public TypeExpr system_boolean_expr, system_decimal_expr;
	static public TypeExpr system_single_expr, system_double_expr;
	static public TypeExpr system_sbyte_expr, system_byte_expr;
	static public TypeExpr system_int16_expr, system_uint16_expr;
	static public TypeExpr system_int32_expr, system_uint32_expr;
	static public TypeExpr system_int64_expr, system_uint64_expr;
	static public TypeExpr system_char_expr, system_void_expr;
	static public TypeExpr system_asynccallback_expr;
	static public TypeExpr system_iasyncresult_expr;
	static public TypeExpr system_valuetype_expr;
	static public TypeExpr system_intptr_expr;

	//
	// This is only used when compiling corlib
	//
	static public Type system_int32_type;
	static public Type system_array_type;
	static public Type system_type_type;
	static public Type system_assemblybuilder_type;
	static public MethodInfo system_int_array_get_length;
	static public MethodInfo system_int_array_get_rank;
	static public MethodInfo system_object_array_clone;
	static public MethodInfo system_int_array_get_length_int;
	static public MethodInfo system_int_array_get_lower_bound_int;
	static public MethodInfo system_int_array_get_upper_bound_int;
	static public MethodInfo system_void_array_copyto_array_int;

	
	//
	// Internal, not really used outside
	//
	static Type runtime_helpers_type;
	
	//
	// These methods are called by code generated by the compiler
	//
	static public MethodInfo string_concat_string_string;
	static public MethodInfo string_concat_string_string_string;
	static public MethodInfo string_concat_string_string_string_string;
	static public MethodInfo string_concat_string_dot_dot_dot;
	static public MethodInfo string_concat_object_object;
	static public MethodInfo string_concat_object_object_object;
	static public MethodInfo string_concat_object_dot_dot_dot;
	static public MethodInfo string_isinterneted_string;
	static public MethodInfo system_type_get_type_from_handle;
	static public MethodInfo bool_movenext_void;
	static public MethodInfo ienumerable_getenumerator_void;
	static public MethodInfo void_reset_void;
	static public MethodInfo void_dispose_void;
	static public MethodInfo void_monitor_enter_object;
	static public MethodInfo void_monitor_exit_object;
	static public MethodInfo void_initializearray_array_fieldhandle;
	static public MethodInfo int_getlength_int;
	static public MethodInfo delegate_combine_delegate_delegate;
	static public MethodInfo delegate_remove_delegate_delegate;
	static public MethodInfo int_get_offset_to_string_data;
	static public MethodInfo int_array_get_length;
	static public MethodInfo int_array_get_rank;
	static public MethodInfo object_array_clone;
	static public MethodInfo int_array_get_length_int;
	static public MethodInfo int_array_get_lower_bound_int;
	static public MethodInfo int_array_get_upper_bound_int;
	static public MethodInfo void_array_copyto_array_int;
	static public PropertyInfo ienumerator_getcurrent;
	
	//
	// The attribute constructors.
	//
	static public ConstructorInfo object_ctor;
	static private ConstructorInfo cons_param_array_attribute;
	static public ConstructorInfo void_decimal_ctor_five_args;
	static public ConstructorInfo void_decimal_ctor_int_arg;
	static public ConstructorInfo unverifiable_code_ctor;
	static public ConstructorInfo default_member_ctor;
	static public ConstructorInfo decimal_constant_attribute_ctor;
	static internal ConstructorInfo struct_layout_attribute_ctor;
	static public ConstructorInfo field_offset_attribute_ctor;
	
	///
	/// A new in C# 2.0
	/// 
	static internal CustomAttributeBuilder compiler_generated_attr;
	static internal ConstructorInfo fixed_buffer_attr_ctor;

	static PtrHashtable builder_to_declspace;

	static PtrHashtable builder_to_member_cache;

	// <remarks>
	//   Tracks the interfaces implemented by typebuilders.  We only
	//   enter those who do implement or or more interfaces
	// </remarks>
	static PtrHashtable builder_to_ifaces;

	// <remarks>
	//   Maps PropertyBuilder to a Type array that contains
	//   the arguments to the indexer
	// </remarks>
	static Hashtable indexer_arguments;

	// <remarks>
	//   Maps a MethodBase to its ParameterData (either InternalParameters or ReflectionParameters)
	// <remarks>
	static Hashtable method_params;

	// <remarks>
	//  A hash table from override methods to their base virtual method.
	// <remarks>
	static Hashtable method_overrides;

	// <remarks>
	//  Keeps track of methods
	// </remarks>

	static Hashtable builder_to_method;

	// <remarks>
	//  Contains all public types from referenced assemblies.
	//  This member is used only if CLS Compliance verification is required.
	// </remarks>
	public static Hashtable AllClsTopLevelTypes;

	static Hashtable fieldbuilders_to_fields;
	static Hashtable fields;

	static PtrHashtable assembly_internals_vis_attrs;

	struct Signature {
		public string name;
		public Type [] args;
	}

	public static void CleanUp ()
	{
		// Lets get everything clean so that we can collect before generating code
		builder_to_declspace = null;
		builder_to_member_cache = null;
		builder_to_ifaces = null;
		indexer_arguments = null;
		method_params = null;
		builder_to_method = null;
		
		fields = null;
		fieldbuilders_to_fields = null;
		events = null;
		priv_fields_events = null;

		type_hash = null;

		assembly_internals_vis_attrs = null;
		
		CleanUpGenerics ();
		TypeHandle.CleanUp ();
	}

	/// <summary>
	///   A filter for Findmembers that uses the Signature object to
	///   extract objects
	/// </summary>
	static bool SignatureFilter (MemberInfo mi, object criteria)
	{
		Signature sig = (Signature) criteria;

		if (!(mi is MethodBase))
			return false;
		
		if (mi.Name != sig.name)
			return false;

		int count = sig.args.Length;
		
		if (mi is MethodBuilder || mi is ConstructorBuilder){
			Type [] candidate_args = GetParameterData ((MethodBase) mi).Types;

			if (candidate_args.Length != count)
				return false;
			
			for (int i = 0; i < count; i++)
				if (candidate_args [i] != sig.args [i])
					return false;
			
			return true;
		} else {
			ParameterInfo [] pars = ((MethodBase) mi).GetParameters ();

			if (pars.Length != count)
				return false;

			for (int i = 0; i < count; i++)
				if (pars [i].ParameterType != sig.args [i])
					return false;
			return true;
		}
	}

	// A delegate that points to the filter above.
	static MemberFilter signature_filter;

	//
	// These are expressions that represent some of the internal data types, used
	// elsewhere
	//
	static void InitExpressionTypes ()
	{
		system_object_expr  = new TypeLookupExpression ("System.Object");
		system_string_expr  = new TypeLookupExpression ("System.String");
		system_boolean_expr = new TypeLookupExpression ("System.Boolean");
		system_decimal_expr = new TypeLookupExpression ("System.Decimal");
		system_single_expr  = new TypeLookupExpression ("System.Single");
		system_double_expr  = new TypeLookupExpression ("System.Double");
		system_sbyte_expr   = new TypeLookupExpression ("System.SByte");
		system_byte_expr    = new TypeLookupExpression ("System.Byte");
		system_int16_expr   = new TypeLookupExpression ("System.Int16");
		system_uint16_expr  = new TypeLookupExpression ("System.UInt16");
		system_int32_expr   = new TypeLookupExpression ("System.Int32");
		system_uint32_expr  = new TypeLookupExpression ("System.UInt32");
		system_int64_expr   = new TypeLookupExpression ("System.Int64");
		system_uint64_expr  = new TypeLookupExpression ("System.UInt64");
		system_char_expr    = new TypeLookupExpression ("System.Char");
		system_void_expr    = new TypeLookupExpression ("System.Void");
		system_asynccallback_expr = new TypeLookupExpression ("System.AsyncCallback");
		system_iasyncresult_expr = new TypeLookupExpression ("System.IAsyncResult");
		system_valuetype_expr  = new TypeLookupExpression ("System.ValueType");
		system_intptr_expr  = new TypeLookupExpression ("System.IntPtr");
	}

	static TypeManager ()
	{
		Reset ();

		signature_filter = new MemberFilter (SignatureFilter);
		InitExpressionTypes ();
	}

	static public void Reset ()
	{
		builder_to_declspace = new PtrHashtable ();
		builder_to_member_cache = new PtrHashtable ();
		builder_to_method = new PtrHashtable ();
		method_params = new PtrHashtable ();
		method_overrides = new PtrHashtable ();
		indexer_arguments = new PtrHashtable ();
		builder_to_ifaces = new PtrHashtable ();
		
		fieldbuilders_to_fields = new Hashtable ();
		fields = new Hashtable ();
		type_hash = new DoubleHash ();

		assembly_internals_vis_attrs = new PtrHashtable ();
		
		InitGenerics ();

		// to uncover regressions
		cons_param_array_attribute = null;
	}

	public static void AddUserType (DeclSpace ds)
	{
		builder_to_declspace.Add (ds.TypeBuilder, ds);
	}

	//
	// This entry point is used by types that we define under the covers
	// 
	public static void RegisterBuilder (Type tb, Type [] ifaces)
	{
		if (ifaces != null)
			builder_to_ifaces [tb] = ifaces;
	}	

	public static void AddMethod (MethodBase builder, IMethodData method)
	{
		builder_to_method.Add (builder, method);
		method_params.Add (builder, method.ParameterInfo);
	}

	public static IMethodData GetMethod (MethodBase builder)
	{
		return (IMethodData) builder_to_method [builder];
	}

	/// <summary>
	///   Returns the DeclSpace whose Type is `t' or null if there is no
	///   DeclSpace for `t' (ie, the Type comes from a library)
	/// </summary>
	public static DeclSpace LookupDeclSpace (Type t)
	{
		return builder_to_declspace [t] as DeclSpace;
	}

	/// <summary>
	///   Returns the TypeContainer whose Type is `t' or null if there is no
	///   TypeContainer for `t' (ie, the Type comes from a library)
	/// </summary>
	public static TypeContainer LookupTypeContainer (Type t)
	{
		return builder_to_declspace [t] as TypeContainer;
	}

	public static MemberCache LookupMemberCache (Type t)
	{
		if (t is TypeBuilder) {
			IMemberContainer container = builder_to_declspace [t] as IMemberContainer;
			if (container != null)
				return container.MemberCache;
		}

		if (t is GenericTypeParameterBuilder) {
			IMemberContainer container = builder_to_type_param [t] as IMemberContainer;

			if (container != null)
				return container.MemberCache;
		}

		return TypeHandle.GetMemberCache (t);
	}

	public static MemberCache LookupBaseInterfacesCache (Type t)
	{
		Type [] ifaces = t.GetInterfaces ();

		if (ifaces != null && ifaces.Length == 1)
			return LookupMemberCache (ifaces [0]);

		// TODO: the builder_to_member_cache should be indexed by 'ifaces', not 't'
		MemberCache cache = builder_to_member_cache [t] as MemberCache;
		if (cache != null)
			return cache;

		cache = new MemberCache (ifaces);
		builder_to_member_cache.Add (t, cache);
		return cache;
	}

	public static TypeContainer LookupInterface (Type t)
	{
		TypeContainer tc = (TypeContainer) builder_to_declspace [t];
		if ((tc == null) || (tc.Kind != Kind.Interface))
			return null;

		return tc;
	}

	public static Delegate LookupDelegate (Type t)
	{
		return builder_to_declspace [t] as Delegate;
	}

	public static Class LookupClass (Type t)
	{
		return (Class) builder_to_declspace [t];
	}

	//
	// We use this hash for multiple kinds of constructed types:
	//
	//    (T, "&")	Given T, get T &
	//    (T, "*")	Given T, get T *
	//    (T, "[]")	Given T and a array dimension, get T []
	//    (T, X)	Given a type T and a simple name X, get the type T+X
	//
	// Accessibility tests, if necessary, should be done by the user
	//
	static DoubleHash type_hash = new DoubleHash ();

	//
	// Gets the reference to T version of the Type (T&)
	//
	public static Type GetReferenceType (Type t)
	{
		return t.MakeByRefType ();
	}

	//
	// Gets the pointer to T version of the Type  (T*)
	//
	public static Type GetPointerType (Type t)
	{
		return GetConstructedType (t, "*");
	}

	public static Type GetConstructedType (Type t, string dim)
	{
		object ret = null;
		if (type_hash.Lookup (t, dim, out ret))
			return (Type) ret;

		ret = t.Module.GetType (t.ToString () + dim);
		if (ret != null) {
			type_hash.Insert (t, dim, ret);
			return (Type) ret;
		}

		if (dim == "&") {
			ret = GetReferenceType (t);
			type_hash.Insert (t, dim, ret);
			return (Type) ret;
		}

		if (t.IsGenericParameter || t.IsGenericInstance) {
		int pos = 0;
		Type result = t;
		while ((pos < dim.Length) && (dim [pos] == '[')) {
			pos++;

			if (dim [pos] == ']') {
				result = result.MakeArrayType ();
				pos++;

				if (pos < dim.Length)
					continue;

				type_hash.Insert (t, dim, result);
				return result;
			}

			int rank = 0;
			while (dim [pos] == ',') {
				pos++; rank++;
			}

			if ((dim [pos] != ']') || (pos != dim.Length-1))
				break;

			result = result.MakeArrayType (rank + 1);
			type_hash.Insert (t, dim, result);
			return result;
		}
		}

		type_hash.Insert (t, dim, null);
		return null;
	}

	public static Type GetNestedType (Type t, string name)
	{
		object ret = null;
		if (!type_hash.Lookup (t, name, out ret)) {
			ret = t.GetNestedType (name,
			       BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			type_hash.Insert (t, name, ret);
		}
		return (Type) ret;
	}

	/// <summary>
	/// Fills static table with exported types from all referenced assemblies.
	/// This information is required for CLS Compliance tests.
	/// </summary>
	public static void LoadAllImportedTypes ()
	{
		AllClsTopLevelTypes = new Hashtable (1500);
		foreach (Assembly a in RootNamespace.Global.Assemblies) {
			foreach (Type t in a.GetExportedTypes ()) {
				AllClsTopLevelTypes [t.FullName.ToLower (System.Globalization.CultureInfo.InvariantCulture)] = null;
			}
		}
	}

	public static bool NamespaceClash (string name, Location loc)
	{
		if (! RootNamespace.Global.IsNamespace (name))
			return false;

		Report.Error (519, loc, String.Format ("`{0}' clashes with a predefined namespace", name));
		return true;
	}

	/// <summary>
	///   Returns the C# name of a type if possible, or the full type name otherwise
	/// </summary>
	static public string CSharpName (Type t)
	{
		string name = GetFullName (t);

		return Regex.Replace (name, 
			@"^System\." +
			@"(Int32|UInt32|Int16|UInt16|Int64|UInt64|" +
			@"Single|Double|Char|Decimal|Byte|SByte|Object|" +
			@"Boolean|String|Void|Null)" +
			@"(\W+|\b)", 
			new MatchEvaluator (CSharpNameMatch)).Replace ('+', '.');
	}	

	static public string CSharpName (Type[] types)
	{
		StringBuilder sb = new StringBuilder ();
		foreach (Type t in types) {
			sb.Append (CSharpName (t));
			sb.Append (',');
		}
		sb.Remove (sb.Length - 1, 1);
		return sb.ToString ();
	}
	
	static String CSharpNameMatch (Match match) 
	{
		string s = match.Groups [1].Captures [0].Value;
		return s.ToLower ().
		Replace ("int32", "int").
		Replace ("uint32", "uint").
		Replace ("int16", "short").
		Replace ("uint16", "ushort").
		Replace ("int64", "long").
		Replace ("uint64", "ulong").
		Replace ("single", "float").
		Replace ("boolean", "bool")
		+ match.Groups [2].Captures [0].Value;
	}

        /// <summary>
	///  Returns the signature of the method with full namespace classification
	/// </summary>
	static public string GetFullNameSignature (MemberInfo mi)
	{
		PropertyInfo pi = mi as PropertyInfo;
		if (pi != null) {
			MethodBase pmi = pi.GetGetMethod ();
			if (pmi == null)
				pmi = pi.GetSetMethod ();
			if (GetParameterData (pmi).Count > 0)
				mi = pmi;
		}
		return (mi is MethodBase)
			? CSharpSignature (mi as MethodBase) 
			: CSharpName (mi.DeclaringType) + '.' + mi.Name;
	}

	private static int GetFullName (Type t, StringBuilder sb)
	{
		int pos = 0;

		if (!t.IsGenericType) {
			sb.Append (t.FullName);
			return 0;
		}

		if (t.DeclaringType != null) {
			pos = GetFullName (t.DeclaringType, sb);
			sb.Append ('.');
			sb.Append (RemoveGenericArity (t.Name));
		} else {
			sb.Append (RemoveGenericArity (t.FullName));
		}

		Type[] this_args = GetTypeArguments (t);

		if (this_args.Length < pos)
			throw new InternalErrorException (
				"Enclosing class " + t.DeclaringType + " has more type arguments than " + t);
		if (this_args.Length == pos)
			return pos;

		sb.Append ('<');
		for (;;) {
			sb.Append (CSharpName (this_args [pos++]));
			if (pos == this_args.Length)
				break;
			sb.Append (',');
		}
		sb.Append ('>');
		return pos;
	}

	public static string GetFullName (Type t)
	{
		if (t.IsGenericParameter)
			return t.Name;
		if (!t.IsGenericType)
			return t.FullName;

		StringBuilder sb = new StringBuilder ();
		int pos = GetFullName (t, sb);
		if (pos <= 0)
			throw new InternalErrorException ("Generic Type " + t + " doesn't have type arguments");
		return sb.ToString ();
	}

	static string RemoveGenericArity (string from)
	{
		int i = from.IndexOf ('`');
		if (i > 0)
			return from.Substring (0, i);
		return from;
	}

	/// <summary>
	/// When we need to report accessors as well
	/// </summary>
	static public string CSharpSignature (MethodBase mb)
	{
		return CSharpSignature (mb, false);
	}

	/// <summary>
	///   Returns the signature of the method
	/// </summary>
	static public string CSharpSignature (MethodBase mb, bool show_accessor)
	{
		StringBuilder sig = new StringBuilder (CSharpName (mb.DeclaringType));
		sig.Append ('.');

		ParameterData iparams = GetParameterData (mb);
		string parameters = iparams.GetSignatureForError ();
		string accessor = "";

		// Is property
		if (mb.IsSpecialName) {
			Operator.OpType ot = Operator.GetOperatorType (mb.Name);
			if (ot != Operator.OpType.TOP) {
				sig.Append ("operator ");
				sig.Append (Operator.GetName (ot));
				sig.Append (parameters);
				return sig.ToString ();
                        }

			if (mb.Name.StartsWith ("get_") || mb.Name.StartsWith ("set_")) {
				accessor = mb.Name.Substring (0, 3);
			}
                }

		// Is indexer
		if (mb.IsSpecialName && !mb.IsConstructor) {
			if (iparams.Count > (mb.Name.StartsWith ("get_") ? 0 : 1)) {
				sig.Append ("this[");
				if (show_accessor) {
					sig.Append (parameters.Substring (1, parameters.Length - 2));
				}
				else {
					int before_ret_val = parameters.LastIndexOf (',');
					if (before_ret_val < 0)
						sig.Append (parameters.Substring (1, parameters.Length - 2));
					else
						sig.Append (parameters.Substring (1, before_ret_val - 1));
				}
				sig.Append (']');
			} else {
				sig.Append (mb.Name.Substring (4));
			}
		} else {
			if (mb.Name == ".ctor")
				sig.Append (mb.DeclaringType.Name);
			else {
				sig.Append (mb.Name);

				if (mb.Mono_IsInflatedMethod || TypeManager.IsGenericMethod (mb)) {
					Type[] args = mb.GetGenericArguments ();
					sig.Append ('<');
					for (int i = 0; i < args.Length; i++) {
						if (i > 0)
							sig.Append (',');
						sig.Append (args [i].Name);
					}
					sig.Append ('>');
				}
			}

			sig.Append (parameters);
		}

		if (show_accessor && accessor.Length > 0) {
			sig.Append ('.');
			sig.Append (accessor);
		}

		return sig.ToString ();
        }

	public static string GetMethodName (MethodInfo m)
	{
		if (!IsGenericMethod (m))
			return m.Name;

		return MemberName.MakeName (m.Name, m.GetGenericArguments ().Length);
	}

	static public string CSharpSignature (EventInfo ei)
	{
		return CSharpName (ei.DeclaringType) + '.' + ei.Name;
	}

	/// <summary>
	///   Looks up a type, and aborts if it is not found.  This is used
	///   by types required by the compiler
	/// </summary>
	static Type CoreLookupType (string ns_name, string name)
	{
		Namespace ns = RootNamespace.Global.GetNamespace (ns_name, true);
		FullNamedExpression fne = ns.Lookup (RootContext.Tree.Types, name, Location.Null);
		Type t = fne == null ? null : fne.Type;
		if (t == null)
			Report.Error (518, "The predefined type `" + name + "' is not defined or imported");
		return t;
	}

	/// <summary>
	///   Returns the MethodInfo for a method named `name' defined
	///   in type `t' which takes arguments of types `args'
	/// </summary>
	static MethodInfo GetMethod (Type t, string name, Type [] args, bool is_private, bool report_errors)
	{
		MemberList list;
		Signature sig;
		BindingFlags flags = instance_and_static | BindingFlags.Public;

		sig.name = name;
		sig.args = args;
		
		if (is_private)
			flags |= BindingFlags.NonPublic;

		list = FindMembers (t, MemberTypes.Method, flags, signature_filter, sig);
		if (list.Count == 0) {
			if (report_errors)
				Report.Error (-19, "Can not find the core function `" + name + "'");
			return null;
		}

		MethodInfo mi = list [0] as MethodInfo;
		if (mi == null) {
			if (report_errors)
				Report.Error (-19, "Can not find the core function `" + name + "'");
			return null;
		}

		return mi;
	}

	static MethodInfo GetMethod (Type t, string name, Type [] args, bool report_errors)
	{
		return GetMethod (t, name, args, false, report_errors);
	}

	static MethodInfo GetMethod (Type t, string name, Type [] args)
	{
		return GetMethod (t, name, args, true);
	}

	/// <summary>
	///   Returns the PropertyInfo for a property named `name' defined
	///   in type `t'
	/// </summary>
	public static PropertyInfo GetProperty (Type t, string name)
	{
		MemberList list = FindMembers (t, MemberTypes.Property, BindingFlags.Public |
					       BindingFlags.Instance, Type.FilterName, name);
		if (list.Count == 0) {
			Report.Error (-19, "Can not find the core property `" + name + "'");
			return null;
		}

		PropertyInfo pi = list [0] as PropertyInfo;
		if (pi == null) {
			Report.Error (-19, "Can not find the core function `" + name + "'");
			return null;
		}

		return pi;
	}

	/// <summary>
	///    Returns the ConstructorInfo for "args"
	/// </summary>
	public static ConstructorInfo GetConstructor (Type t, Type [] args)
	{
		MemberList list;
		Signature sig;

		sig.name = ".ctor";
		sig.args = args;

		if (t == null)
			throw new InternalErrorException ("Core types haven't been initialized yet?");
		
		list = FindMembers (t, MemberTypes.Constructor,
				    instance_and_static | BindingFlags.Public | BindingFlags.DeclaredOnly,
				    signature_filter, sig);
		if (list.Count == 0){
			Report.Error (-19, "Can not find the core constructor for type `" + t.Name + "'");
			return null;
		}

		ConstructorInfo ci = list [0] as ConstructorInfo;
		if (ci == null){
			Report.Error (-19, "Can not find the core constructor for type `" + t.Name + "'");
			return null;
		}

		return ci;
	}

	public static void InitEnumUnderlyingTypes ()
	{

		int32_type    = CoreLookupType ("System", "Int32");
		int64_type    = CoreLookupType ("System", "Int64");
		uint32_type   = CoreLookupType ("System", "UInt32"); 
		uint64_type   = CoreLookupType ("System", "UInt64"); 
		byte_type     = CoreLookupType ("System", "Byte");
		sbyte_type    = CoreLookupType ("System", "SByte");
		short_type    = CoreLookupType ("System", "Int16");
		ushort_type   = CoreLookupType ("System", "UInt16");
	}
	
	/// <remarks>
	///   The types have to be initialized after the initial
	///   population of the type has happened (for example, to
	///   bootstrap the corlib.dll
	/// </remarks>
	public static void InitCoreTypes ()
	{
		object_type   = CoreLookupType ("System", "Object");
		value_type    = CoreLookupType ("System", "ValueType");

		InitEnumUnderlyingTypes ();

		char_type     = CoreLookupType ("System", "Char");
		string_type   = CoreLookupType ("System", "String");
		float_type    = CoreLookupType ("System", "Single");
		double_type   = CoreLookupType ("System", "Double");
		char_ptr_type = GetPointerType (char_type);
		decimal_type  = CoreLookupType ("System", "Decimal");
		bool_type     = CoreLookupType ("System", "Boolean");
		enum_type     = CoreLookupType ("System", "Enum");

		multicast_delegate_type = CoreLookupType ("System", "MulticastDelegate");
		delegate_type           = CoreLookupType ("System", "Delegate");

		array_type    = CoreLookupType ("System", "Array");
		void_type     = CoreLookupType ("System", "Void");
		type_type     = CoreLookupType ("System", "Type");

		runtime_field_handle_type = CoreLookupType ("System", "RuntimeFieldHandle");
		runtime_argument_handle_type = CoreLookupType ("System", "RuntimeArgumentHandle");
		runtime_helpers_type = CoreLookupType ("System.Runtime.CompilerServices", "RuntimeHelpers");
		default_member_type  = CoreLookupType ("System.Reflection", "DefaultMemberAttribute");
		runtime_handle_type  = CoreLookupType ("System", "RuntimeTypeHandle");
		asynccallback_type   = CoreLookupType ("System", "AsyncCallback");
		iasyncresult_type    = CoreLookupType ("System", "IAsyncResult");
		ienumerator_type     = CoreLookupType ("System.Collections", "IEnumerator");
		ienumerable_type     = CoreLookupType ("System.Collections", "IEnumerable");
		idisposable_type     = CoreLookupType ("System", "IDisposable");
		icloneable_type      = CoreLookupType ("System", "ICloneable");
		iconvertible_type    = CoreLookupType ("System", "IConvertible");
		monitor_type         = CoreLookupType ("System.Threading", "Monitor");
		intptr_type          = CoreLookupType ("System", "IntPtr");

		attribute_type       = CoreLookupType ("System", "Attribute");
		attribute_usage_type = CoreLookupType ("System", "AttributeUsageAttribute");
		dllimport_type       = CoreLookupType ("System.Runtime.InteropServices", "DllImportAttribute");
		methodimpl_attr_type = CoreLookupType ("System.Runtime.CompilerServices", "MethodImplAttribute");
		marshal_as_attr_type = CoreLookupType ("System.Runtime.InteropServices", "MarshalAsAttribute");
		param_array_type     = CoreLookupType ("System", "ParamArrayAttribute");
		in_attribute_type    = CoreLookupType ("System.Runtime.InteropServices", "InAttribute");
		out_attribute_type   = CoreLookupType ("System.Runtime.InteropServices", "OutAttribute");
		typed_reference_type = CoreLookupType ("System", "TypedReference");
		arg_iterator_type    = CoreLookupType ("System", "ArgIterator");
		mbr_type             = CoreLookupType ("System", "MarshalByRefObject");
		decimal_constant_attribute_type = CoreLookupType ("System.Runtime.CompilerServices", "DecimalConstantAttribute");

		unverifiable_code_type= CoreLookupType ("System.Security", "UnverifiableCodeAttribute");

		void_ptr_type         = GetPointerType (void_type);

		indexer_name_type     = CoreLookupType ("System.Runtime.CompilerServices", "IndexerNameAttribute");

		exception_type        = CoreLookupType ("System", "Exception");
		invalid_operation_exception_type = CoreLookupType ("System", "InvalidOperationException");
		not_supported_exception_type = CoreLookupType ("System", "NotSupportedException");

		//
		// Attribute types
		//
		obsolete_attribute_type = CoreLookupType ("System", "ObsoleteAttribute");
		conditional_attribute_type = CoreLookupType ("System.Diagnostics", "ConditionalAttribute");
		cls_compliant_attribute_type = CoreLookupType ("System", "CLSCompliantAttribute");
		struct_layout_attribute_type = CoreLookupType ("System.Runtime.InteropServices", "StructLayoutAttribute");
		field_offset_attribute_type = CoreLookupType ("System.Runtime.InteropServices", "FieldOffsetAttribute");
		security_attr_type = CoreLookupType ("System.Security.Permissions", "SecurityAttribute");
		required_attr_type = CoreLookupType ("System.Runtime.CompilerServices", "RequiredAttributeAttribute");
		guid_attr_type = CoreLookupType ("System.Runtime.InteropServices", "GuidAttribute");
		assembly_culture_attribute_type = CoreLookupType ("System.Reflection", "AssemblyCultureAttribute");
		comimport_attr_type = CoreLookupType ("System.Runtime.InteropServices", "ComImportAttribute");
		coclass_attr_type = CoreLookupType ("System.Runtime.InteropServices", "CoClassAttribute");

		InitGenericCoreTypes ();

		//
		// .NET 2.0
		//
		compiler_generated_attr_type = CoreLookupType ("System.Runtime.CompilerServices", "CompilerGeneratedAttribute");
		fixed_buffer_attr_type = CoreLookupType ("System.Runtime.CompilerServices", "FixedBufferAttribute");
		default_charset_type = CoreLookupType ("System.Runtime.InteropServices", "DefaultCharSetAttribute");
		internals_visible_attr_type = CoreLookupType ("System.Runtime.CompilerServices", "InternalsVisibleToAttribute");
		runtime_compatibility_attr_type = CoreLookupType ("System.Runtime.CompilerServices", "RuntimeCompatibilityAttribute");
		//
		// When compiling corlib, store the "real" types here.
		//
		if (!RootContext.StdLib) {
			system_int32_type = typeof (System.Int32);
			system_array_type = typeof (System.Array);
			system_type_type = typeof (System.Type);
			system_assemblybuilder_type = typeof (System.Reflection.Emit.AssemblyBuilder);

			system_int_array_get_length = GetMethod (
				system_array_type, "get_Length", Type.EmptyTypes);
			system_int_array_get_rank = GetMethod (
				system_array_type, "get_Rank", Type.EmptyTypes);
			system_object_array_clone = GetMethod (
				system_array_type, "Clone", Type.EmptyTypes);

			Type [] system_int_arg = { system_int32_type };
			system_int_array_get_length_int = GetMethod (
				system_array_type, "GetLength", system_int_arg);
			system_int_array_get_upper_bound_int = GetMethod (
				system_array_type, "GetUpperBound", system_int_arg);
			system_int_array_get_lower_bound_int = GetMethod (
				system_array_type, "GetLowerBound", system_int_arg);

			Type [] system_array_int_arg = { system_array_type, system_int32_type };
			system_void_array_copyto_array_int = GetMethod (
				system_array_type, "CopyTo", system_array_int_arg);

			Type [] system_3_type_arg = {
				system_type_type, system_type_type, system_type_type };
			Type [] system_4_type_arg = {
				system_type_type, system_type_type, system_type_type, system_type_type };

			MethodInfo set_corlib_type_builders = GetMethod (
				system_assemblybuilder_type, "SetCorlibTypeBuilders",
				system_4_type_arg, true, false);

			if (set_corlib_type_builders != null) {
				object[] args = new object [4];
				args [0] = object_type;
				args [1] = value_type;
				args [2] = enum_type;
				args [3] = void_type;
				
				set_corlib_type_builders.Invoke (CodeGen.Assembly.Builder, args);
			} else {
				// Compatibility for an older version of the class libs.
				set_corlib_type_builders = GetMethod (
					system_assemblybuilder_type, "SetCorlibTypeBuilders",
					system_3_type_arg, true, true);

				if (set_corlib_type_builders == null) {
					Report.Error (-26, "Corlib compilation is not supported in Microsoft.NET due to bugs in it");
					return;
				}

				object[] args = new object [3];
				args [0] = object_type;
				args [1] = value_type;
				args [2] = enum_type;
				
				set_corlib_type_builders.Invoke (CodeGen.Assembly.Builder, args);
			}
		}

		system_object_expr.Type = object_type;
		system_string_expr.Type = string_type;
		system_boolean_expr.Type = bool_type;
		system_decimal_expr.Type = decimal_type;
		system_single_expr.Type = float_type;
		system_double_expr.Type = double_type;
		system_sbyte_expr.Type = sbyte_type;
		system_byte_expr.Type = byte_type;
		system_int16_expr.Type = short_type;
		system_uint16_expr.Type = ushort_type;
		system_int32_expr.Type = int32_type;
		system_uint32_expr.Type = uint32_type;
		system_int64_expr.Type = int64_type;
		system_uint64_expr.Type = uint64_type;
		system_char_expr.Type = char_type;
		system_void_expr.Type = void_type;
		system_asynccallback_expr.Type = asynccallback_type;
		system_iasyncresult_expr.Type = iasyncresult_type;
		system_valuetype_expr.Type = value_type;

		//
		// These are only used for compare purposes
		//
		anonymous_method_type = typeof (AnonymousMethod);
		null_type = typeof (NullType);
	}

	//
	// The helper methods that are used by the compiler
	//
	public static void InitCodeHelpers ()
	{
		//
		// Now load the default methods that we use.
		//
		Type [] string_string = { string_type, string_type };
		string_concat_string_string = GetMethod (
			string_type, "Concat", string_string);
		Type [] string_string_string = { string_type, string_type, string_type };
		string_concat_string_string_string = GetMethod (
			string_type, "Concat", string_string_string);
		Type [] string_string_string_string = { string_type, string_type, string_type, string_type };
		string_concat_string_string_string_string = GetMethod (
			string_type, "Concat", string_string_string_string);
		Type[] params_string = { GetConstructedType (string_type, "[]") };
		string_concat_string_dot_dot_dot = GetMethod (
			string_type, "Concat", params_string);

		Type [] object_object = { object_type, object_type };
		string_concat_object_object = GetMethod (
			string_type, "Concat", object_object);
		Type [] object_object_object = { object_type, object_type, object_type };
		string_concat_object_object_object = GetMethod (
			string_type, "Concat", object_object_object);
		Type[] params_object = { GetConstructedType (object_type, "[]") };
		string_concat_object_dot_dot_dot = GetMethod (
			string_type, "Concat", params_object);

		Type [] string_ = { string_type };
		string_isinterneted_string = GetMethod (
			string_type, "IsInterned", string_);
		
		Type [] runtime_type_handle = { runtime_handle_type };
		system_type_get_type_from_handle = GetMethod (
			type_type, "GetTypeFromHandle", runtime_type_handle);

		Type [] delegate_delegate = { delegate_type, delegate_type };
		delegate_combine_delegate_delegate = GetMethod (
				delegate_type, "Combine", delegate_delegate);

		delegate_remove_delegate_delegate = GetMethod (
				delegate_type, "Remove", delegate_delegate);

		//
		// Void arguments
		//
		ienumerator_getcurrent = GetProperty (
			ienumerator_type, "Current");
		bool_movenext_void = GetMethod (
			ienumerator_type, "MoveNext", Type.EmptyTypes);
		void_reset_void = GetMethod (
			ienumerator_type, "Reset", Type.EmptyTypes);
		void_dispose_void = GetMethod (
			idisposable_type, "Dispose", Type.EmptyTypes);
		int_get_offset_to_string_data = GetMethod (
			runtime_helpers_type, "get_OffsetToStringData", Type.EmptyTypes);
		int_array_get_length = GetMethod (
			array_type, "get_Length", Type.EmptyTypes);
		int_array_get_rank = GetMethod (
			array_type, "get_Rank", Type.EmptyTypes);
		ienumerable_getenumerator_void = GetMethod (
			ienumerable_type, "GetEnumerator", Type.EmptyTypes);
		
		//
		// Int32 arguments
		//
		Type [] int_arg = { int32_type };
		int_array_get_length_int = GetMethod (
			array_type, "GetLength", int_arg);
		int_array_get_upper_bound_int = GetMethod (
			array_type, "GetUpperBound", int_arg);
		int_array_get_lower_bound_int = GetMethod (
			array_type, "GetLowerBound", int_arg);

		//
		// System.Array methods
		//
		object_array_clone = GetMethod (
			array_type, "Clone", Type.EmptyTypes);
		Type [] array_int_arg = { array_type, int32_type };
		void_array_copyto_array_int = GetMethod (
			array_type, "CopyTo", array_int_arg);
		
		//
		// object arguments
		//
		Type [] object_arg = { object_type };
		void_monitor_enter_object = GetMethod (
			monitor_type, "Enter", object_arg);
		void_monitor_exit_object = GetMethod (
			monitor_type, "Exit", object_arg);

		Type [] array_field_handle_arg = { array_type, runtime_field_handle_type };
		
		void_initializearray_array_fieldhandle = GetMethod (
			runtime_helpers_type, "InitializeArray", array_field_handle_arg);

		//
		// Array functions
		//
		int_getlength_int = GetMethod (
			array_type, "GetLength", int_arg);

		//
		// Decimal constructors
		//
		Type [] dec_arg = { int32_type, int32_type, int32_type, bool_type, byte_type };
		void_decimal_ctor_five_args = GetConstructor (
			decimal_type, dec_arg);
		
		void_decimal_ctor_int_arg = GetConstructor (decimal_type, int_arg);

		//
		// Attributes
		//
		unverifiable_code_ctor = GetConstructor (unverifiable_code_type, Type.EmptyTypes);
		default_member_ctor = GetConstructor (default_member_type, string_);

		Type[] short_arg = { short_type };
		struct_layout_attribute_ctor = GetConstructor (struct_layout_attribute_type, short_arg);

		decimal_constant_attribute_ctor = GetConstructor (decimal_constant_attribute_type, new Type []
			{ byte_type, byte_type, uint32_type, uint32_type, uint32_type } );

		field_offset_attribute_ctor = GetConstructor (field_offset_attribute_type, new Type []
			{ int32_type });

		//
		// .NET 2.0 types
		//
		compiler_generated_attr = new CustomAttributeBuilder (
			GetConstructor (compiler_generated_attr_type, Type.EmptyTypes), new object[0]);

		Type[] type_int_arg = { type_type, int32_type };
		fixed_buffer_attr_ctor = GetConstructor (fixed_buffer_attr_type, type_int_arg);

		// Object
		object_ctor = GetConstructor (object_type, Type.EmptyTypes);

		InitGenericCodeHelpers ();
	}

	static public ConstructorInfo ConsParamArrayAttribute {
		get {
			if (cons_param_array_attribute == null)
				cons_param_array_attribute = GetConstructor (param_array_type, Type.EmptyTypes);
			return cons_param_array_attribute;
		}
	}

	const BindingFlags instance_and_static = BindingFlags.Static | BindingFlags.Instance;

	/// <remarks>
	///   This is the "old", non-cache based FindMembers() function.  We cannot use
	///   the cache here because there is no member name argument.
	/// </remarks>
	public static MemberList FindMembers (Type t, MemberTypes mt, BindingFlags bf,
					      MemberFilter filter, object criteria)
	{
		DeclSpace decl = (DeclSpace) builder_to_declspace [t];

		//
		// `builder_to_declspace' contains all dynamic types.
		//
		if (decl != null) {
			MemberList list;
			Timer.StartTimer (TimerType.FindMembers);
			list = decl.FindMembers (mt, bf, filter, criteria);
			Timer.StopTimer (TimerType.FindMembers);
			return list;
		}

		//
		// We have to take care of arrays specially, because GetType on
		// a TypeBuilder array will return a Type, not a TypeBuilder,
		// and we can not call FindMembers on this type.
		//
		if (t.IsSubclassOf (TypeManager.array_type))
			return new MemberList (TypeManager.array_type.FindMembers (mt, bf, filter, criteria));

		if (t is GenericTypeParameterBuilder) {
			TypeParameter tparam = (TypeParameter) builder_to_type_param [t];

			Timer.StartTimer (TimerType.FindMembers);
			MemberList list = tparam.FindMembers (
				mt, bf | BindingFlags.DeclaredOnly, filter, criteria);
			Timer.StopTimer (TimerType.FindMembers);
			return list;
		}

		//
		// Since FindMembers will not lookup both static and instance
		// members, we emulate this behaviour here.
		//
		if ((bf & instance_and_static) == instance_and_static){
			MemberInfo [] i_members = t.FindMembers (
				mt, bf & ~BindingFlags.Static, filter, criteria);

			int i_len = i_members.Length;
			if (i_len == 1){
				MemberInfo one = i_members [0];

				//
				// If any of these are present, we are done!
				//
				if ((one is Type) || (one is EventInfo) || (one is FieldInfo))
					return new MemberList (i_members);
			}
				
			MemberInfo [] s_members = t.FindMembers (
				mt, bf & ~BindingFlags.Instance, filter, criteria);

			int s_len = s_members.Length;
			if (i_len > 0 || s_len > 0)
				return new MemberList (i_members, s_members);
			else {
				if (i_len > 0)
					return new MemberList (i_members);
				else
					return new MemberList (s_members);
			}
		}

		return new MemberList (t.FindMembers (mt, bf, filter, criteria));
	}


	/// <summary>
	///   This method is only called from within MemberLookup.  It tries to use the member
	///   cache if possible and falls back to the normal FindMembers if not.  The `used_cache'
	///   flag tells the caller whether we used the cache or not.  If we used the cache, then
	///   our return value will already contain all inherited members and the caller don't need
	///   to check base classes and interfaces anymore.
	/// </summary>
	private static MemberInfo [] MemberLookup_FindMembers (Type t, MemberTypes mt, BindingFlags bf,
							       string name, out bool used_cache)
	{
		MemberCache cache;

		//
		// If this is a dynamic type, it's always in the `builder_to_declspace' hash table
		// and we can ask the DeclSpace for the MemberCache.
		//
		if (t is TypeBuilder) {
			DeclSpace decl = (DeclSpace) builder_to_declspace [t];
			cache = decl.MemberCache;

			//
			// If this DeclSpace has a MemberCache, use it.
			//

			if (cache != null) {
				used_cache = true;
				return cache.FindMembers (
					mt, bf, name, FilterWithClosure_delegate, null);
			}

			// If there is no MemberCache, we need to use the "normal" FindMembers.
			// Note, this is a VERY uncommon route!

			MemberList list;
			Timer.StartTimer (TimerType.FindMembers);
			list = decl.FindMembers (mt, bf | BindingFlags.DeclaredOnly,
						 FilterWithClosure_delegate, name);
			Timer.StopTimer (TimerType.FindMembers);
			used_cache = false;
			return (MemberInfo []) list;
		}

		//
		// We have to take care of arrays specially, because GetType on
		// a TypeBuilder array will return a Type, not a TypeBuilder,
		// and we can not call FindMembers on this type.
		//
		if (t == TypeManager.array_type || t.IsSubclassOf (TypeManager.array_type)) {
			used_cache = true;
			return TypeHandle.ArrayType.MemberCache.FindMembers (
				mt, bf, name, FilterWithClosure_delegate, null);
		}

		if (t is GenericTypeParameterBuilder) {
			TypeParameter tparam = (TypeParameter) builder_to_type_param [t];

			MemberList list;
			Timer.StartTimer (TimerType.FindMembers);
			list = tparam.FindMembers (mt, bf | BindingFlags.DeclaredOnly,
						   FilterWithClosure_delegate, name);
			Timer.StopTimer (TimerType.FindMembers);
			used_cache = false;
			return (MemberInfo []) list;
		}

		if (t.IsGenericInstance && (mt == MemberTypes.NestedType)) {
			//
			// This happens if we're resolving a class'es base class and interfaces
			// in TypeContainer.DefineType().  At this time, the types aren't
			// populated yet, so we can't use the cache.
			//
			MemberInfo[] info = t.FindMembers (mt, bf | BindingFlags.DeclaredOnly,
							   FilterWithClosure_delegate, name);
			used_cache = false;
			return info;
		}

		//
		// This call will always succeed.  There is exactly one TypeHandle instance per
		// type, TypeHandle.GetMemberCache() will, if necessary, create a new one, and return
		// the corresponding MemberCache.
		//
		cache = TypeHandle.GetMemberCache (t);

		used_cache = true;
		return cache.FindMembers (mt, bf, name, FilterWithClosure_delegate, null);
	}

	public static bool IsBuiltinType (Type t)
	{
		t = TypeToCoreType (t);
		if (t == object_type || t == string_type || t == int32_type || t == uint32_type ||
		    t == int64_type || t == uint64_type || t == float_type || t == double_type ||
		    t == char_type || t == short_type || t == decimal_type || t == bool_type ||
		    t == sbyte_type || t == byte_type || t == ushort_type || t == void_type)
			return true;
		else
			return false;
	}

	public static bool IsBuiltinType (TypeContainer tc)
	{
		return IsBuiltinType (tc.TypeBuilder);
	}

	//
	// This is like IsBuiltinType, but lacks decimal_type, we should also clean up
	// the pieces in the code where we use IsBuiltinType and special case decimal_type.
	// 
	public static bool IsPrimitiveType (Type t)
	{
		return (t == int32_type || t == uint32_type ||
		    t == int64_type || t == uint64_type || t == float_type || t == double_type ||
		    t == char_type || t == short_type || t == bool_type ||
		    t == sbyte_type || t == byte_type || t == ushort_type);
	}

	public static bool IsDelegateType (Type t)
	{
		t = DropGenericTypeArguments (t);
		if (t.IsSubclassOf (TypeManager.delegate_type))
			return true;
		else
			return false;
	}
	
	public static bool IsEnumType (Type t)
	{
		if (builder_to_declspace [t] is Enum)
			return true;

		return t.IsEnum;
	}

	public static bool IsBuiltinOrEnum (Type t)
	{
		if (IsBuiltinType (t))
			return true;
		
		if (IsEnumType (t))
			return true;

		return false;
	}

	public static bool IsNullType (Type t)
	{
		return t == null_type;
	}

	public static bool IsAttributeType (Type t)
	{
		return t == attribute_type && t.BaseType != null || IsSubclassOf (t, attribute_type);
	}
	
	static Stack unmanaged_enclosing_types = new Stack (4);

	//
	// Whether a type is unmanaged.  This is used by the unsafe code (25.2)
	//
	public static bool IsUnmanagedType (Type t)
	{
		// Avoid infloops in the case of: unsafe struct Foo { Foo *x; }
		if (unmanaged_enclosing_types.Contains (t))
			return true;

		// builtins that are not unmanaged types
		if (t == TypeManager.object_type || t == TypeManager.string_type)
			return false;

		if (IsBuiltinOrEnum (t))
			return true;

		// Someone did the work of checking if the ElementType of t is unmanaged.  Let's not repeat it.
		if (t.IsPointer)
			return true;

		// Arrays are disallowed, even if we mark them with [MarshalAs(UnmanagedType.ByValArray, ...)]
		if (t.IsArray)
			return false;

		if (!IsValueType (t))
			return false;

		for (Type p = t.DeclaringType; p != null; p = p.DeclaringType) {
			if (p.IsGenericTypeDefinition)
				return false;
		}

		unmanaged_enclosing_types.Push (t);

		bool retval = true;

		if (t is TypeBuilder) {
			TypeContainer tc = LookupTypeContainer (t);
			if (tc.Fields != null){
				foreach (FieldMember f in tc.Fields){
					// Avoid using f.FieldBuilder: f.Define () may not yet have been invoked.
					if ((f.ModFlags & Modifiers.STATIC) != 0)
						continue;
					if (f.MemberType == null)
						continue;
					if (!IsUnmanagedType (f.MemberType)){
						Report.SymbolRelatedToPreviousError (f.Location, CSharpName (t) + "." + f.Name);
						retval = false;
					}
				}
			}
		} else {
			FieldInfo [] fields = t.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			
			foreach (FieldInfo f in fields){
				if (!IsUnmanagedType (f.FieldType)){
					Report.SymbolRelatedToPreviousError (f);
					retval = false;
				}
			}
		}

		unmanaged_enclosing_types.Pop ();

		return retval;
	}
		
	public static bool IsValueType (Type t)
	{
		return t.IsGenericParameter || t.IsValueType;
	}
	
	public static bool IsInterfaceType (Type t)
	{
		TypeContainer tc = (TypeContainer) builder_to_declspace [t];
		if (tc == null)
			return false;

		return tc.Kind == Kind.Interface;
	}

	public static bool IsSubclassOf (Type type, Type base_type)
	{
		TypeParameter tparam = LookupTypeParameter (type);
		TypeParameter pparam = LookupTypeParameter (base_type);

		if ((tparam != null) && (pparam != null)) {
			if (tparam == pparam)
				return true;

			return tparam.IsSubclassOf (base_type);
		}

		do {
			if (IsEqual (type, base_type))
				return true;

			type = type.BaseType;
		} while (type != null);

		return false;
	}

	public static bool IsPrivateAccessible (Type type, Type parent)
	{
		if (type.Equals (parent))
			return true;
		return DropGenericTypeArguments (type) == DropGenericTypeArguments (parent);
	}

	public static bool IsFamilyAccessible (Type type, Type parent)
	{
		TypeParameter tparam = LookupTypeParameter (type);
		TypeParameter pparam = LookupTypeParameter (parent);

		if ((tparam != null) && (pparam != null)) {
			if (tparam == pparam)
				return true;

			return tparam.IsSubclassOf (parent);
		}

		do {
			if (IsInstantiationOfSameGenericType (type, parent))
				return true;

			type = type.BaseType;
		} while (type != null);

		return false;
	}

	//
	// Checks whether `type' is a subclass or nested child of `base_type'.
	//
	public static bool IsNestedFamilyAccessible (Type type, Type base_type)
	{
		do {
			if (IsFamilyAccessible (type, base_type))
				return true;

			// Handle nested types.
			type = type.DeclaringType;
		} while (type != null);

		return false;
	}

	//
	// Checks whether `type' is a nested child of `parent'.
	//
	public static bool IsNestedChildOf (Type type, Type parent)
	{
		if (IsEqual (type, parent))
			return false;

		type = type.DeclaringType;
		while (type != null) {
			if (IsEqual (type, parent))
				return true;

			type = type.DeclaringType;
		}

		return false;
	}

	//
	// Checks whether `extern_type' is friend of the output assembly
	//
	public static bool IsFriendAssembly (Assembly assembly)
	{
		if (assembly_internals_vis_attrs.Contains (assembly))
			return (bool)(assembly_internals_vis_attrs [assembly]);
		
		object [] attrs = assembly.GetCustomAttributes (internals_visible_attr_type, false);
		if (attrs.Length == 0) {
			assembly_internals_vis_attrs.Add (assembly, false);
			return false;
		}

		AssemblyName this_name = CodeGen.Assembly.Name;
		byte [] this_token = this_name.GetPublicKeyToken ();
		bool is_friend = false;
		foreach (InternalsVisibleToAttribute attr in attrs) {
			if (attr.AssemblyName == null || attr.AssemblyName.Length == 0)
				continue;
			
			AssemblyName aname = null;
			try {
				aname = new AssemblyName (attr.AssemblyName);
			} catch (FileLoadException) {
			} catch (ArgumentException) {
			}

			if (aname == null || aname.Name != this_name.Name)
				continue;
			
			byte [] key_token = aname.GetPublicKeyToken ();
			if (key_token != null) {
				if (this_token == null) {
					// Same name, but key token is null
					Error_FriendAccessNameNotMatching (aname.FullName);
					break;
				}
				
				if (!CompareKeyTokens (this_token, key_token))
					continue;
			}

			is_friend = true;
			break;
		}

		assembly_internals_vis_attrs.Add (assembly, is_friend);
		return is_friend;
	}

	static bool CompareKeyTokens (byte [] token1, byte [] token2)
	{
		for (int i = 0; i < token1.Length; i++)
			if (token1 [i] != token2 [i])
				return false;

		return true;
	}

	static void Error_FriendAccessNameNotMatching (string other_name)
	{
		Report.Error (281, "Friend access was granted to `" + other_name + 
				"', but the output assembly is named `" + CodeGen.Assembly.Name.FullName +
				"'. Try adding a reference to `" + other_name + 
				"' or change the output assembly name to match it");
	}
	
        //
        // Do the right thing when returning the element type of an
        // array type based on whether we are compiling corlib or not
        //
        public static Type GetElementType (Type t)
        {
                if (RootContext.StdLib)
                        return t.GetElementType ();
                else
                        return TypeToCoreType (t.GetElementType ());
        }

	/// <summary>
	/// This method is not implemented by MS runtime for dynamic types
	/// </summary>
	public static bool HasElementType (Type t)
	{
		return t.IsArray || t.IsPointer || t.IsByRef;
	}
	
	/// <summary>
	///   Gigantic work around for missing features in System.Reflection.Emit follows.
	/// </summary>
	///
	/// <remarks>
	///   Since System.Reflection.Emit can not return MethodBase.GetParameters
	///   for anything which is dynamic, and we need this in a number of places,
	///   we register this information here, and use it afterwards.
	/// </remarks>
	static public void RegisterMethod (MethodBase mb, Parameters ip)
	{
		method_params.Add (mb, ip);
	}
	
	static public ParameterData GetParameterData (MethodBase mb)
	{
		ParameterData pd = (ParameterData)method_params [mb];
		if (pd == null) {
			if (mb is MethodBuilder || mb is ConstructorBuilder)
				throw new InternalErrorException ("Argument for Method not registered" + mb);

			pd = new ReflectionParameters (mb);
			method_params.Add (mb, pd);
		}
		return pd;
	}

	static public void RegisterOverride (MethodBase override_method, MethodBase base_method)
	{
		if (!method_overrides.Contains (override_method))
			method_overrides [override_method] = base_method;
		if (method_overrides [override_method] != base_method)
			throw new InternalErrorException ("Override mismatch: " + override_method);
	}

	static public bool IsOverride (MethodBase m)
	{
		return m.IsVirtual &&
			(m.Attributes & MethodAttributes.NewSlot) == 0 &&
			(m is MethodBuilder || method_overrides.Contains (m));
	}

	static public MethodBase TryGetBaseDefinition (MethodBase m)
	{
		return (MethodBase) method_overrides [m];
	}

	/// <summary>
	///    Returns the argument types for an indexer based on its PropertyInfo
	///
	///    For dynamic indexers, we use the compiler provided types, for
	///    indexers from existing assemblies we load them from GetParameters,
	///    and insert them into the cache
	/// </summary>
	static public Type [] GetArgumentTypes (PropertyInfo indexer)
	{
		if (indexer_arguments.Contains (indexer))
			return (Type []) indexer_arguments [indexer];
		else if (indexer is PropertyBuilder)
			// If we're a PropertyBuilder and not in the
			// `indexer_arguments' hash, then we're a property and
			// not an indexer.
			return Type.EmptyTypes;
		else {
			ParameterInfo [] pi = indexer.GetIndexParameters ();
			// Property, not an indexer.
			if (pi == null)
				return Type.EmptyTypes;
			int c = pi.Length;
			Type [] types = new Type [c];
			
			for (int i = 0; i < c; i++)
				types [i] = pi [i].ParameterType;

			indexer_arguments.Add (indexer, types);
			return types;
		}
	}
	
	public static void RegisterConstant (FieldInfo fb, IConstant ic)
	{
		fields.Add (fb, ic);
	}

	public static IConstant GetConstant (FieldInfo fb)
	{
		if (fb == null)
			return null;

		return (IConstant)fields [fb];
	}

	static public bool RegisterFieldBase (FieldBuilder fb, FieldBase f)
	{
		if (fieldbuilders_to_fields.Contains (fb))
			return false;

		fieldbuilders_to_fields.Add (fb, f);
		return true;
	}

	//
	// The return value can be null;  This will be the case for
	// auxiliary FieldBuilders created by the compiler that have no
	// real field being declared on the source code
	//
	static public FieldBase GetField (FieldInfo fb)
	{
		if (fb.DeclaringType.IsGenericInstance)
			fb = fb.Mono_GetGenericFieldDefinition ();

		return (FieldBase) fieldbuilders_to_fields [fb];
	}
	
	static Hashtable events;

	static public void RegisterEvent (MyEventBuilder eb, MethodBase add, MethodBase remove)
	{
		if (events == null)
			events = new Hashtable ();

		if (!events.Contains (eb)) {
		events.Add (eb, new Pair (add, remove));
		}
	}

	static public MethodInfo GetAddMethod (EventInfo ei)
	{
		if (ei is MyEventBuilder) {
			Pair pair = (Pair) events [ei];

			return (MethodInfo) pair.First;
		}
		return ei.GetAddMethod (true);
	}

	static public MethodInfo GetRemoveMethod (EventInfo ei)
	{
		if (ei is MyEventBuilder) {
			Pair pair = (Pair) events [ei];

			return (MethodInfo) pair.Second;
		}
		return ei.GetRemoveMethod (true);
	}

	static Hashtable priv_fields_events;

	static public bool RegisterPrivateFieldOfEvent (EventInfo einfo, FieldBuilder builder)
	{
		if (priv_fields_events == null)
			priv_fields_events = new Hashtable ();

		if (priv_fields_events.Contains (einfo))
			return false;

		priv_fields_events.Add (einfo, builder);

		return true;
	}

	static public MemberInfo GetPrivateFieldOfEvent (EventInfo ei)
	{
		if (priv_fields_events == null)
			return null;
		else
			return (MemberInfo) priv_fields_events [ei];
	}
		
	static public bool RegisterIndexer (PropertyBuilder pb, MethodBase get,
                                            MethodBase set, Type[] args)
	{
		indexer_arguments.Add (pb, args);

		return true;
	}

	public static bool CheckStructCycles (TypeContainer tc, Hashtable seen)
	{
		Hashtable hash = new Hashtable ();
		return CheckStructCycles (tc, seen, hash);
	}

	public static bool CheckStructCycles (TypeContainer tc, Hashtable seen,
					      Hashtable hash)
	{
		if ((tc.Kind != Kind.Struct) || IsBuiltinType (tc))
			return true;

		//
		// `seen' contains all types we've already visited.
		//
		if (seen.Contains (tc))
			return true;
		seen.Add (tc, null);

		if (tc.Fields == null)
			return true;

		foreach (FieldMember field in tc.Fields) {
			if (field.FieldBuilder == null || field.FieldBuilder.IsStatic)
				continue;

			Type ftype = field.FieldBuilder.FieldType;
			TypeContainer ftc = LookupTypeContainer (ftype);
			if (ftc == null)
				continue;

			if (hash.Contains (ftc)) {
				Report.Error (523, tc.Location,
					      "Struct member `{0}.{1}' of type `{2}' " +
					      "causes a cycle in the struct layout",
					      tc.Name, field.Name, ftc.Name);
				return false;
			}

			//
			// `hash' contains all types in the current path.
			//
			hash.Add (tc, null);

			bool ok = CheckStructCycles (ftc, seen, hash);

			hash.Remove (tc);

			if (!ok)
				return false;

			if (!seen.Contains (ftc))
				seen.Add (ftc, null);
		}

		return true;
	}

	/// <summary>
	///   Given an array of interface types, expand and eliminate repeated ocurrences
	///   of an interface.  
	/// </summary>
	///
	/// <remarks>
	///   This expands in context like: IA; IB : IA; IC : IA, IB; the interface "IC" to
	///   be IA, IB, IC.
	/// </remarks>
	public static Type[] ExpandInterfaces (EmitContext ec, TypeExpr [] base_interfaces)
	{
		ArrayList new_ifaces = new ArrayList ();

		foreach (TypeExpr iface in base_interfaces){
			TypeExpr texpr = iface.ResolveAsTypeTerminal (ec);
			if (texpr == null)
				return null;

			Type itype = texpr.ResolveType (ec);

			if (new_ifaces.Contains (itype))
				continue;

			new_ifaces.Add (itype);
			
			Type [] implementing = itype.GetInterfaces ();

			foreach (Type imp in implementing){
				if (!new_ifaces.Contains (imp))
					new_ifaces.Add (imp);
			}
		}
		Type [] ret = new Type [new_ifaces.Count];
		new_ifaces.CopyTo (ret, 0);
		return ret;
	}

	public static Type[] ExpandInterfaces (Type [] base_interfaces)
	{
		ArrayList new_ifaces = new ArrayList ();

		foreach (Type itype in base_interfaces){
			if (new_ifaces.Contains (itype))
				continue;

			new_ifaces.Add (itype);
			
			Type [] implementing = itype.GetInterfaces ();

			foreach (Type imp in implementing){
				if (!new_ifaces.Contains (imp))
					new_ifaces.Add (imp);
			}
		}
		Type [] ret = new Type [new_ifaces.Count];
		new_ifaces.CopyTo (ret, 0);
		return ret;
	}
		
	static PtrHashtable iface_cache = new PtrHashtable ();
		
	/// <summary>
	///   This function returns the interfaces in the type `t'.  Works with
	///   both types and TypeBuilders.
	/// </summary>
	public static Type [] GetInterfaces (Type t)
	{
		Type [] cached = iface_cache [t] as Type [];
		if (cached != null)
			return cached;
		
		//
		// The reason for catching the Array case is that Reflection.Emit
		// will not return a TypeBuilder for Array types of TypeBuilder types,
		// but will still throw an exception if we try to call GetInterfaces
		// on the type.
		//
		// Since the array interfaces are always constant, we return those for
		// the System.Array
		//
		
		if (t.IsArray)
			t = TypeManager.array_type;
		
		if ((t is TypeBuilder) || t.IsGenericInstance) {
			Type [] base_ifaces;
			
			if (t.BaseType == null)
				base_ifaces = Type.EmptyTypes;
			else
				base_ifaces = GetInterfaces (t.BaseType);
			Type[] type_ifaces;
			if (t.IsGenericInstance)
				type_ifaces = t.GetInterfaces ();
			else
				type_ifaces = (Type []) builder_to_ifaces [t];
			if (type_ifaces == null)
				type_ifaces = Type.EmptyTypes;

			int base_count = base_ifaces.Length;
			Type [] result = new Type [base_count + type_ifaces.Length];
			base_ifaces.CopyTo (result, 0);
			type_ifaces.CopyTo (result, base_count);

			iface_cache [t] = result;
			return result;
		} else if (t is GenericTypeParameterBuilder){
			Type[] type_ifaces = (Type []) builder_to_ifaces [t];
			if (type_ifaces == null)
				type_ifaces = Type.EmptyTypes;

			iface_cache [t] = type_ifaces;
			return type_ifaces;
		} else {
			Type[] ifaces = t.GetInterfaces ();
			iface_cache [t] = ifaces;
			return ifaces;
		}
	}
	
	//
	// gets the interfaces that are declared explicitly on t
	//
	public static Type [] GetExplicitInterfaces (TypeBuilder t)
	{
		return (Type []) builder_to_ifaces [t];
	}
	
	/// <remarks>
	///  The following is used to check if a given type implements an interface.
	///  The cache helps us reduce the expense of hitting Type.GetInterfaces everytime.
	/// </remarks>
	public static bool ImplementsInterface (Type t, Type iface)
	{
		Type [] interfaces;

		//
		// FIXME OPTIMIZATION:
		// as soon as we hit a non-TypeBuiler in the interface
		// chain, we could return, as the `Type.GetInterfaces'
		// will return all the interfaces implement by the type
		// or its bases.
		//
		do {
			interfaces = GetInterfaces (t);

			if (interfaces != null){
				foreach (Type i in interfaces){
					if (i == iface)
						return true;
				}
			}
			
			t = t.BaseType;
		} while (t != null);
		
		return false;
	}

	static NumberFormatInfo nf_provider = CultureInfo.CurrentCulture.NumberFormat;

	// This is a custom version of Convert.ChangeType() which works
	// with the TypeBuilder defined types when compiling corlib.
	public static object ChangeType (object value, Type conversionType, out bool error)
	{
		IConvertible convert_value = value as IConvertible;
		
		if (convert_value == null){
			error = true;
			return null;
		}
		
		//
		// We must use Type.Equals() here since `conversionType' is
		// the TypeBuilder created version of a system type and not
		// the system type itself.  You cannot use Type.GetTypeCode()
		// on such a type - it'd always return TypeCode.Object.
		//
		error = false;
		try {
			if (conversionType.Equals (typeof (Boolean)))
				return (object)(convert_value.ToBoolean (nf_provider));
			else if (conversionType.Equals (typeof (Byte)))
				return (object)(convert_value.ToByte (nf_provider));
			else if (conversionType.Equals (typeof (Char)))
				return (object)(convert_value.ToChar (nf_provider));
			else if (conversionType.Equals (typeof (DateTime)))
				return (object)(convert_value.ToDateTime (nf_provider));
			else if (conversionType.Equals (TypeManager.decimal_type)) // typeof (Decimal)))
				return (object)(convert_value.ToDecimal (nf_provider));
			else if (conversionType.Equals (typeof (Double)))
				return (object)(convert_value.ToDouble (nf_provider));
			else if (conversionType.Equals (typeof (Int16)))
				return (object)(convert_value.ToInt16 (nf_provider));
			else if (conversionType.Equals (typeof (Int32)))
				return (object)(convert_value.ToInt32 (nf_provider));
			else if (conversionType.Equals (typeof (Int64)))
				return (object)(convert_value.ToInt64 (nf_provider));
			else if (conversionType.Equals (typeof (SByte)))
				return (object)(convert_value.ToSByte (nf_provider));
			else if (conversionType.Equals (typeof (Single)))
				return (object)(convert_value.ToSingle (nf_provider));
			else if (conversionType.Equals (typeof (String)))
				return (object)(convert_value.ToString (nf_provider));
			else if (conversionType.Equals (typeof (UInt16)))
				return (object)(convert_value.ToUInt16 (nf_provider));
			else if (conversionType.Equals (typeof (UInt32)))
				return (object)(convert_value.ToUInt32 (nf_provider));
			else if (conversionType.Equals (typeof (UInt64)))
				return (object)(convert_value.ToUInt64 (nf_provider));
			else if (conversionType.Equals (typeof (Object)))
				return (object)(value);
			else 
				error = true;
		} catch {
			error = true;
		}
		return null;
	}

	//
	// This is needed, because enumerations from assemblies
	// do not report their underlyingtype, but they report
	// themselves
	//
	public static Type EnumToUnderlying (Type t)
	{
		if (t == TypeManager.enum_type)
			return t;

		t = t.UnderlyingSystemType;
		if (!TypeManager.IsEnumType (t))
			return t;
	
		if (t is TypeBuilder) {
			// slow path needed to compile corlib
			if (t == TypeManager.bool_type ||
					t == TypeManager.byte_type ||
					t == TypeManager.sbyte_type ||
					t == TypeManager.char_type ||
					t == TypeManager.short_type ||
					t == TypeManager.ushort_type ||
					t == TypeManager.int32_type ||
					t == TypeManager.uint32_type ||
					t == TypeManager.int64_type ||
					t == TypeManager.uint64_type)
				return t;
		}
		TypeCode tc = Type.GetTypeCode (t);

		switch (tc){
		case TypeCode.Boolean:
			return TypeManager.bool_type;
		case TypeCode.Byte:
			return TypeManager.byte_type;
		case TypeCode.SByte:
			return TypeManager.sbyte_type;
		case TypeCode.Char:
			return TypeManager.char_type;
		case TypeCode.Int16:
			return TypeManager.short_type;
		case TypeCode.UInt16:
			return TypeManager.ushort_type;
		case TypeCode.Int32:
			return TypeManager.int32_type;
		case TypeCode.UInt32:
			return TypeManager.uint32_type;
		case TypeCode.Int64:
			return TypeManager.int64_type;
		case TypeCode.UInt64:
			return TypeManager.uint64_type;
		}
		throw new Exception ("Unhandled typecode in enum " + tc + " from " + t.AssemblyQualifiedName);
	}

	//
	// When compiling corlib and called with one of the core types, return
	// the corresponding typebuilder for that type.
	//
	public static Type TypeToCoreType (Type t)
	{
		if (RootContext.StdLib || (t is TypeBuilder))
			return t;

		TypeCode tc = Type.GetTypeCode (t);

		switch (tc){
		case TypeCode.Boolean:
			return TypeManager.bool_type;
		case TypeCode.Byte:
			return TypeManager.byte_type;
		case TypeCode.SByte:
			return TypeManager.sbyte_type;
		case TypeCode.Char:
			return TypeManager.char_type;
		case TypeCode.Int16:
			return TypeManager.short_type;
		case TypeCode.UInt16:
			return TypeManager.ushort_type;
		case TypeCode.Int32:
			return TypeManager.int32_type;
		case TypeCode.UInt32:
			return TypeManager.uint32_type;
		case TypeCode.Int64:
			return TypeManager.int64_type;
		case TypeCode.UInt64:
			return TypeManager.uint64_type;
		case TypeCode.Single:
			return TypeManager.float_type;
		case TypeCode.Double:
			return TypeManager.double_type;
		case TypeCode.String:
			return TypeManager.string_type;
		case TypeCode.Decimal:
			return TypeManager.decimal_type;
		default:
			if (t == typeof (void))
				return TypeManager.void_type;
			if (t == typeof (object))
				return TypeManager.object_type;
			if (t == typeof (System.Type))
				return TypeManager.type_type;
			if (t == typeof (System.IntPtr))
				return TypeManager.intptr_type;
			return t;
		}
	}

	/// <summary>
	///   Utility function that can be used to probe whether a type
	///   is managed or not.  
	/// </summary>
	public static bool VerifyUnManaged (Type t, Location loc)
	{
		if (IsUnmanagedType (t))
			return true;

		Report.Error (208, loc, "Cannot take the address of, get the size of, or declare a pointer to a managed type `{0}'",
			CSharpName (t));

		return false;	
	}
	
	/// <summary>
	///   Returns the name of the indexer in a given type.
	/// </summary>
	/// <remarks>
	///   The default is not always `Item'.  The user can change this behaviour by
	///   using the IndexerNameAttribute in the container.
	///
	///   For example, the String class indexer is named `Chars' not `Item' 
	/// </remarks>
	public static string IndexerPropertyName (Type t)
	{
		t = DropGenericTypeArguments (t);
		if (t is TypeBuilder) {
			TypeContainer tc = t.IsInterface ? LookupInterface (t) : LookupTypeContainer (t);
			return tc == null ? TypeContainer.DefaultIndexerName : tc.IndexerName;
		}
		
		System.Attribute attr = System.Attribute.GetCustomAttribute (
			t, TypeManager.default_member_type);
		if (attr != null){
			DefaultMemberAttribute dma = (DefaultMemberAttribute) attr;
			return dma.MemberName;
		}

		return TypeContainer.DefaultIndexerName;
	}

	static MethodInfo declare_local_method = null;
	
	public static LocalBuilder DeclareLocalPinned (ILGenerator ig, Type t)
	{
		if (declare_local_method == null){
			declare_local_method = typeof (ILGenerator).GetMethod (
				"DeclareLocal",
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				null, 
				new Type [] { typeof (Type), typeof (bool)},
				null);
			if (declare_local_method == null){
				Report.RuntimeMissingSupport (Location.Null, "pinned local variables");
				return ig.DeclareLocal (t);
			}
		}
		return (LocalBuilder) declare_local_method.Invoke (ig, new object [] { t, true });
	}

	//
	// Returns whether the array of memberinfos contains the given method
	//
	public static bool ArrayContainsMethod (MemberInfo [] array, MethodBase new_method)
	{
		Type [] new_args = TypeManager.GetParameterData (new_method).Types;
		
		foreach (MethodBase method in array) {
			if (method.Name != new_method.Name)
				continue;

                        if (method is MethodInfo && new_method is MethodInfo)
                                if (((MethodInfo) method).ReturnType != ((MethodInfo) new_method).ReturnType)
                                        continue;

                        
			Type [] old_args = TypeManager.GetParameterData (method).Types;
			int old_count = old_args.Length;
			int i;
			
			if (new_args.Length != old_count)
				continue;
			
			for (i = 0; i < old_count; i++){
				if (old_args [i] != new_args [i])
					break;
			}
			if (i != old_count)
				continue;

			return true;
		}
                
		return false;
	}
	
	//
	// We copy methods from `new_members' into `target_list' if the signature
	// for the method from in the new list does not exist in the target_list
	//
	// The name is assumed to be the same.
	//
	public static ArrayList CopyNewMethods (ArrayList target_list, IList new_members)
	{
		if (target_list == null){
			target_list = new ArrayList ();

			foreach (MemberInfo mi in new_members){
				if (mi is MethodBase)
					target_list.Add (mi);
			}
			return target_list;
		}
		
		MemberInfo [] target_array = new MemberInfo [target_list.Count];
		target_list.CopyTo (target_array, 0);
		
		foreach (MemberInfo mi in new_members){
			MethodBase new_method = (MethodBase) mi;
			
			if (!ArrayContainsMethod (target_array, new_method))
				target_list.Add (new_method);
		}
		return target_list;
	}

	// This method always return false for non-generic compiler,
	// while Type.IsGenericParameter is returned if it is supported.
	public static bool IsGenericParameter (Type type)
	{
		return type.IsGenericParameter;
	}

	public static int GenericParameterPosition (Type type)
	{
		return type.GenericParameterPosition;
	}

#region MemberLookup implementation
	
	//
	// Whether we allow private members in the result (since FindMembers
	// uses NonPublic for both protected and private), we need to distinguish.
	//

	static internal bool FilterNone (MemberInfo m, object filter_criteria)
	{
		return true;
	}

	internal class Closure {
		internal bool     private_ok;

		// Who is invoking us and which type is being queried currently.
		internal Type     invocation_type;
		internal Type     qualifier_type;

		// The assembly that defines the type is that is calling us
		internal Assembly invocation_assembly;
		internal IList almost_match;

		private bool CheckValidFamilyAccess (bool is_static, MemberInfo m)
		{
			if (invocation_type == null)
				return false;

			if (is_static && qualifier_type == null)
				// It resolved from a simple name, so it should be visible.
				return true;

			// A nested class has access to all the protected members visible to its parent.
			if (qualifier_type != null && TypeManager.IsNestedChildOf (invocation_type, qualifier_type))
				return true;

			if (IsNestedFamilyAccessible (invocation_type, m.DeclaringType)) {
				// Although a derived class can access protected members of its base class
				// it cannot do so through an instance of the base class (CS1540).
				// => Ancestry should be: declaring_type ->* invocation_type ->*  qualified_type
				if (is_static ||
				    qualifier_type == null ||
				    IsInstantiationOfSameGenericType (invocation_type, qualifier_type) ||
				    !IsFamilyAccessible (invocation_type, qualifier_type) ||
				    IsNestedChildOf (invocation_type, qualifier_type))
					return true;
			}
	
			if (almost_match != null)
				almost_match.Add (m);

			return false;
		}
		
		//
		// This filter filters by name + whether it is ok to include private
		// members in the search
		//
		internal bool Filter (MemberInfo m, object filter_criteria)
		{
			//
			// Hack: we know that the filter criteria will always be in the
			// `closure' // fields. 
			//

			if ((filter_criteria != null) && (m.Name != (string) filter_criteria))
				return false;

			if (((qualifier_type == null) || (qualifier_type == invocation_type)) &&
			    (invocation_type != null) &&
			    IsPrivateAccessible (m.DeclaringType, invocation_type))
				return true;

			//
			// Ugly: we need to find out the type of `m', and depending
			// on this, tell whether we accept or not
			//
			if (m is MethodBase){
				MethodBase mb = (MethodBase) m;
				MethodAttributes ma = mb.Attributes & MethodAttributes.MemberAccessMask;

				if (ma == MethodAttributes.Public)
					return true;
				
				if (ma == MethodAttributes.Private)
					return private_ok ||
						IsPrivateAccessible (invocation_type, m.DeclaringType) ||
						IsNestedChildOf (invocation_type, m.DeclaringType);

				if (invocation_assembly == mb.DeclaringType.Assembly ||
						TypeManager.IsFriendAssembly (mb.DeclaringType.Assembly)) {
					if (ma == MethodAttributes.Assembly || ma == MethodAttributes.FamORAssem)
						return true;
				} else {
					if (ma == MethodAttributes.Assembly || ma == MethodAttributes.FamANDAssem)
						return false;
				}

				// Family, FamORAssem or FamANDAssem
				return CheckValidFamilyAccess (mb.IsStatic, m);
			}
			
			if (m is FieldInfo){
				FieldInfo fi = (FieldInfo) m;
				FieldAttributes fa = fi.Attributes & FieldAttributes.FieldAccessMask;
				
				if (fa == FieldAttributes.Public)
					return true;
				
				if (fa == FieldAttributes.Private)
					return private_ok ||
						IsPrivateAccessible (invocation_type, m.DeclaringType) ||
						IsNestedChildOf (invocation_type, m.DeclaringType);

				if (invocation_assembly == fi.DeclaringType.Assembly ||
						TypeManager.IsFriendAssembly (fi.DeclaringType.Assembly)) {
					if (fa == FieldAttributes.Assembly || fa == FieldAttributes.FamORAssem)
						return true;
				} else {
					if (fa == FieldAttributes.Assembly || fa == FieldAttributes.FamANDAssem)
						return false;
				}

				// Family, FamORAssem or FamANDAssem
				return CheckValidFamilyAccess (fi.IsStatic, m);
			}

			//
			// EventInfos and PropertyInfos, return true because they lack
			// permission information, so we need to check later on the methods.
			//
			return true;
		}
	}

	static Closure closure = new Closure ();
	static MemberFilter FilterWithClosure_delegate = new MemberFilter (closure.Filter);

	//
	// Looks up a member called `name' in the `queried_type'.  This lookup
	// is done by code that is contained in the definition for `invocation_type'
	// through a qualifier of type `qualifier_type' (or null if there is no qualifier).
	//
	// `invocation_type' is used to check whether we're allowed to access the requested
	// member wrt its protection level.
	//
	// When called from MemberAccess, `qualifier_type' is the type which is used to access
	// the requested member (`class B { A a = new A (); a.foo = 5; }'; here invocation_type
	// is B and qualifier_type is A).  This is used to do the CS1540 check.
	//
	// When resolving a SimpleName, `qualifier_type' is null.
	//
	// The `qualifier_type' is used for the CS1540 check; it's normally either null or
	// the same than `queried_type' - except when we're being called from BaseAccess;
	// in this case, `invocation_type' is the current type and `queried_type' the base
	// type, so this'd normally trigger a CS1540.
	//
	// The binding flags are `bf' and the kind of members being looked up are `mt'
	//
	// The return value always includes private members which code in `invocation_type'
	// is allowed to access (using the specified `qualifier_type' if given); only use
	// BindingFlags.NonPublic to bypass the permission check.
	//
	// The 'almost_match' argument is used for reporting error CS1540.
	//
	// Returns an array of a single element for everything but Methods/Constructors
	// that might return multiple matches.
	//
	public static MemberInfo [] MemberLookup (Type invocation_type, Type qualifier_type,
						  Type queried_type, MemberTypes mt,
						  BindingFlags original_bf, string name, IList almost_match)
	{
		Timer.StartTimer (TimerType.MemberLookup);

		MemberInfo[] retval = RealMemberLookup (invocation_type, qualifier_type,
							queried_type, mt, original_bf, name, almost_match);

		Timer.StopTimer (TimerType.MemberLookup);

		return retval;
	}

	static MemberInfo [] RealMemberLookup (Type invocation_type, Type qualifier_type,
					       Type queried_type, MemberTypes mt,
					       BindingFlags original_bf, string name, IList almost_match)
	{
		BindingFlags bf = original_bf;
		
		ArrayList method_list = null;
		Type current_type = queried_type;
		bool searching = (original_bf & BindingFlags.DeclaredOnly) == 0;
		bool skip_iface_check = true, used_cache = false;
		bool always_ok_flag = invocation_type != null && IsNestedChildOf (invocation_type, queried_type);

		closure.invocation_type = invocation_type;
		closure.invocation_assembly = invocation_type != null ? invocation_type.Assembly : null;
		closure.qualifier_type = qualifier_type;
		closure.almost_match = almost_match;
		
		// This is from the first time we find a method
		// in most cases, we do not actually find a method in the base class
		// so we can just ignore it, and save the arraylist allocation
		MemberInfo [] first_members_list = null;
		bool use_first_members_list = false;
		
		do {
			MemberInfo [] list;

			//
			// `NonPublic' is lame, because it includes both protected and
			// private methods, so we need to control this behavior by
			// explicitly tracking if a private method is ok or not.
			//
			// The possible cases are:
			//    public, private and protected (internal does not come into the
			//    equation)
			//
			if ((invocation_type != null) &&
			    ((invocation_type == current_type) ||
			     IsNestedChildOf (invocation_type, current_type)) ||
			    always_ok_flag)
				bf = original_bf | BindingFlags.NonPublic;
			else
				bf = original_bf;

			closure.private_ok = (original_bf & BindingFlags.NonPublic) != 0;

			Timer.StopTimer (TimerType.MemberLookup);

			list = MemberLookup_FindMembers (
				current_type, mt, bf, name, out used_cache);

			Timer.StartTimer (TimerType.MemberLookup);

			//
			// When queried for an interface type, the cache will automatically check all
			// inherited members, so we don't need to do this here.  However, this only
			// works if we already used the cache in the first iteration of this loop.
			//
			// If we used the cache in any further iteration, we can still terminate the
			// loop since the cache always looks in all base classes.
			//

			if (used_cache)
				searching = false;
			else
				skip_iface_check = false;

			if (current_type == TypeManager.object_type)
				searching = false;
			else {
				current_type = current_type.BaseType;
				
				//
				// This happens with interfaces, they have a null
				// basetype.  Look members up in the Object class.
				//
				if (current_type == null) {
					current_type = TypeManager.object_type;
					searching = true;
				}
			}
			
			if (list.Length == 0)
				continue;

			//
			// Events and types are returned by both `static' and `instance'
			// searches, which means that our above FindMembers will
			// return two copies of the same.
			//
			if (list.Length == 1 && !(list [0] is MethodBase)){
				return list;
			}

			//
			// Multiple properties: we query those just to find out the indexer
			// name
			//
			if (list [0] is PropertyInfo)
				return list;

			//
			// We found an event: the cache lookup returns both the event and
			// its private field.
			//
			if (list [0] is EventInfo) {
				if ((list.Length == 2) && (list [1] is FieldInfo))
					return new MemberInfo [] { list [0] };

				// Oooops
				return null;
			}

			//
			// We found methods, turn the search into "method scan"
			// mode.
			//

 			if (first_members_list != null) {
 				if (use_first_members_list) {
 					method_list = CopyNewMethods (method_list, first_members_list);
 					use_first_members_list = false;
 				}
 				
				method_list = CopyNewMethods (method_list, list);
			} else {
				first_members_list = list;
 				use_first_members_list = true;

				mt &= (MemberTypes.Method | MemberTypes.Constructor);
			}
		} while (searching);

 		if (use_first_members_list) {
 			foreach (MemberInfo mi in first_members_list) {
 				if (! (mi is MethodBase)) {
 					method_list = CopyNewMethods (method_list, first_members_list);
 					return (MemberInfo []) method_list.ToArray (typeof (MemberInfo));
 				}
 			}
 			return (MemberInfo []) first_members_list;
 		}

		if (method_list != null && method_list.Count > 0) {
                        return (MemberInfo []) method_list.ToArray (typeof (MemberInfo));
                }
		//
		// This happens if we already used the cache in the first iteration, in this case
		// the cache already looked in all interfaces.
		//
		if (skip_iface_check)
			return null;

		//
		// Interfaces do not list members they inherit, so we have to
		// scan those.
		// 
		if (!queried_type.IsInterface)
			return null;

		if (queried_type.IsArray)
			queried_type = TypeManager.array_type;
		
		Type [] ifaces = GetInterfaces (queried_type);
		if (ifaces == null)
			return null;
		
		foreach (Type itype in ifaces){
			MemberInfo [] x;

			x = MemberLookup (null, null, itype, mt, bf, name, null);
			if (x != null)
				return x;
		}
					
		return null;
	}

	// Tests whether external method is really special
	public static bool IsSpecialMethod (MethodBase mb)
	{
		string name = mb.Name;
		if (name.StartsWith ("get_") || name.StartsWith ("set_"))
			return mb.DeclaringType.GetProperty (name.Substring (4)) != null;
				
		if (name.StartsWith ("add_"))
			return mb.DeclaringType.GetEvent (name.Substring (4)) != null;

		if (name.StartsWith ("remove_"))
			return mb.DeclaringType.GetEvent (name.Substring (7)) != null;

		if (name.StartsWith ("op_")){
			foreach (string oname in Unary.oper_names) {
				if (oname == name)
					return true;
			}

			foreach (string oname in Binary.oper_names) {
				if (oname == name)
					return true;
			}
		}
		return false;
	}

#endregion
	
}

/// <summary>
///   There is exactly one instance of this class per type.
/// </summary>
public sealed class TypeHandle : IMemberContainer {
	public readonly IMemberContainer BaseType;

	readonly int id = ++next_id;
	static int next_id = 0;

	static TypeHandle ()
	{
		Reset ();
	}

	/// <summary>
	///   Lookup a TypeHandle instance for the given type.  If the type doesn't have
	///   a TypeHandle yet, a new instance of it is created.  This static method
	///   ensures that we'll only have one TypeHandle instance per type.
	/// </summary>
	private static TypeHandle GetTypeHandle (Type t)
	{
		TypeHandle handle = (TypeHandle) type_hash [t];
		if (handle != null)
			return handle;

		handle = new TypeHandle (t);
		type_hash.Add (t, handle);
		return handle;
	}

	public static MemberCache GetMemberCache (Type t)
	{
		return GetTypeHandle (t).MemberCache;
	}
	
	public static void CleanUp ()
	{
		type_hash = null;
	}

	public static void Reset ()
	{
		type_hash = new PtrHashtable ();
	}

	/// <summary>
	///   Returns the TypeHandle for TypeManager.object_type.
	/// </summary>
	public static IMemberContainer ObjectType {
		get {
			if (object_type != null)
				return object_type;

			object_type = GetTypeHandle (TypeManager.object_type);

			return object_type;
		}
	}

	/// <summary>
	///   Returns the TypeHandle for TypeManager.array_type.
	/// </summary>
	public static IMemberContainer ArrayType {
		get {
			if (array_type != null)
				return array_type;

			array_type = GetTypeHandle (TypeManager.array_type);

			return array_type;
		}
	}

	private static PtrHashtable type_hash;

	private static TypeHandle object_type = null;
	private static TypeHandle array_type = null;

	private Type type;
	private string full_name;
	private bool is_interface;
	private MemberCache member_cache;
	private MemberCache base_cache;

	private TypeHandle (Type type)
	{
		this.type = type;
		full_name = type.FullName != null ? type.FullName : type.Name;
		if (type.BaseType != null) {
			base_cache = TypeManager.LookupMemberCache (type.BaseType);
			BaseType = base_cache.Container;
		} else if (type.IsInterface)
			base_cache = TypeManager.LookupBaseInterfacesCache (type);
		this.is_interface = type.IsInterface || type.IsGenericParameter;
		this.member_cache = new MemberCache (this);
	}

	// IMemberContainer methods

	public string Name {
		get {
			return full_name;
		}
	}

	public Type Type {
		get {
			return type;
		}
	}

	public MemberCache BaseCache {
		get {
			return base_cache;
		}
	}

	public bool IsInterface {
		get {
			return is_interface;
		}
	}

	public MemberList GetMembers (MemberTypes mt, BindingFlags bf)
	{
                MemberInfo [] members;
		if (type is GenericTypeParameterBuilder)
			return MemberList.Empty;
		if (mt == MemberTypes.Event)
                        members = type.GetEvents (bf | BindingFlags.DeclaredOnly);
                else
                        members = type.FindMembers (mt, bf | BindingFlags.DeclaredOnly,
                                                    null, null);
                Array.Reverse (members);

                return new MemberList (members);
	}

	// IMemberFinder methods

	public MemberList FindMembers (MemberTypes mt, BindingFlags bf, string name,
				       MemberFilter filter, object criteria)
	{
		return new MemberList (member_cache.FindMembers (mt, bf, name, filter, criteria));
	}

	public MemberCache MemberCache {
		get {
			return member_cache;
		}
	}

	public override string ToString ()
	{
		if (BaseType != null)
			return "TypeHandle (" + id + "," + Name + " : " + BaseType + ")";
		else
			return "TypeHandle (" + id + "," + Name + ")";
	}
}

}
