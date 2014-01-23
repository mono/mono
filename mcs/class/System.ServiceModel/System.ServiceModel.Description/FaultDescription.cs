//
// FaultDescription.cs
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace System.ServiceModel.Description
{
	[DebuggerDisplay ("Name={name}, Action={action}, DetailType={detailType}")]
	public class FaultDescription
	{
		string action, name, ns;
		Type detail_type;
		bool has_protection_level;
		ProtectionLevel protection_level;

		public FaultDescription (string action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			this.action = action;
		}

		public string Action {
			get { return action; }
		}

		public Type DetailType {
			get { return detail_type; }
			set { detail_type = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Namespace {
			get { return ns; }
			set { ns= value; }
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
	}
}
