//------------------------------------------------------------------------------
// <copyright file="DpapiProtectedConfigurationProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration
{
    using System.Collections.Specialized;
    using System.Runtime.Serialization;
    using System.Configuration.Provider;
    using System.Xml;
    using System.Text;
    using  System.Runtime.InteropServices;
    using Microsoft.Win32;
    using System.Security.Permissions;
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.CompilerServices;

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public sealed class DpapiProtectedConfigurationProvider : ProtectedConfigurationProvider
    {
        public override XmlNode Decrypt(XmlNode encryptedNode)
        {
            if (encryptedNode.NodeType != XmlNodeType.Element ||
                encryptedNode.Name != "EncryptedData") {
                throw new ConfigurationErrorsException(SR.GetString(SR.DPAPI_bad_data));
            }

            XmlNode cipherNode = TraverseToChild(encryptedNode, "CipherData", false);
            if (cipherNode == null)
                throw new ConfigurationErrorsException(SR.GetString(SR.DPAPI_bad_data));

            XmlNode cipherValue = TraverseToChild(cipherNode, "CipherValue", true);
            if (cipherValue == null)
                throw new ConfigurationErrorsException(SR.GetString(SR.DPAPI_bad_data));

            string encText = cipherValue.InnerText;
            if (encText == null)
                throw new ConfigurationErrorsException(SR.GetString(SR.DPAPI_bad_data));

            string decText = DecryptText(encText);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(decText);
            return xmlDocument.DocumentElement;
        }

        public override XmlNode Encrypt(XmlNode node)
        {
            string text = node.OuterXml;
            string encText = EncryptText(text);
            string pre = @"<EncryptedData><CipherData><CipherValue>";
            string post = @"</CipherValue></CipherData></EncryptedData>";
            string xmlText = pre + encText + post;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(xmlText);
            return xmlDocument.DocumentElement;
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private string EncryptText(string clearText)
        {
            if (clearText == null || clearText.Length < 1)
                return clearText;

            DATA_BLOB inputData, entData, outputData;
            SafeNativeMemoryHandle safeInputDataHandle = new SafeNativeMemoryHandle();
            SafeNativeMemoryHandle safeOutputDataHandle = new SafeNativeMemoryHandle(true);
            SafeNativeMemoryHandle safeEntDataHandle = new SafeNativeMemoryHandle();

            inputData.pbData = entData.pbData = outputData.pbData = IntPtr.Zero;
            inputData.cbData = entData.cbData = outputData.cbData = 0;
            try {

                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                }
                finally {
                    inputData = PrepareDataBlob(clearText);
                    safeInputDataHandle.SetDataHandle(inputData.pbData);

                    entData = PrepareDataBlob(_KeyEntropy);
                    safeEntDataHandle.SetDataHandle(entData.pbData);
                }

                CRYPTPROTECT_PROMPTSTRUCT   prompt      = PreparePromptStructure();
                UInt32                      dwFlags     = CRYPTPROTECT_UI_FORBIDDEN;
                if (UseMachineProtection)
                    dwFlags |= CRYPTPROTECT_LOCAL_MACHINE;
                bool success = false;

                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                }
                finally {
                    success = UnsafeNativeMethods.CryptProtectData(ref inputData, "", ref entData,
                                                                    IntPtr.Zero, ref prompt,
                                                                    dwFlags, ref outputData);
                    safeOutputDataHandle.SetDataHandle(outputData.pbData);
                }
                if (!success || outputData.pbData == IntPtr.Zero) {
                    outputData.pbData = IntPtr.Zero;
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
                byte[] buf = new byte[outputData.cbData];
                Marshal.Copy(outputData.pbData, buf, 0, buf.Length);
                return Convert.ToBase64String(buf);
            } finally {
                if (!(safeOutputDataHandle == null || safeOutputDataHandle.IsInvalid)) {
                    safeOutputDataHandle.Dispose();
                    outputData.pbData = IntPtr.Zero;
                }
                if (!(safeEntDataHandle == null || safeEntDataHandle.IsInvalid)) {
                    safeEntDataHandle.Dispose();
                    entData.pbData = IntPtr.Zero;
                }
                if (!(safeInputDataHandle == null || safeInputDataHandle.IsInvalid)) {
                    safeInputDataHandle.Dispose();
                    inputData.pbData = IntPtr.Zero;
                }
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private string DecryptText(string encText)
        {
            if (encText == null || encText.Length < 1)
                return encText;

            DATA_BLOB inputData, entData, outputData;
            SafeNativeMemoryHandle safeInputDataHandle = new SafeNativeMemoryHandle();
            SafeNativeMemoryHandle safeOutputDataHandle = new SafeNativeMemoryHandle(true);
            SafeNativeMemoryHandle safeEntDataHandle = new SafeNativeMemoryHandle();

            inputData.pbData = entData.pbData = outputData.pbData = IntPtr.Zero;
            inputData.cbData = entData.cbData = outputData.cbData = 0;

            try {
                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                }
                finally {
                    inputData = PrepareDataBlob(Convert.FromBase64String(encText));
                    safeInputDataHandle.SetDataHandle(inputData.pbData);

                    entData = PrepareDataBlob(_KeyEntropy);
                    safeEntDataHandle.SetDataHandle(entData.pbData);
                }

                CRYPTPROTECT_PROMPTSTRUCT   prompt      = PreparePromptStructure();
                UInt32                      dwFlags     = CRYPTPROTECT_UI_FORBIDDEN;

                if (UseMachineProtection)
                    dwFlags |= CRYPTPROTECT_LOCAL_MACHINE;
                bool success = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                }
                finally {
                    success = UnsafeNativeMethods.CryptUnprotectData(ref inputData, IntPtr.Zero,
                                                                      ref entData, IntPtr.Zero,
                                                                      ref prompt, dwFlags, ref outputData);
                    safeOutputDataHandle.SetDataHandle(outputData.pbData);
                }

                if (!success || outputData.pbData == IntPtr.Zero) {
                    outputData.pbData = IntPtr.Zero;
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
                byte[] buf = new byte[outputData.cbData];
                Marshal.Copy(outputData.pbData, buf, 0, buf.Length);
                return Encoding.Unicode.GetString(buf);
            } finally {
                if (!(safeOutputDataHandle == null || safeOutputDataHandle.IsInvalid)) {
                    safeOutputDataHandle.Dispose();
                    outputData.pbData = IntPtr.Zero;
                }
                if (!(safeEntDataHandle == null || safeEntDataHandle.IsInvalid)) {
                    safeEntDataHandle.Dispose();
                    entData.pbData = IntPtr.Zero;
                }
                if (!(safeInputDataHandle == null || safeInputDataHandle.IsInvalid)) {
                    safeInputDataHandle.Dispose();
                    inputData.pbData = IntPtr.Zero;
                }
            }
        }

        public bool     UseMachineProtection   { get { return _UseMachineProtection; }}
        //private virtual string   KeyEntropy    { get { return _KeyEntropy; } }

        public override void Initialize(string name, NameValueCollection configurationValues)
        {
            base.Initialize(name, configurationValues);
            _UseMachineProtection = GetBooleanValue(configurationValues, "useMachineProtection", true);
            _KeyEntropy = configurationValues["keyEntropy"];
            configurationValues.Remove("keyEntropy");
            if (configurationValues.Count > 0)
                throw new ConfigurationErrorsException(SR.GetString(SR.Unrecognized_initialization_value, configurationValues.GetKey(0)));
        }

        private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
        private const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;
        private bool   _UseMachineProtection = true;
        private string  _KeyEntropy;

        private static XmlNode TraverseToChild(XmlNode node, string name, bool onlyChild)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                    continue;

                if (child.Name == name)
                    return child; // found it!

                if (onlyChild)
                    return null;
            }

            return null;
        }

        private static DATA_BLOB PrepareDataBlob(byte[] buf)
        {
            if (buf == null)
                buf = new byte[0];
            DATA_BLOB db = new DATA_BLOB();
            db.cbData = buf.Length;
            db.pbData = Marshal.AllocHGlobal(db.cbData);
            Marshal.Copy(buf, 0, db.pbData, db.cbData);
            return db;
        }
        private static DATA_BLOB PrepareDataBlob(string s)
        {
            return PrepareDataBlob((s != null) ? Encoding.Unicode.GetBytes(s) : new byte[0]);
        }
        private static CRYPTPROTECT_PROMPTSTRUCT PreparePromptStructure()
        {
            CRYPTPROTECT_PROMPTSTRUCT cps = new CRYPTPROTECT_PROMPTSTRUCT();
            cps.cbSize = Marshal.SizeOf(typeof(CRYPTPROTECT_PROMPTSTRUCT));
            cps.dwPromptFlags = 0;
            cps.hwndApp = IntPtr.Zero;
            cps.szPrompt = null;
            return cps;
        }
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
