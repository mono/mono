//
// System.Management.PutOptions
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;

namespace System.Management
{
	public class PutOptions : ManagementOptions, ICloneable
	{
		public PutOptions ()
		{
		}

		public PutOptions (ManagementNamedValueCollection context)
		{
		}

		public PutOptions (ManagementNamedValueCollection context,
				   TimeSpan timeout,
				   bool useAmendedQualifiers,
				   PutType putType)
		{
		}

		[MonoTODO]
		public override object Clone ()
		{
			throw new NotImplementedException ();
		}

		public PutType Type {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		public bool UseAmendedQualifiers {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}
	}
}

