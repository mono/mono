//
// X509Extension.cs: Base class for all X.509 extensions.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Text;

using Mono.Security;

namespace Mono.Security.X509 {
	/*
	 * Extension  ::=  SEQUENCE  {
	 *	extnID      OBJECT IDENTIFIER,
	 *	critical    BOOLEAN DEFAULT FALSE,
	 *	extnValue   OCTET STRING  
	 * }
	 */
#if INSIDE_CORLIB
	internal
#else
	public
#endif
	class X509Extension {

		protected string extnOid;
		protected bool extnCritical;
		protected ASN1 extnValue;

		internal X509Extension () 
		{
			extnCritical = false;
		}

		public X509Extension (ASN1 asn1) 
		{
			if ((asn1.Tag != 0x30) || (asn1.Count < 2))
				throw new ArgumentException ("Invalid X.509 extension");
			if (asn1[0].Tag != 0x06)
				throw new ArgumentException ("Invalid X.509 extension");
			extnOid = ASN1Convert.ToOID (asn1 [0]);
			extnCritical = ((asn1[1].Tag == 0x01) && (asn1[1].Value[0] == 0xFF));
			extnValue = asn1 [asn1.Count - 1]; // last element
			Decode ();
		}

		public X509Extension (X509Extension extension) : this () 
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");
			if ((extension.Value.Tag != 0x04) || (extension.Value.Count != 0))
				throw new ArgumentException ("Invalid extension");
			extnOid = extension.OID;
			extnCritical = extension.Critical;
			extnValue = extension.Value;
			Decode ();
		}

		protected virtual void Decode () {}

		protected virtual void Encode () {}

		public ASN1 ASN1 {
			get {
				ASN1 extension = new ASN1 (0x30);
				extension.Add (ASN1Convert.FromOID (extnOid));
				if (extnCritical)
					extension.Add (new ASN1 (0x01, new byte [1] { 0x01 }));
				ASN1 os = extension.Add (new ASN1 (0x04));
				Encode ();
				os.Add (extnValue);
				return extension;
			}
		}

		public string OID {
			get { return extnOid; }
		}

		public bool Critical {
			get { return extnCritical; }
		}

		// this gets overrided with more meaningful names
		public virtual string Name {
			get { return extnOid; }
		}

		public ASN1 Value {
			get { return extnValue; }
		}

		public byte[] GetBytes () 
		{
			return ASN1.GetBytes ();
		}

		private void WriteLine (StringBuilder sb, int n, int pos) 
		{
			byte[] value = extnValue.Value;
			int p = pos;
			StringBuilder preview = new StringBuilder ();
			for (int j=0; j < 8; j++) {
				if (j < n) {
					sb.Append (value [p++].ToString ("X2"));
					sb.Append (" ");
				}
				else
					sb.Append ("   ");
			}
			sb.Append ("  ");
			p = pos;
			for (int j=0; j < n; j++) {
				byte b = value [p++];
				if (b < 0x20)
					sb.Append (".");
				else
					sb.Append (Convert.ToChar (b));
			}
			sb.Append (Environment.NewLine);
		}

		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			int div = (extnValue.Length >> 3);
			int rem = (extnValue.Length - (div << 3));
			int x = 0;
			for (int i=0; i < div; i++) {
				WriteLine (sb, 8, x);
				x += 8;
			}
			WriteLine (sb, rem, x);
			return sb.ToString ();
		}
	}
}
