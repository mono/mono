<%@ WebService Language="c#" Codebehind="BusinessList.wsdl.cs" Class="BusinessList.wsdl.BusinessList" %>
using System.Xml.Serialization;
using System;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;

namespace BusinessList.wsdl{

/// <remarks/>
[System.Web.Services.WebServiceBindingAttribute(Name="BusinessListSoap", Namespace="http://tempuri.org/")]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(UddiCore))]
public class BusinessList : System.Web.Services.WebService {

   
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetBusinessList", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public BusinessInfo[] GetBusinessList( string  Name, BusinessInfo[] X_WS_ReturnValue_X)
		{
		return null;
		}
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
public class BusinessInfo : UddiCore {
    
    /// <remarks/>
    public string name;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("description")]
    public Description[] description;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("serviceInfo")]
    public ServiceInfo[] serviceInfos;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string businessKey;
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
public class Description : UddiCore {
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified, Namespace="http://www.w3.org/XML/1998/namespace")]
    public string lang;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string[] Text;
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(ServiceInfo))]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(Description))]
[System.Xml.Serialization.XmlIncludeAttribute(typeof(BusinessInfo))]
public class UddiCore {
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
public class ServiceInfo : UddiCore {
    
    /// <remarks/>
    public string name;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string serviceKey;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string businessKey;
}
}
