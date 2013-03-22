//
// XmlObjectSerializer.cs
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
using System.Collections;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;

using QName = System.Xml.XmlQualifiedName;

namespace System.Runtime.Serialization
{
	public abstract class XmlObjectSerializer
	{
		// This is only for compatible mode.
		IDataContractSurrogate surrogate;

		SerializationBinder binder;
		ISurrogateSelector selector;

		int max_items = 0x10000; // FIXME: could be from config.

		protected XmlObjectSerializer ()
		{
		}

		public virtual bool IsStartObject (XmlReader reader)
		{
			return IsStartObject (XmlDictionaryReader.CreateDictionaryReader (reader));
		}

		public abstract bool IsStartObject (XmlDictionaryReader reader);

		public virtual object ReadObject (Stream stream)
		{
			var settings = new XmlReaderSettings ();
			settings.CheckCharacters = false;
			return ReadObject (XmlReader.Create (stream, settings));
		}

		public virtual object ReadObject (XmlReader reader)
		{
			return ReadObject (XmlDictionaryReader.CreateDictionaryReader (reader));
		}

		public virtual object ReadObject (XmlDictionaryReader reader)
		{
			return ReadObject (reader, true);
		}

		public virtual object ReadObject (XmlReader reader, bool readContentOnly)
		{
			return ReadObject (
				XmlDictionaryReader.CreateDictionaryReader (reader),
				readContentOnly);
		}

		[MonoTODO]
		public abstract object ReadObject (XmlDictionaryReader reader, bool readContentOnly);

		public virtual void WriteObject (Stream stream, object graph)
		{
			var settings = new XmlWriterSettings ();
			settings.Encoding = Encoding.UTF8;
			settings.CloseOutput = false;
			settings.OmitXmlDeclaration = true;
			settings.CheckCharacters = false;
			using (XmlWriter xw = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (stream, settings))) {
				WriteObject (xw, graph);
			}
		}

		public virtual void WriteObject (XmlWriter writer, object graph)
		{
			WriteObject (XmlDictionaryWriter.CreateDictionaryWriter (writer), graph);
		}

		public virtual void WriteStartObject (XmlWriter writer, object graph)
		{
			WriteStartObject (XmlDictionaryWriter.CreateDictionaryWriter (writer), graph);
		}

		public virtual void WriteObject (XmlDictionaryWriter writer, object graph)
		{
			WriteStartObject (writer, graph);
			WriteObjectContent (writer, graph);
			WriteEndObject (writer);
		}

		public abstract void WriteStartObject (
			XmlDictionaryWriter writer, object graph);

		public virtual void WriteObjectContent (XmlWriter writer, object graph)
		{
			WriteObjectContent (
				XmlDictionaryWriter.CreateDictionaryWriter (writer),
				graph);
		}

		public abstract void WriteObjectContent (
			XmlDictionaryWriter writer, object graph);

		public virtual void WriteEndObject (XmlWriter writer)
		{
			WriteEndObject (XmlDictionaryWriter.CreateDictionaryWriter (writer));
		}

		public abstract void WriteEndObject (
			XmlDictionaryWriter writer);
	}
}
