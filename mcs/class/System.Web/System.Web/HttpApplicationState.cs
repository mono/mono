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

		bool IsLockHeld {
			get { return _Lock.IsReadLockHeld || _Lock.IsWriteLockHeld; }
		}
		
		public void Add (string name, object value)
		{
			bool acquired = false;
			try {
				if (!IsLockHeld) {
					_Lock.EnterWriteLock ();
					acquired = true;
				}
				BaseAdd (name, value);
			} finally {
				if (acquired && IsLockHeld)
					_Lock.ExitWriteLock ();
			}
		}

		public void Clear ()
		{
			bool acquired = false;
			try {
				if (!IsLockHeld) {
					_Lock.EnterWriteLock ();
					acquired = true;
				}
				BaseClear ();
			} finally {
				if (acquired && IsLockHeld)
					_Lock.ExitWriteLock ();
			}
		} 

		public object Get (string name)
		{
			object ret = null;
			bool acquired = false;
			try {
				if (!IsLockHeld) {
					_Lock.EnterReadLock ();
					acquired = true;
				}
				ret = BaseGet (name);
			}  finally {
				if (acquired && IsLockHeld)
					_Lock.ExitReadLock ();
			}

			return ret;
		}

		public object Get (int index)
		{
			bool acquired = false;
			try {
				if (!IsLockHeld) {
					_Lock.EnterReadLock ();
					acquired = true;
				}
				return BaseGet (index);
			} finally {
				if (acquired && IsLockHeld)
					_Lock.ExitReadLock ();
			}
		}   

		public string GetKey (int index)
		{
			bool acquired = false;
			try {
				if (!IsLockHeld) {
					_Lock.EnterReadLock ();
					acquired = true;
				}
				return BaseGetKey (index);
			} finally {
				if (acquired && IsLockHeld)
					_Lock.ExitReadLock ();
			}
		}      

		public void Lock ()
		{
			if (!_Lock.IsWriteLockHeld)
				_Lock.EnterWriteLock ();
		}

		public void Remove (string name)
		{
			bool acquired = false;
			try {
				if (!IsLockHeld) {
					_Lock.EnterWriteLock ();
					acquired = true;
				}
				BaseRemove (name);
			} finally  {
				if (acquired && IsLockHeld)
					_Lock.ExitWriteLock ();
			}      
		}

		public void RemoveAll ()
		{
			Clear ();
		}

		public void RemoveAt (int index)
		{
			bool acquired = false;
			try {
				if (!IsLockHeld) {
					_Lock.EnterWriteLock ();
					acquired = true;
				}
				BaseRemoveAt (index);
			} finally  {
				if (acquired && IsLockHeld)
					_Lock.ExitWriteLock ();
			}      
		}

		public void Set (string name, object value)
		{
			bool acquired = false;
			try {
				if (!IsLockHeld) {
					_Lock.EnterWriteLock ();
					acquired = true;
				}
				BaseSet (name, value);
			} finally  {
				if (acquired && IsLockHeld)
					_Lock.ExitWriteLock ();
			}      
		}   

		public void UnLock ()
		{
			if (_Lock.IsWriteLockHeld)
				_Lock.ExitWriteLock ();
		}

		public string [] AllKeys {
			get {
				bool acquired = false;
				try {
					if (!IsLockHeld) {
						_Lock.EnterReadLock ();
						acquired = true;
					}
					return BaseGetAllKeys ();
				} finally  {
					if (acquired && IsLockHeld)
						_Lock.ExitReadLock ();
				}
			}
		}

		public HttpApplicationState Contents {
			get { return this; }
		}

		public override int Count {
			get {
				bool acquired = false;
				try {
					if (!IsLockHeld) {
						_Lock.EnterReadLock ();
						acquired = true;
					}
					return base.Count;
				} finally  {
					if (acquired && IsLockHeld)
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

