//
// System.Diagnostics.DiagnosticsConfigurationHandler.cs
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// Authors: 
//	John R. Hicks <angryjohn69@nc.rr.com>
//	Jonathan Pryor <jonpryor@vt.edu>
//
// (C) 2002, 2005
//

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
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Threading;
#if (XML_DEP)
using System.Xml;
#endif
namespace System.Diagnostics
{
	// It handles following elements in <system.diagnostics> :
	//	- <sharedListeners> [2.0]
	//	- <sources>
	//		- <source>
	//			- <listeners> (collection)
	//	- <switches>
	//		- <add name=string value=string />
	//	- <trace autoflush=bool indentsize=int useGlobalLock=bool>
	//		- <listeners>
	internal sealed class DiagnosticsConfiguration
	{
#if NO_LOCK_FREE
		private static object lock_ = new object();
#endif
		private static object settings;

		public static IDictionary Settings {
			get {
#if !NO_LOCK_FREE
				if (settings == null) {
					object s = ConfigurationSettings.GetConfig ("system.diagnostics");
					if (s == null)
						throw new Exception ("INTERNAL configuration error: failed to get configuration 'system.diagnostics'");
					Thread.MemoryBarrier ();
					while (Interlocked.CompareExchange (ref settings, s, null) == null) {
						// do nothing; we're just setting settings.
					}
					Thread.MemoryBarrier ();
				}
#else
				lock (lock_) {
					if (settings == null)
						settings = ConfigurationSettings.GetConfig ("system.diagnostics");
				}
#endif
				return (IDictionary) settings;
			}
		}
	}
#if (XML_DEP)
#if NET_2_0
	[Obsolete ("This class is obsoleted")]
#endif
	public class DiagnosticsConfigurationHandler : IConfigurationSectionHandler
	{
		TraceImplSettings configValues;

		delegate void ElementHandler (IDictionary d, XmlNode node);

		IDictionary elementHandlers = new Hashtable ();

		public DiagnosticsConfigurationHandler ()
		{
			elementHandlers ["assert"] = new ElementHandler (AddAssertNode);
			elementHandlers ["switches"] = new ElementHandler (AddSwitchesNode);
			elementHandlers ["trace"] = new ElementHandler (AddTraceNode);
#if NET_2_0
			elementHandlers ["sources"] = new ElementHandler (AddSourcesNode);
#endif
		}

		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			IDictionary d;
			if (parent == null)
				d = new Hashtable (CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);
			else
				d = (IDictionary) ((ICloneable)parent).Clone();

			if (d.Contains (TraceImplSettings.Key))
				configValues = (TraceImplSettings) d [TraceImplSettings.Key];
			else
				d.Add (TraceImplSettings.Key, configValues = new TraceImplSettings ());

#if NET_2_0
			// process <sharedListeners> first
			foreach (XmlNode child in section.ChildNodes) {
				switch (child.NodeType) {
				case XmlNodeType.Element:
					if (child.LocalName != "sharedListeners")
						continue;
					AddTraceListeners (d, child, GetSharedListeners (d));
					break;
				}
			}
#endif

			foreach (XmlNode child in section.ChildNodes) {
				XmlNodeType type = child.NodeType;

				switch (type) {
				/* ignore */
				case XmlNodeType.Whitespace:
				case XmlNodeType.Comment:
					continue;
				case XmlNodeType.Element:
#if NET_2_0
					if (child.LocalName == "sharedListeners")
						continue;
#endif
					ElementHandler eh = (ElementHandler) elementHandlers [child.Name];
					if (eh != null)
						eh (d, child);
					else
						ThrowUnrecognizedElement (child);
					break;
				default:
					ThrowUnrecognizedElement (child);
					break;
				}
			}

			return d;
		}

		// Remarks: Both attribute are optional
		private void AddAssertNode (IDictionary d, XmlNode node)
		{
			XmlAttributeCollection c = node.Attributes;
			string assertuienabled = GetAttribute (c, "assertuienabled", false, node);
			string logfilename = GetAttribute (c, "logfilename", false, node);
			ValidateInvalidAttributes (c, node);
			if (assertuienabled != null) {
				try {
					d ["assertuienabled"] = bool.Parse (assertuienabled);
				}
				catch (Exception e) {
					throw new ConfigurationException ("The `assertuienabled' attribute must be `true' or `false'",
							e, node);
				}
			}

			if (logfilename != null)
				d ["logfilename"] = logfilename;

			DefaultTraceListener dtl = (DefaultTraceListener) configValues.Listeners["Default"];
			if (dtl != null) {
				if (assertuienabled != null)
					dtl.AssertUiEnabled = (bool) d ["assertuienabled"];
				if (logfilename != null)
					dtl.LogFileName = logfilename;
			}

			if (node.ChildNodes.Count > 0)
				ThrowUnrecognizedElement (node.ChildNodes[0]);
		}

		// name and value attributes are required
		// Docs do not define "remove" or "clear" elements, but .NET recognizes
		// them
		private void AddSwitchesNode (IDictionary d, XmlNode node)
		{
#if !TARGET_JVM
			// There are no attributes on <switch/>
			ValidateInvalidAttributes (node.Attributes, node);

			IDictionary newNodes = new Hashtable ();

			foreach (XmlNode child in node.ChildNodes) {
				XmlNodeType t = child.NodeType;
				if (t == XmlNodeType.Whitespace || t == XmlNodeType.Comment)
					continue;
				if (t == XmlNodeType.Element) {
					XmlAttributeCollection attributes = child.Attributes;
					string name = null;
					string value = null;
					switch (child.Name) {
						case "add":
							name = GetAttribute (attributes, "name", true, child);
							value = GetAttribute (attributes, "value", true, child);
							newNodes [name] = GetSwitchValue (name, value);
							break;
						case "remove":
							name = GetAttribute (attributes, "name", true, child);
							newNodes.Remove (name);
							break;
						case "clear":
							newNodes.Clear ();
							break;
						default:
							ThrowUnrecognizedElement (child);
							break;
					}
					ValidateInvalidAttributes (attributes, child);
				}
				else
					ThrowUnrecognizedNode (child);
			}

			d [node.Name] = newNodes;
#endif
		}

		private static object GetSwitchValue (string name, string value)
		{
#if NET_2_0
			return value;
#else
			try {
				return int.Parse (value);
			} catch {
				throw new ConfigurationException (string.Format (
					"Error in trace switch '{0}': " + 
					"The value of a switch must be integral",
					name));
			}
#endif
		}

		private void AddTraceNode (IDictionary d, XmlNode node)
		{
			AddTraceAttributes (d, node);

			foreach (XmlNode child in node.ChildNodes) {
				XmlNodeType t = child.NodeType;
				if (t == XmlNodeType.Whitespace || t == XmlNodeType.Comment)
					continue;
				if (t == XmlNodeType.Element) {
					if (child.Name == "listeners")
						AddTraceListeners (d, child, configValues.Listeners);
					else
						ThrowUnrecognizedElement (child);
					ValidateInvalidAttributes (child.Attributes, child);
				}
				else
					ThrowUnrecognizedNode (child);
			}
		}

		// all attributes are optional
		private void AddTraceAttributes (IDictionary d, XmlNode node)
		{
			XmlAttributeCollection c = node.Attributes;
			string autoflushConf = GetAttribute (c, "autoflush", false, node);
			string indentsizeConf = GetAttribute (c, "indentsize", false, node);
			ValidateInvalidAttributes (c, node);
			if (autoflushConf != null) {
				bool autoflush = false;
				try {
					autoflush = bool.Parse (autoflushConf);
					d ["autoflush"] = autoflush;
				} catch (Exception e) {
					throw new ConfigurationException ("The `autoflush' attribute must be `true' or `false'",
							e, node);
				}
				configValues.AutoFlush = autoflush;
			}
			if (indentsizeConf != null) {
				int indentsize = 0;
				try {
					indentsize = int.Parse (indentsizeConf);
					d ["indentsize"] = indentsize;
				} catch (Exception e) {
					throw new ConfigurationException ("The `indentsize' attribute must be an integral value.",
							e, node);
				}
				configValues.IndentSize = indentsize;
			}
		}

#if NET_2_0
		private TraceListenerCollection GetSharedListeners (IDictionary d)
		{
			TraceListenerCollection shared_listeners = d ["sharedListeners"] as TraceListenerCollection;
			if (shared_listeners == null) {
				shared_listeners = new TraceListenerCollection (false);
				d ["sharedListeners"] = shared_listeners;
			}
			return shared_listeners;
		}

		private void AddSourcesNode (IDictionary d, XmlNode node)
		{
			// FIXME: are there valid attributes?
			ValidateInvalidAttributes (node.Attributes, node);
			Hashtable sources = d ["sources"] as Hashtable;
			if (sources == null) {
				sources = new Hashtable ();
				d ["sources"] = sources;
			}

			foreach (XmlNode child in node.ChildNodes) {
				XmlNodeType t = child.NodeType;
				if (t == XmlNodeType.Whitespace || t == XmlNodeType.Comment)
					continue;
				if (t == XmlNodeType.Element) {
					if (child.Name == "source")
						AddTraceSource (d, sources, child);
					else
						ThrowUnrecognizedElement (child);
//					ValidateInvalidAttributes (child.Attributes, child);
				}
				else
					ThrowUnrecognizedNode (child);
			}
		}

		private void AddTraceSource (IDictionary d, Hashtable sources, XmlNode node)
		{
			string name = null;
			SourceLevels levels = SourceLevels.Error;
			StringDictionary atts = new StringDictionary ();
			foreach (XmlAttribute a in node.Attributes) {
				switch (a.Name) {
				case "name":
					name = a.Value;
					break;
				case "switchValue":
					levels = (SourceLevels) Enum.Parse (typeof (SourceLevels), a.Value);
					break;
				default:
					atts [a.Name] = a.Value;
					break;
				}
			}
			if (name == null)
				throw new ConfigurationException ("Mandatory attribute 'name' is missing in 'source' element.");

			// ignore duplicate ones (no error occurs)
			if (sources.ContainsKey (name))
				return;

			TraceSourceInfo sinfo = new TraceSourceInfo (name, levels, configValues);
			sources.Add (sinfo.Name, sinfo);
			
			foreach (XmlNode child in node.ChildNodes) {
				XmlNodeType t = child.NodeType;
				if (t == XmlNodeType.Whitespace || t == XmlNodeType.Comment)
					continue;
				if (t == XmlNodeType.Element) {
					if (child.Name == "listeners")
						AddTraceListeners (d, child, sinfo.Listeners);
					else
						ThrowUnrecognizedElement (child);
					ValidateInvalidAttributes (child.Attributes, child);
				}
				else
					ThrowUnrecognizedNode (child);
			}
		}
#endif

		// only defines "add" and "remove", but "clear" also works
		// for add, "name" is required; initializeData is optional; "type" is required in 1.x, optional in 2.0.
		private void AddTraceListeners (IDictionary d, XmlNode listenersNode, TraceListenerCollection listeners)
		{
#if !TARGET_JVM
			// There are no attributes on <listeners/>
			ValidateInvalidAttributes (listenersNode.Attributes, listenersNode);

			foreach (XmlNode child in listenersNode.ChildNodes) {
				XmlNodeType t = child.NodeType;
				if (t == XmlNodeType.Whitespace || t == XmlNodeType.Comment)
					continue;
				if (t == XmlNodeType.Element) {
					XmlAttributeCollection attributes = child.Attributes;
					string name = null;
					switch (child.Name) {
						case "add":
							AddTraceListener (d, child, attributes, listeners);
							break;
						case "remove":
							name = GetAttribute (attributes, "name", true, child);
							RemoveTraceListener (name);
							break;
						case "clear":
							configValues.Listeners.Clear ();
							break;
						default:
							ThrowUnrecognizedElement (child);
							break;
					}
					ValidateInvalidAttributes (attributes, child);
				}
				else
					ThrowUnrecognizedNode (child);
			}
#endif
		}

		private void AddTraceListener (IDictionary d, XmlNode child, XmlAttributeCollection attributes, TraceListenerCollection listeners)
		{
			string name = GetAttribute (attributes, "name", true, child);
			string type = null;

#if NET_2_0 && CONFIGURATION_DEP
			type = GetAttribute (attributes, "type", false, child);
			if (type == null) {
				// indicated by name.
				TraceListener shared = GetSharedListeners (d) [name];
				if (shared == null)
					throw new ConfigurationException (String.Format ("Shared trace listener {0} does not exist.", name));
				if (attributes.Count != 0)
					throw new ConfigurationErrorsException (string.Format (
						"Listener '{0}' references a shared " +
						"listener and can only have a 'Name' " +
						"attribute.", name));
				listeners.Add (shared, configValues);
				return;
			}
#else
			type = GetAttribute (attributes, "type", true, child);
#endif

			Type t = Type.GetType (type);
			if (t == null)
				throw new ConfigurationException (string.Format ("Invalid Type Specified: {0}", type));

			object[] args;
			Type[] types;

			string initializeData = GetAttribute (attributes, "initializeData", false, child);
			if (initializeData != null) {
				args = new object[] { initializeData };
				types = new Type[] { typeof(string) };
			} else {
				args = null;
				types = Type.EmptyTypes;
			}

			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			if (t.Assembly == GetType ().Assembly)
				flags |= BindingFlags.NonPublic;

			ConstructorInfo ctor = t.GetConstructor (flags, null, types, null);
			if (ctor == null) 
				throw new ConfigurationException ("Couldn't find constructor for class " + type);
			
			TraceListener l = (TraceListener) ctor.Invoke (args);
			l.Name = name;

#if NET_2_0 && CONFIGURATION_DEP
			string trace = GetAttribute (attributes, "traceOutputOptions", false, child);
			if (trace != null) {
				if (trace != trace.Trim ())
					throw new ConfigurationErrorsException (string.Format (
						"Invalid value '{0}' for 'traceOutputOptions'.",
						trace), child);

				TraceOptions trace_options;
	
				try {
					trace_options = (TraceOptions) Enum.Parse (
						typeof (TraceOptions), trace);
				} catch (ArgumentException) {
					throw new ConfigurationErrorsException (string.Format (
						"Invalid value '{0}' for 'traceOutputOptions'.",
						trace), child);
				}

				l.TraceOutputOptions = trace_options;
			}

			string [] supported_attributes = l.GetSupportedAttributes ();
			if (supported_attributes != null) {
				for (int i = 0; i < supported_attributes.Length; i++) {
					string key = supported_attributes [i];
					string value = GetAttribute (attributes, key, false, child);
					if (value != null)
						l.Attributes.Add (key, value);
				}
			}
#endif

			listeners.Add (l, configValues);
		}

		private void RemoveTraceListener (string name)
		{
			try {
				configValues.Listeners.Remove (name);
			}
			catch (ArgumentException) {
				// The specified listener wasn't in the collection
				// Ignore this; .NET does.
			}
			catch (Exception e) {
				throw new ConfigurationException (
						string.Format ("Unknown error removing listener: {0}", name),
						e);
			}
		}

		private string GetAttribute (XmlAttributeCollection attrs, string attr, bool required, XmlNode node)
		{
			XmlAttribute a = attrs[attr];

			string r = null;

			if (a != null) {
				r = a.Value;
				if (required)
					ValidateAttribute (attr, r, node);
				attrs.Remove (a);
			}
			else if (required)
				ThrowMissingAttribute (attr, node);

			return r;
		}

		private void ValidateAttribute (string attribute, string value, XmlNode node)
		{
			if (value == null || value.Length == 0)
				throw new ConfigurationException (string.Format ("Required attribute '{0}' cannot be empty.", attribute), node);
		}

		private void ValidateInvalidAttributes (XmlAttributeCollection c, XmlNode node)
		{
			if (c.Count != 0)
				ThrowUnrecognizedAttribute (c[0].Name, node);
		}

		private void ThrowMissingAttribute (string attribute, XmlNode node)
		{
			throw new ConfigurationException (string.Format ("Required attribute '{0}' not found.", attribute), node);
		}

		private void ThrowUnrecognizedNode (XmlNode node)
		{
			throw new ConfigurationException (
					string.Format ("Unrecognized node `{0}'; nodeType={1}", node.Name, node.NodeType),
					node);
		}

		private void ThrowUnrecognizedElement (XmlNode node)
		{
			throw new ConfigurationException (
					string.Format ("Unrecognized element '{0}'.", node.Name),
					node);
		}

		private void ThrowUnrecognizedAttribute (string attribute, XmlNode node)
		{
			throw new ConfigurationException (
					string.Format ("Unrecognized attribute '{0}' on element <{1}/>.", attribute, node.Name),
					node);
		}
	}
#endif
}

