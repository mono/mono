//------------------------------------------------------------------------------
// <copyright file="ClientFormsIdentity.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.ClientServices
{
    using System;
    using System.Net;
    using System.Security.Principal;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Web.Security;
    using System.Diagnostics.CodeAnalysis;

    public class ClientFormsIdentity : IIdentity, IDisposable
    {
        public  string              Name                    { get { return _Name; }}
        public  bool                IsAuthenticated         { get { return _IsAuthenticated; }}
        public  string              AuthenticationType      { get { return _AuthenticationType; } }
        public  CookieContainer     AuthenticationCookies   { get { return _AuthenticationCookies; } }
        public  MembershipProvider  Provider                { get { return _Provider; } }

        public ClientFormsIdentity(string name, string password, MembershipProvider provider, string authenticationType, bool isAuthenticated, CookieContainer authenticationCookies)
        {
            _Name = name;
            _AuthenticationType = authenticationType;
            _IsAuthenticated = isAuthenticated;
            _AuthenticationCookies = authenticationCookies;
            _Password = GetSecureStringFromString(password);
            _Provider = provider;
        }

        public void RevalidateUser()
        {
            if (_Disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            _Provider.ValidateUser(_Name, GetStringFromSecureString(_Password));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_Password != null)
                {
                    _Password.Dispose();
                }
            }
            _Disposed = true;
        }

        private string              _Name;
        private bool                _IsAuthenticated;
        private string              _AuthenticationType;
        private CookieContainer     _AuthenticationCookies;
        private SecureString        _Password;
        private MembershipProvider  _Provider;
        private bool                _Disposed;

        private static SecureString GetSecureStringFromString(string password)
        {
            char[] passwordChars = password.ToCharArray();
            SecureString ss = new SecureString();
            for (int iter = 0; iter < passwordChars.Length; iter++)
                ss.AppendChar(passwordChars[iter]);
            ss.MakeReadOnly();
            return ss;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Reviewed and approved by feature crew")]
        [SecuritySafeCritical]
        private static string GetStringFromSecureString(SecureString securePass)
        {

            IntPtr bstr = IntPtr.Zero;
            try {
                bstr = Marshal.SecureStringToBSTR(securePass);
                return Marshal.PtrToStringBSTR(bstr);
            } finally {
                if (bstr != IntPtr.Zero)
                    Marshal.FreeBSTR(bstr);
            }
        }
    }
}
