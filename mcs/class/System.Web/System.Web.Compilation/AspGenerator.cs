//
// System.Web.Compilation.AspGenerator
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.Util;

namespace System.Web.Compilation
{

class ControlStack
{
	private Stack controls;
	private ControlStackData top;
	private bool space_between_tags;
	private bool sbt_valid;

	class ControlStackData 
	{
		public Type controlType;
		public string controlID;
		public string tagID;
		public ChildrenKind childKind;
		public string defaultPropertyName;
		public int childrenNumber;
		public Type container;
		public StringBuilder dataBindFunction;
		public StringBuilder codeRenderFunction;
		public bool useCodeRender;
		public int codeRenderIndex;

		public ControlStackData (Type controlType,
					 string controlID,
					 string tagID,
					 ChildrenKind childKind,
					 string defaultPropertyName,
					 Type container)
		{
			this.controlType = controlType;
			this.controlID = controlID;
			this.tagID = tagID;
			this.childKind = childKind;
			this.defaultPropertyName = defaultPropertyName;
			this.container = container;
			childrenNumber = 0;
		}

		public override string ToString ()
		{
			return controlType + " " + controlID + " " + tagID + " " + childKind + " " + childrenNumber;
		}
	}
	
	public ControlStack ()
	{
		controls = new Stack ();
	}

	private Type GetContainerType (Type type)
	{
		if (type != typeof (System.Web.UI.Control) &&
		    !type.IsSubclassOf (typeof (System.Web.UI.Control)))
			return null;
		
		Type container_type;
		if (type == typeof (System.Web.UI.WebControls.DataList))
			container_type = typeof (System.Web.UI.WebControls.DataListItem);
		else if (type == typeof (System.Web.UI.WebControls.DataGrid))
			container_type = typeof (System.Web.UI.WebControls.DataGridItem);
		else if (type == typeof (System.Web.UI.WebControls.Repeater))
			container_type = typeof (System.Web.UI.WebControls.RepeaterItem);
		else if (type == typeof (ListControl) || type.IsSubclassOf (typeof (ListControl)))
			container_type = type;
		else
			container_type = Container;

		return container_type;
	}

	public void Push (object o)
	{
		if (!(o is ControlStackData))
			return;

		controls.Push (o);
		top = (ControlStackData) o;
		sbt_valid = false;
	}
	
	public void Push (Type controlType,
			  string controlID,
			  string tagID,
			  ChildrenKind childKind,
			  string defaultPropertyName)
	{
		Type container_type = null;
		if (controlType != null){
			AddChild ();
			container_type = GetContainerType (controlType);
			if (container_type == null)
				container_type = this.Container;
		}

		top = new ControlStackData (controlType,
					    controlID,
					    tagID,
					    childKind,
					    defaultPropertyName,
					    container_type);
		sbt_valid = false;
		controls.Push (top);
	}

	public object Pop ()
	{
		object item = controls.Pop ();
		if (controls.Count != 0)
			top = (ControlStackData) controls.Peek ();
		sbt_valid = false;
		return item;
	}

	public Type PeekType ()
	{
		return top.controlType;
	}

	public string PeekControlID ()
	{
		return top.controlID;
	}

	public string PeekTagID ()
	{
		return top.tagID;
	}

	public ChildrenKind PeekChildKind ()
	{
		return top.childKind;
	}

	public string PeekDefaultPropertyName ()
	{
		return top.defaultPropertyName;
	}

	public void AddChild ()
	{
		if (top != null)
			top.childrenNumber++;
	}

	public bool HasDataBindFunction ()
	{
		if (top.dataBindFunction == null || top.dataBindFunction.Length == 0)
			return false;
		return true;
	}
	
	public bool UseCodeRender
	{
		get {
			if (top.codeRenderFunction == null || top.codeRenderFunction.Length == 0)
				return false;
			return top.useCodeRender;
		}

		set { top.useCodeRender= value; }
	}
	
	public bool SpaceBetweenTags
	{
		get {
			if (!sbt_valid){
				sbt_valid = true;
				Type type = top.controlType;
				if (type.Namespace == "System.Web.UI.WebControls")
					space_between_tags = true;
				else if (type.IsSubclassOf (typeof (System.Web.UI.WebControls.WebControl)))
					space_between_tags = true;
				else if (type == typeof (System.Web.UI.HtmlControls.HtmlSelect))
					space_between_tags = true;
				else if (type == typeof (System.Web.UI.HtmlControls.HtmlTable))
					space_between_tags = true;
				else if (type == typeof (System.Web.UI.HtmlControls.HtmlTableRow))
					space_between_tags = true;
				else if (type == typeof (System.Web.UI.HtmlControls.HtmlTableCell))
					space_between_tags = true;
				else
					space_between_tags = false;
			}
			return space_between_tags;
		}
	}
	
	public Type Container {
		get {
			if (top == null)
				return null;
				
			return top.container;
		}
	}
	
	public StringBuilder DataBindFunction
	{
		get {
			if (top.dataBindFunction == null)
				top.dataBindFunction = new StringBuilder ();
			return top.dataBindFunction;
		}
	}

	public int CodeRenderIndex {
		get {
			return top.codeRenderIndex++;
		}
	}
	
	public StringBuilder CodeRenderFunction
	{
		get {
			if (top.codeRenderFunction == null)
				top.codeRenderFunction = new StringBuilder ();
			return top.codeRenderFunction;
		}
	}

	public int ChildIndex
	{
		get { return top.childrenNumber - 1; }
	}
	
	public int Count
	{
		get { return controls.Count; }
	}

	public override string ToString ()
	{
		return top.ToString () + " " + top.useCodeRender;
	}
		
}

class ArrayListWrapper
{
	private ArrayList list;
	private int index;

	public ArrayListWrapper (ArrayList list)
	{
		this.list = list;
		index = -1;
	}

	private void CheckIndex ()
	{
		if (index == -1 || index == list.Count)
			throw new InvalidOperationException ();
	}
			
	public object Current
	{
		get {
			CheckIndex ();
			return list [index];
		}

		set {
			CheckIndex ();
			list [index] = value;
		}
	}

	public bool MoveNext ()
	{
		if (index < list.Count)
			index++;

		return index < list.Count;
	}
}

class AspGenerator
{
	private object [] parts;
	private ArrayListWrapper elements;
	private StringBuilder prolog;
	private StringBuilder declarations;
	private StringBuilder script;
	private StringBuilder constructor;
	private StringBuilder init_funcs;
	private StringBuilder epilog;
	private StringBuilder current_function;
	private Stack functions;
	private ControlStack controls;
	private bool parse_ok;
	private bool has_form_tag;
	private AspComponentFoundry aspFoundry;

	private string classDecl;
	private string className;
	private string interfaces;
	private string basetype;
	private string parent;
	private Type parentType;
	private string fullPath;

	Hashtable options;
	string privateBinPath;
	string main_directive;
	static string app_file_wrong = "The content in the application file is not valid.";

	bool isPage;
	bool isUserControl;
	bool isApplication;

	HttpContext context;

	SessionState sessionState = SessionState.Enabled;

	static Type styleType = typeof (System.Web.UI.WebControls.Style);
	static Type fontinfoType = typeof (System.Web.UI.WebControls.FontInfo);

	enum UserControlResult
	{
		OK = 0,
		FileNotFound = 1,
		CompilationFailed = 2
	}

	enum SessionState
	{
		Enabled,
		ReadOnly,
		Disabled
	}
	
	public AspGenerator (string pathToFile, ArrayList elements)
	{
		if (elements == null)
			throw new ArgumentNullException ();

		this.elements = new ArrayListWrapper (elements);
		string filename = Path.GetFileName (pathToFile);
		this.className = filename.Replace ('.', '_'); // Overridden by @ Page classname
		this.className = className.Replace ('-', '_'); 
		this.className = className.Replace (' ', '_');
		Options ["ClassName"] = this.className;
		this.fullPath = Path.GetFullPath (pathToFile);

		this.has_form_tag = false;
		AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
		privateBinPath = setup.PrivateBinPath;
		// This is a hack until we can run stuff in different domains
		if (privateBinPath == null || privateBinPath.Length == 0)
			privateBinPath = "bin";
			
		if (!Path.IsPathRooted (privateBinPath))
			privateBinPath = Path.Combine (setup.ApplicationBase, privateBinPath);
		
		Init ();
	}

	public string BaseType {
		get { return basetype; }

		set {
			if (parent == null)
				parent = value;

			basetype = value;
			isUserControl = (basetype == "System.Web.UI.UserControl");
			isPage = (basetype == "System.Web.UI.Page");
			isApplication = (basetype == "System.Web.HttpApplication");
		}
	}

	public bool IsUserControl {
		get { return isUserControl; }
	}
	
	public bool IsPage {
		get { return isPage; }
	}
	
	public bool IsApplication {
		get { return isApplication; }
	}

	public string Interfaces {
		get { return interfaces; }
	}

	public Hashtable Options {
		get {
			if (options == null)
				options = new Hashtable ();

			return options;
		}
	}
	
	internal HttpContext Context {
		get { return context; }
		set { context = value; }
	}
	
	bool AddUsing (string nspace)
	{
		string _using = "using " + nspace + ";";
		if (prolog.ToString ().IndexOf (_using) == -1) {
			prolog.AppendFormat ("\t{0}\n", _using);
			return true;
		}

		return false;
	}

	void AddInterface (Type type)
	{
		AddInterface (type.ToString ());
	}
	
	public void AddInterface (string iface)
	{
		if (interfaces == null) {
			interfaces = ", " + iface;
		} else {
			string s = ", " + iface;
			if (interfaces.IndexOf (s) == -1)
				interfaces += s;
		}
	}

	private AspComponentFoundry Foundry
	{
		get {
			if (aspFoundry == null)
				aspFoundry = new AspComponentFoundry ();

			return aspFoundry;
		}
	}

	private void Init ()
	{
		controls = new ControlStack ();
		controls.Push (typeof (System.Web.UI.Control), "Root", null, ChildrenKind.CONTROLS, null);
		prolog = new StringBuilder ();
		declarations = new StringBuilder ();
		script = new StringBuilder ();
		constructor = new StringBuilder ();
		init_funcs = new StringBuilder ();
		epilog = new StringBuilder ();

		current_function = new StringBuilder ();
		functions = new Stack ();
		functions.Push (current_function);

		parts = new Object [6];
		parts [0] = prolog;
		parts [1] = declarations;
		parts [2] = script;
		parts [3] = constructor;
		parts [4] = init_funcs;
		parts [5] = epilog;

		prolog.Append ("namespace ASP {\n" +
			      "\tusing System;\n" + 
			      "\tusing System.Collections;\n" + 
			      "\tusing System.Collections.Specialized;\n" + 
			      "\tusing System.Configuration;\n" + 
			      "\tusing System.IO;\n" + 
			      "\tusing System.Text;\n" + 
			      "\tusing System.Text.RegularExpressions;\n" + 
			      "\tusing System.Web;\n" + 
			      "\tusing System.Web.Caching;\n" + 
			      "\tusing System.Web.Security;\n" + 
			      "\tusing System.Web.SessionState;\n" + 
			      "\tusing System.Web.UI;\n" + 
			      "\tusing System.Web.UI.WebControls;\n" + 
			      "\tusing System.Web.UI.HtmlControls;\n");

		declarations.Append ("\t\tprivate static int __autoHandlers;\n");

		current_function.Append ("\t\tprivate void __BuildControlTree (System.Web.UI.Control __ctrl)\n\t\t{\n");
		if (!IsUserControl)
			current_function.Append ("\t\t\tSystem.Web.UI.IParserAccessor __parser = " + 
						 "(System.Web.UI.IParserAccessor) __ctrl;\n\n");
		else
			controls.UseCodeRender = true;
	}

	public StringReader GetCode ()
	{
		if (!parse_ok)
			throw new ApplicationException ("You gotta call ProcessElements () first!");

		StringBuilder code = new StringBuilder ();
		for (int i = 0; i < parts.Length; i++)
			code.Append ((StringBuilder) parts [i]);

		return new StringReader (code.ToString ());
	}

	public void Print ()
	{
		if (!parse_ok){
			Console.WriteLine ("//Warning!!!: Elements not correctly parsed.");
		}

		Console.Write (GetCode ().ReadToEnd ());
	}

	// Regex.Escape () make some illegal escape sequences for a C# source.
	private string Escape (string input)
	{
		if (input == null)
			return String.Empty;

		string output = input.Replace ("\\", "\\\\");
		output = output.Replace ("\"", "\\\"");
		output = output.Replace ("\t", "\\t");
		output = output.Replace ("\r", "\\r");
		output = output.Replace ("\n", "\\n");
		output = output.Replace ("\n", "\\n");
		return output;
	}
	
	bool AddProtectedField (Type type, string fieldName)
	{
		if (parentType == null) {
			declarations.AppendFormat ("\t\tprotected {0} {1};\n", type.ToString (), fieldName);
			return true;
		}

		FieldInfo field = parentType.GetField (fieldName, BindingFlags.Public |
								  BindingFlags.NonPublic |
								  BindingFlags.Instance |
								  BindingFlags.Static);

		if (field == null || (!field.IsPublic && !field.IsFamily)) {
			declarations.AppendFormat ("\t\tprotected {0} {1};\n", type.ToString (), fieldName);
			return true;
		}

		if (!field.FieldType.IsAssignableFrom (type)) {
			string message = String.Format ("The base class includes the field '{0}', but its " +
							"type '{1}' is not compatible with {2}",
							fieldName, field.FieldType, type);

			throw new ApplicationException (message);
		}

		return false;
	}
	
	private Type LoadParentType (string typeName)
	{
		// First try loaded assemblies, then try assemblies in Bin directory.
		// By now i do this 'by hand' but may be this is a runtime/gac task.
		Type type = Type.GetType (typeName);
		if (type != null)
			return type;

		string [] binDlls = Directory.GetFiles (privateBinPath, "*.dll");
		Assembly assembly;
		foreach (string dll in binDlls) {
			string dllPath = Path.Combine (privateBinPath, dll);
			assembly = null;
			try {
				assembly = Assembly.LoadFrom (dllPath);
				type = assembly.GetType (typeName);
			} catch (Exception e) {
				if (assembly != null) {
					Console.WriteLine ("ASP.NET Warning: assembly {0} loaded", dllPath);
					Console.WriteLine ("ASP.NET Warning: but type {0} not found", typeName);
				} else {
					Console.WriteLine ("ASP.NET Warning: unable to load type {0} from {1}",
							   typeName, dllPath);
				}
				Console.WriteLine ("ASP.NET Warning: error was: {0}", e.Message);
			}

			if (type != null)
				return type;
		}

		return null;
	}

	private void PageDirective (TagAttributes att)
	{
		if (att ["ClassName"] != null){
			this.className = (string) att ["ClassName"];
			Options ["ClassName"] = className;
		}

		if (att ["EnableSessionState"] != null){
			if (!IsPage)
				throw new ApplicationException ("EnableSessionState not allowed here.");
			
			string est = (string) att ["EnableSessionState"];
			if (0 == String.Compare (est, "false", true))
				sessionState = SessionState.Disabled;
			else if (0 == String.Compare (est, "true", true))
				sessionState = SessionState.Enabled;
			else if (0 == String.Compare (est, "readonly", true))
				sessionState = SessionState.ReadOnly;
			else
				throw new ApplicationException ("EnableSessionState in Page directive not set to " +
								"a correct value: " + est);
		}

		if (att ["Inherits"] != null) {
			parent = (string) att ["Inherits"];
			parentType = LoadParentType (parent);
			if (parentType == null)
				throw new ApplicationException ("The class " + parent + " cannot be found.");
		}

		if (att ["CompilerOptions"] != null)
			Options ["CompilerOptions"] = (string) att ["CompilerOptions"];

		if (att ["AutoEventWireup"] != null) {
			if (options ["AutoEventWireup"] != null)
				throw new ApplicationException ("Already have an AutoEventWireup attribute");
			
			bool autoevent = true;
			string v = att ["AutoEventWireup"] as string;
			try {
				autoevent = Convert.ToBoolean (v);
			} catch (Exception) {
				throw new ApplicationException ("'" + v + "' is not a valid value for AutoEventWireup");
			}
			options ["AutoEventWireup"] = autoevent;
		}

		//FIXME: add support for more attributes.
	}

	void AddReference (string dll)
	{
		string references = Options ["References"] as string;
		if (references == null)
			references = dll;
		else
			references = references + " " + dll;

		Options ["References"] = references;
	}

	private void RegisterDirective (TagAttributes att)
	{
		string tag_prefix = (string) (att ["tagprefix"] == null ?  "" : att ["tagprefix"]);
		string name_space = (string) (att ["namespace"] == null ?  "" : att ["namespace"]);
		string assembly_name = (string) (att ["assembly"] == null ?  "" : att ["assembly"]);
		string tag_name =  (string) (att ["tagname"] == null ?  "" : att ["tagname"]);
		string src = (string) (att ["src"] == null ?  "" : att ["src"]);

		if (tag_prefix != "" && name_space != "" && assembly_name != ""){
			if (tag_name != "" || src != "")
				throw new ApplicationException ("Invalid attributes for @ Register: " +
								att.ToString ());

			AddUsing (name_space);
			string dll = privateBinPath + Path.DirectorySeparatorChar + assembly_name + ".dll";
			// Hack: it should use assembly.load semantics...
			// may be when we don't run mcs as a external program...
			if (!File.Exists (dll))
				dll = assembly_name;

			Foundry.RegisterFoundry (tag_prefix, dll, name_space);
			AddReference (dll);
			return;
		}

		if (tag_prefix != "" && tag_name != "" && src != ""){
			if (name_space != "" && assembly_name != "")
				throw new ApplicationException ("Invalid attributes for @ Register: " +
								att.ToString ());
			
			if (!src.EndsWith (".ascx"))
				throw new ApplicationException ("Source file extension for controls " + 
								"must be .ascx");

			UserControlData data = GenerateUserControl (src, Context);
			switch (data.result) {
			case UserControlResult.OK:
				AddUsing ("ASP");
				string dll = "output" + Path.DirectorySeparatorChar + data.assemblyName + ".dll";
				Foundry.RegisterFoundry (tag_prefix, tag_name, data.assemblyName, "ASP", data.className);
				AddReference (data.assemblyName);
				break;
			case UserControlResult.FileNotFound:
				throw new ApplicationException ("File '" + src + "' not found.");
			case UserControlResult.CompilationFailed:
				//TODO: should say where the generated .cs file is for the server to
				//show the source and the compiler error
				throw new NotImplementedException ();
			}
			return;
		}

		throw new ApplicationException ("Invalid combination of attributes in " +
						"@ Register: " + att.ToString ());
	}

	private void ProcessDirective ()
	{
		Directive directive = (Directive) elements.Current;
		TagAttributes att = directive.Attributes;
		if (att == null)
			return;

		string value;
		string id = directive.TagID.ToUpper ();
		switch (id){
		case "APPLICATION":
			if (main_directive != null)
				throw new ApplicationException (id + " not allowed after " + main_directive);

			if (!IsApplication)
				throw new ApplicationException ("@Application not allowed.");

			string inherits = att ["inherits"] as string;
			if (inherits != null)
				Options ["Inherits"] = inherits;

			main_directive = directive.TagID;
			break;
		case "PAGE":
		case "CONTROL":
			if (main_directive != null)
				throw new ApplicationException (id + " not allowed after " + main_directive);

			if (IsUserControl && id != "CONTROL")
				throw new ApplicationException ("@Page not allowed for user controls.");
			else if (IsPage && id != "PAGE")
				throw new ApplicationException ("@Control not allowed here. This is a page!");

			PageDirective (att);
			main_directive = directive.TagID;
			break;
		case "IMPORT":
			value = att ["namespace"] as string;
			if (value == null || att.Count > 1)
				throw new ApplicationException ("Wrong syntax in Import directive.");

			string _using = "using " + value + ";";
			if (AddUsing (value) == true) {
				string imports = Options ["Import"] as string;
				if (imports == null) {
					imports = value;
				} else {
					imports += "," + value;
				}

				Options ["Import"] = imports;
			}
			break;
		case "IMPLEMENTS":
			if (IsApplication)
				throw new ApplicationException ("@ Implements not allowed in an application file.");

			string iface = (string) att ["interface"];
			AddInterface (iface);
			break;
		case "REGISTER":
			if (IsApplication)
				throw new ApplicationException ("@ Register not allowed in an application file.");

			RegisterDirective (att);
			break;
		case "ASSEMBLY":
			if (att.Count > 1)
				throw new ApplicationException ("Wrong syntax in Assembly directive.");

			string name = att ["name"] as string;
			string src = att ["src"] as string;

			if (name == null && src == null)
				throw new ApplicationException ("Wrong syntax in Assembly directive.");

			if (IsApplication && src != null)
				throw new ApplicationException ("'name' attribute expected.");

			value = (name == null) ? src : name;
			string assemblies = Options ["Assembly"] as string;
			if (assemblies == null) {
				assemblies = value;
			} else {
				assemblies += "," + value;
			}

			Options ["Assembly"] = assemblies;
			break;
		}
	}

	private void ProcessPlainText ()
	{
		PlainText asis = (PlainText) elements.Current;
		string trimmed = asis.Text.Trim ();
		if (trimmed == String.Empty && controls.SpaceBetweenTags == true)
			return;

		if (IsApplication) {
			if (trimmed != String.Empty)
				throw new ApplicationException (app_file_wrong);
			return;
		}

		if (trimmed != String.Empty && controls.PeekChildKind () != ChildrenKind.CONTROLS){
			string tag_id = controls.PeekTagID ();
			throw new ApplicationException ("Literal content not allowed for " + tag_id);
		}
		
		string escaped_text = Escape (asis.Text);
		current_function.AppendFormat ("\t\t\t__parser.AddParsedSubObject (" + 
					       "new System.Web.UI.LiteralControl (\"{0}\"));\n",
					       escaped_text);
		StringBuilder codeRenderFunction = controls.CodeRenderFunction;
		codeRenderFunction.AppendFormat ("\t\t\t__output.Write (\"{0}\");\n", escaped_text);
	}

	private string EnumValueNameToString (Type enum_type, string value_name)
	{
		if (value_name.EndsWith ("*"))
			throw new ApplicationException ("Invalid property value: '" + value_name + 
							". It must be a valid " + enum_type.ToString () + " value.");

		MemberInfo [] nested_types = enum_type.FindMembers (MemberTypes.Field, 
								    BindingFlags.Public | BindingFlags.Static,
								    Type.FilterNameIgnoreCase,
								    value_name);

		if (nested_types.Length == 0)
			throw new ApplicationException ("Value " + value_name + " not found in enumeration " +
							enum_type.ToString ());
		if (nested_types.Length > 1)
			throw new ApplicationException ("Value " + value_name + " found " + 
							nested_types.Length + " in enumeration " +
							enum_type.ToString ());

		return enum_type.ToString () + "." + nested_types [0].Name;
	}
	
	private void NewControlFunction (string tag_id,
					 string control_id,
					 Type control_type,
					 ChildrenKind children_kind,
					 string defaultPropertyName)
	{
		ChildrenKind prev_children_kind = controls.PeekChildKind ();
		if (prev_children_kind == ChildrenKind.NONE || 
		    prev_children_kind == ChildrenKind.PROPERTIES){
			string prev_tag_id = controls.PeekTagID ();
			throw new ApplicationException ("Child controls not allowed for " + prev_tag_id);
		}

		if (prev_children_kind == ChildrenKind.DBCOLUMNS &&
		    control_type != typeof (System.Web.UI.WebControls.DataGridColumn) &&
		    !control_type.IsSubclassOf (typeof (System.Web.UI.WebControls.DataGridColumn)))
			throw new ApplicationException ("Inside " + controls.PeekTagID () + " only " + 
							"System.Web.UI.WebControls.DataGridColum " + 
							"objects are allowed");
		else if (prev_children_kind == ChildrenKind.LISTITEM &&
			 control_type != typeof (System.Web.UI.WebControls.ListItem))
			throw new ApplicationException ("Inside " + controls.PeekTagID () + " only " + 
							"System.Web.UI.WebControls.ListItem " + 
							"objects are allowed");
		else if (prev_children_kind == ChildrenKind.HTMLROW &&
			 control_type != typeof (System.Web.UI.HtmlControls.HtmlTableRow))
			throw new ApplicationException ("Inside " + controls.PeekTagID () + " only " + 
							"System.Web.UI.HtmlControls.HtmlTableRow " + 
							"objects are allowed");
		else if (prev_children_kind == ChildrenKind.HTMLCELL &&
			 control_type != typeof (System.Web.UI.HtmlControls.HtmlTableCell))
			throw new ApplicationException ("Inside " + controls.PeekTagID () + " only " + 
							"System.Web.UI.HtmlControls.HtmlTableCell " + 
							"objects are allowed");
	
					
		StringBuilder func_code = new StringBuilder ();
		current_function = func_code;
		if (0 == String.Compare (tag_id, "form", true)){
			if (has_form_tag)
				throw new ApplicationException ("Only one form server tag allowed.");
			has_form_tag = true;
		}

		controls.Push (control_type, control_id, tag_id, children_kind, defaultPropertyName);
		bool is_generic = control_type ==  typeof (System.Web.UI.HtmlControls.HtmlGenericControl);
		functions.Push (current_function);
		if (control_type != typeof (System.Web.UI.WebControls.ListItem))
			current_function.AppendFormat ("\t\tprivate System.Web.UI.Control __BuildControl_" +
							"{0} ()\n\t\t{{\n\t\t\t{1} __ctrl;\n\n\t\t\t__ctrl" +
							" = new {1} ({2});\n\t\t\tthis.{0} = __ctrl;\n",
							control_id, control_type,
							(is_generic? "\"" + tag_id + "\"" : ""));
		else
			current_function.AppendFormat ("\t\tprivate void __BuildControl_{0} ()\n\t\t{{" +
							"\n\t\t\t{1} __ctrl;\n\t\t\t__ctrl = new {1} ();" +
							"\n\t\t\tthis.{0} = __ctrl;\n",
							control_id, control_type);

		if (children_kind == ChildrenKind.CONTROLS || children_kind == ChildrenKind.OPTION)
			current_function.Append ("\t\t\tSystem.Web.UI.IParserAccessor __parser = " + 
						 "(System.Web.UI.IParserAccessor) __ctrl;\n");
	}
	
	private void DataBoundProperty (Type target, string varName, string value)
	{
		if (value == "")
			throw new ApplicationException ("Empty data binding tag.");

		string control_id = controls.PeekControlID ();
		string control_type_string = controls.PeekType ().ToString ();
		StringBuilder db_function = controls.DataBindFunction;
		string container;
		if (controls.Container == null)
			container = "System.Web.UI.Control";
		else
			container = controls.Container.ToString ();

		if (db_function.Length == 0)
			db_function.AppendFormat ("\t\tpublic void __DataBind_{0} (object sender, " + 
						  "System.EventArgs e) {{\n" +
						  "\t\t\t{1} Container;\n" +
						  "\t\t\t{2} target;\n" +
						  "\t\t\ttarget = ({2}) sender;\n" +
						  "\t\t\tContainer = ({1}) target.BindingContainer;\n",
						  control_id, container, control_type_string);

		/* Removes '<%#' and '%>' */
		string real_value = value.Remove (0,3);
		real_value = real_value.Remove (real_value.Length - 2, 2);
		real_value = real_value.Trim ();

		if (target == typeof (string))
			db_function.AppendFormat ("\t\t\ttarget.{0} = System.Convert.ToString ({1});\n",
						  varName, real_value);
		else
			db_function.AppendFormat ("\t\t\ttarget.{0} = ({1}) ({2});\n",
						  varName, target, real_value);
	}

	/*
	 * Returns true if it generates some code for the specified property
	 */
	private void AddCodeForPropertyOrField (Type type, string var_name, string att, bool isDataBound)
	{
		/* FIXME: should i check for this or let the compiler fail?
		 * if (!prop.CanWrite)
		 *    ....
		 */
		if (isDataBound) {
			DataBoundProperty (type, var_name, att);
		}
		else if (type == typeof (string)){
			if (att == null)
				throw new ApplicationException ("null value for attribute " + var_name );

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = \"{1}\";\n", var_name,
							Escape (att)); // FIXME: really Escape this?
		} 
		else if (type.IsEnum){
			if (att == null)
				throw new ApplicationException ("null value for attribute " + var_name );

			string enum_value = EnumValueNameToString (type, att);

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = {1};\n", var_name, enum_value);
		} 
		else if (type == typeof (bool)){
			string value;
			if (att == null)
				value = "true"; //FIXME: is this ok for non Style properties?
			else if (0 == String.Compare (att, "true", true))
				value = "true";
			else if (0 == String.Compare (att, "false", true))
				value = "false";
			else
				throw new ApplicationException ("Value '" + att  + "' is not a valid boolean.");

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = {1};\n", var_name, value);
		}
		else if (type == typeof (System.Web.UI.WebControls.Unit)){
			 //FIXME: should use the culture specified in Page
			try {
				Unit value = Unit.Parse (att, System.Globalization.CultureInfo.InvariantCulture);
			} catch (Exception) {
				throw new ApplicationException ("'" + att + "' cannot be parsed as a unit.");
			}
			current_function.AppendFormat ("\t\t\t__ctrl.{0} = " + 
							"System.Web.UI.WebControls.Unit.Parse (\"{1}\", " + 
							"System.Globalization.CultureInfo.InvariantCulture);\n", 
							var_name, att);
		}
		else if (type == typeof (System.Web.UI.WebControls.FontUnit)){
			 //FIXME: should use the culture specified in Page
			try {
				FontUnit value = FontUnit.Parse (att, System.Globalization.CultureInfo.InvariantCulture);
			} catch (Exception) {
				throw new ApplicationException ("'" + att + "' cannot be parsed as a unit.");
			}
			current_function.AppendFormat ("\t\t\t__ctrl.{0} = " + 
							"System.Web.UI.WebControls.FontUnit.Parse (\"{1}\", " + 
							"System.Globalization.CultureInfo.InvariantCulture);\n", 
							var_name, att);
		}
		else if (type == typeof (Int16) || type == typeof (Int32) || type == typeof (Int64)) {
			long value;
			try {
				value = Int64.Parse (att); //FIXME: should use the culture specified in Page
			} catch (Exception){
				throw new ApplicationException (att + " is not a valid signed number " + 
								"or is out of range.");
			}

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = {1};\n", var_name, value);
		}
		else if (type == typeof (UInt16) || type == typeof (UInt32) || type == typeof (UInt64)) {
			ulong value;
			try {
				value = UInt64.Parse (att); //FIXME: should use the culture specified in Page
			} catch (Exception){
				throw new ApplicationException (att + " is not a valid unsigned number " + 
								"or is out of range.");
			}

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = {1};\n", var_name, value);
		}
		else if (type == typeof (float)) {
			float value;
			try {
				value = Single.Parse (att);
			} catch (Exception){
				throw new ApplicationException (att + " is not  avalid float number or " +
								"is out of range.");
			}

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = {1};\n", var_name, value);
		}
		else if (type == typeof (double)){
			double value;
			try {
				value = Double.Parse (att);
			} catch (Exception){
				throw new ApplicationException (att + " is not  avalid double number or " +
								"is out of range.");
			}

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = {1};\n", var_name, value);
		}
		else if (type == typeof (System.Drawing.Color)){
			Color c;
			try {
				c = (Color) TypeDescriptor.GetConverter (typeof (Color)).ConvertFromString (att);
			} catch (Exception e){
				throw new ApplicationException ("Color " + att + " is not a valid color.", e);
			}

			// Should i also test for IsSystemColor?
			// Are KnownColor members in System.Drawing.Color?
			if (c.IsKnownColor){
				current_function.AppendFormat ("\t\t\t__ctrl.{0} = System.Drawing.Color." +
							       "{1};\n", var_name, c.Name);
			}
			else {
				current_function.AppendFormat ("\t\t\t__ctrl.{0} = System.Drawing.Color." +
							       "FromArgb ({1}, {2}, {3}, {4});\n",
							       var_name, c.A, c.R, c.G, c.B);
			}
		}	
		else {
			throw new ApplicationException ("Unsupported type in property: " + 
							type.ToString ());
		}
	}

	private bool ProcessPropertiesAndFields (MemberInfo member, string id, TagAttributes att)
	{
		int hyphen = id.IndexOf ('-');

		bool isPropertyInfo = (member is PropertyInfo);

		bool is_processed = false;
		bool isDataBound = att.IsDataBound ((string) att [id]);
		Type type;
		if (isPropertyInfo) {
			type = ((PropertyInfo) member).PropertyType;
			if (hyphen == -1 && ((PropertyInfo) member).CanWrite == false)
				return false;
		} else {
			type = ((FieldInfo) member).FieldType;
		}

		if (0 == String.Compare (member.Name, id, true)){
			AddCodeForPropertyOrField (type, member.Name, (string) att [id], isDataBound);
			is_processed = true;
		} else if (hyphen != -1 && (type == fontinfoType || type == styleType || type.IsSubclassOf (styleType))){
			string prop_field = id.Replace ("-", ".");
			string [] parts = prop_field.Split (new char [] {'.'});
			if (parts.Length != 2 || 0 != String.Compare (member.Name, parts [0], true))
				return false;

			PropertyInfo [] subprops = type.GetProperties ();
			foreach (PropertyInfo subprop in subprops){
				if (0 != String.Compare (subprop.Name, parts [1], true))
					continue;

				if (subprop.CanWrite == false)
					return false;

				bool is_bool = subprop.PropertyType == typeof (bool);
				if (!is_bool && att [id] == null){
					att [id] = ""; // Font-Size -> Font-Size="" as html
					return false;
				}

				string value;
				if (att [id] == null && is_bool)
					value = "true"; // Font-Bold <=> Font-Bold="true"
				else
					value = (string) att [id];

				AddCodeForPropertyOrField (subprop.PropertyType,
						 member.Name + "." + subprop.Name,
						 value, isDataBound);
				is_processed = true;
			}
		}

		return is_processed;
	}
	
	private void AddCodeForAttributes (Type type, TagAttributes att)
	{
		EventInfo [] ev_info = type.GetEvents ();
		PropertyInfo [] prop_info = type.GetProperties ();
		FieldInfo [] field_info = type.GetFields ();
		bool is_processed = false;
		ArrayList processed = new ArrayList ();

		foreach (string id in att.Keys){
			if (0 == String.Compare (id, "runat", true) || 0 == String.Compare (id, "id", true))
				continue;

			if (id.Length > 2 && id.Substring (0, 2).ToUpper () == "ON"){
				string id_as_event = id.Substring (2);
				foreach (EventInfo ev in ev_info){
					if (0 == String.Compare (ev.Name, id_as_event, true)){
						current_function.AppendFormat (
								"\t\t\t__ctrl.{0} += " + 
								"new {1} (this.{2});\n", 
								ev.Name, ev.EventHandlerType, att [id]);
						is_processed = true;
						break;
					}
				}
				if (is_processed){
					is_processed = false;
					continue;
				}
			} 

			foreach (PropertyInfo prop in prop_info){
				is_processed = ProcessPropertiesAndFields (prop, id, att);
				if (is_processed)
					break;
			}

			if (!is_processed) {
				foreach (FieldInfo field in field_info){
					is_processed = ProcessPropertiesAndFields (field, id, att);
					if (is_processed)
						break;
				}
			}

			if (is_processed){
				is_processed = false;
				continue;
			}

			current_function.AppendFormat ("\t\t\t((System.Web.UI.IAttributeAccessor) __ctrl)." +
						"SetAttribute (\"{0}\", \"{1}\");\n",
						id, Escape ((string) att [id]));
		}
	}
	
	private void AddCodeRenderControl (StringBuilder function)
	{
		AddCodeRenderControl (function, controls.CodeRenderIndex);
	}

	private void AddCodeRenderControl (StringBuilder function, int index)
	{
		function.AppendFormat ("\t\t\tparameterContainer.Controls [{0}]." + 
				       "RenderControl (__output);\n", index);
	}

	private void AddRenderMethodDelegate (StringBuilder function, string control_id)
	{
		function.AppendFormat ("\t\t\t__ctrl.SetRenderMethodDelegate (new System.Web." + 
				       "UI.RenderMethod (this.__Render_{0}));\n", control_id);
	}

	private void AddCodeRenderFunction (string codeRender, string control_id)
	{
		StringBuilder codeRenderFunction = new StringBuilder ();
		codeRenderFunction.AppendFormat ("\t\tprivate void __Render_{0} " + 
						 "(System.Web.UI.HtmlTextWriter __output, " + 
						 "System.Web.UI.Control parameterContainer)\n" +
						 "\t\t{{\n", control_id);
		codeRenderFunction.Append (codeRender);
		codeRenderFunction.Append ("\t\t}\n\n");
		init_funcs.Append (codeRenderFunction);
	}

	private void RemoveLiterals (StringBuilder function)
	{
		string no_literals = Regex.Replace (function.ToString (),
						    @"\t\t\t__parser.AddParsedSubObject \(" + 
						    @"new System.Web.UI.LiteralControl \(.+\);\n", "");
		function.Length = 0;
		function.Append (no_literals);
	}

	private bool FinishControlFunction (string tag_id)
	{
		if (functions.Count == 0)
			throw new ApplicationException ("Unbalanced open/close tags");

		if (controls.Count == 0)
			return false;

		string saved_id = controls.PeekTagID ();
		if (0 != String.Compare (saved_id, tag_id, true))
			return false;

		StringBuilder old_function = (StringBuilder) functions.Pop ();
		current_function = (StringBuilder) functions.Peek ();

		string control_id = controls.PeekControlID ();
		Type control_type = controls.PeekType ();
		ChildrenKind child_kind = controls.PeekChildKind ();

		bool hasDataBindFunction = controls.HasDataBindFunction ();
		if (hasDataBindFunction)
			old_function.AppendFormat ("\t\t\t__ctrl.DataBinding += new System.EventHandler " +
						   "(this.__DataBind_{0});\n", control_id);

		bool useCodeRender = controls.UseCodeRender;
		if (useCodeRender)
			AddRenderMethodDelegate (old_function, control_id);
		
		if (control_type == typeof (System.Web.UI.ITemplate)){
			old_function.Append ("\n\t\t}\n\n");
			current_function.AppendFormat ("\t\t\t__ctrl.{0} = new System.Web.UI." + 
						       "CompiledTemplateBuilder (new System.Web.UI." +
						       "BuildTemplateMethod (this.__BuildControl_{1}));\n",
						       saved_id, control_id);
		}
		else if (control_type == typeof (System.Web.UI.WebControls.DataGridColumnCollection)){
			old_function.Append ("\n\t\t}\n\n");
			current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} (__ctrl.{1});\n",
							control_id, saved_id);
		}
		else if (control_type == typeof (System.Web.UI.WebControls.DataGridColumn) ||
			 control_type.IsSubclassOf (typeof (System.Web.UI.WebControls.DataGridColumn)) ||
			 control_type == typeof (System.Web.UI.WebControls.ListItem)){
			old_function.Append ("\n\t\t}\n\n");
			string parsed = "";
			string ctrl_name = "ctrl";
			Type cont = controls.Container;
			if (cont == null || cont == typeof (System.Web.UI.HtmlControls.HtmlSelect)){
				parsed = "ParsedSubObject";
				ctrl_name = "parser";
			}

			current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} ();\n" +
						       "\t\t\t__{1}.Add{2} (this.{0});\n\n",
						       control_id, ctrl_name, parsed);
		}
		else if (child_kind == ChildrenKind.LISTITEM){
			old_function.Append ("\n\t\t}\n\n");
			init_funcs.Append (old_function); // Closes the BuildList function
			old_function = (StringBuilder) functions.Pop ();
			current_function = (StringBuilder) functions.Peek ();
			old_function.AppendFormat ("\n\t\t\tthis.__BuildControl_{0} (__ctrl.{1});\n\t\t\t" +
						   "return __ctrl;\n\t\t}}\n\n",
						   control_id, controls.PeekDefaultPropertyName ());

			controls.Pop ();
			control_id = controls.PeekControlID ();
			current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} ();\n\t\t\t__parser." +
						       "AddParsedSubObject (this.{0});\n\n", control_id);
		} else if (control_type == typeof (HtmlTableCell)) {
			old_function.Append ("\n\t\t\treturn __ctrl;\n\t\t}\n\n");
			object top = controls.Pop ();
			Type t = controls.PeekType ();
			controls.Push (top);
			string parsed = "";
			string ctrl_name = "ctrl";
			if (t != typeof (HtmlTableRow)) {
				parsed = "ParsedSubObject";
				ctrl_name = "parser";
			}

			current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} ();\n" +
						       "\t\t\t__{1}.Add{2} (this.{0});\n\n",
						       control_id, ctrl_name, parsed);
		} else if (child_kind == ChildrenKind.HTMLROW || child_kind == ChildrenKind.HTMLCELL) {
			old_function.Append ("\n\t\t}\n\n");
			init_funcs.Append (old_function);
			old_function = (StringBuilder) functions.Pop ();
			current_function = (StringBuilder) functions.Peek ();
			old_function.AppendFormat ("\n\t\t\tthis.__BuildControl_{0} (__ctrl.{1});\n\t\t\t" +
						   "return __ctrl;\n\t\t}}\n\n",
						   control_id, controls.PeekDefaultPropertyName ());

			controls.Pop ();
			control_id = controls.PeekControlID ();
			current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} ();\n", control_id);
			if (child_kind == ChildrenKind.HTMLROW) {
				current_function.AppendFormat ("\t\t\t__parser.AddParsedSubObject ({0});\n",
								control_id);
			} else {
				current_function.AppendFormat ("\t\t\t__ctrl.Add (this.{0});\n", control_id);
			}
		} else {
			old_function.Append ("\n\t\t\treturn __ctrl;\n\t\t}\n\n");
			current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} ();\n\t\t\t__parser." +
						       "AddParsedSubObject (this.{0});\n\n", control_id);
		}

		if (useCodeRender)
			RemoveLiterals (old_function);

		init_funcs.Append (old_function);
		if (useCodeRender)
			AddCodeRenderFunction (controls.CodeRenderFunction.ToString (), control_id);
		
		if (hasDataBindFunction){
			StringBuilder db_function = controls.DataBindFunction;
			db_function.Append ("\t\t}\n\n");
			init_funcs.Append (db_function);
		}

		// Avoid getting empty stacks for unbalanced open/close tags
		if (controls.Count > 1){
			controls.Pop ();
			AddCodeRenderControl (controls.CodeRenderFunction, controls.ChildIndex);
		}

		return true;
	}

	private void NewTableElementFunction (HtmlControlTag ctrl)
	{
		string control_id = Tag.GetDefaultID ();
		ChildrenKind child_kind;

		Type t;
		if (ctrl.ControlType == typeof (HtmlTable)) {
			t = typeof (HtmlTableRowCollection);
			child_kind = ChildrenKind.HTMLROW;
		} else {
			t = typeof (HtmlTableCellCollection);
			child_kind = ChildrenKind.HTMLCELL;
		}

		controls.Push (ctrl.ControlType,
			       control_id,
			       ctrl.TagID,
			       child_kind,
			       ctrl.ParseChildren);


		current_function = new StringBuilder ();
		functions.Push (current_function);
		current_function.AppendFormat ("\t\tprivate void __BuildControl_{0} ({1} __ctrl)\n" +
						"\t\t{{\n", control_id, t);
	}

	private void ProcessHtmlControlTag ()
	{
		HtmlControlTag html_ctrl = (HtmlControlTag) elements.Current;
		if (html_ctrl.TagID.ToUpper () == "SCRIPT"){
			//FIXME: if the is script is to be read from disk, do it!
			if (html_ctrl.SelfClosing)
				throw new ApplicationException ("Read script from file not supported yet.");

			if (elements.MoveNext () == false)
				throw new ApplicationException ("Error after " + html_ctrl.ToString ());

			if (elements.Current is PlainText){
				script.Append (((PlainText) elements.Current).Text);
				if (!elements.MoveNext ())
					throw new ApplicationException ("Error after " +
									elements.Current.ToString ());
			}

			if (elements.Current is CloseTag)
				elements.MoveNext ();
			return;
		} else if (IsApplication) {
			throw new ApplicationException (app_file_wrong);
		}
		
		Type controlType = html_ctrl.ControlType;
		AddProtectedField (controlType, html_ctrl.ControlID);

		ChildrenKind children_kind;
		if (0 == String.Compare (html_ctrl.TagID, "table", true))
			children_kind = ChildrenKind.HTMLROW;
		else if (0 == String.Compare (html_ctrl.TagID, "tr", true))
			children_kind = ChildrenKind.HTMLCELL;
		else if (0 != String.Compare (html_ctrl.TagID, "select", true))
			children_kind = html_ctrl.IsContainer ? ChildrenKind.CONTROLS :
								ChildrenKind.NONE;
		else
			children_kind = ChildrenKind.OPTION;

		NewControlFunction (html_ctrl.TagID, html_ctrl.ControlID, controlType, children_kind, html_ctrl.ParseChildren); 

		current_function.AppendFormat ("\t\t\t__ctrl.ID = \"{0}\";\n", html_ctrl.ControlID);
		AddCodeForAttributes (html_ctrl.ControlType, html_ctrl.Attributes);

		if (children_kind == ChildrenKind.HTMLROW || children_kind == ChildrenKind.HTMLCELL)
			NewTableElementFunction (html_ctrl);

		if (!html_ctrl.SelfClosing)
			JustDoIt ();
		else
			FinishControlFunction (html_ctrl.TagID);
	}

	// Closing is performed in FinishControlFunction ()
	private void NewBuildListFunction (AspComponent component)
	{
		string control_id = Tag.GetDefaultID ();

		controls.Push (component.ComponentType,
			       control_id, 
			       component.TagID, 
			       ChildrenKind.LISTITEM, 
			       component.DefaultPropertyName);

		current_function = new StringBuilder ();
		functions.Push (current_function);
		current_function.AppendFormat ("\t\tprivate void __BuildControl_{0} " +
						"(System.Web.UI.WebControls.ListItemCollection __ctrl)\n" +
						"\t\t{{\n", control_id);
	}

	private void ProcessComponent ()
	{
		AspComponent component = (AspComponent) elements.Current;
		Type component_type = component.ComponentType;
		AddProtectedField (component_type, component.ControlID);

		NewControlFunction (component.TagID, component.ControlID, component_type,
				    component.ChildrenKind, component.DefaultPropertyName); 

		if (component_type.IsSubclassOf (typeof (System.Web.UI.UserControl)))
			current_function.Append ("\t\t\t__ctrl.InitializeAsUserControl (Page);\n");

		if (component_type.IsSubclassOf (typeof (System.Web.UI.Control)))
			current_function.AppendFormat ("\t\t\t__ctrl.ID = \"{0}\";\n", component.ControlID);

		AddCodeForAttributes (component.ComponentType, component.Attributes);
		if (component.ChildrenKind == ChildrenKind.LISTITEM)
			NewBuildListFunction (component);

		if (!component.SelfClosing)
			JustDoIt ();
		else
			FinishControlFunction (component.TagID);
	}

	private void ProcessServerObjectTag ()
	{
		ServerObjectTag obj = (ServerObjectTag) elements.Current;
		declarations.AppendFormat ("\t\tprivate {0} cached{1};\n", obj.ObjectClass, obj.ObjectID);
		constructor.AppendFormat ("\n\t\tprivate {0} {1}\n\t\t{{\n\t\t\tget {{\n\t\t\t\t" + 
					  "if (this.cached{1} == null)\n\t\t\t\t\tthis.cached{1} = " + 
					  "new {0} ();\n\t\t\t\treturn cached{1};\n\t\t\t}}\n\t\t}}\n\n",
					  obj.ObjectClass, obj.ObjectID);
	}

	// Creates a new function that sets the values of subproperties.
	private void NewStyleFunction (PropertyTag tag)
	{
		current_function = new StringBuilder ();

		string prop_id = tag.PropertyID;
		Type prop_type = tag.PropertyType;
		// begin function
		current_function.AppendFormat ("\t\tprivate void __BuildControl_{0} ({1} __ctrl)\n" +
						"\t\t{{\n", prop_id, prop_type);
		
		// Add property initialization code
		PropertyInfo [] subprop_info = prop_type.GetProperties ();
		TagAttributes att = tag.Attributes;

		string subprop_name = null;
		foreach (string id in att.Keys){
			if (0 == String.Compare (id, "runat", true) || 0 == String.Compare (id, "id", true))
				continue;

			bool is_processed = false;
			foreach (PropertyInfo subprop in subprop_info){
				is_processed = ProcessPropertiesAndFields (subprop, id, att);
				if (is_processed){
					subprop_name = subprop.Name;
					break;
				}
			}

			if (subprop_name == null)
				throw new ApplicationException ("Property " + tag.TagID + " does not have " + 
								"a " + id + " subproperty.");
		}

		// Finish function
		current_function.Append ("\n\t\t}\n\n");
		init_funcs.Append (current_function);
		current_function = (StringBuilder) functions.Peek ();
		current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} (__ctrl.{1});\n",
						prop_id, tag.PropertyName);

		if (!tag.SelfClosing){
			// Next tag should be the closing tag
			controls.Push (null, null, null, ChildrenKind.NONE, null);
			bool closing_tag_found = false;
			Element elem;
			while (!closing_tag_found && elements.MoveNext ()){
				elem = (Element) elements.Current;
				if (elem is PlainText)
					ProcessPlainText ();
				else if (!(elem is CloseTag))
					throw new ApplicationException ("Tag " + tag.TagID + 
									" not properly closed.");
				else
					closing_tag_found = true;
			}

			if (!closing_tag_found)
				throw new ApplicationException ("Tag " + tag.TagID + " not properly closed.");

			controls.Pop ();
		}
	}

	// This one just opens the function. Closing is performed in FinishControlFunction ()
	private void NewTemplateFunction (PropertyTag tag)
	{
		/*
		 * FIXME
		 * This function does almost the same as NewControlFunction.
		 * Consider merging.
		 */
		string prop_id = tag.PropertyID;
		Type prop_type = tag.PropertyType;
		string tag_id = tag.PropertyName; // Real property name used in FinishControlFunction

		controls.Push (prop_type, prop_id, tag_id, ChildrenKind.CONTROLS, null);
		current_function = new StringBuilder ();
		functions.Push (current_function);
		current_function.AppendFormat ("\t\tprivate void __BuildControl_{0} " +
						"(System.Web.UI.Control __ctrl)\n" +
						"\t\t{{\n" +
						"\t\t\tSystem.Web.UI.IParserAccessor __parser " + 
						"= (System.Web.UI.IParserAccessor) __ctrl;\n" , prop_id);
	}

	// Closing is performed in FinishControlFunction ()
	private void NewDBColumnFunction (PropertyTag tag)
	{
		/*
		 * FIXME
		 * This function also does almost the same as NewControlFunction.
		 * Consider merging.
		 */
		string prop_id = tag.PropertyID;
		Type prop_type = tag.PropertyType;
		string tag_id = tag.PropertyName; // Real property name used in FinishControlFunction

		controls.Push (prop_type, prop_id, tag_id, ChildrenKind.DBCOLUMNS, null);
		current_function = new StringBuilder ();
		functions.Push (current_function);
		current_function.AppendFormat ("\t\tprivate void __BuildControl_{0} " +
						"(System.Web.UI.WebControl.DataGridColumnCollection __ctrl)\n" +
						"\t\t{{\n", prop_id);
	}

	private void NewPropertyFunction (PropertyTag tag)
	{
		if (tag.PropertyType == typeof (System.Web.UI.WebControls.Style) ||
		    tag.PropertyType.IsSubclassOf (typeof (System.Web.UI.WebControls.Style)))
			NewStyleFunction (tag);
		else if (tag.PropertyType == typeof (System.Web.UI.ITemplate))
			NewTemplateFunction (tag);
		else if (tag.PropertyType == typeof (System.Web.UI.WebControls.DataGridColumnCollection))
			NewDBColumnFunction (tag);
		else
			throw new ApplicationException ("Other than Style and ITemplate not supported yet. " + 
							tag.PropertyType);
	}
	
	private void ProcessHtmlTag ()
	{
		Tag tag = (Tag) elements.Current;
		ChildrenKind child_kind = controls.PeekChildKind ();
		if (child_kind == ChildrenKind.NONE){
			string tag_id = controls.PeekTagID ();
			throw new ApplicationException (tag + " not allowed inside " + tag_id);
		}
					
		if (child_kind == ChildrenKind.OPTION){
			if (0 != String.Compare (tag.TagID, "option", true))
				throw new ApplicationException ("Only <option> tags allowed inside <select>.");

			string default_id = Tag.GetDefaultID ();
			Type type = typeof (System.Web.UI.WebControls.ListItem);
			AddProtectedField (type, default_id);
			NewControlFunction (tag.TagID, default_id, type, ChildrenKind.CONTROLS, null); 
			return;
		}

		if (child_kind == ChildrenKind.CONTROLS) {
			ArrayList tag_elements = tag.GetElements ();
			foreach (Element e in tag_elements) {
				if (e is PlainText) {
					elements.Current = e;
					ProcessPlainText ();
				} else if (e is CodeRenderTag) {
					elements.Current = e;
					ProcessCodeRenderTag ();
				} else if (e is DataBindingTag) {
					elements.Current = e;
					ProcessDataBindingLiteral ();
				} else {
					throw new ApplicationException (fullPath + ": unexpected tag type " + e.GetType ());
				}
			}
			return;
		}

		if (child_kind == ChildrenKind.HTMLROW) {
			if (0 == String.Compare (tag.TagID, "tr", true)) {
				elements.Current = new HtmlControlTag (tag);
				ProcessHtmlControlTag ();
				return;
			}
		}

		if (child_kind == ChildrenKind.HTMLCELL) {
			if (0 == String.Compare (tag.TagID, "td", true)) {
				elements.Current = new HtmlControlTag (tag);
				ProcessHtmlControlTag ();
				return;
			}
		}

		// Now child_kind should be PROPERTIES, so only allow tag_id == property
		Type control_type = controls.PeekType ();
		PropertyInfo [] prop_info = control_type.GetProperties ();
		bool is_processed = false;
		foreach (PropertyInfo prop in prop_info){
			if (0 == String.Compare (prop.Name, tag.TagID, true)){
				PropertyTag prop_tag = new PropertyTag (tag, prop.PropertyType, prop.Name);
				NewPropertyFunction (prop_tag);
				is_processed = true;
				break;
			}
		}
		
		if (!is_processed){
			string tag_id = controls.PeekTagID ();
			throw new ApplicationException (tag.TagID + " is not a property of " + control_type);
		}
	}

	private Tag Map (Tag tag)
	{
		int pos = tag.TagID.IndexOf (":");
		if (pos == -1) {
			ChildrenKind child_kind = controls.PeekChildKind ();
			if (child_kind == ChildrenKind.HTMLROW && 0 == String.Compare (tag.TagID, "tr", true)) {
				tag.Attributes.Add ("runat", "server");
				return new HtmlControlTag (tag);
			} else if (child_kind == ChildrenKind.HTMLROW && 0 == String.Compare (tag.TagID, "tr", true)) {
				tag.Attributes.Add ("runat", "server");
				return new HtmlControlTag (tag);
			}
		}

		if (tag is CloseTag ||
		    ((tag.Attributes == null || 
		    !tag.Attributes.IsRunAtServer ()) && pos == -1))
			return tag;

		if (pos == -1){
			if (0 == String.Compare (tag.TagID, "object", true))
				return new ServerObjectTag (tag);

			return new HtmlControlTag (tag);
		}

		string foundry_name = tag.TagID.Substring (0, pos);
		string component_name = tag.TagID.Substring (pos + 1);

		if (Foundry.LookupFoundry (foundry_name) == false)
			throw new ApplicationException ("Cannot find foundry for alias'" + foundry_name + "'");

		AspComponent component = Foundry.MakeAspComponent (foundry_name, component_name, tag);
		if (component == null)
			throw new ApplicationException ("Cannot find component '" + component_name + 
							"' for alias '" + foundry_name + "'");

		return component;
	}
	
	private void ProcessCloseTag ()
	{
		CloseTag close_tag = (CloseTag) elements.Current;
		if (FinishControlFunction (close_tag.TagID))
				return;

		elements.Current = new PlainText (close_tag.PlainHtml);
		ProcessPlainText ();
	}

	private void ProcessDataBindingLiteral ()
	{
		DataBindingTag dataBinding = (DataBindingTag) elements.Current;
		string actual_value = dataBinding.Data;
		if (actual_value == "")
			throw new ApplicationException ("Empty data binding tag.");

		if (controls.PeekChildKind () != ChildrenKind.CONTROLS)
			throw new ApplicationException ("Data bound content not allowed for " + 
							controls.PeekTagID ());

		StringBuilder db_function = new StringBuilder ();
		string control_id = Tag.GetDefaultID ();
		string control_type_string = "System.Web.UI.DataBoundLiteralControl";
		AddProtectedField (typeof (System.Web.UI.DataBoundLiteralControl), control_id);
		// Build the control
		db_function.AppendFormat ("\t\tprivate System.Web.UI.Control __BuildControl_{0} ()\n" +
					  "\t\t{{\n\t\t\t{1} __ctrl;\n\n" +
					  "\t\t\t__ctrl = new {1} (0, 1);\n" + 
					  "\t\t\tthis.{0} = __ctrl;\n" +
					  "\t\t\t__ctrl.DataBinding += new System.EventHandler " + 
					  "(this.__DataBind_{0});\n" +
					  "\t\t\treturn __ctrl;\n"+
					  "\t\t}}\n\n",
					  control_id, control_type_string);
		// DataBinding handler
		db_function.AppendFormat ("\t\tpublic void __DataBind_{0} (object sender, " + 
					  "System.EventArgs e) {{\n" +
					  "\t\t\t{1} Container;\n" +
					  "\t\t\t{2} target;\n" +
					  "\t\t\ttarget = ({2}) sender;\n" +
					  "\t\t\tContainer = ({1}) target.BindingContainer;\n" +
					  "\t\t\ttarget.SetDataBoundString (0, System.Convert." +
					  "ToString ({3}));\n" +
					  "\t\t}}\n\n",
					  control_id, controls.Container, control_type_string,
					  actual_value);

		init_funcs.Append (db_function);
		current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} ();\n\t\t\t__parser." +
					       "AddParsedSubObject (this.{0});\n\n", control_id);

		AddCodeRenderControl (controls.CodeRenderFunction);
	}

	private void ProcessCodeRenderTag ()
	{
		CodeRenderTag code_tag = (CodeRenderTag) elements.Current;

		controls.UseCodeRender = true;
		if (code_tag.IsVarName)
			controls.CodeRenderFunction.AppendFormat ("\t\t\t__output.Write ({0});\n",
								  code_tag.Code);
		else
			controls.CodeRenderFunction.AppendFormat ("\t\t\t{0}\n", code_tag.Code);
	}
	
	public void ProcessElements ()
	{
		JustDoIt ();
		End ();
		parse_ok = true;
	}
	
	private void JustDoIt ()
	{
		Element element;

		while (elements.MoveNext ()){
			element = (Element) elements.Current;
			if (element is Directive){
				ProcessDirective ();
			} else if (element is PlainText){
				ProcessPlainText ();
			} else if (element is DataBindingTag){
				if (IsApplication)
					throw new ApplicationException (app_file_wrong);
				ProcessDataBindingLiteral ();
			} else if (element is CodeRenderTag){
				if (IsApplication)
					throw new ApplicationException (app_file_wrong);
				ProcessCodeRenderTag ();
			} else {
				elements.Current = Map ((Tag) element);
				if (elements.Current is ServerObjectTag) {
					ProcessServerObjectTag ();
					continue;
				}

				if (elements.Current is HtmlControlTag) {
					ProcessHtmlControlTag ();
					continue;
				}

				if (IsApplication)
					throw new ApplicationException (app_file_wrong);

				else if (elements.Current is AspComponent)
					ProcessComponent ();
				else if (elements.Current is CloseTag)
					ProcessCloseTag ();
				else if (elements.Current is Tag)
					ProcessHtmlTag ();
				else
					throw new ApplicationException ("This place should not be reached.");
			}
		}
	}

	private string GetTemplateDirectory ()
	{
		string templatePath = Path.GetDirectoryName (fullPath);
		string appPath = Path.GetDirectoryName (HttpRuntime.AppDomainAppPath);

		if (templatePath == appPath)
			return "/";

		templatePath = templatePath.Substring (appPath.Length);
		if (Path.DirectorySeparatorChar != '/')
			templatePath = templatePath.Replace (Path.DirectorySeparatorChar, '/');
			
		return templatePath;
	}

	private void End ()
	{
		if (isPage) {
			if (sessionState == SessionState.Enabled || sessionState == SessionState.ReadOnly)
				AddInterface (typeof (System.Web.SessionState.IRequiresSessionState));

			if (sessionState == SessionState.ReadOnly)
				AddInterface (typeof (System.Web.SessionState.IReadOnlySessionState));
		}
		
		classDecl = "\tpublic class " + className + " : " + parent + interfaces + " {\n"; 
		prolog.Append ("\n" + classDecl);
		declarations.Append ("\t\tprivate static bool __intialized = false;\n\n");
		if (IsPage)
			declarations.Append ("\t\tprivate static ArrayList __fileDependencies;\n\n");

		// adds the constructor
		constructor.AppendFormat ("\t\tpublic {0} ()\n\t\t{{\n", className);
		if (!IsApplication)
			constructor.Append ("\t\t\tSystem.Collections.ArrayList dependencies;\n\n");
			
		constructor.AppendFormat ("\t\t\tif (ASP.{0}.__intialized == false){{\n", className); 

		if (IsPage) {
			constructor.AppendFormat ("\t\t\t\tdependencies = new System.Collections.ArrayList ();\n" +
						"\t\t\t\tdependencies.Add (@\"{1}\");\n" +
						"\t\t\t\tASP.{0}.__fileDependencies = dependencies;\n",
						className, fullPath);
		}

		constructor.AppendFormat ("\t\t\t\tASP.{0}.__intialized = true;\n\t\t\t}}\n\t\t}}\n\n",
					  className);
         
		if (!IsApplication) {
			//FIXME: add AutoHandlers: don't know what for...yet!
			constructor.AppendFormat (
				"\t\tprotected override int AutoHandlers\n\t\t{{\n" +
				"\t\t\tget {{ return ASP.{0}.__autoHandlers; }}\n" +
				"\t\t\tset {{ ASP.{0}.__autoHandlers = value; }}\n" +
				"\t\t}}\n\n", className);

			constructor.Append (
				"\t\tprotected System.Web.HttpApplication ApplicationInstance\n\t\t{\n" +
				"\t\t\tget { return (System.Web.HttpApplication) this.Context.ApplicationInstance; }\n" +
				"\t\t}\n\n");

			constructor.AppendFormat (
				"\t\tpublic override string TemplateSourceDirectory\n\t\t{{\n" +
				"\t\t\tget {{ return \"{0}\"; }}\n" +
				"\t\t}}\n\n", GetTemplateDirectory ());

			epilog.Append ("\n\t\tprotected override void FrameworkInitialize ()\n\t\t{\n" +
					"\t\t\tthis.__BuildControlTree (this);\n");

			if (IsPage) {
				epilog.AppendFormat ("\t\t\tthis.FileDependencies = ASP.{0}.__fileDependencies;\n" +
							"\t\t\tthis.EnableViewStateMac = true;\n", className);
			}

			epilog.Append ("\t\t}\n\n");
		}

		if (IsPage) {
			Random rnd = new Random ();
			epilog.AppendFormat ("\t\tpublic override int GetTypeHashCode ()\n\t\t{{\n" +
					     "\t\t\treturn {0};\n" +
					     "\t\t}}\n", rnd.Next ());
		}

		epilog.Append ("\t}\n}\n");

		// Closes the currently opened tags
		StringBuilder old_function = current_function;
		string control_id;
		while (functions.Count > 1){
			old_function.Append ("\n\t\t\treturn __ctrl;\n\t\t}\n\n");
			init_funcs.Append (old_function);
			control_id = controls.PeekControlID ();
			FinishControlFunction (control_id);
			controls.AddChild ();
			old_function = (StringBuilder) functions.Pop ();
			current_function = (StringBuilder) functions.Peek ();
			controls.Pop ();
		}

		bool useCodeRender = controls.UseCodeRender;
		if (useCodeRender){
			RemoveLiterals (current_function);
			AddRenderMethodDelegate (current_function, controls.PeekControlID ());
		}
		
		current_function.Append ("\t\t}\n\n");
		init_funcs.Append (current_function);
		if (useCodeRender)
			AddCodeRenderFunction (controls.CodeRenderFunction.ToString (), controls.PeekControlID ());

		functions.Pop ();
	}

	//
	// Functions related to compilation of user controls
	//
	
	private static char dirSeparator = Path.DirectorySeparatorChar;
	struct UserControlData
	{
		public UserControlResult result;
		public string className;
		public string assemblyName;
	}

	private static UserControlData GenerateUserControl (string src, HttpContext context)
	{
		UserControlData data = new UserControlData ();
		data.result = UserControlResult.OK;

		UserControlCompiler compiler = new UserControlCompiler (new UserControlParser (src, context));
		Type t = compiler.GetCompiledType ();
		if (t == null) {
			data.result = UserControlResult.CompilationFailed;
			return data;
		}
		
		data.className = t.Name;
		data.assemblyName = compiler.TargetFile;
		
		return data;
	}
}

}

