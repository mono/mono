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

	[MonoTODO("We should support I18N")]
	[Serializable]
	public class TimestampFormatException : SoapHeaderException {
		public static readonly string BadDelayAttribute = "Bad Delay Attribute";
		public static readonly string DelayAttributeWithPlusSign = "Delay Attribute With Plus Sign";
		public static readonly string DuplicateCreatedElement = "Duplicate Created Element";
		public static readonly string DuplicateExpiresElement = "Duplicate Expires Element";
		public static readonly string MissingActorAttributeInReceivedElement = "Missing Actor Attribute In Received Element";
		public static readonly string MissingCreatedElement = "Missing Created Element";
		public static readonly string MoreThanOneTimestampHeaders = "More Than One Timestamp Headers";
#if WSE2
		public static readonly string BadActorAttribute = "Bad Actor Attribute";
		public static readonly string BadCreatedElement = "Bad Created Element";
		public static readonly string BadExpiresElement = "Bad Expires Element";
		public static readonly string BadNamespaceForActor = "Bad Namespace For Actor";
		public static readonly string BadNamespaceForMustUnderstand = "Bad Namespace For MustUnderstand";
		public static readonly string BadReceivedElement = "Bad Received Element";
		public static readonly string BadTimestampActorAttribute = "BadTimestamp Actor Attribute";
		public static readonly string DuplicateActorAttribute = "Duplicate Actor Attribute";
		public static readonly string DuplicateDelayAttribute = "Duplicate Delay Attribute";
#endif
		public TimestampFormatException (string message)
                        : base (message, XmlQualifiedName.Empty) {}
		
#if WSE2
		public TimestampFormatException (string message, Exception ex)
                        : base (message, XmlQualifiedName.Empty, ex) {}
#endif
	}
}
