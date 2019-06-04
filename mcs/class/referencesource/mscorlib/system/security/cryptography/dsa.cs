// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// DSA.cs
//

namespace System.Security.Cryptography {
    using System.Text;
    using System.Runtime.Serialization;
    using System.Security.Util;
    using System.Globalization;
    using System.IO;
#if MONO
    using System.Buffers;
#endif
    using System.Diagnostics.Contracts;

    // DSAParameters is serializable so that one could pass the public parameters
    // across a remote call, but we explicitly make the private key X non-serializable
    // so you cannot accidently send it along with the public parameters.
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public struct DSAParameters {
        public byte[]      P;
        public byte[]      Q;
        public byte[]      G;
        public byte[]      Y;
        public byte[]      J;
        [NonSerialized] public byte[]      X;
        public byte[]      Seed;
        public int         Counter;
    }

[System.Runtime.InteropServices.ComVisible(true)]
    public abstract class DSA : AsymmetricAlgorithm
    {
        //
        //  Extending this class allows us to know that you are really implementing
        //  an DSA key.  This is required for anybody providing a new DSA key value
        //  implemention.
        //
        //  The class provides no methods, fields or anything else.  Its only purpose is
        //  as a heirarchy member for identification of the algorithm.
        //

        protected DSA() { }

        //
        // public methods
        //

        new static public DSA Create() {
#if FULL_AOT_RUNTIME
            return new System.Security.Cryptography.DSACryptoServiceProvider ();
#else
            return Create("System.Security.Cryptography.DSA");
#endif
        }

        new static public DSA Create(String algName) {
            return (DSA) CryptoConfig.CreateFromName(algName);
        }

        // DSA does not encode the algorithm identifier into the signature blob, therefore CreateSignature and
        // VerifySignature do not need the HashAlgorithmName value, only SignData and VerifyData do.
        abstract public byte[] CreateSignature(byte[] rgbHash);

        abstract public bool VerifySignature(byte[] rgbHash, byte[] rgbSignature); 

        protected virtual byte[] HashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm)
        {
            throw DerivedClassMustOverride();
        }

        protected virtual byte[] HashData(Stream data, HashAlgorithmName hashAlgorithm)
        {
            throw DerivedClassMustOverride();
        }

        public byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm)
        {
            if (data == null) { throw new ArgumentNullException("data"); }

            return SignData(data, 0, data.Length, hashAlgorithm);
        }

        public virtual byte[] SignData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (offset < 0 || offset > data.Length) { throw new ArgumentOutOfRangeException("offset"); }
            if (count < 0 || count > data.Length - offset) { throw new ArgumentOutOfRangeException("count"); }
            if (String.IsNullOrEmpty(hashAlgorithm.Name)) { throw HashAlgorithmNameNullOrEmpty(); }

            byte[] hash = HashData(data, offset, count, hashAlgorithm);
            return CreateSignature(hash);
        }

        public virtual byte[] SignData(Stream data, HashAlgorithmName hashAlgorithm)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (String.IsNullOrEmpty(hashAlgorithm.Name)) { throw HashAlgorithmNameNullOrEmpty(); }

            byte[] hash = HashData(data, hashAlgorithm);
            return CreateSignature(hash);
        }

        public bool VerifyData(byte[] data, byte[] signature, HashAlgorithmName hashAlgorithm)
        {
            if (data == null) { throw new ArgumentNullException("data"); }

            return VerifyData(data, 0, data.Length, signature, hashAlgorithm);
        }

        public virtual bool VerifyData(byte[] data, int offset, int count, byte[] signature, HashAlgorithmName hashAlgorithm)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (offset < 0 || offset > data.Length) { throw new ArgumentOutOfRangeException("offset"); }
            if (count < 0 || count > data.Length - offset) { throw new ArgumentOutOfRangeException("count"); }
            if (signature == null) { throw new ArgumentNullException("signature"); }
            if (String.IsNullOrEmpty(hashAlgorithm.Name)) { throw HashAlgorithmNameNullOrEmpty(); }

            byte[] hash = HashData(data, offset, count, hashAlgorithm);
            return VerifySignature(hash, signature);
        }

        public virtual bool VerifyData(Stream data, byte[] signature, HashAlgorithmName hashAlgorithm)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (signature == null) { throw new ArgumentNullException("signature"); }
            if (String.IsNullOrEmpty(hashAlgorithm.Name)) { throw HashAlgorithmNameNullOrEmpty(); }

            byte[] hash = HashData(data, hashAlgorithm);
            return VerifySignature(hash, signature);
        }

        // We can provide a default implementation of FromXmlString because we require 
        // every DSA implementation to implement ImportParameters
        // All we have to do here is parse the XML.

        public override void FromXmlString(String xmlString) {
            if (xmlString == null) throw new ArgumentNullException("xmlString");
            Contract.EndContractBlock();
            DSAParameters dsaParams = new DSAParameters();
            Parser p = new Parser(xmlString);
            SecurityElement topElement = p.GetTopElement();

            // P is always present
            String pString = topElement.SearchForTextOfLocalName("P");
            if (pString == null) {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString","DSA","P"));
            }
            dsaParams.P = Convert.FromBase64String(Utils.DiscardWhiteSpaces(pString));

            // Q is always present
            String qString = topElement.SearchForTextOfLocalName("Q");
            if (qString == null) {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString","DSA","Q"));
            }
            dsaParams.Q = Convert.FromBase64String(Utils.DiscardWhiteSpaces(qString));

            // G is always present
            String gString = topElement.SearchForTextOfLocalName("G");
            if (gString == null) {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString","DSA","G"));
            }
            dsaParams.G = Convert.FromBase64String(Utils.DiscardWhiteSpaces(gString));

            // Y is always present
            String yString = topElement.SearchForTextOfLocalName("Y");
            if (yString == null) {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString","DSA","Y"));
            }
            dsaParams.Y = Convert.FromBase64String(Utils.DiscardWhiteSpaces(yString));

            // J is optional
            String jString = topElement.SearchForTextOfLocalName("J");
            if (jString != null) dsaParams.J = Convert.FromBase64String(Utils.DiscardWhiteSpaces(jString));

            // X is optional -- private key if present
            String xString = topElement.SearchForTextOfLocalName("X");
            if (xString != null) dsaParams.X = Convert.FromBase64String(Utils.DiscardWhiteSpaces(xString));

            // Seed and PgenCounter are optional as a unit -- both present or both absent
            String seedString = topElement.SearchForTextOfLocalName("Seed");
            String pgenCounterString = topElement.SearchForTextOfLocalName("PgenCounter");
            if ((seedString != null) && (pgenCounterString != null)) {
                dsaParams.Seed = Convert.FromBase64String(Utils.DiscardWhiteSpaces(seedString));
                dsaParams.Counter = Utils.ConvertByteArrayToInt(Convert.FromBase64String(Utils.DiscardWhiteSpaces(pgenCounterString)));
            } else if ((seedString != null) || (pgenCounterString != null)) {
                if (seedString == null) {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString","DSA","Seed"));
                } else {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidFromXmlString","DSA","PgenCounter"));
                }
            }

            ImportParameters(dsaParams);
        }

        // We can provide a default implementation of ToXmlString because we require 
        // every DSA implementation to implement ImportParameters
        // If includePrivateParameters is false, this is just an XMLDSIG DSAKeyValue
        // clause.  If includePrivateParameters is true, then we extend DSAKeyValue with 
        // the other (private) elements.

        public override String ToXmlString(bool includePrivateParameters) {
            // From the XMLDSIG spec, RFC 3075, Section 6.4.1, a DSAKeyValue looks like this:
            /* 
               <element name="DSAKeyValue"> 
                 <complexType> 
                   <sequence>
                     <sequence>
                       <element name="P" type="ds:CryptoBinary"/> 
                       <element name="Q" type="ds:CryptoBinary"/> 
                       <element name="G" type="ds:CryptoBinary"/> 
                       <element name="Y" type="ds:CryptoBinary"/> 
                       <element name="J" type="ds:CryptoBinary" minOccurs="0"/> 
                     </sequence>
                     <sequence minOccurs="0">
                       <element name="Seed" type="ds:CryptoBinary"/> 
                       <element name="PgenCounter" type="ds:CryptoBinary"/> 
                     </sequence>
                   </sequence>
                 </complexType>
               </element>
            */
            // we extend appropriately for private component X
            DSAParameters dsaParams = this.ExportParameters(includePrivateParameters);
            StringBuilder sb = new StringBuilder();
            sb.Append("<DSAKeyValue>");
            // Add P, Q, G and Y
            sb.Append("<P>"+Convert.ToBase64String(dsaParams.P)+"</P>");
            sb.Append("<Q>"+Convert.ToBase64String(dsaParams.Q)+"</Q>");
            sb.Append("<G>"+Convert.ToBase64String(dsaParams.G)+"</G>");
            sb.Append("<Y>"+Convert.ToBase64String(dsaParams.Y)+"</Y>");
            // Add optional components if present
            if (dsaParams.J != null) {
                sb.Append("<J>"+Convert.ToBase64String(dsaParams.J)+"</J>");
            }
            if ((dsaParams.Seed != null)) {  // note we assume counter is correct if Seed is present
                sb.Append("<Seed>"+Convert.ToBase64String(dsaParams.Seed)+"</Seed>");
                sb.Append("<PgenCounter>"+Convert.ToBase64String(Utils.ConvertIntToByteArray(dsaParams.Counter))+"</PgenCounter>");
            }

            if (includePrivateParameters) {
                // Add the private component
                sb.Append("<X>"+Convert.ToBase64String(dsaParams.X)+"</X>");
            } 
            sb.Append("</DSAKeyValue>");
            return(sb.ToString());
        }

        abstract public DSAParameters ExportParameters(bool includePrivateParameters);

        abstract public void ImportParameters(DSAParameters parameters);

        private static Exception DerivedClassMustOverride()
        {
            return new NotImplementedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
        }

        internal static Exception HashAlgorithmNameNullOrEmpty()
        {
            return new ArgumentException(Environment.GetResourceString("Cryptography_HashAlgorithmNameNullOrEmpty"), "hashAlgorithm");
        }

#if MONO
        // these methods were copied from CoreFX for NS2.1 support
        public static DSA Create(int keySizeInBits)
        {
            DSA dsa = Create();

            try
            {
                dsa.KeySize = keySizeInBits;
                return dsa;
            }
            catch
            {
                dsa.Dispose();
                throw;
            }
        }

        public static DSA Create(DSAParameters parameters)
        {
            DSA dsa = Create();

            try
            {
                dsa.ImportParameters(parameters);
                return dsa;
            }
            catch
            {
                dsa.Dispose();
                throw;
            }
        }

        public virtual bool TryCreateSignature(ReadOnlySpan<byte> hash, Span<byte> destination, out int bytesWritten)
        {
            byte[] sig = CreateSignature(hash.ToArray());
            if (sig.Length <= destination.Length)
            {
                new ReadOnlySpan<byte>(sig).CopyTo(destination);
                bytesWritten = sig.Length;
                return true;
            }
            else
            {
                bytesWritten = 0;
                return false;
            }
        }

        protected virtual bool TryHashData(ReadOnlySpan<byte> data, Span<byte> destination, HashAlgorithmName hashAlgorithm, out int bytesWritten)
        {
            byte[] array = ArrayPool<byte>.Shared.Rent(data.Length);
            try
            {
                data.CopyTo(array);
                byte[] hash = HashData(array, 0, data.Length, hashAlgorithm);
                if (destination.Length >= hash.Length)
                {
                    new ReadOnlySpan<byte>(hash).CopyTo(destination);
                    bytesWritten = hash.Length;
                    return true;
                }
                else
                {
                    bytesWritten = 0;
                    return false;
                }
            }
            finally
            {
                Array.Clear(array, 0, data.Length);
                ArrayPool<byte>.Shared.Return(array);
            }
        }

        public virtual bool TrySignData(ReadOnlySpan<byte> data, Span<byte> destination, HashAlgorithmName hashAlgorithm, out int bytesWritten)
        {
            if (string.IsNullOrEmpty(hashAlgorithm.Name))
            {
                throw HashAlgorithmNameNullOrEmpty();
            }

            if (TryHashData(data, destination, hashAlgorithm, out int hashLength) &&
                TryCreateSignature(destination.Slice(0, hashLength), destination, out bytesWritten))
            {
                return true;
            }

            bytesWritten = 0;
            return false;
        }

        public virtual bool VerifyData(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature, HashAlgorithmName hashAlgorithm)
        {
            if (string.IsNullOrEmpty(hashAlgorithm.Name))
            {
                throw HashAlgorithmNameNullOrEmpty();
            }

            for (int i = 256; ; i = checked(i * 2))
            {
                int hashLength = 0;
                byte[] hash = ArrayPool<byte>.Shared.Rent(i);
                try
                {
                    if (TryHashData(data, hash, hashAlgorithm, out hashLength))
                    {
                        return VerifySignature(new ReadOnlySpan<byte>(hash, 0, hashLength), signature);
                    }
                }
                finally
                {
                    Array.Clear(hash, 0, hashLength);
                    ArrayPool<byte>.Shared.Return(hash);
                }
            }
        }

        public virtual bool VerifySignature(ReadOnlySpan<byte> hash, ReadOnlySpan<byte> signature) =>
            VerifySignature(hash.ToArray(), signature.ToArray());
#endif
    }
}
