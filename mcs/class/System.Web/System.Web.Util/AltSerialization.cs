//
// System.Web.Util.AltSerialization
//
// Author(s):
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Web.Util {

	internal sealed class AltSerialization {

                private static ArrayList types;

                internal static readonly int NullIndex = 16;
                
		private AltSerialization () { }
                
                
                static AltSerialization ()
                {
                        types = new ArrayList ();
                        types.Add ("");
                        types.Add (typeof (string));
                        types.Add (typeof (int));
                        types.Add (typeof (bool));
                        types.Add (typeof (DateTime));
                        types.Add (typeof (Decimal));
                        types.Add (typeof (Byte));
                        types.Add (typeof (Char));
                        types.Add (typeof (Single));
                        types.Add (typeof (Double));
                        types.Add (typeof (short));
                        types.Add (typeof (long));
                        types.Add (typeof (ushort));
                        types.Add (typeof (uint));
                        types.Add (typeof (ulong));
                }
                
		internal static void SerializeByType (BinaryWriter w, object value)
		{
			Type type = value.GetType ();
			int i = types.IndexOf (type);
			if (i == -1) {
				w.Write (15); // types.Count
				BinaryFormatter bf = new BinaryFormatter ();
				bf.Serialize (w.BaseStream, value);
				return;
			}
			
			w.Write (i);
			switch (i) {
			case 1:
				w.Write ((string) value);
				break;
			case 2:
				w.Write ((int) value);
				break;
			case 3:
				w.Write ((bool) value);
				break;
			case 4:
				w.Write (((DateTime) value).Ticks);
				break;
			case 5:
				w.Write ((decimal) value);
				break;
			case 6:
				w.Write ((byte) value);
				break;
			case 7:
				w.Write ((char) value);
				break;
			case 8:
				w.Write ((float) value);
				break;
			case 9:
				w.Write ((double) value);
				break;
			case 10:
				w.Write ((short) value);
				break;
			case 11:
				w.Write ((long) value);
				break;
			case 12:
				w.Write ((ushort) value);
				break;
			case 13:
				w.Write ((uint) value);
				break;
			case 14:
				w.Write ((ulong) value);
				break;
			}
		}

		internal static object DeserializeFromIndex (int index, BinaryReader r)
		{
			if (index == 15){
				BinaryFormatter bf = new BinaryFormatter ();
				return bf.Deserialize (r.BaseStream);
			}
			
			object value = null;
			switch (index) {
			case 1:
				value = r.ReadString ();
				break;
			case 2:
				value = r.ReadInt32 ();
				break;
			case 3:
				value = r.ReadBoolean ();
				break;
			case 4:
				value = new DateTime (r.ReadInt64 ());
				break;
			case 5:
				value = r.ReadDecimal ();
				break;
			case 6:
				value = r.ReadByte ();
				break;
			case 7:
				value = r.ReadChar ();
				break;
			case 8:
				value = r.ReadSingle ();
				break;
			case 9:
				value = r.ReadDouble ();
				break;
			case 10:
				value = r.ReadInt16 ();
				break;
			case 11:
				value = r.ReadInt64 ();
				break;
			case 12:
				value = r.ReadUInt16 ();
				break;
			case 13:
				value = r.ReadUInt32 ();
				break;
			case 14:
				value = r.ReadUInt64 ();
				break;
			}
			
			return value;
		}
	}
}

