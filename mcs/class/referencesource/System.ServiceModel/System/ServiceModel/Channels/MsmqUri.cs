//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Globalization;
    using System.Net;
    using System.ServiceModel;
    using System.Text;

    static class MsmqUri
    {
        static IAddressTranslator netMsmqAddressTranslator;
        static IAddressTranslator activeDirectoryAddressTranslator;
        static IAddressTranslator deadLetterQueueAddressTranslator;
        static IAddressTranslator srmpAddressTranslator;
        static IAddressTranslator srmpsAddressTranslator;
        static IAddressTranslator formatnameAddressTranslator;

        public static IAddressTranslator NetMsmqAddressTranslator
        {
            get
            {
                if (null == netMsmqAddressTranslator)
                    netMsmqAddressTranslator = new MsmqUri.NetMsmq();
                return netMsmqAddressTranslator;
            }
        }

        public static IAddressTranslator ActiveDirectoryAddressTranslator
        {
            get
            {
                if (null == activeDirectoryAddressTranslator)
                    activeDirectoryAddressTranslator = new MsmqUri.ActiveDirectory();
                return activeDirectoryAddressTranslator;
            }
        }

        public static IAddressTranslator DeadLetterQueueAddressTranslator
        {
            get
            {
                if (null == deadLetterQueueAddressTranslator)
                    deadLetterQueueAddressTranslator = new MsmqUri.Dlq();
                return deadLetterQueueAddressTranslator;
            }
        }

        public static IAddressTranslator SrmpAddressTranslator
        {
            get
            {
                if (null == srmpAddressTranslator)
                    srmpAddressTranslator = new MsmqUri.Srmp();
                return srmpAddressTranslator;
            }
        }

        public static IAddressTranslator SrmpsAddressTranslator
        {
            get
            {
                if (null == srmpsAddressTranslator)
                    srmpsAddressTranslator = new MsmqUri.SrmpSecure();
                return srmpsAddressTranslator;
            }
        }

        public static IAddressTranslator FormatNameAddressTranslator
        {
            get
            {
                if (null == formatnameAddressTranslator)
                    formatnameAddressTranslator = new MsmqUri.FormatName();
                return formatnameAddressTranslator;
            }
        }

        public static string UriToFormatNameByScheme(Uri uri)
        {
            if (uri.Scheme == NetMsmqAddressTranslator.Scheme)
            {
                return NetMsmqAddressTranslator.UriToFormatName(uri);
            }
            else if (uri.Scheme == FormatNameAddressTranslator.Scheme)
            {
                return FormatNameAddressTranslator.UriToFormatName(uri);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("uri");
            }
        }

        static void AppendQueueName(StringBuilder builder, string relativePath, string slash)
        {
            const string privatePart = "/private";

            if (relativePath.StartsWith("/private$", StringComparison.OrdinalIgnoreCase))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqWrongPrivateQueueSyntax)));

            if (relativePath.StartsWith(privatePart, StringComparison.OrdinalIgnoreCase))
            {
                if (privatePart.Length == relativePath.Length)
                {
                    builder.Append("private$");
                    builder.Append(slash);
                    relativePath = "/";
                }
                else if ('/' == relativePath[privatePart.Length])
                {
                    builder.Append("private$");
                    builder.Append(slash);
                    relativePath = relativePath.Substring(privatePart.Length);
                }
            }
            builder.Append(relativePath.Substring(1));
        }

        internal interface IAddressTranslator
        {
            string Scheme { get; }
            string UriToFormatName(Uri uri);
            Uri CreateUri(string host, string name, bool isPrivate);
        }

        class NetMsmq : IAddressTranslator
        {
            public string Scheme
            {
                get { return "net.msmq"; }
            }

            public string UriToFormatName(Uri uri)
            {
                if (null == uri)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("uri"));
                if (uri.Scheme != this.Scheme)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MsmqInvalidScheme), "uri"));
                if (String.IsNullOrEmpty(uri.Host))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.MsmqWrongUri));
                if (-1 != uri.Port)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.MsmqUnexpectedPort));

                StringBuilder builder = new StringBuilder();
                builder.Append("DIRECT=");
                if (0 == String.Compare(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
                    builder.Append("OS:.");
                else
                {
                    IPAddress address = null;
                    if (IPAddress.TryParse(uri.Host, out address))
                        builder.Append("TCP:");
                    else
                        builder.Append("OS:");

                    builder.Append(uri.Host);
                }
                builder.Append("\\");
                MsmqUri.AppendQueueName(builder, Uri.UnescapeDataString(uri.PathAndQuery), "\\");

                return builder.ToString();
            }

            public Uri CreateUri(string host, string name, bool isPrivate)
            {
                string path = "/" + name;
                if (isPrivate)
                {
                    path = "/private" + path;
                }

                return (new UriBuilder(Scheme, host, -1, path)).Uri;
            }
        }

        class PathName : IAddressTranslator
        {
            public string Scheme
            {
                get { return "net.msmq"; }
            }

            public virtual string UriToFormatName(Uri uri)
            {
                if (null == uri)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("uri"));
                if (uri.Scheme != this.Scheme)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MsmqInvalidScheme), "uri"));
                if (String.IsNullOrEmpty(uri.Host))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.MsmqWrongUri));
                if (-1 != uri.Port)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.MsmqUnexpectedPort));

                uri = PostVerify(uri);

                StringBuilder builder = new StringBuilder();
                if (0 == String.Compare(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
                    builder.Append(".");
                else
                    builder.Append(uri.Host);

                builder.Append("\\");
                MsmqUri.AppendQueueName(builder, Uri.UnescapeDataString(uri.PathAndQuery), "\\");

                return builder.ToString();
            }

            public Uri CreateUri(string host, string name, bool isPrivate)
            {
                string path = "/" + name;
                if (isPrivate)
                {
                    path = "/private" + path;
                }

                return (new UriBuilder(Scheme, host, -1, path)).Uri;
            }

            protected virtual Uri PostVerify(Uri uri)
            {
                return uri;
            }
        }

        class ActiveDirectory : PathName
        {
            public override string UriToFormatName(Uri uri)
            {
                return MsmqFormatName.FromQueuePath(base.UriToFormatName(uri));
            }
        }

        class Dlq : PathName
        {
            protected override Uri PostVerify(Uri uri)
            {
                if (0 == String.Compare(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
                    return uri;
                try
                {
                    if (0 == String.Compare(DnsCache.MachineName, DnsCache.Resolve(uri).HostName, StringComparison.OrdinalIgnoreCase))
                    {
                        return new UriBuilder(Scheme, "localhost", -1, uri.PathAndQuery).Uri;
                    }
                }
                catch (EndpointNotFoundException ex)
                {
                    MsmqDiagnostics.ExpectedException(ex);
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MsmqDLQNotLocal), "uri"));
            }
        }

        abstract class SrmpBase : IAddressTranslator
        {
            const string msmqPart = "/msmq/";

            public string Scheme
            {
                get { return "net.msmq"; }
            }

            public string UriToFormatName(Uri uri)
            {
                if (null == uri)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("uri"));
                if (uri.Scheme != this.Scheme)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MsmqInvalidScheme), "uri"));
                if (String.IsNullOrEmpty(uri.Host))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.MsmqWrongUri));

                StringBuilder builder = new StringBuilder();
                builder.Append("DIRECT=");
                builder.Append(this.DirectScheme);
                builder.Append(uri.Host);
                if (-1 != uri.Port)
                {
                    builder.Append(":");
                    builder.Append(uri.Port.ToString(CultureInfo.InvariantCulture));
                }

                string relativePath = Uri.UnescapeDataString(uri.PathAndQuery);
                builder.Append(msmqPart);
                MsmqUri.AppendQueueName(builder, relativePath, "/");

                return builder.ToString();
            }

            abstract protected string DirectScheme { get; }

            public Uri CreateUri(string host, string name, bool isPrivate)
            {
                string path = "/" + name;
                if (isPrivate)
                {
                    path = "/private" + path;
                }

                return (new UriBuilder(Scheme, host, -1, path)).Uri;
            }
        }

        class Srmp : SrmpBase
        {
            protected override string DirectScheme
            {
                get { return "http://"; }
            }
        }

        class SrmpSecure : SrmpBase
        {
            protected override string DirectScheme
            {
                get { return "https://"; }
            }
        }

        class FormatName : IAddressTranslator
        {
            public string Scheme
            {
                get { return "msmq.formatname"; }
            }

            public string UriToFormatName(Uri uri)
            {
                if (null == uri)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("uri"));
                if (uri.Scheme != this.Scheme)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MsmqInvalidScheme), "uri"));
                return Uri.UnescapeDataString(uri.AbsoluteUri.Substring(this.Scheme.Length + 1));
            }

            public Uri CreateUri(string host, string name, bool isPrivate)
            {
                string path;
                if (isPrivate)
                {
                    path = "PRIVATE$\\" + name;
                }
                else
                {
                    path = name;
                }

                path = "DIRECT=OS:" + host + "\\" + path;

                return new Uri(Scheme + ":" + path);
            }
        }
    }
}
