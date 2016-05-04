// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Diagnostics.Contracts;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Utility class to convert NCrypt keys into XML and back using a format similar to the one described
    ///     in RFC 4050 (http://www.ietf.org/rfc/rfc4050.txt).
    /// 
    ///     #RFC4050ECKeyFormat
    /// 
    ///     The format looks similar to the following:
    /// 
    ///         <ECDSAKeyValue xmlns="http://www.w3.org/2001/04/xmldsig-more#">
    ///             <DomainParameters>
    ///                 <NamedCurve URN="urn:oid:1.3.132.0.35" />
    ///             </DomainParameters>
    ///             <PublicKey>
    ///                 <X Value="0123456789..." xsi:type="PrimeFieldElemType" />
    ///                 <Y Value="0123456789..." xsi:type="PrimeFieldElemType" />
    ///             </PublicKey>
    ///         </ECDSAKeyValue>
    /// </summary>
    internal static class Rfc4050KeyFormatter {
        private const string DomainParametersRoot = "DomainParameters";
        private const string ECDHRoot = "ECDHKeyValue";
        private const string ECDsaRoot = "ECDSAKeyValue";
        private const string NamedCurveElement = "NamedCurve";
        private const string Namespace = "http://www.w3.org/2001/04/xmldsig-more#";
        private const string PublicKeyRoot = "PublicKey";
        private const string UrnAttribute = "URN";
        private const string ValueAttribute = "Value";
        private const string XElement = "X";
        private const string YElement = "Y";

        private const string XsiTypeAttribute = "type";
        private const string XsiTypeAttributeValue = "PrimeFieldElemType";
        private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        private const string XsiNamespacePrefix = "xsi";

        private const string Prime256CurveUrn = "urn:oid:1.2.840.10045.3.1.7";
        private const string Prime384CurveUrn = "urn:oid:1.3.132.0.34";
        private const string Prime521CurveUrn = "urn:oid:1.3.132.0.35";

        /// <summary>
        ///     Restore a key from XML
        /// </summary>
        internal static CngKey FromXml(string xml) {
            Contract.Requires(xml != null);
            Contract.Ensures(Contract.Result<CngKey>() != null);

            // Load the XML into an XPathNavigator to access sub elements
            using (TextReader textReader = new StringReader(xml))
            using (XmlTextReader xmlReader = new XmlTextReader(textReader)) {
                XPathDocument document = new XPathDocument(xmlReader);
                XPathNavigator navigator = document.CreateNavigator();
                
                // Move into the root element - we don't do a specific namespace check here for compatibility
                // with XML that Windows generates.
                if (!navigator.MoveToFirstChild()) {
                    throw new ArgumentException(SR.GetString(SR.Cryptography_MissingDomainParameters));
                }

                // First figure out which algorithm this key belongs to
                CngAlgorithm algorithm = ReadAlgorithm(navigator);

                // Then read out the public key value
                if (!navigator.MoveToNext(XPathNodeType.Element)) {
                    throw new ArgumentException(SR.GetString(SR.Cryptography_MissingPublicKey));
                }

                BigInteger x;
                BigInteger y;
                ReadPublicKey(navigator, out x, out y);

                // Finally, convert them into a key blob to import into a CngKey
                byte[] keyBlob = NCryptNative.BuildEccPublicBlob(algorithm.Algorithm, x, y);
                return CngKey.Import(keyBlob, CngKeyBlobFormat.EccPublicBlob);
            }
        }

        /// <summary>
        ///     Map a curve URN to the size of the key associated with the curve
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The parameters to the exception are in the correct order")]
        private static int GetKeySize(string urn) {
            Contract.Requires(!String.IsNullOrEmpty(urn));
            Contract.Ensures(Contract.Result<int>() > 0);

            switch (urn) {
                case Prime256CurveUrn:
                    return 256;

                case Prime384CurveUrn:
                    return 384;

                case Prime521CurveUrn:
                    return 521;

                default:
                    throw new ArgumentException(SR.GetString(SR.Cryptography_UnknownEllipticCurve), "algorithm");
            }
        }

        /// <summary>
        ///     Get the OID which represents an elliptic curve
        /// </summary>
        private static string GetCurveUrn(CngAlgorithm algorithm) {
            Contract.Requires(algorithm != null);

            if (algorithm == CngAlgorithm.ECDsaP256 || algorithm == CngAlgorithm.ECDiffieHellmanP256) {
                return Prime256CurveUrn;
            }
            else if (algorithm == CngAlgorithm.ECDsaP384 || algorithm == CngAlgorithm.ECDiffieHellmanP384) {
                return Prime384CurveUrn;
            }
            else if (algorithm == CngAlgorithm.ECDsaP521 || algorithm == CngAlgorithm.ECDiffieHellmanP521) {
                return Prime521CurveUrn;
            }
            else {
                throw new ArgumentException(SR.GetString(SR.Cryptography_UnknownEllipticCurve), "algorithm");
            }
        }

        /// <summary>
        ///     Determine which ECC algorithm the key refers to
        /// </summary>
        private static CngAlgorithm ReadAlgorithm(XPathNavigator navigator) {
            Contract.Requires(navigator != null);
            Contract.Ensures(Contract.Result<CngAlgorithm>() != null);

            if (navigator.NamespaceURI != Namespace) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_UnexpectedXmlNamespace,
                                                         navigator.NamespaceURI,
                                                         Namespace));
            }

            //
            // The name of the root element determines which algorithm to use, while the DomainParameters
            // element specifies which curve we should be using.
            //

            bool isDHKey = navigator.Name == ECDHRoot;
            bool isDsaKey = navigator.Name == ECDsaRoot;

            if (!isDHKey && !isDsaKey) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_UnknownEllipticCurveAlgorithm));
            }

            // Move into the DomainParameters element
            if (!navigator.MoveToFirstChild() || navigator.Name != DomainParametersRoot) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_MissingDomainParameters));
            }

            // Now move into the NamedCurve element
            if (!navigator.MoveToFirstChild() || navigator.Name != NamedCurveElement) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_MissingDomainParameters));
            }

            // And read its URN value
            if (!navigator.MoveToFirstAttribute() || navigator.Name != UrnAttribute || String.IsNullOrEmpty(navigator.Value)) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_MissingDomainParameters));
            }

            int keySize = GetKeySize(navigator.Value);

            // position the navigator at the end of the domain parameters
            navigator.MoveToParent();   // NamedCurve
            navigator.MoveToParent();   // DomainParameters

            //
            // Given the algorithm type and key size, we can now map back to a CNG algorithm ID
            //

            if (isDHKey) {
                if (keySize == 256) {
                    return CngAlgorithm.ECDiffieHellmanP256;
                }
                else if (keySize == 384) {
                    return CngAlgorithm.ECDiffieHellmanP384;
                }
                else {
                    Debug.Assert(keySize == 521, "keySize == 521");
                    return CngAlgorithm.ECDiffieHellmanP521;
                }
            }
            else {
                Debug.Assert(isDsaKey, "isDsaKey");

                if (keySize == 256) {
                    return CngAlgorithm.ECDsaP256;
                }
                else if (keySize == 384) {
                    return CngAlgorithm.ECDsaP384;
                }
                else {
                    Debug.Assert(keySize == 521, "keySize == 521");
                    return CngAlgorithm.ECDsaP521;
                }
            }
        }

        /// <summary>
        ///     Read the x and y components of the public key
        /// </summary>
        private static void ReadPublicKey(XPathNavigator navigator, out BigInteger x, out BigInteger y) {
            Contract.Requires(navigator != null);

            if (navigator.NamespaceURI != Namespace) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_UnexpectedXmlNamespace,
                                                         navigator.NamespaceURI,
                                                         Namespace));
            }

            if (navigator.Name != PublicKeyRoot) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_MissingPublicKey));
            }

            // First get the x parameter
            if (!navigator.MoveToFirstChild() || navigator.Name != XElement) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_MissingPublicKey));
            }
            if (!navigator.MoveToFirstAttribute() || navigator.Name != ValueAttribute || String.IsNullOrEmpty(navigator.Value)) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_MissingPublicKey));
            }

            x = BigInteger.Parse(navigator.Value, CultureInfo.InvariantCulture);
            navigator.MoveToParent();

            // Then the y parameter
            if (!navigator.MoveToNext(XPathNodeType.Element) || navigator.Name != YElement) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_MissingPublicKey));
            }
            if (!navigator.MoveToFirstAttribute() || navigator.Name != ValueAttribute || String.IsNullOrEmpty(navigator.Value)) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_MissingPublicKey));
            }

            y = BigInteger.Parse(navigator.Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Serialize out information about the elliptic curve
        /// </summary>
        private static void WriteDomainParameters(XmlWriter writer, CngKey key) {
            Contract.Requires(writer != null);
            Contract.Requires(key != null && (key.AlgorithmGroup == CngAlgorithmGroup.ECDsa || key.AlgorithmGroup == CngAlgorithmGroup.ECDiffieHellman));

            writer.WriteStartElement(DomainParametersRoot);

            // We always use OIDs for the named prime curves
            writer.WriteStartElement(NamedCurveElement);
            writer.WriteAttributeString(UrnAttribute, GetCurveUrn(key.Algorithm));
            writer.WriteEndElement();   // </NamedCurve>

            writer.WriteEndElement();   // </DomainParameters>
        }

        private static void WritePublicKeyValue(XmlWriter writer, CngKey key) {
            Contract.Requires(writer != null);
            Contract.Requires(key != null && (key.AlgorithmGroup == CngAlgorithmGroup.ECDsa || key.AlgorithmGroup == CngAlgorithmGroup.ECDiffieHellman));

            writer.WriteStartElement(PublicKeyRoot);

            byte[] exportedKey = key.Export(CngKeyBlobFormat.EccPublicBlob);
            BigInteger x;
            BigInteger y;
            NCryptNative.UnpackEccPublicBlob(exportedKey, out x, out y);

            writer.WriteStartElement(XElement);
            writer.WriteAttributeString(ValueAttribute, x.ToString("R", CultureInfo.InvariantCulture));
            writer.WriteAttributeString(XsiNamespacePrefix, XsiTypeAttribute, XsiNamespace, XsiTypeAttributeValue);
            writer.WriteEndElement();   // </X>

            writer.WriteStartElement(YElement);
            writer.WriteAttributeString(ValueAttribute, y.ToString("R", CultureInfo.InvariantCulture));
            writer.WriteAttributeString(XsiNamespacePrefix, XsiTypeAttribute, XsiNamespace, XsiTypeAttributeValue);
            writer.WriteEndElement();   // </Y>

            writer.WriteEndElement();   // </PublicKey>
        }

        /// <summary>
        ///     Convert a key to XML
        /// </summary>
        internal static string ToXml(CngKey key) {
            Contract.Requires(key != null && (key.AlgorithmGroup == CngAlgorithmGroup.ECDsa || key.AlgorithmGroup == CngAlgorithmGroup.ECDiffieHellman));
            Contract.Ensures(Contract.Result<String>() != null);

            StringBuilder keyXml = new StringBuilder();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.OmitXmlDeclaration = true;

            using (XmlWriter writer = XmlWriter.Create(keyXml, settings)) {
                // The root element depends upon the type of key
                string rootElement = key.AlgorithmGroup == CngAlgorithmGroup.ECDsa ? ECDsaRoot : ECDHRoot;
                writer.WriteStartElement(rootElement, Namespace);

                WriteDomainParameters(writer, key);
                WritePublicKeyValue(writer, key);

                writer.WriteEndElement();   // root element
            }

            return keyXml.ToString();
        }
    }
}
