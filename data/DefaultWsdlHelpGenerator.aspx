<%
//
// DefaultWsdlHelpGenerator.aspx: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
%>

﻿<%@ Import Namespace="System.Collections" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Xml.Serialization" %>
<%@ Import Namespace="System.Xml" %>
<%@ Import Namespace="System.Xml.Schema" %>
<%@ Import Namespace="System.Web.Services.Description" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Net" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="System.Resources" %>
<%@ Import Namespace="System.Diagnostics" %>
<%@ Import Namespace="System.CodeDom" %>
<%@ Import Namespace="System.CodeDom.Compiler" %>
<%@ Import Namespace="Microsoft.CSharp" %>
<%@ Import Namespace="Microsoft.VisualBasic" %>
<%@ Import Namespace="System.Text.RegularExpressions" %>
<%@ Assembly name="System.Web.Services" %>
<%@ Page debug="true" %>

<html>
<script language="C#" runat="server">

ServiceDescriptionCollection descriptions;
XmlSchemas schemas;

string WebServiceName;
string WebServiceDescription;
string PageName;

string DefaultBinding;
ArrayList ServiceProtocols;

string CurrentOperationName;
string CurrentOperationBinding;
string OperationDocumentation;
string CurrentOperationFormat;
bool CurrentOperationSupportsTest;
ArrayList InParams;
ArrayList OutParams;
string CurrentOperationProtocols;
int CodeTextColumns = 95;

void Page_Load(object sender, EventArgs e)
{
	descriptions = (ServiceDescriptionCollection) Context.Items["wsdls"];
	schemas = (XmlSchemas) Context.Items["schemas"];

	ServiceDescription desc = descriptions [0];
	if (schemas.Count == 0) schemas = desc.Types.Schemas;
	
	Service service = desc.Services[0];
	WebServiceName = service.Name;
	DefaultBinding = desc.Bindings[0].Name;
	WebServiceDescription = service.Documentation;
	ServiceProtocols = FindServiceProtocols (null);
	
	CurrentOperationName = Request.QueryString["op"];
	CurrentOperationBinding = Request.QueryString["bnd"];
	if (CurrentOperationName != null) BuildOperationInfo ();

	PageName = HttpUtility.UrlEncode (Path.GetFileName(Request.Path), Encoding.UTF8);

	ArrayList list = new ArrayList ();
	foreach (ServiceDescription sd in descriptions) {
		foreach (Binding bin in sd.Bindings)
			if (bin.Extensions.Find (typeof(SoapBinding)) != null) list.Add (bin);
	}

	BindingsRepeater.DataSource = list;
	Page.DataBind();
}

void BuildOperationInfo ()
{
	InParams = new ArrayList ();
	OutParams = new ArrayList ();
	
	Binding binding = FindBinding (CurrentOperationBinding);
	PortType portType = descriptions.GetPortType (binding.Type);
	Operation oper = FindOperation (portType, CurrentOperationName);
	
	OperationDocumentation = oper.Documentation;
	if (OperationDocumentation == null || OperationDocumentation == "")
		OperationDocumentation = "No additional remarks";
	
	foreach (OperationMessage opm in oper.Messages)
	{
		if (opm is OperationInput)
			BuildParameters (InParams, opm);
		else if (opm is OperationOutput)
			BuildParameters (OutParams, opm);
	}
	
	// Protocols supported by the operation
	CurrentOperationProtocols = "";
	ArrayList prots = FindServiceProtocols (CurrentOperationName);
	for (int n=0; n<prots.Count; n++) {
		if (n != 0) CurrentOperationProtocols += ", ";
		CurrentOperationProtocols += (string) prots[n];
	}
	
	CurrentOperationSupportsTest = prots.Contains ("HttpGet") || prots.Contains ("HttpPost");

	// Operation format
	OperationBinding obin = FindOperation (binding, CurrentOperationName);
	if (obin != null)
		CurrentOperationFormat = GetOperationFormat (obin);

	InputParamsRepeater.DataSource = InParams;
	InputFormParamsRepeater.DataSource = InParams;
	OutputParamsRepeater.DataSource = OutParams;
}

void BuildParameters (ArrayList list, OperationMessage opm)
{
	Message msg = descriptions.GetMessage (opm.Message);
	if (msg.Parts.Count > 0 && msg.Parts[0].Name == "parameters")
	{
		MessagePart part = msg.Parts[0];
		XmlSchemaComplexType ctype;
		if (part.Element == XmlQualifiedName.Empty)
		{
			ctype = (XmlSchemaComplexType) schemas.Find (part.Type, typeof(XmlSchemaComplexType));
		}
		else
		{
			XmlSchemaElement elem = (XmlSchemaElement) schemas.Find (part.Element, typeof(XmlSchemaElement));
			ctype = (XmlSchemaComplexType) elem.SchemaType;
		}
		XmlSchemaSequence seq = ctype.Particle as XmlSchemaSequence;
		if (seq == null) return;
		
		foreach (XmlSchemaObject ob in seq.Items)
		{
			Parameter p = new Parameter();
			p.Description = "No additional remarks";
			
			if (ob is XmlSchemaElement)
			{
				XmlSchemaElement selem = GetRefElement ((XmlSchemaElement)ob);
				p.Name = selem.Name;
				p.Type = selem.SchemaTypeName.Name;
			}
			else
			{
				p.Name = "Unknown";
				p.Type = "Unknown";
			}
			list.Add (p);
		}
	}
	else
	{
		foreach (MessagePart part in msg.Parts)
		{
			Parameter p = new Parameter ();
			p.Description = "No additional remarks";
			p.Name = part.Name;
			if (part.Element == XmlQualifiedName.Empty)
				p.Type = part.Type.Name;
			else
			{
				XmlSchemaElement elem = (XmlSchemaElement) schemas.Find (part.Element, typeof(XmlSchemaElement));
				p.Type = elem.SchemaTypeName.Name;
			}
			list.Add (p);
		}
	}
}

string GetOperationFormat (OperationBinding obin)
{
	string format = "";
	SoapOperationBinding sob = obin.Extensions.Find (typeof(SoapOperationBinding)) as SoapOperationBinding;
	if (sob != null) {
		format = sob.Style.ToString ();
		SoapBodyBinding sbb = obin.Input.Extensions.Find (typeof(SoapBodyBinding)) as SoapBodyBinding;
		if (sbb != null)
			format += " / " + sbb.Use;
	}
	return format;
}

XmlSchemaElement GetRefElement (XmlSchemaElement elem)
{
	if (!elem.RefName.IsEmpty)
		return (XmlSchemaElement) schemas.Find (elem.RefName, typeof(XmlSchemaElement));
	else
		return elem;
}

ArrayList FindServiceProtocols(string operName)
{
	ArrayList table = new ArrayList ();
	Service service = descriptions[0].Services[0];
	foreach (Port port in service.Ports)
	{
		string prot = null;
		Binding bin = descriptions.GetBinding (port.Binding);
		if (bin.Extensions.Find (typeof(SoapBinding)) != null)
			prot = "Soap";
		else 
		{
			HttpBinding hb = (HttpBinding) bin.Extensions.Find (typeof(HttpBinding));
			if (hb != null && hb.Verb == "POST") prot = "HttpPost";
			else if (hb != null && hb.Verb == "GET") prot = "HttpGet";
		}
		
		if (prot != null && operName != null)
		{
			if (FindOperation (bin, operName) == null)
				prot = null;
		}

		if (prot != null && !table.Contains (prot))
			table.Add (prot);
	}
	return table;
}

Binding FindBinding (string portName)
{
	Service service = descriptions[0].Services[0];
	foreach (Port port in service.Ports)
		if (port.Name == portName)
			return descriptions.GetBinding (port.Binding);
	return null;
}

Operation FindOperation (PortType portType, string name)
{
	foreach (Operation oper in portType.Operations) {
		if (oper.Messages.Input.Name != null) {
			if (oper.Messages.Input.Name == name) return oper;
		}
		else
			if (oper.Name == name) return oper;
	}
		
	return null;
}

OperationBinding FindOperation (Binding binding, string name)
{
	foreach (OperationBinding oper in binding.Operations) {
		if (oper.Input.Name != null) {
			if (oper.Input.Name == name) return oper;
		}
		else 
			if (oper.Name == name) return oper;
	}
		
	return null;
}

string FormatBindingName (string name)
{
	if (name == DefaultBinding) return "Methods";
	else return "Methods for binding<br>" + name;
}

string GetOpName (object op)
{
	OperationBinding ob = op as OperationBinding;
	if (ob == null) return "";
	if (ob.Input.Name != null) return ob.Input.Name;
	else return ob.Name;
}

bool HasFormResult
{
	get { return Request.QueryString ["ext"] == "testform"; }
}

string GetTestResult ()
{ 
	if (!HasFormResult) return null;
	
	bool fill = false;
	string qs = "";
	for (int n=0; n<Request.QueryString.Count; n++)
	{
		if (fill) {
			if (qs != "") qs += "&";
			qs += Request.QueryString.GetKey(n) + "=" + Request.QueryString [n];
		}
		if (Request.QueryString.GetKey(n) == "ext") fill = true;
	}
		
	string location = null;
	ServiceDescription desc = descriptions [0];
	Service service = desc.Services[0];
	foreach (Port port in service.Ports)
		if (port.Name == CurrentOperationBinding)
		{
			SoapAddressBinding sbi = (SoapAddressBinding) port.Extensions.Find (typeof(SoapAddressBinding));
			if (sbi != null)
				location = sbi.Location;
		}

	if (location == null) 
		return "Could not locate web service";
	
	try
	{
		WebRequest req = WebRequest.Create (location + "/" + CurrentOperationName + "?" + qs);
		WebResponse resp = req.GetResponse();
		StreamReader sr = new StreamReader (resp.GetResponseStream());
		string s = sr.ReadToEnd ();
		sr.Close ();
		return "<div class='code-xml'>" + ColorizeXml(WrapText(s,CodeTextColumns)) + "</div>";
	}
	catch (Exception ex)
	{ 
		string res = "<b style='color:red'>" + ex.Message + "</b>";
		WebException wex = ex as WebException;
		if (wex != null)
		{
			WebResponse resp = wex.Response;
			if (resp != null) {
				StreamReader sr = new StreamReader (resp.GetResponseStream());
				string s = sr.ReadToEnd ();
				sr.Close ();
				res += "<div class='code-xml'>" + ColorizeXml(WrapText(s,CodeTextColumns)) + "</div>";
			}
		}
		return res;
	}
}

//
// Proxy code generation
//

string GetProxyCode ()
{
	CodeNamespace codeNamespace = new CodeNamespace();
	CodeCompileUnit codeUnit = new CodeCompileUnit();
	
	codeUnit.Namespaces.Add (codeNamespace);

	ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
	
	foreach (ServiceDescription sd in descriptions)
		importer.AddServiceDescription(sd, null, null);

	foreach (XmlSchema sc in schemas)
		importer.Schemas.Add (sc);

	importer.Import(codeNamespace, codeUnit);

	string langId = Request.QueryString ["lang"];
	if (langId == null || langId == "") langId = "cs";
	CodeDomProvider provider = GetProvider (langId);
	ICodeGenerator generator = provider.CreateGenerator();
	CodeGeneratorOptions options = new CodeGeneratorOptions();
	
	StringWriter sw = new StringWriter ();
	generator.GenerateCodeFromCompileUnit(codeUnit, sw, options);

	return Colorize (WrapText (sw.ToString (), CodeTextColumns), langId);
}

public string CurrentLanguage
{
	get {
		string langId = Request.QueryString ["lang"];
		if (langId == null || langId == "") langId = "cs";
		return langId;
	}
}

public string CurrentProxytName
{
	get {
		string lan = CurrentLanguage == "cs" ? "C#" : "Visual Basic";
		return lan + " Client Proxy";
	}
}

private CodeDomProvider GetProvider(string langId)
{
	switch (langId.ToUpper())
	{
		case "CS": return new CSharpCodeProvider();
		case "VB": return new VBCodeProvider();
		default: return null;
	}
}

//
// Document generation
//

string GenerateDocument ()
{
	StringWriter sw = new StringWriter ();
	
	if (CurrentDocType == "wsdl")
		descriptions [CurrentDocInd].Write (sw);
	else if (CurrentDocType == "schema")
		schemas [CurrentDocInd].Write (sw);
		
	return Colorize (WrapText (sw.ToString (), CodeTextColumns), "xml");
}

public string CurrentDocType
{
	get { return Request.QueryString ["doctype"] != null ? Request.QueryString ["doctype"] : "wsdl"; }
}

public int CurrentDocInd
{
	get { return Request.QueryString ["docind"] != null ? int.Parse (Request.QueryString ["docind"]) : 0; }
}

public string CurrentDocumentName
{
	get {
		if (CurrentDocType == "wsdl")
			return "WSDL document for namespace \"" + descriptions [CurrentDocInd].TargetNamespace + "\"";
		else
			return "Xml Schema for namespace \"" + schemas [CurrentDocInd].TargetNamespace + "\"";
	}
}

//
// Pages and tabs
//

bool firstTab = true;
ArrayList disabledTabs = new ArrayList ();

string CurrentTab
{
	get { return Request.QueryString["tab"] != null ? Request.QueryString["tab"] : "main" ; }
}

string CurrentPage
{
	get { return Request.QueryString["page"] != null ? Request.QueryString["page"] : "main" ; }
}

void WriteTabs ()
{
	if (CurrentOperationName != null)
	{
		WriteTab ("main","Overview");
		WriteTab ("test","Test Form");
		WriteTab ("msg","Message Layout");
	}
}

void WriteTab (string id, string label)
{
	if (!firstTab) Response.Write("&nbsp;|&nbsp;");
	firstTab = false;
	
	string cname = CurrentTab == id ? "tabLabelOn" : "tabLabelOff";
	Response.Write ("<a href='" + PageName + "?" + GetPageContext(null) + GetDataContext() + "tab=" + id + "' style='text-decoration:none'>");
	Response.Write ("<span class='" + cname + "'>" + label + "</span>");
	Response.Write ("</a>");
}

string GetTabContext (string pag, string tab)
{
	if (tab == null) tab = CurrentTab;
	if (pag == null) pag = CurrentPage;
	if (pag != CurrentPage) tab = "main";
	return "page=" + pag + "&tab=" + tab + "&"; 
}

string GetPageContext (string pag)
{
	if (pag == null) pag = CurrentPage;
	return "page=" + pag + "&"; 
}

class Tab
{
	public string Id;
	public string Label;
}

//
// Syntax coloring
//

static string keywords_cs =
	"(\\babstract\\b|\\bevent\\b|\\bnew\\b|\\bstruct\\b|\\bas\\b|\\bexplicit\\b|\\bnull\\b|\\bswitch\\b|\\bbase\\b|\\bextern\\b|" +
	"\\bobject\\b|\\bthis\\b|\\bbool\\b|\\bfalse\\b|\\boperator\\b|\\bthrow\\b|\\bbreak\\b|\\bfinally\\b|\\bout\\b|\\btrue\\b|" +
	"\\bbyte\\b|\\bfixed\\b|\\boverride\\b|\\btry\\b|\\bcase\\b|\\bfloat\\b|\\bparams\\b|\\btypeof\\b|\\bcatch\\b|\\bfor\\b|" +
	"\\bprivate\\b|\\buint\\b|\\bchar\\b|\\bforeach\\b|\\bprotected\\b|\\bulong\\b|\\bchecked\\b|\\bgoto\\b|\\bpublic\\b|" +
	"\\bunchecked\\b|\\bclass\\b|\\bif\\b|\\breadonly\\b|\\bunsafe\\b|\\bconst\\b|\\bimplicit\\b|\\bref\\b|\\bushort\\b|" +
	"\\bcontinue\\b|\\bin\\b|\\breturn\\b|\\busing\\b|\\bdecimal\\b|\\bint\\b|\\bsbyte\\b|\\bvirtual\\b|\\bdefault\\b|" +
	"\\binterface\\b|\\bsealed\\b|\\bvolatile\\b|\\bdelegate\\b|\\binternal\\b|\\bshort\\b|\\bvoid\\b|\\bdo\\b|\\bis\\b|" +
	"\\bsizeof\\b|\\bwhile\\b|\\bdouble\\b|\\block\\b|\\bstackalloc\\b|\\belse\\b|\\blong\\b|\\bstatic\\b|\\benum\\b|" +
	"\\bnamespace\\b|\\bstring\\b)";

static string keywords_vb =
	"(\\bAddHandler\\b|\\bAddressOf\\b|\\bAlias\\b|\\bAnd\\b|\\bAndAlso\\b|\\bAnsi\\b|\\bAs\\b|\\bAssembly\\b|" +
	"\\bAuto\\b|\\bBoolean\\b|\\bByRef\\b|\\bByte\\b|\\bByVal\\b|\\bCall\\b|\\bCase\\b|\\bCatch\\b|" +
	"\\bCBool\\b|\\bCByte\\b|\\bCChar\\b|\\bCDate\\b|\\bCDec\\b|\\bCDbl\\b|\\bChar\\b|\\bCInt\\b|" +
	"\\bClass\\b|\\bCLng\\b|\\bCObj\\b|\\bConst\\b|\\bCShort\\b|\\bCSng\\b|\\bCStr\\b|\\bCType\\b|" +
	"\\bDate\\b|\\bDecimal\\b|\\bDeclare\\b|\\bDefault\\b|\\bDelegate\\b|\\bDim\\b|\\bDirectCast\\b|\\bDo\\b|" +
	"\\bDouble\\b|\\bEach\\b|\\bElse\\b|\\bElseIf\\b|\\bEnd\\b|\\bEnum\\b|\\bErase\\b|\\bError\\b|" +
	"\\bEvent\\b|\\bExit\\b|\\bFalse\\b|\\bFinally\\b|\\bFor\\b|\\bFriend\\b|\\bFunction\\b|\\bGet\\b|" +
	"\\bGetType\\b|\\bGoSub\\b|\\bGoTo\\b|\\bHandles\\b|\\bIf\\b|\\bImplements\\b|\\bImports\\b|\\bIn\\b|" +
	"\\bInherits\\b|\\bInteger\\b|\\bInterface\\b|\\bIs\\b|\\bLet\\b|\\bLib\\b|\\bLike\\b|\\bLong\\b|" +
	"\\bLoop\\b|\\bMe\\b|\\bMod\\b|\\bModule\\b|\\bMustInherit\\b|\\bMustOverride\\b|\\bMyBase\\b|\\bMyClass\\b|" +
	"\\bNamespace\\b|\\bNew\\b|\\bNext\\b|\\bNot\\b|\\bNothing\\b|\\bNotInheritable\\b|\\bNotOverridable\\b|\\bObject\\b|" +
	"\\bOn\\b|\\bOption\\b|\\bOptional\\b|\\bOr\\b|\\bOrElse\\b|\\bOverloads\\b|\\bOverridable\\b|\\bOverrides\\b|" +
	"\\bParamArray\\b|\\bPreserve\\b|\\bPrivate\\b|\\bProperty\\b|\\bProtected\\b|\\bPublic\\b|\\bRaiseEvent\\b|\\bReadOnly\\b|" +
	"\\bReDim\\b|\\bREM\\b|\\bRemoveHandler\\b|\\bResume\\b|\\bReturn\\b|\\bSelect\\b|\\bSet\\b|\\bShadows\\b|" +
	"\\bShared\\b|\\bShort\\b|\\bSingle\\b|\\bStatic\\b|\\bStep\\b|\\bStop\\b|\\bString\\b|\\bStructure\\b|" +
	"\\bSub\\b|\\bSyncLock\\b|\\bThen\\b|\\bThrow\\b|\\bTo\\b|\\bTrue\\b|\\bTry\\b|\\bTypeOf\\b|" +
	"\\bUnicode\\b|\\bUntil\\b|\\bVariant\\b|\\bWhen\\b|\\bWhile\\b|\\bWith\\b|\\bWithEvents\\b|\\bWriteOnly\\b|\\bXor\\b)";

string Colorize (string text, string lang)
{
	if (lang == "xml") return ColorizeXml (text);
	else if (lang == "cs") return ColorizeCs (text);
	else if (lang == "vb") return ColorizeVb (text);
	else return text;
}

string ColorizeXml (string text)
{
	text = text.Replace (" ", "&nbsp;");
	Regex re = new Regex ("\r\n|\r|\n");
	text = re.Replace (text, "_br_");
	
	re = new Regex ("<\\s*(\\/?)\\s*([\\s\\S]*?)\\s*(\\/?)\\s*>");
	text = re.Replace (text,"{blue:&lt;$1}{maroon:$2}{blue:$3&gt;}");
	
	re = new Regex ("\\{(\\w*):([\\s\\S]*?)\\}");
	text = re.Replace (text,"<span style='color:$1'>$2</span>");

	re = new Regex ("\"(.*?)\"");
	text = re.Replace (text,"\"<span style='color:purple'>$1</span>\"");

	
	text = text.Replace ("\t", "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
	text = text.Replace ("_br_", "<br>");
	return text;
}

string ColorizeCs (string text)
{
	text = text.Replace (" ", "&nbsp;");

	text = text.Replace ("<", "&lt;");
	text = text.Replace (">", "&gt;");

	Regex re = new Regex ("\"((((?!\").)|\\\")*?)\"");
	text = re.Replace (text,"<span style='color:purple'>\"$1\"</span>");

	re = new Regex ("//(((.(?!\"</span>))|\"(((?!\").)*)\"</span>)*)(\r|\n|\r\n)");
	text = re.Replace (text,"<span style='color:green'>//$1</span><br/>");
	
	re = new Regex (keywords_cs);
	text = re.Replace (text,"<span style='color:blue'>$1</span>");
	
	text = text.Replace ("\t","&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
	text = text.Replace ("\n","<br/>");
	
	return text;
}

string ColorizeVb (string text)
{
	text = text.Replace (" ", "&nbsp;");
	
/*	Regex re = new Regex ("\"((((?!\").)|\\\")*?)\"");
	text = re.Replace (text,"<span style='color:purple'>\"$1\"</span>");

	re = new Regex ("'(((.(?!\"\\<\\/span\\>))|\"(((?!\").)*)\"\\<\\/span\\>)*)(\r|\n|\r\n)");
	text = re.Replace (text,"<span style='color:green'>//$1</span><br/>");
	
	re = new Regex (keywords_vb);
	text = re.Replace (text,"<span style='color:blue'>$1</span>");
*/	
	text = text.Replace ("\t","&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
	text = text.Replace ("\n","<br/>");
	return text;
}

//
// Helper methods and classes
//

string GetDataContext ()
{
	return "op=" + CurrentOperationName + "&bnd=" + CurrentOperationBinding + "&";
}

string GetOptionSel (string v1, string v2)
{
	string op = "<option ";
	if (v1 == v2) op += "selected ";
	return op + "value='" + v1 + "'>";
}

string WrapText (string text, int maxChars)
{
	text =  text.Replace(" />","/>");
	
	string linspace = null;
	int lincount = 0;
	int breakpos = 0;
	int linstart = 0;
	bool inquotes = false;
	char lastc = ' ';
	string sublineIndent = "";
	System.Text.StringBuilder sb = new System.Text.StringBuilder ();
	for (int n=0; n<text.Length; n++)
	{
		char c = text [n];
		
		if (c=='\r' || c=='\n' || n==text.Length-1)
		{
			sb.Append (linspace + sublineIndent + text.Substring (linstart, n-linstart+1));
			linspace = null;
			lincount = 0;
			linstart = n+1;
			breakpos = linstart;
			sublineIndent = "";
			lastc = c;
			continue;
		}
		
		if (lastc==',' || lastc=='(')
		{
			if (!inquotes) breakpos = n;
		}
		
		if (lincount > maxChars && breakpos >= linstart)
		{
			if (linspace != null)
				sb.Append (linspace + sublineIndent);
			sb.Append (text.Substring (linstart, breakpos-linstart));
			sb.Append ("\n");
			sublineIndent = "     ";
			lincount = linspace.Length + sublineIndent.Length + (n-breakpos);
			linstart = breakpos;
		}
		
		if (c==' ' || c=='\t')
		{
			if (!inquotes)
				breakpos = n;
		}
		else if (c=='"')
		{
			inquotes = !inquotes;
		}
		else 
			if (linspace == null) {
				linspace = text.Substring (linstart, n-linstart);
				linstart = n;
			}

		lincount++;
		lastc = c;
	}
	return sb.ToString ();
}

class Parameter
{
	string name;
	string type;
	string description;

	public string Name { get { return name; } set { name = value; } }
	public string Type { get { return type; } set { type = value; } }
	public string Description { get { return description; } set { description = value; } }
}

</script>

<head>
	<title><%=WebServiceName%> Web Service</title>
    <style type="text/css">
		BODY { font-family: Arial; margin-left: 20px; margin-top: 20px; font-size: x-small}
		TABLE { font-size: x-small }
		.title { color:dimgray; font-family: Arial; font-size:20pt; font-weight:900}
		.operationTitle { color:dimgray; font-family: Arial; font-size:15pt; font-weight:900}
		.method { font-size: x-small }
		.bindingLabel { font-size: x-small; font-weight:bold; color:darkgray; line-height:8pt; display:block; margin-bottom:3px }
		.label { font-size: small; font-weight:bold; color:darkgray }
		.paramTable { font-size: x-small }
		.paramTable TR { background-color: gainsboro }
		.paramFormTable { font-size: x-small; padding: 10px; background-color: gainsboro }
		.paramFormTable TR { background-color: gainsboro }
		.paramInput { border: solid 1px gray }
		.button {border: solid 1px gray }
		.smallSeparator { height:3px; overflow:hidden }
		.panel { background-color:whitesmoke; border: solid 1px silver; border-top: solid 1px silver  }
		.codePanel { background-color: white; font-size:x-small; padding:7px; border:solid 1px silver}
		.code-xml { font-size:10pt; font-family:courier }
		.code-cs { font-size:10pt; font-family:courier }
		.code-vb { font-size:10pt; font-family:courier }
		.tabLabelOn { font-weight:bold }
		.tabLabelOff {color: darkgray }
		A:link { color: black; }
		A:visited { color: black; }
		A:active { color: black; }
		A:hover { color: blue }
    </style>
	
<script>
function clearForm ()
{
	document.getElementById("testFormResult").style.display="none";
}
</script>

</head>

<body>
<div class="title" style="margin-left:20px">
<span class="label">Web Service</span><br>
<%=WebServiceName%>
</div>

<!--
	**********************************************************
	Left panel
-->

<table border="0" width="100%" cellpadding="15px" cellspacing="15px">
<tr valign="top"><td width="150px" class="panel">
<div style="width:150px"></div>
<a class="method" href='<%=PageName%>'>Overview</a><br>
<div class="smallSeparator"></div>
<a class="method" href='<%=PageName + "?" + GetPageContext("wsdl")%>'>Service Description</a>
<div class="smallSeparator"></div>
<a class="method" href='<%=PageName + "?" + GetPageContext("proxy")%>'>Client proxy</a>
<br><br>
	<asp:repeater id="BindingsRepeater" runat=server>
		<itemtemplate name="itemtemplate">
			<span class="bindingLabel"><%#FormatBindingName(DataBinder.Eval(Container.DataItem, "Name").ToString())%></span>
			<asp:repeater id="OperationsRepeater" runat=server datasource='<%# ((Binding)Container.DataItem).Operations %>'>
				<itemtemplate>
					<a class="method" href="<%#PageName%>?<%#GetTabContext("op",null)%>op=<%#GetOpName(Container.DataItem)%>&bnd=<%#DataBinder.Eval(Container.DataItem, "Binding.Name")%>"><%#GetOpName(Container.DataItem)%></a>
					<div class="smallSeparator"></div>
				</itemtemplate>
			</asp:repeater>
			<br>
		</itemtemplate>
	</asp:repeater>

</td><td class="panel">

<% if (CurrentPage == "main") {%>

<!--
	**********************************************************
	Web service overview
-->

	<p class="label">Web Service Overview</p>
	<%#WebServiceDescription%>
	
<%} if (CurrentPage == "op") {%>

<!--
	**********************************************************
	Operation description
-->

	<span class="operationTitle"><%#CurrentOperationName%></span>
	<br><br>
	<% WriteTabs (); %>
	<br><br><br>
	
	<% if (CurrentTab == "main") { %>
		<span class="label">Input Parameters</span>
		<div class="smallSeparator"></div>
		<% if (InParams.Count == 0) { %>
			No input parameters<br>
		<% } else { %>
			<table class="paramTable" cellspacing="1" cellpadding="5">
			<asp:repeater id="InputParamsRepeater" runat=server>
				<itemtemplate>
					<tr>
					<td width="150"><%#DataBinder.Eval(Container.DataItem, "Name")%></td>
					<td width="150"><%#DataBinder.Eval(Container.DataItem, "Type")%></td>
					</tr>
				</itemtemplate>
			</asp:repeater>
			</table>
		<% } %>
		<br>
		
		<% if (OutParams.Count > 0) { %>
		<span class="label">Output Parameters</span>
			<div class="smallSeparator"></div>
			<table class="paramTable" cellspacing="1" cellpadding="5">
			<asp:repeater id="OutputParamsRepeater" runat=server>
				<itemtemplate>
					<tr>
					<td width="150"><%#DataBinder.Eval(Container.DataItem, "Name")%></td>
					<td width="150"><%#DataBinder.Eval(Container.DataItem, "Type")%></td>
					</tr>
				</itemtemplate>
			</asp:repeater>
			</table>
		<br>
		<% } %>
		
		<span class="label">Remarks</span>
		<div class="smallSeparator"></div>
		<%#OperationDocumentation%>
		<br><br>
		<span class="label">Technical information</span>
		<div class="smallSeparator"></div>
		Format: <%#CurrentOperationFormat%>
		<br>Supported protocols: <%#CurrentOperationProtocols%>
	<% } %>
	
	<% if (CurrentTab == "test") { 
		if (CurrentOperationSupportsTest) {%>
			Enter values for the parameters and click the 'Invoke' button to test this method:<br><br>
			<form action="<%#PageName%>" method="GET">
			<input type="hidden" name="page" value="<%#CurrentPage%>">
			<input type="hidden" name="tab" value="<%#CurrentTab%>">
			<input type="hidden" name="op" value="<%#CurrentOperationName%>">
			<input type="hidden" name="bnd" value="<%#CurrentOperationBinding%>">
			<input type="hidden" name="ext" value="testform">
			<table class="paramFormTable" cellspacing="0" cellpadding="3">
			<asp:repeater id="InputFormParamsRepeater" runat=server>
				<itemtemplate>
					<tr>
					<td><%#DataBinder.Eval(Container.DataItem, "Name")%>:&nbsp;</td>
					<td width="150"><input class="paramInput" type="text" size="20" name="<%#DataBinder.Eval(Container.DataItem, "Name")%>"></td>
					</tr>
				</itemtemplate>
			</asp:repeater>
			<tr><td></td><td><input class="button" type="submit" value="Invoke">&nbsp;<input class="button" type="button" onclick="clearForm()" value="Clear"></td></tr>
			</table>
			</form>
			<div id="testFormResult" style="display:<%# (HasFormResult?"block":"none") %>">
			The web service returned the following result:<br/><br/>
			<div class="codePanel"><%#GetTestResult()%></div>
			</div>
		<% } else {%>
		The test form is not available for this operation because it has parameters with a complex structure.
		<% } %>
	<% } %>
	<% if (CurrentTab == "msg") { %>
		TODO
	<% } %>
<%}%>

<% if (CurrentPage == "proxy") {%>
<!--
	**********************************************************
	Client Proxy
-->
	<form action="<%#PageName%>" name="langForm" method="GET">
		Select the language for which you want to generate a proxy 
		<input type="hidden" name="page" value="<%#CurrentPage%>">&nbsp;
		<SELECT name="lang" onchange="langForm.submit()">
			<%#GetOptionSel("cs",CurrentLanguage)%>C#</option>
			<%#GetOptionSel("vb",CurrentLanguage)%>Visual Basic</option>
		</SELECT>
		&nbsp;&nbsp;
	</form>
	<br>
	<span class="label"><%#CurrentProxytName%></span>&nbsp;&nbsp;&nbsp;
	<a href="<%#PageName + "?code=" + CurrentLanguage%>">Download</a>
	<br><br>
	<div class="codePanel">
	<div class="code-<%#CurrentLanguage%>"><%#GetProxyCode ()%></div>
	</div>
<%}%>

<% if (CurrentPage == "wsdl") {%>
<!--
	**********************************************************
	Service description
-->
	<% if (descriptions.Count > 1 || schemas.Count > 1) {%>
	The description of this web service is composed by several documents. Click on the document you want to see:
	
	<ul>
	<% 
		for (int n=0; n<descriptions.Count; n++)
			Response.Write ("<li><a href='" + PageName + "?" + GetPageContext(null) + "doctype=wsdl&docind=" + n + "'>WSDL document " + descriptions[n].TargetNamespace + "</a></li>");
		for (int n=0; n<schemas.Count; n++)
			Response.Write ("<li><a href='" + PageName + "?" + GetPageContext(null) + "doctype=schema&docind=" + n + "'>Xml Schema " + schemas[n].TargetNamespace + "</a></li>");
	%>
	</ul>
	
	<%} else {%>
	<%}%>
	<br>
	<span class="label"><%#CurrentDocumentName%></span>&nbsp;&nbsp;&nbsp;
	<a href="<%=PageName + "?" + CurrentDocType + "=" + CurrentDocInd %>">Download</a>
	<br><br>
	<div class="codePanel">
	<div class="code-xml"><%#GenerateDocument ()%></div>
	</div>

<%}%>

<br><br><br><br><br><br><br><br><br><br><br><br><br><br><br>
</td>
<td withd="20px"></td>
</tr>

</table>
</body>
</html>
