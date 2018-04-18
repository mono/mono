//
// System.ServiceModel.MessageBuffer.cs
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
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Mono.Xml.XPath;

namespace System.ServiceModel.Channels {

	public abstract class MessageBuffer : IXPathNavigable, IDisposable
	{
		XPathNavigator nav_cache;

		protected MessageBuffer () {}

		public abstract void Close ();
		public abstract Message CreateMessage ();

		public XPathNavigator CreateNavigator ()
		{
			return CreateNavigator (XmlSpace.Default);
		}

		public XPathNavigator CreateNavigator (int nodeQuota)
		{
			return CreateNavigator (nodeQuota, XmlSpace.Default);
		}

		[MonoTODO ("supply proper quota")]
		public XPathNavigator CreateNavigator (XmlSpace space)
		{
			// FIXME: I never counted expected quota value.
			return CreateNavigator (1000, XmlSpace.Default);
		}

		[MonoTODO ("Handle node_quota and xmlspace")]
		public XPathNavigator CreateNavigator (int nodeQuota, XmlSpace space)
		{
			if (nav_cache == null) {
				DTMXPathDocumentWriter2 pw = new DTMXPathDocumentWriter2 (new NameTable (), nodeQuota);
				XmlDictionaryWriter w = XmlDictionaryWriter.CreateDictionaryWriter (pw);
				CreateMessage ().WriteMessage (w);
				nav_cache = pw.CreateDocument ().CreateNavigator ();
			}
			return nav_cache.Clone ();
		}

		void IDisposable.Dispose ()
		{
			this.Close ();
		}

		public virtual void WriteMessage (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream is null");
			
			XmlDictionaryWriter w = XmlDictionaryWriter.CreateBinaryWriter (stream);
			Message m = CreateMessage ();

			m.WriteMessage (w);
			w.Close ();
		}

		public abstract int BufferSize { get; }

		public virtual string MessageContentType {
			get { return "application/soap+msbin1"; }
		}
	}
}