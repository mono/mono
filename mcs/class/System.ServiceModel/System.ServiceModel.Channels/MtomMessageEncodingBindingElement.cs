//
// MtomMessageEncodingBindingElement.cs
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
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Channels
{
	[MonoTODO]
	public sealed class MtomMessageEncodingBindingElement
		: MessageEncodingBindingElement,
		  IWsdlExportExtension, IPolicyExportExtension
	{
		MessageVersion version;
		Encoding encoding;
		int max_buffer_size = 0x10000, max_read_pool_size = 64, max_write_pool_size = 16;
		XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas ();

		public MtomMessageEncodingBindingElement ()
			: this (MessageVersion.Default, Encoding.UTF8)
		{
		}

		public MtomMessageEncodingBindingElement (
			MessageVersion version, Encoding encoding)
		{
			this.version = version;
			this.encoding = encoding;
		}

		public int MaxBufferSize {
			get { return max_buffer_size; }
			set { max_buffer_size = value; }
		}

		public int MaxReadPoolSize {
			get { return max_read_pool_size; }
			set { max_read_pool_size = value; }
		}

		public int MaxWritePoolSize {
			get { return max_write_pool_size; }
			set { max_write_pool_size = value; }
		}

		public override MessageVersion MessageVersion {
			get { return version; }
			set { version = value; }
		}

		public Encoding WriteEncoding {
			get { return encoding; }
			set { encoding = value; }
		}

		public XmlDictionaryReaderQuotas ReaderQuotas {
			get { return quotas; }
			set { quotas = value; }
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			//context.RemainingBindingElements.Add (this);
			return base.BuildChannelFactory<TChannel> (context);
		}

		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (
			BindingContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			//context.RemainingBindingElements.Add (this);
			return base.BuildChannelListener<TChannel> (context);
		}

		public override bool CanBuildChannelListener<TChannel> (
			BindingContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			return context.CanBuildInnerChannelListener<TChannel> ();
		}

		public override BindingElement Clone ()
		{
			return (MtomMessageEncodingBindingElement) MemberwiseClone ();
		}

		public override T GetProperty<T> (BindingContext context)
		{
			if (typeof (T) == typeof (MessageVersion))
				return (T) (object) MessageVersion;
			return context.GetInnerProperty<T> ();
		}

		public override MessageEncoderFactory
			CreateMessageEncoderFactory ()
		{
			return new MtomMessageEncoderFactory (this);
		}

		[MonoTODO]
		protected override void OnImportPolicy (XmlElement assertion,
			MessageVersion messageVersion,
			MetadataImporter exporter,
			PolicyConversionContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IWsdlExportExtension.ExportContract (WsdlExporter exporter,
			WsdlContractConversionContext context)
		{
			throw new NotImplementedException ();
		}

		void IWsdlExportExtension.ExportEndpoint (WsdlExporter exporter,
			WsdlEndpointConversionContext context)
		{
		}

		public void ExportPolicy (MetadataExporter exporter,
			PolicyConversionContext context)
		{
			PolicyAssertionCollection assertions = context.GetBindingAssertions ();
			XmlDocument doc = new XmlDocument ();
			
			assertions.Add (doc.CreateElement ("wsoma", "OptimizedMimeSerialization", "http://schemas.xmlsoap.org/ws/2004/09/policy/optimizedmimeserialization"));
		}
	}
}
