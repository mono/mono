//
// System.Management.CompletedEventArgs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;

namespace System.Management
{
	public class CompletedEventArgs : ManagementEventArgs
	{
		[MonoTODO]
		internal CompletedEventArgs ()
		{
		}

		public ManagementStatus Status
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public ManagementBaseObject StatusObject
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

