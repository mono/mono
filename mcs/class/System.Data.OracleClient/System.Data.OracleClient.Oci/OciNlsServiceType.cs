//
// OciNlsServiceType.cs - OCI NLS Service Type
//
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.Oci
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient.Oci
//
// Author:
//     Daniel Morgan <danielmorgan@verizon.net>
//
// Copyright (C) Daniel Morgan, 2005
//
//

using System;

namespace System.Data.OracleClient.Oci {
	internal enum OciNlsServiceType : ushort {
		// native name
		DAYNAME1           = 1,  // Monday
		DAYNAME2           = 2,  // Tuesday
		DAYNAME3           = 3,  // Wednesday
		DAYNAME4           = 4,  // Thursday
		DAYNAME5           = 5,  // Friday
		DAYNAME6           = 6,  // Saturday
		DAYNAME7           = 7,  // Sunday
		// native abbreviated name
		ABDAYNAME1         = 8,  // Monday
		ABDAYNAME2         = 9,  // Tuesday
		ABDAYNAME3         = 10, // Wednesday
		ABDAYNAME4         = 11, // Thursday
		ABDAYNAME5         = 12, // Friday
		ABDAYNAME6         = 13, // Saturday
		ABDAYNAME7         = 14, // Sunday
		// native name
		MONTHNAME1         = 15, // January
		MONTHNAME2         = 16, // February
		MONTHNAME3         = 17, // March
		MONTHNAME4         = 18, // April,
		MONTHNAME5         = 19, // May
		MONTHNAME6         = 20, // June
		MONTHNAME7         = 21, // July
		MONTHNAME8         = 22, // August
		MONTHNAME9         = 23, // September
		MONTHNAME10        = 24, // October
		MONTHNAME11        = 25, // November
		MONTHNAME12        = 26, // December
		// native abbreviated name
		ABMONTHNAME1       = 27, // January
		ABMONTHNAME2       = 28, // February
		ABMONTHNAME3       = 29, // March
		ABMONTHNAME4       = 30, // April
		ABMONTHNAME5       = 31, // May
		ABMONTHNAME6       = 32, // June
		ABMONTHNAME7       = 33, // July
		ABMONTHNAME8       = 34, // August
		ABMONTHNAME9       = 35, // September
		ABMONTHNAME10      = 36, // October
		ABMONTHNAME11      = 37, // November
		ABMONTHNAME12      = 38, // December
		// native string 
		YES                = 39, // Affirmative response
		NO                 = 40, // Negative response
		// native equivalent string
		AM                 = 41, // AM
		PM                 = 42, // PM
		AD                 = 43, // AD
		BC                 = 44, // BC
		DECIMAL            = 45, // Decimal character
		GROUP              = 46, // Group separator
		DEBIT              = 47, // native symbol for Debit
		CREDIT             = 48, // native symbol for Credit
		DATEFORMAT         = 49, // Oracle Date Format
		INT_CURRENCY       = 50, // International Currency symbol
		LOC_CURRENCY       = 51, // Locale Currency symbol
		LANGUAGE           = 52, // Language Name
		ABLANGUAGE         = 53, // abbreviation for Language Name
		TERRITORY          = 54, // Territory Name
		CHARACTER_SET      = 55, // Character set Name
		LINGUISTIC_NAME    = 56, // Linguistic Name
		CALENDAR           = 57, // Calendar name
		DUAL_CURRENCY      = 78, // Dual currency symbol
		WRITINGDIR         = 79, // Language writing direction
		ABTERRITORY        = 80, // Territory Abbreviation
		DDATEFORMAT        = 81, // Oracle default date format
		DTIMEFORMAT        = 82, // Oracle default time format
		SFDATEFORMAT       = 83, // Local string formatted date format
		SFTIMEFORMAT       = 84, // Local string formatted time format
		NUMGROUPING        = 85, // Number grouping fields
		LISTSEP            = 86, // List separator
		MONDECIMAL         = 87, // Monetary decimal character
		MONGROUP           = 88, // Monetary group separator
		MONGROUPING        = 89, // Monetary grouping fields
		INT_CURRENCYSEP    = 90, // International currency separator
		CHARSET_MAXBYTESZ  = 91, // Maximum character byte size      
		CHARSET_FIXEDWIDTH = 92, // Fixed-width charset byte size   
		CHARSET_ID         = 93, // Character set id
		NCHARSET_ID        = 94, // NCharacter set id
		MAXBUFSZ           = 100 // Max buffer size for OCINlsGetInfo
	}
}


