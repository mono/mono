//
// System.Web.Compilation.AspElements
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace System.Web.Compilation
{
	
	enum ElementType
	{
		TAG,
		PLAINTEXT
	}

	abstract class Element
	{
		private ElementType elementType;

		public Element (ElementType type)
		{
			elementType = type;
		}
		
		public ElementType GetElementType
		{
			get { return elementType; }
		}
	} // class Element

	class PlainText : Element
	{
		private StringBuilder text;

		public PlainText () : base (ElementType.PLAINTEXT)
		{
			text = new StringBuilder ();
		}

		public PlainText (StringBuilder text) : base (ElementType.PLAINTEXT)
		{
			this.text = text;
		}

		public PlainText (string text) : this ()
		{
			this.text.Append (text);
		}

		public void Append (string more)
		{
			text.Append (more);
		}
		
		public string Text
		{
			get { return text.ToString (); }
		}

		public override string ToString ()
		{
			return "PlainText: " + Text;
		}
	}

	enum TagType
	{
		DIRECTIVE,
		HTML,
		HTMLCONTROL,
		SERVERCONTROL,
		INLINEVAR,
		INLINECODE,
		CLOSING,
		SERVEROBJECT,
		PROPERTYTAG,
		CODERENDER,
		DATABINDING,
		SERVERCOMMENT,
		NOTYET
	}

	/*
	 * Attributes and values are stored in a couple of ArrayList in Add ().
	 * When MakeHash () is called, they are converted to a Hashtable. If there are any
	 * attributes duplicated it throws an ArgumentException.
	 *
	 * The [] operator works with the Hashtable if the values are in it, otherwise
	 * it uses the ArrayList's.
	 *
	 * Why? You can have a tag in HTML like <a att="value" att="xxx">, but not in tags
	 * marked runat=server and Hashtable requires the key to be unique.
	 * 
	 */
	class TagAttributes
	{
		private Hashtable atts_hash;
		private ArrayList keys;
		private ArrayList values;
		private bool got_hashed;

		public TagAttributes ()
		{
			got_hashed = false;
			keys = new ArrayList ();
			values = new ArrayList ();
		}

		private void MakeHash ()
		{
			atts_hash = new Hashtable (new CaseInsensitiveHashCodeProvider (),
						   new CaseInsensitiveComparer ());
			for (int i = 0; i < keys.Count; i++)
				atts_hash.Add (keys [i], values [i]);
			got_hashed = true;
			keys = null;
			values = null;
		}
		
		public bool IsRunAtServer ()
		{
			return got_hashed;
		}

		public void Add (object key, object value)
		{
			if (key != null && value != null &&
			    0 == String.Compare ((string) key,  "runat", true) &&
			    0 == String.Compare ((string) value,  "server", true))
				MakeHash ();

			if (got_hashed)
				atts_hash.Add (key, value);
			else {
				keys.Add (key);
				values.Add (value);
			}
		}
		
		public ICollection Keys 
		{
			get { return (got_hashed ? atts_hash.Keys : keys); }
		}

		private int CaseInsensitiveSearch (string key)
		{
			// Hope not to have many attributes when the tag is not a server tag...
			for (int i = 0; i < keys.Count; i++){
				if (0 == String.Compare ((string) keys [i], key, true))
					return i;
			}
			return -1;
		}
		
		public object this [object key]
		{
			get {
				if (got_hashed)
					return atts_hash [key];

				int idx = CaseInsensitiveSearch ((string) key);
				if (idx == -1)
					return null;
						
				return values [idx];
			}

			set {
				if (got_hashed)
					atts_hash [key] = value;
				else {
					int idx = CaseInsensitiveSearch ((string) key);
					keys [idx] = value;
				}
			}
		}
		
		public int Count 
		{
			get { return (got_hashed ? atts_hash.Count : keys.Count);}
		}

		public bool IsDataBound (string att)
		{
			if (att == null || !got_hashed)
				return false;

			return (att.StartsWith ("<%#") && att.EndsWith ("%>"));
		}
		
		public override string ToString ()
		{
			string ret = "";
			string value;
			foreach (string key in Keys){
				value = (string) this [key];
				value = value == null ? "" : value;
				ret += key + "=" + value + " ";
			}

			return ret;
		}
	}

	class Tag : Element
	{
		protected string tag;
		protected TagType tagType;
		protected TagAttributes attributes;
		protected bool self_closing;
		protected bool hasDefaultID;
		private static int ctrlNumber = 1;

		internal Tag (ElementType etype) : base (etype) { }

		internal Tag (Tag other) :
			this (other.tag, other.attributes, other.self_closing)
		{
			this.tagType = other.tagType;
		}

		public Tag (string tag, TagAttributes attributes, bool self_closing) :
			  base (ElementType.TAG)
		{
			if (tag == null)
				throw new ArgumentNullException ();

			this.tag = tag;
			this.attributes = attributes;
			this.tagType = TagType.NOTYET;
			this.self_closing = self_closing;
			this.hasDefaultID = false;
		}
		
		public string TagID
		{
			get { return tag; }
		}

		public TagType TagType
		{
			get { return tagType; }
		}

		public bool SelfClosing
		{
			get { return self_closing; }
		}

		public TagAttributes Attributes
		{
			get { return attributes; }
		}

		public string PlainHtml
		{
			get {
				StringBuilder plain = new StringBuilder ();
				plain.Append ('<');
				if (tagType == TagType.CLOSING)
					plain.Append ('/');
				plain.Append (tag);
				if (attributes != null){
					plain.Append (' ');
					foreach (string key in attributes.Keys){
						plain.Append (key);
						if (attributes [key] != null){
							plain.Append ("=\"");
							plain.Append ((string) attributes [key]);
							plain.Append ("\" ");
						}
					}
				}
				
				if (self_closing)
					plain.Append ('/');
				plain.Append ('>');
				return plain.ToString ();
			}
		}

		public override string ToString ()
		{
			return TagID + " " + Attributes + " " + self_closing;
		}

		public bool HasDefaultID
		{
			get { return hasDefaultID; }
		}
		
		protected virtual void SetNewID ()
		{
			if (attributes == null)
				attributes = new TagAttributes ();
			attributes.Add ("ID", GetDefaultID ());
			hasDefaultID = true;
		}

		public static string GetDefaultID ()
		{
			return "_control" + ctrlNumber++;
		}
	}

	class CloseTag : Tag
	{
		public CloseTag (string tag) : base (tag, null, false)
		{
			tagType = TagType.CLOSING;
		}
	}

	class Directive : Tag
	{
		private static Hashtable directivesHash;
		private static string [] page_atts = {  "AspCompat", "AutoEventWireup ", "Buffer",
							"ClassName", "ClientTarget", "CodePage",
							"CompilerOptions", "ContentType", "Culture", "Debug",
							"Description", "EnableSessionState", "EnableViewState",
							"EnableViewStateMac", "ErrorPage", "Explicit",
							"Inherits", "Language", "LCID", "ResponseEncoding",
							"Src", "SmartNavigation", "Strict", "Trace",
							"TraceMode", "Transaction", "UICulture",
							"WarningLevel" };

		private static string [] control_atts = { "AutoEventWireup", "ClassName", "CompilerOptions",
							  "Debug", "Description", "EnableViewState",
							  "Explicit", "Inherits", "Language", "Strict", "Src",
							  "WarningLevel" };

		private static string [] import_atts = { "namespace" };
		private static string [] implements_atts = { "interface" };
		private static string [] assembly_atts = { "name", "src" };
		private static string [] register_atts = { "tagprefix", "tagname", "Namespace",
							   "Src", "Assembly" };

		private static string [] outputcache_atts = { "Duration", "Location", "VaryByControl", 
							      "VaryByCustom", "VaryByHeader", "VaryByParam" };
		private static string [] reference_atts = { "page", "control" };

		private static string [] webservice_atts = { "class", "codebehind", "debug", "language" };

		static Directive ()
		{
			InitHash ();
		}
		
		private static void InitHash ()
		{
			CaseInsensitiveHashCodeProvider provider = new CaseInsensitiveHashCodeProvider ();
			CaseInsensitiveComparer comparer =  new CaseInsensitiveComparer ();

			directivesHash = new Hashtable (provider, comparer); 

			// Use Hashtable 'cause is O(1) in Contains (ArrayList is O(n))
			Hashtable valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in page_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("PAGE", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in control_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("CONTROL", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in import_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("IMPORT", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in implements_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("IMPLEMENTS", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in register_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("REGISTER", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in assembly_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("ASSEMBLY", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in outputcache_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("OUTPUTCACHE", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in reference_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("REFERENCE", valid_attributes);

			valid_attributes = new Hashtable (provider, comparer);
			foreach (string att in webservice_atts) valid_attributes.Add (att, null);
			directivesHash.Add ("WEBSERVICE", valid_attributes);
		}
		
		public Directive (string tag, TagAttributes attributes) :
		       base (tag, attributes, true)
		{
			CheckAttributes ();
			tagType = TagType.DIRECTIVE;
		}

		private void CheckAttributes ()
		{
			Hashtable atts;
			if (!(directivesHash [tag] is Hashtable))
				throw new ApplicationException ("Unknown directive: " + tag);

			atts = (Hashtable) directivesHash [tag];
			foreach (string att in attributes.Keys){
				if (!atts.Contains (att))
					throw new ApplicationException ("Attribute " + att +
									" not valid for tag " + tag);
			}
		}

		public static bool IsDirectiveID (string id)
		{
			return directivesHash.Contains (id);
		}
		
		public override string ToString ()
		{
			return "Directive: " + tag;
		}
	}

	class ServerObjectTag : Tag
	{
		public ServerObjectTag (Tag tag) :
			base (tag.TagID, tag.Attributes, tag.SelfClosing) 
		{
			tagType = TagType.SERVEROBJECT;
			if (!attributes.IsRunAtServer ())
				throw new ApplicationException ("<object> without runat=server");
			
			if (attributes.Count != 3 || !SelfClosing || ObjectID == null || ObjectClass == null)
				throw new ApplicationException ("Incorrect syntax: <object id=\"name\" " + 
								"class=\"full.class.name\" runat=\"server\" />");
		}

		public string ObjectID
		{
			get { return (string) attributes ["id"]; }
		}
			
		public string ObjectClass
		{
			get { return (string) attributes ["class"]; }
		}
	}

	class HtmlControlTag : Tag
	{
		private Type control_type;
		private bool is_container;

		private static Hashtable controls;
		private static Hashtable inputTypes;

		private static void InitHash ()
		{
			controls = new Hashtable (new CaseInsensitiveHashCodeProvider (),
						  new CaseInsensitiveComparer ()); 

			controls.Add ("A", typeof (HtmlAnchor));
			controls.Add ("BUTTON", typeof (HtmlButton));
			controls.Add ("FORM", typeof (HtmlForm));
			controls.Add ("IMG", typeof (HtmlImage));
			controls.Add ("INPUT", "INPUT");
			controls.Add ("SELECT", typeof (HtmlSelect));
			controls.Add ("TABLE", typeof (HtmlTable));
			controls.Add ("TD", typeof (HtmlTableCell));
			controls.Add ("TH", typeof (HtmlTableCell));
			controls.Add ("TR", typeof (HtmlTableRow));
			controls.Add ("TEXTAREA", typeof (HtmlTextArea));

			inputTypes = new Hashtable (new CaseInsensitiveHashCodeProvider (),
						    new CaseInsensitiveComparer ());

			inputTypes.Add ("BUTTON", typeof (HtmlInputButton));
			inputTypes.Add ("SUBMIT", typeof (HtmlInputButton));
			inputTypes.Add ("RESET", typeof (HtmlInputButton));
			inputTypes.Add ("CHECKBOX", typeof (HtmlInputCheckBox));
			inputTypes.Add ("FILE", typeof (HtmlInputFile));
			inputTypes.Add ("HIDDEN", typeof (HtmlInputHidden));
			inputTypes.Add ("IMAGE", typeof (HtmlInputImage));
			inputTypes.Add ("RADIO", typeof (HtmlInputRadioButton));
			inputTypes.Add ("TEXT", typeof (HtmlInputText));
			inputTypes.Add ("PASSWORD", typeof (HtmlInputText));
		}
		
		static HtmlControlTag ()
		{
			InitHash ();
		}
		
		public HtmlControlTag (string tag, TagAttributes attributes, bool self_closing) : 
			base (tag, attributes, self_closing) 
		{
			SetData ();
			if (attributes == null || attributes ["ID"] == null)
				SetNewID ();
		}

		public HtmlControlTag (Tag source_tag) :
			this (source_tag.TagID, source_tag.Attributes, source_tag.SelfClosing) 
		{
		}

		private void SetData ()
		{
			tagType = TagType.HTMLCONTROL; 
			if (!(controls [tag] is string)){
				control_type = (Type) controls [tag];
				if (control_type == null)
					control_type = typeof (HtmlGenericControl);
				is_container = (0 != String.Compare (tag, "img", true));
			} else {
				string type_value = (string) attributes ["TYPE"];
				if (type_value== null)
					throw new ArgumentException ("INPUT tag without TYPE attribute!!!");

				control_type = (Type) inputTypes [type_value];
				//TODO: what does MS with this one?
				if (control_type == null)
					throw new ArgumentException ("Unknown input type -> " + type_value);
				is_container = false;
				self_closing = true; // All <input ...> are self-closing
			}
		}

		public Type ControlType
		{
			get { return control_type; }
		}

		public string ControlID
		{
			get { return (string) attributes ["ID"]; }
		}

		public bool IsContainer
		{
			get { return is_container; }
		}

		public override string ToString ()
		{
			string ret = "HtmlControlTag: " + tag + " Name: " + ControlID + "Type:" +
				     control_type.ToString () + "\n\tAttributes:\n";

			foreach (string key in attributes.Keys){
				ret += "\t" + key + "=" + attributes [key];
			}
			return ret;
		}
	}

	enum ChildrenKind
	{
		NONE,
		/* 
		 * Children must be ASP.NET server controls. Literal text is passed as LiteralControl.
		 * Child controls and text are added using AddParsedSubObject ().
		 */
		CONTROLS, 
		/*
		 * Children must correspond to properties of the parent control. No literal text allowed.
		 */
		PROPERTIES,
		/*
		 * Special case used inside <columns>...</columns>
		 * Only allow DataGridColumn and derived classes.
		 */
		DBCOLUMNS,
		/*
		 * Special case for list controls (ListBox, DropDownList...)
		 */
		LISTITEM,
		/* For HtmlSelect children. They are <option> tags that must
		 * be treated as ListItem
		 */
		OPTION
	}

	// TODO: support for ControlBuilderAttribute that may be used in custom controls
	class AspComponent : Tag
	{
		private Type type;
		private string alias;
		private string control_type;
		private bool is_close_tag;
		private bool allow_children;
		private ChildrenKind children_kind;
		private string defaultPropertyName;

		private ChildrenKind GuessChildrenKind (Type type)
		{
			object [] custom_atts = type.GetCustomAttributes (true);
			foreach (object custom_att in custom_atts){
				if (custom_att is ParseChildrenAttribute){
					/* FIXME
					 * When adding full support for custom controls, we gotta
					 * bear in mind the pca.DefaultProperty value
					 */
					ParseChildrenAttribute pca = custom_att as ParseChildrenAttribute;
					defaultPropertyName = pca.DefaultProperty;
					/* this property will be true for all controls derived from
					 * WebControls. */
					if (pca.ChildrenAsProperties == false)
						return ChildrenKind.CONTROLS;
					else if (defaultPropertyName == "")
						return ChildrenKind.PROPERTIES;
					else
						return ChildrenKind.LISTITEM;
				}
			}

			return ChildrenKind.NONE;
		}

		private static bool GuessAllowChildren (Type type)
		{
			PropertyInfo controls = type.GetProperty ("Controls");
			if (controls == null)
				return false;
			MethodInfo getm = controls.GetGetMethod ();
			object control_instance = Activator.CreateInstance (type);
			object control_collection = getm.Invoke (control_instance, null);
			return (!(control_collection is System.Web.UI.EmptyControlCollection));
		}
		
		public AspComponent (Tag input_tag, Type type) :
			base (input_tag)
		{
			tagType = TagType.SERVERCONTROL;
			this.is_close_tag = input_tag is CloseTag;
			this.type = type;
			this.defaultPropertyName = "";
			this.allow_children = GuessAllowChildren (type);
			if (input_tag.SelfClosing)
				this.children_kind = ChildrenKind.NONE;
			else if (type == typeof (System.Web.UI.WebControls.DataGridColumn) ||
				 type.IsSubclassOf (typeof (System.Web.UI.WebControls.DataGridColumn)))
				this.children_kind = ChildrenKind.PROPERTIES;
			else if (type == typeof (System.Web.UI.WebControls.ListItem))
				this.children_kind = ChildrenKind.CONTROLS;
			else
				this.children_kind = GuessChildrenKind (type);

			int pos = input_tag.TagID.IndexOf (':');
			alias = tag.Substring (0, pos);
			control_type = tag.Substring (pos + 1);
			if (attributes == null || attributes ["ID"] == null)
				SetNewID ();
		}

		public Type ComponentType
		{
			get { return type; }
		}
		
		public string ControlID
		{
			get { return (string) attributes ["ID"]; }
		}

		public bool IsCloseTag
		{
			get { return is_close_tag; }
		}

		public bool AllowChildren
		{
			get { return allow_children; }
		}

		public ChildrenKind ChildrenKind
		{
			get { return children_kind; }	
		}

		public string DefaultPropertyName
		{
			get { return defaultPropertyName; }	
		}
			
			
		public override string ToString ()
		{
			return type.ToString () + " Alias: " + alias + " ID: " + (string) attributes ["id"];
		}
	}

	class PropertyTag : Tag
	{
		private Type type;
		private string name;

		public PropertyTag (Tag tag, Type type, string name)
			: base (tag)
		{
			tagType = TagType.PROPERTYTAG;
			SetNewID ();
			this.name = name;
			this.type = type;
		}

		public Type PropertyType
		{
			get { return type; }
		}

		public string PropertyID
		{
			get { return (string) attributes ["ID"]; }
		}

		public string PropertyName
		{
			get { return name; }
		}
	}

	class CodeRenderTag : Tag
	{
		private string code;
		private bool isVarName;

		public CodeRenderTag (bool isVarName, string code) : base ("", null, false)
		{
			tagType = TagType.CODERENDER;
			this.isVarName = isVarName;
			this.code = code.Trim ();
		}

		public string Code
		{
			get { return code; }
		}

		public bool IsVarName
		{
			get { return isVarName; }
		}

		public string AsText
		{
			get { return "<%" + (IsVarName ? "=" : "") + Code + "%>"; }
		}	
	}

	class DataBindingTag : Tag
	{
		private string data;

		public DataBindingTag (string data) : base ("", null, false)
		{
			tagType = TagType.DATABINDING;
			this.data = data.Trim ();
		}

		public string Data
		{
			get { return data; }
		}

		public string AsText
		{
			get { return "<%#" + Data + "%>"; }
		}	
	}
	
	class ServerComment : Tag
	{
		public ServerComment (string tag)
			: base (ElementType.TAG)
		{
			if (tag == null)
				throw new ArgumentNullException ();

			this.tag = tag;
			this.attributes = null;
			this.tagType = TagType.SERVERCOMMENT;
			this.self_closing = true;
			this.hasDefaultID = false;
		}
		
		public override string ToString ()
		{
			return TagID;
		}

		protected override void SetNewID ()
		{
			throw new NotSupportedException ();
		}
	}
}

