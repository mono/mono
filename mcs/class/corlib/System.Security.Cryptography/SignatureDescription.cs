//
// System.Security.Cryptography SignatureDescription Class implementation
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

// Notes:
// There seems to be some (internal?) class inheriting from SignatureDescription
// http://www.csharpfriends.com/Members/Main/Classes/get_class.aspx?assembly=mscorlib,%20Version=1.0.3300.0,%20Culture=neutral,%20PublicKeyToken=b77a5c561934e089&namespace=System.Security.Cryptography&class=SignatureDescription
// Those 2 classes are returned by CryptoConfig.CreateFromName and used in XMLDSIG

using System;
using System.Security;

namespace System.Security.Cryptography {
	
public class SignatureDescription {
	private string _DeformatterAlgorithm;
	private string _DigestAlgorithm;		
	private string _FormatterAlgorithm;		
	private string _KeyAlgorithm;		

	public SignatureDescription () {}
	
	/// LAMESPEC: ArgumentNullException is thrown (not CryptographicException)
	[MonoTODO("Parse SecurityElement")]
	public SignatureDescription (SecurityElement el) 
	{
		if (el == null)
			throw new ArgumentNullException ();
		// TODO: Parse the SecurityElement 
		// Clearly it must contains Deformatter, Digest, 
		// Formatter and KeyAlgorithm... 
		// But what do the SecurityElement looks like ?
	}

	// There are no validation of the property
	public string DeformatterAlgorithm {
		get { return _DeformatterAlgorithm; }
		set { _DeformatterAlgorithm = value; }
	}

	// There are no validation of the property
	public string DigestAlgorithm {
		get { return _DigestAlgorithm; }
		set { _DigestAlgorithm = value; }
	}

	// There are no validation of the property
	public string FormatterAlgorithm {
		get { return _FormatterAlgorithm; }
		set { _FormatterAlgorithm = value; }
	}

	// There are no validation of the property
	public string KeyAlgorithm {
		get { return _KeyAlgorithm; }
		set { _KeyAlgorithm = value; }
	}

	public virtual AsymmetricSignatureDeformatter CreateDeformatter (AsymmetricAlgorithm key) 
	{
		if (_DeformatterAlgorithm == null)
			throw new ArgumentNullException ("DeformatterAlgorithm");

		// this should throw the InvalidCastException if we have an invalid class
		// (but not if the class doesn't exist - as null is valid for AsymmetricSignatureDeformatter)
		AsymmetricSignatureDeformatter def = (AsymmetricSignatureDeformatter) CryptoConfig.CreateFromName (_DeformatterAlgorithm);

		if (_KeyAlgorithm == null)
			throw new NullReferenceException ("KeyAlgorithm");

		def.SetKey (key);
		return def;
	}
	
	/// <summary>
	/// Create the hash algorithm assigned with this object
	/// </summary>
	public virtual HashAlgorithm CreateDigest ()
	{
		if (_DigestAlgorithm == null)
			throw new ArgumentNullException ("DigestAlgorithm");
		return (HashAlgorithm) CryptoConfig.CreateFromName (_DigestAlgorithm);
	}

	public virtual AsymmetricSignatureFormatter CreateFormatter (AsymmetricAlgorithm key)
	{
		if (_FormatterAlgorithm == null)
			throw new ArgumentNullException ("FormatterAlgorithm");

		// this should throw the InvalidCastException if we have an invalid class
		// (but not if the class doesn't exist - as null is valid for AsymmetricSignatureDeformatter)
		AsymmetricSignatureFormatter fmt = (AsymmetricSignatureFormatter) CryptoConfig.CreateFromName (_FormatterAlgorithm);

		if (_KeyAlgorithm == null)
			throw new NullReferenceException ("KeyAlgorithm");

		fmt.SetKey (key);
		return fmt;
	}
	
} // SignatureDescription

internal class DSASignatureDescription : SignatureDescription {
	public DSASignatureDescription () 
	{
		DeformatterAlgorithm = "System.Security.Cryptography.DSASignatureDeformatter";
		DigestAlgorithm = "System.Security.Cryptography.SHA1CryptoServiceProvider";
		FormatterAlgorithm = "System.Security.Cryptography.DSASignatureFormatter";		
		KeyAlgorithm = "System.Security.Cryptography.DSACryptoServiceProvider";		
	}
}

internal class RSAPKCS1SHA1SignatureDescription : SignatureDescription {
	public RSAPKCS1SHA1SignatureDescription () 
	{
		DeformatterAlgorithm = "System.Security.Cryptography.RSAPKCS1SignatureDeformatter";
		DigestAlgorithm = "System.Security.Cryptography.SHA1CryptoServiceProvider";
		FormatterAlgorithm = "System.Security.Cryptography.RSAPKCS1SignatureFormatter";		
		KeyAlgorithm = "System.Security.Cryptography.RSACryptoServiceProvider";		
	}

	public override AsymmetricSignatureDeformatter CreateDeformatter (AsymmetricAlgorithm key) 
	{
		// just to please corcompare
		return base.CreateDeformatter (key);
	}
}
	
} // System.Security.Cryptography
