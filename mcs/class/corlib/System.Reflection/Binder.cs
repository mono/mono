// System.Reflection.Binder
//
// Authors:
// 	Sean MacIsaac (macisaac@ximian.com)
// 	Paolo Molaro (lupus@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Marek Safar (marek.safar@gmail.com)
//
// (C) Ximian, Inc. 2001 - 2003
// (c) Copyright 2004 Novell, Inc. (http://www.novell.com)
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace System.Reflection
{
	[ComVisible (true)]
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class Binder
	{
		protected Binder () {}

		public abstract FieldInfo BindToField (BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture);
		public abstract MethodBase BindToMethod (BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state);
		public abstract object ChangeType (object value, Type type, CultureInfo culture);
		public abstract void ReorderArgumentArray( ref object[] args, object state);
		public abstract MethodBase SelectMethod (BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers);
		public abstract PropertyInfo SelectProperty( BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers);

		static readonly Binder default_binder = new Default ();

		internal static Binder DefaultBinder {
			get {
				return default_binder;
			}
		}
		
		internal void ConvertValues (object[] args, ParameterInfo[] pinfo, CultureInfo culture, bool exactMatch)
		{
			if (args == null) {
				if (pinfo.Length == 0)
					return;
				
				throw new TargetParameterCountException ();
			}

			if (pinfo.Length != args.Length)
				throw new TargetParameterCountException ();
			
			for (int i = 0; i < args.Length; ++i) {
				var arg = args [i];
				var pi = pinfo [i];
				if (arg == Type.Missing) {
					args [i] = pi.DefaultValue;
					continue;
				}

				args [i] = ConvertValue (arg, pi.ParameterType, culture, exactMatch);
			}
		}

		internal object ConvertValue (object value, Type type, CultureInfo culture, bool exactMatch)
		{
			bool failed = false;
			var res = TryConvertToType (value, type, ref failed);
			if (!failed)
				return res;

			if (exactMatch || this == default_binder)
				throw new ArgumentException ("Object type " + value.GetType() + " cannot be converted to target type: " + type.FullName);

			return ChangeType (value, type, culture);
		}

		object TryConvertToType (object value, Type type, ref bool failed)
		{
			if (type.IsInstanceOfType (value)) {
				return value;
			}

			if (type.IsByRef) {
        		var elementType = type.GetElementType ();
        		if (value == null || elementType.IsInstanceOfType (value)) {
					return value;
				}
			}

			if (value == null)
				return value;

			if (type.IsEnum) {
				type = Enum.GetUnderlyingType (type);
				if (type == value.GetType ())
					return value;
			}

			if (type.IsPrimitive) {
				var res = IsConvertibleToPrimitiveType (value, type);
				if (res != null)
					return res;
			} else if (type.IsPointer) {
				var vtype = value.GetType ();
				if (vtype == typeof (IntPtr) || vtype == typeof (UIntPtr))
					return value;
			}

			failed = true;
			return null;
		}

		// Binder uses some incompatible conversion rules. For example
		// int value cannot be used with decimal parameter but in other
		// ways it's more flexible than normal convertor, for example
		// long value can be used with int based enum
		static object IsConvertibleToPrimitiveType (object value, Type targetType)		
		{
			var type = value.GetType ();
			if (type.IsEnum) {
				type = Enum.GetUnderlyingType (type);
				if (type == targetType)
					return value;
			}

			var from = Type.GetTypeCode (type);
			var to = Type.GetTypeCode (targetType);

			switch (to) {
				case TypeCode.Char:
					switch (from) {
						case TypeCode.Byte:
							return (Char) (Byte) value;
						case TypeCode.UInt16:
							return value;
					}
					break;
				case TypeCode.Int16:
					switch (from) {
						case TypeCode.Byte:
							return (Int16) (Byte) value;
						case TypeCode.SByte:
							return (Int16) (SByte) value;						
					}
					break;
				case TypeCode.UInt16:
					switch (from) {
						case TypeCode.Byte:
							return (UInt16) (Byte) value;
						case TypeCode.Char:
							return value;
					}
					break;
				case TypeCode.Int32:
					switch (from) {
						case TypeCode.Byte:
							return (Int32) (Byte) value;
						case TypeCode.SByte:
							return (Int32) (SByte) value;
						case TypeCode.Char:
							return (Int32) (Char) value;
						case TypeCode.Int16:
							return (Int32) (Int16) value;
						case TypeCode.UInt16:
							return (Int32) (UInt16) value;
					}
					break;
				case TypeCode.UInt32:
					switch (from) {
						case TypeCode.Byte:
							return (UInt32) (Byte) value;
						case TypeCode.Char:
							return (UInt32) (Char) value;
						case TypeCode.UInt16:
							return (UInt32) (UInt16) value;
					}
					break;
				case TypeCode.Int64:
					switch (from) {
						case TypeCode.Byte:
							return (Int64) (Byte) value;
						case TypeCode.SByte:
							return (Int64) (SByte) value;							
						case TypeCode.Int16:
							return (Int64) (Int16) value;
						case TypeCode.Char:
							return (Int64) (Char) value;
						case TypeCode.UInt16:
							return (Int64) (UInt16) value;
						case TypeCode.Int32:
							return (Int64) (Int32) value;
						case TypeCode.UInt32:
							return (Int64) (UInt32) value;
					}
					break;
				case TypeCode.UInt64:
					switch (from) {
						case TypeCode.Byte:
							return (UInt64) (Byte) value;
						case TypeCode.Char:
							return (UInt64) (Char) value;
						case TypeCode.UInt16:
							return (UInt64) (UInt16) value;
						case TypeCode.UInt32:
							return (UInt64) (UInt32) value;
					}
					break;
				case TypeCode.Single:
					switch (from) {
						case TypeCode.Byte:
							return (Single) (Byte) value;
						case TypeCode.SByte:
							return (Single) (SByte) value;
						case TypeCode.Int16:
							return (Single) (Int16) value;
						case TypeCode.Char:
							return (Single) (Char) value;
						case TypeCode.UInt16:
							return (Single) (UInt16) value;
						case TypeCode.Int32:
							return (Single) (Int32) value;
						case TypeCode.UInt32:
							return (Single) (UInt32) value;
						case TypeCode.Int64:
							return (Single) (Int64) value;
						case TypeCode.UInt64:
							return (Single) (UInt64) value;
					}
					break;
				case TypeCode.Double:
					switch (from) {
						case TypeCode.Byte:
							return (Double) (Byte) value;
						case TypeCode.SByte:
							return (Double) (SByte) value;
						case TypeCode.Char:
							return (Double) (Char) value;
						case TypeCode.Int16:
							return (Double) (Int16) value;
						case TypeCode.UInt16:
							return (Double) (UInt16) value;
						case TypeCode.Int32:
							return (Double) (Int32) value;
						case TypeCode.UInt32:
							return (Double) (UInt32) value;
						case TypeCode.Int64:
							return (Double) (Int64) value;
						case TypeCode.UInt64:
							return (Double) (UInt64) value;
						case TypeCode.Single:
							return (Double) (Single) value;
					}
					break;
			}

			// Everything else is rejected
			return null;
		}

		internal static int GetDerivedLevel (Type type) 
		{
			Type searchType = type;
			int level = 1;

			while (searchType.BaseType != null) 
			{
				level++;
				searchType = searchType.BaseType;
			}

			return level;
		}

		internal static MethodBase FindMostDerivedMatch (MethodBase [] match) 
		{
			int highLevel = 0;
			int matchId = -1;
			int count = match.Length;

			for (int current = 0; current < count; current++) 
			{
				MethodBase m = match [current];
				int level = GetDerivedLevel (m.DeclaringType);
				if (level == highLevel)
					throw new AmbiguousMatchException ();
				// If the argument types differ we
				// have an ambigous match, as well
				if (matchId >= 0) {
					ParameterInfo[] p1 = m.GetParametersInternal ();
					ParameterInfo[] p2 = match [matchId].GetParametersInternal ();
					bool equal = true;

					if (p1.Length != p2.Length)
						equal = false;
					else {
						int i;

						for (i = 0; i < p1.Length; ++i) {
							if (p1 [i].ParameterType != p2 [i].ParameterType) {
								equal = false;
								break;
							}
						}
					}

					if (!equal)
						throw new AmbiguousMatchException ();
				}

				if (level > highLevel) 
				{
					highLevel = level;
					matchId = current;
				}
			}

			return match[matchId];
		}

		internal sealed class Default : Binder {
			public override FieldInfo BindToField (BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture) 
			{
				if (match == null)
					throw new ArgumentNullException ("match");
				foreach (FieldInfo f in match) {
					if (check_type (value.GetType (), f.FieldType))
						return f;
				}
				return null;
			}

			public override MethodBase BindToMethod (BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state)
			{
				Type[] types;
				if (args == null)
					types = Type.EmptyTypes;
				else {
					types = new Type [args.Length];
					for (int i = 0; i < args.Length; ++i) {
						if (args [i] != null)
							types [i] = args [i].GetType ();
					}
				}

				MethodBase selected = null;
				if (names != null) {
					foreach (var m in match) {
						var parameters = m.GetParametersInternal ();
						int i;

						/*
						 * Find the corresponding parameter for each parameter name,
						 * reorder types/modifiers array during the search.
						 */
						Type[] newTypes = new Type [types.Length];
						Array.FastCopy (types, 0, newTypes, 0, types.Length);

						ParameterModifier[] newModifiers = null;
						if (modifiers != null) {
							newModifiers = new ParameterModifier [modifiers.Length];
							Array.FastCopy (modifiers, 0, newModifiers, 0, modifiers.Length);
						}

						for (i = 0; i < names.Length; ++i) {
							/* Find the corresponding parameter */
							int nindex = -1;
							for (int j = 0; j < parameters.Length; ++j) {
								if (parameters [j].Name == names [i]) {
									nindex = j;
									break;
								}
							}
							if (nindex == -1)
								break;
							if (i < newTypes.Length && nindex < types.Length)
								newTypes [i] = types [nindex];
							if (modifiers != null && i < newModifiers.Length && nindex < modifiers.Length)
								newModifiers [i] = modifiers [nindex];
						}
						if (i < names.Length)
							continue;

						selected = SelectMethod (bindingAttr, new MethodBase [] { m }, newTypes, newModifiers, true, ref args);
						if (selected != null)
							break;
					}
				} else {
					selected = SelectMethod (bindingAttr, match, types, modifiers, true, ref args);
				}

				state = null;
				if (selected != null && names != null)
					ReorderParameters (names, ref args, selected);

				if (selected != null) {
					if (args == null)
						args = EmptyArray<object>.Value;
	
					AdjustArguments (selected, ref args);
				}

				return selected;
			}

			// probably belongs in ReorderArgumentArray
			static void AdjustArguments (MethodBase selected, ref object [] args)
			{
				var parameters = selected.GetParametersInternal ();
				var parameters_length = parameters.Length;
				if (parameters_length == 0)
					return;

				var last_parameter = parameters [parameters.Length - 1];
				Type last_parameter_type = last_parameter.ParameterType;
				if (!Attribute.IsDefined (last_parameter, typeof (ParamArrayAttribute)))
					return;

				var args_length = args.Length;
				var param_args_count = args_length + 1 - parameters_length;
				var first_vararg_index = args_length - param_args_count;
				if (first_vararg_index < args_length) {
					var first_vararg = args [first_vararg_index];
					if (first_vararg != null && first_vararg.GetType () == last_parameter_type)
						return;
				}
				
				var params_args = Array.CreateInstance (last_parameter_type.GetElementType (), param_args_count);
				for (int i = 0; i < param_args_count; i++)
					params_args.SetValue (args [first_vararg_index + i], i);

				var adjusted = new object [parameters_length];
				Array.Copy (args, adjusted, parameters_length - 1);
				
				adjusted [adjusted.Length - 1] = params_args;
				args = adjusted;
			}

			void ReorderParameters (string [] names, ref object [] args, MethodBase selected)
			{
				object [] newArgs = new object [args.Length];
				Array.Copy (args, newArgs, args.Length);
				ParameterInfo [] plist = selected.GetParametersInternal ();
				for (int n = 0; n < names.Length; n++)
					for (int p = 0; p < plist.Length; p++) {
						if (names [n] == plist [p].Name) {
							newArgs [p] = args [n];
							break;
						}
					}
				Array.Copy (newArgs, args, args.Length);
			}
			
			public override object ChangeType (object value, Type type, CultureInfo culture)
			{
				throw new NotSupportedException ();
			}

			[MonoTODO ("This method does not do anything in Mono")]
			public override void ReorderArgumentArray (ref object[] args, object state)
			{
				//do nothing until we support named arguments
				//throw new NotImplementedException ();
			}

			private static bool check_type (Type from, Type to) {
				if (from == to)
					return true;

				if (from == null)
					return true;

				if (to.IsByRef != from.IsByRef)
					return false;

				if (to.IsInterface)
					return to.IsAssignableFrom (from);

				if (to.IsEnum) {
					to = Enum.GetUnderlyingType (to);
					if (from == to)
						return true;
				}

				if (to.IsGenericType && to.GetGenericTypeDefinition () == typeof (Nullable<>) && to.GetGenericArguments ()[0] == from)
					return true;

				TypeCode fromt = Type.GetTypeCode (from);
				TypeCode tot = Type.GetTypeCode (to);

				switch (fromt) {
				case TypeCode.Char:
					switch (tot) {
					case TypeCode.UInt16:
					case TypeCode.UInt32:
					case TypeCode.Int32:
					case TypeCode.UInt64:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return to == typeof (object);
				case TypeCode.Byte:
					switch (tot) {
					case TypeCode.Char:
					case TypeCode.UInt16:
					case TypeCode.Int16:
					case TypeCode.UInt32:
					case TypeCode.Int32:
					case TypeCode.UInt64:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return to == typeof (object) || (from.IsEnum && to == typeof (Enum));
				case TypeCode.SByte:
					switch (tot) {
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return to == typeof (object) || (from.IsEnum && to == typeof (Enum));
				case TypeCode.UInt16:
					switch (tot) {
					case TypeCode.UInt32:
					case TypeCode.Int32:
					case TypeCode.UInt64:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return to == typeof (object) || (from.IsEnum && to == typeof (Enum));
				case TypeCode.Int16:
					switch (tot) {
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return to == typeof (object) || (from.IsEnum && to == typeof (Enum));
				case TypeCode.UInt32:
					switch (tot) {
					case TypeCode.UInt64:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return to == typeof (object) || (from.IsEnum && to == typeof (Enum));
				case TypeCode.Int32:
					switch (tot) {
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return to == typeof (object) || (from.IsEnum && to == typeof (Enum));
				case TypeCode.UInt64:
				case TypeCode.Int64:
					switch (tot) {
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return to == typeof (object) || (from.IsEnum && to == typeof (Enum));
				case TypeCode.Single:
					return tot == TypeCode.Double || to == typeof (object);
				default:
					/* TODO: handle valuetype -> byref */
					if (to == typeof (object) && from.IsValueType)
						return true;
					if (to.IsPointer && from == typeof (IntPtr))
						return true;

					return to.IsAssignableFrom (from);
				}
			}

			private static bool check_arguments (Type[] types, ParameterInfo[] args, bool allowByRefMatch) {
				for (int i = 0; i < types.Length; ++i) {
					bool match = check_type (types [i], args [i].ParameterType);
					if (!match && allowByRefMatch) {
						Type param_type = args [i].ParameterType;
						if (param_type.IsByRef)
							match = check_type (types [i], param_type.GetElementType ());
					}
					if (!match)
						return false;
				}
				return true;
			}

			public override MethodBase SelectMethod (BindingFlags bindingAttr, MethodBase [] match, Type [] types, ParameterModifier [] modifiers)
			{
				object[] args = null;
				return SelectMethod (bindingAttr, match, types, modifiers, false, ref args);
			}

			MethodBase SelectMethod (BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers, bool allowByRefMatch, ref object[] arguments)
			{
				MethodBase m;
				int i, j;

				if (match == null)
					throw new ArgumentNullException ("match");

				/* first look for an exact match... */
				MethodBase exact_match = null;
				for (i = 0; i < match.Length; ++i) {
					m = match [i];
					if (m.GetParametersCount () != types.Length)
						continue;

					ParameterInfo[] args = m.GetParametersInternal ();
					for (j = 0; j < types.Length; ++j) {
						if (types [j] != args [j].ParameterType)
							break;
					}
					if (j == types.Length) {
						if (exact_match != null) {
							exact_match = null;
							break;
						} else {
							exact_match = m;
						}
					}
				}
				if (exact_match != null)
					return exact_match;

				/* Try methods with ParamArray attribute */
				if (arguments != null) {
					for (i = 0; i < match.Length; ++i) {
						m = match [i];

						var count = m.GetParametersCount ();
						if (count == 0 || count > types.Length + 1)
							continue;

						var pi = m.GetParametersInternal ();
						if (!Attribute.IsDefined (pi [pi.Length - 1], typeof (ParamArrayAttribute)))
							continue;

						var elementType = pi [pi.Length - 1].ParameterType.GetElementType ();
						for (j = 0; j < types.Length; ++j) {
							if (j < (pi.Length - 1) && types [j] != pi [j].ParameterType)
								break;
							
							if (j >= (pi.Length - 1) && types [j] != elementType) 
								break;
						}

						if (j == types.Length)
							return m;
					}
				}

				if ((bindingAttr & BindingFlags.ExactBinding) != 0)
					return null;

				MethodBase result = null;
				ParameterInfo[] result_pi = null;
				for (i = 0; i < match.Length; ++i) {
					m = match [i];
					var pi = m.GetParametersInternal ();
					var full_pi = pi;
					if (pi.Length != types.Length) {
						if ((bindingAttr & BindingFlags.OptionalParamBinding) == 0)
							continue;

						List<ParameterInfo> pi_reduced = null;
						for (var ii = pi.Length - 1; ii >= 0; --ii) {
							if ((pi [ii].Attributes & ParameterAttributes.HasDefault) == 0)
								break;

							if (pi_reduced == null) {
								pi_reduced = new List<ParameterInfo> (pi);
							}

							pi_reduced.RemoveAt (ii);
						}

						if (pi_reduced == null || pi_reduced.Count != types.Length)
							continue;

						pi = pi_reduced.ToArray ();
					}

					if (!check_arguments (types, pi, allowByRefMatch))
						continue;

					if (result != null) {
						result = GetBetterMethod (result, m, types);
						if (result != m)
							continue;
					}

					result = m;
					result_pi = full_pi;
				}

				if (result != null) {
					i = arguments == null ? 0 : arguments.Length;
					Array.Resize (ref arguments, result_pi.Length);
					for (; i < arguments.Length; ++i)
						arguments [i] = result_pi [i].DefaultValue;

					return result;
				}

				if (arguments == null || types.Length != arguments.Length)
					return null;

				// Xamarin-5278: try with parameters that are COM objects
				// REVIEW: do we also need to implement best method match?
				for (i = 0; i < match.Length; ++i) {
					m = match [i];
					ParameterInfo[] methodArgs = m.GetParametersInternal ();
					if (methodArgs.Length != types.Length)
						continue;
					for (j = 0; j < types.Length; ++j) {
						var requiredType = methodArgs [j].ParameterType;
						if (types [j] == requiredType)
							continue;
#if !MOBILE
						if (types [j] == typeof (__ComObject) && requiredType.IsInterface) {
							var iface = Marshal.GetComInterfaceForObject (arguments [j], requiredType);
							if (iface != IntPtr.Zero) {
								// the COM object implements the desired interface
								Marshal.Release (iface);
								continue;
							}
						}
#endif
						break;
					}

					if (j == types.Length)
						return m;
				}
				return null;
			}

			MethodBase GetBetterMethod (MethodBase m1, MethodBase m2, Type [] types)
			{
				ParameterInfo [] pl1 = m1.GetParametersInternal ();
				ParameterInfo [] pl2 = m2.GetParametersInternal ();
				int prev = 0;
				for (int i = 0; i < pl1.Length; i++) {
					int cmp = CompareCloserType (pl1 [i].ParameterType, pl2 [i].ParameterType);
					if (cmp != 0 && prev != 0 && prev != cmp)
						throw new AmbiguousMatchException ();
					if (cmp != 0)
						prev = cmp;
				}
				if (prev != 0)
					return prev > 0 ? m2 : m1;

				Type dt1 = m1.DeclaringType;
				Type dt2 = m2.DeclaringType;
				if (dt1 != dt2) {
					if (dt1.IsSubclassOf(dt2))
						return m1;
					if (dt2.IsSubclassOf(dt1))
						return m2;
				}

				bool va1 = (m1.CallingConvention & CallingConventions.VarArgs) != 0;
				bool va2 = (m2.CallingConvention & CallingConventions.VarArgs) != 0;
				if (va1 && !va2)
					return m2;
				if (va2 && !va1)
					return m1;

				throw new AmbiguousMatchException ();
			}

			int CompareCloserType (Type t1, Type t2)
			{
				if (t1 == t2)
					return 0;
				if (t1.IsGenericParameter && !t2.IsGenericParameter)
					return 1; // t2
				if (!t1.IsGenericParameter && t2.IsGenericParameter)
					return -1; // t1
				if (t1.HasElementType && t2.HasElementType)
					return CompareCloserType (
						t1.GetElementType (),
						t2.GetElementType ());

				if (t1.IsSubclassOf (t2))
					return -1; // t1
				if (t2.IsSubclassOf (t1))
					return 1; // t2

				if (t1.IsInterface && Array.IndexOf (t2.GetInterfaces (), t1) >= 0)
					return 1; // t2
				if (t2.IsInterface && Array.IndexOf (t1.GetInterfaces (), t2) >= 0)
					return -1; // t1

				// What kind of cases could reach here?
				return 0;
			}

			public override PropertyInfo SelectProperty (BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
			{
				if (match == null || match.Length == 0)
					throw new ArgumentException ("No properties provided", "match");

				bool haveRet = (returnType != null);
				int idxlen = (indexes != null) ? indexes.Length : -1;
				PropertyInfo result = null;
				int i;
				int best_score = Int32.MaxValue - 1;
				int fail_score = Int32.MaxValue;
				int level = 0;
				
				for (i = match.Length - 1; i >= 0; i--) {
					PropertyInfo p = match [i];
					ParameterInfo[] args = p.GetIndexParameters ();
					if (idxlen >= 0 && idxlen != args.Length)
						continue;

					if (haveRet && p.PropertyType != returnType)
						continue;

					int score = Int32.MaxValue - 1;
					if (idxlen > 0) {
						score = check_arguments_with_score (indexes, args);
						if (score == -1)
							continue;
					}

					int new_level = GetDerivedLevel (p.DeclaringType);
					if (result != null) {
						if (best_score < score)
							continue;

						if (best_score == score) {
							if (level == new_level) {
								// Keep searching. May be there's something
								// better for us.
								fail_score = score;
								continue;
							}

							if (level > new_level)
								continue;
						}
					}

					result = p;
					best_score = score;
					level = new_level;
				}

				if (fail_score <= best_score)
					throw new AmbiguousMatchException ();

				return result;
			}

			static int check_arguments_with_score (Type [] types, ParameterInfo [] args)
			{
				int worst = -1;

				for (int i = 0; i < types.Length; ++i) {
					int res = check_type_with_score (types [i], args [i].ParameterType);
					if (res == -1)
						return -1;

					if (worst < res)
						worst = res;
				}

				return worst;
			}

			// 0 -> same type or null and !valuetype
			// 1 -> to == Enum
			// 2 -> value type that don't lose data
			// 3 -> to == IsAssignableFrom
			// 4 -> to == object
			static int check_type_with_score (Type from, Type to)
			{
				if (from == null)
					return to.IsValueType ? -1 : 0;

				if (from == to)
					return 0;

				if (to == typeof (object))
					return 4;

				TypeCode fromt = Type.GetTypeCode (from);
				TypeCode tot = Type.GetTypeCode (to);

				switch (fromt) {
				case TypeCode.Char:
					switch (tot) {
					case TypeCode.UInt16:
						return 0;

					case TypeCode.UInt32:
					case TypeCode.Int32:
					case TypeCode.UInt64:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return 2;
					}
					return -1;
				case TypeCode.Byte:
					switch (tot) {
					case TypeCode.Char:
					case TypeCode.UInt16:
					case TypeCode.Int16:
					case TypeCode.UInt32:
					case TypeCode.Int32:
					case TypeCode.UInt64:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return 2;
					}
					return (from.IsEnum && to == typeof (Enum)) ? 1 : -1;
				case TypeCode.SByte:
					switch (tot) {
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return 2;
					}
					return (from.IsEnum && to == typeof (Enum)) ? 1 : -1;
				case TypeCode.UInt16:
					switch (tot) {
					case TypeCode.UInt32:
					case TypeCode.Int32:
					case TypeCode.UInt64:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return 2;
					}
					return (from.IsEnum && to == typeof (Enum)) ? 1 : -1;
				case TypeCode.Int16:
					switch (tot) {
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return 2;
					}
					return (from.IsEnum && to == typeof (Enum)) ? 1 : -1;
				case TypeCode.UInt32:
					switch (tot) {
					case TypeCode.UInt64:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return 2;
					}
					return (from.IsEnum && to == typeof (Enum)) ? 1 : -1;
				case TypeCode.Int32:
					switch (tot) {
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return 2;
					}
					return (from.IsEnum && to == typeof (Enum)) ? 1 : -1;
				case TypeCode.UInt64:
				case TypeCode.Int64:
					switch (tot) {
					case TypeCode.Single:
					case TypeCode.Double:
						return 2;
					}
					return (from.IsEnum && to == typeof (Enum)) ? 1 : -1;
				case TypeCode.Single:
					return tot == TypeCode.Double ? 2 : -1;
				default:
					return (to.IsAssignableFrom (from)) ? 3 : -1;
				}
			}
		}
	}
}

