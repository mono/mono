<%@ WebService Language="c#" Codebehind="spellcheckservice.wsdl.cs" Class="WebService3.DuplicateArrayTest" %>

using System.Xml.Serialization;
using System;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;

namespace WebService3{

/// <remarks/>
[System.Web.Services.WebServiceBindingAttribute(Name="SpellCheckServiceSoap", Namespace="http://www.worldwidedesktop.com/spellcheck")]
public class DuplicateArrayTest : System.Web.Services.WebService {
    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.worldwidedesktop.com/spellcheck/SpellCheck", RequestNamespace="http://www.worldwidedesktop.com/spellcheck", ResponseNamespace="http://www.worldwidedesktop.com/spellcheck", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    [return: System.Xml.Serialization.XmlArrayItemAttribute("correction", Namespace="http://www.worldwidedesktop.com/spellcheck/SpellCheckResult.xsd", IsNullable=false)]
    public correctionsCorrection[] SpellCheck(ref string  LicenseText, ref string  TextToCheck,  correctionsCorrection[] X_WS_ReturnValue_X)
		{
		//Fill Parameters
		// --- Param 0---
		LicenseText = LicenseText;
		// --- Param 1---
		TextToCheck = TextToCheck;
		
		//Return Value
		return X_WS_ReturnValue_X;
		}
    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.worldwidedesktop.com/spellcheck/MSSpellCheck", RequestNamespace="http://www.worldwidedesktop.com/spellcheck", ResponseNamespace="http://www.worldwidedesktop.com/spellcheck", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public System.Xml.XmlNode MSSpellCheck(ref string  LicenseText, ref string  TextToCheck,  System.Xml.XmlNode X_WS_ReturnValue_X)
		{
		//Fill Parameters
		// --- Param 0---
		LicenseText = LicenseText;
		// --- Param 1---
		TextToCheck = TextToCheck;
		
		//Return Value
		return X_WS_ReturnValue_X;
		}
    
    /// <remarks/>
    [System.Web.Services.WebMethodAttribute()]
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.worldwidedesktop.com/spellcheck/WSpellCheck", RequestNamespace="http://www.worldwidedesktop.com/spellcheck", ResponseNamespace="http://www.worldwidedesktop.com/spellcheck", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public System.Xml.XmlNode WSpellCheck(ref string  LicenseText, ref string  TextToCheck,  System.Xml.XmlNode X_WS_ReturnValue_X)
		{
		//Fill Parameters
		// --- Param 0---
		LicenseText = LicenseText;
		// --- Param 1---
		TextToCheck = TextToCheck;
		
		//Return Value
		return X_WS_ReturnValue_X;
		}
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.worldwidedesktop.com/spellcheck/SpellCheckResult.xsd")]
public class correctionsCorrection {
    
    /// <remarks/>
    public string word;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("suggestion", IsNullable=false)]
    public string[] suggestions;
}
}
