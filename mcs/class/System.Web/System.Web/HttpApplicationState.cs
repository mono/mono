// 
// System.Web.HttpApplicationState
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Threading;
using System.Web;
using System.Collections.Specialized;

namespace System.Web {

	[MonoTODO("Performance - Use SWMR lock here")]
	public sealed class HttpApplicationState : NameObjectCollectionBase {
		private HttpStaticObjectsCollection _AppObjects;
		private HttpStaticObjectsCollection _SessionObjects;

		// TODO : Change to ReadWriteLock when ready
		private Mutex _Lock; 

		private void LockRead ()
		{
			Monitor.Enter (this);
		}

		private void LockWrite ()
		{
			Monitor.Enter (this);
		}

		private void UnlockRead ()
		{
			Monitor.Exit (this);
		}

		private void UnlockWrite ()
		{
			Monitor.Exit (this);
		}

		internal HttpApplicationState ()
		{
			_AppObjects = new HttpStaticObjectsCollection ();
			_SessionObjects = new HttpStaticObjectsCollection ();
			_Lock = new Mutex ();
		}

		internal HttpApplicationState (HttpStaticObjectsCollection AppObj,
						HttpStaticObjectsCollection SessionObj)
		{
			if (null != AppObj) {
				_AppObjects = AppObj;
			} else {
				_AppObjects = new HttpStaticObjectsCollection ();
			}

			if (null != SessionObj) {
				_SessionObjects = SessionObj;
			} else {
				_SessionObjects = new HttpStaticObjectsCollection ();
			}
			_Lock = new Mutex ();
		}

		public void Add (string name, object value)
		{
			LockWrite (); 
			try {
				BaseAdd (name, value);
			} finally {
				UnlockWrite ();
			}
		}

		public void Clear ()
		{
			LockWrite (); 
			try {
				BaseClear ();
			} finally {
				UnlockWrite ();
			}
		} 

		public object Get (string name)
		{
			object ret = null;

			LockRead (); 
			try {
				ret = BaseGet (name);
			} finally {
				UnlockRead ();
			}

			return ret;
		}

		public object Get (int index)
		{
			object ret = null;

			LockRead (); 
			try {
				ret = BaseGet (index);
			} finally {
				UnlockRead ();
			}

			return ret;
		}   

		public string GetKey (int index)
		{
			string ret = null;

			LockRead (); 
			try {
				ret = BaseGetKey (index);
			} finally {
				UnlockRead ();
			}

			return ret;
		}      

		public void Lock ()
		{
			LockWrite ();
		}

		public void Remove (string name)
		{
			LockWrite (); 
			try {
				BaseRemove (name);
			} finally {
				UnlockWrite ();
			}      
		}

		public void RemoveAll ()
		{
			Clear ();
		}

		public void RemoveAt (int index)
		{
			LockWrite (); 
			try {
				BaseRemoveAt (index);
			} finally {
				UnlockWrite ();
			}      
		}

		public void Set (string name, object value)
		{
			LockWrite (); 
			try {
				BaseSet (name, value);
			} finally {
				UnlockWrite ();
			}      
		}   

		public void UnLock ()
		{
			UnlockWrite ();
		}

		public string [] AllKeys {
			get {
				string [] ret = null;

				LockRead (); 
				try {
					ret = BaseGetAllKeys ();
				} finally {
					UnlockRead ();
				}     

				return ret;
			}
		}

		public HttpApplicationState Contents {
			get { return this; }
		}

		public override int Count {
			get {
				int ret = 0;

				LockRead(); 
				try {
					ret = base.Count;
				} finally {
					UnlockRead ();
				}     

				return ret;
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
			get { return _SessionObjects; }
		}

		//  ASP App based objects
		public HttpStaticObjectsCollection StaticObjects {
			get { return _AppObjects; }
		}
	}
}

