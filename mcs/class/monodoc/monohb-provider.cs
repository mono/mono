//
// monohb-provider.cs: Handbook provider for Monodoc
//
// Authors:
// Copyright 2003 Lee Mallabone <gnome@fonicmonkey.net>
//   Johannes Roith <johannes@jroith.de>
//   Miguel de Icaza <miguel@ximian.com>
//
namespace Monodoc {
using System.Xml;
using System;

/**
 * Processes the mono handbook to remove extra web-specific div sections.
 */
public class MonoHBHelpSource: XhtmlHelpSource

{
	public MonoHBHelpSource (string base_file, bool create) : base (base_file, create)
	{
	}
	
	public override XmlDocument ProcessContent(XmlDocument docToProcess)
	{

		Console.WriteLine ("x1");
		XmlNamespaceManager nsmgr = new XmlNamespaceManager(docToProcess.NameTable);
		nsmgr.AddNamespace("default", "http://www.w3.org/1999/xhtml");
		nsmgr.AddNamespace("monodoc", "http://www.go-mono.org/xml/monodoc");
		nsmgr.PushScope();

		Console.WriteLine ("x2");
		XmlElement root = docToProcess.DocumentElement;
		XmlNode body = root.SelectSingleNode("/default:html/default:body", nsmgr);

		// Use the DC.Description meta tag as sign, that the file is in the new format

		Console.WriteLine ("x3");
		if (root.SelectNodes("/default:html/default:head/default:meta[@name='DC.Description']", nsmgr).Count != 0)

		{


			//////////////////////////////////////////////////////////////////////
			// Start of temporary code, until the tutorial is converted completely
			//////////////////////////////////////////////////////////////////////

			XmlNodeList nodeList = docToProcess.GetElementsByTagName("div");
		
			/* Remove the mono handbook specific decorations */
		        foreach(XmlNode node in nodeList)
			{
				string cssClass = ((XmlElement)node).GetAttribute("class");
				if (cssClass != null && (cssClass == "topframe" || cssClass == "navbar" || cssClass == "copyright"))
				{
					node.RemoveAll();
				}
	                                                                                
			}
	                                                                                
	
			string headinginner = "Mono Handbook";
			XmlNode firstheading = docToProcess.GetElementsByTagName("title")[0];
			headinginner = firstheading.InnerXml;
	
	
			try {
	
				XmlNode bodynode =  docToProcess.GetElementsByTagName("body")[0];
				bodynode.InnerXml =	"<table width=\"100%\">" +
					"<tr bgcolor=\"#b0c4de\"><td><i></i>Mono Handbook<h3>" + headinginner + "</h3></td></tr></table><p />" +
				bodynode.InnerXml;
			}
			catch {
			}

		Console.WriteLine ("x5");
			
	
			//////////////////////////////////////////////////////////////////////
			// End of temporary code, until the tutorial is converted completely
			//////////////////////////////////////////////////////////////////////
	
			XmlNodeList nodeList2 = docToProcess.GetElementsByTagName("pre");
			foreach(XmlNode node in nodeList2)
			{
				string cssClass = ((XmlElement)node).GetAttribute("class");
	
				if (cssClass != null) {
	
					switch(cssClass)       
					{         
	
					case "code":   
		
						node.InnerXml = "<table width='100%' border='0' cellspacing='0' cellpadding='3'><tr><td>" + 
							"<table width='100%' border='0' cellspacing='0' cellpadding='0' height='100%'>" + 
							"<tr><td bgcolor='#c0c0c0'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
							"<td bgcolor='#c0c0c0'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
							"<td bgcolor='#c0c0c0'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
							"</tr><tr> <td bgcolor='#c0c0c0'> </td><td width='100%' bgcolor='#efefef'> " + 
							"<table width='100%' border='0' cellspacing='0' cellpadding='0'><tr><td bgcolor='#ffffff'>" + 
							"<table width='100%' border='0' cellspacing='0' cellpadding='2'><tr><td bgcolor='#efefef'><pre>" + 
	
						node.InnerXml +
		
							"</pre></td></tr></table></td></tr></table></td><td bgcolor='#999999'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
							"</tr><tr> <td bgcolor='#c0c0c0'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
							"<td bgcolor='#c0c0c0'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
							"<td bgcolor='#c0c0c0'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
							"</tr></table></td></tr></table>";
	
					break;        
			     	    	case "console":   
		
						node.InnerXml = "<table width='100%' border='0' cellspacing='0' cellpadding='3'><tr><td>" + 
							"<table width='100%' border='0' cellspacing='0' cellpadding='0' height='100%'>" + 
							"<tr><td bgcolor='#555555'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
							"<td bgcolor='#555555'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
							"<td bgcolor='#555555'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
							"</tr><tr> <td bgcolor='#c0c0c0'> </td><td width='100%' bgcolor='#efefef'> " + 
							"<table width='100%' border='0' cellspacing='0' cellpadding='0'><tr><td bgcolor='#ffffff'>" + 
							"<table width='100%' border='0' cellspacing='0' cellpadding='2'><tr><td bgcolor='#999999'><pre>" + 
	
						node.InnerXml +
	
						"</pre></td></tr></table></td></tr></table></td><td bgcolor='#999999'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"</tr><tr> <td bgcolor='#555555'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"<td bgcolor='#555555'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"<td bgcolor='#555555'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"</tr></table></td></tr></table>";
					break;
	
					default:
					break;      
				}
		
		
				}
		
			}

			nodeList = root.SelectNodes("//monodoc:example", nsmgr);
			foreach (XmlNode node in nodeList) {
				//XmlNode csnode = root.SelectSingleNode("/monodoc:source[@lang='CS']", nsmgr);
				body.RemoveChild(node);
		
			}

			//string copyright_string = ""; // no string for now
	
			nodeList = root.SelectNodes("//default:link[@type='text/css']", nsmgr);
			if (nodeList.Count == 0) {
	
				// TODO: Stylesheet path maybe variable
				root.SelectSingleNode("/default:html/default:head", nsmgr).InnerXml += "\n	<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\" />";
			}
	
			string contributor_string = "<a id=\"credits\"><h2>Credits</h2></a>";
	
			nodeList = root.SelectNodes("//default:meta[@name='DC.Contributor']", nsmgr);
			foreach (XmlNode node in nodeList) {
		
				contributor_string += node.Attributes.GetNamedItem("content").Value + "<br />\n";
		
			}
	
			//			body.InnerXml += contributor_string + copyright_string;
	
		}
		else {
		Console.WriteLine ("x4");

		XmlNodeList nodeList = docToProcess.GetElementsByTagName("div");

		if (nodeList != null){
		/* Remove the mono handbook specific decorations */
		foreach(XmlNode node in nodeList) {
			string cssClass = ((XmlElement)node).GetAttribute("class");
			if (cssClass != null && (cssClass == "topframe" || cssClass == "navbar" || cssClass == "copyright")){
				node.RemoveAll();
			}
			
		}
		}
                                                                                
		Console.WriteLine ("x6");

		string headinginner = "Mono Handbook";
		XmlNode firstheading = null;
		
		try {
			firstheading =  docToProcess.GetElementsByTagName("h1")[0];
			headinginner = firstheading.InnerXml;
		}
		
		catch {
			
			try {
				
				firstheading =  docToProcess.GetElementsByTagName("h2")[0];
				headinginner = firstheading.InnerXml;
				
			}
			catch {}
		}
		
		Console.WriteLine ("x8");
		
		try {
			
			XmlNode bodynode =  docToProcess.GetElementsByTagName("body")[0];
			if (firstheading != null)
				bodynode.RemoveChild(firstheading);
			
			bodynode.InnerXml =	"<table width=\"100%\">" +
				"<tr bgcolor=\"#b0c4de\"><td><i></i>Mono Handbook<h3>" + headinginner + "</h3></td></tr></table><p />" +
				bodynode.InnerXml;
		}
		catch {
		}
		
		Console.WriteLine ("x9");
		XmlNodeList nodeList2 = docToProcess.GetElementsByTagName("pre");
		foreach(XmlNode node in nodeList2){
			string cssClass = ((XmlElement)node).GetAttribute("class");

			if (cssClass != null){
				switch(cssClass) {
					
				case "code":   
					
					node.InnerXml = "<table width='100%' border='0' cellspacing='0' cellpadding='3'><tr><td>" + 
						"<table width='100%' border='0' cellspacing='0' cellpadding='0' height='100%'>" + 
						"<tr><td bgcolor='#c0c0c0'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"<td bgcolor='#c0c0c0'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"<td bgcolor='#c0c0c0'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"</tr><tr> <td bgcolor='#c0c0c0'> </td><td width='100%' bgcolor='#efefef'> " + 
						"<table width='100%' border='0' cellspacing='0' cellpadding='0'><tr><td bgcolor='#ffffff'>" + 
						"<table width='100%' border='0' cellspacing='0' cellpadding='2'><tr><td bgcolor='#efefef'>" +
						"<pre>" +
						
						node.InnerXml +
						
						"</pre>" +
						"</td></tr></table></td></tr></table></td><td bgcolor='#999999'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"</tr><tr> <td bgcolor='#c0c0c0'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"<td bgcolor='#c0c0c0'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"<td bgcolor='#c0c0c0'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"</tr></table></td></tr></table>";
					
					break;        
				case "console":   
					
					node.InnerXml = "<table width='100%' border='0' cellspacing='0' cellpadding='3'><tr><td>" + 
						"<table width='100%' border='0' cellspacing='0' cellpadding='0' height='100%'>" + 
						"<tr><td bgcolor='#555555'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"<td bgcolor='#555555'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"<td bgcolor='#555555'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"</tr><tr> <td bgcolor='#c0c0c0'> </td><td width='100%' bgcolor='#efefef'> " + 
						"<table width='100%' border='0' cellspacing='0' cellpadding='0'><tr><td bgcolor='#ffffff'>" + 
						"<table width='100%' border='0' cellspacing='0' cellpadding='2'><tr><td bgcolor='#999999'>" + 
						"<pre>" +
						
						node.InnerXml +
						
						"</pre>" + 
						"</td></tr></table></td></tr></table></td><td bgcolor='#999999'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"</tr><tr> <td bgcolor='#555555'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"<td bgcolor='#555555'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"<td bgcolor='#555555'><img src='/html/en/images/empty.png' width='1' height='1' /></td>" + 
						"</tr></table></td></tr></table>";
					break;
					
				default:
					break;      
				}
				
				
			}
		}
		
		}
					       return docToProcess;
	}
}
}
