<%@ WebService Language="c#" Codebehind="InteropTestExtensibilityRequired.wsdl.cs" Class="InteropTestExtensibilityRequired.wsdl.InteropTestExtensibilityRequired" %>
using System.Xml.Serialization;
using System;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;

namespace InteropTestExtensibilityRequired.wsdl{

/// <remarks/>
[System.Web.Services.WebServiceBindingAttribute(Name="InteropTestExtensibilityRequiredSoap", Namespace="http://tempuri.org/")]
public class InteropTestExtensibilityRequired : System.Web.Services.WebService {

	//Added by ServerGeneration
    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soapinterop.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return: System.Xml.Serialization.XmlElementAttribute("echoStringReturn", Namespace="http://soapinterop.org/xsd")]
    public string echoString([System.Xml.Serialization.XmlElementAttribute(Namespace="http://soapinterop.org/xsd")]  string  echoStringParam, string X_WS_ReturnValue_X)
		{
			return null;
		}
	}
}