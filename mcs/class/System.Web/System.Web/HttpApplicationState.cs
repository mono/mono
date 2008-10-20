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

namespace System.Web {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HttpApplicationState : NameObjectCollectionBase 
	{
		HttpStaticObjectsCollection _AppObjects;
		HttpStaticObjectsCollection _SessionObjects;

		ReaderWriterLock _Lock; 

		internal HttpApplicationState ()
		{
			// do not use the public (empty) ctor as it required UnmanagedCode permission
			_AppObjects = new HttpStaticObjectsCollection (this);
			_SessionObjects = new HttpStaticObjectsCollection (this);
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
				// do not use the public (empty) ctor as it required UnmanagedCode permission
				_AppObjects = new HttpStaticObjectsCollection (this);
			}

			if (null != SessionObj) 
			{
				_SessionObjects = SessionObj;
			} 
			else 
			{
				// do not use the public (empty) ctor as it required UnmanagedCode permission
				_SessionObjects = new HttpStaticObjectsCollection (this);
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

