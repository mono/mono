//
// System.ServiceModel.FaultException.cs
//
// Author: Duncan Mak (duncan@novell.com)
//	   Atsushi Enomoto (atsushi@ximian.com)
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
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;

namespace System.ServiceModel
{
	[Serializable]
	public class FaultException : CommunicationException
	{
		MessageFault fault;
		string action;

		public FaultException ()
			: this ("Communication has faulted.")
		{
		}

		public FaultException (string reason)
			: this (new FaultReason (reason))
		{
		}

		public FaultException (string reason, FaultCode code)
			: this (new FaultReason (reason), code)
		{
		}

		[MonoTODO]
		protected FaultException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public FaultException (MessageFault fault)
			: this (fault, String.Empty)
		{
		}

		public FaultException (MessageFault fault, string action)
		{
			if (fault == null)
				throw new ArgumentNullException ("fault");
			//if (action == null)
			//	throw new ArgumentNullException ("action");

			this.fault = fault;
			this.action = action;
		}

		[MonoTODO]
		public FaultException (FaultReason reason)
			: this (reason, new FaultCode (String.Empty))
		{
		}

		public FaultException (FaultReason reason, FaultCode code)
			: this (MessageFault.CreateFault (code, reason))
		{
		}

		public FaultException (string reason, FaultCode code, string action)
			: this (MessageFault.CreateFault (code, reason), action)
		{
		}

		public FaultException (FaultReason reason, FaultCode code, string action)
			: this (MessageFault.CreateFault (code, reason), action)
		{
		}

		[MonoTODO]
		public static FaultException CreateFault (MessageFault messageFault,  params Type [] faultDetailTypes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static FaultException CreateFault (MessageFault messageFault, string action, params Type[] faultDetailTypes)
		{
			throw new NotImplementedException ();
		}

		public virtual MessageFault CreateMessageFault ()
		{
			return fault;
		}

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			// How could it be Serializable while none of
			// FaultReason and FaultCode are serializable.
			throw new NotImplementedException ();
		}

		public string Action {
			get { return action; }
		}

		public FaultCode Code {
			get { return fault.Code; }
		}

		public FaultReason Reason {
			get{ return fault.Reason; }
		}

		public override string Message {
			get { return Reason.GetMatchingTranslation ().Text; }
		}
	}
}
