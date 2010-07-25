<%@ WebService Language="c#" Codebehind="InteropTestDocLitParameters.wsdl.cs" Class="InteropTestDocLitParameters.wsdl.WSDLInteropTestDocLitParameters" %>
using System.Xml.Serialization;
using System;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;

namespace InteropTestDocLitParameters.wsdl{

/// <remarks/>
[System.Web.Services.WebServiceBindingAttribute(Name="WSDLInteropTestDocLitParametersSoap", Namespace="http://soapinterop.org")]
public class WSDLInteropTestDocLitParameters : System.Web.Services.WebService {

   
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soapinterop.org/", RequestNamespace="http://soapinterop.org/xsd", ResponseNamespace="http://soapinterop.org/xsd", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    [return: System.Xml.Serialization.XmlElementAttribute("return", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public SOAPStruct echoStruct([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]  SOAPStruct  param0, SOAPStruct X_WS_ReturnValue_X)
		{
			return null;
		}
    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soapinterop.org/", RequestNamespace="http://soapinterop.org/xsd", ResponseNamespace="http://soapinterop.org/xsd", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    [return: System.Xml.Serialization.XmlArrayAttribute("return", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [return: System.Xml.Serialization.XmlArrayItemAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
    public string[] echoStringArray([System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] [System.Xml.Serialization.XmlArrayItemAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]  string[]  param0, string[] X_WS_ReturnValue_X)
		{
			return null;
		}
    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soapinterop.org/", RequestNamespace="http://soapinterop.org/xsd", ResponseNamespace="http://soapinterop.org/xsd", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    [return: System.Xml.Serialization.XmlElementAttribute("return", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string echoString([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]  string  param0, string X_WS_ReturnValue_X)
		{
			return null;
		}
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://soapinterop.org/xsd")]
public class SOAPStruct {
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public System.Single varFloat;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public int varInt;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string varString;
}
}
