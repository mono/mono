//
// mono-api-info.cs - Dumps public assembly information to an xml file.
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) 2003-2008 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Xml;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CorCompare
{
	public class Driver
	{
		public static int Main (string [] args)
		{
			if (args.Length == 0)
				return 1;

			AbiMode = false;

			AssemblyCollection acoll = new AssemblyCollection ();

			foreach (string arg in args) {
				if (arg == "--abi")
					AbiMode = true;
				else
					acoll.Add (arg);
			}

			XmlDocument doc = new XmlDocument ();
			acoll.Document = doc;
			acoll.DoOutput ();

			var writer = new WellFormedXmlWriter (new XmlTextWriter (Console.Out) { Formatting = Formatting.Indented });
			XmlNode decl = doc.CreateXmlDeclaration ("1.0", "utf-8", null);
			doc.InsertBefore (decl, doc.DocumentElement);
			doc.WriteTo (writer);
			return 0;
		}

		internal static bool AbiMode { get; private set; }
	}

	public class Utils {

		public static string CleanupTypeName (TypeReference type)
		{
			return CleanupTypeName (type.FullName);
		}

		static string CleanupTypeName (string t)
		{
			return t.Replace ('<', '[').Replace ('>', ']').Replace ('/', '+');
		}
	}

	class AssemblyCollection
	{
		XmlDocument document;
		List<AssemblyDefinition> assemblies = new List<AssemblyDefinition> ();

		public AssemblyCollection ()
		{
		}

		public bool Add (string name)
		{
			AssemblyDefinition ass = LoadAssembly (name);
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
			foreach (AssemblyDefinition a in assemblies) {
				AssemblyData data = new AssemblyData (document, nassemblies, a);
				data.DoOutput ();
			}
		}

		public XmlDocument Document {
			set { document = value; }
		}

		AssemblyDefinition LoadAssembly (string assembly)
		{
			try {
				return TypeHelper.Resolver.Resolve (assembly);
			} catch {
				return null;
			}
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

	class TypeForwardedToData : BaseData
	{
		AssemblyDefinition ass;

		public TypeForwardedToData (XmlDocument document, XmlNode parent, AssemblyDefinition ass)
			: base (document, parent)
		{
			this.ass = ass;
		}

		public override void DoOutput ()
		{
			XmlNode natts = parent.SelectSingleNode("attributes");
			if (natts == null) {
				natts = document.CreateElement ("attributes", null);
				parent.AppendChild (natts);
			}
			
			foreach (TypeReference tref in ass.MainModule.ExternTypes) {
				TypeDefinition def = tref.Resolve ();
				if (def == null)
					continue;

				if (((uint)def.Attributes & 0x200000u) == 0)
					continue;

				XmlNode node = document.CreateElement ("attribute");
				AddAttribute (node, "name", typeof (TypeForwardedToAttribute).FullName);
				XmlNode properties = node.AppendChild (document.CreateElement ("properties"));
				XmlNode property = properties.AppendChild (document.CreateElement ("property"));
				AddAttribute (property, "name", "Destination");
				AddAttribute (property, "value", Utils.CleanupTypeName (tref));
				natts.AppendChild (node);
			}
		}

		public static void OutputForwarders (XmlDocument document, XmlNode parent, AssemblyDefinition ass)
		{
			TypeForwardedToData tftd = new TypeForwardedToData (document, parent, ass);
			tftd.DoOutput ();
		}
	}
	
	class AssemblyData : BaseData
	{
		AssemblyDefinition ass;

		public AssemblyData (XmlDocument document, XmlNode parent, AssemblyDefinition ass)
			: base (document, parent)
		{
			this.ass = ass;
		}

		public override void DoOutput ()
		{
			if (document == null)
				throw new InvalidOperationException ("Document not set");

			XmlNode nassembly = document.CreateElement ("assembly", null);
			AssemblyNameDefinition aname = ass.Name;
			AddAttribute (nassembly, "name", aname.Name);
			AddAttribute (nassembly, "version", aname.Version.ToString ());
			parent.AppendChild (nassembly);
			TypeForwardedToData.OutputForwarders (document, nassembly, ass);
			AttributeData.OutputAttributes (document, nassembly, ass.CustomAttributes);
			TypeDefinitionCollection typesCollection = ass.MainModule.Types;
			if (typesCollection == null || typesCollection.Count == 0)
				return;
			object [] typesArray = new object [typesCollection.Count];
			for (int i = 0; i < typesCollection.Count; i++) {
				typesArray [i] = typesCollection [i];
			}
			Array.Sort (typesArray, TypeReferenceComparer.Default);

			XmlNode nss = document.CreateElement ("namespaces", null);
			nassembly.AppendChild (nss);

			string current_namespace = "$%&$&";
			XmlNode ns = null;
			XmlNode classes = null;
			foreach (TypeDefinition t in typesArray) {
				if (string.IsNullOrEmpty (t.Namespace))
					continue;

				if (!Driver.AbiMode && ((t.Attributes & TypeAttributes.VisibilityMask) != TypeAttributes.Public))
					continue;

				if (t.DeclaringType != null)
					continue; // enforce !nested

				if (t.Namespace != current_namespace) {
					current_namespace = t.Namespace;
					ns = document.CreateElement ("namespace", null);
					AddAttribute (ns, "name", current_namespace);
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
		MemberReference [] members;

		public MemberData (XmlDocument document, XmlNode parent, MemberReference [] members)
			: base (document, parent)
		{
			this.members = members;
		}

		public override void DoOutput ()
		{
			XmlNode mclass = document.CreateElement (ParentTag, null);
			parent.AppendChild (mclass);

			foreach (MemberReference member in members) {
				XmlNode mnode = document.CreateElement (Tag, null);
				mclass.AppendChild (mnode);
				AddAttribute (mnode, "name", GetName (member));
				if (!NoMemberAttributes)
					AddAttribute (mnode, "attrib", GetMemberAttributes (member));

				AttributeData.OutputAttributes (document, mnode, GetCustomAttributes (member));

				AddExtraData (mnode, member);
			}
		}


		protected abstract CustomAttributeCollection GetCustomAttributes (MemberReference member);

		protected virtual void AddExtraData (XmlNode p, MemberReference memberDefenition)
		{
		}

		protected virtual string GetName (MemberReference memberDefenition)
		{
			return "NoNAME";
		}

		protected virtual string GetMemberAttributes (MemberReference memberDefenition)
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

		public static void OutputGenericParameters (XmlDocument document, XmlNode nclass, IGenericParameterProvider provider)
		{
			if (provider.GenericParameters.Count == 0)
				return;

			var gparameters = provider.GenericParameters;

			XmlElement ngeneric = document.CreateElement (string.Format ("generic-parameters"));
			nclass.AppendChild (ngeneric);

			foreach (GenericParameter gp in gparameters) {
				XmlElement nparam = document.CreateElement (string.Format ("generic-parameter"));
				nparam.SetAttribute ("name", gp.Name);
				nparam.SetAttribute ("attributes", ((int) gp.Attributes).ToString ());

				AttributeData.OutputAttributes (document, nparam, gp.CustomAttributes);

				ngeneric.AppendChild (nparam);

				var constraints = gp.Constraints;
				if (constraints.Count == 0)
					continue;

				XmlElement nconstraint = document.CreateElement ("generic-parameter-constraints");

				foreach (TypeReference constraint in constraints) {
					XmlElement ncons = document.CreateElement ("generic-parameter-constraint");
					ncons.SetAttribute ("name", Utils.CleanupTypeName (constraint));
					nconstraint.AppendChild (ncons);
				}

				nparam.AppendChild (nconstraint);
			}
		}
	}

	class TypeData : MemberData
	{
		TypeDefinition type;

		public TypeData (XmlDocument document, XmlNode parent, TypeDefinition type)
			: base (document, parent, null)
		{
			this.type = type;
		}

		protected override CustomAttributeCollection GetCustomAttributes (MemberReference member) {
			return ((TypeDefinition) member).CustomAttributes;
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
				AddAttribute (nclass, "base", Utils.CleanupTypeName (type.BaseType));

			if (type.IsSealed)
				AddAttribute (nclass, "sealed", "true");

			if (type.IsAbstract)
				AddAttribute (nclass, "abstract", "true");

			if ( (type.Attributes & TypeAttributes.Serializable) != 0 || type.IsEnum)
				AddAttribute (nclass, "serializable", "true");

			string charSet = GetCharSet (type);
			AddAttribute (nclass, "charset", charSet);

			string layout = GetLayout (type);
			if (layout != null)
				AddAttribute (nclass, "layout", layout);

			parent.AppendChild (nclass);

			AttributeData.OutputAttributes (document, nclass, GetCustomAttributes(type));

			XmlNode ifaces = null;

			foreach (TypeReference iface in  TypeHelper.GetInterfaces (type)) {
				if (!TypeHelper.IsPublic (iface))
					// we're only interested in public interfaces
					continue;

				if (ifaces == null) {
					ifaces = document.CreateElement ("interfaces", null);
					nclass.AppendChild (ifaces);
				}

				XmlNode iface_node = document.CreateElement ("interface", null);
				AddAttribute (iface_node, "name", Utils.CleanupTypeName (iface));
				ifaces.AppendChild (iface_node);
			}

			MemberData.OutputGenericParameters (document, nclass, type);

			ArrayList members = new ArrayList ();

			FieldDefinition [] fields = GetFields (type);
			if (fields.Length > 0) {
				Array.Sort (fields, MemberReferenceComparer.Default);
				FieldData fd = new FieldData (document, nclass, fields);
				members.Add (fd);
			}

			if (type.IsEnum) {
				var value_type = GetEnumValueField (type);
				if (value_type == null)
					throw new NotSupportedException ();

				AddAttribute (nclass, "enumtype", Utils.CleanupTypeName (value_type.FieldType));
			}

			if (!Driver.AbiMode) {

				MethodDefinition [] ctors = GetConstructors (type);
				if (ctors.Length > 0) {
					Array.Sort (ctors, MemberReferenceComparer.Default);
					members.Add (new ConstructorData (document, nclass, ctors));
				}

				PropertyDefinition[] properties = GetProperties (type);
				if (properties.Length > 0) {
					Array.Sort (properties, MemberReferenceComparer.Default);
					members.Add (new PropertyData (document, nclass, properties));
				}

				EventDefinition [] events = GetEvents (type);
				if (events.Length > 0) {
					Array.Sort (events, MemberReferenceComparer.Default);
					members.Add (new EventData (document, nclass, events));
				}

				MethodDefinition [] methods = GetMethods (type);
				if (methods.Length > 0) {
					Array.Sort (methods, MemberReferenceComparer.Default);
					members.Add (new MethodData (document, nclass, methods));
				}
			}

			foreach (MemberData md in members)
				md.DoOutput ();

			NestedTypeCollection nested = type.NestedTypes;
			//remove non public(familiy) and nested in second degree
			for (int i = nested.Count - 1; i >= 0; i--) {
				TypeDefinition t = nested [i];
				if ((t.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic ||
					(t.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamily ||
					(t.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamORAssem) {
					// public
					if (t.DeclaringType == type)
						continue; // not nested of nested
				}

				nested.RemoveAt (i);
			}


			if (nested.Count > 0) {
				XmlNode classes = document.CreateElement ("classes", null);
				nclass.AppendChild (classes);
				foreach (TypeDefinition t in nested) {
					TypeData td = new TypeData (document, classes, t);
					td.DoOutput ();
				}
			}
		}

		static FieldReference GetEnumValueField (TypeDefinition type)
		{
			foreach (FieldDefinition field in type.Fields)
				if (field.IsSpecialName && field.Name == "value__")
					return field;

			return null;
		}

		protected override string GetMemberAttributes (MemberReference member)
		{
			if (member != type)
				throw new InvalidOperationException ("odd");

			return ((int) type.Attributes).ToString (CultureInfo.InvariantCulture);
		}

		public static bool MustDocumentMethod (MethodDefinition method) {
			// All other methods
			MethodAttributes maskedAccess = method.Attributes & MethodAttributes.MemberAccessMask;
			return maskedAccess == MethodAttributes.Public
				|| maskedAccess == MethodAttributes.Family
				|| maskedAccess == MethodAttributes.FamORAssem;
		}

		static string GetClassType (TypeDefinition t)
		{
			if (t.IsEnum)
				return "enum";

			if (t.IsValueType)
				return "struct";

			if (t.IsInterface)
				return "interface";

			if (TypeHelper.IsDelegate(t))
				return "delegate";

			return "class";
		}

		static string GetCharSet (TypeDefinition type)
		{
			TypeAttributes maskedStringFormat = type.Attributes & TypeAttributes.StringFormatMask;
			if (maskedStringFormat == TypeAttributes.AnsiClass)
				return CharSet.Ansi.ToString ();

			if (maskedStringFormat == TypeAttributes.AutoClass)
				return CharSet.Auto.ToString ();

			if (maskedStringFormat == TypeAttributes.UnicodeClass)
				return CharSet.Unicode.ToString ();

			return CharSet.None.ToString ();
		}

		static string GetLayout (TypeDefinition type)
		{
			TypeAttributes maskedLayout = type.Attributes & TypeAttributes.LayoutMask;
			if (maskedLayout == TypeAttributes.AutoLayout)
				return LayoutKind.Auto.ToString ();

			if (maskedLayout == TypeAttributes.ExplicitLayout)
				return LayoutKind.Explicit.ToString ();

			if (maskedLayout == TypeAttributes.SequentialLayout)
				return LayoutKind.Sequential.ToString ();

			return null;
		}

		FieldDefinition [] GetFields (TypeDefinition type) {
			ArrayList list = new ArrayList ();

			FieldDefinitionCollection fields = type.Fields;
			foreach (FieldDefinition field in fields) {
				if (field.IsSpecialName)
					continue;

				if (Driver.AbiMode && field.IsStatic)
					continue;

				// we're only interested in public or protected members
				FieldAttributes maskedVisibility = (field.Attributes & FieldAttributes.FieldAccessMask);
				if (Driver.AbiMode && !field.IsNotSerialized) {
					list.Add (field);
				} else {
					if (maskedVisibility == FieldAttributes.Public
						|| maskedVisibility == FieldAttributes.Family
						|| maskedVisibility == FieldAttributes.FamORAssem) {
						list.Add (field);
					}
				}
			}

			return (FieldDefinition []) list.ToArray (typeof (FieldDefinition));
		}


		internal static PropertyDefinition [] GetProperties (TypeDefinition type) {
			ArrayList list = new ArrayList ();

			PropertyDefinitionCollection properties = type.Properties;//type.GetProperties (flags);
			foreach (PropertyDefinition property in properties) {
				MethodDefinition getMethod = property.GetMethod;
				MethodDefinition setMethod = property.SetMethod;

				bool hasGetter = (getMethod != null) && MustDocumentMethod (getMethod);
				bool hasSetter = (setMethod != null) && MustDocumentMethod (setMethod);

				// if neither the getter or setter should be documented, then
				// skip the property
				if (hasGetter || hasSetter) {
					list.Add (property);
				}
			}

			return (PropertyDefinition []) list.ToArray (typeof (PropertyDefinition));
		}

		private MethodDefinition[] GetMethods (TypeDefinition type)
		{
			ArrayList list = new ArrayList ();

			MethodDefinitionCollection methods = type.Methods;//type.GetMethods (flags);
			foreach (MethodDefinition method in methods) {
				if (method.IsSpecialName && !method.Name.StartsWith ("op_"))
					continue;

				// we're only interested in public or protected members
				if (!MustDocumentMethod(method))
					continue;

				if (IsFinalizer (method))
					continue;

				list.Add (method);
			}

			return (MethodDefinition []) list.ToArray (typeof (MethodDefinition));
		}

		static bool IsFinalizer (MethodDefinition method)
		{
			if (method.Name != "Finalize")
				return false;

			if (!method.IsVirtual)
				return false;

			if (method.Parameters.Count != 0)
				return false;

			return true;
		}

		private MethodDefinition [] GetConstructors (TypeDefinition type)
		{
			ArrayList list = new ArrayList ();

			ConstructorCollection ctors = type.Constructors;//type.GetConstructors (flags);
			foreach (MethodDefinition constructor in ctors) {
				// we're only interested in public or protected members
				if (!MustDocumentMethod(constructor))
					continue;

				list.Add (constructor);
			}

			return (MethodDefinition []) list.ToArray (typeof (MethodDefinition));
		}

		private EventDefinition[] GetEvents (TypeDefinition type)
		{
			ArrayList list = new ArrayList ();

			EventDefinitionCollection events = type.Events;//type.GetEvents (flags);
			foreach (EventDefinition eventDef in events) {
				MethodDefinition addMethod = eventDef.AddMethod;//eventInfo.GetAddMethod (true);

				if (addMethod == null || !MustDocumentMethod (addMethod))
					continue;

				list.Add (eventDef);
			}

			return (EventDefinition []) list.ToArray (typeof (EventDefinition));
		}
	}

	class FieldData : MemberData
	{
		public FieldData (XmlDocument document, XmlNode parent, FieldDefinition [] members)
			: base (document, parent, members)
		{
		}

		protected override CustomAttributeCollection GetCustomAttributes (MemberReference member) {
			return ((FieldDefinition) member).CustomAttributes;
		}

		protected override string GetName (MemberReference memberDefenition)
		{
			FieldDefinition field = (FieldDefinition) memberDefenition;
			return field.Name;
		}

		protected override string GetMemberAttributes (MemberReference memberDefenition)
		{
			FieldDefinition field = (FieldDefinition) memberDefenition;
			return ((int) field.Attributes).ToString (CultureInfo.InvariantCulture);
		}

		protected override void AddExtraData (XmlNode p, MemberReference memberDefenition)
		{
			base.AddExtraData (p, memberDefenition);
			FieldDefinition field = (FieldDefinition) memberDefenition;
			AddAttribute (p, "fieldtype", Utils.CleanupTypeName (field.FieldType));

			if (field.IsLiteral) {
				object value = field.Constant;//object value = field.GetValue (null);
				string stringValue = null;
				//if (value is Enum) {
				//    // FIXME: when Mono bug #60090 has been
				//    // fixed, we should just be able to use
				//    // Convert.ToString
				//    stringValue = ((Enum) value).ToString ("D", CultureInfo.InvariantCulture);
				//}
				//else {
					stringValue = Convert.ToString (value, CultureInfo.InvariantCulture);
				//}

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
		public PropertyData (XmlDocument document, XmlNode parent, PropertyDefinition [] members)
			: base (document, parent, members)
		{
		}

		protected override CustomAttributeCollection GetCustomAttributes (MemberReference member) {
			return ((PropertyDefinition) member).CustomAttributes;
		}

		protected override string GetName (MemberReference memberDefenition)
		{
			PropertyDefinition prop = (PropertyDefinition) memberDefenition;
			return prop.Name;
		}

		protected override void AddExtraData (XmlNode p, MemberReference memberDefenition)
		{
			base.AddExtraData (p, memberDefenition);
			PropertyDefinition prop = (PropertyDefinition) memberDefenition;
			AddAttribute (p, "ptype", Utils.CleanupTypeName (prop.PropertyType));
			MethodDefinition _get = prop.GetMethod;
			MethodDefinition _set = prop.SetMethod;
			bool haveGet = (_get != null && TypeData.MustDocumentMethod(_get));
			bool haveSet = (_set != null && TypeData.MustDocumentMethod(_set));
			MethodDefinition [] methods;

			if (haveGet && haveSet) {
				methods = new MethodDefinition [] { _get, _set };
			} else if (haveGet) {
				methods = new MethodDefinition [] { _get };
			} else if (haveSet) {
				methods = new MethodDefinition [] { _set };
			} else {
				//odd
				return;
			}

			string parms = Parameters.GetSignature (methods [0].Parameters);
			if (!string.IsNullOrEmpty (parms))
				AddAttribute (p, "params", parms);

			MethodData data = new MethodData (document, p, methods);
			//data.NoMemberAttributes = true;
			data.DoOutput ();
		}

		protected override string GetMemberAttributes (MemberReference memberDefenition)
		{
			PropertyDefinition prop = (PropertyDefinition) memberDefenition;
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
		public EventData (XmlDocument document, XmlNode parent, EventDefinition [] members)
			: base (document, parent, members)
		{
		}

		protected override CustomAttributeCollection GetCustomAttributes (MemberReference member) {
			return ((EventDefinition) member).CustomAttributes;
		}

		protected override string GetName (MemberReference memberDefenition)
		{
			EventDefinition evt = (EventDefinition) memberDefenition;
			return evt.Name;
		}

		protected override string GetMemberAttributes (MemberReference memberDefenition)
		{
			EventDefinition evt = (EventDefinition) memberDefenition;
			return ((int) evt.Attributes).ToString (CultureInfo.InvariantCulture);
		}

		protected override void AddExtraData (XmlNode p, MemberReference memberDefenition)
		{
			base.AddExtraData (p, memberDefenition);
			EventDefinition evt = (EventDefinition) memberDefenition;
			AddAttribute (p, "eventtype", Utils.CleanupTypeName (evt.EventType));
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

		public MethodData (XmlDocument document, XmlNode parent, MethodDefinition [] members)
			: base (document, parent, members)
		{
		}

		protected override CustomAttributeCollection GetCustomAttributes (MemberReference member) {
			return ((MethodDefinition) member).CustomAttributes;
		}

		protected override string GetName (MemberReference memberDefenition)
		{
			MethodDefinition method = (MethodDefinition) memberDefenition;
			string name = method.Name;
			string parms = Parameters.GetSignature (method.Parameters);

			return string.Format ("{0}({1})", name, parms);
		}

		protected override string GetMemberAttributes (MemberReference memberDefenition)
		{
			MethodDefinition method = (MethodDefinition) memberDefenition;
			return ((int)( method.Attributes)).ToString (CultureInfo.InvariantCulture);
		}

		protected override void AddExtraData (XmlNode p, MemberReference memberDefenition)
		{
			base.AddExtraData (p, memberDefenition);

			if (!(memberDefenition is MethodDefinition))
				return;

			MethodDefinition mbase = (MethodDefinition) memberDefenition;

			ParameterData parms = new ParameterData (document, p, mbase.Parameters);
			parms.DoOutput ();

			if (mbase.IsAbstract)
				AddAttribute (p, "abstract", "true");
			if (mbase.IsVirtual)
				AddAttribute (p, "virtual", "true");
			if (mbase.IsStatic)
				AddAttribute (p, "static", "true");

			string rettype = Utils.CleanupTypeName (mbase.ReturnType.ReturnType);
			if (rettype != "System.Void" || !mbase.IsConstructor)
				AddAttribute (p, "returntype", (rettype));

			AttributeData.OutputAttributes (document, p, mbase.ReturnType.CustomAttributes);

			MemberData.OutputGenericParameters (document, p, mbase);
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
		public ConstructorData (XmlDocument document, XmlNode parent, MethodDefinition [] members)
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
		private ParameterDefinitionCollection parameters;

		public ParameterData (XmlDocument document, XmlNode parent, ParameterDefinitionCollection parameters)
			: base (document, parent)
		{
			this.parameters = parameters;
		}

		public override void DoOutput ()
		{
			XmlNode parametersNode = document.CreateElement ("parameters");
			parent.AppendChild (parametersNode);

			foreach (ParameterDefinition parameter in parameters) {
				XmlNode paramNode = document.CreateElement ("parameter");
				parametersNode.AppendChild (paramNode);
				AddAttribute (paramNode, "name", parameter.Name);
				AddAttribute (paramNode, "position", parameter.Method.Parameters.IndexOf(parameter).ToString(CultureInfo.InvariantCulture));
				AddAttribute (paramNode, "attrib", ((int) parameter.Attributes).ToString());

				string direction = "in";

				if (parameter.ParameterType is ReferenceType)
					direction = parameter.IsOut ? "out" : "ref";

				TypeReference t = parameter.ParameterType;
				AddAttribute (paramNode, "type", Utils.CleanupTypeName (t));

				if (parameter.IsOptional) {
					AddAttribute (paramNode, "optional", "true");
					if (parameter.HasConstant)
						AddAttribute (paramNode, "defaultValue", parameter.Constant == null ? "NULL" : parameter.Constant.ToString ());
				}

				if (direction != "in")
					AddAttribute (paramNode, "direction", direction);

				AttributeData.OutputAttributes (document, paramNode, parameter.CustomAttributes);
			}
		}
	}

	class AttributeData : BaseData
	{
		CustomAttributeCollection atts;

		AttributeData (XmlDocument doc, XmlNode parent, CustomAttributeCollection attributes)
			: base (doc, parent)
		{
			atts = attributes;
		}

		public override void DoOutput ()
		{
			if (document == null)
				throw new InvalidOperationException ("Document not set");

			if (atts == null || atts.Count == 0)
				return;

			XmlNode natts = parent.SelectSingleNode("attributes");
			if (natts == null) {
				natts = document.CreateElement ("attributes", null);
				parent.AppendChild (natts);
			}

			for (int i = 0; i < atts.Count; ++i) {
				CustomAttribute att = atts [i];
				try {
					att.Resolve ();
				} catch {}

				if (!att.Resolved)
					continue;

				string attName = Utils.CleanupTypeName (att.Constructor.DeclaringType);
				if (SkipAttribute (att))
					continue;

				XmlNode node = document.CreateElement ("attribute");
				AddAttribute (node, "name", attName);

				XmlNode properties = null;

				Dictionary<string, object> attribute_mapping = CreateAttributeMapping (att);

				foreach (string name in attribute_mapping.Keys) {
					if (name == "TypeId")
						continue;

					if (properties == null) {
						properties = node.AppendChild (document.CreateElement ("properties"));
					}

					object o = attribute_mapping [name];

					XmlNode n = properties.AppendChild (document.CreateElement ("property"));
					AddAttribute (n, "name", name);

					if (o == null) {
						AddAttribute (n, "value", "null");
						continue;
					}
					string value = o.ToString ();
					if (attName.EndsWith ("GuidAttribute"))
						value = value.ToUpper ();
					AddAttribute (n, "value", value);
				}

				natts.AppendChild (node);
			}
		}

		static Dictionary<string, object> CreateAttributeMapping (CustomAttribute attribute)
		{
			var mapping = new Dictionary<string, object> ();

			PopulateMapping (mapping, attribute);

			var constructor = attribute.Constructor.Resolve ();
			if (constructor == null || constructor.Parameters.Count == 0)
				return mapping;

			PopulateMapping (mapping, constructor, attribute);

			return mapping;
		}

		static void PopulateMapping (Dictionary<string, object> mapping, CustomAttribute attribute)
		{
			foreach (DictionaryEntry entry in attribute.Properties) {
				var name = (string) entry.Key;

				mapping.Add (name, GetArgumentValue (attribute.GetPropertyType (name), entry.Value));
			}
		}

		static Dictionary<FieldReference, int> CreateArgumentFieldMapping (MethodDefinition constructor)
		{
			Dictionary<FieldReference, int> field_mapping = new Dictionary<FieldReference, int> ();

			int? argument = null;

			foreach (Instruction instruction in constructor.Body.Instructions) {
				switch (instruction.OpCode.Code) {
				case Code.Ldarg_1:
					argument = 1;
					break;
				case Code.Ldarg_2:
					argument = 2;
					break;
				case Code.Ldarg_3:
					argument = 3;
					break;
				case Code.Ldarg:
				case Code.Ldarg_S:
					argument = ((ParameterDefinition) instruction.Operand).Sequence;
					break;

				case Code.Stfld:
					FieldReference field = (FieldReference) instruction.Operand;
					if (field.DeclaringType.FullName != constructor.DeclaringType.FullName)
						continue;

					if (!argument.HasValue)
						break;

					if (!field_mapping.ContainsKey (field))
						field_mapping.Add (field, (int) argument - 1);

					argument = null;
					break;
				}
			}

			return field_mapping;
		}

		static Dictionary<PropertyDefinition, FieldReference> CreatePropertyFieldMapping (TypeDefinition type)
		{
			Dictionary<PropertyDefinition, FieldReference> property_mapping = new Dictionary<PropertyDefinition, FieldReference> ();

			foreach (PropertyDefinition property in type.Properties) {
				if (property.GetMethod == null)
					continue;
				if (!property.GetMethod.HasBody)
					continue;

				foreach (Instruction instruction in property.GetMethod.Body.Instructions) {
					if (instruction.OpCode.Code != Code.Ldfld)
						continue;

					FieldReference field = (FieldReference) instruction.Operand;
					if (field.DeclaringType.FullName != type.FullName)
						continue;

					property_mapping.Add (property, field);
					break;
				}
			}

			return property_mapping;
		}

		static void PopulateMapping (Dictionary<string, object> mapping, MethodDefinition constructor, CustomAttribute attribute)
		{
			if (!constructor.HasBody)
				return;

			var field_mapping = CreateArgumentFieldMapping (constructor);
			var property_mapping = CreatePropertyFieldMapping ((TypeDefinition) constructor.DeclaringType);

			foreach (var pair in property_mapping) {
				int argument;
				if (!field_mapping.TryGetValue (pair.Value, out argument))
					continue;

				mapping.Add (pair.Key.Name, GetArgumentValue (constructor.Parameters [argument].ParameterType, attribute.ConstructorParameters [argument]));
			}
		}

		static object GetArgumentValue (TypeReference reference, object value)
		{
			var type = reference.Resolve ();
			if (type == null)
				return value;

			if (type.IsEnum) {
				if (IsFlaggedEnum (type))
					return GetFlaggedEnumValue (type, value);

				return GetEnumValue (type, value);
			}

			return value;
		}

		static bool IsFlaggedEnum (TypeDefinition type)
		{
			if (!type.IsEnum)
				return false;

			if (type.CustomAttributes.Count == 0)
				return false;

			foreach (CustomAttribute attribute in type.CustomAttributes)
				if (attribute.Constructor.DeclaringType.FullName == "System.FlagsAttribute")
					return true;

			return false;
		}

		static object GetFlaggedEnumValue (TypeDefinition type, object value)
		{
			long flags = Convert.ToInt64 (value);
			var signature = new StringBuilder ();

			for (int i = type.Fields.Count - 1; i >= 0; i--) {
				FieldDefinition field = type.Fields [i];

				if (!field.HasConstant)
					continue;

				long flag = Convert.ToInt64 (field.Constant);

				if (flag == 0)
					continue;

				if ((flags & flag) == flag) {
					if (signature.Length != 0)
						signature.Append (", ");

					signature.Append (field.Name);
					flags -= flag;
				}
			}

			return signature.ToString ();
		}

		static object GetEnumValue (TypeDefinition type, object value)
		{
			foreach (FieldDefinition field in type.Fields) {
				if (!field.HasConstant)
					continue;

				if (Comparer.Default.Compare (field.Constant, value) == 0)
					return field.Name;
			}

			return value;
		}

		static bool SkipAttribute (CustomAttribute attribute)
		{
			var type_name = Utils.CleanupTypeName (attribute.Constructor.DeclaringType);

			return !TypeHelper.IsPublic (attribute)
				|| type_name.EndsWith ("TODOAttribute");
		}

		public static void OutputAttributes (XmlDocument doc, XmlNode parent, CustomAttributeCollection attributes)
		{
			AttributeData ad = new AttributeData (doc, parent, attributes);
			ad.DoOutput ();
		}
	}

	static class Parameters {

		public static string GetSignature (ParameterDefinitionCollection infos)
		{
			if (infos == null || infos.Count == 0)
				return "";

			var signature = new StringBuilder ();
			for (int i = 0; i < infos.Count; i++) {

				if (i > 0)
					signature.Append (", ");

				ParameterDefinition info = infos [i];

				string modifier;
				if ((info.Attributes & ParameterAttributes.In) != 0)
					modifier = "in";
				else if ((info.Attributes & ParameterAttributes.Retval) != 0)
					modifier = "ref";
				else if ((info.Attributes & ParameterAttributes.Out) != 0)
					modifier = "out";
				else
					modifier = string.Empty;

				if (modifier.Length > 0)
					signature.AppendFormat ("{0} ", modifier);

				signature.Append (Utils.CleanupTypeName (info.ParameterType));
			}

			return signature.ToString ();
		}

	}

	class TypeReferenceComparer : IComparer
	{
		public static TypeReferenceComparer Default = new TypeReferenceComparer ();

		public int Compare (object a, object b)
		{
			TypeReference ta = (TypeReference) a;
			TypeReference tb = (TypeReference) b;
			int result = String.Compare (ta.Namespace, tb.Namespace);
			if (result != 0)
				return result;

			return String.Compare (ta.Name, tb.Name);
		}
	}

	class MemberReferenceComparer : IComparer
	{
		public static MemberReferenceComparer Default = new MemberReferenceComparer ();

		public int Compare (object a, object b)
		{
			MemberReference ma = (MemberReference) a;
			MemberReference mb = (MemberReference) b;
			return String.Compare (ma.Name, mb.Name);
		}
	}

	class MethodDefinitionComparer : IComparer
	{
		public static MethodDefinitionComparer Default = new MethodDefinitionComparer ();

		public int Compare (object a, object b)
		{
			MethodDefinition ma = (MethodDefinition) a;
			MethodDefinition mb = (MethodDefinition) b;
			int res = String.Compare (ma.Name, mb.Name);
			if (res != 0)
				return res;

			ParameterDefinitionCollection pia = ma.Parameters ;
			ParameterDefinitionCollection pib = mb.Parameters;
			res = pia.Count - pib.Count;
			if (res != 0)
				return res;

			string siga = Parameters.GetSignature (pia);
			string sigb = Parameters.GetSignature (pib);
			return String.Compare (siga, sigb);
		}
	}
}

