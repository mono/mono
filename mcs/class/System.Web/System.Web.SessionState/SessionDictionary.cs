//
// System.Web.SessionState.SessionDictionary
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Web.SessionState {
internal class SessionDictionary : NameObjectCollectionBase
{
	static ArrayList types;
	bool _dirty;

	static SessionDictionary ()
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

	public SessionDictionary ()
	{
	}

	internal void Clear ()
	{
		_dirty = true;
		BaseClear ();
	}

	internal string GetKey (int index)
	{
		return BaseGetKey (index);
	}

	internal static bool IsInmutable (object o)
	{
		Type t = o.GetType ();
		return (t == typeof (string) || t.IsPrimitive);
	}

	internal void Remove (string s)
	{
		BaseRemove (s);
		_dirty = true;
	}

	internal void RemoveAt (int index)
	{
		BaseRemoveAt (index);
		_dirty = true;
	}

	internal void Serialize (BinaryWriter w)
	{
		w.Write (Count);
		foreach (string key in Keys) {
			w.Write (key);
			object value = BaseGet (key);
			if (value == null) {
				w.Write (16); // types.Count + 1
				continue;
			}

			SerializeByType (w, value);
		}
	}

	static void SerializeByType (BinaryWriter w, object value)
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

	internal static SessionDictionary Deserialize (BinaryReader r)
	{
		SessionDictionary result = new SessionDictionary ();
		for (int i = r.ReadInt32 (); i > 0; i--) {
			string key = r.ReadString ();
			int index = r.ReadInt32 ();
			if (index == 16)
				result [key] = null;
			else
				result [key] = DeserializeFromIndex (index, r);
		}

		return result;
	}

	static object DeserializeFromIndex (int index, BinaryReader r)
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


	internal bool Dirty
	{
		get { return _dirty; }
		set { _dirty = value; }
	}

	internal object this [string s]
	{
		get { return BaseGet (s); }
		set {
			BaseSet (s, value);
			_dirty = true;
		}
	}

	public object this [int index]
	{
		get { return BaseGet (index); }
		set {
			BaseSet (index, value);
			_dirty = true;
		}
	}
}

}

