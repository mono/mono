<%@ WebService Language="c#" Class="IncludeTest" %>

using System;
using System.Collections;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Protocols;

public class IncludeTest
{
	[WebMethod] 
    [XmlInclude(typeof(ComplexThing))] 
    [SoapInclude(typeof(ComplexThing))] 
    public ArrayList foo() 
    { 
        ArrayList list = new ArrayList(); 
        list.Add(new ComplexThing("abc", 1.1f)); 
        list.Add(new ComplexThing("xyz", 2.0f)); 
        return list; 
    }
}

public class ComplexThing 
{ 
	public ComplexThing() {} 

	public ComplexThing(string name, float val) 
	{ 
		this.name = name; 
		this.val = val; 
	} 
	public string name; 
	public float val; 
}
