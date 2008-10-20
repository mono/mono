//
// System.Web.Util.AltSerialization
//
// Author(s):
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

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
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Web.Util {

	internal sealed class AltSerialization 
	{
		AltSerialization () { }

		internal static void Serialize (BinaryWriter w, object value)
		{
			TypeCode typeCode = value != null ? Type.GetTypeCode (value.GetType ()) : TypeCode.Empty;
			w.Write ((byte)typeCode);

			switch (typeCode) {
			case TypeCode.Boolean:
				w.Write ((bool) value);
				break;
			case TypeCode.Byte:
				w.Write ((byte) value);
				break;
			case TypeCode.Char:
				w.Write ((char) value);
				break;
			case TypeCode.DateTime:
				w.Write (((DateTime) value).Ticks);
				break;
			case TypeCode.DBNull:
				break;
			case TypeCode.Decimal:
				w.Write ((decimal) value);
				break;
			case TypeCode.Double:
				w.Write ((double) value);
				break;
			case TypeCode.Empty:
				break;
			case TypeCode.Int16:
				w.Write ((short) value);
				break;
			case TypeCode.Int32:
				w.Write ((int) value);
				break;
			case TypeCode.Int64:
				w.Write ((long) value);
				break;
			case TypeCode.Object:
#if TARGET_J2EE
				if (w.BaseStream is java.io.ObjectOutput) {
					((java.io.ObjectOutput) w.BaseStream).writeObject (value);
					return;
				}
#endif
				BinaryFormatter bf = new BinaryFormatter ();
				bf.Serialize (w.BaseStream, value);
				break;
			case TypeCode.SByte:
				w.Write ((sbyte) value);
				break;
			case TypeCode.Single:
				w.Write ((float) value);
				break;
			case TypeCode.String:
				w.Write ((string) value);
				break;
			case TypeCode.UInt16:
				w.Write ((ushort) value);
				break;
			case TypeCode.UInt32:
				w.Write ((uint) value);
				break;
			case TypeCode.UInt64:
				w.Write ((ulong) value);
				break;

			}
		}

		internal static object Deserialize (BinaryReader r)
		{
			TypeCode typeCode = (TypeCode)r.ReadByte();
			switch (typeCode) {
			case TypeCode.Boolean:
				return r.ReadBoolean ();
			case TypeCode.Byte:
				return r.ReadByte ();
			case TypeCode.Char:
				return r.ReadChar ();
			case TypeCode.DateTime:
				return new DateTime (r.ReadInt64 ());
			case TypeCode.DBNull:
				return DBNull.Value;
			case TypeCode.Decimal:
				return r.ReadDecimal ();
			case TypeCode.Double:
				return r.ReadDouble ();
			case TypeCode.Empty:
				return null;
			case TypeCode.Int16:
				return r.ReadInt16 ();
			case TypeCode.Int32:
				return r.ReadInt32 ();
			case TypeCode.Int64:
				return r.ReadInt64 ();
			case TypeCode.Object:
#if TARGET_J2EE
				if (r.BaseStream is java.io.ObjectInput)
					return ((java.io.ObjectInput) r.BaseStream).readObject ();
#endif
				BinaryFormatter bf = new BinaryFormatter ();
				return bf.Deserialize (r.BaseStream);
			case TypeCode.SByte:
				return r.ReadSByte ();
			case TypeCode.Single:
				return r.ReadSingle ();
			case TypeCode.String:
				return r.ReadString ();
			case TypeCode.UInt16:
				return r.ReadUInt16 ();
			case TypeCode.UInt32:
				return r.ReadUInt32 ();
			case TypeCode.UInt64:
				return r.ReadUInt64 ();
			default:
				throw new ArgumentOutOfRangeException ("TypeCode:" + typeCode);
			}
		}
	}
}

