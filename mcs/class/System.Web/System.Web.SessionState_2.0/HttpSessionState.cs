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
using System.Collections;
using System.Collections.Specialized;
using System.Security.Permissions;
using System.Web;

namespace System.Web.SessionState {

// CAS - no InheritanceDemand here as the class is sealed
[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
public sealed class HttpSessionState : ICollection, IEnumerable
{
	IHttpSessionState container;
	
	internal HttpSessionState (IHttpSessionState container)
	{
		this.container = container;
	}

	internal IHttpSessionState Container {
		get { return container; }
	}
	
	public int CodePage {
		get { return container.CodePage; }
		set { container.CodePage = value; }
	}

	public HttpSessionState Contents {
		get { return this; }
	}

	public HttpCookieMode CookieMode {
		get {
			if (IsCookieless)
				return HttpCookieMode.UseUri;
			else
				return HttpCookieMode.UseCookies;
		}
	}

	public int Count {
		get { return container.Count; }
	}

	public bool IsCookieless {
		get { return container.IsCookieless; }
	}

	public bool IsNewSession {
		get { return container.IsNewSession; }
	}

	public bool IsReadOnly {
		get { return container.IsReadOnly; }
	}

	public bool IsSynchronized {
		get { return container.IsSynchronized; }
	}

	public object this [string name] {
		get { return container [name]; }
		set { container [name] = value; }
	}

	public object this [int index] {
		get { return container [index]; }
		set { container [index] = value; }
	}

	public NameObjectCollectionBase.KeysCollection Keys {
		get { return container.Keys; }
	}

	public int LCID {
		get { return container.LCID; }
		set { container.LCID = value; }
	}

	public SessionStateMode Mode {
		get { return container.Mode; }
	}

	public string SessionID {
		get { return container.SessionID; }
	}

	public HttpStaticObjectsCollection StaticObjects {
		get { return container.StaticObjects; }
	}

	public object SyncRoot {
		get { return container.SyncRoot; }
	}

	public int Timeout {
		get { return container.Timeout; }
		set { container.Timeout = value; }
	}

	public void Abandon ()
	{
		container.Abandon ();
	}

	public void Add (string name, object value)
	{
		container.Add (name, value);
	}

	public void Clear ()
	{
		container.Clear ();
	}
	
	public void CopyTo (Array array, int index)
	{
		container.CopyTo (array, index);
	}

	public IEnumerator GetEnumerator ()
	{
		return container.GetEnumerator ();
	}
	
	public void Remove (string name)
	{
		container.Remove (name);
	}

	public void RemoveAll ()
	{
		container.Clear ();
	}

	public void RemoveAt (int index)
	{
		container.RemoveAt (index);
	}
}
}
