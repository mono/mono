<%@ WebService Language="c#" Codebehind="main.wsdl.cs" Class="main.wsdl.Main" %>

using System.Xml.Serialization;
using System;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;

namespace main.wsdl{

/// <remarks/>
[System.Web.Services.WebServiceBindingAttribute(Name="MainSoap", Namespace="http://msdn.microsoft.com/vbasic/")]
public class Main : System.Web.Services.WebService {

    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://msdn.microsoft.com/vbasic/About", RequestNamespace="http://msdn.microsoft.com/vbasic/", ResponseNamespace="http://msdn.microsoft.com/vbasic/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string About(string X_WS_ReturnValue_X)
		{
			return null;
		}
    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://msdn.microsoft.com/vbasic/GetCustomerOrderHistory", RequestNamespace="http://msdn.microsoft.com/vbasic/", ResponseNamespace="http://msdn.microsoft.com/vbasic/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public CustomerAndOrderHistoryInfo GetCustomerOrderHistory( string  strCustID, CustomerAndOrderHistoryInfo X_WS_ReturnValue_X)
		{
			return null;
		}
    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://msdn.microsoft.com/vbasic/GetTenMostExpensiveProducts", RequestNamespace="http://msdn.microsoft.com/vbasic/", ResponseNamespace="http://msdn.microsoft.com/vbasic/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public System.Data.DataSet GetTenMostExpensiveProducts(System.Data.DataSet X_WS_ReturnValue_X)
		{
			return null;
		}
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://msdn.microsoft.com/vbasic/")]
public class CustomerAndOrderHistoryInfo {
    
    /// <remarks/>
    public System.Xml.XmlElement Orders;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Company;
}
}