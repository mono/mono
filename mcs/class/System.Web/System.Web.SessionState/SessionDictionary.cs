//
// System.Web.SessionState.SessionDictionary
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.IO;
using System.Collections.Specialized;

namespace System.Web.SessionState {
internal class SessionDictionary : NameObjectCollectionBase
{
	private bool _dirty;

	static SessionDictionary ()
	{
	}

	public SessionDictionary ()
	{
	}

	void Clear ()
	{
		_dirty = true;
		BaseClear ();
	}

	[MonoTODO]
	static SessionDictionary Deserialize (BinaryReader r)
	{
		throw new NotImplementedException ();
	}

	public string GetKey (int index)
	{
		return BaseGetKey (index);
	}

	[MonoTODO]
	static bool IsInmutable (object o)
	{
		throw new NotImplementedException ();
	}

	void Remove (string s)
	{
		BaseRemove (s);
		_dirty = true;
	}

	void RemoveAt (int index)
	{
		BaseRemoveAt (index);
		_dirty = true;
	}

	[MonoTODO]
	void Serialize(BinaryWriter w)
	{
		throw new NotImplementedException ();
	}

	bool Dirty
	{
		get { return _dirty; }
		set { _dirty = value; }
	}

	object this [string s]
	{
		get { return BaseGet (s); }
		set {
			BaseSet (s, value);
			_dirty = true;
		}
	}

	object this [int index]
	{
		get { return BaseGet (index); }
		set {
			BaseSet (index, value);
			_dirty = true;
		}
	}
}

}

