//
// System.ServiceModel.MessageHeader.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

// I think it is incorrectly put in this namespace ...
namespace System.ServiceModel
{
	public class MessageHeader<T>
	{
		T content;
		string actor;
		bool must_understand, relay;

		XmlObjectSerializer formatter;
		
		public MessageHeader ()
			: this (default (T), false, null, false)
		{
		}

		public MessageHeader (T content)
			: this (content, false, null, false)
		{
		}

		public MessageHeader (T content, bool mustUnderstand, string actor, bool relay)
		{
			this.content = content;
			this.must_understand = mustUnderstand;
			this.actor = actor;
			this.relay = relay;
		}

		public MessageHeader GetUntypedHeader (string name, string ns)
		{
			if (formatter == null)
				formatter = new DataContractSerializer (typeof (T));
			// FIXME: how to handle IsReferenceParameter
			return new MessageHeader.DefaultMessageHeader (
				name, ns, content, formatter, false, must_understand, actor, relay);
		}

		public string Actor {
			get { return actor; }
			set { actor = value; }
		}

		public T Content {
			get { return content; }
			set { content = value; }
		}

		public bool MustUnderstand {
			get { return must_understand; }
			set { must_understand = value; }
		}

		public bool Relay {
			get { return relay; }
			set { relay = value; }
		}
	}

}
