//
// OperationContractAttribute.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System.Net.Security;

namespace System.ServiceModel
{
	[AttributeUsage (AttributeTargets.Method)]
	public sealed class OperationContractAttribute : Attribute
	{
		string action, reply_action, name;
		bool is_initiating = true, is_terminating, is_oneway, is_async;
		ProtectionLevel protection_level = ProtectionLevel.None;
		bool has_protection_level;

		public string Action {
			get { return action; }
			set { action = value; }
		}

		public bool AsyncPattern {
			get { return is_async; }
			set { is_async = value; }
		}

		public bool IsInitiating {
			get { return is_initiating; }
			set { is_initiating = value; }
		}

		public bool IsOneWay {
			get { return is_oneway; }
			set { is_oneway = value; }
		}

		public bool IsTerminating {
			get { return is_terminating; }
			set { is_terminating = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public bool HasProtectionLevel {
			get { return has_protection_level; }
		}

		public ProtectionLevel ProtectionLevel {
			get { return protection_level; }
			set {
				protection_level = value;
				has_protection_level = true;
			}
		}

		public string ReplyAction {
			get { return reply_action; }
			set { reply_action = value; }
		}
	}
}
