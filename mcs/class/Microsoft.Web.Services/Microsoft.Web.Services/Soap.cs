//
// Microsoft.Web.Services.Soap.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian Inc, 2003.
//

using System;
using System.Globalization;

namespace Microsoft.Web.Services  {

        public class Soap 
        {
                public Soap () {}

                public const string ActorNext = "http://schema.xmlsoap.org/soap/actor/next";

                public const string ActorNextURI = "http://schema.xmlsoap.org/soap/actor/next";

                public const string DimeContentType = "application/dime";

                public const string FaultNamespaceURI = "http://schemas.microsoft.com/wsdk/2002/10";

                public const string NamespaceURI = "http://schemas.xmlsoap.org/soap/envelope";

                public const string Prefix = "soap";

                public const string SoapContentType = "text/xml";

                public class AttributesName
                {
                        public const string Actor = "actor";
                        public const string MustUnderstand = "mustUnderstand";
                }

                public class Elementsname
                {
                        public const string Body = "Body";
                        public const string Envelope  = "Envelope";
                        public const string Fault  = "Fault";
                        public const string FaultActor  = "faultactor";
                        public const string FaultCode  = "faultcode";
                        public const string FaultDetail = "detail";
                        public const string FaultString = "faultstring";
                        public const string Header = "Header";
                        public const string Message = "Message";
                        public const string StackTrace = "StackTrace";
                }
        }
}
