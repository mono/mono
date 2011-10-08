//
// Mono.Dns.DnsResourceRecordA
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
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mono.Dns {
	class DnsResourceRecordA : DnsResourceRecordIPAddress {
		internal DnsResourceRecordA (DnsResourceRecord rr)
			: base (rr, 4)
		{
		}
	}
}

