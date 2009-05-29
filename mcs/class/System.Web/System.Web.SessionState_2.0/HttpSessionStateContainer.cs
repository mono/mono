//
// System.Web.Compilation.SessionStateItemCollection
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
#if NET_2_0
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web.Util;

namespace System.Web.SessionState 
{
	public class HttpSessionStateContainer : IHttpSessionState
	{
		string id;
		HttpStaticObjectsCollection staticObjects;
		int timeout;
		bool newSession;
		bool isCookieless;
		SessionStateMode mode;
		bool isReadOnly;
		internal bool abandoned;
		ISessionStateItemCollection sessionItems;
		HttpCookieMode cookieMode;
		
		public HttpSessionStateContainer (string id,
						  ISessionStateItemCollection sessionItems,
						  HttpStaticObjectsCollection staticObjects,
						  int timeout,
						  bool newSession,
						  HttpCookieMode cookieMode,
						  SessionStateMode mode,
						  bool isReadonly)
		{
			if (id == null)
				throw new ArgumentNullException ("id");
			
			this.sessionItems = sessionItems;
			this.id = id;
			this.staticObjects = staticObjects;
			this.timeout = timeout;
			this.newSession = newSession;
			this.cookieMode = cookieMode;
			this.mode = mode;
			this.isReadOnly = isReadonly;
			this.isCookieless = cookieMode == HttpCookieMode.UseUri;
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
		
		public HttpCookieMode CookieMode {
			get { return cookieMode; }
		}
		
		public int Count {
			get {
				if (sessionItems != null)
					return sessionItems.Count;
				return 0;
			}
		}
		
		public bool IsAbandoned {
			get { return abandoned; }
		}
		
		public bool IsCookieless {
			get { return isCookieless; }
		}
		
		public bool IsNewSession {
			get { return newSession; }
		}
		
		public bool IsReadOnly {
			get { return isReadOnly; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		object IHttpSessionState.this [int index] {
			get {
				if (sessionItems == null || sessionItems.Count == 0)
					return null;
				return sessionItems [index];
			}
			
			set {
				if (sessionItems != null)
					sessionItems [index] = value;
			}
		}
		
                object IHttpSessionState.this [string name] {
			get {
				if (sessionItems == null || sessionItems.Count == 0)
					return null;
				return sessionItems [name];
			}
			
			set {
				if (sessionItems != null)
					sessionItems [name] = value;
			}
		}
		
		NameObjectCollectionBase.KeysCollection IHttpSessionState.Keys {
			get {
				if (sessionItems != null)
					return sessionItems.Keys;
				return null;
			}
		}
		
		public int LCID {
			get { return Thread.CurrentThread.CurrentCulture.LCID; }
			set { Thread.CurrentThread.CurrentCulture = new CultureInfo(value); }
		}
		
		public SessionStateMode Mode {
			get { return mode; }
		}
		
		public string SessionID {
			get { return id; }
		}
		
		public HttpStaticObjectsCollection StaticObjects {
			get { return staticObjects; }
		}
		
		public Object SyncRoot {
			get { return this; }
		}
		
		public int Timeout {
			get { return timeout; }
			set {
				if (value < 1)
					throw new ArgumentException ("The argument to SetTimeout must be greater than 0.");
				timeout = value;
			}
		}

		internal void SetNewSession (bool value)
		{
			newSession = value;
		}
		
		public void Abandon ()
		{
			abandoned = true;
		}

		public void Add (string name, Object value)
		{
			if (sessionItems == null)
				return;
			sessionItems [name] = value;
		}

		public void Clear ()
		{
			if (sessionItems == null)
				return;
			sessionItems.Clear ();
		}

		public void CopyTo (Array array, int index)
		{
			if (sessionItems == null)
				return;
			NameObjectCollectionBase.KeysCollection all = sessionItems.Keys;
			for (int i = 0; i < all.Count; i++)
				array.SetValue (all.Get(i), i + index);
		}

		public IEnumerator GetEnumerator ()
		{
			if (sessionItems == null)
				return null;
			return sessionItems.GetEnumerator ();
		}
		
		public void Remove (string name)
		{
			if (sessionItems == null)
				return;
			sessionItems.Remove (name);
		}

		public void RemoveAll ()
		{
			if (sessionItems == null)
				return;
			sessionItems.Clear ();
		}

		public void RemoveAt (int index)
		{
			if (sessionItems == null)
				return;
			sessionItems.RemoveAt (index);
		}
	}
}
#endif