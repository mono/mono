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

		public override MessageVersion MessageVersion {
			get { return version; }
			set { version = value; }
		}

		public Encoding WriteEncoding {
			get { return encoding; }
			set { encoding = value; }
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
			return new MtomMessageEncodingBindingElement (
				version, encoding);
		}

		[MonoTODO]
		public override T GetProperty<T> (BindingContext context)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		void IWsdlExportExtension.ExportEndpoint (WsdlExporter exporter,
			WsdlEndpointConversionContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ExportPolicy (MetadataExporter exporter,
			PolicyConversionContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
