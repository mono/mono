//
// System.Management.ObjectPutEventArgs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
namespace System.Management
{
	public class ObjectPutEventArgs : ManagementEventArgs
	{
		internal ObjectPutEventArgs ()
		{
		}

		[MonoTODO]
		public ManagementPath Path
		{
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

