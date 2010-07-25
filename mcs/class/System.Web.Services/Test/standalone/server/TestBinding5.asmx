<%@ WebService Language="c#" Codebehind="GetData.wsdl.cs" Class="GetData.wsdl.GetData" %>

using System.Xml.Serialization;
using System;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;

namespace GetData.wsdl{

	[System.Web.Services.WebServiceBindingAttribute(Name="GetDataSoap", Namespace="http://IUnknown.Team/ePortal/GetData")]
	public class GetData : System.Web.Services.WebService {

  		
		[System.Web.Services.WebMethodAttribute()]
    		public bool RebuildTabList(bool X_WS_ReturnValue_X)
		{
			return false;
		}
    


	}
}