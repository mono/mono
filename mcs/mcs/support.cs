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
		int  Count { get; }
		bool HasParams { get; }
		string ParameterName (int pos);
		string ParameterDesc (int pos);
		Parameter.Modifier ParameterModifier (int pos);
	}

	public class ReflectionParameters : ParameterData {
		ParameterInfo [] pi;
		bool last_arg_is_params = false;
		bool is_varargs = false;
		
		public ReflectionParameters (MethodBase mb)
		{
			object [] attrs;

			ParameterInfo [] pi = mb.GetParameters ();
			is_varargs = (mb.CallingConvention & CallingConventions.VarArgs) != 0;
			
			this.pi = pi;
			int count = pi.Length-1;

			if (count >= 0) {
				attrs = pi [count].GetCustomAttributes (TypeManager.param_array_type, true);

				if (attrs == null)
					return;
				
				if (attrs.Length == 0)
					return;

				last_arg_is_params = true;
			}
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
			
			sb.Append (TypeManager.CSharpName (partype));

			return sb.ToString ();
			
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			if (last_arg_is_params && pos >= pi.Length - 1)
				return Parameter.Modifier.PARAMS;
			else if (is_varargs && pos >= pi.Length)
				return Parameter.Modifier.ARGLIST;
			
			Type t = pi [pos].ParameterType;
			if (t.IsByRef){
				if ((pi [pos].Attributes & ParameterAttributes.Out) != 0)
					return Parameter.Modifier.ISBYREF | Parameter.Modifier.OUT;
				else
					return Parameter.Modifier.ISBYREF | Parameter.Modifier.REF;
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
		
	}

	public class InternalParameters : ParameterData {
		Type [] param_types;
		bool has_varargs;
		int count;

		public readonly Parameters Parameters;
		
		public InternalParameters (Type [] param_types, Parameters parameters)
		{
			this.param_types = param_types;
			this.Parameters = parameters;

			has_varargs = parameters.HasArglist;

			if (param_types == null)
				count = 0;
			else
				count = param_types.Length;
		}

		public int Count {
			get {
				return has_varargs ? count + 1 : count;
			}
		}

		public bool HasParams {
			get {
				return Parameters.ArrayParameter != null;
			}
		}

		Parameter GetParameter (int pos)
		{
			Parameter [] fixed_pars = Parameters.FixedParameters;
			if (fixed_pars != null){
				int len = fixed_pars.Length;
				if (pos < len)
					return Parameters.FixedParameters [pos];
			}

			return Parameters.ArrayParameter;
		}

		public Type ParameterType (int pos)
		{
			if (has_varargs && pos >= count)
				return TypeManager.runtime_argument_handle_type;

			if (param_types == null)
				return null;

			return GetParameter (pos).ExternalType ();
		}


		public string ParameterName (int pos)
		{
			if (has_varargs && pos >= count)
				return "__arglist";

			return GetParameter (pos).Name;
		}

		public string ParameterDesc (int pos)
		{
			if (has_varargs && pos >= count)
				return "__arglist";

			string tmp = String.Empty;
			Parameter p = GetParameter (pos);

			//
			// We need to and for REF/OUT, because if either is set the
			// extra flag ISBYREF will be set as well
			//
			if ((p.ModFlags & Parameter.Modifier.REF) != 0)
				tmp = "ref ";
			else if ((p.ModFlags & Parameter.Modifier.OUT) != 0)
				tmp = "out ";
			else if (p.ModFlags == Parameter.Modifier.PARAMS)
				tmp = "params ";

			Type t = ParameterType (pos);

			return tmp + TypeManager.CSharpName (t);
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			if (has_varargs && pos >= count)
				return Parameter.Modifier.ARGLIST;

			Parameter.Modifier mod = GetParameter (pos).ModFlags;

			if ((mod & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) != 0)
				mod |= Parameter.Modifier.ISBYREF;

			return mod;
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
	///   This is a wrapper around StreamReader which is seekable.
	/// </summary>
	public class SeekableStreamReader
	{
		public SeekableStreamReader (StreamReader reader)
		{
			this.reader = reader;
			this.buffer = new char [DefaultCacheSize];
			
			// Compute the preamble size
			
			// Let the StreamWriter autodetect the encoder
			reader.Peek ();
			
			reader.BaseStream.Position = 0;
			Encoding enc = reader.CurrentEncoding;
			// First of all, get at least a char
			
			byte[] auxb = new byte [50];
			int num_bytes = 0;
			int num_chars = 0;
			int br = 0;
			do {
				br = reader.BaseStream.Read (auxb, num_bytes, auxb.Length - num_bytes);
				num_bytes += br;
				num_chars = enc.GetCharCount (auxb, 0, num_bytes);
			}
			while (num_chars == 0 && br > 0);
			
			if (num_chars != 0)
			{
				// Now, check which bytes at the beginning have no effect in the
				// char count
				
				int p = 0;
				while (enc.GetCharCount (auxb, p, num_bytes-p) >= num_chars)
					p++;
				
				preamble_size = p - 1;
				reader.BaseStream.Position = 0;
				reader.DiscardBufferedData ();
				
				buffer_start = preamble_size;
			}
		}

		public SeekableStreamReader (Stream stream, Encoding encoding, bool detect_encoding_from_bytemarks)
			: this (new StreamReader (stream, encoding, detect_encoding_from_bytemarks))
		{ }

		StreamReader reader;

		private const int DefaultCacheSize = 1024;

		char[] buffer;
		int buffer_start;       // in bytes
		int buffer_size;        // in bytes
		int char_count;         // count buffer[] valid characters
		int pos;                // index into buffer[]
		int preamble_size;

		/// <remarks>
		///   The difference to the StreamReader's BaseStream.Position is that this one is reliable; ie. it
		//    always reports the correct position and if it's modified, it also takes care of the buffered data.
		/// </remarks>
		public int Position {
			get {
				return buffer_start + reader.CurrentEncoding.GetByteCount (buffer, 0, pos);
			}

			set {
				// This one is easy: we're modifying the position within our current
				// buffer.
				if ((value >= buffer_start) && (value < buffer_start + buffer_size)) {
					int byte_offset = value - buffer_start;
					pos = byte_offset;
					// encoded characters can take more than 1 byte length
					while (reader.CurrentEncoding.GetByteCount (buffer, 0, pos) > byte_offset)
						pos--;
					
					return;
				}
				
				if (value == 0)	// Skip preamble
					value = preamble_size;

				// Ok, now we need to seek.
				reader.DiscardBufferedData ();
				reader.BaseStream.Position = buffer_start = value;
				char_count = buffer_size = pos = 0;
			}
		}

		private bool ReadBuffer ()
		{
			pos = 0;
			buffer_start += buffer_size;
			char_count = reader.Read (buffer, 0, buffer.Length);
			buffer_size = reader.CurrentEncoding.GetByteCount (buffer, 0, char_count);
			return buffer_size > 0;
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
