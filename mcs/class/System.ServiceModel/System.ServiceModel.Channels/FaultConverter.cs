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
			Message message, MessageFault fault, out Exception exception);

		[MonoTODO]
		protected abstract bool OnTryCreateFaultMessage (
			Exception exception, out Message message);

		public bool TryCreateException (Message message, MessageFault fault, out Exception exception)
		{
			return OnTryCreateException (message, fault, out exception);
		}

		public bool TryCreateFaultMessage (Exception exception, out Message message)
		{
			return OnTryCreateFaultMessage (exception, out message);
		}
	}

	class SimpleFaultConverter : FaultConverter
	{
		static readonly Dictionary<Type,string> map;
		
		static SimpleFaultConverter ()
		{
			map = new Dictionary<Type,string> ();
			map [typeof (EndpointNotFoundException)] = "DestinationUnreachable";
			map [typeof (ActionNotSupportedException)] = "ActionNotSupported";
		}

		MessageVersion version;

		public SimpleFaultConverter (MessageVersion version)
		{
			this.version = version;
		}

		protected override bool OnTryCreateException (
			Message message, MessageFault fault, out Exception error)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (fault == null)
				throw new ArgumentNullException ("fault");

			error = null;

			FaultCode fc;
			if (version.Envelope.Equals (EnvelopeVersion.Soap11))
				fc = fault.Code;
			else
				fc = fault.Code.SubCode;

			if (fc == null)
				return false;

			string msg = fault.Reason.GetMatchingTranslation ().Text;
			if (fc.Namespace == message.Version.Addressing.Namespace) {
				switch (fc.Name) {
				case "ActionNotSupported":
					error = new ActionNotSupportedException (msg);
					return true;
				case "DestinationUnreachable":
					error = new EndpointNotFoundException (msg);
					return true;
				}
			}

			return false;
		}

		protected override bool OnTryCreateFaultMessage (Exception error, out Message message)
		{
			if (version.Envelope.Equals (EnvelopeVersion.None)) {
				message = null;
				return false;
			}

			string action;
			if (!map.TryGetValue (error.GetType (), out action)) {
				message = null;
				return false;
			}

			FaultCode fc;
			if (version.Envelope.Equals (EnvelopeVersion.Soap12))
				fc = new FaultCode ("Sender", version.Envelope.Namespace, new FaultCode (action, version.Addressing.Namespace));
			else
				fc = new FaultCode (action, version.Addressing.Namespace);

			OperationContext ctx = OperationContext.Current;
			// FIXME: support more fault code depending on the exception type.
#if !MOBILE && !XAMMAC_4_5
			// FIXME: set correct fault reason.
			if (ctx != null && ctx.EndpointDispatcher.ChannelDispatcher.IncludeExceptionDetailInFaults) {
				ExceptionDetail detail = new ExceptionDetail (error);
				message = Message.CreateMessage (version, fc,
					error.Message, detail, version.Addressing.FaultNamespace);
			}
			else
#endif
				message = Message.CreateMessage (version, fc, error.Message, version.Addressing.FaultNamespace);

			return true;
		}
	}
}
