//
// Mono.Net.Dns.DnsQType
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo.mono@gmail.com)
//
// Copyright 2011 Gonzalo Paniagua Javier
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
namespace Mono.Net.Dns {
	enum DnsQType : ushort {
		A             =         1,
		NS            =         2,
		[Obsolete] MD            =         3,
		[Obsolete] MF            =         4,
		CNAME         =         5,
		SOA           =         6,
		[Obsolete] MB            =         7,
		[Obsolete] MG            =         8,
		[Obsolete] MR            =         9,
		[Obsolete] NULL          =        10,
		[Obsolete] WKS           =        11,
		PTR           =        12,
		[Obsolete] HINFO         =        13,
		[Obsolete] MINFO         =        14,
		MX            =        15,
		TXT           =        16,
		[Obsolete] RP            =        17,
		AFSDB         =        18,
		[Obsolete] X25           =        19,
		[Obsolete] ISDN          =        20,
		[Obsolete] RT            =        21,
		[Obsolete] NSAP          =        22,
		[Obsolete] NSAPPTR       =        23,
		SIG           =        24,
		KEY           =        25,
		[Obsolete] PX            =        26,
		[Obsolete] GPOS          =        27,
		AAAA          =        28,
		LOC           =        29,
		[Obsolete] NXT           =        30,
		[Obsolete] EID           =        31,
		[Obsolete] NIMLOC        =        32,
		SRV           =        33,
		[Obsolete] ATMA          =        34,
		NAPTR         =        35,
		KX            =        36,
		CERT          =        37,
		[Obsolete] A6            =        38,
		DNAME         =        39,
		[Obsolete] SINK          =        40,
		OPT           =        41,
		[Obsolete] APL           =        42,
		DS            =        43,
		SSHFP         =        44,
		IPSECKEY      =        45,
		RRSIG         =        46,
		NSEC          =        47,
		DNSKEY        =        48,
		DHCID         =        49,
		NSEC3         =        50,
		NSEC3PARAM    =        51,
		HIP           =        55,
		NINFO         =        56,
		RKEY          =        57,
		TALINK        =        58,
		SPF           =        99,
		[Obsolete] UINFO         =       100,
		[Obsolete] UID           =       101,
		[Obsolete] GID           =       102,
		[Obsolete] UNSPEC        =       103,
		TKEY          =       249,
		TSIG          =       250,
		IXFR          =       251,
		AXFR          =       252,
		[Obsolete] MAILB         =       253,
		[Obsolete] MAILA         =       254,
		ALL           =       255,
		URI           =       256,
		TA            =     32768,
		DLV           =     32769,
	}
}

