// System.Reflection.Binder
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
//
// (C) Ximian, Inc. 2001 - 2002

using System.Globalization;

namespace System.Reflection
{
	[Serializable]
	public abstract class Binder
	{
		protected Binder () {}

		public abstract FieldInfo BindToField (BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture);
		public abstract MethodBase BindToMethod (BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state);
		public abstract object ChangeType (object value, Type type, CultureInfo culture);
		public abstract void ReorderArgumentArray( ref object[] args, object state);
		public abstract MethodBase SelectMethod (BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers);
		public abstract PropertyInfo SelectProperty( BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers);

		static Binder default_binder;

		internal static Binder DefaultBinder {
			get {
				lock (typeof (Binder)) {
					if (default_binder == null)
						default_binder = new Default ();
					return default_binder;
				}
			}
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

			[MonoTODO]
			public override MethodBase BindToMethod (BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state)
			{
				throw new NotImplementedException ();
			}

			public override object ChangeType (object value, Type type, CultureInfo culture)
			{
				if (check_type (value.GetType (), type))
					return Convert.ChangeType (value, type);
				return null;
			}

			[MonoTODO]
			public override void ReorderArgumentArray (ref object[] args, object state)
			{
				throw new NotImplementedException ();
			}

			private static bool check_type (Type from, Type to) {
				if (from == to)
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
					return false;
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
					return false;
				case TypeCode.SByte:
					switch (tot) {
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return false;
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
					return false;
				case TypeCode.Int16:
					switch (tot) {
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return false;
				case TypeCode.UInt32:
					switch (tot) {
					case TypeCode.UInt64:
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return false;
				case TypeCode.Int32:
					switch (tot) {
					case TypeCode.Int64:
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return false;
				case TypeCode.UInt64:
				case TypeCode.Int64:
					switch (tot) {
					case TypeCode.Single:
					case TypeCode.Double:
						return true;
					}
					return false;
				case TypeCode.Single:
					return tot == TypeCode.Double;
				default:
					/* TODO: handle valuetype -> byref */
					return to.IsAssignableFrom (from);
				}
			}

			private static bool check_arguments (Type[] types, ParameterInfo[] args) {
				for (int i = 0; i < types.Length; ++i) {
					if (!check_type (types [i], args [i].ParameterType))
						return false;
				}
				return true;
			}

			public override MethodBase SelectMethod (BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
			{
				if (match == null)
					throw new ArgumentNullException ("match");
				foreach (MethodBase m in match) {
					//Console.WriteLine ("Considering method: {0}", m);
					ParameterInfo[] args = m.GetParameters ();
					if (args.Length != types.Length)
						continue;
					if (!check_arguments (types, args))
						continue;
					return m;
				}
				return null;
			}

			public override PropertyInfo SelectProperty (BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
			{
				if (match == null)
					throw new ArgumentNullException ("match");
				foreach (PropertyInfo m in match) {
					ParameterInfo[] args = m.GetIndexParameters ();
					if (args.Length != indexes.Length)
						continue;
					if (!check_arguments (indexes, args))
						continue;
					return m;
				}
				return null;
			}
		}
	}
}
