//
// FaultConverter.cs
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
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
	public abstract class FaultConverter
	{
		public static FaultConverter GetDefaultFaultConverter (MessageVersion version)
		{
			if (version == null)
				throw new ArgumentNullException ("version");
			return new SimpleFaultConverter (version);
		}

		protected FaultConverter ()
		{
		}

		[MonoTODO]
		protected abstract bool OnTryCreateException (
			Message message, MessageFault fault, out Exception error);

		[MonoTODO]
		protected abstract bool OnTryCreateFaultMessage (
			Exception error, out Message message);

		public bool TryCreateException (Message message, MessageFault fault, out Exception error)
		{
			return OnTryCreateException (message, fault, out error);
		}

		public bool TryCreateFaultMessage (Exception error, out Message message)
		{
			return OnTryCreateFaultMessage (error, out message);
		}

		Message CreateSimpleFaultMessage (Exception error, MessageVersion version)
		{
			OperationContext ctx = OperationContext.Current;
			// FIXME: support more fault code depending on the exception type.
			FaultCode fc = null;
			if (error is EndpointNotFoundException)
				fc = new FaultCode (
					"DestinationUnreachable",
					version.Addressing.Namespace);
			else
				fc = new FaultCode (
					"FIXME_InternalError",
					version.Addressing.Namespace);

#if !NET_2_1
			// FIXME: set correct fault reason.
			if (ctx.EndpointDispatcher.ChannelDispatcher.IncludeExceptionDetailInFaults) {
				ExceptionDetail detail = new ExceptionDetail (error);
				return Message.CreateMessage (version, fc,
					"error occured", detail, ctx.IncomingMessageHeaders != null ? ctx.IncomingMessageHeaders.Action : null);
			}
#endif
			return Message.CreateMessage (version, fc, "error occured", ctx.IncomingMessageHeaders.Action);
		}

		class SimpleFaultConverter : FaultConverter
		{
			MessageVersion version;
			public SimpleFaultConverter (MessageVersion version)
			{
				this.version = version;
			}

			protected override bool OnTryCreateException (
				Message message, MessageFault fault, out Exception error)
			{
				error = null;
				return false;
			}

			protected override bool OnTryCreateFaultMessage (
				Exception error, out Message message)
			{
				message = null;
				if (OperationContext.Current == null)
					return false;

				message = CreateSimpleFaultMessage (error, version);
				return true;
			}
		}
	}
}
