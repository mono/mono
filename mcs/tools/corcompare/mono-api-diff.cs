//
// mono-api-diff.cs - Compares 2 xml files produced by mono-api-info and
//		      produces a file suitable to build class status pages.
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Marek Safar		(marek.safar@gmail.com)
//
// Maintainer:
//	C.J. Adams-Collier	(cjac@colliertech.org)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
// (C) 2009,2010 Collier Technologies (http://www.colliertech.org)

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
			if (args.Length != 2) {
				Console.WriteLine ("Usage: mono mono-api-diff.exe <assembly 1 xml> <assembly 2 xml>");
				return 1;
			}

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
				Console.Error.WriteLine ("Error loading {0}: {1}\n{2}", file, e.Message, e);
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

	abstract class XMLData
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

		public static bool IsMeaninglessAttribute (string s)
		{
			if (s == null)
				return false;
			if (s == "System.Runtime.CompilerServices.CompilerGeneratedAttribute")
				return true;
			return false;
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

		protected void AddExtra (XmlNode node)
		{
			//TODO: count all the subnodes?
			AddAttribute (node, "presence", "extra");
			AddAttribute (node, "ok", "1");
			AddAttribute (node, "ok_total", "1");
			AddAttribute (node, "extra", "1");
			AddAttribute (node, "extra_total", "1");
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
				int percent = (100 * counters.Ok / counters.Total);
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
				int percent = (100 * counters.OkTotal / counters.AbsTotal);
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
			XmlNode warning = document.CreateElement ("warning", null);
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
		protected XmlNode group;
		protected Hashtable keys;

		public override void LoadData (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			if (node.Name != GroupName)
				throw new FormatException (String.Format ("Expecting <{0}>", GroupName));

			keys = new Hashtable ();
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

		public override void CompareTo (XmlDocument doc, XmlNode parent, object other)
		{
			this.document = doc;
			if (group == null)
				group = doc.CreateElement (GroupName, null);

			Hashtable okeys = null;
			if (other != null && ((XMLNameGroup) other).keys != null) {
				okeys = ((XMLNameGroup) other).keys;
			}

			XmlNode node = null;
			bool onull = (okeys == null);
			if (keys != null) {
				foreach (DictionaryEntry entry in keys) {
					node = doc.CreateElement (Name, null);
					group.AppendChild (node);
					string key = (string) entry.Key;
					string name = (string) entry.Value;
					AddAttribute (node, "name", name);

					if (!onull && HasKey (key, okeys)) {
						CompareToInner (key, node, (XMLNameGroup) other);
						okeys.Remove (key);
						counters.Present++;
					} else {
						AddAttribute (node, "presence", "missing");
						counters.Missing++;
					}
				}
			}

			if (!onull && okeys.Count != 0) {
				foreach (string value in okeys.Values) {
					node = doc.CreateElement (Name, null);
					AddAttribute (node, "name", (string) value);
					AddAttribute (node, "presence", "extra");
					group.AppendChild (node);
					counters.Extra++;
				}
			}

			if (group.HasChildNodes)
				parent.AppendChild (group);
		}

		protected virtual void CompareToInner (string name, XmlNode node, XMLNameGroup other)
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
				counters.TodoTotal++;
				counters.ErrorTotal++;
				AddAttribute (childA, "error", "todo");
				if (assembly.attributes.Comment != null)
					AddAttribute (childA, "comment", assembly.attributes.Comment);
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

				int idx = -1;
				if (oh.ContainsKey (xns.Name))
					idx = (int) oh [xns.Name];
				XMLNamespace ons = idx >= 0 ? (XMLNamespace) other [idx] : null;
				xns.CompareTo (document, node, ons);
				if (idx >= 0)
					other [idx] = null;
				xns.AddCountersAttributes (node);
				counters.Present++;
				counters.PresentTotal++;
				counters.AddPartialToTotal (xns.Counters);
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
					AddExtra (node);
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

			CompareTypes (childA, nspace != null ? nspace.types : new XMLClass [0]);
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

				int idx = -1;
				if (oh.ContainsKey (xclass.Name))
					idx = (int) oh [xclass.Name];
				xclass.CompareTo (document, node, idx >= 0 ? other [idx] : new XMLClass ());
				if (idx >= 0)
					other [idx] = null;
				counters.AddPartialToPartial (xclass.Counters);
			}

			if (other != null) {
				count = other.Length;
				for (int i = 0; i < count; i++) {
					XMLClass c = other [i];
					if (c == null || IsMonoTODOAttribute (c.Name))
						continue;

					node = document.CreateElement ("class", null);
					newNodes.Add (node);
					AddAttribute (node, "name", c.Name);
					AddAttribute (node, "type", c.Type);
					AddExtra (node);
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
		bool isSerializable;
		bool isAbstract;
		string charSet;
		string layout;
		XMLAttributes attributes;
		XMLInterfaces interfaces;
		XMLGenericTypeConstraints genericConstraints;
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

			if (child != null && child.Name == "generic-type-constraints") {
				genericConstraints = new XMLGenericTypeConstraints ();
				genericConstraints.LoadData (child);
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

			if (child != null && child.Name == "generic-parameters") {
				// HACK: ignore this tag as it doesn't seem to
				// add any value when checking for differences
				return;
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
					if (oclass.attributes.Comment != null)
						AddAttribute (parent, "comment", oclass.attributes.Comment);
				}
			}

			if (type != oclass.type)
				AddWarning (parent, "Class type is wrong: {0} != {1}", type, oclass.type);

			if (baseName != oclass.baseName)
				AddWarning (parent, "Base class is wrong: {0} != {1}", baseName, oclass.baseName);

			if (isAbstract != oclass.isAbstract || isSealed != oclass.isSealed) {
				if ((isAbstract && isSealed) || (oclass.isAbstract && oclass.isSealed))
					AddWarning (parent, "Should {0}be static", (isAbstract && isSealed) ? "" : "not ");
				else if (isAbstract != oclass.isAbstract)
					AddWarning (parent, "Should {0}be abstract", isAbstract ? "" : "not ");
				else if (isSealed != oclass.isSealed)
					AddWarning (parent, "Should {0}be sealed", isSealed ? "" : "not ");
			}

			if (isSerializable != oclass.isSerializable)
				AddWarning (parent, "Should {0}be serializable", isSerializable ? "" : "not ");

			if (charSet != oclass.charSet)
				AddWarning (parent, "CharSet is wrong: {0} != {1}", charSet, oclass.charSet);

			if (layout != oclass.layout)
				AddWarning (parent, "Layout is wrong: {0} != {1}", layout, oclass.layout);

			if (interfaces != null || oclass.interfaces != null) {
				if (interfaces == null)
					interfaces = new XMLInterfaces ();

				interfaces.CompareTo (doc, parent, oclass.interfaces);
				counters.AddPartialToPartial (interfaces.Counters);
			}

			if (genericConstraints != null || oclass.genericConstraints != null) {
				if (genericConstraints == null)
					genericConstraints = new XMLGenericTypeConstraints ();

				genericConstraints.CompareTo (doc, parent, oclass.genericConstraints);
				counters.AddPartialToPartial (genericConstraints.Counters);
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
				XmlNode n = doc.CreateElement ("classes", null);
				parent.AppendChild (n);
				CompareTypes (n, oclass.nested);
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
					if (c == null || IsMonoTODOAttribute (c.Name))
						continue;

					node = document.CreateElement ("class", null);
					newNodes.Add (node);
					AddAttribute (node, "name", c.Name);
					AddAttribute (node, "type", c.Type);
					AddExtra (node);
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

	class XMLParameter : XMLData
	{
		string name;
		string type;
		string attrib;
		string direction;
		bool isUnsafe;
		bool isOptional;
		string defaultValue;
		XMLAttributes attributes;

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

		public override void CompareTo (XmlDocument doc, XmlNode parent, object other)
		{
			this.document = doc;

			XMLParameter oparm = (XMLParameter) other;

			if (name != oparm.name)
				AddWarning (parent, "Parameter name is wrong: {0} != {1}", name, oparm.name);

			if (type != oparm.type)
				AddWarning (parent, "Parameter type is wrong: {0} != {1}", type, oparm.type);

			if (attrib != oparm.attrib)
				AddWarning (parent, "Parameter attributes wrong: {0} != {1}", attrib, oparm.attrib);

			if (direction != oparm.direction)
				AddWarning (parent, "Parameter direction wrong: {0} != {1}", direction, oparm.direction);

			if (isUnsafe != oparm.isUnsafe)
				AddWarning (parent, "Parameter unsafe wrong: {0} != {1}", isUnsafe, oparm.isUnsafe);

			if (isOptional != oparm.isOptional)
				AddWarning (parent, "Parameter optional wrong: {0} != {1}", isOptional, oparm.isOptional);

			if (defaultValue != oparm.defaultValue)
				AddWarning (parent, "Parameter default value wrong: {0} != {1}", (defaultValue == null) ? "(no default value)" : defaultValue, (oparm.defaultValue == null) ? "(no default value)" : oparm.defaultValue);

			if (attributes != null || oparm.attributes != null) {
				if (attributes == null)
					attributes = new XMLAttributes ();

				attributes.CompareTo (doc, parent, oparm.attributes);
				counters.AddPartialToPartial (attributes.Counters);
				if (oparm.attributes != null && oparm.attributes.IsTodo) {
					counters.Todo++;
					counters.TodoTotal++;
					counters.ErrorTotal++;
					AddAttribute (parent, "error", "todo");
					if (oparm.attributes.Comment != null)
						AddAttribute (parent, "comment", oparm.attributes.Comment);
				}
			}
		}

		public string Name {
			get { return name; }
		}
	}

	class XMLAttributeProperties: XMLNameGroup
	{
		static Hashtable ignored_properties;
		static XMLAttributeProperties ()
		{
			ignored_properties = new Hashtable ();
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

		Hashtable properties = new Hashtable ();
		string attribute;

		public XMLAttributeProperties (string attribute)
		{
			this.attribute = attribute;
		}

		public override void LoadData(XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			if (node.ChildNodes == null)
				return;

			string ignored = ignored_properties [attribute] as string;

			foreach (XmlNode n in node.ChildNodes) {
				string name = n.Attributes ["name"].Value;
				if (ignored == name)
					continue;

				if (n.Attributes ["null"] != null) {
					properties.Add (name, null);
					continue;
				}
				string value = n.Attributes ["value"].Value;
				properties.Add (name, value);
			}
		}

		public override void CompareTo (XmlDocument doc, XmlNode parent, object other)
		{
			this.document = doc;

			Hashtable other_properties = ((XMLAttributeProperties)other).properties;
			foreach (DictionaryEntry de in other_properties) {
				object other_value = properties [de.Key];

				if (de.Value == null) {
					if (other_value != null)
						AddWarning (parent, "Property '{0}' is 'null' and should be '{1}'", de.Key, other_value);
					continue;
				}

				if (de.Value.Equals (other_value))
					continue;

				AddWarning (parent, "Property '{0}' is '{1}' and should be '{2}'",
					de.Key, de.Value, other_value == null ? "null" : other_value);
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

	class XMLAttributes : XMLNameGroup
	{
		Hashtable properties = new Hashtable ();

		bool isTodo;
		string comment;

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

			return !IsMeaninglessAttribute (value);
		}

		protected override void CompareToInner (string name, XmlNode node, XMLNameGroup other)
		{
			XMLAttributeProperties other_prop = ((XMLAttributes)other).properties [name] as XMLAttributeProperties;
			XMLAttributeProperties this_prop = properties [name] as XMLAttributeProperties;
			if (other_prop == null || this_prop == null)
				return;

			this_prop.CompareTo (document, node, other_prop);
			counters.AddPartialToPartial (this_prop.Counters);
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

		protected override void LoadExtraData(string name, XmlNode node)
		{
			XmlNode pNode = node.SelectSingleNode ("properties");

			if (IsMonoTODOAttribute (name)) {
				isTodo = true;
				if (pNode.ChildNodes [0].Attributes ["value"] != null) {
					comment = pNode.ChildNodes [0].Attributes ["value"].Value;
				}
				return;
			}

			if (pNode != null) {
				XMLAttributeProperties p = new XMLAttributeProperties (name);
				p.LoadData (pNode);

				properties[name] = p;
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

	class XMLInterfaces : XMLNameGroup
	{
		public override string GroupName {
			get { return "interfaces"; }
		}

		public override string Name {
			get { return "interface"; }
		}
	}

	abstract class XMLGenericGroup : XMLNameGroup
	{
		string attributes;

		protected override void LoadExtraData (string name, XmlNode node)
		{
			attributes = ((XmlElement) node).GetAttribute ("generic-attribute");
		}

		protected override void CompareToInner (string name, XmlNode parent, XMLNameGroup other)
		{
			base.CompareToInner (name, parent, other);

			XMLGenericGroup g = (XMLGenericGroup) other;
			if (attributes != g.attributes)
				AddWarning (parent, "Incorrect generic attributes: '{0}' != '{1}'", attributes, g.attributes);
		}
	}

	class XMLGenericTypeConstraints : XMLGenericGroup
	{
		public override string GroupName {
			get { return "generic-type-constraints"; }
		}

		public override string Name {
			get { return "generic-type-constraint"; }
		}
	}

	class XMLGenericMethodConstraints : XMLGenericGroup
	{
		public override string GroupName {
			get { return "generic-method-constraints"; }
		}

		public override string Name {
			get { return "generic-method-constraint"; }
		}
	}

	abstract class XMLMember : XMLNameGroup
	{
		Hashtable attributeMap;
		Hashtable access = new Hashtable ();

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

		protected override void CompareToInner (string name, XmlNode parent, XMLNameGroup other)
		{
			base.CompareToInner (name, parent, other);
			XMLMember mb = other as XMLMember;
			XMLAttributes att = null;
			XMLAttributes oatt = null;
			if (attributeMap != null)
				att = attributeMap [name] as XMLAttributes;

			if (mb != null && mb.attributeMap != null)
				oatt = mb.attributeMap [name] as XMLAttributes;

			if (att != null || oatt != null) {
				if (att == null)
					att = new XMLAttributes ();

				att.CompareTo (document, parent, oatt);
				counters.AddPartialToPartial(att.Counters);
				if (oatt != null && oatt.IsTodo) {
					counters.Todo++;
					counters.ErrorTotal++;
					AddAttribute (parent, "error", "todo");
					if (oatt.Comment != null)
						AddAttribute (parent, "comment", oatt.Comment);
				}
			}

			XMLMember member = (XMLMember) other;
			string acc = access [name] as string;
			if (acc == null)
				return;

			string oacc = null;
			if (member.access != null)
				oacc = member.access [name] as string;

			string accName = ConvertToString (Int32.Parse (acc));
			string oaccName = "";
			if (oacc != null)
				oaccName = ConvertToString (Int32.Parse (oacc));

			if (accName != oaccName)
				AddWarning (parent, "Incorrect attributes: '{0}' != '{1}'", accName, oaccName);
		}

		protected virtual string ConvertToString (int att)
		{
			return null;
		}
	}

	class XMLFields : XMLMember
	{
		Hashtable fieldTypes;
		Hashtable fieldValues;

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

		protected override void CompareToInner (string name, XmlNode parent, XMLNameGroup other)
		{
			base.CompareToInner (name, parent, other);
			XMLFields fields = (XMLFields) other;
			if (fieldTypes != null) {
				string ftype = fieldTypes [name] as string;
				string oftype = null;
				if (fields.fieldTypes != null)
					oftype = fields.fieldTypes [name] as string;

				if (ftype != oftype)
					AddWarning (parent, "Field type is {0} and should be {1}", oftype, ftype);
			}
			if (fieldValues != null) {
				string fvalue = fieldValues [name] as string;
				string ofvalue = null;
				if (fields.fieldValues != null)
					ofvalue = fields.fieldValues [name] as string;

				if (fvalue != ofvalue)
					AddWarning (parent, "Field value is {0} and should be {1}", ofvalue, fvalue);
			}
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

	class XMLParameters : XMLNameGroup
	{
		public override void LoadData (XmlNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			if (node.Name != GroupName)
				throw new FormatException (String.Format ("Expecting <{0}>", GroupName));

			keys = new Hashtable ();
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

		public override void CompareTo (XmlDocument doc, XmlNode parent, object other)
		{
			this.document = doc;
			if (group == null)
				group = doc.CreateElement (GroupName, null);

			Hashtable okeys = null;
			if (other != null && ((XMLParameters) other).keys != null) {
				okeys = ((XMLParameters) other).keys;
			}

			XmlNode node = null;
			bool onull = (okeys == null);
			if (keys != null) {
				foreach (DictionaryEntry entry in keys) {
					node = doc.CreateElement (Name, null);
					group.AppendChild (node);
					string key = (string) entry.Key;
					XMLParameter parm = (XMLParameter) entry.Value;
					AddAttribute (node, "name", parm.Name);

					if (!onull && HasKey (key, okeys)) {
						parm.CompareTo (document, node, okeys[key]);
						counters.AddPartialToPartial (parm.Counters);
						okeys.Remove (key);
						counters.Present++;
					} else {
						AddAttribute (node, "presence", "missing");
						counters.Missing++;
					}
				}
			}

			if (!onull && okeys.Count != 0) {
				foreach (XMLParameter value in okeys.Values) {
					node = doc.CreateElement (Name, null);
					AddAttribute (node, "name", value.Name);
					AddAttribute (node, "presence", "extra");
					group.AppendChild (node);
					counters.Extra++;
				}
			}

			if (group.HasChildNodes)
				parent.AppendChild (group);
		}
	}

	class XMLProperties : XMLMember
	{
		Hashtable nameToMethod = new Hashtable ();

		protected override void CompareToInner (string name, XmlNode parent, XMLNameGroup other)
		{
			Counters copy = counters;
			counters = new Counters();

			XMLProperties oprop = other as XMLProperties;
			if (oprop != null) {
				XMLMethods m = nameToMethod [name] as XMLMethods;
				XMLMethods om = oprop.nameToMethod [name] as XMLMethods;
				if (m != null || om != null) {
					if (m == null)
						m = new XMLMethods ();

					m.CompareTo(document, parent, om);
					counters.AddPartialToPartial(m.Counters);
				}
			}

			base.CompareToInner (name, parent, other);
			AddCountersAttributes(parent);

			copy.AddPartialToPartial(counters);
			counters = copy;
		}

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
					      (atts["name"]   != null ? atts["name"].Value   : ""),
					      (atts["ptype"]  != null ? atts["ptype"].Value  : ""),
					      (atts["params"] != null ? atts["params"].Value : "")
					      );
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
		Hashtable nameToMethod = new Hashtable ();

		protected override void LoadExtraData (string name, XmlNode node)
		{
			XmlAttribute xatt = node.Attributes ["eventtype"];
			if (xatt != null) {
				if (eventTypes == null)
					eventTypes = new Hashtable ();

				eventTypes [name] = xatt.Value;
			}

			XmlNode child = node.FirstChild;
			while (child != null) {
				if (child != null && child.Name == "methods") {
					XMLMethods m = new XMLMethods ();
					XmlNode parent = child.ParentNode;
					string key = GetNodeKey (name, parent);
					m.LoadData (child);
					nameToMethod [key] = m;
					break;
				}
				child = child.NextSibling;
			}

			base.LoadExtraData (name, node);
		}

		protected override void CompareToInner (string name, XmlNode parent, XMLNameGroup other)
		{
			Counters copy = counters;
			counters = new Counters ();

			try {
				base.CompareToInner (name, parent, other);
				AddCountersAttributes (parent);
				if (eventTypes == null)
					return;

				XMLEvents evt = (XMLEvents) other;
				string etype = eventTypes [name] as string;
				string oetype = null;
				if (evt.eventTypes != null)
					oetype = evt.eventTypes [name] as string;

				if (etype != oetype)
					AddWarning (parent, "Event type is {0} and should be {1}", oetype, etype);

				XMLMethods m = nameToMethod [name] as XMLMethods;
				XMLMethods om = evt.nameToMethod [name] as XMLMethods;
				if (m != null || om != null) {
					if (m == null)
						m = new XMLMethods ();

					m.CompareTo (document, parent, om);
					counters.AddPartialToPartial (m.Counters);
				}
			} finally {
				AddCountersAttributes (parent);
				copy.AddPartialToPartial (counters);
				counters = copy;
			}
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
		Hashtable parameters;
		Hashtable genericConstraints;
		Hashtable signatureFlags;

		[Flags]
		enum SignatureFlags
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

			XmlNode genericNode = node.SelectSingleNode ("generic-method-constraints");
			if (genericNode != null) {
				if (genericConstraints == null)
					genericConstraints = new Hashtable ();
				XMLGenericMethodConstraints csts = new XMLGenericMethodConstraints ();
				csts.LoadData (genericNode);
				genericConstraints [name] = csts;
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

		protected override void CompareToInner (string name, XmlNode parent, XMLNameGroup other)
		{
			// create backup of actual counters
			Counters copy = counters;
			// initialize counters for current method
			counters = new Counters();

			try {
				base.CompareToInner(name, parent, other);
				XMLMethods methods = (XMLMethods) other;

				SignatureFlags flags = signatureFlags != null &&
					signatureFlags.ContainsKey (name) ?
					(SignatureFlags) signatureFlags [name] :
					SignatureFlags.None;
				SignatureFlags oflags = methods.signatureFlags != null &&
					methods.signatureFlags.ContainsKey (name) ?
					(SignatureFlags) methods.signatureFlags [name] :
					SignatureFlags.None;

				if (flags!= oflags) {
					if (flags == SignatureFlags.None)
						AddWarning (parent, String.Format ("should not be {0}", oflags));
					else if (oflags == SignatureFlags.None)
						AddWarning (parent, String.Format ("should be {0}", flags));
					else
						AddWarning (parent, String.Format ("{0} and should be {1}", oflags, flags));
				}

				if (returnTypes != null) {
					string rtype = returnTypes[name] as string;
					string ortype = null;
					if (methods.returnTypes != null)
						ortype = methods.returnTypes[name] as string;

					if (rtype != ortype)
						AddWarning (parent, "Return type is {0} and should be {1}", ortype, rtype);
				}

				if (parameters != null) {
					XMLParameters parms = parameters[name] as XMLParameters;
					parms.CompareTo (document, parent, methods.parameters[name]);
					counters.AddPartialToPartial (parms.Counters);
				}
			} finally {
				// output counter attributes in result document
				AddCountersAttributes(parent);

				// add temporary counters to actual counters
				copy.AddPartialToPartial(counters);
				// restore backup of actual counters
				counters = copy;
			}
		}

		protected override string ConvertToString (int att)
		{
			MethodAttributes ma = (MethodAttributes) att;
			// ignore ReservedMasks
			ma &= ~ MethodAttributes.ReservedMask;
			ma &= ~ MethodAttributes.VtableLayoutMask;
			if ((ma & MethodAttributes.FamORAssem) != 0)
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

