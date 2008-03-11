//
// support.cs: Support routines to work around the fact that System.Reflection.Emit
// can not introspect types that are being constructed
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;
using System.Globalization;

namespace Mono.CSharp {

	public interface ParameterData {
		Type ParameterType (int pos);
		Type [] Types { get; }
		int  Count { get; }
		Type ExtensionMethodType { get; }
		bool HasParams { get; }
		string ParameterName (int pos);
		string ParameterDesc (int pos);

		Parameter.Modifier ParameterModifier (int pos);
		string GetSignatureForError ();

#if MS_COMPATIBLE
		ParameterData InflateTypes (Type[] genArguments, Type[] argTypes);
#endif
	}

	public class ReflectionParameters : ParameterData {
		ParameterInfo [] pi;
		Type [] types;
		int params_idx = int.MaxValue;
		bool is_varargs;
		bool is_extension;
		ParameterData gpd;

		public ReflectionParameters (MethodBase mb)
		{
			ParameterInfo [] pi = mb.GetParameters ();
			is_varargs = (mb.CallingConvention & CallingConventions.VarArgs) != 0;

			this.pi = pi;
			int count = pi.Length;

			if (count == 0) {
				types = Type.EmptyTypes;
				return;
			}

			types = new Type [count];
			for (int i = 0; i < count; i++)
				types [i] = TypeManager.TypeToCoreType (pi [i].ParameterType);

			// TODO: This (if) should be done one level higher to correctly use
			// out caching facilities.
			MethodBase generic = TypeManager.DropGenericMethodArguments (mb);
			if (generic != mb) {
				gpd = TypeManager.GetParameterData (generic);
				if (gpd.HasParams) {
					for (int i = gpd.Count; i != 0; --i) {
						if ((gpd.ParameterModifier (i-1) & Parameter.Modifier.PARAMS) != 0) {
							this.params_idx = i-1;
							break;
						}
					}
				}
				return;
			}

			//
			// So far, the params attribute can be used in C# for the last
			// and next to last method parameters.
			// If some other language can place it anywhere we will
			// have to analyze all parameters and not just last 2.
			//
			--count;
			for (int i = count; i >= 0 && i > count - 2; --i) {
				if (!pi [i].ParameterType.IsArray)
					continue;

				if (pi [i].IsDefined (TypeManager.param_array_type, false)) {
					params_idx = i;
					break;
				}
			}

			if (TypeManager.extension_attribute_type != null && mb.IsStatic &&
				(mb.DeclaringType.Attributes & Class.StaticClassAttribute) == Class.StaticClassAttribute &&
				mb.IsDefined (TypeManager.extension_attribute_type, false))
				is_extension = true;
		}

		public override bool Equals (object obj)
		{
			ReflectionParameters rp = obj as ReflectionParameters;
			if (rp == null)
				return false;

			if (Count != rp.Count)
				return false;

			for (int i = 0; i < Count; ++i) {
			if (!types [i].Equals (rp.types [i]))
				return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public string GetSignatureForError ()
		{
			StringBuilder sb = new StringBuilder ("(");
			for (int i = 0; i < pi.Length; ++i) {
				if (i != 0)
					sb.Append (", ");
				sb.Append (ParameterDesc (i));
			}
			if (is_varargs) {
				if (pi.Length > 0)
					sb.Append (", ");
				sb.Append ("__arglist");
			}
			sb.Append (')');
			return sb.ToString ();
		}

#if MS_COMPATIBLE
		public ParameterData InflateTypes (Type[] genArguments, Type[] argTypes)
		{
			ReflectionParameters p = (ReflectionParameters)MemberwiseClone ();

			for (int i = 0; i < types.Length; ++i) {
				if (types[i].IsGenericParameter) {
					for (int ii = 0; ii < genArguments.Length; ++ii) {
						if (types[i] != genArguments[ii])
							continue;

						p.types[i] = argTypes[ii];
						break;
					}
					continue;
				}
				
				if (types[i].IsGenericType) {
					Type[] gen_arguments_open = types[i].GetGenericTypeDefinition ().GetGenericArguments ();
					Type[] gen_arguments = types[i].GetGenericArguments ();
					for (int ii = 0; ii < gen_arguments_open.Length; ++ii) {
						if (gen_arguments [ii].IsGenericParameter) {
							for (int iii = 0; iii < genArguments.Length; ++iii) {
								if (gen_arguments [ii] != genArguments [iii])
									continue;

								gen_arguments_open [ii] = argTypes [iii];
								break;
							}
						} else {
							gen_arguments_open [ii] = gen_arguments [ii];
						}
					}

					p.types[i] = types[i].GetGenericTypeDefinition ().MakeGenericType (gen_arguments_open);
				}
			}
			return p;
		}
#endif

		public Type ParameterType (int pos)
		{
			if (is_varargs && pos >= pi.Length)
				return TypeManager.runtime_argument_handle_type;

			return types [pos];
		}

		public string ParameterName (int pos)
		{
			if (gpd != null)
				return gpd.ParameterName (pos);

			if (is_varargs && pos >= pi.Length)
				return "__arglist";

			return pi [pos].Name;
		}

		public string ParameterDesc (int pos)
		{
			if (is_varargs && pos >= pi.Length)
				return "";

			StringBuilder sb = new StringBuilder ();

			if (pi [pos].IsIn)
				sb.Append ("in ");

			Type partype = ParameterType (pos);
			if (partype.IsByRef){
				partype = TypeManager.GetElementType (partype);
				if (pi [pos].IsOut)
					sb.Append ("out ");
				else
					sb.Append ("ref ");
			}

			if (params_idx == pos)
				sb.Append ("params ");

			if (pos == 0 && ExtensionMethodType != null)
				sb.Append ("this ");

			sb.Append (TypeManager.CSharpName (partype).Replace ("&", ""));

			return sb.ToString ();
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			if (pos >= params_idx)
				return Parameter.Modifier.PARAMS;
			if (is_varargs && pos >= pi.Length)
				return Parameter.Modifier.ARGLIST;

			if (gpd != null)
				return gpd.ParameterModifier (pos);

			Type t = types [pos];
			if (t.IsByRef){
				if ((pi [pos].Attributes & (ParameterAttributes.Out|ParameterAttributes.In)) == ParameterAttributes.Out)
					return Parameter.Modifier.OUT;
				else
					return Parameter.Modifier.REF;
			}

			return Parameter.Modifier.NONE;
		}

		public int Count {
			get { return is_varargs ? pi.Length + 1 : pi.Length; }
		}

		public Type ExtensionMethodType {
			get {
				if (!is_extension)
					return null;

				return types [0];
			}
		}

		public bool HasParams {
			get { return params_idx != int.MaxValue; }
		}

		public Type[] Types {
			get { return types; }
		}
	}

#if GMCS_SOURCE
	public class ReflectionConstraints : GenericConstraints
	{
		GenericParameterAttributes attrs;
		Type base_type;
		Type class_constraint;
		Type[] iface_constraints;
		string name;

		public static GenericConstraints GetConstraints (Type t)
		{
			Type [] constraints = t.GetGenericParameterConstraints ();
			GenericParameterAttributes attrs = t.GenericParameterAttributes;
			if (constraints.Length == 0 && attrs == GenericParameterAttributes.None)
				return null;
			return new ReflectionConstraints (t.Name, constraints, attrs);
		}

		private ReflectionConstraints (string name, Type [] constraints, GenericParameterAttributes attrs)
		{
			this.name = name;
			this.attrs = attrs;

			if ((constraints.Length > 0) && !constraints [0].IsInterface) {
				class_constraint = constraints [0];
				iface_constraints = new Type [constraints.Length - 1];
				Array.Copy (constraints, 1, iface_constraints, 0, constraints.Length - 1);
			} else
				iface_constraints = constraints;

			if (HasValueTypeConstraint)
				base_type = TypeManager.value_type;
			else if (class_constraint != null)
				base_type = class_constraint;
			else
				base_type = TypeManager.object_type;
		}

		public override string TypeParameter {
			get { return name; }
		}

		public override GenericParameterAttributes Attributes {
			get { return attrs; }
		}

		public override Type ClassConstraint {
			get { return class_constraint; }
		}

		public override Type EffectiveBaseClass {
			get { return base_type; }
		}

		public override Type[] InterfaceConstraints {
			get { return iface_constraints; }
		}
	}
#endif

	class PtrHashtable : Hashtable {
		sealed class PtrComparer : IComparer {
			private PtrComparer () {}

			public static PtrComparer Instance = new PtrComparer ();

			public int Compare (object x, object y)
			{
				if (x == y)
					return 0;
				else
					return 1;
			}
		}

		public PtrHashtable ()
		{
			comparer = PtrComparer.Instance;
		}
	}

	/*
	 * Hashtable whose keys are character arrays with the same length
	 */
	class CharArrayHashtable : Hashtable {
		sealed class ArrComparer : IComparer {
			private int len;

			public ArrComparer (int len) {
				this.len = len;
			}

			public int Compare (object x, object y)
			{
				char[] a = (char[])x;
				char[] b = (char[])y;

				for (int i = 0; i < len; ++i)
					if (a [i] != b [i])
						return 1;
				return 0;
			}
		}

		private int len;

		protected override int GetHash (Object key)
		{
			char[] arr = (char[])key;
			int h = 0;

			for (int i = 0; i < len; ++i)
				h = (h << 5) - h + arr [i];

			return h;
		}

		public CharArrayHashtable (int len)
		{
			this.len = len;
			comparer = new ArrComparer (len);
		}
	}

	struct Pair {
		public object First;
		public object Second;

		public Pair (object f, object s)
		{
			First = f;
			Second = s;
		}
	}

	public class Accessors {
		public Accessor get_or_add;
		public Accessor set_or_remove;

		// was 'set' declared before 'get'?  was 'remove' declared before 'add'?
		public bool declared_in_reverse;

		public Accessors (Accessor get_or_add, Accessor set_or_remove)
		{
			this.get_or_add = get_or_add;
			this.set_or_remove = set_or_remove;
		}
	}

	/// <summary>
	///   This is a wrapper around StreamReader which is seekable backwards
	///   within a window of around 2048 chars.
	/// </summary>
	public class SeekableStreamReader
	{
		const int AverageReadLength = 1024;
		TextReader reader;
		Stream stream;
		Encoding encoding;

		char[] buffer;
		int buffer_start;       // in chars
		int char_count;         // count buffer[] valid characters
		int pos;                // index into buffer[]

		public SeekableStreamReader (Stream stream, Encoding encoding)
		{
			this.stream = stream;
			this.encoding = encoding;
			
			this.reader = new StreamReader (stream, encoding, true);
			this.buffer = new char [AverageReadLength * 3];

			// Let the StreamWriter autodetect the encoder
			reader.Peek ();
		}

		/// <remarks>
		///   This value corresponds to the current position in a stream of characters.
		///   The StreamReader hides its manipulation of the underlying byte stream and all
		///   character set/decoding issues.  Thus, we cannot use this position to guess at
		///   the corresponding position in the underlying byte stream even though there is
		///   a correlation between them.
		/// </remarks>
		public int Position {
			get { return buffer_start + pos; }

			set {
				if (value > buffer_start + char_count)
					throw new InternalErrorException ("can't seek that far forward: " + (pos - value));
				
				if (value < buffer_start){
					// Reinitialize.
					stream.Position = 0;
					reader = new StreamReader (stream, encoding, true);
					buffer_start = 0;
					char_count = 0;
					pos = 0;
					Peek ();

					while (value > buffer_start + char_count){
						pos = char_count+1;
						Peek ();
					}
					pos = value - buffer_start;
				}

				pos = value - buffer_start;
			}
		}

		private bool ReadBuffer ()
		{
			int slack = buffer.Length - char_count;
			if (slack <= AverageReadLength / 2) {
				// shift the buffer to make room for AverageReadLength number of characters
				int shift = AverageReadLength - slack;
				Array.Copy (buffer, shift, buffer, 0, char_count - shift);
				pos -= shift;
				char_count -= shift;
				buffer_start += shift;
				slack += shift;		// slack == AverageReadLength
			}

			int chars_read = reader.Read (buffer, char_count, slack);
			char_count += chars_read;

			return pos < char_count;
		}

		public int Peek ()
		{
			if ((pos >= char_count) && !ReadBuffer ())
				return -1;

			return buffer [pos];
		}

		public int Read ()
		{
			if ((pos >= char_count) && !ReadBuffer ())
				return -1;

			return buffer [pos++];
		}
	}

	public class DoubleHash {
		const int DEFAULT_INITIAL_BUCKETS = 100;

		public DoubleHash () : this (DEFAULT_INITIAL_BUCKETS) {}

		public DoubleHash (int size)
		{
			count = size;
			buckets = new Entry [size];
		}

		int count;
		Entry [] buckets;
		int size = 0;

		class Entry {
			public object key1;
			public object key2;
			public int hash;
			public object value;
			public Entry next;

			public Entry (object key1, object key2, int hash, object value, Entry next)
			{
				this.key1 = key1;
				this.key2 = key2;
				this.hash = hash;
				this.next = next;
				this.value = value;
			}
		}

		public bool Lookup (object a, object b, out object res)
		{
			int h = (a.GetHashCode () ^ b.GetHashCode ()) & 0x7FFFFFFF;

			for (Entry e = buckets [h % count]; e != null; e = e.next) {
				if (e.hash == h && e.key1.Equals (a) && e.key2.Equals (b)) {
					res = e.value;
					return true;
				}
			}
			res = null;
			return false;
		}

		public void Insert (object a, object b, object value)
		{
			// Is it an existing one?

			int h = (a.GetHashCode () ^ b.GetHashCode ()) & 0x7FFFFFFF;

			for (Entry e = buckets [h % count]; e != null; e = e.next) {
				if (e.hash == h && e.key1.Equals (a) && e.key2.Equals (b))
					e.value = value;
			}

			int bucket = h % count;
			buckets [bucket] = new Entry (a, b, h, value, buckets [bucket]);

			// Grow whenever we double in size
			if (size++ == count) {
				count <<= 1;
				count ++;

				Entry [] newBuckets = new Entry [count];
				foreach (Entry root in buckets) {
					Entry e = root;
					while (e != null) {
						int newLoc = e.hash % count;
						Entry n = e.next;
						e.next = newBuckets [newLoc];
						newBuckets [newLoc] = e;
						e = n;
					}
				}

				buckets = newBuckets;
			}
		}
	}

	class PartialMethodDefinitionInfo : MethodInfo
	{
		MethodOrOperator mc;
		MethodAttributes attrs;

		public PartialMethodDefinitionInfo (MethodOrOperator mc)
		{
			this.mc = mc;
			if ((mc.ModFlags & Modifiers.STATIC) != 0)
				attrs = MethodAttributes.Static;
		}

		public override MethodInfo GetBaseDefinition ()
		{
			throw new NotImplementedException ();
		}

		public override ICustomAttributeProvider ReturnTypeCustomAttributes
		{
			get { throw new NotImplementedException (); }
		}

		public override MethodAttributes Attributes
		{
			get { return attrs; }
		}

		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			throw new NotImplementedException ();
		}

		public override ParameterInfo [] GetParameters ()
		{
			throw new NotImplementedException ();
		}

		public override object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object [] parameters, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}

		public override RuntimeMethodHandle MethodHandle
		{
			get { throw new NotImplementedException (); }
		}

		public override Type DeclaringType
		{
			get { return mc.Parent.TypeBuilder; }
		}

		public override object [] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotImplementedException ();
		}

		public override object [] GetCustomAttributes (bool inherit)
		{
			throw new NotImplementedException ();
		}

		public override Type ReturnType {
			get {
				return mc.MemberType;
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotImplementedException ();
		}

		public override string Name
		{
			get { return mc.Name; }
		}

		public override Type ReflectedType
		{
			get { throw new NotImplementedException (); }
		}
	}

}
