//
// System.ComponentModel.LicenseManager.cs
//
// Authors:
//   Ivan Hamilton (ivan@chimerical.com.au)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2004 Ivan Hamilton
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public sealed class LicenseManager
	{
		private static LicenseContext mycontext = null;
		private static object contextLockUser = null;

		private LicenseManager ()
		{
		}

		public static LicenseContext CurrentContext {
			get {
				lock (typeof(LicenseManager)) {
					//Tests indicate a System.ComponentModel.Design.RuntimeLicenseContext should be returned.
					if  (mycontext==null)
						mycontext = new Design.RuntimeLicenseContext();
					return mycontext;
				}
			} 
			set { 
				lock (typeof(LicenseManager)) {
					if (contextLockUser==null) {
						mycontext = value;
					} else {
						throw new InvalidOperationException("The CurrentContext property of the LicenseManager is currently locked and cannot be changed.");
					}
				}
			}
		}

		public static LicenseUsageMode UsageMode {
			get {
				return CurrentContext.UsageMode;
			}
		}

		public static object CreateWithContext (Type type,
							LicenseContext creationContext)
		{
			return CreateWithContext (type, creationContext, new object [0]);
		}

		public static object CreateWithContext (Type type, 
							LicenseContext creationContext, 
							object[] args)
		{
			object newObject = null;
			lock (typeof (LicenseManager)) {
				object lockObject = new object ();
				LicenseContext oldContext = CurrentContext;
				CurrentContext = creationContext;
				LockContext (lockObject);
				try {
					newObject = Activator.CreateInstance (type, args);
				} catch (Reflection.TargetInvocationException exception) {
					throw exception.InnerException;
				} finally {
					UnlockContext (lockObject);
					CurrentContext = oldContext;
				}
			}
			return newObject;
		}

		public static bool IsLicensed (Type type)
		{
			License license = null;
			if (!privateGetLicense (type, null, false, out license)) {
				return false;
			} else {
				if (license != null)
					license.Dispose ();
				return true;
			}
		}

		public static bool IsValid (Type type)
		//This method does not throw a LicenseException when it cannot grant a valid License
		{
			License license=null;
			if (!privateGetLicense (type, null, false, out license)) {
				return false;
			} else {
				if (license != null)
					license.Dispose ();
				return true;
			}
		}

		public static bool IsValid (Type type, object instance, 
					    out License license)
		//This method does not throw a LicenseException when it cannot grant a valid License
		{
			return privateGetLicense (type, null, false, out license);
		}

		public static void LockContext (object contextUser)
		{
			lock (typeof (LicenseManager)) {
				contextLockUser = contextUser;
			}
		}

		public static void UnlockContext (object contextUser)
		{
			lock (typeof(LicenseManager)) {
				//Ignore if we're not locked
				if (contextLockUser == null)
					return;
				//Don't unlock if not locking user
				if (contextLockUser != contextUser)
					throw new ArgumentException ("The CurrentContext property of the LicenseManager can only be unlocked with the same contextUser.");
				//Remove lock
				contextLockUser = null;
			}
		}

		public static void Validate (Type type)
		// Throws a  LicenseException if the type is licensed, but a License could not be granted. 
		{
			License license = null;
			if (!privateGetLicense (type, null, true, out license))
				throw new LicenseException (type, null);
			if (license != null)
				license.Dispose ();
		}

		public static License Validate (Type type, object instance)
		// Throws a  LicenseException if the type is licensed, but a License could not be granted. 
		{
			License license=null;
			if (!privateGetLicense(type, instance, true, out license))
				throw new LicenseException(type, instance);
			return license;
		}

		private static bool privateGetLicense (Type type, object instance, bool allowExceptions, out License license) 
		//Returns if a component is licensed, and the license if provided
		{
			bool isLicensed = false;
			License foundLicense = null;
			//Get the LicProc Attrib for our type
			LicenseProviderAttribute licenseproviderattribute = (LicenseProviderAttribute) Attribute.GetCustomAttribute(type, typeof (LicenseProviderAttribute), true);
			//Check it's got an attrib
			if (licenseproviderattribute != null) {
				Type licenseprovidertype = licenseproviderattribute.LicenseProvider;
				//Check the attrib has a type
				if (licenseprovidertype != null) {
					//Create the provider
					LicenseProvider licenseprovider = (LicenseProvider) Activator.CreateInstance(licenseprovidertype);
					//Check we've got the provider
					if (licenseprovider != null) {
						//Call provider, throw an LicenseException if error.
						foundLicense = licenseprovider.GetLicense(CurrentContext, type, instance, allowExceptions);
						if (foundLicense != null)
							isLicensed = true;
						//licenseprovider.Dispose();
					} else {
						//There is was some problem creating the provider
					}
					//licenseprovidertype.Dispose();
				} else {
					//licenseprovidertype is null
				}
				//licenseproviderattribute.Dispose ();
			} else {
				//Didn't have a LicenseProviderAttribute, so it's licensed
				isLicensed = true;
			}
			license = foundLicense;
			return isLicensed;
		}
	}
}
