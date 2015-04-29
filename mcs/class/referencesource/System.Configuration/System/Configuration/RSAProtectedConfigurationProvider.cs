//------------------------------------------------------------------------------
// <copyright file="RsaProtectedConfigurationProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration
{
    using System.Collections.Specialized;
    using System.Runtime.Serialization;
    using System.Configuration.Provider;
    using System.Xml;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public sealed class RsaProtectedConfigurationProvider : ProtectedConfigurationProvider
    {
        // Note: this name has to match the name used in RegiisUtility 
        const string DefaultRsaKeyContainerName = "NetFrameworkConfigurationKey";
        
        public override XmlNode Decrypt(XmlNode encryptedNode)
        {
            XmlDocument                 xmlDocument = new XmlDocument();
            EncryptedXml                exml        = null;
            RSACryptoServiceProvider    rsa         = GetCryptoServiceProvider(false, true);

            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(encryptedNode.OuterXml);
            exml = new EncryptedXml(xmlDocument);
            exml.AddKeyNameMapping(_KeyName, rsa);
            exml.DecryptDocument();
            rsa.Clear();
            return xmlDocument.DocumentElement;
        }

        public override XmlNode Encrypt(XmlNode node)
        {
            XmlDocument         xmlDocument;
            EncryptedXml        exml;
            byte[]              rgbOutput;
            EncryptedData       ed;
            KeyInfoName         kin;
            SymmetricAlgorithm  symAlg;
            EncryptedKey        ek;
            KeyInfoEncryptedKey kek;
            XmlElement          inputElement;
            RSACryptoServiceProvider rsa = GetCryptoServiceProvider(false, false);


            // Encrypt the node with the new key
            xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml("<foo>"+ node.OuterXml+ "</foo>");
            exml = new EncryptedXml(xmlDocument);
            inputElement = xmlDocument.DocumentElement;

            // Create a new 3DES key
            symAlg = new TripleDESCryptoServiceProvider();
            byte[] rgbKey1 = GetRandomKey();
            symAlg.Key = rgbKey1;
            symAlg.Mode = CipherMode.ECB;
            symAlg.Padding = PaddingMode.PKCS7;
            rgbOutput = exml.EncryptData(inputElement, symAlg, true);
            ed = new EncryptedData();
            ed.Type = EncryptedXml.XmlEncElementUrl;
            ed.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncTripleDESUrl);
            ed.KeyInfo = new KeyInfo();

            ek = new EncryptedKey();
            ek.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSA15Url);
            ek.KeyInfo = new KeyInfo();
            ek.CipherData = new CipherData();
            ek.CipherData.CipherValue = EncryptedXml.EncryptKey(symAlg.Key, rsa, UseOAEP);
            kin = new KeyInfoName();
            kin.Value = _KeyName;
            ek.KeyInfo.AddClause(kin);
            kek = new KeyInfoEncryptedKey(ek);
            ed.KeyInfo.AddClause(kek);
            ed.CipherData = new CipherData();
            ed.CipherData.CipherValue = rgbOutput;
            EncryptedXml.ReplaceElement(inputElement, ed, true);
                // Get node from the document
            foreach (XmlNode node2 in xmlDocument.ChildNodes)
                if (node2.NodeType == XmlNodeType.Element)
                    foreach (XmlNode node3 in node2.ChildNodes) // node2 is the "foo" node
                        if (node3.NodeType == XmlNodeType.Element)
                            return node3; // node3 is the "EncryptedData" node
                return null;

        }

        public void AddKey(int keySize, bool exportable)
        {
            RSACryptoServiceProvider rsa = GetCryptoServiceProvider(exportable, false);
            rsa.KeySize = keySize;
            rsa.PersistKeyInCsp = true;
            rsa.Clear();
        }

        public void DeleteKey()
        {
            RSACryptoServiceProvider rsa = GetCryptoServiceProvider(false, true);
            rsa.PersistKeyInCsp = false;
            rsa.Clear();
        }
        public void ImportKey(string xmlFileName, bool exportable)
        {
            RSACryptoServiceProvider rsa = GetCryptoServiceProvider(exportable, false);
            rsa.FromXmlString(File.ReadAllText(xmlFileName));
            rsa.PersistKeyInCsp = true;
            rsa.Clear();
        }

        public void ExportKey(string xmlFileName, bool includePrivateParameters)
        {
            RSACryptoServiceProvider rsa = GetCryptoServiceProvider(false, false);
            string xmlString = rsa.ToXmlString(includePrivateParameters);
            File.WriteAllText(xmlFileName, xmlString);
            rsa.Clear();
        }

        public string   KeyContainerName    { get { return _KeyContainerName; } }
        public string   CspProviderName     { get { return _CspProviderName; } }
        public bool     UseMachineContainer { get { return _UseMachineContainer; } }
        public bool UseOAEP { get { return _UseOAEP; } }
        public override void Initialize(string name, NameValueCollection configurationValues)
        {
            base.Initialize(name, configurationValues);

            _KeyName = "Rsa Key";
            _KeyContainerName = configurationValues["keyContainerName"];
            configurationValues.Remove("keyContainerName");
            if (_KeyContainerName == null || _KeyContainerName.Length < 1)
                _KeyContainerName = DefaultRsaKeyContainerName;

            _CspProviderName = configurationValues["cspProviderName"];
            configurationValues.Remove("cspProviderName");
            _UseMachineContainer = GetBooleanValue(configurationValues, "useMachineContainer", true);
            _UseOAEP = GetBooleanValue(configurationValues, "useOAEP", false);
            if (configurationValues.Count > 0)
                throw new ConfigurationErrorsException(SR.GetString(SR.Unrecognized_initialization_value, configurationValues.GetKey(0)));
        }


        private string _KeyName;
        private string _KeyContainerName;
        private string _CspProviderName;
        private bool _UseMachineContainer;
        private bool _UseOAEP;

        public RSAParameters RsaPublicKey { get { return GetCryptoServiceProvider(false, false).ExportParameters(false); } }

        private RSACryptoServiceProvider GetCryptoServiceProvider(bool exportable, bool keyMustExist)
        {
            try {
                CspParameters csp = new CspParameters();
                csp.KeyContainerName = KeyContainerName;
                csp.KeyNumber = 1;
                csp.ProviderType = 1; // Dev10 Bug #548719: Explicitly require "RSA Full (Signature and Key Exchange)"

                if (CspProviderName != null && CspProviderName.Length > 0)
                    csp.ProviderName = CspProviderName;

                if (UseMachineContainer)
                    csp.Flags |= CspProviderFlags.UseMachineKeyStore;
                if (!exportable && !keyMustExist)
                    csp.Flags |= CspProviderFlags.UseNonExportableKey;
                if (keyMustExist)
                    csp.Flags |= CspProviderFlags.UseExistingKey;

                return new RSACryptoServiceProvider(csp);

            } catch {
                ThrowBetterException(keyMustExist);

                // If a better exception can't be found, this will propagate
                // the original one
                throw;
            }
        }
        private byte[] GetRandomKey()
        {
            byte [] buf = new byte[24];
            (new RNGCryptoServiceProvider()).GetBytes(buf);
            return buf;
        }

        private void ThrowBetterException(bool keyMustExist)
        {
            SafeCryptContextHandle hProv = null;
            int success = 0;
            try {
                success = UnsafeNativeMethods.CryptAcquireContext(out hProv, KeyContainerName, CspProviderName, PROV_Rsa_FULL, UseMachineContainer ? CRYPT_MACHINE_KEYSET : 0);
                if (success != 0)
                    return; // propagate original exception

                int hr = Marshal.GetHRForLastWin32Error();
                if (hr == HResults.NteBadKeySet && !keyMustExist) {
                    return; // propagate original exception
                }

                switch (hr) {
                    case HResults.NteBadKeySet:
                    case HResults.Win32AccessDenied:
                    case HResults.Win32InvalidHandle:
                        throw new ConfigurationErrorsException(SR.GetString(SR.Key_container_doesnt_exist_or_access_denied));

                    default:
                        Marshal.ThrowExceptionForHR(hr);
                        break;
                }
            } finally {
                if (!(hProv == null || hProv.IsInvalid))
                    hProv.Dispose();
            }
        }
        const uint PROV_Rsa_FULL = 1;
        const uint CRYPT_MACHINE_KEYSET = 0x00000020;

        private static bool GetBooleanValue(NameValueCollection configurationValues, string valueName, bool defaultValue) {
            string s = configurationValues[valueName];
            if (s == null)
                return defaultValue;
            configurationValues.Remove(valueName);
            if (s == "true")
                return true;
            if (s == "false")
                return false;
            throw new ConfigurationErrorsException(SR.GetString(SR.Config_invalid_boolean_attribute, valueName));
        }
    }
}
