//
// BodyWriter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005,2009 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace System.ServiceModel.Channels
{
	public abstract class BodyWriter
	{
		bool is_buffered;

		protected BodyWriter (bool isBuffered)
		{
			is_buffered = isBuffered;
		}

		public bool IsBuffered {
			get { return is_buffered; }
		}

		public BodyWriter CreateBufferedCopy (
			int maxBufferSize)
		{
			return OnCreateBufferedCopy (maxBufferSize);
		}

		public void WriteBodyContents (XmlDictionaryWriter writer)
		{
			OnWriteBodyContents (writer);
		}

		[MonoTODO ("use maxBufferSize somewhere")]
		protected virtual BodyWriter OnCreateBufferedCopy (
			int maxBufferSize)
		{
			var s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			s.ConformanceLevel = ConformanceLevel.Auto;
			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter w = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw, s)))
				WriteBodyContents (w);
			return new XmlReaderBodyWriter (sw.ToString ());
		}

		protected abstract void OnWriteBodyContents (
			XmlDictionaryWriter writer);
	}
}
