// 
// System.Web.Services.Description.SoapHttpTransportImporter.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
//


namespace System.Web.Services.Description 
{
	internal class SoapHttpTransportImporter : SoapTransportImporter
	{
		public SoapHttpTransportImporter ()
		{
		}
		
		public override void ImportClass ()
		{
		}
		
		public override bool IsSupportedTransport (string transport)
		{
			return (transport == SoapBinding.HttpTransport);
		}
	}
}
