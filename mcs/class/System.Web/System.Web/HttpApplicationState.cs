// 
// System.Web.HttpApplicationState
//
// Author:
//   Patrik Torstensson
//
using System;
using System.Threading;
using System.Web;
using System.Collections.Specialized;

namespace System.Web 
{

	public sealed class HttpApplicationState : NameObjectCollectionBase 
	{
		private HttpStaticObjectsCollection _AppObjects;
		private HttpStaticObjectsCollection _SessionObjects;

		private ReaderWriterLock _Lock; 

		internal HttpApplicationState ()
		{
			_AppObjects = new HttpStaticObjectsCollection ();
			_SessionObjects = new HttpStaticObjectsCollection ();
			_Lock = new ReaderWriterLock ();
		}

		internal HttpApplicationState (HttpStaticObjectsCollection AppObj,
			HttpStaticObjectsCollection SessionObj)
		{
			if (null != AppObj) 
			{
				_AppObjects = AppObj;
			} 
			else 
			{
				_AppObjects = new HttpStaticObjectsCollection ();
			}

			if (null != SessionObj) 
			{
				_SessionObjects = SessionObj;
			} 
			else 
			{
				_SessionObjects = new HttpStaticObjectsCollection ();
			}
			_Lock = new ReaderWriterLock ();
		}

		public void Add (string name, object value)
		{
			_Lock.AcquireWriterLock (-1); 
			try 
			{
				BaseAdd (name, value);
			} 
			finally 
			{
				_Lock.ReleaseWriterLock ();
			}
		}

		public void Clear ()
		{
			_Lock.AcquireWriterLock (-1); 
			try 
			{
				BaseClear ();
			} 
			finally 
			{
				_Lock.ReleaseWriterLock ();
			}
		} 

		public object Get (string name)
		{
			object ret = null;

			_Lock.AcquireReaderLock (-1); 
			try 
			{
				ret = BaseGet (name);
			} 
			finally 
			{
				_Lock.ReleaseReaderLock ();
			}

			return ret;
		}

		public object Get (int index)
		{
			object ret = null;

			_Lock.AcquireReaderLock (-1); 
			try 
			{
				ret = BaseGet (index);
			} 
			finally 
			{
				_Lock.ReleaseReaderLock ();
			}

			return ret;
		}   

		public string GetKey (int index)
		{
			string ret = null;

			_Lock.AcquireReaderLock (-1); 
			try 
			{
				ret = BaseGetKey (index);
			} 
			finally 
			{
				_Lock.ReleaseReaderLock ();
			}

			return ret;
		}      

		public void Lock ()
		{
			_Lock.AcquireWriterLock (-1);
		}

		public void Remove (string name)
		{
			_Lock.AcquireWriterLock (-1); 
			try 
			{
				BaseRemove (name);
			} 
			finally 
			{
				_Lock.ReleaseWriterLock ();
			}      
		}

		public void RemoveAll ()
		{
			Clear ();
		}

		public void RemoveAt (int index)
		{
			_Lock.AcquireWriterLock (-1); 
			try 
			{
				BaseRemoveAt (index);
			} 
			finally 
			{
				_Lock.ReleaseWriterLock ();
			}      
		}

		public void Set (string name, object value)
		{
			_Lock.AcquireWriterLock (-1); 
			try 
			{
				BaseSet (name, value);
			} 
			finally 
			{
				_Lock.ReleaseWriterLock ();
			}      
		}   

		public void UnLock ()
		{
			_Lock.ReleaseWriterLock ();
		}

		public string [] AllKeys 
		{
			get 
			{
				string [] ret = null;

				_Lock.AcquireReaderLock (-1); 
				try 
				{
					ret = BaseGetAllKeys ();
				} 
				finally 
				{
					_Lock.ReleaseReaderLock ();
				}     

				return ret;
			}
		}

		public HttpApplicationState Contents 
		{
			get { return this; }
		}

		public override int Count 
		{
			get 
			{
				int ret = 0;

				_Lock.AcquireReaderLock (-1); 
				try 
				{
					ret = base.Count;
				} 
				finally 
				{
					_Lock.ReleaseReaderLock ();
				}     

				return ret;
			}
		}   

		public object this [string name] 
		{
			get { return Get (name); }
			set { Set (name, value); }
		}

		public object this [int index] 
		{
			get { return Get (index); }
		}

		//  ASP Session based objects
		internal HttpStaticObjectsCollection SessionObjects 
		{
			get { return _SessionObjects; }
		}

		//  ASP App based objects
		public HttpStaticObjectsCollection StaticObjects 
		{
			get { return _AppObjects; }
		}
	}
}

