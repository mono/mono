//
// System.Management.ObjectReadyEventArgs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
namespace System.Management
{
	public class ObjectReadyEventArgs : ManagementEventArgs
	{
		internal ObjectReadyEventArgs ()
		{
		}

		public ManagementBaseObject NewObject
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

