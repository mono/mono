//
// Masterinfo.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Marek Safar				(marek.safar@gmail.com)
//
// (C) 2003 - 2008 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace GuiCompare {

	public class Counters
	{
		public int Present;
		public int PresentTotal;
		public int Missing;
		public int MissingTotal;
		public int Todo;
		public int TodoTotal;

		public int Extra;
		public int ExtraTotal;
		public int Warning;
		public int WarningTotal;
		public int ErrorTotal;

		public Counters ()
		{
		}

		public void AddPartialToPartial (Counters other)
		{
			Present += other.Present;
			Extra += other.Extra;
			Missing += other.Missing;

			Todo += other.Todo;
			Warning += other.Warning;
			AddPartialToTotal (other);
		}

		public void AddPartialToTotal (Counters other)
		{
			PresentTotal += other.Present;
			ExtraTotal += other.Extra;
			MissingTotal += other.Missing;

			TodoTotal += other.Todo;
			WarningTotal += other.Warning;
		}

		public void AddTotalToPartial (Counters other)
		{
			Present += other.PresentTotal;
			Extra += other.ExtraTotal;
			Missing += other.MissingTotal;

			Todo += other.TodoTotal;
			Warning += other.WarningTotal;
			AddTotalToTotal (other);
		}

		public void AddTotalToTotal (Counters other)
		{
			PresentTotal += other.PresentTotal;
			ExtraTotal += other.ExtraTotal;
			MissingTotal += other.MissingTotal;

			TodoTotal += other.TodoTotal;
			WarningTotal += other.WarningTotal;
			ErrorTotal += other.ErrorTotal;
		}

		public int Total {
			get { return Present + Missing; }
		}

		public int AbsTotal {
			get { return PresentTotal + MissingTotal; }
		}

		public int Ok {
			get { return Present - Todo; }
		}

		public int OkTotal {
			get { return PresentTotal - TodoTotal - ErrorTotal; }
		}

		public override string ToString ()
		{
			StringWriter sw = new StringWriter ();
			sw.WriteLine ("Present: {0}", Present);
			sw.WriteLine ("PresentTotal: {0}", PresentTotal);
			sw.WriteLine ("Missing: {0}", Missing);
			sw.WriteLine ("MissingTotal: {0}", MissingTotal);
			sw.WriteLine ("Todo: {0}", Todo);
			sw.WriteLine ("TodoTotal: {0}", TodoTotal);
			sw.WriteLine ("Extra: {0}", Extra);
			sw.WriteLine ("ExtraTotal: {0}", ExtraTotal);
			sw.WriteLine ("Warning: {0}", Warning);
			sw.WriteLine ("WarningTotal: {0}", WarningTotal);
			sw.WriteLine ("ErrorTotal: {0}", ErrorTotal);
			sw.WriteLine ("--");
			return sw.GetStringBuilder ().ToString ();
		}
	}

	public abstract class XMLData
	{
		protected XmlDocument document;
		protected Counters counters;
		bool haveWarnings;

		public XMLData ()
		{
			counters = new Counters ();
		}

		public virtual void LoadData (XmlNode node)
		{
		}

		protected object [] LoadRecursive (XmlNodeList nodeList, Type type)
		{
			ArrayList list = new ArrayList ();
			foreach (XmlNode node in nodeList) {
				XMLData data = (XMLData) Activator.CreateInstance (type);
				data.LoadData (node);
				list.Add (data);
			}

			return (object []) list.ToArray (type);
		}

		public static bool IsMonoTODOAttribute (string s)
		{
			if (s == null)
				return false;
			if (//s.EndsWith ("MonoTODOAttribute") ||
			    s.EndsWith ("MonoDocumentationNoteAttribute") ||
			    s.EndsWith ("MonoExtensionAttribute") ||
//			    s.EndsWith ("MonoInternalNoteAttribute") ||
			    s.EndsWith ("MonoLimitationAttribute") ||
			    s.EndsWith ("MonoNotSupportedAttribute"))
				return true;
			return s.EndsWith ("TODOAttribute");
		}

		protected void AddAttribute (XmlNode node, string name, string value)
		{
			XmlAttribute attr = document.CreateAttribute (name);
			attr.Value = value;
			node.Attributes.Append (attr);
		}
		
		public Counters Counters {
			get { return counters; }
		}
	}
	
	public abstract class XMLNameGroup : XMLData
	{
		protected XmlNode group;
		public System.Collections.Specialized.ListDictionary keys;

		public override void LoadData (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			if (node.Name != GroupName)
				throw new FormatException (String.Format ("Expecting <{0}>", GroupName));

			keys = new System.Collections.Specialized.ListDictionary ();
			foreach (XmlNode n in node.ChildNodes) {
				string name = n.Attributes ["name"].Value;
				if (CheckIfAdd (name, n)) {
					string key = GetNodeKey (name, n);
					//keys.Add (key, name);
					keys [key] = name;
					LoadExtraData (key, n);
				}
			}
		}

		protected virtual bool CheckIfAdd (string value, XmlNode node)
		{
			return true;
		}

		protected virtual void LoadExtraData (string name, XmlNode node)
		{
		}

		public virtual string GetNodeKey (string name, XmlNode node)
		{
			return name;
		}

		public virtual bool HasKey (string key, Hashtable other)
		{
			return other.ContainsKey (key);
		}

		public abstract string GroupName { get; }
		public abstract string Name { get; }
	}

	public class XMLAssembly : XMLData
	{
		public XMLAttributes attributes;
		public XMLNamespace [] namespaces;
		string name;
		string version;

		public static XMLAssembly CreateFromFile (string file)
		{
			XmlDocument doc = new XmlDocument ();
			doc.Load (File.OpenRead (file));

			XmlNode node = doc.SelectSingleNode ("/assemblies/assembly");
			if (node != null) {
				XMLAssembly result = new XMLAssembly ();
				try {
					result.LoadData (node);
				} catch (Exception e) {
					Console.Error.WriteLine ("Error loading {0}: {1}\n{2}", file, e.Message, e);
					return null;
				}
				return result;
			}

			return null;
		}

		public override void LoadData (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			name = node.Attributes ["name"].Value;
			version = node.Attributes  ["version"].Value;
			XmlNode atts = node.FirstChild;
			attributes = new XMLAttributes ();
			if (atts.Name == "attributes") {
				attributes.LoadData (atts);
				atts = atts.NextSibling;
			}

			if (atts == null || atts.Name != "namespaces") {
				Console.Error.WriteLine ("Warning: no namespaces found!");
				return;
			}

			namespaces = (XMLNamespace []) LoadRecursive (atts.ChildNodes, typeof (XMLNamespace));
		}

		static Hashtable CreateHash (XMLNamespace [] other)
		{
			Hashtable result = new Hashtable ();
			if (other != null) {
				int i = 0;
				foreach (XMLNamespace n in other) {
					result [n.Name] = i++;
				}
			}

			return result;
		}

	}

	public class XMLNamespace : XMLData
	{
		public string name;
		public XMLClass [] types;

		public override void LoadData (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			if (node.Name != "namespace")
				throw new FormatException ("Expecting <namespace>");

			name = node.Attributes  ["name"].Value;
			XmlNode classes = node.FirstChild;
			if (classes == null) {
				Console.Error.WriteLine ("Warning: no classes for {0}", node.Attributes  ["name"]);
				return;
			}

			if (classes.Name != "classes")
				throw new FormatException ("Expecting <classes>. Got <" + classes.Name + ">");

			types = (XMLClass []) LoadRecursive (classes.ChildNodes, typeof (XMLClass));
		}

		static Hashtable CreateHash (XMLClass [] other)
		{
			Hashtable result = new Hashtable ();
			if (other != null) {
				int i = 0;
				foreach (XMLClass c in other) {
					result [c.Name] = i++;
				}
			}

			return result;
		}

		public string Name {
			get { return name; }
		}
	}

	public class XMLClass : XMLData
	{
		public string name;
		public string type;
		public string baseName;
		public bool isSealed;
		bool isSerializable;
		public bool isAbstract;
		string charSet;
		string layout;
		public XMLAttributes attributes;
		public XMLInterfaces interfaces;
		XMLGenericParameters genericParameters;
		public XMLFields fields;
		public XMLConstructors constructors;
		public XMLProperties properties;
		public XMLEvents events;
		public XMLMethods methods;
		public XMLClass [] nested;
		
		public override void LoadData (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			name = node.Attributes ["name"].Value;
			type = node.Attributes  ["type"].Value;
			XmlAttribute xatt = node.Attributes ["base"];
			if (xatt != null)
				baseName = xatt.Value;

			xatt = node.Attributes ["sealed"];
			isSealed = (xatt != null && xatt.Value == "true");

			xatt = node.Attributes ["abstract"];
			isAbstract = (xatt != null && xatt.Value == "true");

			xatt = node.Attributes["serializable"];
			isSerializable = (xatt != null && xatt.Value == "true");

			xatt = node.Attributes["charset"];
			if (xatt != null)
				charSet = xatt.Value;

			xatt = node.Attributes["layout"];
			if (xatt != null)
				layout = xatt.Value;

			XmlNode child = node.FirstChild;
			if (child == null) {
				// Console.Error.WriteLine ("Empty class {0} {1}", name, type);
				return;
			}
				
			if (child.Name == "attributes") {
				attributes = new XMLAttributes ();
				attributes.LoadData (child);
				child = child.NextSibling;
			}

			if (child != null && child.Name == "interfaces") {
				interfaces = new XMLInterfaces ();
				interfaces.LoadData (child);
				child = child.NextSibling;
			}

			if (child != null && child.Name == "generic-parameters") {
				genericParameters = new XMLGenericParameters ();
				genericParameters.LoadData (child);
				child = child.NextSibling;
			}

			if (child != null && child.Name == "fields") {
				fields = new XMLFields ();
				fields.LoadData (child);
				child = child.NextSibling;
			}

			if (child != null && child.Name == "constructors") {
				constructors = new XMLConstructors ();
				constructors.LoadData (child);
				child = child.NextSibling;
			}

			if (child != null && child.Name == "properties") {
				properties = new XMLProperties ();
				properties.LoadData (child);
				child = child.NextSibling;
			}

			if (child != null && child.Name == "events") {
				events = new XMLEvents ();
				events.LoadData (child);
				child = child.NextSibling;
			}

			if (child != null && child.Name == "methods") {
				methods = new XMLMethods ();
				methods.LoadData (child);
				child = child.NextSibling;
			}

			if (child == null)
				return;

			if (child.Name != "classes") {
				Console.WriteLine ("name: {0} type: {1} {2}", name, type, child.NodeType);
				throw new FormatException ("Expecting <classes>. Got <" + child.Name + ">");
			}

			nested = (XMLClass []) LoadRecursive (child.ChildNodes, typeof (XMLClass));
		}

		static Hashtable CreateHash (XMLClass [] other)
		{
			Hashtable result = new Hashtable ();
			if (other != null) {
				int i = 0;
				foreach (XMLClass c in other) {
					result [c.Name] = i++;
				}
			}

			return result;
		}
		
		public List<CompGenericParameter> GetTypeParameters ()
		{
			return MasterUtils.GetTypeParameters (genericParameters);
		}

		public string Name {
			get { return name; }
		}

		public string Type {
			get { return type; }
		}
	}

	public class XMLParameter : XMLData
	{
		public string name;
		public string type;
		public string attrib;
		public string direction;
		public bool isUnsafe;
		public bool isOptional;
		public string defaultValue;
		public XMLAttributes attributes;

		public override void LoadData (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			if (node.Name != "parameter")
				throw new ArgumentException ("Expecting <parameter>");

			name = node.Attributes["name"].Value;
			type = node.Attributes["type"].Value;
			attrib = node.Attributes["attrib"].Value;
			if (node.Attributes ["direction"] != null)
				direction = node.Attributes["direction"].Value;
			if (node.Attributes["unsafe"] != null)
				isUnsafe = bool.Parse (node.Attributes["unsafe"].Value);
			if (node.Attributes["optional"] != null)
				isOptional = bool.Parse (node.Attributes["optional"].Value);
			if (node.Attributes["defaultValue"] != null)
				defaultValue = node.Attributes["defaultValue"].Value;

			XmlNode child = node.FirstChild;
			if (child == null)
				return;

			if (child.Name == "attributes") {
				attributes = new XMLAttributes ();
				attributes.LoadData (child);
				child = child.NextSibling;
			}
		}

		public string Name {
			get { return name; }
		}
	}

	public class XMLAttributeProperties: XMLNameGroup
	{
		static Dictionary <string, string> ignored_properties;
		SortedDictionary <string, string> properties;

		static XMLAttributeProperties ()
		{

			ignored_properties = new Dictionary <string, string> ();
			ignored_properties.Add ("System.Reflection.AssemblyKeyFileAttribute", "KeyFile");
			ignored_properties.Add ("System.Reflection.AssemblyCompanyAttribute", "Company");
			ignored_properties.Add ("System.Reflection.AssemblyConfigurationAttribute", "Configuration");
			ignored_properties.Add ("System.Reflection.AssemblyCopyrightAttribute", "Copyright");
			ignored_properties.Add ("System.Reflection.AssemblyProductAttribute", "Product");
			ignored_properties.Add ("System.Reflection.AssemblyTrademarkAttribute", "Trademark");
			ignored_properties.Add ("System.Reflection.AssemblyInformationalVersionAttribute", "InformationalVersion");

			ignored_properties.Add ("System.ObsoleteAttribute", "Message");
			ignored_properties.Add ("System.IO.IODescriptionAttribute", "Description");
			ignored_properties.Add ("System.Diagnostics.MonitoringDescriptionAttribute", "Description");
		}

		string attribute;

		public XMLAttributeProperties ()
			: this (null)
		{}

		public XMLAttributeProperties (string attribute)
		{
			this.attribute = attribute;
		}

		public override void LoadData (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			if (node.ChildNodes == null)
				return;

			string ignored;

			if (!ignored_properties.TryGetValue (attribute, out ignored))
				ignored = null;

			foreach (XmlNode n in node.ChildNodes) {
				string name = n.Attributes["name"].Value;
				if (ignored != null && ignored == name)
					continue;

				if (n.Attributes["null"] != null) {
					Properties.Add (name, null);
					continue;
				}
				Properties.Add (name, n.Attributes ["value"].Value);
			}
		}

		public IDictionary <string, string> Properties {
			get {
				if (properties == null)
					properties = new SortedDictionary <string, string> ();
				return properties;
			}
		}

		public override string GroupName {
			get {
				return "properties";
			}
		}

		public override string Name {
			get {
				return "";
			}
		}
	}

	public class XMLAttributes : XMLNameGroup
	{
		bool isTodo;
		string comment;
		SortedDictionary <string, XMLAttributeProperties> properties;

		protected override bool CheckIfAdd (string value, XmlNode node)
		{
			if (IsMonoTODOAttribute (value)) {
				isTodo = true;

				XmlNode pNode = node.SelectSingleNode ("properties");
				if (pNode != null && pNode.ChildNodes.Count > 0 && pNode.ChildNodes [0].Attributes ["value"] != null) {
					comment = pNode.ChildNodes [0].Attributes ["value"].Value;
				}
				return false;
			}
			
			if (MasterUtils.IsImplementationSpecificAttribute (value))
				return false;

			return true;
		}

		public override string GetNodeKey (string name, XmlNode node)
		{
			string key = null;

			// if multiple attributes with the same name (type) exist, then we 
			// cannot be sure which attributes correspond, so we must use the
			// name of the attribute (type) and the name/value of its properties
			// as key

			XmlNodeList attributes = node.ParentNode.SelectNodes("attribute[@name='" + name + "']");
			if (attributes.Count > 1) {
				ArrayList keyParts = new ArrayList ();

				XmlNodeList properties = node.SelectNodes ("properties/property");
				foreach (XmlNode property in properties) {
					XmlAttributeCollection attrs = property.Attributes;
					if (attrs["value"] != null) {
						keyParts.Add (attrs["name"].Value + "=" + attrs["value"].Value);
					} else {
						keyParts.Add (attrs["name"].Value + "=");
					}
				}

				// sort properties by name, as order of properties in XML is 
				// undefined
				keyParts.Sort ();

				// insert name (type) of attribute
				keyParts.Insert (0, name);

				StringBuilder sb = new StringBuilder ();
				foreach (string value in keyParts) {
					sb.Append (value);
					sb.Append (';');
				}
				key = sb.ToString ();
			} else {
				key = name;
			}

			return key;
		}

		protected override void LoadExtraData (string name, XmlNode node)
		{
			XmlNode pNode = node.SelectSingleNode ("properties");

			if (IsMonoTODOAttribute (name)) {
				isTodo = true;
				if (pNode.ChildNodes [0].Attributes ["value"] != null) {
					comment = pNode.ChildNodes [0].Attributes ["value"].Value;
				}
				return;
			}
			
			if (MasterUtils.IsImplementationSpecificAttribute (name))
				return;

			if (pNode != null) {
				XMLAttributeProperties p = new XMLAttributeProperties (name);
				p.LoadData (pNode);

				IDictionary <string, XMLAttributeProperties> properties = Properties;
				if (properties.ContainsKey (name))
					properties [name] = p;
				else
					properties.Add (name, p);
			}
		}

		public IDictionary <string, XMLAttributeProperties> Properties {
			get {
				if (properties == null)
					properties = new SortedDictionary <string, XMLAttributeProperties> ();
				return properties;
			}
		}

		public override string GroupName {
			get { return "attributes"; }
		}

		public override string Name {
			get { return "attribute"; }
		}

		public bool IsTodo {
			get { return isTodo; }
		}

		public string Comment {
			get { return comment; }
		}
	}

	public class XMLInterfaces : XMLNameGroup
	{
		public override string GroupName {
			get { return "interfaces"; }
		}

		public override string Name {
			get { return "interface"; }
		}
	}

	public class XMLGenericParameters : XMLMember
	{
		public Dictionary<string, XMLGenericParameterConstraints> constraints = new Dictionary<string, XMLGenericParameterConstraints> ();
		
		public override string GroupName {
			get { return "generic-parameters"; }
		}

		public override string Name {
			get { return "generic-parameter"; }
		}

		protected override void LoadExtraData (string name, XmlNode node)
		{
			var attributes = ((XmlElement) node).GetAttribute ("attributes");
			var xml_constraints = new XMLGenericParameterConstraints (attributes);
			constraints.Add (name, xml_constraints);

			XmlNode orig = node;

			var child = node.FirstChild;
			if (child != null && child.Name == "generic-parameter-constraints") {
				xml_constraints.LoadData (child);
			}
			
			base.LoadExtraData (name, orig);
		}
	}

	public class XMLGenericParameterConstraints : XMLNameGroup
	{
		public string attributes;
		
		public XMLGenericParameterConstraints (string attributes)
		{
			this.attributes = attributes;
		}
		
		public override string GroupName {
			get { return "generic-parameter-constraints"; }
		}

		public override string Name {
			get { return "generic-parameter-constraint"; }
		}
	}

	public abstract class XMLMember : XMLNameGroup
	{
		public Hashtable attributeMap;
		public Hashtable access = new Hashtable ();

		protected override void LoadExtraData (string name, XmlNode node)
		{
			XmlAttribute xatt = node.Attributes ["attrib"];
			if (xatt != null)
				access [name] = xatt.Value;
			
			XmlNode orig = node;

			node = node.FirstChild;
			while (node != null) {
				if (node != null && node.Name == "attributes") {
					XMLAttributes a = new XMLAttributes ();
					a.LoadData (node);
					if (attributeMap == null)
						attributeMap = new Hashtable ();

					attributeMap [name] = a;
					break;
				}
				node = node.NextSibling;
			}

			base.LoadExtraData (name, orig);
		}

		public virtual string ConvertToString (int att)
		{
			return null;
		}
	}
	
	public class XMLFields : XMLMember
	{
		public Hashtable fieldTypes;
		public Hashtable fieldValues;

		protected override void LoadExtraData (string name, XmlNode node)
		{
			XmlAttribute xatt = node.Attributes ["fieldtype"];
			if (xatt != null) {
				if (fieldTypes == null)
					fieldTypes = new Hashtable ();

				fieldTypes [name] = xatt.Value;
			}

			xatt = node.Attributes ["value"];
			if (xatt != null) {
				if (fieldValues == null)
					fieldValues = new Hashtable ();

				fieldValues[name] = xatt.Value;
			}

			base.LoadExtraData (name, node);
		}

		public override string ConvertToString (int att)
		{
			FieldAttributes fa = (FieldAttributes) att;
			return fa.ToString ();
		}

		public override string GroupName {
			get { return "fields"; }
		}

		public override string Name {
			get { return "field"; }
		}
	}

	public class XMLParameters : XMLNameGroup
	{
		public override void LoadData (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			if (node.Name != GroupName)
				throw new FormatException (String.Format ("Expecting <{0}>", GroupName));

			keys = new System.Collections.Specialized.ListDictionary ();
			foreach (XmlNode n in node.ChildNodes) {
				string name = n.Attributes["name"].Value;
				string key = GetNodeKey (name, n);
				XMLParameter parm = new XMLParameter ();
				parm.LoadData (n);
				keys.Add (key, parm);
				LoadExtraData (key, n);
			}
		}

		public override string GroupName {
			get {
				return "parameters";
			}
		}

		public override string Name {
			get {
				return "parameter";
			}
		}

		public override string GetNodeKey (string name, XmlNode node)
		{
			return node.Attributes["position"].Value;
		}
	}

	public class XMLProperties : XMLMember
	{
		public Hashtable nameToMethod = new Hashtable ();

		protected override void LoadExtraData (string name, XmlNode node)
		{
			XmlNode orig = node;
			node = node.FirstChild;
			while (node != null) {
				if (node != null && node.Name == "methods") {
					XMLMethods m = new XMLMethods ();
					XmlNode parent = node.ParentNode;
					string key = GetNodeKey (name, parent);
					m.LoadData (node);
					nameToMethod [key] = m;
					break;
				}
				node = node.NextSibling;
			}


			base.LoadExtraData (name, orig);
		}

		public override string GetNodeKey (string name, XmlNode node)
		{
			XmlAttributeCollection atts = node.Attributes;
			return String.Format ("{0}:{1}:{2}",
					      atts ["name"].Value,
					      atts ["ptype"].Value,
					      atts ["params"] == null ? "" : atts ["params"].Value);
		}

		public override string GroupName {
			get { return "properties"; }
		}

		public override string Name {
			get { return "property"; }
		}
	}

	public class XMLEvents : XMLMember
	{
		public Hashtable eventTypes;

		protected override void LoadExtraData (string name, XmlNode node)
		{
			XmlAttribute xatt = node.Attributes ["eventtype"];
			if (xatt != null) {
				if (eventTypes == null)
					eventTypes = new Hashtable ();

				eventTypes [name] = xatt.Value;
			}

			base.LoadExtraData (name, node);
		}

		public override string ConvertToString (int att)
		{
			EventAttributes ea = (EventAttributes) att;
			return ea.ToString ();
		}

		public override string GroupName {
			get { return "events"; }
		}

		public override string Name {
			get { return "event"; }
		}
	}

	public class XMLMethods : XMLMember
	{
		public Hashtable returnTypes;
		public Hashtable parameters;
		public Hashtable genericParameters;
		public Hashtable signatureFlags;

		[Flags]
		public enum SignatureFlags
		{
			None = 0,
			Abstract = 1,
			Virtual = 2,
			Static = 4,
			Final = 8,
		}

		protected override void LoadExtraData (string name, XmlNode node)
		{
			XmlAttribute xatt = node.Attributes ["returntype"];
			if (xatt != null) {
				if (returnTypes == null)
					returnTypes = new Hashtable ();

				returnTypes [name] = xatt.Value;
			}

			SignatureFlags flags = SignatureFlags.None;
			if (((XmlElement) node).GetAttribute ("abstract") == "true")
				flags |= SignatureFlags.Abstract;
			if (((XmlElement) node).GetAttribute ("static") == "true")
				flags |= SignatureFlags.Static;
			if (((XmlElement) node).GetAttribute ("virtual") == "true")
				flags |= SignatureFlags.Virtual;
			if (((XmlElement) node).GetAttribute ("final") == "true")
				flags |= SignatureFlags.Final;
			if (flags != SignatureFlags.None) {
				if (signatureFlags == null)
					signatureFlags = new Hashtable ();
				signatureFlags [name] = flags;
			}

			XmlNode parametersNode = node.SelectSingleNode ("parameters");
			if (parametersNode != null) {
				if (parameters == null)
					parameters = new Hashtable ();

				XMLParameters parms = new XMLParameters ();
				parms.LoadData (parametersNode);

				parameters[name] = parms;
			}

			XmlNode genericNode = node.SelectSingleNode ("generic-parameters");
			if (genericNode != null) {
				if (genericParameters == null)
					genericParameters = new Hashtable ();

				XMLGenericParameters gparams = new XMLGenericParameters ();
				gparams.LoadData (genericNode);
				genericParameters [name] = gparams;
			}

			base.LoadExtraData (name, node);
		}

		public override string GetNodeKey (string name, XmlNode node)
		{
			// for explicit/implicit operators we need to include the return
			// type in the key to allow matching; as a side-effect, differences
			// in return types will be reported as extra/missing methods
			//
			// for regular methods we do not need to take into account the
			// return type for matching methods; differences in return types
			// will be reported as a warning on the method
			if (name.StartsWith ("op_")) {
				XmlAttribute xatt = node.Attributes ["returntype"];
				string returnType = xatt != null ? xatt.Value + " " : string.Empty;
				return returnType + name;
			}
			return name;
		}

		public override string ConvertToString (int att)
		{
			MethodAttributes ma = (MethodAttributes) att;
			// ignore ReservedMasks
			ma &= ~ MethodAttributes.ReservedMask;
			ma &= ~ MethodAttributes.VtableLayoutMask;
			if ((ma & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem)
				ma = (ma & ~ MethodAttributes.FamORAssem) | MethodAttributes.Family;

			// ignore the HasSecurity attribute for now
			if ((ma & MethodAttributes.HasSecurity) != 0)
				ma = (MethodAttributes) (att - (int) MethodAttributes.HasSecurity);

			// ignore the RequireSecObject attribute for now
			if ((ma & MethodAttributes.RequireSecObject) != 0)
				ma = (MethodAttributes) (att - (int) MethodAttributes.RequireSecObject);

			// we don't care if the implementation is forwarded through PInvoke 
			if ((ma & MethodAttributes.PinvokeImpl) != 0)
				ma = (MethodAttributes) (att - (int) MethodAttributes.PinvokeImpl);

			return ma.ToString ();
		}

		public override string GroupName {
			get { return "methods"; }
		}

		public override string Name {
			get { return "method"; }
		}
	}

	public class XMLConstructors : XMLMethods
	{
		public override string GroupName {
			get { return "constructors"; }
		}

		public override string Name {
			get { return "constructor"; }
		}
	}

	public class XmlNodeComparer : IComparer
	{
		public static XmlNodeComparer Default = new XmlNodeComparer ();

		public int Compare (object a, object b)
		{
			XmlNode na = (XmlNode) a;
			XmlNode nb = (XmlNode) b;
			return String.Compare (na.Attributes ["name"].Value, nb.Attributes ["name"].Value);
		}
	}
}
