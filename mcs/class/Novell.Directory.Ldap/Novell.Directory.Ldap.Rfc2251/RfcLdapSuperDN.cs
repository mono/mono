//
// Novell.Directory.Ldap.Rfc2251.RfcLdapSuperDN.cs
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//

using System;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
	///<summary>Represents an [0] LDAP DN OPTIONAL used as newSuperior attribute of
	/// ModifyDNRequest (For more detail on this Syntax refer to rfc2251).
	/// </summary>
	public class RfcLdapSuperDN : Asn1Tagged
	{
		private sbyte[] content;
	
		/// <summary>
		/// ASN.1 [0] LDAP DN OPTIONAL tag definition.
		/// </summary>
		public static readonly int TAG = 0x00;

		/// <summary> ID is added for Optimization.
		/// Id needs only be one Value for every instance, thus we create it only once.
		/// </summary>
		protected static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.CONTEXT, false, TAG);
	   
		/// <summary> Constructs an RfcLDAPSuperDN object from a String object.
		/// </summary>
		/// <param name="content"> A string value that will be contained in the this RfcLDAPSuperDN object </param>
		public RfcLdapSuperDN(String s) : base(ID, new Asn1OctetString(s), false) //type is encoded IMPLICITLY 
		{			
			try {
				System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
				byte[] ibytes = encoder.GetBytes(s);
				sbyte[] sbytes=SupportClass.ToSByteArray(ibytes);

				this.content = sbytes;
			} 
			catch(System.IO.IOException uee) {
				throw new System.SystemException(uee.ToString());
			}
		}
		
		/// <summary> Constructs an RfcLDAPSuperDN object from a byte array. </summary>
		/// <param name="content"> A byte array representing the string that will be contained in the this RfcLDAPSuperDN object </param>
		[CLSCompliantAttribute(false)]
		public RfcLdapSuperDN(sbyte[] ba) : base(ID, new Asn1OctetString(ba), false) //type is encoded IMPLICITLY 
		{			
			this.content = ba;
		}
	
		/// <summary> Encodes the current instance into the
		/// specified output stream using the specified encoder object.
		/// 
		/// </summary>
		/// <param name="enc">Encoder object to use when encoding self.
		/// 
		/// </param>
		/// <param name="out">The output stream onto which the encoded byte
		/// stream is written.
		/// </param>
		public override void  encode(Asn1Encoder enc, System.IO.Stream out_Renamed)
		{
			enc.encode(this, out_Renamed);
			return ;
		}

		/// <summary> Returns the content of this RfcLdapSuperDN as a byte array.</summary>
		[CLSCompliantAttribute(false)]
		public sbyte[] byteValue()
		{
			return content;
		}
		
		
		/// <summary> Returns the content of this RfcLdapSuperDN as a String.</summary>
		public System.String stringValue()
		{
			System.String s = null;
			try {
				System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
				char[] dchar = encoder.GetChars(SupportClass.ToByteArray(content));
				s = new String(dchar);
			}
			catch (System.IO.IOException uee) {
				throw new System.SystemException(uee.ToString());
			}			
			return s;
		}
		
		
		/// <summary> Return a String representation of this RfcLdapSuperDN.</summary>
		public override System.String ToString()
		{
			return base.ToString() + " " + stringValue();
		}

	}
}
