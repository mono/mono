//
// acompare.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Mono.AssemblyCompare
{
	class Driver
	{
		static int Main (string [] args)
		{
			if (args.Length != 2)
				return 1;

			XMLAssembly ms = CreateXMLAssembly (args [0]);
			XMLAssembly mono = CreateXMLAssembly (args [1]);
			XmlDocument doc = ms.CompareAndGetDocument (mono);

			XmlTextWriter writer = new XmlTextWriter (Console.Out);
			writer.Formatting = Formatting.Indented;
			doc.WriteTo (writer);

			return 0;
		}

		static XMLAssembly CreateXMLAssembly (string file)
		{
			XmlDocument doc = new XmlDocument ();
			doc.Load (File.OpenRead (file));

			XmlNode node = doc.SelectSingleNode ("/assemblies/assembly");
			XMLAssembly result = new XMLAssembly ();
			try {
				result.LoadData (node);
			} catch (Exception e) {
				Console.WriteLine ("Error loading {0}: {1}\n{2}", file, e.Message, e);
				Environment.Exit (1);
			}

			return result;
		}
	}

	class Counters
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
		string name;

		public Counters (string name)
		{
			this.name = name;
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
			sw.WriteLine ("name: {0}", name);
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

	abstract class XMLData
	{
		protected XmlDocument document;
		protected Counters counters;
		bool haveWarnings;

		public XMLData ()
		{
			counters = new Counters (GetType ().ToString ());
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

		protected void AddAttribute (XmlNode node, string name, string value)
		{
			XmlAttribute attr = document.CreateAttribute (name);
			attr.Value = value;
			node.Attributes.Append (attr);
		}

		public void AddCountersAttributes (XmlNode node)
		{
  			if (counters.Missing > 0)
				AddAttribute (node, "missing", counters.Missing.ToString ());

  			if (counters.Present > 0)
				AddAttribute (node, "present", counters.Present.ToString ());

  			if (counters.Extra > 0)
				AddAttribute (node, "extra", counters.Extra.ToString ());

  			if (counters.Ok > 0)
				AddAttribute (node, "ok", counters.Ok.ToString ());

  			if (counters.Total > 0) {
				int percent = (100 * counters.Present / counters.Total);
				AddAttribute (node, "complete", percent.ToString ());
			}

  			if (counters.Todo > 0)
				AddAttribute (node, "todo", counters.Todo.ToString ());

  			if (counters.Warning > 0)
				AddAttribute (node, "warning", counters.Warning.ToString ());

  			if (counters.MissingTotal > 0)
				AddAttribute (node, "missing_total", counters.MissingTotal.ToString ());

  			if (counters.PresentTotal > 0)
				AddAttribute (node, "present_total", counters.PresentTotal.ToString ());

  			if (counters.ExtraTotal > 0)
				AddAttribute (node, "extra_total", counters.ExtraTotal.ToString ());

  			if (counters.OkTotal > 0)
				AddAttribute (node, "ok_total", counters.OkTotal.ToString ());

  			if (counters.AbsTotal > 0) {
				int percent = (100 * counters.PresentTotal / counters.AbsTotal);
				AddAttribute (node, "complete_total", percent.ToString ());
			}

  			if (counters.TodoTotal > 0) {
				AddAttribute (node, "todo_total", counters.TodoTotal.ToString ());
				//TODO: should be different on error. check error cases in corcompare.
				AddAttribute (node, "error_total", counters.Todo.ToString ());
			}

  			if (counters.WarningTotal > 0)
				AddAttribute (node, "warning_total", counters.WarningTotal.ToString ());
		}

		protected void AddWarning (XmlNode parent, string fmt, params object [] args)
		{
			counters.Warning++;
			haveWarnings = true;
			XmlNode warnings = parent.SelectSingleNode ("warnings");
			if (warnings == null) {
				warnings = document.CreateElement ("warnings", null);
				parent.AppendChild (warnings);
			}

			AddAttribute (parent, "error", "warning");
			XmlNode warning = document.CreateElement ("warnings", null);
			AddAttribute (warning, "text", String.Format (fmt, args));
			warnings.AppendChild (warning);
		}

		public bool HaveWarnings {
			get { return haveWarnings; }
		}
		
		public Counters Counters {
			get { return counters; }
		}
		
		public abstract void CompareTo (XmlDocument doc, XmlNode parent, object other);
	}
	
	abstract class XMLNameGroup : XMLData
	{
		protected ArrayList data;
		protected XmlNode group;

		public override void LoadData (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			if (node.Name != GroupName)
				throw new FormatException (String.Format ("Expecting <{0}>", GroupName));

			data = new ArrayList ();
			foreach (XmlNode n in node.ChildNodes) {
				string val = n.Attributes ["name"].Value;
				if (CheckIfAdd (val))
					data.Add (val);

				if (n.HasChildNodes)
					LoadExtraData (val, n.FirstChild);
			}
		}

		protected virtual bool CheckIfAdd (string value)
		{
			return true;
		}

		protected virtual void LoadExtraData (string name, XmlNode node)
		{
		}

		public override void CompareTo (XmlDocument doc, XmlNode parent, object other)
		{
			this.document = doc;
			if (group == null)
				group = doc.CreateElement (GroupName, null);

			ArrayList odata = null;
			if (other == null) {
				odata = new ArrayList (1);
			} else {
				odata = ((XMLNameGroup) other).data;
			}

			int count = (data == null) ? 0 : data.Count;
			XmlNode node = null;
			for (int i = 0; i < count; i++) {
				string name = data [i] as string;
				node = doc.CreateElement (Name, null);
				group.AppendChild (node);
				AddAttribute (node, "name", name);

				int index = odata.IndexOf (name);
				if (index == -1) {
					AddAttribute (node, "presence", "missing");
					counters.Missing++;
				} else {
					CompareToInner (name, node, (XMLNameGroup) other);
					odata.RemoveAt (index);
					counters.Present++;
				}
			}

			count = odata.Count;
			for (int i = 0; i < count; i++) {
				node = doc.CreateElement (Name, null);
				AddAttribute (node, "name", (string) odata [i]);
				AddAttribute (node, "presence", "extra");
				group.AppendChild (node);
				counters.Extra++;
			}

			if (group.HasChildNodes)
				parent.AppendChild (group);
		}

		protected virtual void CompareToInner (string name, XmlNode node, XMLNameGroup other)
		{
		}

		public abstract string GroupName { get; }
		public abstract string Name { get; }
	}

	class XMLAssembly : XMLData
	{
		XMLAttributes attributes;
		XMLNamespace [] namespaces;
		string name;
		string version;

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

		public override void CompareTo (XmlDocument doc, XmlNode parent, object other)
		{
			XMLAssembly assembly = (XMLAssembly) other;

			XmlNode childA = doc.CreateElement ("assembly", null);
			AddAttribute (childA, "name", name);
			AddAttribute (childA, "version", version);
			if (name != assembly.name)
				AddWarning (childA, "Assembly names not equal: {0}, {1}", name, assembly.name);

			if (version != assembly.version)
				AddWarning (childA, "Assembly version not equal: {0}, {1}", version, assembly.version);

			parent.AppendChild (childA);

			attributes.CompareTo (doc, childA, assembly.attributes);
			counters.AddPartialToPartial (attributes.Counters);

			CompareNamespaces (childA, assembly.namespaces);
			if (assembly.attributes != null && assembly.attributes.IsTodo) {
				counters.Todo++;
				counters.ErrorTotal++;
				AddAttribute (childA, "error", "todo");
			}

			AddCountersAttributes (childA);
		}

		void CompareNamespaces (XmlNode parent, XMLNamespace [] other)
		{
			ArrayList newNS = new ArrayList ();
			XmlNode group = document.CreateElement ("namespaces", null);
			parent.AppendChild (group);

			Hashtable oh = CreateHash (other);
			XmlNode node = null;
			int count = (namespaces == null) ? 0 : namespaces.Length;
			for (int i = 0; i < count; i++) {
				XMLNamespace xns = namespaces [i];

				node = document.CreateElement ("namespace", null);
				newNS.Add (node);
				AddAttribute (node, "name", xns.Name);

				if (oh.ContainsKey (xns.Name)) {
					int idx = (int) oh [xns.Name];
					xns.CompareTo (document, node, other [idx]);
					other [idx] = null;
					xns.AddCountersAttributes (node);
					counters.Present++;
					counters.PresentTotal++;
					counters.AddPartialToTotal (xns.Counters);
				} else {
					AddAttribute (node, "presence", "missing");
					counters.Missing++;
					counters.MissingTotal++;
				}
			}

			if (other != null) {
				count = other.Length;
				for (int i = 0; i < count; i++) {
					XMLNamespace n = other [i];
					if (n == null)
						continue;

					node = document.CreateElement ("namespace", null);
					newNS.Add (node);
					AddAttribute (node, "name", n.Name);
					AddAttribute (node, "presence", "extra");
					counters.ExtraTotal++;
				}
			}

			XmlNode [] nodes = (XmlNode []) newNS.ToArray (typeof (XmlNode));
			Array.Sort (nodes, XmlNodeComparer.Default);
			foreach (XmlNode nn in nodes)
				group.AppendChild (nn);
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

		public XmlDocument CompareAndGetDocument (XMLAssembly other)
		{
			XmlDocument doc = new XmlDocument ();
			this.document = doc;
			XmlNode parent = doc.CreateElement ("assemblies", null);
			doc.AppendChild (parent);
			
			CompareTo (doc, parent, other);

			XmlNode decl = doc.CreateXmlDeclaration ("1.0", null, null);
			doc.InsertBefore (decl, doc.DocumentElement);

			return doc;
		}
	}

	class XMLNamespace : XMLData
	{
		string name;
		XMLClass [] types;

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

		public override void CompareTo (XmlDocument doc, XmlNode parent, object other)
		{
			this.document = doc;
			XMLNamespace nspace = (XMLNamespace) other;

			XmlNode childA = doc.CreateElement ("classes", null);
			parent.AppendChild (childA);

			CompareTypes (childA, nspace.types);
		}

		void CompareTypes (XmlNode parent, XMLClass [] other)
		{
			ArrayList newNodes = new ArrayList ();
			Hashtable oh = CreateHash (other);
			XmlNode node = null;
			int count = (types == null) ? 0 : types.Length;
			for (int i = 0; i < count; i++) {
				XMLClass xclass = types [i];

				node = document.CreateElement ("class", null);
				newNodes.Add (node);
				AddAttribute (node, "name", xclass.Name);
				AddAttribute (node, "type", xclass.Type);

				if (oh.ContainsKey (xclass.Name)) {
					int idx = (int) oh [xclass.Name];
					xclass.CompareTo (document, node, other [idx]);
					other [idx] = null;
					counters.AddPartialToPartial (xclass.Counters);
				} else {
					AddAttribute (node, "presence", "missing");
					counters.Missing++;
					counters.MissingTotal++;
				}
			}

			if (other != null) {
				count = other.Length;
				for (int i = 0; i < count; i++) {
					XMLClass c = other [i];
					if (c == null || c.Name == "MonoTODOAttribute")
						continue;

					node = document.CreateElement ("class", null);
					newNodes.Add (node);
					AddAttribute (node, "name", c.Name);
					AddAttribute (node, "presence", "extra");
					counters.Extra++;
					counters.ExtraTotal++;
				}
			}

			XmlNode [] nodes = (XmlNode []) newNodes.ToArray (typeof (XmlNode));
			Array.Sort (nodes, XmlNodeComparer.Default);
			foreach (XmlNode nn in nodes)
				parent.AppendChild (nn);
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

	class XMLClass : XMLData
	{
		string name;
		string type;
		string baseName;
		bool isSealed;
		XMLAttributes attributes;
		XMLInterfaces interfaces;
		XMLFields fields;
		XMLConstructors constructors;
		XMLProperties properties;
		XMLEvents events;
		XMLMethods methods;
		XMLClass [] nested;
		
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

		public override void CompareTo (XmlDocument doc, XmlNode parent, object other)
		{
			this.document = doc;
			XMLClass oclass = (XMLClass) other;

			if (attributes != null || oclass.attributes != null) {
				if (attributes == null)
					attributes = new XMLAttributes ();

				attributes.CompareTo (doc, parent, oclass.attributes);
				counters.AddPartialToPartial (attributes.Counters);
				if (oclass.attributes != null && oclass.attributes.IsTodo) {
					counters.Todo++;
					counters.TodoTotal++;
					counters.ErrorTotal++;
					AddAttribute (parent, "error", "todo");
				}
			}

			if (baseName != oclass.baseName)
				AddWarning (parent, "Base class is wrong: {0} != {1}", baseName, oclass.baseName);

			if (isSealed != oclass.isSealed)
				AddWarning (parent, "Should {0}be sealed", isSealed ? "" : "not ");

			if (interfaces != null || oclass.interfaces != null) {
				if (interfaces == null)
					interfaces = new XMLInterfaces ();

				interfaces.CompareTo (doc, parent, oclass.interfaces);
				counters.AddPartialToPartial (interfaces.Counters);
			}

			if (fields != null || oclass.fields != null) {
				if (fields == null)
					fields = new XMLFields ();

				fields.CompareTo (doc, parent, oclass.fields);
				counters.AddPartialToPartial (fields.Counters);
			}

			if (constructors != null || oclass.constructors != null) {
				if (constructors == null)
					constructors = new XMLConstructors ();

				constructors.CompareTo (doc, parent, oclass.constructors);
				counters.AddPartialToPartial (constructors.Counters);
			}

			if (properties != null || oclass.properties != null) {
				if (properties == null)
					properties = new XMLProperties ();

				properties.CompareTo (doc, parent, oclass.properties);
				counters.AddPartialToPartial (properties.Counters);
			}

			if (events != null || oclass.events != null) {
				if (events == null)
					events = new XMLEvents ();

				events.CompareTo (doc, parent, oclass.events);
				counters.AddPartialToPartial (events.Counters);
			}

			if (methods != null || oclass.methods != null) {
				if (methods == null)
					methods = new XMLMethods ();

				methods.CompareTo (doc, parent, oclass.methods);
				counters.AddPartialToPartial (methods.Counters);
			}

			if (nested != null || oclass.nested != null) {
				CompareTypes (parent, oclass.nested);
			}

			AddCountersAttributes (parent);
		}

		void CompareTypes (XmlNode parent, XMLClass [] other)
		{
			ArrayList newNodes = new ArrayList ();
			Hashtable oh = CreateHash (other);
			XmlNode node = null;
			int count = (nested == null) ? 0 : nested.Length;
			for (int i = 0; i < count; i++) {
				XMLClass xclass = nested [i];

				node = document.CreateElement ("class", null);
				newNodes.Add (node);
				AddAttribute (node, "name", xclass.Name);
				AddAttribute (node, "type", xclass.Type);

				if (oh.ContainsKey (xclass.Name)) {
					int idx = (int) oh [xclass.Name];
					xclass.CompareTo (document, node, other [idx]);
					other [idx] = null;
					counters.AddPartialToPartial (xclass.Counters);
				} else {
					// TODO: Should I count here?
					AddAttribute (node, "presence", "missing");
					counters.Missing++;
					counters.MissingTotal++;
				}
			}

			if (other != null) {
				count = other.Length;
				for (int i = 0; i < count; i++) {
					XMLClass c = other [i];
					if (c == null || c.Name == "MonoTODOAttribute")
						continue;

					node = document.CreateElement ("class", null);
					newNodes.Add (node);
					AddAttribute (node, "name", c.Name);
					AddAttribute (node, "presence", "extra");
					counters.Extra++;
					counters.ExtraTotal++;
				}
			}

			XmlNode [] nodes = (XmlNode []) newNodes.ToArray (typeof (XmlNode));
			Array.Sort (nodes, XmlNodeComparer.Default);
			foreach (XmlNode nn in nodes)
				parent.AppendChild (nn);
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

		public string Type {
			get { return type; }
		}
	}

	class XMLAttributes : XMLNameGroup
	{
		bool isTodo;

		protected override bool CheckIfAdd (string value)
		{
			if (value.EndsWith (".MonoTODOAttribute")) {
				isTodo = true;
				return false;
			}

			return true;
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
	}

	class XMLInterfaces : XMLNameGroup
	{
		public override string GroupName {
			get { return "interfaces"; }
		}

		public override string Name {
			get { return "interface"; }
		}
	}

	abstract class XMLMember : XMLNameGroup
	{
		XMLAttributes attributes;
		Hashtable attribs = new Hashtable ();

		public override void CompareTo (XmlDocument doc, XmlNode parent, object other)
		{
			this.document = doc;
			XMLMember mb = other as XMLMember;
			XMLAttributes oa = (mb != null) ? mb.attributes : null;
			if (attributes != null || oa != null) {
				if (group == null)
					group = doc.CreateElement (GroupName, null);

				if (attributes == null)
					attributes = new XMLAttributes ();

				attributes.CompareTo (doc, group, oa);
				counters.AddPartialToTotal (attributes.Counters);
				if (oa != null && oa.IsTodo) {
					counters.Todo++;
					counters.TodoTotal++;
					counters.ErrorTotal++;
					AddAttribute (parent, "error", "todo");
				}
			}

			base.CompareTo (doc, parent, other);
		}

		protected override void LoadExtraData (string name, XmlNode node)
		{
			XmlAttribute xatt = node.Attributes ["attrib"];
			if (xatt != null)
				attribs [name] = xatt.Value;
			
			XmlNode orig = node;
			while (node != null) {
				if (node != null && node.Name == "attributes") {
					attributes = new XMLAttributes ();
					attributes.LoadData (node);
					break;
				}
				node = node.NextSibling;
			}

			base.LoadExtraData (name, orig);
		}

		protected override void CompareToInner (string name, XmlNode parent, XMLNameGroup other)
		{
			if (attribs == null)
				return;

			XMLMember member = (XMLMember) other;
			string att = attribs [name] as string;
			if (att == null)
				return;

			string oatt = null;
			if (member.attribs != null)
				oatt = member.attribs [name] as string;

			string attName = ConvertToString (Int32.Parse (att));
			string otherAttName = "";
			if (oatt != null)
				otherAttName = ConvertToString (Int32.Parse (oatt));

			AddWarning (parent, "Incorrect attributes: '{0}' != '{1}'", attName, otherAttName);
		}

		protected virtual string ConvertToString (int att)
		{
			return null;
		}
	}
	
	class XMLFields : XMLMember
	{
		Hashtable fieldTypes;

		protected override void LoadExtraData (string name, XmlNode node)
		{
			XmlAttribute xatt = node.Attributes ["fieldtype"];
			if (xatt != null) {
				if (fieldTypes == null)
					fieldTypes = new Hashtable ();

				fieldTypes [name] = xatt.Value;
			}

			base.LoadExtraData (name, node);
		}

		protected override void CompareToInner (string name, XmlNode parent, XMLNameGroup other)
		{
			if (fieldTypes == null)
				return;

			XMLFields fields = (XMLFields) other;
			string ftype = fieldTypes [name] as string;
			string oftype = null;
			if (fields.fieldTypes != null)
				oftype = fields.fieldTypes [name] as string;

			AddWarning (parent, "Field type is {0} and should be {1}", oftype, ftype);
		}

		protected override string ConvertToString (int att)
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

	class XMLProperties : XMLMember
	{
		ArrayList methods = new ArrayList ();
		Hashtable map = new Hashtable ();

		protected override void CompareToInner (string name, XmlNode parent, XMLNameGroup other)
		{
			XMLProperties oprop = other as XMLProperties;
			object idx = map [name];
			object oidx = (oprop == null) ? null : oprop.map [name];

			if (idx != null || oidx != null) {
				XMLMethods m, om;

				m = (idx == null) ? new XMLMethods () : (XMLMethods) methods [(int) idx];
				om = (oidx == null) ? null : (XMLMethods) oprop.methods [(int) oidx];

				m.CompareTo (document, parent, om);
				counters.AddPartialToPartial (m.Counters);
			}

			AddCountersAttributes (parent);
			base.CompareToInner (name, parent, other);
		}

		protected override void LoadExtraData (string name, XmlNode node)
		{
			XmlNode orig = node;
			while (node != null) {
				if (node != null && node.Name == "methods") {
					XMLMethods m = new XMLMethods ();
					m.LoadData (node);
					methods.Add (m);
					map [name] = methods.Count - 1;
					break;
				}
				node = node.NextSibling;
			}

			base.LoadExtraData (name, orig);
		}

		public override string GroupName {
			get { return "properties"; }
		}

		public override string Name {
			get { return "property"; }
		}
	}

	class XMLEvents : XMLMember
	{
		Hashtable eventTypes;

		public override void CompareTo (XmlDocument doc, XmlNode parent, object other)
		{
			base.CompareTo (doc, parent, other);
			AddCountersAttributes (parent);
		}

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

		protected override void CompareToInner (string name, XmlNode parent, XMLNameGroup other)
		{
			if (eventTypes == null)
				return;

			XMLEvents evt = (XMLEvents) other;
			string etype = eventTypes [name] as string;
			string oetype = null;
			if (evt.eventTypes != null)
				oetype = evt.eventTypes [name] as string;

			AddWarning (parent, "Event type is {0} and should be {1}", oetype, etype);
		}

		protected override string ConvertToString (int att)
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

	class XMLMethods : XMLMember
	{
		Hashtable returnTypes;

		protected override void LoadExtraData (string name, XmlNode node)
		{
			XmlAttribute xatt = node.Attributes ["returntype"];
			if (xatt != null) {
				if (returnTypes == null)
					returnTypes = new Hashtable ();

				returnTypes [name] = xatt.Value;
			}

			base.LoadExtraData (name, node);
		}

		protected override void CompareToInner (string name, XmlNode parent, XMLNameGroup other)
		{
			if (returnTypes == null)
				return;

			XMLMethods methods = (XMLMethods) other;
			string rtype = returnTypes [name] as string;
			string ortype = null;
			if (methods.returnTypes != null)
				ortype = methods.returnTypes [name] as string;

			AddWarning (parent, "Event type is {0} and should be {1}", ortype, rtype);
		}

		protected override string ConvertToString (int att)
		{
			MethodAttributes ma = (MethodAttributes) att;
			return ma.ToString ();
		}

		public override string GroupName {
			get { return "methods"; }
		}

		public override string Name {
			get { return "method"; }
		}
	}

	class XMLConstructors : XMLMethods
	{
		public override string GroupName {
			get { return "constructors"; }
		}

		public override string Name {
			get { return "constructor"; }
		}
	}

	class XmlNodeComparer : IComparer
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

