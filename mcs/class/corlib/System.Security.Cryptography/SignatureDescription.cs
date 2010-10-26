//
// System.Security.Cryptography SignatureDescription Class implementation
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

// Notes:
// There seems to be some (internal?) class inheriting from SignatureDescription
// http://www.csharpfriends.com/Members/Main/Classes/get_class.aspx?assembly=mscorlib,%20Version=1.0.3300.0,%20Culture=neutral,%20PublicKeyToken=b77a5c561934e089&namespace=System.Security.Cryptography&class=SignatureDescription
// Those 2 classes are returned by CryptoConfig.CreateFromName and used in XMLDSIG

using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Security.Cryptography {
	
#if NET_2_0
[ComVisible (true)]
#endif
public class SignatureDescription {

	private string _DeformatterAlgorithm;
	private string _DigestAlgorithm;		
	private string _FormatterAlgorithm;		
	private string _KeyAlgorithm;		

	public SignatureDescription ()
	{
	}
	
	#if !DISABLE_SECURITY
	/// LAMESPEC: ArgumentNullException is thrown (not CryptographicException)
	public SignatureDescription (SecurityElement el) 
	{
		if (el == null)
			throw new ArgumentNullException ("el");

		// thanksfully documented in VS.NET 2005
		SecurityElement child = el.SearchForChildByTag ("Deformatter");
		_DeformatterAlgorithm = ((child == null) ? null : child.Text);

		child = el.SearchForChildByTag ("Digest");
		_DigestAlgorithm = ((child == null) ? null : child.Text);

		child = el.SearchForChildByTag ("Formatter");
		_FormatterAlgorithm = ((child == null) ? null : child.Text);

		child = el.SearchForChildByTag ("Key");
		_KeyAlgorithm = ((child == null) ? null : child.Text);
	}
	#endif

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
}

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
	
}
