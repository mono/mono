var xml = WScript.CreateObject ("MSXML2.DOMDocument");
var xsl = WScript.CreateObject ("MSXML2.DOMDocument");

xml.async = false;
xsl.async = false;

xml.load (WScript.Arguments (0));
xsl.load (WScript.Arguments (1));

WScript.Echo (xml.transformNode (xsl));

