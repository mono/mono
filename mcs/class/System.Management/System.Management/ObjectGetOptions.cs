//
// System.Management.ObjectGetOptions
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
namespace System.Management
{
	public class ObjectGetOptions : ManagementOptions, ICloneable
	{
		[MonoTODO]
		public ObjectGetOptions ()
		{
		}

		[MonoTODO]
		public ObjectGetOptions (ManagementNamedValueCollection context)
		{
		}

		[MonoTODO]
		public ObjectGetOptions (ManagementNamedValueCollection context,
					 TimeSpan timeout,
					 bool useAmendedQualifiers)
		{
		}

		[MonoTODO]
		public override object Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool UseAmendedQualifiers
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
	}
}

