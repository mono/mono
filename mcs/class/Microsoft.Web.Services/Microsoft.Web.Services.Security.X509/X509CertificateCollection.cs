//
// X509CertificateCollection.cs: Handles certificate collection.
// based on: mcs\class\System\System.Security.Cryptography.X509Certificates\X509CertificateCollection.cs
//
// Authors:
//	Lawrence Pit (loz@cable.a2000.nl) -- original work
//	Sebastien Pouliot (spouliot@motus.com) -- all modifications and bugs ;-)
//

using System;
using System.Collections;

namespace Microsoft.Web.Services.Security.X509 {

	public class X509CertificateCollection : CollectionBase {
		
		public X509CertificateCollection () {}
		
		public X509Certificate this [int index] {
			get { 
				// required for exception ?
				if ((index < 0) || (index > InnerList.Count))
					throw new ArgumentOutOfRangeException ("index");
				return (X509Certificate) InnerList [index]; 
			}
		}
		
		public int Add (X509Certificate certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("value");
			
			return InnerList.Add (certificate);
		}
		
		public bool Contains (X509Certificate certificate) 
		{
			return InnerList.Contains (certificate);
		}

		public void CopyTo (Array array, int index) 
		{
			InnerList.CopyTo (array, index);
		}
		
		public int IndexOf (X509Certificate certificate) 
		{
			return InnerList.IndexOf (certificate);
		}
		
		public void Insert (int index, X509Certificate certificate) 
		{
			InnerList.Insert (index, certificate);
		}
		
		public void Remove (X509Certificate certificate) 
		{
			InnerList.Remove (certificate);
		}
	}
}
