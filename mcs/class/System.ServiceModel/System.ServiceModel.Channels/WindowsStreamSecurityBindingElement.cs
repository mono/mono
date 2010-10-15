//
// WindowsStreamSecurityBindingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Channels
{
	[MonoTODO]
	public class WindowsStreamSecurityBindingElement
		: BindingElement, ISecurityCapabilities, IPolicyExportExtension
	{
		public WindowsStreamSecurityBindingElement ()
		{
		}

		public WindowsStreamSecurityBindingElement (
			WindowsStreamSecurityBindingElement other)
			: base (other)
		{
			ProtectionLevel = other.ProtectionLevel;
		}

		public ProtectionLevel ProtectionLevel { get; set; }

		public override IChannelFactory<TChannel>
			BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			return context.BuildInnerChannelFactory<TChannel> ();
		}

		public override IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			BindingContext context)
		{
			return context.BuildInnerChannelListener<TChannel> ();
		}

		public override bool CanBuildChannelFactory<TChannel> (
			BindingContext context)
		{
			return context.CanBuildInnerChannelFactory<TChannel> ();
		}

		public override bool CanBuildChannelListener<TChannel> (
			BindingContext context)
		{
			return context.CanBuildInnerChannelListener<TChannel> ();
		}

		public override BindingElement Clone ()
		{
			return new WindowsStreamSecurityBindingElement (this);
		}

		public override T GetProperty<T> (BindingContext context)
		{
			if (typeof (T) == typeof (ISecurityCapabilities))
				return (T) (object) this;
			if (typeof (T) == typeof (IdentityVerifier))
				return (T) (object) IdentityVerifier.CreateDefault ();
			return null;
		}

		#region explicit interface implementations
		[MonoTODO]
		ProtectionLevel ISecurityCapabilities.SupportedRequestProtectionLevel {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		ProtectionLevel ISecurityCapabilities.SupportedResponseProtectionLevel {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool ISecurityCapabilities.SupportsClientAuthentication {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool ISecurityCapabilities.SupportsClientWindowsIdentity {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool ISecurityCapabilities.SupportsServerAuthentication {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		void IPolicyExportExtension.ExportPolicy (
			MetadataExporter exporter,
			PolicyConversionContext policyContext)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
