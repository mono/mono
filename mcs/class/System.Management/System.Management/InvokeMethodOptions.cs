//
// System.Management.InvokeMethodOptions
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;

namespace System.Management
{
	public class InvokeMethodOptions : ManagementOptions, ICloneable
	{
		[MonoTODO]
		public InvokeMethodOptions ()
		{
		}

		[MonoTODO]
		public InvokeMethodOptions (ManagementNamedValueCollection context, TimeSpan timeout)
		{
		}

		[MonoTODO]
		public override object Clone ()
		{
			throw new NotImplementedException ();
		}
	}
}

