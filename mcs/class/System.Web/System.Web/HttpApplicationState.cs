// 
// System.Web.HttpApplicationState
//
// Author:
//   Patrik Torstensson
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

using System.Threading;
using System.Collections.Specialized;
using System.Security.Permissions;

namespace System.Web
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HttpApplicationState : NameObjectCollectionBase 
	{
		HttpStaticObjectsCollection _AppObjects;
		HttpStaticObjectsCollection _SessionObjects;

		ReaderWriterLockSlim _Lock; 

		internal HttpApplicationState ()
		{
			_Lock = new ReaderWriterLockSlim ();
		}

		internal HttpApplicationState (HttpStaticObjectsCollection AppObj, HttpStaticObjectsCollection SessionObj)
		{
			_AppObjects = AppObj;
			_SessionObjects = SessionObj;
			_Lock = new ReaderWriterLockSlim ();
		}

		public void Add (string name, object value)
		{
			try {
				_Lock.EnterWriteLock ();
				BaseAdd (name, value);
			} finally {
				_Lock.ExitWriteLock ();
			}
		}

		public void Clear ()
		{
			try {
				_Lock.EnterWriteLock ();
				BaseClear ();
			} finally {
				_Lock.ExitWriteLock ();
			}
		} 

		public object Get (string name)
		{
			object ret = null;

			try {
				_Lock.EnterReadLock ();
				ret = BaseGet (name);
			}  finally {
				_Lock.ExitReadLock ();
			}

			return ret;
		}

		public object Get (int index)
		{
			try {
				_Lock.EnterReadLock ();
				return BaseGet (index);
			} finally {
				_Lock.ExitReadLock ();
			}
		}   

		public string GetKey (int index)
		{
			try {
				_Lock.EnterReadLock ();
				return BaseGetKey (index);
			} finally {
				_Lock.ExitReadLock ();
			}
		}      

		public void Lock ()
		{
			_Lock.EnterWriteLock ();
		}

		public void Remove (string name)
		{
			try {
				_Lock.EnterWriteLock ();
				BaseRemove (name);
			} finally  {
				_Lock.ExitWriteLock ();
			}      
		}

		public void RemoveAll ()
		{
			Clear ();
		}

		public void RemoveAt (int index)
		{
			try {
				_Lock.EnterWriteLock ();
				BaseRemoveAt (index);
			} finally  {
				_Lock.ExitWriteLock ();
			}      
		}

		public void Set (string name, object value)
		{
			try {
				_Lock.EnterWriteLock ();
				BaseSet (name, value);
			} finally  {
				_Lock.ExitWriteLock ();
			}      
		}   

		public void UnLock ()
		{
			_Lock.ExitWriteLock ();
		}

		public string [] AllKeys {
			get {
				try {
					_Lock.EnterReadLock ();
					return BaseGetAllKeys ();
				} finally  {
					_Lock.ExitReadLock ();
				}
			}
		}

		public HttpApplicationState Contents {
			get { return this; }
		}

		public override int Count {
			get {
				try {
					_Lock.EnterReadLock ();
					return base.Count;
				} finally  {
					_Lock.ExitReadLock ();
				}     
			}
		}   

		public object this [string name] {
			get { return Get (name); }
			set { Set (name, value); }
		}

		public object this [int index] {
			get { return Get (index); }
		}

		//  ASP Session based objects
		internal HttpStaticObjectsCollection SessionObjects {
			get {
				if (_SessionObjects == null)
					_SessionObjects = new HttpStaticObjectsCollection ();
				
				return _SessionObjects;
			}
		}

		//  ASP App based objects
		public HttpStaticObjectsCollection StaticObjects {
			get {
				if (_AppObjects == null)
					_AppObjects = new HttpStaticObjectsCollection ();
				
				return _AppObjects;
			}
		}
	}
}

