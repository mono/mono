//
// System.Management.ManagementEventArgs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;

namespace System.Management
{
	public abstract class ManagementEventArgs : EventArgs
	{
		object context;

		internal ManagementEventArgs ()
		{
		}

		internal ManagementEventArgs (object context)
		{
			this.context = context;
		}

		public object Context {
			get { return context; }
		}
	}
}

