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

namespace System.Web.SessionState {
internal class SessionDictionary : NameObjectCollectionBase
{
	bool _dirty;
	
	public SessionDictionary ()
	{
	}

	internal void Clear ()
	{
		_dirty = true;
		lock (this)
			BaseClear ();
	}

	internal string GetKey (int index)
	{
		string value;
		lock (this)
			value = BaseGetKey (index);
			
		return value;
	}

	internal static bool IsInmutable (object o)
	{
		Type t = o.GetType ();
		return (t == typeof (string) || t.IsPrimitive);
	}

	internal void Remove (string s)
	{
		lock (this)
			BaseRemove (s);
		_dirty = true;
	}

	internal void RemoveAt (int index)
	{
		lock (this)
			BaseRemoveAt (index);
		_dirty = true;
	}

	internal void Serialize (BinaryWriter w)
	{
		lock (this) {
			w.Write (Count);
			foreach (string key in Keys) {
				w.Write (key);
				object value = BaseGet (key);
				if (value == null) {
					w.Write (System.Web.Util.AltSerialization.NullIndex);
					continue;
				}

				System.Web.Util.AltSerialization.SerializeByType (w, value);
			}
		}
	}

	internal static SessionDictionary Deserialize (BinaryReader r)
	{
		SessionDictionary result = new SessionDictionary ();
		for (int i = r.ReadInt32 (); i > 0; i--) {
			string key = r.ReadString ();
			int index = r.ReadInt32 ();
			if (index == System.Web.Util.AltSerialization.NullIndex)
				result [key] = null;
			else
				result [key] = System.Web.Util.AltSerialization.DeserializeFromIndex (index, r);
		}

		return result;
	}

	internal bool Dirty
	{
		get { return _dirty; }
		set { _dirty = value; }
	}

	internal object this [string s]
	{
		get {
			object o;
			lock (this)
				o = BaseGet (s);

			return o;
		}

		set {
			lock (this)
			{				 
				object obj = BaseGet(s);
				if ((obj == null) && (value == null))
					return; 
				BaseSet (s, value);
			}

			_dirty = true;
		}
	}

	public object this [int index]
	{
		get {
			object o;
			lock (this)
				o = BaseGet (index);

			return o;
		}
		set {
			lock (this)
			{				 
				object obj = BaseGet(index);
				if ((obj == null) && (value == null))
					return; 
				BaseSet (index, value);
			}

			_dirty = true;
		}
	}

	internal byte [] ToByteArray ()
	{
		MemoryStream stream = null;
		try {
			stream = new MemoryStream ();
			Serialize (new BinaryWriter (stream));
			return stream.GetBuffer ();
		} catch {
			throw;
		} finally {
			if (stream != null)
				stream.Close ();
		}
	}

	internal static SessionDictionary FromByteArray (byte [] data)
	{
		SessionDictionary result = null;
		MemoryStream stream = null;
		try {
			stream = new MemoryStream (data);
			result = Deserialize (new BinaryReader (stream));
		} catch {
			throw;
		} finally {
			if (stream != null)
				stream.Close ();
		}
		return result;
	}
}

}

