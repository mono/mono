//
// CardSpacePolicyElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Xml;

namespace System.IdentityModel.Selectors
{
	public class CardSpacePolicyElement
	{
		public CardSpacePolicyElement (
			XmlElement target, XmlElement issuer,
			Collection<XmlElement> parameters,
			Uri privacyNoticeLink,
			int privacyNoticeVersion,
			bool isManagedIssuer)
		{
			this.target = target;
			this.issuer = issuer;
			this.parameters = parameters ?? new Collection<XmlElement> ();
			this.policy_link = privacyNoticeLink;
			policy_ver = privacyNoticeVersion;
			is_managed = isManagedIssuer;
		}

		XmlElement target;
		XmlElement issuer;
		Collection<XmlElement> parameters;
		Uri policy_link;
		int policy_ver;
		bool is_managed;

		public bool IsManagedIssuer {
			get { return is_managed; }
			set { is_managed = value; }
		}

		public XmlElement Issuer {
			get { return issuer; }
			set { issuer = value; }
		}

		public Collection<XmlElement> Parameters {
			get { return parameters; }
		}

		public Uri PolicyNoticeLink {
			get { return policy_link; }
			set { policy_link = value; }
		}

		public int PolicyNoticeVersion {
			get { return policy_ver; }
			set { policy_ver = value; }
		}

		public XmlElement Target {
			get { return target; }
			set { target = value; }
		}
	}
}
