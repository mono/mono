//
// System.Management.ManagementOptions
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;

namespace System.Management
{
	public abstract class ManagementOptions : ICloneable
	{
		public static readonly TimeSpan InfiniteTimeout = TimeSpan.MaxValue;
		ManagementNamedValueCollection context;
		TimeSpan timeout;

		internal ManagementOptions ()
			: this (null, InfiniteTimeout)
		{
		}

		internal ManagementOptions (ManagementNamedValueCollection context, TimeSpan timeout)
		{
			this.context = context;
			this.timeout = timeout;
		}

		[MonoTODO]
		public virtual object Clone ()
		{
			throw new NotImplementedException ();
		}

		public ManagementNamedValueCollection Context {
			get { return context; }
			set { context = value; }
		}

		public TimeSpan Timeout {
			get { return timeout; }
			set { timeout = value; }
		}
	}
}

