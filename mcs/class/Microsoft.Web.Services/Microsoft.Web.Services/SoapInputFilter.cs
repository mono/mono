//
// SoapIntputFilter.cs: SOAP Input Filter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Xml;

namespace Microsoft.Web.Services {

	public abstract class SoapInputFilter {

		public SoapInputFilter () {}

		[MonoTODO("always return true - for now")]
		protected virtual bool CanProcessHeader (XmlElement header, SoapContext context) 
		{
			if (header == null)
				throw new ArgumentNullException ("header");
			if (context == null)
				throw new ArgumentNullException ("context");
			// The header can be processed if any of the following conditions are true: 
			// 1. Actor is equal to ActorNext.
			if (context.Actor.AbsoluteUri == Soap.ActorNext)
				return true;
			// 2. Actor matches this node. 
			// 3. Actor is empty and the IsIntermediary property of context is false. 
			//if ((context.Actor == null) && (context.IsIntermediary))
				return true;
			//return false;
		}
		
		public abstract void ProcessMessage (SoapEnvelope envelope);
	} 
}
