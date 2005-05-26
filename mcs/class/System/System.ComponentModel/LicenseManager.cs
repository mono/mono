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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.ComponentModel
{
	public sealed class LicenseManager
	{
		static LicenseContext mycontext;
		static object contextLockUser;
		
		static object lockObject = new object ();

		private LicenseManager ()
		{
		}

		public static LicenseContext CurrentContext {
			get {
				lock (lockObject) {
					//Tests indicate a System.ComponentModel.Design.RuntimeLicenseContext should be returned.
					if  (mycontext==null)
						mycontext = new Design.RuntimeLicenseContext();
					return mycontext;
				}
			} 
			set { 
				lock (lockObject) {
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
			lock (lockObject) {
				object contextUser = new object ();
				LicenseContext oldContext = CurrentContext;
				CurrentContext = creationContext;
				LockContext (contextUser);
				try {
					newObject = Activator.CreateInstance (type, args);
				} catch (Reflection.TargetInvocationException exception) {
					throw exception.InnerException;
				} finally {
					UnlockContext (contextUser);
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
			lock (lockObject) {
				contextLockUser = contextUser;
			}
		}

		public static void UnlockContext (object contextUser)
		{
			lock (lockObject) {
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
