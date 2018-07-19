//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Description;
    using System.Reflection;
    using System.Net;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;


    internal class ChannelCredentials : IChannelCredentials, IDisposable
    {
        protected IProvideChannelBuilderSettings channelBuilderSettings;
        internal ChannelCredentials(IProvideChannelBuilderSettings channelBuilderSettings)
        {
            this.channelBuilderSettings = channelBuilderSettings;
        }
        internal static ComProxy Create(IntPtr outer, IProvideChannelBuilderSettings channelBuilderSettings)
        {

            if (channelBuilderSettings == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotCreateChannelOption)));


            ChannelCredentials ChannelCredentials = null;
            ComProxy proxy = null;
            try
            {
                ChannelCredentials = new ChannelCredentials(channelBuilderSettings);
                proxy = ComProxy.Create(outer, ChannelCredentials, ChannelCredentials);
                return proxy;
            }
            finally
            {
                if (proxy == null)
                {
                    if (ChannelCredentials != null)
                        ((IDisposable)ChannelCredentials).Dispose();
                }

            }
        }
        void IDisposable.Dispose()
        {
        }
        void IChannelCredentials.SetWindowsCredential(string domain, string userName, string password, int impersonationLevel, bool allowNtlm)
        {
            lock (channelBuilderSettings)
            {
                KeyedByTypeCollection<IEndpointBehavior> behaviors = channelBuilderSettings.Behaviors;
                NetworkCredential newCredentials = null;
                if ((!String.IsNullOrEmpty(domain)) || (!String.IsNullOrEmpty(userName)) || (!String.IsNullOrEmpty(password)))
                {
                    if (String.IsNullOrEmpty(userName))
                    {
                        userName = "";
                    }
                    System.ServiceModel.Security.SecurityUtils.PrepareNetworkCredential();
                    newCredentials = new NetworkCredential(userName, password, domain);
                }
                ClientCredentials channelCredentials = behaviors.Find<ClientCredentials>();
                if (channelCredentials == null)
                {
                    channelCredentials = new ClientCredentials();
                    behaviors.Add(channelCredentials);
                }
                channelCredentials.Windows.AllowedImpersonationLevel = (TokenImpersonationLevel)impersonationLevel;

                // To disable AllowNtlm warning.
#pragma warning disable 618
                channelCredentials.Windows.AllowNtlm = allowNtlm;
#pragma warning restore 618

                channelCredentials.Windows.ClientCredential = newCredentials;
            }
        }
        void IChannelCredentials.SetUserNameCredential(string userName, string password)
        {
            lock (channelBuilderSettings)
            {
                KeyedByTypeCollection<IEndpointBehavior> behaviors = channelBuilderSettings.Behaviors;
                ClientCredentials channelCredentials = behaviors.Find<ClientCredentials>();
                if (channelCredentials == null)
                {
                    channelCredentials = new ClientCredentials();
                    behaviors.Add(channelCredentials);
                }
                channelCredentials.UserName.UserName = userName;
                channelCredentials.UserName.Password = password;
            }
        }

        void IChannelCredentials.SetServiceCertificateAuthentication(string storeLocation, string revocationMode, string certificationValidationMode)
        {
            lock (channelBuilderSettings)
            {
                StoreLocation location = (StoreLocation)Enum.Parse(typeof(StoreLocation), storeLocation);
                X509RevocationMode mode = (X509RevocationMode)Enum.Parse(typeof(X509RevocationMode), revocationMode);

                X509CertificateValidationMode validationMode = X509ServiceCertificateAuthentication.DefaultCertificateValidationMode;
                if (!String.IsNullOrEmpty(certificationValidationMode))
                    validationMode = (X509CertificateValidationMode)Enum.Parse(typeof(X509CertificateValidationMode), certificationValidationMode);

                KeyedByTypeCollection<IEndpointBehavior> behaviors = channelBuilderSettings.Behaviors;
                ClientCredentials channelCredentials = behaviors.Find<ClientCredentials>();
                if (channelCredentials == null)
                {
                    channelCredentials = new ClientCredentials();
                    behaviors.Add(channelCredentials);
                }
                channelCredentials.ServiceCertificate.Authentication.TrustedStoreLocation = location;
                channelCredentials.ServiceCertificate.Authentication.RevocationMode = mode;
                channelCredentials.ServiceCertificate.Authentication.CertificateValidationMode = validationMode;
            }
        }

        void IChannelCredentials.SetClientCertificateFromStore(string storeLocation, string storeName, string findType, object findValue)
        {
            lock (channelBuilderSettings)
            {
                StoreLocation location = (StoreLocation)Enum.Parse(typeof(StoreLocation), storeLocation);
                StoreName name = (StoreName)Enum.Parse(typeof(StoreName), storeName);
                X509FindType type = (X509FindType)Enum.Parse(typeof(X509FindType), findType);
                KeyedByTypeCollection<IEndpointBehavior> behaviors = channelBuilderSettings.Behaviors;
                ClientCredentials channelCredentials = behaviors.Find<ClientCredentials>();
                if (channelCredentials == null)
                {
                    channelCredentials = new ClientCredentials();
                    behaviors.Add(channelCredentials);
                }
                channelCredentials.ClientCertificate.SetCertificate(location, name, type, findValue);
            }
        }

        void IChannelCredentials.SetClientCertificateFromStoreByName(string subjectName, string storeLocation, string storeName)
        {
            ((IChannelCredentials)this).SetClientCertificateFromStore(storeLocation, storeName, X509CertificateInitiatorClientCredential.DefaultFindType.ToString("G"), subjectName);
        }


        void IChannelCredentials.SetClientCertificateFromFile(string fileName, string password, string keyStorageFlags)
        {
            lock (channelBuilderSettings)
            {
                KeyedByTypeCollection<IEndpointBehavior> behaviors = channelBuilderSettings.Behaviors;

                X509Certificate2 cert;
                if (!String.IsNullOrEmpty(keyStorageFlags))
                {
                    X509KeyStorageFlags flags = (X509KeyStorageFlags)Enum.Parse(typeof(X509KeyStorageFlags), keyStorageFlags);
                    cert = new X509Certificate2(fileName, password, flags);
                }
                else
                {
                    cert = new X509Certificate2(fileName, password);
                }
                ClientCredentials channelCredentials = behaviors.Find<ClientCredentials>();
                if (channelCredentials == null)
                {
                    channelCredentials = new ClientCredentials();
                    behaviors.Add(channelCredentials);
                }
                channelCredentials.ClientCertificate.Certificate = cert;
            }
        }

        void IChannelCredentials.SetDefaultServiceCertificateFromStore(string storeLocation, string storeName, string findType, object findValue)
        {
            lock (channelBuilderSettings)
            {
                StoreLocation location = (StoreLocation)Enum.Parse(typeof(StoreLocation), storeLocation);
                StoreName name = (StoreName)Enum.Parse(typeof(StoreName), storeName);
                X509FindType type = (X509FindType)Enum.Parse(typeof(X509FindType), findType);
                KeyedByTypeCollection<IEndpointBehavior> behaviors = channelBuilderSettings.Behaviors;
                ClientCredentials channelCredentials = behaviors.Find<ClientCredentials>();
                if (channelCredentials == null)
                {
                    channelCredentials = new ClientCredentials();
                    behaviors.Add(channelCredentials);
                }
                channelCredentials.ServiceCertificate.SetDefaultCertificate(location, name, type, findValue);
            }
        }

        void IChannelCredentials.SetDefaultServiceCertificateFromStoreByName(string subjectName, string storeLocation, string storeName)
        {
            ((IChannelCredentials)this).SetDefaultServiceCertificateFromStore(storeLocation, storeName, X509CertificateInitiatorClientCredential.DefaultFindType.ToString("G"), subjectName);
        }

        void IChannelCredentials.SetDefaultServiceCertificateFromFile(string fileName, string password, string keyStorageFlags)
        {
            lock (channelBuilderSettings)
            {
                KeyedByTypeCollection<IEndpointBehavior> behaviors = channelBuilderSettings.Behaviors;

                X509Certificate2 cert;
                if (!String.IsNullOrEmpty(keyStorageFlags))
                {
                    X509KeyStorageFlags flags = (X509KeyStorageFlags)Enum.Parse(typeof(X509KeyStorageFlags), keyStorageFlags);
                    cert = new X509Certificate2(fileName, password, flags);
                }
                else
                {
                    cert = new X509Certificate2(fileName, password);
                }

                ClientCredentials channelCredentials = behaviors.Find<ClientCredentials>();
                if (channelCredentials == null)
                {
                    channelCredentials = new ClientCredentials();
                    behaviors.Add(channelCredentials);
                }
                channelCredentials.ServiceCertificate.DefaultCertificate = cert;
            }
        }
        void IChannelCredentials.SetIssuedToken(string localIssuerAddres, string localIssuerBindingType, string localIssuerBinding)
        {
            lock (channelBuilderSettings)
            {
                Binding binding = null;

                binding = ConfigLoader.LookupBinding(localIssuerBindingType, localIssuerBinding);
                KeyedByTypeCollection<IEndpointBehavior> behaviors = channelBuilderSettings.Behaviors;
                ClientCredentials channelCredentials = behaviors.Find<ClientCredentials>();
                if (channelCredentials == null)
                {
                    channelCredentials = new ClientCredentials();
                    behaviors.Add(channelCredentials);
                }
                channelCredentials.IssuedToken.LocalIssuerAddress = new EndpointAddress(localIssuerAddres);
                channelCredentials.IssuedToken.LocalIssuerBinding = binding;
            }
        }

    }
}


