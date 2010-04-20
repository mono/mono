// System.Reflection.Binder
//
// Authors:
// 	Sean MacIsaac (macisaac@ximian.com)
// 	Paolo Molaro (lupus@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Ximian, Inc. 2001 - 2003
// (c) Copyright 2004 Novell, Inc. (http://www.novell.com)

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

		static Binder default_binder = new Default ();

		internal static Binder DefaultBinder {
			get {
				return default_binder;
			}
		}
		
		internal static bool ConvertArgs (Binder binder, object[] args, ParameterInfo[] pinfo, CultureInfo culture) {
			if (args == null) {
				if ( pinfo.Length == 0)
					return true;
				else
					throw new TargetParameterCountException ();
			}
			if (pinfo.Length != args.Length)
				throw new TargetParameterCountException ();
			for (int i = 0; i < args.Length; ++i) {
				object v = binder.ChangeType (args [i], pinfo[i].ParameterType, culture);
				if ((v == null) && (args [i] != null))
					return false;
				args [i] = v;
			}
			return true;
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
					ParameterInfo[] p1 = m.GetParameters ();
					ParameterInfo[] p2 = match [matchId].GetParameters ();
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

			//
			// FIXME: There was a MonoTODO, but it does not explain what the problem is
			// 
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
				MethodBase selected = SelectMethod (bindingAttr, match, types, modifiers, true);
				state = null;
				if (names != null)
					ReorderParameters (names, ref args, selected);

				if (selected != null) {
					if (args == null)
						args = new object [0];
	
					AdjustArguments (selected, ref args);
				}

				return selected;
			}

			// probably belongs in ReorderArgumentArray
			static void AdjustArguments (MethodBase selected, ref object [] args)
			{
				var parameters = selected.GetParameters ();
				if (parameters.Length == 0)
					return;

				var last_parameter = parameters [parameters.Length - 1];
				if (!Attribute.IsDefined (last_parameter, typeof (ParamArrayAttribute)))
					return;

				var adjusted = new object [parameters.Length];
				Array.Copy (args, adjusted, parameters.Length - 1);

				var param_args_count = args.Length + 1 - parameters.Length;
				var params_args = Array.CreateInstance (last_parameter.ParameterType.GetElementType (), param_args_count);

				for (int i = 0; i < param_args_count; i++)
					params_args.SetValue (args [args.Length - param_args_count + i], i);

				adjusted [adjusted.Length - 1] = params_args;
				args = adjusted;
			}

			void ReorderParameters (string [] names, ref object [] args, MethodBase selected)
			{
				object [] newArgs = new object [args.Length];
				Array.Copy (args, newArgs, args.Length);
				ParameterInfo [] plist = selected.GetParameters ();
				for (int n = 0; n < names.Length; n++)
					for (int p = 0; p < plist.Length; p++) {
						if (names [n] == plist [p].Name) {
							newArgs [p] = args [n];
							break;
						}
					}
				Array.Copy (newArgs, args, args.Length);
			}

			static bool IsArrayAssignable (Type object_type, Type target_type)
			{
				if (object_type.IsArray && target_type.IsArray)
					return IsArrayAssignable (object_type.GetElementType (), target_type.GetElementType ());
						
				if (target_type.IsAssignableFrom (object_type))
					return true;

				return false;
			}
			
			public override object ChangeType (object value, Type type, CultureInfo culture)
			{
				if (value == null)
					return null;
				Type vtype = value.GetType ();
				if (type.IsByRef)
					type = type.GetElementType ();
				if (vtype == type || type.IsInstanceOfType (value))
					return value;
				if (vtype.IsArray && type.IsArray){
					if (IsArrayAssignable (vtype.GetElementType (), type.GetElementType ()))
						return value;
				}

				if (check_type (vtype, type)) {
					// These are not supported by Convert
					if (type.IsEnum)
						return Enum.ToObject (type, value);
					if (vtype == typeof (Char)) {
						if (type == typeof (double))
							return (double)(char)value;
						if (type == typeof (float))
							return (float)(char)value;
					}
					if (vtype == typeof (IntPtr) && type.IsPointer)
						return value;
					return Convert.ChangeType (value, type);
				}
				return null;
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
				return SelectMethod (bindingAttr, match, types, modifiers,
					false);
			}

			MethodBase SelectMethod (BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers, bool allowByRefMatch)
			{
				MethodBase m;
				int i, j;

				if (match == null)
					throw new ArgumentNullException ("match");

				/* first look for an exact match... */
				MethodBase exact_match = null;
				for (i = 0; i < match.Length; ++i) {
					m = match [i];
					ParameterInfo[] args = m.GetParameters ();
					if (args.Length != types.Length)
						continue;
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
				bool isdefParamArray = false;
				Type elementType = null;
				for (i = 0; i < match.Length; ++i) {
					m = match [i];
					ParameterInfo[] args = m.GetParameters ();
					if (args.Length > types.Length + 1)
						continue;
					else if (args.Length == 0)
						continue;
					isdefParamArray = Attribute.IsDefined (args [args.Length - 1], typeof (ParamArrayAttribute));
					if (!isdefParamArray)
						continue;
					elementType = args [args.Length - 1].ParameterType.GetElementType ();
					for (j = 0; j < types.Length; ++j) {
						if (j < (args.Length - 1) && types [j] != args [j].ParameterType)
							break;
						else if (j >= (args.Length - 1) && types [j] != elementType) 
							break;
					}
					if (j == types.Length)
						return m;
				}

				if ((int)(bindingAttr & BindingFlags.ExactBinding) != 0)
					return null;

				MethodBase result = null;
				for (i = 0; i < match.Length; ++i) {
					m = match [i];
					ParameterInfo[] args = m.GetParameters ();
					if (args.Length != types.Length)
						continue;
					if (!check_arguments (types, args, allowByRefMatch))
						continue;

					if (result != null)
						result = GetBetterMethod (result, m, types);
					else
						result = m;
				}

				return result;
			}

			MethodBase GetBetterMethod (MethodBase m1, MethodBase m2, Type [] types)
			{
				ParameterInfo [] pl1 = m1.GetParameters ();
				ParameterInfo [] pl2 = m2.GetParameters ();
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

