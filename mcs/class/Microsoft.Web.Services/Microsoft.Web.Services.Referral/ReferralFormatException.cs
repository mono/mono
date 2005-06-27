//
// Microsoft.Web.Services.Referral.ReferralFormatException.cs
//
// Authors:
//	Daniel Kornhauser <dkor@alum.mit.edu>
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) Ximian, Inc. 2003.
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

// TODO: Figure out what the Uri parameter does.

using System;
using System.Web.Services.Protocols;
using System.Xml;

namespace Microsoft.Web.Services.Referral {
	
	[Serializable]
	[MonoTODO("I18N")]
	public class ReferralFormatException : SoapHeaderException {

		public static readonly string BadDescValue = Locale.GetText ("BadDescValue");
		public static readonly string BadExactValue = Locale.GetText ("BadExactValue");
		public static readonly string BadMatchCombination = Locale.GetText ("BadMatchCombination");
		public static readonly string BadPrefixValue = Locale.GetText ("BadPrefixValue");
		public static readonly string BadRefAddrValue = Locale.GetText ("BadRefAddrValue");
		public static readonly string BadRefIdValue = Locale.GetText ("BadRefIdValue");
		public static readonly string BadRidValue = Locale.GetText ("BadRidValue");
		public static readonly string BadTransport = Locale.GetText ("BadTransport");
		public static readonly string BadTtlValue = Locale.GetText ("BadTtlValue");
		public static readonly string BadViaValue = Locale.GetText ("BadViaValue");
		public static readonly string DuplicateDescElement = Locale.GetText ("DuplicateDescElement");
		public static readonly string DuplicateExactElement = Locale.GetText ("DuplicateExactElement");
		public static readonly string DuplicateForElement = Locale.GetText ("DuplicateForElement");
		public static readonly string DuplicateGoElement = Locale.GetText ("DuplicateGoElement");
		public static readonly string DuplicateIfElement = Locale.GetText ("DuplicateIfElement");
		public static readonly string DuplicatePrefixElement = Locale.GetText ("DuplicatePrefixElement");
		public static readonly string DuplicateRefIdElement = Locale.GetText ("DuplicateRefIdElement");
		public static readonly string ExactIsNotAbsoluteUri = Locale.GetText ("ExactIsNotAbsoluteUri");
		public static readonly string MissingForElement = Locale.GetText ("MissingForElement");
		public static readonly string MissingGoElement = Locale.GetText ("MissingGoElement");
		public static readonly string MissingRefIdElement = Locale.GetText ("MissingRefIdElement");
		public static readonly string MissingRidElement = Locale.GetText ("MissingRidElement");
		public static readonly string MissingViaElement = Locale.GetText ("MissingViaElement");
		public static readonly string MoreThanOneReferralHeaders = Locale.GetText ("MoreThanOneReferralHeaders");
		public static readonly string NegativeTtlValue = Locale.GetText ("NegativeTtlValue");
		public static readonly string PrefixIsNotAbsoluteUri = Locale.GetText ("PrefixIsNotAbsoluteUri");
		public static readonly string RefAddrIsNotAbsoluteUri = Locale.GetText ("RefAddrIsNotAbsoluteUri");
		public static readonly string SignedTtlValue = Locale.GetText ("SignedTtlValue");
		public static readonly string ViaIsNotAbsoluteUri = Locale.GetText ("ViaIsNotAbsoluteUri");

		Uri reference;
                
		public ReferralFormatException (string message)
			: base (message, XmlQualifiedName.Empty) {
		}

		public ReferralFormatException (Uri refid, string message)
			: base (message, XmlQualifiedName.Empty) {
			reference = refid;
		}

		public ReferralFormatException (Uri refid, string message, Exception innerException)
			: base (message, XmlQualifiedName.Empty, innerException) {
			reference = refid;
		}
	}
}
