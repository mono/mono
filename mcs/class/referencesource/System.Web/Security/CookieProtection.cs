//------------------------------------------------------------------------------
// <copyright file="CookieProtection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security 
{
    using System;
    using System.Web.Configuration;
    using System.Web.Security.Cryptography;
    

    public enum CookieProtection
    {

        None, Validation, Encryption, All
    }

    internal class CookieProtectionHelper
    {
        internal static string Encode (CookieProtection cookieProtection, byte [] buf, Purpose purpose)
        {
            if (AspNetCryptoServiceProvider.Instance.IsDefaultProvider) {
                // If we're configured to go through the new crypto routines, do so.
                ICryptoService cryptoService = AspNetCryptoServiceProvider.Instance.GetCryptoService(purpose);
                return HttpServerUtility.UrlTokenEncode(cryptoService.Protect(buf));
            }

#pragma warning disable 618 // calling obsolete methods
            // Otherwise fall back to using MachineKeySection.
            int count = buf.Length;
            if (cookieProtection == CookieProtection.All || cookieProtection == CookieProtection.Validation)
            {
                byte[] bMac = MachineKeySection.HashData (buf, null, 0, count);

                if (bMac == null)
                    return null;
                if (buf.Length >= count + bMac.Length)
                {
                    Buffer.BlockCopy (bMac, 0, buf, count, bMac.Length);
                }
                else
                {
                    byte[] bTemp = buf;
                    buf = new byte[count + bMac.Length];
                    Buffer.BlockCopy (bTemp, 0, buf, 0, count);
                    Buffer.BlockCopy (bMac, 0, buf, count, bMac.Length);
                }
                count += bMac.Length;
            }

            if (cookieProtection == CookieProtection.All || cookieProtection == CookieProtection.Encryption)
            {
                buf = MachineKeySection.EncryptOrDecryptData (true, buf, null, 0, count);
                count = buf.Length;
            }
            if (count < buf.Length)
            {
                byte[] bTemp = buf;
                buf = new byte[count];
                Buffer.BlockCopy (bTemp, 0, buf, 0, count);
            }
#pragma warning restore 618 // calling obsolete methods

            return HttpServerUtility.UrlTokenEncode(buf);
        }

        internal static byte[] Decode (CookieProtection cookieProtection, string data, Purpose purpose)
        {
            byte[] buf = HttpServerUtility.UrlTokenDecode(data);
            if (AspNetCryptoServiceProvider.Instance.IsDefaultProvider) {
                // If we're configured to go through the new crypto routines, do so.
                ICryptoService cryptoService = AspNetCryptoServiceProvider.Instance.GetCryptoService(purpose);
                return cryptoService.Unprotect(buf);
            }

#pragma warning disable 618 // calling obsolete methods
            // Otherwise fall back to using MachineKeySection.
            if (buf == null || cookieProtection == CookieProtection.None)
                return buf;
            if (cookieProtection == CookieProtection.All || cookieProtection == CookieProtection.Encryption)
            {
                buf = MachineKeySection.EncryptOrDecryptData (false, buf, null, 0, buf.Length);
                if (buf == null)
                    return null;
            }

            if (cookieProtection == CookieProtection.All || cookieProtection == CookieProtection.Validation)
                return MachineKeySection.GetUnHashedData(buf);
            return buf;
#pragma warning restore 618 // calling obsolete methods
        }
    }
}
