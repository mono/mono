//
// System.Security.Cryptography.X509Certificates.X509CertificateCollection
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.Security.Cryptography;

namespace System.Security.Cryptography.X509Certificates {

	[Serializable]
	public class X509CertificateCollection : CollectionBase, IEnumerable {
		
		private ArrayList list = new ArrayList ();
		
		// Constructors
		
		public X509CertificateCollection () { }
		
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
			get { return (X509Certificate) list [index]; }
			set { list [index] = value; }
		}
		
		// Methods

		public int Add (X509Certificate value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			
			return list.Add (value);
		}
		
		public void AddRange (X509Certificate [] value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			for (int i = 0; i < value.Length; i++) 
				list.Add (value);
		}
		
		public void AddRange (X509CertificateCollection value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			int len = value.list.Count;
			for (int i = 0; i < len; i++) 
				this.list.Add (value);
		}
		
		public bool Contains (X509Certificate value) 
		{
			return list.Contains (value);
		}


		public void CopyTo (X509Certificate [] array, int index)
		{
			list.CopyTo (array, index);
		}
		
		public new X509CertificateEnumerator GetEnumerator()
		{
			return new X509CertificateEnumerator (this);
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator ();
		}
		
		public override int GetHashCode() 
		{
			return list.GetHashCode ();
		}
		
		public int IndexOf (X509Certificate value)
		{
			return list.IndexOf (value);
		}
		
		public void Insert (int index, X509Certificate value)
		{
			list.Insert (index, value);
		}
		
		public void Remove (X509Certificate value)
		{
			list.Remove (value);
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

