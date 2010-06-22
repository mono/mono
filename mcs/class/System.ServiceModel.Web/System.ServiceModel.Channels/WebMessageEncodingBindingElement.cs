//
// WebMessageEncodingBindingElement.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Channels
{
	public sealed class WebMessageEncodingBindingElement
#if NET_2_1
		: MessageEncodingBindingElement
#else
		: MessageEncodingBindingElement, IWsdlExportExtension
#endif
	{
		Encoding write_encoding;
		XmlDictionaryReaderQuotas reader_quotas;
		WebContentTypeMapper content_type_mapper;
		int max_read_pool_size = 0x10000, max_write_pool_size = 0x10000;

		// Constructors

		public WebMessageEncodingBindingElement ()
			: this (Encoding.UTF8)
		{
		}

		public WebMessageEncodingBindingElement (Encoding writeEncoding)
		{
			if (writeEncoding == null)
				throw new ArgumentNullException ("writeEncoding");
			WriteEncoding = writeEncoding;
#if !NET_2_1
			reader_quotas = new XmlDictionaryReaderQuotas ();
#endif
		}

		// Properties

		public WebContentTypeMapper ContentTypeMapper {
			get { return content_type_mapper; }
			set { content_type_mapper = value; }
		}

		[MonoTODO]
		public int MaxReadPoolSize {
			get { return max_read_pool_size; }
			set { max_read_pool_size = value; }
		}

		[MonoTODO]
		public int MaxWritePoolSize {
			get { return max_write_pool_size; }
			set { max_write_pool_size = value; }
		}

		public override MessageVersion MessageVersion {
			get { return MessageVersion.None; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				if (!value.Equals (MessageVersion.None))
					throw new ArgumentException ("Only MessageVersion.None is supported for WebMessageEncodingBindingElement");
			}
		}

		public XmlDictionaryReaderQuotas ReaderQuotas {
			get { return reader_quotas; }
		}

		public Encoding WriteEncoding {
			get { return write_encoding; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				write_encoding = value;
			}
		}

		// Methods

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			context.RemainingBindingElements.Add (this);
			return base.BuildChannelFactory<TChannel> (context);
		}

#if !NET_2_1
		[MonoTODO ("Why is it overriden?")]
		public override bool CanBuildChannelListener<TChannel> (BindingContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			return context.CanBuildInnerChannelListener<TChannel> ();
		}

		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (BindingContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			context.RemainingBindingElements.Add (this);
			return base.BuildChannelListener<TChannel> (context);
		}
#endif

		public override BindingElement Clone ()
		{
			return (WebMessageEncodingBindingElement) MemberwiseClone ();
		}

		public override MessageEncoderFactory CreateMessageEncoderFactory ()
		{
			return new WebMessageEncoderFactory (this);
		}

		public override T GetProperty<T> (BindingContext context)
		{
			if (typeof (T) == typeof (MessageVersion))
				return (T) (object) MessageVersion;
			if (typeof (T) == typeof (XmlDictionaryReaderQuotas))
				return (T) (object) ReaderQuotas;
			return context.GetInnerProperty<T> ();
		}

#if !NET_2_1
		[MonoTODO]
		void IWsdlExportExtension.ExportContract (WsdlExporter exporter, WsdlContractConversionContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IWsdlExportExtension.ExportEndpoint (WsdlExporter exporter, WsdlEndpointConversionContext context)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
