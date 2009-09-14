//
// MsmqIntegrationBindingElement.cs
//
// Author: Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel.Channels;

namespace System.ServiceModel.MsmqIntegration
{
	public sealed class MsmqIntegrationBindingElement : MsmqBindingElementBase
	{
		MsmqMessageSerializationFormat format;
		Type [] target_types;

		public MsmqIntegrationBindingElement ()
		{
		}

		public MsmqMessageSerializationFormat SerializationFormat {
			get { return format; }
			set { format = value; }
		}

		public override string Scheme {
			get { return "net.msmq"; }
		}

		public Type[] TargetSerializationTypes {
			get { return target_types; }
			set { target_types = value; }
		}

		public override BindingElement Clone ()
		{
			return (MsmqIntegrationBindingElement) MemberwiseClone ();
		}

		public override bool CanBuildChannelFactory<TChannel> (BindingContext context)
		{
			if (typeof (TChannel) == typeof (IOutputChannel))
				return true;
			else
				return false;
		}

		[MonoTODO]
		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext context)
		{
			if (!CanBuildChannelFactory<TChannel> (context))
				throw new InvalidOperationException ("MSMQ integration binding element only supports IOutputChannel");

			throw new NotImplementedException ();
		}

		public override bool CanBuildChannelListener<TChannel> (BindingContext context)
		{
			if (typeof (TChannel) == typeof (IInputChannel))
				return true;
			else
				return false;
		}

		[MonoTODO]
		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (BindingContext context)
		{
			if (!CanBuildChannelListener<TChannel> (context))
				throw new InvalidOperationException ("MSMQ integration binding element only supports IOutputChannel");

			throw new NotImplementedException ();
		}

		public override T GetProperty<T> (BindingContext context)
		{
			// http://blogs.msdn.com/drnick/archive/2007/04/10/interfaces-for-getproperty-part-1.aspx
			if (typeof (T) == typeof (MessageVersion))
				return (T) (object) MessageVersion.None;
			return base.GetProperty<T> (context);
		}
	}
}
