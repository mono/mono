//
// System.Web.SessionState.HttpSessionState
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
#if !NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System.Web.SessionState {

// CAS - no InheritanceDemand here as the class is sealed
[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
public sealed class HttpSessionState : ICollection, IEnumerable
{
	string _id;
	SessionDictionary _dict;
	HttpStaticObjectsCollection _staticObjects;
	int _timeout;
	bool _newSession;
	bool _isCookieless;
	SessionStateMode _mode;
	bool _isReadonly;
	internal bool _abandoned;

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
		_staticObjects = staticObjects.Clone ();
		_timeout = timeout;
		_newSession = newSession;
		_isCookieless = isCookieless;
		_mode = mode;
		_isReadonly = isReadonly;
	}

	internal HttpSessionState Clone ()
	{
		return new HttpSessionState (_id, _dict.Clone (), _staticObjects, _timeout, _newSession,
					     _isCookieless, _mode, _isReadonly);
	}
	
	public int CodePage {
		get {
			HttpContext current = HttpContext.Current;
			if (current == null)
				return Encoding.Default.CodePage;

			return current.Response.ContentEncoding.CodePage;
		}
		
		set {
			HttpContext current = HttpContext.Current;
			if (current != null)
				current.Response.ContentEncoding = Encoding.GetEncoding (value);
		}
	}

	public HttpSessionState Contents {
		get { return this; }
	}

	public int Count {
		get { return _dict.Count; }
	}

	internal bool IsAbandoned {
		get { return _abandoned; }
	}

	public bool IsCookieless {
		get { return _isCookieless; }
	}

	public bool IsNewSession {
		get { return _newSession; }
	}

	public bool IsReadOnly {
		get { return _isReadonly; }
	}

	public bool IsSynchronized {
		get { return false; }
	}

	public object this [string key] {
		get { return _dict [key]; }
		set { _dict [key] = value; }
	}

	public object this [int index] {
		get { return _dict [index]; }
		set { _dict [index] = value; }
	}

	public NameObjectCollectionBase.KeysCollection Keys {
		get { return _dict.Keys; }
	}

	public int LCID {
		get { return Thread.CurrentThread.CurrentCulture.LCID; }
		set { Thread.CurrentThread.CurrentCulture = new CultureInfo(value); }
	}

	public SessionStateMode Mode {
		get { return _mode; }
	}

	public string SessionID {
		get { return _id; }
	}

	public HttpStaticObjectsCollection StaticObjects {
		get { return _staticObjects; }
	}

	public object SyncRoot {
		get { return this; }
	}

	public int Timeout {
		get { return _timeout; }
		set {
                        if (value < 1)
                                throw new ArgumentException ("The argument to SetTimeout must be greater than 0.");
                        _timeout = value;
                }
	}

	internal SessionDictionary SessionDictionary {
		get { return _dict; }
	}

	internal void SetNewSession (bool value)
	{
		_newSession = value;
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
			array.SetValue (all.Get(i), i + index);
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
#endif
