//
// MessageDescription.cs
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
using System.Net.Security;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Description
{
	public class MessageDescription
	{
		string action;
		MessageDirection direction;
		MessagePropertyDescriptionCollection properties
			= new MessagePropertyDescriptionCollection ();
		MessageBodyDescription body = new MessageBodyDescription ();
		MessageHeaderDescriptionCollection headers
			= new MessageHeaderDescriptionCollection ();
		Type message_type;
		bool has_protection_level;
		ProtectionLevel protection_level;

		public MessageDescription (string action,
			MessageDirection direction)
		{
			this.action = action;
			this.direction = direction;
		}

		public string Action {
			get { return action; }
		}

		public MessageBodyDescription Body {
			get { return body; }
		}

		public MessageDirection Direction {
			get { return direction; }
		}

		public MessageHeaderDescriptionCollection Headers {
			get { return headers; }
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

		public Type MessageType {
			get { return message_type; }
			set { message_type = value; }
		}

		public MessagePropertyDescriptionCollection Properties {
			get { return properties; }
		}
	}
}
