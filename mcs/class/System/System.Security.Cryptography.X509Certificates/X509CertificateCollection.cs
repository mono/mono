//
// System.Security.Cryptography.X509Certificates.X509CertificateCollection
//
// Author:
//	Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.Security.Cryptography;

namespace System.Security.Cryptography.X509Certificates {

[Serializable]
public class X509CertificateCollection : CollectionBase, IEnumerable {
	
	public X509CertificateCollection () {}
	
	public X509CertificateCollection (X509Certificate [] value) 
	{
		AddRange (value);
	}
	
	public X509CertificateCollection (X509CertificateCollection value)
	{
		AddRange (value);
	}
	
	// Properties
	
	public X509Certificate this [int index] {
		get { return (X509Certificate) InnerList [index]; }
		set { InnerList [index] = value; }
	}
	
	// Methods

	public int Add (X509Certificate value)
	{
		if (value == null)
			throw new ArgumentNullException ("value");
		
		return InnerList.Add (value);
	}
	
	public void AddRange (X509Certificate [] value) 
	{
		if (value == null)
			throw new ArgumentNullException ("value");
		for (int i = 0; i < value.Length; i++) 
			InnerList.Add (value);
	}
	
	public void AddRange (X509CertificateCollection value)
	{
		if (value == null)
			throw new ArgumentNullException ("value");
		int len = value.InnerList.Count;
		for (int i = 0; i < len; i++) 
			InnerList.Add (value);
	}
	
	public bool Contains (X509Certificate value) 
	{
		return InnerList.Contains (value);
	}


	public void CopyTo (X509Certificate[] array, int index)
	{
		InnerList.CopyTo (array, index);
	}
	
	public new X509CertificateEnumerator GetEnumerator ()
	{
		return new X509CertificateEnumerator (this);
	}
	
	IEnumerator IEnumerable.GetEnumerator ()
	{
		return InnerList.GetEnumerator ();
	}
	
	public override int GetHashCode () 
	{
		return InnerList.GetHashCode ();
	}
	
	public int IndexOf (X509Certificate value)
	{
		return InnerList.IndexOf (value);
	}
	
	public void Insert (int index, X509Certificate value)
	{
		InnerList.Insert (index, value);
	}
	
	public void Remove (X509Certificate value)
	{
		InnerList.Remove (value);
	}

	// Inner Class
	
	public class X509CertificateEnumerator : IEnumerator {

		private IEnumerator enumerator;

		// Constructors
		
		public X509CertificateEnumerator (X509CertificateCollection mappings)
		{
			enumerator = ((IEnumerable) mappings).GetEnumerator ();
		}

		// Properties
		
		public X509Certificate Current {
			get { return (X509Certificate) enumerator.Current; }
		}
		
		object IEnumerator.Current {
			get { return (X509Certificate) enumerator.Current; }
		}

		// Methods
		
		bool IEnumerator.MoveNext ()
		{
			return enumerator.MoveNext ();
		}
		
		void IEnumerator.Reset () 
		{
			enumerator.Reset ();
		}
		
		public bool MoveNext () 
		{
			return enumerator.MoveNext ();
		}
		
		public void Reset ()
		{
			enumerator.Reset ();
		}
	}		
}

}

