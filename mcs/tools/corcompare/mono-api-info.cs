//
// mono-api-info.cs - Dumps public assembly information to an xml file.
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Mono.AssemblyInfo
{
	class Driver
	{
		static int Main (string [] args)
		{
			if (args.Length == 0)
				return 1;

			AssemblyCollection acoll = new AssemblyCollection ();
			
			foreach (string fullName in args) {
				acoll.Add (fullName);
			}

			XmlDocument doc = new XmlDocument ();
			acoll.Document = doc;
			acoll.DoOutput ();

			XmlTextWriter writer = new XmlTextWriter (Console.Out);
			writer.Formatting = Formatting.Indented;
			XmlNode decl = doc.CreateXmlDeclaration ("1.0", null, null);
			doc.InsertBefore (decl, doc.DocumentElement);
			doc.WriteTo (writer);
			return 0;
		}
	}

	class AssemblyCollection
	{
		XmlDocument document;
		ArrayList assemblies;

		public AssemblyCollection ()
		{
			assemblies = new ArrayList ();
		}

		public bool Add (string name)
		{
			Assembly ass = LoadAssembly (name);
			if (ass == null)
				return false;

			assemblies.Add (ass);
			return true;
		}

		public void DoOutput ()
		{
			if (document == null)
				throw new InvalidOperationException ("Document not set");

			XmlNode nassemblies = document.CreateElement ("assemblies", null);
			document.AppendChild (nassemblies);
			foreach (Assembly a in assemblies) {
				AssemblyData data = new AssemblyData (document, nassemblies, a);
				data.DoOutput ();
			}
		}

		public XmlDocument Document {
			set { document = value; }
		}
		
		static Assembly LoadAssembly (string aname)
		{
			Assembly ass = null;
			try {
				string name = aname;
				if (!name.EndsWith (".dll"))
					name += ".dll";
				ass = Assembly.LoadFrom (name);
				return ass;
			} catch { }

			try {
				ass = Assembly.LoadWithPartialName (aname);
				return ass;
			} catch { }

			return null;
		}
	}

	abstract class BaseData
	{
		protected XmlDocument document;
		protected XmlNode parent;

		protected BaseData (XmlDocument doc, XmlNode parent)
		{
			this.document = doc;
			this.parent = parent;
		}

		public abstract void DoOutput ();

		protected void AddAttribute (XmlNode node, string name, string value)
		{
			XmlAttribute attr = document.CreateAttribute (name);
			attr.Value = value;
			node.Attributes.Append (attr);
		}
	}

	class AssemblyData : BaseData
	{
		Assembly ass;
		
		public AssemblyData (XmlDocument document, XmlNode parent, Assembly ass)
			: base (document, parent)
		{
			this.ass = ass;
		}

		public override void DoOutput ()
		{
			if (document == null)
				throw new InvalidOperationException ("Document not set");

			XmlNode nassembly = document.CreateElement ("assembly", null);
			AssemblyName aname = ass.GetName ();
			AddAttribute (nassembly, "name", aname.Name);
			AddAttribute (nassembly, "version", aname.Version.ToString ());
			parent.AppendChild (nassembly);
			AttributeData.OutputAttributes (document, nassembly, ass.GetCustomAttributes (false));
			Type [] types = ass.GetTypes ();
			if (types == null || types.Length == 0)
				return;

			Array.Sort (types, TypeComparer.Default);

			XmlNode nss = document.CreateElement ("namespaces", null);
			nassembly.AppendChild (nss);

			string currentNS = "$%&$&";
			XmlNode ns = null;
			XmlNode classes = null;
			foreach (Type t in types) {
				if (t.Namespace == null || t.Namespace == "")
					continue;

				if (t.IsNotPublic)
					continue;

				if (t.IsNestedPublic || t.IsNestedAssembly || t.IsNestedFamANDAssem ||
					t.IsNestedFamORAssem || t.IsNestedPrivate)
					continue;

				if (t.DeclaringType != null)
					continue; // enforce !nested
				
				if (t.Namespace != currentNS) {
					currentNS = t.Namespace;
					ns = document.CreateElement ("namespace", null);
					AddAttribute (ns, "name", currentNS);
					nss.AppendChild (ns);
					classes = document.CreateElement ("classes", null);
					ns.AppendChild (classes);
				}
				
				TypeData bd = new TypeData (document, classes, t);
				bd.DoOutput ();
			}
		}
	}

	abstract class MemberData : BaseData
	{
		MemberInfo [] members;

		public MemberData (XmlDocument document, XmlNode parent, MemberInfo [] members)
			: base (document, parent)
		{
			this.members = members;
		}

		public override void DoOutput ()
		{
			XmlNode mclass = document.CreateElement (ParentTag, null);
			parent.AppendChild (mclass);

			foreach (MemberInfo member in members) {
				XmlNode mnode = document.CreateElement (Tag, null);
				mclass.AppendChild (mnode);
				AddAttribute (mnode, "name", GetName (member));
				if (!NoMemberAttributes)
					AddAttribute (mnode, "attrib", GetMemberAttributes (member));

				AttributeData.OutputAttributes (document, mnode,
								member.GetCustomAttributes (false));

				AddExtraData (mnode, member);
			}
		}

		protected virtual void AddExtraData (XmlNode p, MemberInfo member)
		{
		}

		protected virtual string GetName (MemberInfo member)
		{
			return "NoNAME";
		}

		protected virtual string GetMemberAttributes (MemberInfo member)
		{
			return null;
		}

		public virtual bool NoMemberAttributes {
			get { return false; }
			set {}
		}

		public virtual string ParentTag {
			get { return "NoPARENTTAG"; }
		}
		
		public virtual string Tag {
			get { return "NoTAG"; }
		}
	}

	class TypeData : MemberData
	{
		Type type;
		const BindingFlags flags = BindingFlags.Public | BindingFlags.Static |
						BindingFlags.Instance | BindingFlags.DeclaredOnly | 
						BindingFlags.NonPublic;
		
		public TypeData (XmlDocument document, XmlNode parent, Type type)
			: base (document, parent, null)
		{
			this.type = type;
		}

		public override void DoOutput ()
		{
			if (document == null)
				throw new InvalidOperationException ("Document not set");

			XmlNode nclass = document.CreateElement ("class", null);
			AddAttribute (nclass, "name", type.Name);
			string classType = GetClassType (type);
			AddAttribute (nclass, "type", classType);

			if (type.BaseType != null)
				AddAttribute (nclass, "base", type.BaseType.FullName);

			if (type.IsSealed)
				AddAttribute (nclass, "sealed", "true");

			if (type.IsAbstract)
				AddAttribute (nclass, "abstract", "true");

			if (type.IsSerializable)
				AddAttribute (nclass, "serializable", "true");

			string charSet = GetCharSet (type);
			AddAttribute (nclass, "charset", charSet);

			string layout = GetLayout (type);
			if (layout != null)
				AddAttribute (nclass, "layout", layout);

			parent.AppendChild (nclass);
			
			AttributeData.OutputAttributes (document, nclass, type.GetCustomAttributes (false));

			Type [] interfaces = type.GetInterfaces ();
			if (interfaces != null && interfaces.Length > 0) {
				XmlNode ifaces = document.CreateElement ("interfaces", null);
				nclass.AppendChild (ifaces);
				foreach (Type t in interfaces) {
					if (!t.IsPublic) {
						// we're only interested in public interfaces
						continue;
					}
					XmlNode iface = document.CreateElement ("interface", null);
					AddAttribute (iface, "name", t.FullName);
					ifaces.AppendChild (iface);
				}
			}

			ArrayList members = new ArrayList ();

			FieldInfo[] fields = GetFields (type);
			if (fields.Length > 0) {
				Array.Sort (fields, MemberInfoComparer.Default);
				FieldData fd = new FieldData (document, nclass, fields);
				// Special case for enum fields
				if (classType == "enum") {
					string etype = fields [0].GetType ().FullName;
					AddAttribute (nclass, "enumtype", etype);
				}
				members.Add (fd);
			}

			ConstructorInfo [] ctors = GetConstructors (type);
			if (ctors.Length > 0) {
				Array.Sort (ctors, MemberInfoComparer.Default);
				members.Add (new ConstructorData (document, nclass, ctors));
			}

			PropertyInfo[] properties = GetProperties (type);
			if (properties.Length > 0) {
				Array.Sort (properties, MemberInfoComparer.Default);
				members.Add (new PropertyData (document, nclass, properties));
			}

			EventInfo [] events = GetEvents (type);
			if (events.Length > 0) {
				Array.Sort (events, MemberInfoComparer.Default);
				members.Add (new EventData (document, nclass, events));
			}

			MethodInfo [] methods = GetMethods (type);
			if (methods.Length > 0) {
				Array.Sort (methods, MemberInfoComparer.Default);
				members.Add (new MethodData (document, nclass, methods));
			}

			foreach (MemberData md in members)
				md.DoOutput ();

			Type [] nested = type.GetNestedTypes ();
			if (nested != null && nested.Length > 0) {
				XmlNode classes = document.CreateElement ("classes", null);
				nclass.AppendChild (classes);
				foreach (Type t in nested) {
					TypeData td = new TypeData (document, classes, t);
					td.DoOutput ();
				}
			}
		}

		protected override string GetMemberAttributes (MemberInfo member)
		{
			if (member != type)
				throw new InvalidOperationException ("odd");
				
			return ((int) type.Attributes).ToString (CultureInfo.InvariantCulture);
		}

		public static bool MustDocumentMethod(MethodBase method)
		{
			// All other methods
			return (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
		}

		static string GetClassType (Type t)
		{
			if (t.IsEnum)
				return "enum";

			if (t.IsValueType)
				return "struct";

			if (t.IsInterface)
				return "interface";

			if (typeof (Delegate).IsAssignableFrom (t))
				return "delegate";

			return "class";
		}

		private static string GetCharSet (Type type)
		{
			if (type.IsAnsiClass)
				return CharSet.Ansi.ToString (CultureInfo.InvariantCulture);

			if (type.IsAutoClass)
				return CharSet.Auto.ToString (CultureInfo.InvariantCulture);

			if (type.IsUnicodeClass)
				return CharSet.Unicode.ToString (CultureInfo.InvariantCulture);

			return CharSet.None.ToString (CultureInfo.InvariantCulture);
		}

		private static string GetLayout (Type type)
		{
			if (type.IsAutoLayout)
				return LayoutKind.Auto.ToString (CultureInfo.InvariantCulture);

			if (type.IsExplicitLayout)
				return LayoutKind.Explicit.ToString (CultureInfo.InvariantCulture);

			if (type.IsLayoutSequential)
				return LayoutKind.Sequential.ToString (CultureInfo.InvariantCulture);

			return null;
		}

		private FieldInfo[] GetFields (Type type)
		{
			ArrayList list = new ArrayList ();

			FieldInfo[] fields = type.GetFields (flags);
			foreach (FieldInfo field in fields) {
				if (field.IsSpecialName)
					continue;

				// we're only interested in public or protected members
				if (!field.IsPublic && !field.IsFamily && !field.IsFamilyOrAssembly)
					continue;

				list.Add (field);
			}

			return (FieldInfo[]) list.ToArray (typeof (FieldInfo));
		}

		private PropertyInfo[] GetProperties (Type type)
		{
			ArrayList list = new ArrayList ();

			PropertyInfo[] properties = type.GetProperties (flags);
			foreach (PropertyInfo property in properties) {
				MethodInfo getMethod = null;
				MethodInfo setMethod = null;

				if (property.CanRead) {
					try { getMethod = property.GetGetMethod (true); }
					catch (System.Security.SecurityException) { }
				}
				if (property.CanWrite) {
					try { setMethod = property.GetSetMethod (true); }
					catch (System.Security.SecurityException) { }
				}

				bool hasGetter = (getMethod != null) && MustDocumentMethod (getMethod);
				bool hasSetter = (setMethod != null) && MustDocumentMethod (setMethod);

				// if neither the getter or setter should be documented, then
				// skip the property
				if (!hasGetter && !hasSetter) {
					continue;
				}

				list.Add (property);
			}

			return (PropertyInfo[]) list.ToArray (typeof (PropertyInfo));
		}

		private MethodInfo[] GetMethods (Type type)
		{
			ArrayList list = new ArrayList ();

			MethodInfo[] methods = type.GetMethods (flags);
			foreach (MethodInfo method in methods) {
				if (method.IsSpecialName)
					continue;

				// we're only interested in public or protected members
				if (!MustDocumentMethod(method))
					continue;

				list.Add (method);
			}

			return (MethodInfo[]) list.ToArray (typeof (MethodInfo));
		}

		private ConstructorInfo[] GetConstructors (Type type)
		{
			ArrayList list = new ArrayList ();

			ConstructorInfo[] ctors = type.GetConstructors (flags);
			foreach (ConstructorInfo constructor in ctors) {
				// we're only interested in public or protected members
				if (!constructor.IsPublic && !constructor.IsFamily && !constructor.IsFamilyOrAssembly)
					continue;

				list.Add (constructor);
			}

			return (ConstructorInfo[]) list.ToArray (typeof (ConstructorInfo));
		}

		private EventInfo[] GetEvents (Type type)
		{
			ArrayList list = new ArrayList ();

			EventInfo[] events = type.GetEvents (flags);
			foreach (EventInfo eventInfo in events) {
				MethodInfo addMethod = eventInfo.GetAddMethod (true);

				if (addMethod == null || !MustDocumentMethod (addMethod))
					continue;

				list.Add (eventInfo);
			}

			return (EventInfo[]) list.ToArray (typeof (EventInfo));
		}
	}

	class FieldData : MemberData
	{
		public FieldData (XmlDocument document, XmlNode parent, FieldInfo [] members)
			: base (document, parent, members)
		{
		}

		protected override string GetName (MemberInfo member)
		{
			FieldInfo field = (FieldInfo) member;
			return field.Name;
		}

		protected override string GetMemberAttributes (MemberInfo member)
		{
			FieldInfo field = (FieldInfo) member;
			return ((int) field.Attributes).ToString (CultureInfo.InvariantCulture);
		}

		protected override void AddExtraData (XmlNode p, MemberInfo member)
		{
			base.AddExtraData (p, member);
			FieldInfo field = (FieldInfo) member;
			AddAttribute (p, "fieldtype", field.FieldType.FullName);

			if (field.IsLiteral) {
				object value = field.GetValue (null);
				string stringValue = null;
				if (value is Enum) {
					// FIXME: when Mono bug #60090 has been
					// fixed, we should just be able to use
					// Convert.ToString
					stringValue = ((Enum) value).ToString ("D", CultureInfo.InvariantCulture);
				} else {
					stringValue = Convert.ToString (value, CultureInfo.InvariantCulture);
				}

				if (stringValue != null)
					AddAttribute (p, "value", stringValue);
			}
		}

		public override string ParentTag {
			get { return "fields"; }
		}

		public override string Tag {
			get { return "field"; }
		}
	}

	class PropertyData : MemberData
	{
		public PropertyData (XmlDocument document, XmlNode parent, PropertyInfo [] members)
			: base (document, parent, members)
		{
		}

		protected override string GetName (MemberInfo member)
		{
			PropertyInfo prop = (PropertyInfo) member;
			return prop.Name;
		}

		protected override void AddExtraData (XmlNode p, MemberInfo member)
		{
			base.AddExtraData (p, member);
			PropertyInfo prop = (PropertyInfo) member;
			AddAttribute (p, "ptype", prop.PropertyType.FullName);
			MethodInfo _get = prop.GetGetMethod (true);
			MethodInfo _set = prop.GetSetMethod (true);
			bool haveGet = (_get != null && TypeData.MustDocumentMethod(_get));
			bool haveSet = (_set != null && TypeData.MustDocumentMethod(_set));
			MethodInfo [] methods;

			if (haveGet && haveSet) {
				methods = new MethodInfo [] {_get, _set};
			} else if (haveGet) {
				methods = new MethodInfo [] {_get};
			} else if (haveSet) {
				methods = new MethodInfo [] {_set};
			} else {
				//odd
				return;
			}

			string parms = Parameters.GetSignature (methods [0].GetParameters ());
			AddAttribute (p, "params", parms);

			MethodData data = new MethodData (document, p, methods);
			data.NoMemberAttributes = true;
			data.DoOutput ();
		}

		protected override string GetMemberAttributes (MemberInfo member)
		{
			PropertyInfo prop = (PropertyInfo) member;
			return ((int) prop.Attributes).ToString (CultureInfo.InvariantCulture);
		}

		public override string ParentTag {
			get { return "properties"; }
		}

		public override string Tag {
			get { return "property"; }
		}
	}

	class EventData : MemberData
	{
		public EventData (XmlDocument document, XmlNode parent, EventInfo [] members)
			: base (document, parent, members)
		{
		}

		protected override string GetName (MemberInfo member)
		{
			EventInfo evt = (EventInfo) member;
			return evt.Name;
		}

		protected override string GetMemberAttributes (MemberInfo member)
		{
			EventInfo evt = (EventInfo) member;
			return ((int) evt.Attributes).ToString (CultureInfo.InvariantCulture);
		}

		protected override void AddExtraData (XmlNode p, MemberInfo member)
		{
			base.AddExtraData (p, member);
			EventInfo evt = (EventInfo) member;
			AddAttribute (p, "eventtype", evt.EventHandlerType.FullName);
		}

		public override string ParentTag {
			get { return "events"; }
		}

		public override string Tag {
			get { return "event"; }
		}
	}

	class MethodData : MemberData
	{
		bool noAtts;

		public MethodData (XmlDocument document, XmlNode parent, MethodBase [] members)
			: base (document, parent, members)
		{
		}

		protected override string GetName (MemberInfo member)
		{
			MethodBase method = (MethodBase) member;
			string name = method.Name;
			string parms = Parameters.GetSignature (method.GetParameters ());
			return String.Format ("{0}({1})", name, parms);
		}

		protected override string GetMemberAttributes (MemberInfo member)
		{
			MethodBase method = (MethodBase) member;
			return ((int) method.Attributes).ToString (CultureInfo.InvariantCulture);
		}

		protected override void AddExtraData (XmlNode p, MemberInfo member)
		{
			base.AddExtraData (p, member);

			ParameterData parms = new ParameterData (document, p, 
				((MethodBase) member).GetParameters ());
			parms.DoOutput ();

			if (!(member is MethodInfo))
				return;

			MethodInfo method = (MethodInfo) member;
			AddAttribute (p, "returntype", method.ReturnType.FullName);

			AttributeData.OutputAttributes (document, p,
				method.ReturnTypeCustomAttributes.GetCustomAttributes (false));
		}

		public override bool NoMemberAttributes {
			get { return noAtts; }
			set { noAtts = value; }
		}
		
		public override string ParentTag {
			get { return "methods"; }
		}

		public override string Tag {
			get { return "method"; }
		}
	}

	class ConstructorData : MethodData
	{
		public ConstructorData (XmlDocument document, XmlNode parent, ConstructorInfo [] members)
			: base (document, parent, members)
		{
		}

		public override string ParentTag {
			get { return "constructors"; }
		}

		public override string Tag {
			get { return "constructor"; }
		}
	}

	class ParameterData : BaseData
	{
		private ParameterInfo[] parameters;

		public ParameterData (XmlDocument document, XmlNode parent, ParameterInfo[] parameters)
			: base (document, parent)
		{
			this.parameters = parameters;
		}

		public override void DoOutput ()
		{
			XmlNode parametersNode = document.CreateElement ("parameters", null);
			parent.AppendChild (parametersNode);

			foreach (ParameterInfo parameter in parameters) {
				XmlNode paramNode = document.CreateElement ("parameter", null);
				parametersNode.AppendChild (paramNode);
				AddAttribute (paramNode, "name", parameter.Name);
				AddAttribute (paramNode, "position", parameter.Position.ToString(CultureInfo.InvariantCulture));
				AddAttribute (paramNode, "attrib", ((int) parameter.Attributes).ToString());

				string direction = "in";

				if (parameter.ParameterType.IsByRef) {
					direction = parameter.IsOut ? "out" : "ref";
				}

				Type t = parameter.ParameterType;
				AddAttribute (paramNode, "type", t.FullName);

				if (parameter.IsOptional) {
					AddAttribute (paramNode, "optional", "true");
					if (parameter.DefaultValue != null)
						AddAttribute (paramNode, "defaultValue", parameter.DefaultValue.ToString ());
				}

				if (direction != "in")
					AddAttribute (paramNode, "direction", direction);

				AttributeData.OutputAttributes (document, paramNode, parameter.GetCustomAttributes (false));
			}
		}
	}

	class AttributeData : BaseData
	{
		object [] atts;
		string target;

		AttributeData (XmlDocument doc, XmlNode parent, object[] attributes, string target)
			: base (doc, parent)
		{
			atts = attributes;
			this.target = target;
		}

		AttributeData (XmlDocument doc, XmlNode parent, object [] attributes)
			: this (doc, parent, attributes, null)
		{
		}

		public override void DoOutput ()
		{
			if (document == null)
				throw new InvalidOperationException ("Document not set");

			if (atts == null || atts.Length == 0)
				return;

			XmlNode natts = parent.SelectSingleNode("attributes");
			if (natts == null) {
				natts = document.CreateElement ("attributes", null);
				parent.AppendChild (natts);
			}

			ArrayList typeList = new ArrayList (atts.Length);
			string comment = null;
			for (int i = atts.Length - 1; i >= 0; i--) {
				Type attType = atts [i].GetType ();
				if (!MustDocumentAttribute (attType))
					continue;
				typeList.Add (attType);
				if (attType.Name.EndsWith ("TODOAttribute")) {
					PropertyInfo prop = attType.GetProperty ("Comment");
					if (prop != null)
						comment = (string) prop.GetValue (atts [i], null);
				}
			}

			Type[] types = (Type[]) typeList.ToArray (typeof (Type));
			Array.Sort (types, TypeComparer.Default);
			foreach (Type t in types) {
				XmlNode node = document.CreateElement ("attribute");
				AddAttribute (node, "name", t.FullName);
				if (target != null) {
					AddAttribute (node, "target", target);
				}
				if (comment != null && t.Name.EndsWith ("TODOAttribute"))
					AddAttribute (node, "comment", comment);

				natts.AppendChild (node);
			}
		}

		public static void OutputAttributes (XmlDocument doc, XmlNode parent, object[] attributes)
		{
			AttributeData ad = new AttributeData (doc, parent, attributes, null);
			ad.DoOutput ();
		}

		public static void OutputAttributes (XmlDocument doc, XmlNode parent, object [] attributes, string target)
		{
			AttributeData ad = new AttributeData (doc, parent, attributes, target);
			ad.DoOutput ();
		}

		private static bool MustDocumentAttribute (Type attributeType)
		{
			// only document MonoTODOAttribute and public attributes
			return attributeType.Name.EndsWith ("TODOAttribute") || attributeType.IsPublic;
		}
	}

	class Parameters
	{
		private Parameters () {}

		public static string GetSignature (ParameterInfo [] infos)
		{
			if (infos == null || infos.Length == 0)
				return "";

			StringBuilder sb = new StringBuilder ();
			foreach (ParameterInfo info in infos) {
				string modifier;
				if (info.IsIn)
					modifier = "in ";
				else if (info.IsRetval)
					modifier = "ref ";
				else if (info.IsOut)
					modifier = "out ";
				else
					modifier = "";

				string type_name = info.ParameterType.ToString ();
				sb.AppendFormat ("{0}{1}, ", modifier, type_name);
			}

			sb.Length -= 2; // remove ", "
			return sb.ToString ();
		}

	}
	
	class TypeComparer : IComparer
	{
		public static TypeComparer Default = new TypeComparer ();

		public int Compare (object a, object b)
		{
			Type ta = (Type) a;
			Type tb = (Type) b;
			int result = String.Compare (ta.Namespace, tb.Namespace);
			if (result != 0)
				return result;

			return String.Compare (ta.Name, tb.Name);
		}
	}

	class MemberInfoComparer : IComparer
	{
		public static MemberInfoComparer Default = new MemberInfoComparer ();

		public int Compare (object a, object b)
		{
			MemberInfo ma = (MemberInfo) a;
			MemberInfo mb = (MemberInfo) b;
			return String.Compare (ma.Name, mb.Name);
		}
	}

	class MethodBaseComparer : IComparer
	{
		public static MethodBaseComparer Default = new MethodBaseComparer ();

		public int Compare (object a, object b)
		{
			MethodBase ma = (MethodBase) a;
			MethodBase mb = (MethodBase) b;
			int res = String.Compare (ma.Name, mb.Name);
			if (res != 0)
				return res;

			ParameterInfo [] pia = ma.GetParameters ();
			ParameterInfo [] pib = mb.GetParameters ();
			if (pia.Length != pib.Length)
				return pia.Length - pib.Length;

			string siga = Parameters.GetSignature (pia);
			string sigb = Parameters.GetSignature (pib);
			return String.Compare (siga, sigb);
		}
	}
}

