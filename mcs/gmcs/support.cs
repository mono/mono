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
		bool HasParams { get; }
		string ParameterName (int pos);
		string ParameterDesc (int pos);
		Parameter.Modifier ParameterModifier (int pos);
		string GetSignatureForError ();
	}

	public class ReflectionParameters : ParameterData {
		ParameterInfo [] pi;
		Type [] types;
		bool last_arg_is_params = false;
		bool is_varargs = false;
		ParameterData gpd;

		public ReflectionParameters (MethodBase mb)
		{
			object [] attrs;

			ParameterInfo [] pi = mb.GetParameters ();
			is_varargs = (mb.CallingConvention & CallingConventions.VarArgs) != 0;

			this.pi = pi;
			int count = pi.Length-1;

			if (pi.Length == 0) {
				types = TypeManager.NoTypes;
			} else {
				types = new Type [pi.Length];
				for (int i = 0; i < pi.Length; i++)
					types [i] = pi [i].ParameterType;
			}

			if (count < 0)
				return;

			if (mb.Mono_IsInflatedMethod) {
				MethodInfo generic = mb.GetGenericMethodDefinition ();
				gpd = TypeManager.GetParameterData (generic);

				last_arg_is_params = gpd.HasParams;
				return;
			}

			attrs = pi [count].GetCustomAttributes (TypeManager.param_array_type, true);
			if (attrs == null)
				return;

			if (attrs.Length == 0)
				return;

			last_arg_is_params = true;
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

		public Type ParameterType (int pos)
		{
			if (last_arg_is_params && pos >= pi.Length - 1)
				return pi [pi.Length - 1].ParameterType;
			else if (is_varargs && pos >= pi.Length)
				return TypeManager.runtime_argument_handle_type;
			else {
				Type t = pi [pos].ParameterType;

				return t;
			}
		}

		public string ParameterName (int pos)
		{
			if (gpd != null)
				return gpd.ParameterName (pos);

			if (last_arg_is_params && pos >= pi.Length - 1)
				return pi [pi.Length - 1].Name;
			else if (is_varargs && pos >= pi.Length)
				return "__arglist";
			else 
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

			if (pos >= pi.Length - 1 && last_arg_is_params)
				sb.Append ("params ");

			sb.Append (TypeManager.CSharpName (partype).Replace ("&", ""));

			return sb.ToString ();
			
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			if (last_arg_is_params && pos >= pi.Length - 1)
					return Parameter.Modifier.PARAMS;
			else if (is_varargs && pos >= pi.Length)
				return Parameter.Modifier.ARGLIST;
			
			if (gpd != null)
				return gpd.ParameterModifier (pos);

			Type t = pi [pos].ParameterType;
			if (t.IsByRef){
				if ((pi [pos].Attributes & (ParameterAttributes.Out|ParameterAttributes.In)) == ParameterAttributes.Out)
					return Parameter.Modifier.OUT;
				else
					return Parameter.Modifier.REF;
			}
			
			return Parameter.Modifier.NONE;
		}

		public int Count {
			get {
				return is_varargs ? pi.Length + 1 : pi.Length;
			}
		}

		public bool HasParams {
			get {
				return this.last_arg_is_params;
			}
		}

		public Type[] Types {
			get {
				return types;
			}
		}		
	}

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

	/// <summary>
	///   This is a wrapper around StreamReader which is seekable backwards
	///   within a window of around 2048 chars.
	/// </summary>
	public class SeekableStreamReader
	{
		public SeekableStreamReader (StreamReader reader)
		{
			this.reader = reader;
			this.buffer = new char [AverageReadLength * 3];

			// Let the StreamWriter autodetect the encoder
			reader.Peek ();
		}

		public SeekableStreamReader (Stream stream, Encoding encoding)
			: this (new StreamReader (stream, encoding, true))
		{ }

		StreamReader reader;

		private const int AverageReadLength = 1024;

		char[] buffer;
		int buffer_start;       // in chars
		int char_count;         // count buffer[] valid characters
		int pos;                // index into buffer[]

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
				if (value < buffer_start || value > buffer_start + char_count)
					throw new InternalErrorException ("can't seek that far back: " + (pos - value));
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
}
