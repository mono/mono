//
// System.Security.Cryptography SignatureDescription Class implementation
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

// TODO: Implement SecurityElement parsing
// TODO: Complete AsymmetricSignatureFormatter & AsymmetricSignatureDeformatter methods

// Notes:
// There seems to be some (internal?) class inheriting from SignatureDescription
// http://www.csharpfriends.com/Members/Main/Classes/get_class.aspx?assembly=mscorlib,%20Version=1.0.3300.0,%20Culture=neutral,%20PublicKeyToken=b77a5c561934e089&namespace=System.Security.Cryptography&class=SignatureDescription
// However I've no idea where the class is being used in the framework 
// (doesn't look like it's for every users ;-)

using System;
using System.Security;

namespace System.Security.Cryptography {
	
public class SignatureDescription {
	private string _DeformatterAlgorithm;
	private string _DigestAlgorithm;		
	private string _FormatterAlgorithm;		
	private string _KeyAlgorithm;		

	public SignatureDescription () {}
	
	/// <summary>
	/// LAMESPEC: ArgumentNullException is thrown (not CryptographicException)
	/// </summary>
	[MonoTODO]
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

	private object CreateFromName (string objectName) 
	{
		try {
			// first try
			Type algoClass = Type.GetType (objectName);
			if (algoClass == null) {
				// second (and last) try
				algoClass = Type.GetType ("System.Security.Cryptography." + objectName);
			}
			// call the constructor for the type
			return Activator.CreateInstance (algoClass);
		}
		catch {
			return null;
		}
	}

	[MonoTODO]
	public virtual AsymmetricSignatureDeformatter CreateDeformatter (AsymmetricAlgorithm key) 
	{
		if (_DeformatterAlgorithm == null)
			throw new ArgumentNullException ("DeformatterAlgorithm");

		// this should throw the InvalidCastException if we have an invalid class
		// (but not if the class doesn't exist - as null is valid for AsymmetricSignatureDeformatter)
		AsymmetricSignatureDeformatter def = (AsymmetricSignatureDeformatter) CreateFromName (_DeformatterAlgorithm);
		if (def == null)
			throw new InvalidCastException ("DeformatterAlgorithm");
		def.SetKey (key);

		throw new NullReferenceException ("why?");
		
		// We must make a choice of the Deformatter based on
		// the DeformatterAlgorithm property (factory like CryptoConfig ?)
		// There are only 2 SignatureDeformatter based on the
		// key algorithm (DSA or RSA) - but how does the 
		// KeyAlgorithm property string really looks like ?
	}
	
	/// <summary>
	/// Create the hash algorithm assigned with this object
	/// </summary>
	public virtual HashAlgorithm CreateDigest ()
	{
		return HashAlgorithm.Create (_DigestAlgorithm);
	}

	[MonoTODO]
	public virtual AsymmetricSignatureFormatter CreateFormatter (AsymmetricAlgorithm key)
	{
		if (_FormatterAlgorithm == null)
			throw new ArgumentNullException ("FormatterAlgorithm");

		// this should throw the InvalidCastException if we have an invalid class
		// (but not if the class doesn't exist - as null is valid for AsymmetricSignatureDeformatter)
		AsymmetricSignatureFormatter fmt = (AsymmetricSignatureFormatter) CreateFromName (_FormatterAlgorithm);
		if (fmt == null)
			throw new InvalidCastException ("FormatterAlgorithm");
		fmt.SetKey (key);

		throw new NullReferenceException ("why?");

		// We must make a choice of the Formatter based on
		// the FormatterAlgorithm property (factory like CryptoConfig ?)
		// There are only 2 SignatureFormatter based on the
		// key algorithm (DSA or RSA) - but how does the 
		// KeyAlgorithm property string really looks like ?
	}
	
} // SignatureDescription
	
} // System.Security.Cryptography

