//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Web
{
    using System;
    using System.Linq;
    using System.Runtime;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;

    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "REST Exceptions cannot contain InnerExceptions or messages")]
    public class WebFaultException : FaultException, IWebFaultException
    {
        internal const string WebFaultCodeNamespace = "http://schemas.microsoft.com/2009/WebFault";

        public WebFaultException(HttpStatusCode statusCode)
            : base(GetDefaultReason(statusCode), GetFaultCode(statusCode))
        {
            this.StatusCode = statusCode;
        }

        protected WebFaultException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.StatusCode = (HttpStatusCode)info.GetValue("statusCode", typeof(HttpStatusCode));
        }

        public HttpStatusCode StatusCode { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "The interface is used merely to be able to handled both the generic and non-generic versions and doesn't need to be exposed.")]
        Type IWebFaultException.DetailType
        {
            get { return null; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "The interface is used merely to be able to handled both the generic and non-generic versions and doesn't need to be exposed.")]
        object IWebFaultException.DetailObject
        {
            get { return null; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "The interface is used merely to be able to handled both the generic and non-generic versions and doesn't need to be exposed.")]
        Type[] IWebFaultException.KnownTypes
        {
            get { return null; }
        }

        [Fx.Tag.SecurityNote(Critical = "Overrides the base.GetObjectData which is critical, as well as calling this method.", Safe = "Replicates the LinkDemand.")]
        [SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("statusCode", this.StatusCode);
        }

        internal static FaultCode GetFaultCode(HttpStatusCode statusCode)
        {
            if ((int) statusCode >= (int) HttpStatusCode.InternalServerError)
            {
                return FaultCode.CreateReceiverFaultCode(statusCode.ToString(), WebFaultCodeNamespace);
            }
            else 
            {
                return FaultCode.CreateSenderFaultCode(statusCode.ToString(), WebFaultCodeNamespace);
            }
        }

        // These reasons come from section 6.1.1 of http://www.ietf.org/rfc/rfc2616.txt
        internal static string GetDefaultReason(HttpStatusCode statusCode)
        {
            switch ((int)statusCode)
            {
                case 100: return "Continue";
                case 101: return "Switching Protocols";
                case 200: return "OK";
                case 201: return "Created";
                case 202: return "Accepted";
                case 203: return "Non-Authoritative Information";
                case 204: return "No Content";
                case 205: return "Reset Content";
                case 206: return "Partial Content";
                case 300: return "Multiple Choices";
                case 301: return "Moved Permanently";
                case 302: return "Found";
                case 303: return "See Other";
                case 304: return "Not Modified";
                case 305: return "Use Proxy";
                case 307: return "Temporary Redirect";
                case 400: return "Bad Request";
                case 401: return "Unauthorized";
                case 402: return "Payment Required";
                case 403: return "Forbidden";
                case 404: return "Not Found";
                case 405: return "Method Not Allowed";
                case 406: return "Not Acceptable";
                case 407: return "Proxy Authentication Required";
                case 408: return "Request Time-out";
                case 409: return "Conflict";
                case 410: return "Gone";
                case 411: return "Length Required";
                case 412: return "Precondition Failed";
                case 413: return "Request Entity Too Large";
                case 414: return "Request-URI Too Large";
                case 415: return "Unsupported Media Type";
                case 416: return "Requested range not satisfiable";
                case 417: return "Expectation Failed";
                case 500: return "Internal Server Error";
                case 501: return "Not Implemented";
                case 502: return "Bad Gateway";
                case 503: return "Service Unavailable";
                case 504: return "Gateway Time-out";
                case 505: return "HTTP Version not supported";
                default:
                    {
                        int errorClass = ((int)statusCode) / 100;
                        switch (errorClass)
                        {
                            case 1: return "Informational";
                            case 2: return "Success";
                            case 3: return "Redirection";
                            case 4: return "Client Error";
                            case 5: return "Server Error";
                            default: return null;
                        }
                    }
            }
        }
    }

    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "REST Exceptions cannot contain InnerExceptions or messages")]
    public class WebFaultException<T> : FaultException<T>, IWebFaultException
    {
        Type[] knownTypes;

        public WebFaultException(T detail, HttpStatusCode statusCode)
            : base(detail, WebFaultException.GetDefaultReason(statusCode), WebFaultException.GetFaultCode(statusCode))
        {
            this.StatusCode = statusCode;
        }

        public WebFaultException(T detail, HttpStatusCode statusCode, IEnumerable<Type> knownTypes)
            : base(detail, WebFaultException.GetDefaultReason(statusCode), WebFaultException.GetFaultCode(statusCode))
        {
            this.StatusCode = statusCode;
            this.knownTypes = (knownTypes == null) ? null : new List<Type>(knownTypes).ToArray();
        }

        protected WebFaultException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.StatusCode = (HttpStatusCode)info.GetValue("statusCode", typeof(HttpStatusCode));
            this.knownTypes = (Type[])info.GetValue("knownTypes", typeof(Type[]));
        }

        public HttpStatusCode StatusCode { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "The interface is used merely to be able to handled both the generic and non-generic versions and doesn't need to be exposed.")]
        Type IWebFaultException.DetailType
        {
            get { return typeof(T); }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "The interface is used merely to be able to handled both the generic and non-generic versions and doesn't need to be exposed.")]
        object IWebFaultException.DetailObject
        {
            get { return this.Detail; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "The interface is used merely to be able to handled both the generic and non-generic versions and doesn't need to be exposed.")]
        Type[] IWebFaultException.KnownTypes
        {
            get { return this.knownTypes; }
        }

        [Fx.Tag.SecurityNote(Critical = "Overrides the base.GetObjectData which is critical, as well as calling this method.", Safe = "Replicates the LinkDemand.")]
        [SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("statusCode", this.StatusCode);
            info.AddValue("knownTypes", this.knownTypes);
        }
    }
}
