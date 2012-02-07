//
// System.ServiceModel.FaultException.cs
//
// Author: Duncan Mak (duncan@novell.com)
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
	public class FaultException<TDetail> : FaultException
	{
		TDetail detail;

#if MONOTOUCH
		// WCF creates FaultExceptions using reflection, so unless we reference
		// the corresponding ctor, it will not be possible to use FaultExceptions
		// in MonoTouch. This ctor reference will work as long as TDetail
		// is a reference type.
		static FaultException<object> ctor_reference = new FaultException<object> (new object (), new FaultReason ("reason"), new FaultCode ("code"), "action");
#endif

		public FaultException (TDetail detail)
			: this (detail, "Unspecified ServiceModel Fault.")
		{
		}

		[MonoTODO]
		protected FaultException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			throw new NotImplementedException ();
		}

		public FaultException (TDetail detail, string reason)
			: this (detail, new FaultReason (reason)) {}
		
		public FaultException (TDetail detail, FaultReason reason)
			: this (detail, reason, FaultCode.CreateSenderFaultCode (null)) {}

		public FaultException (TDetail detail, string reason, FaultCode code)
			: this (detail, new FaultReason (reason), code) {}
		
		public FaultException (TDetail detail, FaultReason reason, FaultCode code)
			: this (detail, reason, code, null) {}

		public FaultException (TDetail detail, string reason, FaultCode code, string action)
			: this (detail, new FaultReason (reason), code, action) {}

		public FaultException (TDetail detail, FaultReason reason, FaultCode code, string action)
			: base (reason, code, action)
		{
			this.detail = detail;
		}

		public override MessageFault CreateMessageFault ()
		{
			return MessageFault.CreateFault (Code, Reason, detail);
		}

		[MonoTODO ("see FaultException.TestGetObjectData to see how it's serialized")]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("detail", detail);
		}

		public override string ToString ()
		{
			return String.Format ("{0}: {1} (Fault Detail is equal to {2}).",
					      this.GetType (), Message, Detail);
		}

		public TDetail Detail { get { return detail ; } }
	}
}