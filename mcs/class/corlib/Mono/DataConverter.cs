//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//
// See the following url for documentation:
//     http://www.mono-project.com/Mono_DataConvert
//
// Compilation Options:
//     MONO_DATACONVERTER_PUBLIC:
//         Makes the class public instead of the default internal.
//
//     MONO_DATACONVERTER_STATIC_METHODS:     
//         Exposes the public static methods.
//
// TODO:
//   Support for "DoubleWordsAreSwapped" for ARM devices
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System;
using System.Collections;
using System.Text;

#pragma warning disable 3021

namespace Mono {

#if MONO_DATACONVERTER_PUBLIC
	unsafe public abstract class DataConverter {
#else
	unsafe internal abstract class DataConverter {

// Disables the warning: CLS compliance checking will not be performed on
//  `XXXX' because it is not visible from outside this assembly
#pragma warning disable  3019
#endif
		static DataConverter SwapConv = new SwapConverter ();
		static DataConverter CopyConv = new CopyConverter ();

		public static readonly bool IsLittleEndian = BitConverter.IsLittleEndian;
			
		public abstract double GetDouble (byte [] data, int index);
		public abstract float  GetFloat  (byte [] data, int index);
		public abstract long   GetInt64  (byte [] data, int index);
		public abstract int    GetInt32  (byte [] data, int index);

		public abstract short  GetInt16  (byte [] data, int index);

                [CLSCompliant (false)]
		public abstract uint   GetUInt32 (byte [] data, int index);
                [CLSCompliant (false)]
		public abstract ushort GetUInt16 (byte [] data, int index);
                [CLSCompliant (false)]
		public abstract ulong  GetUInt64 (byte [] data, int index);
		
		public abstract void PutBytes (byte [] dest, int destIdx, double value);
		public abstract void PutBytes (byte [] dest, int destIdx, float value);
		public abstract void PutBytes (byte [] dest, int destIdx, int value);
		public abstract void PutBytes (byte [] dest, int destIdx, long value);
		public abstract void PutBytes (byte [] dest, int destIdx, short value);

                [CLSCompliant (false)]
		public abstract void PutBytes (byte [] dest, int destIdx, ushort value);
                [CLSCompliant (false)]
		public abstract void PutBytes (byte [] dest, int destIdx, uint value);
                [CLSCompliant (false)]
		public abstract void PutBytes (byte [] dest, int destIdx, ulong value);

		public byte[] GetBytes (double value)
		{
			byte [] ret = new byte [8];
			PutBytes (ret, 0, value);
			return ret;
		}
		
		public byte[] GetBytes (float value)
		{
			byte [] ret = new byte [4];
			PutBytes (ret, 0, value);
			return ret;
		}
		
		public byte[] GetBytes (int value)
		{
			byte [] ret = new byte [4];
			PutBytes (ret, 0, value);
			return ret;
		}
		
		public byte[] GetBytes (long value)
		{
			byte [] ret = new byte [8];
			PutBytes (ret, 0, value);
			return ret;
		}
		
		public byte[] GetBytes (short value)
		{
			byte [] ret = new byte [2];
			PutBytes (ret, 0, value);
			return ret;
		}

                [CLSCompliant (false)]
		public byte[] GetBytes (ushort value)
		{
			byte [] ret = new byte [2];
			PutBytes (ret, 0, value);
			return ret;
		}
		
                [CLSCompliant (false)]
		public byte[] GetBytes (uint value)
		{
			byte [] ret = new byte [4];
			PutBytes (ret, 0, value);
			return ret;
		}
		
                [CLSCompliant (false)]
		public byte[] GetBytes (ulong value)
		{
			byte [] ret = new byte [8];
			PutBytes (ret, 0, value);
			return ret;
		}
		
		static public DataConverter LittleEndian {
			get {
				return BitConverter.IsLittleEndian ? CopyConv : SwapConv;
			}
		}

		static public DataConverter BigEndian {
			get {
				return BitConverter.IsLittleEndian ? SwapConv : CopyConv;
			}
		}

		static public DataConverter Native {
			get {
				return CopyConv;
			}
		}

		static int Align (int current, int align)
		{
			return ((current + align - 1) / align) * align;
		}
			
		class PackContext {
			// Buffer
			public byte [] buffer;
			int next;

			public string description;
			public int i; // position in the description
			public DataConverter conv;
			public int repeat;
			
			//
			// if align == -1, auto align to the size of the byte array
			// if align == 0, do not do alignment
			// Any other values aligns to that particular size
			//
			public int align;

			public void Add (byte [] group)
			{
				//Console.WriteLine ("Adding {0} bytes to {1} (next={2}", group.Length,
				// buffer == null ? "null" : buffer.Length.ToString (), next);
				
				if (buffer == null){
					buffer = group;
					next = group.Length;
					return;
				}
				if (align != 0){
					if (align == -1)
						next = Align (next, group.Length);
					else
						next = Align (next, align);
					align = 0;
				}

				if (next + group.Length > buffer.Length){
					byte [] nb = new byte [System.Math.Max (next, 16) * 2 + group.Length];
					Array.Copy (buffer, nb, buffer.Length);
					Array.Copy (group, 0, nb, next, group.Length);
					next = next + group.Length;
					buffer = nb;
				} else {
					Array.Copy (group, 0, buffer, next, group.Length);
					next += group.Length;
				}
			}

			public byte [] Get ()
			{
				if (buffer == null)
					return new byte [0];
				
				if (buffer.Length != next){
					byte [] b = new byte [next];
					Array.Copy (buffer, b, next);
					return b;
				}
				return buffer;
			}
		}

		//
		// Format includes:
		// Control:
		//   ^    Switch to big endian encoding
		//   _    Switch to little endian encoding
		//   %    Switch to host (native) encoding
		//   !    aligns the next data type to its natural boundary (for strings this is 4).
		//
		// Types:
		//   s    Int16
		//   S    UInt16
		//   i    Int32
		//   I    UInt32
		//   l    Int64
		//   L    UInt64
		//   f    float
		//   d    double
		//   b    byte
                //   c    1-byte signed character
                //   C    1-byte unsigned character
		//   z8   string encoded as UTF8 with 1-byte null terminator
		//   z6   string encoded as UTF16 with 2-byte null terminator
		//   z7   string encoded as UTF7 with 1-byte null terminator
		//   zb   string encoded as BigEndianUnicode with 2-byte null terminator
		//   z3   string encoded as UTF32 with 4-byte null terminator
		//   z4   string encoded as UTF32 big endian with 4-byte null terminator
		//   $8   string encoded as UTF8
		//   $6   string encoded as UTF16
		//   $7   string encoded as UTF7
		//   $b   string encoded as BigEndianUnicode
		//   $3   string encoded as UTF32
		//   $4   string encoded as UTF-32 big endian encoding
		//   x    null byte
		//
		// Repeats, these are prefixes:
		//   N    a number between 1 and 9, indicates a repeat count (process N items
		//        with the following datatype
		//   [N]  For numbers larger than 9, use brackets, for example [20]
		//   *    Repeat the next data type until the arguments are exhausted
		//
		static public byte [] Pack (string description, params object [] args)
		{
			int argn = 0;
			PackContext b = new PackContext ();
			b.conv = CopyConv;
			b.description = description;

			for (b.i = 0; b.i < description.Length; ){
				object oarg;

				if (argn < args.Length)
					oarg = args [argn];
				else {
					if (b.repeat != 0)
						break;
					
					oarg = null;
				}

				int save = b.i;
				
				if (PackOne (b, oarg)){
					argn++;
					if (b.repeat > 0){
						if (--b.repeat > 0)
							b.i = save;
						else
							b.i++;
					} else
						b.i++;
				} else
					b.i++;
			}
			return b.Get ();
		}

		static public byte [] PackEnumerable (string description, IEnumerable args)
		{
			PackContext b = new PackContext ();
			b.conv = CopyConv;
			b.description = description;
			
			IEnumerator enumerator = args.GetEnumerator ();
			bool ok = enumerator.MoveNext ();

			for (b.i = 0; b.i < description.Length; ){
				object oarg;

				if (ok)
					oarg = enumerator.Current;
				else {
					if (b.repeat != 0)
						break;
					oarg = null;
				}
						
				int save = b.i;
				
				if (PackOne (b, oarg)){
					ok = enumerator.MoveNext ();
					if (b.repeat > 0){
						if (--b.repeat > 0)
							b.i = save;
						else
							b.i++;
					} else
						b.i++;
				} else
					b.i++;
			}
			return b.Get ();
		}
			
		//
		// Packs one datum `oarg' into the buffer `b', using the string format
		// in `description' at position `i'
		//
		// Returns: true if we must pick the next object from the list
		//
		static bool PackOne (PackContext b, object oarg)
		{
			int n;
			
			switch (b.description [b.i]){
			case '^':
				b.conv = BigEndian;
				return false;
			case '_':
				b.conv = LittleEndian;
				return false;
			case '%':
				b.conv = Native;
				return false;

			case '!':
				b.align = -1;
				return false;
				
			case 'x':
				b.Add (new byte [] { 0 });
				return false;
				
				// Type Conversions
			case 'i':
				b.Add (b.conv.GetBytes (Convert.ToInt32 (oarg)));
				break;
				
			case 'I':
				b.Add (b.conv.GetBytes (Convert.ToUInt32 (oarg)));
				break;
				
			case 's':
				b.Add (b.conv.GetBytes (Convert.ToInt16 (oarg)));
				break;
				
			case 'S':
				b.Add (b.conv.GetBytes (Convert.ToUInt16 (oarg)));
				break;
				
			case 'l':
				b.Add (b.conv.GetBytes (Convert.ToInt64 (oarg)));
				break;
				
			case 'L':
				b.Add (b.conv.GetBytes (Convert.ToUInt64 (oarg)));
				break;
				
			case 'f':
				b.Add (b.conv.GetBytes (Convert.ToSingle (oarg)));
				break;
				
			case 'd':
				b.Add (b.conv.GetBytes (Convert.ToDouble (oarg)));
				break;
				
			case 'b':
				b.Add (new byte [] { Convert.ToByte (oarg) });
				break;

			case 'c':
				b.Add (new byte [] { (byte) (Convert.ToSByte (oarg)) });
				break;

			case 'C':
				b.Add (new byte [] { Convert.ToByte (oarg) });
				break;

				// Repeat acount;
			case '1': case '2': case '3': case '4': case '5':
			case '6': case '7': case '8': case '9':
				b.repeat = ((short) b.description [b.i]) - ((short) '0');
				return false;

			case '*':
				b.repeat = Int32.MaxValue;
				return false;
				
			case '[':
				int count = -1, j;
				
				for (j = b.i+1; j < b.description.Length; j++){
					if (b.description [j] == ']')
						break;
					n = ((short) b.description [j]) - ((short) '0');
					if (n >= 0 && n <= 9){
						if (count == -1)
							count = n;
						else
							count = count * 10 + n;
					}
				}
				if (count == -1)
					throw new ArgumentException ("invalid size specification");
				b.i = j;
				b.repeat = count;
				return false;
				
			case '$': case 'z':
				bool add_null = b.description [b.i] == 'z';
				b.i++;
				if (b.i >= b.description.Length)
					throw new ArgumentException ("$ description needs a type specified", "description");
				char d = b.description [b.i];
				Encoding e;
				
				switch (d){
				case '8':
					e = Encoding.UTF8;
					n = 1;
					break;
				case '6':
					e = Encoding.Unicode;
					n = 2;
					break;
				case '7':
					e = Encoding.UTF7;
					n = 1;
					break;
				case 'b':
					e = Encoding.BigEndianUnicode;
					n = 2;
					break;
				case '3':
					e = Encoding.GetEncoding (12000);
					n = 4;
					break;
				case '4':
					e = Encoding.GetEncoding (12001);
					n = 4;
					break;
					
				default:
					throw new ArgumentException ("Invalid format for $ specifier", "description");
				}
				if (b.align == -1)
					b.align = 4;
				b.Add (e.GetBytes (Convert.ToString (oarg)));
				if (add_null)
					b.Add (new byte [n]);
				break;
			default:
				throw new ArgumentException (String.Format ("invalid format specified `{0}'",
									    b.description [b.i]));
			}
			return true;
		}

		static bool Prepare (byte [] buffer, ref int idx, int size, ref bool align)
		{
			if (align){
				idx = Align (idx, size);
				align = false;
			}
			if (idx + size > buffer.Length){
				idx = buffer.Length;
				return false;
			}
			return true;
		}
		
		static public IList Unpack (string description, byte [] buffer, int startIndex)
		{
			DataConverter conv = CopyConv;
			ArrayList result = new ArrayList ();
			int idx = startIndex;
			bool align = false;
			int repeat = 0, n;
			
			for (int i = 0; i < description.Length && idx < buffer.Length; ){
				int save = i;
				
				switch (description [i]){
				case '^':
					conv = BigEndian;
					break;
				case '_':
					conv = LittleEndian;
					break;
				case '%':
					conv = Native;
					break;
				case 'x':
					idx++;
					break;

				case '!':
					align = true;
					break;

					// Type Conversions
				case 'i':
					if (Prepare (buffer, ref idx, 4, ref align)){
						result.Add (conv.GetInt32 (buffer, idx));
						idx += 4;
					} 
					break;
				
				case 'I':
					if (Prepare (buffer, ref idx, 4, ref align)){
						result.Add (conv.GetUInt32 (buffer, idx));
						idx += 4;
					}
					break;
				
				case 's':
					if (Prepare (buffer, ref idx, 2, ref align)){
						result.Add (conv.GetInt16 (buffer, idx));
						idx += 2;
					}
					break;
				
				case 'S':
					if (Prepare (buffer, ref idx, 2, ref align)){
						result.Add (conv.GetUInt16 (buffer, idx));
						idx += 2;
					}
					break;
				
				case 'l':
					if (Prepare (buffer, ref idx, 8, ref align)){
						result.Add (conv.GetInt64 (buffer, idx));
						idx += 8;
					}
					break;
				
				case 'L':
					if (Prepare (buffer, ref idx, 8, ref align)){
						result.Add (conv.GetUInt64 (buffer, idx));
						idx += 8;
					}
					break;
				
				case 'f':
					if (Prepare (buffer, ref idx, 4, ref align)){
						result.Add (conv.GetDouble (buffer, idx));
						idx += 4;
					}
					break;
				
				case 'd':
					if (Prepare (buffer, ref idx, 8, ref align)){
						result.Add (conv.GetDouble (buffer, idx));
						idx += 8;
					}
					break;
				
				case 'b':
					if (Prepare (buffer, ref idx, 1, ref align)){
						result.Add (buffer [idx]);
						idx++;
					}
					break;

				case 'c': case 'C':
					if (Prepare (buffer, ref idx, 1, ref align)){
						char c;
						
						if (description [i] == 'c')
							c = ((char) ((sbyte)buffer [idx]));
						else
							c = ((char) ((byte)buffer [idx]));
						
						result.Add (c);
						idx++;
					}
					break;
					
					// Repeat acount;
				case '1': case '2': case '3': case '4': case '5':
				case '6': case '7': case '8': case '9':
					repeat = ((short) description [i]) - ((short) '0');
					save = i + 1;
					break;

				case '*':
					repeat = Int32.MaxValue;
					break;
				
				case '[':
					int count = -1, j;
				
					for (j = i+1; j < description.Length; j++){
						if (description [j] == ']')
							break;
						n = ((short) description [j]) - ((short) '0');
						if (n >= 0 && n <= 9){
							if (count == -1)
								count = n;
							else
								count = count * 10 + n;
						}
					}
					if (count == -1)
						throw new ArgumentException ("invalid size specification");
					i = j;
					save = i + 1;
					repeat = count;
					break;
				
				case '$': case 'z':
					// bool with_null = description [i] == 'z';
					i++;
					if (i >= description.Length)
						throw new ArgumentException ("$ description needs a type specified", "description");
					char d = description [i];
					Encoding e;
					if (align){
						idx = Align (idx, 4);
						align = false;
					}
					if (idx >= buffer.Length)
						break;
				
					switch (d){
					case '8':
						e = Encoding.UTF8;
						n = 1;
						break;
					case '6':
						e = Encoding.Unicode;
						n = 2;
						break;
					case '7':
						e = Encoding.UTF7;
						n = 1;
						break;
					case 'b':
						e = Encoding.BigEndianUnicode;
						n = 2;
						break;
					case '3':
						e = Encoding.GetEncoding (12000);
						n = 4;
						break;
					case '4':
						e = Encoding.GetEncoding (12001);
						n = 4;
						break;
					
					default:
						throw new ArgumentException ("Invalid format for $ specifier", "description");
					}
					int k = idx;
					switch (n){
					case 1:
						for (; k < buffer.Length && buffer [k] != 0; k++)
							;
						result.Add (e.GetChars (buffer, idx, k-idx));
						if (k == buffer.Length)
							idx = k;
						else
							idx = k+1;
						break;
						
					case 2:
						for (; k < buffer.Length; k++){
							if (k+1 == buffer.Length){
								k++;
								break;
							}
							if (buffer [k] == 0 && buffer [k+1] == 0)
								break;
						}
						result.Add (e.GetChars (buffer, idx, k-idx));
						if (k == buffer.Length)
							idx = k;
						else
							idx = k+2;
						break;
						
					case 4:
						for (; k < buffer.Length; k++){
							if (k+3 >= buffer.Length){
								k = buffer.Length;
								break;
							}
							if (buffer[k]==0 && buffer[k+1] == 0 && buffer[k+2] == 0 && buffer[k+3]== 0)
								break;
						}
						result.Add (e.GetChars (buffer, idx, k-idx));
						if (k == buffer.Length)
							idx = k;
						else
							idx = k+4;
						break;
					}
					break;
				default:
					throw new ArgumentException (String.Format ("invalid format specified `{0}'",
										    description [i]));
				}

				if (repeat > 0){
					if (--repeat > 0)
						i = save;
				} else
					i++;
			}
			return result;
		}

		internal void Check (byte [] dest, int destIdx, int size)
		{
			if (dest == null)
				throw new ArgumentNullException ("dest");
			if (destIdx < 0 || destIdx > dest.Length - size)
				throw new ArgumentException ("destIdx");
		}
		
		class CopyConverter : DataConverter {
			public override double GetDouble (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 8)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");
				double ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 8; i++)
					b [i] = data [index+i];

				return ret;
			}

			public override ulong GetUInt64 (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 8)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				ulong ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 8; i++)
					b [i] = data [index+i];

				return ret;
			}

			public override long GetInt64 (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 8)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				long ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 8; i++)
					b [i] = data [index+i];

				return ret;
			}
			
			public override float GetFloat  (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 4)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				float ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 4; i++)
					b [i] = data [index+i];

				return ret;
			}
			
			public override int GetInt32  (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 4)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				int ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 4; i++)
					b [i] = data [index+i];

				return ret;
			}
			
			public override uint GetUInt32 (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 4)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				uint ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 4; i++)
					b [i] = data [index+i];

				return ret;
			}
			
			public override short GetInt16 (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 2)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				short ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 2; i++)
					b [i] = data [index+i];

				return ret;
			}
			
			public override ushort GetUInt16 (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 2)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				ushort ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 2; i++)
					b [i] = data [index+i];

				return ret;
			}
			
			public override void PutBytes (byte [] dest, int destIdx, double value)
			{
				Check (dest, destIdx, 8);
				fixed (byte *target = &dest [destIdx]){
					long *source = (long *) &value;

					*((long *)target) = *source;
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, float value)
			{
				Check (dest, destIdx, 4);
				fixed (byte *target = &dest [destIdx]){
					uint *source = (uint *) &value;

					*((uint *)target) = *source;
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, int value)
			{
				Check (dest, destIdx, 4);
				fixed (byte *target = &dest [destIdx]){
					uint *source = (uint *) &value;

					*((uint *)target) = *source;
				}
			}

			public override void PutBytes (byte [] dest, int destIdx, uint value)
			{
				Check (dest, destIdx, 4);
				fixed (byte *target = &dest [destIdx]){
					uint *source = (uint *) &value;

					*((uint *)target) = *source;
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, long value)
			{
				Check (dest, destIdx, 8);
				fixed (byte *target = &dest [destIdx]){
					long *source = (long *) &value;

					*((long*)target) = *source;
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, ulong value)
			{
				Check (dest, destIdx, 8);
				fixed (byte *target = &dest [destIdx]){
					ulong *source = (ulong *) &value;

					*((ulong *) target) = *source;
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, short value)
			{
				Check (dest, destIdx, 2);
				fixed (byte *target = &dest [destIdx]){
					ushort *source = (ushort *) &value;

					*((ushort *)target) = *source;
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, ushort value)
			{
				Check (dest, destIdx, 2);
				fixed (byte *target = &dest [destIdx]){
					ushort *source = (ushort *) &value;

					*((ushort *)target) = *source;
				}
			}
		}

		class SwapConverter : DataConverter {
			public override double GetDouble (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 8)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				double ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 8; i++)
					b [7-i] = data [index+i];

				return ret;
			}

			public override ulong GetUInt64 (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 8)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				ulong ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 8; i++)
					b [7-i] = data [index+i];

				return ret;
			}

			public override long GetInt64 (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 8)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				long ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 8; i++)
					b [7-i] = data [index+i];

				return ret;
			}
			
			public override float GetFloat  (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 4)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				float ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 4; i++)
					b [3-i] = data [index+i];

				return ret;
			}
			
			public override int GetInt32  (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 4)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				int ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 4; i++)
					b [3-i] = data [index+i];

				return ret;
			}
			
			public override uint GetUInt32 (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 4)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				uint ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 4; i++)
					b [3-i] = data [index+i];

				return ret;
			}
			
			public override short GetInt16 (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 2)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				short ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 2; i++)
					b [1-i] = data [index+i];

				return ret;
			}
			
			public override ushort GetUInt16 (byte [] data, int index)
			{
				if (data == null)
					throw new ArgumentNullException ("data");
				if (data.Length - index < 2)
					throw new ArgumentException ("index");
				if (index < 0)
					throw new ArgumentException ("index");

				ushort ret;
				byte *b = (byte *)&ret;

				for (int i = 0; i < 2; i++)
					b [1-i] = data [index+i];

				return ret;
			}

			public override void PutBytes (byte [] dest, int destIdx, double value)
			{
				Check (dest, destIdx, 8);

				fixed (byte *target = &dest [destIdx]){
					byte *source = (byte *) &value;

					for (int i = 0; i < 8; i++)
						target [i] = source [7-i];
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, float value)
			{
				Check (dest, destIdx, 4);

				fixed (byte *target = &dest [destIdx]){
					byte *source = (byte *) &value;

					for (int i = 0; i < 4; i++)
						target [i] = source [3-i];
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, int value)
			{
				Check (dest, destIdx, 4);

				fixed (byte *target = &dest [destIdx]){
					byte *source = (byte *) &value;

					for (int i = 0; i < 4; i++)
						target [i] = source [3-i];
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, uint value)
			{
				Check (dest, destIdx, 4);

				fixed (byte *target = &dest [destIdx]){
					byte *source = (byte *) &value;

					for (int i = 0; i < 4; i++)
						target [i] = source [3-i];
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, long value)
			{
				Check (dest, destIdx, 8);

				fixed (byte *target = &dest [destIdx]){
					byte *source = (byte *) &value;

					for (int i = 0; i < 8; i++)
						target [i] = source [7-i];
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, ulong value)
			{
				Check (dest, destIdx, 8);

				fixed (byte *target = &dest [destIdx]){
					byte *source = (byte *) &value;

					for (int i = 0; i < 4; i++)
						target [i] = source [7-i];
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, short value)
			{
				Check (dest, destIdx, 2);

				fixed (byte *target = &dest [destIdx]){
					byte *source = (byte *) &value;

					for (int i = 0; i < 2; i++)
						target [i] = source [1-i];
				}
			}
			
			public override void PutBytes (byte [] dest, int destIdx, ushort value)
			{
				Check (dest, destIdx, 2);

				fixed (byte *target = &dest [destIdx]){
					byte *source = (byte *) &value;

					for (int i = 0; i < 2; i++)
						target [i] = source [1-i];
				}
			}
		}
		
#if MONO_DATACONVERTER_STATIC_METHODS
		static unsafe void PutBytesLE (byte *dest, byte *src, int count)
		{
			int i = 0;
			
			if (BitConverter.IsLittleEndian){
				for (; i < count; i++)
					*dest++ = *src++;
			} else {
				dest += count;
				for (; i < count; i++)
					*(--dest) = *src++;
			}
		}

		static unsafe void PutBytesBE (byte *dest, byte *src, int count)
		{
			int i = 0;
			
			if (BitConverter.IsLittleEndian){
				dest += count;
				for (; i < count; i++)
					*(--dest) = *src++;
			} else {
				for (; i < count; i++)
					*dest++ = *src++;
			}
		}

		static unsafe void PutBytesNative (byte *dest, byte *src, int count)
		{
			int i = 0;
			
			for (; i < count; i++)
				dest [i-count] = *src++;
		}
		
		static public unsafe double DoubleFromLE (byte[] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 8)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			double ret;
			fixed (byte *src = &data[index]){
				PutBytesLE ((byte *) &ret, src, 8);
			}
			return ret;
		}

		static public unsafe float FloatFromLE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 4)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			float ret;
			fixed (byte *src = &data[index]){
				PutBytesLE ((byte *) &ret, src, 4);
			}
			return ret;
		}

		static public unsafe long Int64FromLE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 8)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			long ret;
			fixed (byte *src = &data[index]){
				PutBytesLE ((byte *) &ret, src, 8);
			}
			return ret;
		}
		
		static public unsafe ulong UInt64FromLE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 8)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			ulong ret;
			fixed (byte *src = &data[index]){
				PutBytesLE ((byte *) &ret, src, 8);
			}
			return ret;
		}

		static public unsafe int Int32FromLE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 4)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			int ret;
			fixed (byte *src = &data[index]){
				PutBytesLE ((byte *) &ret, src, 4);
			}
			return ret;
		}
		
		static public unsafe uint UInt32FromLE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 4)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			uint ret;
			fixed (byte *src = &data[index]){
				PutBytesLE ((byte *) &ret, src, 4);
			}
			return ret;
		}

		static public unsafe short Int16FromLE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 2)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");

			short ret;
			fixed (byte *src = &data[index]){
				PutBytesLE ((byte *) &ret, src, 2);
			}
			return ret;
		}
		
		static public unsafe ushort UInt16FromLE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 2)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			ushort ret;
			fixed (byte *src = &data[index]){
				PutBytesLE ((byte *) &ret, src, 2);
			}
			return ret;
		}

		static public unsafe double DoubleFromBE (byte[] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 8)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			double ret;
			fixed (byte *src = &data[index]){
				PutBytesBE ((byte *) &ret, src, 8);
			}
			return ret;
		}

		static public unsafe float FloatFromBE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 4)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			float ret;
			fixed (byte *src = &data[index]){
				PutBytesBE ((byte *) &ret, src, 4);
			}
			return ret;
		}

		static public unsafe long Int64FromBE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 8)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			long ret;
			fixed (byte *src = &data[index]){
				PutBytesBE ((byte *) &ret, src, 8);
			}
			return ret;
		}
		
		static public unsafe ulong UInt64FromBE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 8)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			ulong ret;
			fixed (byte *src = &data[index]){
				PutBytesBE ((byte *) &ret, src, 8);
			}
			return ret;
		}

		static public unsafe int Int32FromBE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 4)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			int ret;
			fixed (byte *src = &data[index]){
				PutBytesBE ((byte *) &ret, src, 4);
			}
			return ret;
		}
		
		static public unsafe uint UInt32FromBE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 4)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			uint ret;
			fixed (byte *src = &data[index]){
				PutBytesBE ((byte *) &ret, src, 4);
			}
			return ret;
		}

		static public unsafe short Int16FromBE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 2)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");

			short ret;
			fixed (byte *src = &data[index]){
				PutBytesBE ((byte *) &ret, src, 2);
			}
			return ret;
		}
		
		static public unsafe ushort UInt16FromBE (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 2)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			ushort ret;
			fixed (byte *src = &data[index]){
				PutBytesBE ((byte *) &ret, src, 2);
			}
			return ret;
		}

		static public unsafe double DoubleFromNative (byte[] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 8)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			double ret;
			fixed (byte *src = &data[index]){
				PutBytesNative ((byte *) &ret, src, 8);
			}
			return ret;
		}

		static public unsafe float FloatFromNative (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 4)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			float ret;
			fixed (byte *src = &data[index]){
				PutBytesNative ((byte *) &ret, src, 4);
			}
			return ret;
		}

		static public unsafe long Int64FromNative (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 8)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			long ret;
			fixed (byte *src = &data[index]){
				PutBytesNative ((byte *) &ret, src, 8);
			}
			return ret;
		}
		
		static public unsafe ulong UInt64FromNative (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 8)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			ulong ret;
			fixed (byte *src = &data[index]){
				PutBytesNative ((byte *) &ret, src, 8);
			}
			return ret;
		}

		static public unsafe int Int32FromNative (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 4)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			int ret;
			fixed (byte *src = &data[index]){
				PutBytesNative ((byte *) &ret, src, 4);
			}
			return ret;
		}
		
		static public unsafe uint UInt32FromNative (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 4)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			uint ret;
			fixed (byte *src = &data[index]){
				PutBytesNative ((byte *) &ret, src, 4);
			}
			return ret;
		}

		static public unsafe short Int16FromNative (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 2)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");

			short ret;
			fixed (byte *src = &data[index]){
				PutBytesNative ((byte *) &ret, src, 2);
			}
			return ret;
		}
		
		static public unsafe ushort UInt16FromNative (byte [] data, int index)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length - index < 2)
				throw new ArgumentException ("index");
			if (index < 0)
				throw new ArgumentException ("index");
			
			ushort ret;
			fixed (byte *src = &data[index]){
				PutBytesNative ((byte *) &ret, src, 2);
			}
			return ret;
		}

                unsafe static byte[] GetBytesPtr (byte *ptr, int count)
                {
                        byte [] ret = new byte [count];

                        for (int i = 0; i < count; i++) {
                                ret [i] = ptr [i];
                        }

                        return ret;
                }

                unsafe static byte[] GetBytesSwap (bool swap, byte *ptr, int count)
                {
                        byte [] ret = new byte [count];

			if (swap){
				int t = count-1;
				for (int i = 0; i < count; i++) {
					ret [t-i] = ptr [i];
				}
			} else {
				for (int i = 0; i < count; i++) {
					ret [i] = ptr [i];
				}
			}
                        return ret;
                }
		
                unsafe public static byte[] GetBytesNative (bool value)
                {
                        return GetBytesPtr ((byte *) &value, 1);
                }

                unsafe public static byte[] GetBytesNative (char value)
                {
                        return GetBytesPtr ((byte *) &value, 2);
                }

                unsafe public static byte[] GetBytesNative (short value)
                {
                        return GetBytesPtr ((byte *) &value, 2);
                }

                unsafe public static byte[] GetBytesNative (int value)
                {
                        return GetBytesPtr ((byte *) &value, 4);
                }

                unsafe public static byte[] GetBytesNative (long value)
                {
                        return GetBytesPtr ((byte *) &value, 8);
                }

                [CLSCompliant (false)]
                unsafe public static byte[] GetBytesNative (ushort value)
                {
                        return GetBytesPtr ((byte *) &value, 2);
                }

                [CLSCompliant (false)]
                unsafe public static byte[] GetBytesNative (uint value)
                {
                        return GetBytesPtr ((byte *) &value, 4);
                }

                [CLSCompliant (false)]
                unsafe public static byte[] GetBytesNative (ulong value)
                {
                        return GetBytesPtr ((byte *) &value, 8);
                }

                unsafe public static byte[] GetBytesNative (float value)
                {
                        return GetBytesPtr ((byte *) &value, 4);
                }

                unsafe public static byte[] GetBytesNative (double value)
                {
			return GetBytesPtr ((byte *) &value, 8);
                }

                unsafe public static byte[] GetBytesLE (bool value)
                {
                        return GetBytesSwap (!BitConverter.IsLittleEndian, (byte *) &value, 1);
                }

                unsafe public static byte[] GetBytesLE (char value)
                {
                        return GetBytesSwap (!BitConverter.IsLittleEndian, (byte *) &value, 2);
                }

                unsafe public static byte[] GetBytesLE (short value)
                {
                        return GetBytesSwap (!BitConverter.IsLittleEndian, (byte *) &value, 2);
                }

                unsafe public static byte[] GetBytesLE (int value)
                {
                        return GetBytesSwap (!BitConverter.IsLittleEndian, (byte *) &value, 4);
                }

                unsafe public static byte[] GetBytesLE (long value)
                {
                        return GetBytesSwap (!BitConverter.IsLittleEndian, (byte *) &value, 8);
                }

                [CLSCompliant (false)]
                unsafe public static byte[] GetBytesLE (ushort value)
                {
                        return GetBytesSwap (!BitConverter.IsLittleEndian, (byte *) &value, 2);
                }

                [CLSCompliant (false)]
                unsafe public static byte[] GetBytesLE (uint value)
                {
                        return GetBytesSwap (!BitConverter.IsLittleEndian, (byte *) &value, 4);
                }

                [CLSCompliant (false)]
                unsafe public static byte[] GetBytesLE (ulong value)
                {
                        return GetBytesSwap (!BitConverter.IsLittleEndian, (byte *) &value, 8);
                }

                unsafe public static byte[] GetBytesLE (float value)
                {
                        return GetBytesSwap (!BitConverter.IsLittleEndian, (byte *) &value, 4);
                }

                unsafe public static byte[] GetBytesLE (double value)
                {
			return GetBytesSwap (!BitConverter.IsLittleEndian, (byte *) &value, 8);
                }
		
                unsafe public static byte[] GetBytesBE (bool value)
                {
                        return GetBytesSwap (BitConverter.IsLittleEndian, (byte *) &value, 1);
                }

                unsafe public static byte[] GetBytesBE (char value)
                {
                        return GetBytesSwap (BitConverter.IsLittleEndian, (byte *) &value, 2);
                }

                unsafe public static byte[] GetBytesBE (short value)
                {
                        return GetBytesSwap (BitConverter.IsLittleEndian, (byte *) &value, 2);
                }

                unsafe public static byte[] GetBytesBE (int value)
                {
                        return GetBytesSwap (BitConverter.IsLittleEndian, (byte *) &value, 4);
                }

                unsafe public static byte[] GetBytesBE (long value)
                {
                        return GetBytesSwap (BitConverter.IsLittleEndian, (byte *) &value, 8);
                }

                [CLSCompliant (false)]
                unsafe public static byte[] GetBytesBE (ushort value)
                {
                        return GetBytesSwap (BitConverter.IsLittleEndian, (byte *) &value, 2);
                }

                [CLSCompliant (false)]
                unsafe public static byte[] GetBytesBE (uint value)
                {
                        return GetBytesSwap (BitConverter.IsLittleEndian, (byte *) &value, 4);
                }

                [CLSCompliant (false)]
                unsafe public static byte[] GetBytesBE (ulong value)
                {
                        return GetBytesSwap (BitConverter.IsLittleEndian, (byte *) &value, 8);
                }

                unsafe public static byte[] GetBytesBE (float value)
                {
                        return GetBytesSwap (BitConverter.IsLittleEndian, (byte *) &value, 4);
                }

                unsafe public static byte[] GetBytesBE (double value)
                {
			return GetBytesSwap (BitConverter.IsLittleEndian, (byte *) &value, 8);
                }
#endif
		
	}
}
