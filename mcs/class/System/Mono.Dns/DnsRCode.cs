//
// Mono.Dns.DnsRCode
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
namespace Mono.Dns {
#if !NET_2_0
	public
#endif
	enum DnsRCode : ushort {
		NoError = 0,
		FormErr = 1,
		ServFail = 2,
		NXDomain = 3,
		NotImp = 4,
		Refused = 5,
		YXDomain = 6,
		YXRRSet = 7,
		NXRRSet = 8,
		NotAuth = 9,
		NotZone = 10,
		BadVers = 16,
		BadSig = 16,
		BadKey = 17,
		BadTime = 18,
		BadMode = 19,
		BadName = 20,
		BadAlg = 21,
		BadTrunc = 22,
	}
}

