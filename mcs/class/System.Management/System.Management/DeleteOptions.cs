//
// System.Management.DeleteOptions
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;

namespace System.Management
{
	public class DeleteOptions : ManagementOptions, ICloneable
	{
		[MonoTODO]
		public DeleteOptions ()
		{
		}

		[MonoTODO]
		public DeleteOptions (ManagementNamedValueCollection context, TimeSpan timeout)
		{
		}

		[MonoTODO]
		public override object Clone ()
		{
			throw new NotImplementedException ();
		}
	}
}

