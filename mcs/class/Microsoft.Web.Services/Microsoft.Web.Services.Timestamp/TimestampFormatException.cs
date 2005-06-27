//
// Microsoft.Web.Services.Timestamp.TimestampFormatException.cs
//
// Authors:
//	Daniel Kornhauser <dkor@alum.mit.edu>
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) Ximian, Inc. 2003.
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Web.Services.Protocols;
using System.Xml;

namespace Microsoft.Web.Services.Timestamp {

	[Serializable]
	public class TimestampFormatException : SoapHeaderException {
		public static readonly string BadDelayAttribute = Locale.GetText ("Bad Delay Attribute");
		public static readonly string DelayAttributeWithPlusSign = Locale.GetText ("Delay Attribute With Plus Sign");
		public static readonly string DuplicateCreatedElement = Locale.GetText ("Duplicate Created Element");
		public static readonly string DuplicateExpiresElement = Locale.GetText ("Duplicate Expires Element");
		public static readonly string MissingActorAttributeInReceivedElement = Locale.GetText ("Missing Actor Attribute In Received Element");
		public static readonly string MissingCreatedElement = Locale.GetText ("Missing Created Element");
		public static readonly string MoreThanOneTimestampHeaders = Locale.GetText ("More Than One Timestamp Headers");
#if WSE2
		public static readonly string BadActorAttribute = Locale.GetText ("Bad Actor Attribute");
		public static readonly string BadCreatedElement = Locale.GetText ("Bad Created Element");
		public static readonly string BadExpiresElement = Locale.GetText ("Bad Expires Element");
		public static readonly string BadNamespaceForActor = Locale.GetText ("Bad Namespace For Actor");
		public static readonly string BadNamespaceForMustUnderstand = Locale.GetText ("Bad Namespace For MustUnderstand");
		public static readonly string BadReceivedElement = Locale.GetText ("Bad Received Element");
		public static readonly string BadTimestampActorAttribute = Locale.GetText ("BadTimestamp Actor Attribute");
		public static readonly string DuplicateActorAttribute = Locale.GetText ("Duplicate Actor Attribute");
		public static readonly string DuplicateDelayAttribute = Locale.GetText ("Duplicate Delay Attribute");
#endif
		public TimestampFormatException (string message)
                        : base (message, XmlQualifiedName.Empty) {}
		
#if WSE2
		public TimestampFormatException (string message, Exception ex)
                        : base (message, XmlQualifiedName.Empty, ex) {}
#endif
	}
}
