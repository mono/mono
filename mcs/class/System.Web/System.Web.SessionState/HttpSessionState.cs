//
// System.Web.SessionState.HttpSessionState
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.Collections.Specialized;

namespace System.Web.SessionState {
public sealed class HttpSessionState : ICollection, IEnumerable
{
	private int _codePage;
	private NameValueCollection _state; //FIXME: it should be a ManagementNamedValueCollection

	//public int CodePage -> compatibility with ASP

	public HttpSessionState Contents
	{
		get { return this; }
	}

	public int Count
	{
		get { return _state.Count; }
	}

	[MonoTODO]
	public bool IsCookieless
	{
		get { throw new NotImplementedException (); }
	}

	[MonoTODO]
	public bool IsNewSession
	{
		get { throw new NotImplementedException (); }
	}

	[MonoTODO]
	public bool IsReadOnly
	{
		get { throw new NotImplementedException (); }
	}

	[MonoTODO]
	public bool IsSynchronized
	{
		get { throw new NotImplementedException (); }
	}

	public object this [string key]
	{
		get { return _state [key]; }
		set { _state [key] = (string) value; }
	}

	public object this [int index]
	{
		get { return _state [index]; }
		set {
			string key = _state.Keys [index];
			_state [key] = (string) value;
		}
	}

	public NameObjectCollectionBase.KeysCollection Keys
	{
		get { return _state.Keys; }
	}

	[MonoTODO]
	public int LCID
	{
		get { throw new NotImplementedException (); }
		set { throw new NotImplementedException (); }
	}

	[MonoTODO]
	public SessionStateMode Mode
	{
		get { throw new NotImplementedException (); }
	}

	[MonoTODO]
	public string SessionID
	{
		get { throw new NotImplementedException (); }
	}

	[MonoTODO]
	public HttpStaticObjectsCollection StaticObjects
	{
		get { throw new NotImplementedException (); }
	}

	public object SyncRoot
	{
		get { return this; }
	}

	[MonoTODO]
	public int Timeout
	{
		get { throw new NotImplementedException (); }
		set { throw new NotImplementedException (); }
	}

	[MonoTODO]
	public void Abandon ()
	{
		throw new NotImplementedException ();
	}

	public void Add (string name, object value)
	{
		if (_state == null)
			_state = new NameValueCollection ();

		_state.Add (name, (string) value);
	}

	public void Clear ()
	{
		if (_state != null)
			_state.Clear ();
	}
	
	public void CopyTo (Array array, int index)
	{
		if (_state == null)
			_state = new NameValueCollection ();

		_state.CopyTo (array, index);
	}

	public IEnumerator GetEnumerator ()
	{
		if (_state == null)
			_state = new NameValueCollection ();

		return _state.GetEnumerator ();
	}
	
	public void Remove (string name)
	{
		if (_state != null)
			_state.Remove (name);
	}

	public void RemoveAll ()
	{
		if (_state != null)
			foreach (string key in _state.AllKeys)
				_state.Remove (key);
	}

	[MonoTODO("Implement ManagementNameValueCollection")]
	public void RemoveAt (int index)
	{
		throw new NotImplementedException ();
		//if (_state != null)
		//	_state.RemoveAt (index);
	}
}
}

