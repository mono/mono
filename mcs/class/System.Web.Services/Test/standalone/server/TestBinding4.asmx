<%@ WebService Language="c#" Codebehind="GetData.wsdl.cs" Class="GetData.wsdl.GetData" %>

using System.Xml.Serialization;
using System;
using System.Xml;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;

namespace GetData.wsdl{

/// <remarks/>
[System.Web.Services.WebServiceBindingAttribute(Name="GetDataSoap", Namespace="http://IUnknown.Team/ePortal/GetData")]
public class GetData : System.Web.Services.WebService {

    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://IUnknown.Team/ePortal/GetData/GetTabList", ResponseNamespace="http://IUnknown.Team/ePortal/GetData", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return: System.Xml.Serialization.XmlTextAttribute()]
    [return: System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.XmlNode[] GetTabList(System.Xml.XmlNode[] X_WS_ReturnValue_X)
	{
		return X_WS_ReturnValue_X;
	}
    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://IUnknown.Team/ePortal/GetData/RebuildTabList", RequestNamespace="http://IUnknown.Team/ePortal/GetData", ResponseNamespace="http://IUnknown.Team/ePortal/GetData", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public bool RebuildTabList(bool X_WS_ReturnValue_X)
		{
			return X_WS_ReturnValue_X;
		}
    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://IUnknown.Team/ePortal/GetData/RebuildTabStruct", RequestNamespace="http://IUnknown.Team/ePortal/GetData", ResponseNamespace="http://IUnknown.Team/ePortal/GetData", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public bool RebuildTabStruct(string tabname, bool X_WS_ReturnValue_X)
		{
			return X_WS_ReturnValue_X;
		}
    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://IUnknown.Team/ePortal/GetData/GetTabStruct", ResponseNamespace="http://IUnknown.Team/ePortal/GetData", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return: System.Xml.Serialization.XmlTextAttribute()]
    [return: System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.XmlNode[] GetTabStruct([System.Xml.Serialization.XmlElementAttribute(Namespace="http://IUnknown.Team/ePortal/GetData")]  string  tabname, System.Xml.XmlNode[] X_WS_ReturnValue_X)
		{
			return X_WS_ReturnValue_X;
		}
	}
}
