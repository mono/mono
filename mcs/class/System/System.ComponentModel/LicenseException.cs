//
// System.ComponentModel.LicenseException
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	public class LicenseException : SystemException
	{
		[MonoTODO]
		public LicenseException (Type type)
		{
		}
		
		[MonoTODO]
		public LicenseException (Type type, object instance)
		{
		}
		
		[MonoTODO]
		public LicenseException (Type type, object instance, 
					 string message)
		{
		}
		
		[MonoTODO]
		public LicenseException (Type type, object instance,
					 string message,
					 Exception innerException)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public Type LicensedType {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~LicenseException()
		{
		}
		
	}
}
