//
// System.Management.ProgressEventArgs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
namespace System.Management
{
	public class ProgressEventArgs : ManagementEventArgs
	{
		[MonoTODO]
		internal ProgressEventArgs ()
		{
		}

		public int Current
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public string Message
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public int UpperBound
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

