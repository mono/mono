//
// System.Web.SessionState.HttpSessionState.jvm.cs
//
// Authors:
//  Ilya Kharmatsky (ilyak@mainsoft.com)
//  Alon Gazit
//  Pavel Sandler
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.J2EE;
using System.Web.Hosting;

namespace System.Web.SessionState 
{
public sealed class HttpSessionState : ICollection, IEnumerable, java.io.Externalizable
{
	private string _id;
	private SessionDictionary _dict;
	private HttpStaticObjectsCollection _staticObjects;
	private int _timeout;
	private bool _newSession;
	private bool _isCookieless;
	private SessionStateMode _mode;
	private bool _isReadonly;
	internal bool _abandoned;

	private object _app;
	private bool _needSessionPersistence = false;

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

		_app = HttpContext.Current.ApplicationInstance;
		_needSessionPersistence = false;
		javax.servlet.ServletConfig config = (javax.servlet.ServletConfig)AppDomain.CurrentDomain.GetData(J2EEConsts.SERVLET_CONFIG);
		string sessionPersistance = config.getInitParameter(J2EEConsts.Enable_Session_Persistency);
		if (sessionPersistance!= null)
		{
			try
			{
				_needSessionPersistence = Boolean.Parse(sessionPersistance);
			}
			catch (Exception)
			{
				Console.WriteLine("EnableSessionPersistency init param's value is invalid. the value is " + sessionPersistance);
			}
		}
	}

	public HttpSessionState ()
	{
		_id = null;
		_dict = new SessionDictionary();
		_staticObjects = new HttpStaticObjectsCollection();
		_timeout = 0;
		_newSession = false;
		_isCookieless = false;
		_mode = SessionStateMode.Off;
		_isReadonly = false;
	}

	public void writeExternal(java.io.ObjectOutput output)
	{
		lock (this)
		{
			output.writeBoolean(_needSessionPersistence);
			if (!_needSessionPersistence)
				//indicates that there is nothing to serialize for this object
				return;

			System.Web.J2EE.ObjectOutputStream ms = new System.Web.J2EE.ObjectOutputStream(output);
			System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
			bw.Write(_id);
			_dict.Serialize(bw);
			_staticObjects.Serialize(bw);
			bw.Write(_timeout);
			bw.Write(_newSession);
			bw.Write(_isCookieless);
			if (_mode == SessionStateMode.Off)
				bw.Write(0);
			else if (_mode == SessionStateMode.InProc)
				bw.Write(1);
			else if (_mode == SessionStateMode.StateServer)
				bw.Write(2);
			else 
				bw.Write(3);
			bw.Write(_isReadonly);
		}
	}

	public void readExternal(java.io.ObjectInput input)
	{
		lock(this)
		{
			_needSessionPersistence = input.readBoolean();
			if(!_needSessionPersistence) //noting has been written 
				return;

			System.Web.J2EE.ObjectInputStream ms = new System.Web.J2EE.ObjectInputStream( input );
			System.IO.BinaryReader br = new System.IO.BinaryReader(ms);
			_id = br.ReadString();
			_dict =  SessionDictionary.Deserialize(br);
			_staticObjects = HttpStaticObjectsCollection.Deserialize(br);
			_timeout = br.ReadInt32();
			_newSession = br.ReadBoolean();
			_isCookieless = br.ReadBoolean();
			int mode = br.ReadInt32();
			if (mode == 0)
				_mode = SessionStateMode.Off;
			else if (mode == 1)
				_mode = SessionStateMode.InProc;
			else if (mode == 2)
				_mode = SessionStateMode.StateServer;
			else 
				_mode = SessionStateMode.SQLServer;
			_isReadonly = br.ReadBoolean();
			//	_app = HttpContext.Current.ApplicationInstance;
		}
	}

	internal object App
	{
		get
		{
			return _app;
		}
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
		get { 
		   	ServletWorkerRequest worker = (ServletWorkerRequest)HttpContext.Current.Request.WorkerRequest;
		   	return worker.ServletRequest.isRequestedSessionIdFromURL();	
		}
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
		set { 
			_dict [key] = value; 

		   	_newSession = false;
			SetJavaSessionAttribute();
		}
	}

	public object this [int index] {
		get { return _dict [index]; }
		set { 
		   	_dict [index] = value; 

		   	_newSession = false;
			SetJavaSessionAttribute();
		}
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
		get { 
		   	ServletWorkerRequest worker = (ServletWorkerRequest)HttpContext.Current.Request.WorkerRequest;
		   	javax.servlet.http.HttpSession javaSession = worker.ServletRequest.getSession(false);
		   	if (javaSession != null)
		    	 return javaSession.getMaxInactiveInterval()/60;			
		   	else
		     	 throw new NotSupportedException();
		}
		set {
            if (value < 1)
                 throw new ArgumentException ("The argument to SetTimeout must be greater than 0.");
			ServletWorkerRequest worker = (ServletWorkerRequest)HttpContext.Current.Request.WorkerRequest;
			javax.servlet.http.HttpSession javaSession = worker.ServletRequest.getSession(false);
			if (javaSession != null)
				javaSession.setMaxInactiveInterval(value*60);			
			else
				throw new NotSupportedException();
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

		SessionDictionary.Clear();
		ServletWorkerRequest worker = (ServletWorkerRequest)HttpContext.Current.Request.WorkerRequest;
//		worker.Servlet.getServletContext().removeAttribute("GH_SESSION_STATE");
		javax.servlet.http.HttpSession javaSession = worker.ServletRequest.getSession(false);
		if (_app == null)
			_app = HttpContext.Current.ApplicationInstance;
		if (javaSession != null)
		{
			javaSession.setAttribute("GH_SESSION_STATE",this);	
			javaSession.invalidate();
		}
	}

	public void Add (string name, object value)
	{
		_dict [name] = value;

		_newSession = false;
		SetJavaSessionAttribute();
	}

	public void Clear ()
	{
		if (_dict != null)
			_dict.Clear ();

		SetJavaSessionAttribute();
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

		SetJavaSessionAttribute();
	}

	public void RemoveAll ()
	{
		_dict.Clear ();

		SetJavaSessionAttribute();
	}

	public void RemoveAt (int index)
	{
		_dict.RemoveAt (index);

		SetJavaSessionAttribute();
	}
	
	public void SetJavaSessionAttribute ()
	{
		ServletWorkerRequest worker = (ServletWorkerRequest)HttpContext.Current.Request.WorkerRequest;
		javax.servlet.http.HttpSession javaSession = worker.ServletRequest.getSession(false);
		if (javaSession != null)
			javaSession.setAttribute("GH_SESSION_STATE",this);	
	}	
}
}

