//
// System.Web.SessionState.HttpSessionState
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;

namespace System.Web.SessionState {
public sealed class HttpSessionState : ICollection, IEnumerable
{
	private string _id;
	private SessionDictionary _dict;
	private HttpStaticObjectsCollection _staticObjects;
	private int _timeout;
	private bool _newSession;
	private bool _isCookieless;
	private SessionStateMode _mode;
	private bool _isReadonly;
	private bool _abandoned;

	internal HttpSessionState (string id,
				   SessionDictionary dict,
				   HttpStaticObjectsCollection staticObjects,
				   int timeout,
				   bool newSession,
				   bool isCookieless,
				   SessionStateMode mode,
				   bool isReadonly)
	{
		_id = id;
		_dict = dict;
		_staticObjects = staticObjects;
		_timeout = timeout;
		_newSession = newSession;
		_isCookieless = isCookieless;
		_mode = mode;
		_isReadonly = isReadonly;
	}

	// Compatibility with ASP
	public int CodePage
	{
		get { return 0; }
		set { }
	}

	public HttpSessionState Contents
	{
		get { return this; }
	}

	public int Count
	{
		get { return _dict.Count; }
	}

	internal bool IsAbandoned
	{
		get { return _abandoned; }
	}

	public bool IsCookieless
	{
		get { return _isCookieless; }
	}

	public bool IsNewSession
	{
		get { return _newSession; }
	}

	public bool IsReadOnly
	{
		get { return _isReadonly; }
	}

	public bool IsSynchronized
	{
		get { return false; }
	}

	public object this [string key]
	{
		get { return _dict [key]; }
		set { _dict [key] = value; }
	}

	public object this [int index]
	{
		get { return _dict [index]; }
		set {
			string key = _dict.Keys [index];
			_dict [key] = value;
		}
	}

	public NameObjectCollectionBase.KeysCollection Keys
	{
		get { return _dict.Keys; }
	}

	public int LCID
	{
		get { return Thread.CurrentThread.CurrentCulture.LCID; }
		set { Thread.CurrentThread.CurrentCulture = new CultureInfo(value); }
	}

	public SessionStateMode Mode
	{
		get { return _mode; }
	}

	public string SessionID
	{
		get { return _id; }
	}

	public HttpStaticObjectsCollection StaticObjects
	{
		get { return _staticObjects; }
	}

	public object SyncRoot
	{
		get { return this; }
	}

	public int Timeout
	{
		get { return _timeout; }
		set { _timeout = value; }
	}

	public void Abandon ()
	{
		_abandoned = true;
	}

	public void Add (string name, object value)
	{
		_dict [name] = value;
	}

	public void Clear ()
	{
		if (_dict != null)
			_dict.Clear ();
	}
	
	public void CopyTo (Array array, int index)
	{
		NameObjectCollectionBase.KeysCollection all = Keys;
		for (int i = 0; i < all.Count; i++)
			array.SetValue (_dict [all [i]], i + index);
	}

	public IEnumerator GetEnumerator ()
	{
		return _dict.GetEnumerator ();
	}
	
	public void Remove (string name)
	{
		_dict.Remove (name);
	}

	public void RemoveAll ()
	{
		_dict.Clear ();
	}

	public void RemoveAt (int index)
	{
		_dict.RemoveAt (index);
	}
}
}

