<%@ Import Namespace="System.Collections" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Xml.Serialization" %>
<%@ Import Namespace="System.Xml" %>
<%@ Import Namespace="System.Xml.Schema" %>
<%@ Import Namespace="System.Web.Services.Description" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="System.Resources" %>
<%@ Import Namespace="System.Diagnostics" %>
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

string CurrentOperationName;
string CurrentOperationBinding;
string OperationDocumentation;
ArrayList InParams;
ArrayList OutParams;

void Page_Load(object sender, EventArgs e)
{
	descriptions = (ServiceDescriptionCollection) Context.Items["wsdls"];
	schemas = (XmlSchemas) Context.Items["schemas"];

	ServiceDescription desc = descriptions [0];
	Service service = desc.Services[0];
	WebServiceName = service.Name;
	DefaultBinding = desc.Bindings[0].Name;
	WebServiceDescription = service.Documentation;
	
	CurrentOperationName = Request.QueryString["op"];
	CurrentOperationBinding = Request.QueryString["bnd"];
	if (CurrentOperationName != null) BuildOperationInfo ();

	PageName = HttpUtility.UrlEncode (Path.GetFileName(Request.Path), Encoding.UTF8);

	ArrayList list = new ArrayList ();
	foreach (ServiceDescription sd in descriptions)
		list.AddRange (sd.Bindings);

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

	InputParamsRepeater.DataSource = InParams;
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

XmlSchemaElement GetRefElement (XmlSchemaElement elem)
{
	if (!elem.RefName.IsEmpty)
		return (XmlSchemaElement) schemas.Find (elem.RefName, typeof(XmlSchemaElement));
	else
		return elem;
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
	foreach (Operation oper in portType.Operations)
		if (oper.Name == name) return oper;
	return null;
}

string FormatBindingName (string name)
{
	if (name == DefaultBinding) return "Methods";
	else return "Methods for binding<br>" + name;
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
    <style type="text/css">
		BODY { font-family: Arial; margin-left: 20px; margin-top: 20px; font-size: x-small}
		TABLE { font-size: x-small }
		.title { color:dimgray; font-family: Arial; font-size:20pt; font-weight:900}
		.operationTitle { color:dimgray; font-family: Arial; font-size:15pt; font-weight:900}
		.method { font-size: x-small }
		.bindingLabel { font-size: x-small; font-weight:bold; color:darkgray; line-height:8pt; display:block; margin-bottom:3px }
		.label { font-size: x-small; font-weight:bold; color:darkgray }
		.paramTable { font-size: x-small }
		.paramTable TR { background-color: gainsboro }
		.smallSeparator { height:3px; overflow:hidden }
		.panel { background-color:whitesmoke; border: solid 1px silver; border-top: solid 1px silver  }
		A:link { color: black; }
		A:visited { color: black; }
		A:active { color: black; }
		A:hover { color: blue }
    </style>
</head>

<body>
<div class="title" style="margin-left:20px">
<span class="label">Web Service</span><br>
<%=WebServiceName%>
</div>
<table border="0" width="100%" cellpadding="15px" cellspacing="15px">
<tr valign="top"><td width="150px" class="panel">
<a class="method" href='<%=PageName%>'>Overview</a><br>
<div class="smallSeparator"></div>
<a class="method" href='<%=PageName + "?wsdl"%>'>Service Description</a>
<div class="smallSeparator"></div>
<a class="method" href='<%=PageName + "?code=cs"%>'>C# proxy</a>
<br><br>
	<asp:repeater id="BindingsRepeater" runat=server>
		<itemtemplate name="itemtemplate">
			<span class="bindingLabel"><%#FormatBindingName(DataBinder.Eval(Container.DataItem, "Name").ToString())%></span>
			<asp:repeater id="OperationsRepeater" runat=server datasource='<%# ((Binding)Container.DataItem).Operations %>'>
				<itemtemplate>
					<a class="method" href="<%#PageName%>?op=<%#DataBinder.Eval(Container.DataItem, "Name")%>&bnd=<%#DataBinder.Eval(Container.DataItem, "Binding.Name")%>"><%#DataBinder.Eval(Container.DataItem, "Name")%></a>
					<div class="smallSeparator"></div>
				</itemtemplate>
			</asp:repeater>
			<br>
		</itemtemplate>
	</asp:repeater>

</td><td class="panel">

<% if (CurrentOperationName == null) {%>
	<p class="label">Web Service Overview</p>
	<%#WebServiceDescription%>
<%} else {%>
	<span class="operationTitle"><%#CurrentOperationName%></span>
	<br><br>
	<span class="label">Input Parameters</span>
	<% if (InParams.Count == 0) { %>
		<br>No input parameters<br>
	<% } else { %>
		<table class="paramTable" cellspacing="1" cellpadding="5">
		<asp:repeater id="InputParamsRepeater" runat=server>
			<itemtemplate>
				<tr>
				<td width="150"><%#DataBinder.Eval(Container.DataItem, "Name")%></td>
				<td width="150"><%#DataBinder.Eval(Container.DataItem, "Type")%></td>
<!--				<td><%#DataBinder.Eval(Container.DataItem, "Description")%></td>-->
				</tr>
			</itemtemplate>
		</asp:repeater>
		</table>
	<% } %>
	<br>
	
	<% if (OutParams.Count > 0) { %>
	<span class="label">Output Parameters</span>
		<table class="paramTable" cellspacing="1" cellpadding="5">
		<asp:repeater id="OutputParamsRepeater" runat=server>
			<itemtemplate>
				<tr>
				<td width="150"><%#DataBinder.Eval(Container.DataItem, "Name")%></td>
				<td width="150"><%#DataBinder.Eval(Container.DataItem, "Type")%></td>
<!--				<td><%#DataBinder.Eval(Container.DataItem, "Description")%></td> -->
				</tr>
			</itemtemplate>
		</asp:repeater>
		</table>
	<br>
	<% } %>
	<span class="label">Remarks</span>
	<br><%#OperationDocumentation%>
<%}%>
<br><br><br><br><br><br><br><br><br><br><br><br><br><br><br>
</td>
<td withd="20px"></td>
</tr>

</table>
</body>
</html>
