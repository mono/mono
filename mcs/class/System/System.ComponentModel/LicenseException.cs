//
// System.ComponentModel.LicenseException.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public class LicenseException : SystemException
	{

		private Type type;

		public LicenseException (Type type)
			: this (type, null)
		{
		}
		
		public LicenseException (Type type, object instance)
		{
			// LAMESPEC what should we do with instance?
			this.type = type;
		}
		
		public LicenseException (Type type, object instance, 
					 string message)
			: this (type, instance, message, null)
		{
		}
		
		public LicenseException (Type type, object instance,
					 string message,
					 Exception innerException)
			: base (message, innerException)
		{
			// LAMESPEC what should we do with instance?
			this.type = type;
		}
		
		public Type LicensedType {
			get { return type; }
		}		
	}
}
