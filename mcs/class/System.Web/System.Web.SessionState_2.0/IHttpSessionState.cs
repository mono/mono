//
// System.Web.SessionState.IReadOnlySessionState
//
// Authors:
//	Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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


using System.Collections;
using System.Collections.Specialized;

namespace System.Web.SessionState {
	public interface IHttpSessionState
	{
		///methods
		void Abandon ();
		void Add (string name, object value);
		void Clear ();
		void CopyTo (Array array, int index);
		IEnumerator GetEnumerator ();
		void Remove (string name);
		void RemoveAll ();
		void RemoveAt (int index);
	
		///properties
		int CodePage { get; set; }
		HttpCookieMode CookieMode { get; }
		int Count { get; }		
		bool IsCookieless { get; }
		bool IsNewSession { get; }
		bool IsReadOnly { get; }
		bool IsSynchronized { get; }
		object this [int index] { get; set; }
		object this [string name] { get; set;}
		NameObjectCollectionBase.KeysCollection Keys { get; }
		int LCID { get; set; }
		SessionStateMode Mode { get; }
		string SessionID { get; }
		HttpStaticObjectsCollection StaticObjects { get; }
		object SyncRoot { get; }
		int Timeout { get; set; }			
	}
}

