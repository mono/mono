//
// WebServicesExtension.cs: Web Service Extension
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Web.Services.Protocols;

namespace Microsoft.Web.Services {

	public class WebServicesExtension : SoapExtension {

		public WebServicesExtension () {}

		[MonoTODO]
		public override Stream ChainStream (Stream stream) 
		{
			return null;
		}

		[MonoTODO]
		public override object GetInitializer (LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute) 
		{
			return null;
		}

		[MonoTODO]
		public override object GetInitializer (Type type) 
		{
			return null;
		}

		[MonoTODO]
		public override void Initialize (object initializer) 
		{
		}

		[MonoTODO]
		public override void ProcessMessage (SoapMessage message) 
		{
			if (message == null)
				throw new ArgumentNullException ("message");

			switch (message.Stage) {
				case SoapMessageStage.BeforeSerialize:
					// TODO
					break;
				case SoapMessageStage.AfterSerialize:
					// TODO
					break;
				case SoapMessageStage.BeforeDeserialize:
					// TODO
					break;
				case SoapMessageStage.AfterDeserialize:
					// TODO
					break;
			}
		}
	} 
}
