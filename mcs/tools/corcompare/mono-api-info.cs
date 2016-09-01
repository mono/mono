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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Xml;

using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;

namespace CorCompare
{
	public class Driver
	{
		public static int Main (string [] args)
		{
			bool showHelp = false;
			AbiMode = false;
			FollowForwarders = false;
			string output = null;

			var acoll = new AssemblyCollection ();

			var options = new Mono.Options.OptionSet {
				"usage: mono-api-info [OPTIONS+] ASSEMBLY+",
				"",
				"Expose IL structure of CLR assemblies as XML.",
				"",
				"Available Options:",
				{ "abi",
					"Generate ABI, not API; contains only classes with instance fields which are not [NonSerialized].",
					v => AbiMode = v != null },
				{ "f|follow-forwarders",
					"Follow type forwarders.",
					v => FollowForwarders = v != null },
				{ "d|L|lib|search-directory=",
					"Check for assembly references in {DIRECTORY}.",
					v => TypeHelper.Resolver.AddSearchDirectory (v) },
				{ "r=",
					"Read and register the file {ASSEMBLY}, and add the directory containing ASSEMBLY to the search path.",
					v => TypeHelper.Resolver.ResolveFile (v) },
				{ "o=",
					"The output file. If not specified the output will be written to stdout.",
					v => output = v },
				{ "h|?|help",
					"Show this message and exit.",
					v => showHelp = v != null },
				{ "contract-api",
					"Produces contract API with all members at each level of inheritance hierarchy",
					v => FullAPISet = v != null },
			};

			var asms = options.Parse (args);

			if (showHelp || asms.Count == 0) {
				options.WriteOptionDescriptions (Console.Out);
				Console.WriteLine ();
				return showHelp? 0 :1;
			}

			string windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
			string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			TypeHelper.Resolver.AddSearchDirectory (Path.Combine (windir, @"assembly\GAC\MSDATASRC\7.0.3300.0__b03f5f7f11d50a3a"));

			foreach (string arg in asms) {
				acoll.Add (arg);

				if (arg.Contains ("v3.0")) {
					TypeHelper.Resolver.AddSearchDirectory (Path.Combine (windir, @"Microsoft.NET\Framework\v2.0.50727"));
				} else if (arg.Contains ("v3.5")) {
					TypeHelper.Resolver.AddSearchDirectory (Path.Combine (windir, @"Microsoft.NET\Framework\v2.0.50727"));
					TypeHelper.Resolver.AddSearchDirectory (Path.Combine (windir, @"Microsoft.NET\Framework\v3.0\Windows Communication Foundation"));
				} else if (arg.Contains ("v4.0")) {
					if (arg.Contains ("Silverlight")) {
						TypeHelper.Resolver.AddSearchDirectory (Path.Combine (pf, @"Microsoft Silverlight\4.0.51204.0"));
					} else {
						TypeHelper.Resolver.AddSearchDirectory (Path.Combine (windir, @"Microsoft.NET\Framework\v4.0.30319"));
						TypeHelper.Resolver.AddSearchDirectory (Path.Combine (windir, @"Microsoft.NET\Framework\v4.0.30319\WPF"));
					}
				} else {
					TypeHelper.Resolver.AddSearchDirectory (Path.GetDirectoryName (arg));
				}
			}

			StreamWriter outputStream = null;
			if (!string.IsNullOrEmpty (output))
				outputStream = new StreamWriter (output);
			try {
				TextWriter outStream = outputStream ?? Console.Out;
				var settings = new XmlWriterSettings ();
				settings.Indent = true;
				var textWriter = XmlWriter.Create (outStream, settings);
				var writer = new WellFormedXmlWriter (textWriter);
				writer.WriteStartDocument ();
				acoll.Writer = writer;
				acoll.DoOutput ();
				writer.WriteEndDocument ();
				writer.Flush ();
			} finally {
				if (outputStream != null)
					outputStream.Dispose ();
			}
			return 0;
		}

		internal static bool AbiMode { get; private set; }
		internal static bool FollowForwarders { get; private set; }
		internal static bool FullAPISet { get; set; }
	}

	public class Utils {
		static char[] CharsToCleanup = new char[] { '<', '>', '/' };

		public static string CleanupTypeName (TypeReference type)
		{
			return CleanupTypeName (type.FullName);
		}

		public static string CleanupTypeName (string t)
		{
			if (t.IndexOfAny (CharsToCleanup) == -1)
				return t;
			var sb = new StringBuilder (t.Length);
			for (int i = 0; i < t.Length; i++) {
				var ch = t [i];
				switch (ch) {
				case '<':
					sb.Append ('[');
					break;
				case '>':
					sb.Append (']');
					break;
				case '/':
					sb.Append ('+');
					break;
				default:
					sb.Append (ch);
					break;
				}
			}
			return sb.ToString ();
		}
	}

	class AssemblyCollection
	{
		XmlWriter writer;
		List<AssemblyDefinition> assemblies = new List<AssemblyDefinition> ();

		public AssemblyCollection ()
		{
		}

		public bool Add (string name)
		{
			AssemblyDefinition ass = LoadAssembly (name);
			if (ass == null) {
				Console.Error.WriteLine ("Cannot load assembly file " + name);
				return false;
			}

			assemblies.Add (ass);
			return true;
		}

		public void DoOutput ()
		{
			if (writer == null)
				throw new InvalidOperationException ("Document not set");

			writer.WriteStartElement ("assemblies");
			foreach (AssemblyDefinition a in assemblies) {
				AssemblyData data = new AssemblyData (writer, a);
				data.DoOutput ();
			}
			writer.WriteEndElement ();
		}

		public XmlWriter Writer {
			set { writer = value; }
		}

		AssemblyDefinition LoadAssembly (string assembly)
		{
			try {
				if (File.Exists (assembly))
					return TypeHelper.Resolver.ResolveFile (assembly);

				return TypeHelper.Resolver.Resolve (assembly);
			} catch (Exception e) {
				Console.WriteLine (e);
				return null;
			}
		}
	}

	abstract class BaseData
	{
		protected XmlWriter writer;

		protected BaseData (XmlWriter writer)
		{
			this.writer = writer;
		}

		public abstract void DoOutput ();

		protected void AddAttribute (string name, string value)
		{
			writer.WriteAttributeString (name, value);
		}
	}

	class TypeForwardedToData : BaseData
	{
		AssemblyDefinition ass;

		public TypeForwardedToData (XmlWriter writer, AssemblyDefinition ass)
			: base (writer)
		{
			this.ass = ass;
		}

		public override void DoOutput ()
		{
			foreach (ExportedType type in ass.MainModule.ExportedTypes) {

				if (((uint)type.Attributes & 0x200000u) == 0)
					continue;

				writer.WriteStartElement ("attribute");
				AddAttribute ("name", typeof (TypeForwardedToAttribute).FullName);
				writer.WriteStartElement ("properties");
				writer.WriteStartElement ("property");
				AddAttribute ("name", "Destination");
				AddAttribute ("value", Utils.CleanupTypeName (type.FullName));
				writer.WriteEndElement (); // properties
				writer.WriteEndElement (); // properties
				writer.WriteEndElement (); // attribute
			}
		}

		public static void OutputForwarders (XmlWriter writer, AssemblyDefinition ass)
		{
			TypeForwardedToData tftd = new TypeForwardedToData (writer, ass);
			tftd.DoOutput ();
		}
	}

	class AssemblyData : BaseData
	{
		AssemblyDefinition ass;

		public AssemblyData (XmlWriter writer, AssemblyDefinition ass)
			: base (writer)
		{
			this.ass = ass;
		}

		public override void DoOutput ()
		{
			if (writer == null)
				throw new InvalidOperationException ("Document not set");

			writer.WriteStartElement ("assembly");
			AssemblyNameDefinition aname = ass.Name;
			AddAttribute ("name", aname.Name);
			AddAttribute ("version", aname.Version.ToString ());

			AttributeData.OutputAttributes (writer, ass);

			var types = new List<TypeDefinition> ();
			if (ass.MainModule.Types != null) {
				types.AddRange (ass.MainModule.Types);
			}

			if (Driver.FollowForwarders && ass.MainModule.ExportedTypes != null) {
				foreach (var t in ass.MainModule.ExportedTypes) {
					var forwarded = t.Resolve ();
					if (forwarded == null) {
						throw new Exception ("Could not resolve forwarded type " + t.FullName + " in " + ass.Name);
					}
					types.Add (forwarded);
				}
			}

			if (types.Count == 0) {
				writer.WriteEndElement (); // assembly
				return;
			}

			types.Sort (TypeReferenceComparer.Default);

			writer.WriteStartElement ("namespaces");

			string current_namespace = "$%&$&";
			bool in_namespace = false;
			foreach (TypeDefinition t in types) {
				if (string.IsNullOrEmpty (t.Namespace))
					continue;

				if (!Driver.AbiMode && ((t.Attributes & TypeAttributes.VisibilityMask) != TypeAttributes.Public))
					continue;

				if (t.DeclaringType != null)
					continue; // enforce !nested

				if (t.Namespace != current_namespace) {
					current_namespace = t.Namespace;
					if (in_namespace) {
						writer.WriteEndElement (); // classes
						writer.WriteEndElement (); // namespace
					} else {
						in_namespace = true;
					}
					writer.WriteStartElement ("namespace");
					AddAttribute ("name", current_namespace);
					writer.WriteStartElement ("classes");
				}

				TypeData bd = new TypeData (writer, t);
				bd.DoOutput ();

			}

			if (in_namespace) {
				writer.WriteEndElement (); // classes
				writer.WriteEndElement (); // namespace
			}

			writer.WriteEndElement (); // namespaces

			writer.WriteEndElement (); // assembly
		}
	}

	abstract class MemberData : BaseData
	{
		MemberReference [] members;

		public MemberData (XmlWriter writer, MemberReference [] members)
			: base (writer)
		{
			this.members = members;
		}

		protected virtual ICustomAttributeProvider GetAdditionalCustomAttributeProvider (MemberReference member)
		{
			return null;
		}

		public override void DoOutput ()
		{
			writer.WriteStartElement (ParentTag);

			foreach (MemberReference member in members) {
				writer.WriteStartElement (Tag);
				AddAttribute ("name", GetName (member));
				if (!NoMemberAttributes)
					AddAttribute ("attrib", GetMemberAttributes (member));
				AddExtraAttributes (member);

				AttributeData.OutputAttributes (writer, (ICustomAttributeProvider) member, GetAdditionalCustomAttributeProvider (member));

				AddExtraData (member);
				writer.WriteEndElement (); // Tag
			}

			writer.WriteEndElement (); // ParentTag
		}

		protected virtual void AddExtraData (MemberReference memberDefenition)
		{
		}

		protected virtual void AddExtraAttributes (MemberReference memberDefinition)
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

		public static void OutputGenericParameters (XmlWriter writer, IGenericParameterProvider provider)
		{
			if (provider.GenericParameters.Count == 0)
				return;

			var gparameters = provider.GenericParameters;

			writer.WriteStartElement ("generic-parameters");

			foreach (GenericParameter gp in gparameters) {
				writer.WriteStartElement ("generic-parameter");
				writer.WriteAttributeString ("name", gp.Name);
				writer.WriteAttributeString ("attributes", ((int) gp.Attributes).ToString ());

				AttributeData.OutputAttributes (writer, gp);

				var constraints = gp.Constraints;
				if (constraints.Count == 0) {
					writer.WriteEndElement (); // generic-parameter
					continue;
				}

				writer.WriteStartElement ("generic-parameter-constraints");

				foreach (TypeReference constraint in constraints) {
					writer.WriteStartElement ("generic-parameter-constraint");
					writer.WriteAttributeString ("name", Utils.CleanupTypeName (constraint));
					writer.WriteEndElement (); // generic-parameter-constraint
				}

				writer.WriteEndElement (); // generic-parameter-constraints

				writer.WriteEndElement (); // generic-parameter
			}

			writer.WriteEndElement (); // generic-parameters
		}
	}

	class TypeData : MemberData
	{
		TypeDefinition type;

		public TypeData (XmlWriter writer, TypeDefinition type)
			: base (writer, null)
		{
			this.type = type;
		}
		public override void DoOutput ()
		{
			if (writer == null)
				throw new InvalidOperationException ("Document not set");

			writer.WriteStartElement ("class");
			AddAttribute ("name", type.Name);
			string classType = GetClassType (type);
			AddAttribute ("type", classType);

			if (type.BaseType != null)
				AddAttribute ("base", Utils.CleanupTypeName (type.BaseType));

			if (type.IsSealed)
				AddAttribute ("sealed", "true");

			if (type.IsAbstract)
				AddAttribute ("abstract", "true");

			if ( (type.Attributes & TypeAttributes.Serializable) != 0 || type.IsEnum)
				AddAttribute ("serializable", "true");

			string charSet = GetCharSet (type);
			AddAttribute ("charset", charSet);

			string layout = GetLayout (type);
			if (layout != null)
				AddAttribute ("layout", layout);

			if (type.PackingSize >= 0) {
				AddAttribute ("pack", type.PackingSize.ToString ());
			}

			if (type.ClassSize >= 0) {
				AddAttribute ("size", type.ClassSize.ToString ());
			}

			if (type.IsEnum) {
				var value_type = GetEnumValueField (type);
				if (value_type == null)
					throw new NotSupportedException ();

				AddAttribute ("enumtype", Utils.CleanupTypeName (value_type.FieldType));
			}

			AttributeData.OutputAttributes (writer, type);

			var ifaces =  TypeHelper.GetInterfaces (type).
				Where ((iface) => TypeHelper.IsPublic (iface)). // we're only interested in public interfaces
				OrderBy (s => s.FullName, StringComparer.Ordinal);

			if (ifaces.Any ()) {
				writer.WriteStartElement ("interfaces");
				foreach (TypeReference iface in ifaces) {
					writer.WriteStartElement ("interface");
					AddAttribute ("name", Utils.CleanupTypeName (iface));
					writer.WriteEndElement (); // interface
				}
				writer.WriteEndElement (); // interfaces
			}

			MemberData.OutputGenericParameters (writer, type);

			ArrayList members = new ArrayList ();

			FieldDefinition [] fields = GetFields (type);
			if (fields.Length > 0) {
				Array.Sort (fields, MemberReferenceComparer.Default);
				FieldData fd = new FieldData (writer, fields);
				members.Add (fd);
			}

			if (!Driver.AbiMode) {

				MethodDefinition [] ctors = GetConstructors (type);
				if (ctors.Length > 0) {
					Array.Sort (ctors, MethodDefinitionComparer.Default);
					members.Add (new ConstructorData (writer, ctors));
				}

				PropertyDefinition[] properties = GetProperties (type, Driver.FullAPISet);
				if (properties.Length > 0) {
					Array.Sort (properties, PropertyDefinitionComparer.Default);
					members.Add (new PropertyData (writer, properties));
				}

				EventDefinition [] events = GetEvents (type);
				if (events.Length > 0) {
					Array.Sort (events, MemberReferenceComparer.Default);
					members.Add (new EventData (writer, events));
				}

				MethodDefinition [] methods = GetMethods (type, Driver.FullAPISet);
				if (methods.Length > 0) {
					Array.Sort (methods, MethodDefinitionComparer.Default);
					members.Add (new MethodData (writer, methods));
				}
			}

			foreach (MemberData md in members)
				md.DoOutput ();

			var nested = type.NestedTypes;
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
				var nestedArray = nested.ToArray ();
				Array.Sort (nestedArray, TypeReferenceComparer.Default);

				writer.WriteStartElement ("classes");
				foreach (TypeDefinition t in nestedArray) {
					TypeData td = new TypeData (writer, t);
					td.DoOutput ();
				}
				writer.WriteEndElement (); // classes
			}

			writer.WriteEndElement (); // class
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

			if (t.IsPointer)
				return "pointer";

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

			var fields = type.Fields;
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


		internal static PropertyDefinition [] GetProperties (TypeDefinition type, bool fullAPI) {
			var list = new List<PropertyDefinition> ();

			var t = type;
			do {
				var properties = t.Properties;//type.GetProperties (flags);
				foreach (PropertyDefinition property in properties) {
					MethodDefinition getMethod = property.GetMethod;
					MethodDefinition setMethod = property.SetMethod;

					bool hasGetter = (getMethod != null) && MustDocumentMethod (getMethod);
					bool hasSetter = (setMethod != null) && MustDocumentMethod (setMethod);

					// if neither the getter or setter should be documented, then
					// skip the property
					if (hasGetter || hasSetter) {

						if (t != type && list.Any (l => l.Name == property.Name))
							continue;

						list.Add (property);
					}
				}

				if (!fullAPI)
					break;

				if (t.IsInterface || t.IsEnum)
					break;

				if (t.BaseType == null || t.BaseType.FullName == "System.Object")
					t = null;
				else
					t = t.BaseType.Resolve ();

			} while (t != null);

			return list.ToArray ();
		}

		private MethodDefinition[] GetMethods (TypeDefinition type, bool fullAPI)
		{
			var list = new List<MethodDefinition> ();

			var t = type;
			do {
				var methods = t.Methods;//type.GetMethods (flags);
				foreach (MethodDefinition method in methods) {
					if (method.IsSpecialName && !method.Name.StartsWith ("op_", StringComparison.Ordinal))
						continue;

					// we're only interested in public or protected members
					if (!MustDocumentMethod (method))
						continue;

					if (t == type && IsFinalizer (method)) {
						string name = method.DeclaringType.Name;
						int arity = name.IndexOf ('`');
						if (arity > 0)
							name = name.Substring (0, arity);

						method.Name = "~" + name;
					}

					if (t != type && list.Any (l => l.DeclaringType != method.DeclaringType && l.Name == method.Name && l.Parameters.Count == method.Parameters.Count &&
					                           l.Parameters.SequenceEqual (method.Parameters, new ParameterComparer ())))
						continue;

					list.Add (method);
				}

				if (!fullAPI)
					break;

				if (t.IsInterface || t.IsEnum)
					break;

				if (t.BaseType == null || t.BaseType.FullName == "System.Object")
					t = null;
				else
					t = t.BaseType.Resolve ();

			} while (t != null);

			return list.ToArray ();
		}

		sealed class ParameterComparer : IEqualityComparer<ParameterDefinition>
		{
			public bool Equals (ParameterDefinition x, ParameterDefinition y)
			{
				return x.ParameterType.Name == y.ParameterType.Name;
			}

			public int GetHashCode (ParameterDefinition obj)
			{
				return obj.ParameterType.Name.GetHashCode ();
			}
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

			var ctors = type.Methods.Where (m => m.IsConstructor);//type.GetConstructors (flags);
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

			var events = type.Events;//type.GetEvents (flags);
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
		public FieldData (XmlWriter writer, FieldDefinition [] members)
			: base (writer, members)
		{
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

		protected override void AddExtraAttributes (MemberReference memberDefinition)
		{
			base.AddExtraAttributes (memberDefinition);

			FieldDefinition field = (FieldDefinition) memberDefinition;
			AddAttribute ("fieldtype", Utils.CleanupTypeName (field.FieldType));

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
					AddAttribute ("value", stringValue);
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
		public PropertyData (XmlWriter writer, PropertyDefinition [] members)
			: base (writer, members)
		{
		}

		protected override string GetName (MemberReference memberDefenition)
		{
			PropertyDefinition prop = (PropertyDefinition) memberDefenition;
			return prop.Name;
		}

		MethodDefinition [] GetMethods (PropertyDefinition prop, out bool haveParameters)
		{
			MethodDefinition _get = prop.GetMethod;
			MethodDefinition _set = prop.SetMethod;
			bool haveGet = (_get != null && TypeData.MustDocumentMethod(_get));
			bool haveSet = (_set != null && TypeData.MustDocumentMethod(_set));
			haveParameters = haveGet || (haveSet && _set.Parameters.Count > 1);
			MethodDefinition [] methods;

			if (haveGet && haveSet) {
				methods = new MethodDefinition [] { _get, _set };
			} else if (haveGet) {
				methods = new MethodDefinition [] { _get };
			} else if (haveSet) {
				methods = new MethodDefinition [] { _set };
			} else {
				//odd
				return null;
			}

			return methods;
		}

		protected override void AddExtraAttributes (MemberReference memberDefinition)
		{
			base.AddExtraAttributes (memberDefinition);

			PropertyDefinition prop = (PropertyDefinition) memberDefinition;
			AddAttribute ("ptype", Utils.CleanupTypeName (prop.PropertyType));

			bool haveParameters;
			MethodDefinition [] methods = GetMethods ((PropertyDefinition) memberDefinition, out haveParameters);

			if (methods != null && haveParameters) {
				string parms = Parameters.GetSignature (methods [0].Parameters);
				if (!string.IsNullOrEmpty (parms))
					AddAttribute ("params", parms);
			}

		}

		protected override void AddExtraData (MemberReference memberDefenition)
		{
			base.AddExtraData (memberDefenition);

			bool haveParameters;
			MethodDefinition [] methods = GetMethods ((PropertyDefinition) memberDefenition, out haveParameters);

			if (methods == null)
				return;
			
			MethodData data = new MethodData (writer, methods);
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
		public EventData (XmlWriter writer, EventDefinition [] members)
			: base (writer, members)
		{
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

		protected override void AddExtraAttributes (MemberReference memberDefinition)
		{
			base.AddExtraAttributes (memberDefinition);

			EventDefinition evt = (EventDefinition) memberDefinition;
			AddAttribute ("eventtype", Utils.CleanupTypeName (evt.EventType));
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

		public MethodData (XmlWriter writer, MethodDefinition [] members)
			: base (writer, members)
		{
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

		protected override ICustomAttributeProvider GetAdditionalCustomAttributeProvider (MemberReference member)
		{
			var mbase = (MethodDefinition) member;
			return mbase.MethodReturnType;
		}

		protected override void AddExtraAttributes (MemberReference memberDefinition)
		{
			base.AddExtraAttributes (memberDefinition);

			if (!(memberDefinition is MethodDefinition))
				return;

			MethodDefinition mbase = (MethodDefinition) memberDefinition;

			if (mbase.IsAbstract)
				AddAttribute ("abstract", "true");
			if (mbase.IsVirtual)
				AddAttribute ("virtual", "true");
			if (mbase.IsFinal && mbase.IsVirtual && mbase.IsReuseSlot)
				AddAttribute ("sealed", "true");
			if (mbase.IsStatic)
				AddAttribute ("static", "true");
			var baseMethod = TypeHelper.GetBaseMethodInTypeHierarchy (mbase);
			if (baseMethod != null && baseMethod != mbase) {
				// This indicates whether this method is an override of another method.
				// This information is not necessarily available in the api info for any
				// particular assembly, because a method is only overriding another if
				// there is a base virtual function with the same signature, and that
				// base method can come from another assembly.
				AddAttribute ("is-override", "true");
			}
			string rettype = Utils.CleanupTypeName (mbase.MethodReturnType.ReturnType);
			if (rettype != "System.Void" || !mbase.IsConstructor)
				AddAttribute ("returntype", (rettype));
//
//			if (mbase.MethodReturnType.HasCustomAttributes)
//				AttributeData.OutputAttributes (writer, mbase.MethodReturnType);
		}

		protected override void AddExtraData (MemberReference memberDefenition)
		{
			base.AddExtraData (memberDefenition);

			if (!(memberDefenition is MethodDefinition))
				return;

			MethodDefinition mbase = (MethodDefinition)memberDefenition;

			ParameterData parms = new ParameterData (writer, mbase.Parameters) {
				HasExtensionParameter = mbase.CustomAttributes.Any (l => l.AttributeType.FullName == "System.Runtime.CompilerServices.ExtensionAttribute")
			};

			parms.DoOutput ();

			MemberData.OutputGenericParameters (writer, mbase);
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
		public ConstructorData (XmlWriter writer, MethodDefinition [] members)
			: base (writer, members)
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
		private IList<ParameterDefinition> parameters;

		public ParameterData (XmlWriter writer, IList<ParameterDefinition> parameters)
			: base (writer)
		{
			this.parameters = parameters;
		}

		public bool HasExtensionParameter { get; set; }

		public override void DoOutput ()
		{
			bool first = true;
			writer.WriteStartElement ("parameters");
			foreach (ParameterDefinition parameter in parameters) {
				writer.WriteStartElement ("parameter");
				AddAttribute ("name", parameter.Name);
				AddAttribute ("position", parameter.Method.Parameters.IndexOf(parameter).ToString(CultureInfo.InvariantCulture));
				AddAttribute ("attrib", ((int) parameter.Attributes).ToString());

				string direction = first && HasExtensionParameter ? "this" : "in";
				first = false;

				if (parameter.ParameterType is ByReferenceType)
					direction = parameter.IsOut ? "out" : "ref";

				TypeReference t = parameter.ParameterType;
				AddAttribute ("type", Utils.CleanupTypeName (t));

				if (parameter.IsOptional) {
					AddAttribute ("optional", "true");
					if (parameter.HasConstant)
						AddAttribute ("defaultValue", parameter.Constant == null ? "NULL" : parameter.Constant.ToString ());
				}

				if (direction != "in")
					AddAttribute ("direction", direction);

				AttributeData.OutputAttributes (writer, parameter);
				writer.WriteEndElement (); // parameter
			}
			writer.WriteEndElement (); // parameters
		}
	}

	class AttributeData
	{
		public static void DoOutput (XmlWriter writer, IList<ICustomAttributeProvider> providers)
		{
			if (writer == null)
				throw new InvalidOperationException ("Document not set");

			if (providers == null || providers.Count == 0)
				return;
		
			if (!providers.Any ((provider) => provider != null && provider.HasCustomAttributes))
				return;

			writer.WriteStartElement ("attributes");

			foreach (var provider in providers) {
				if (provider == null)
					continue;
				
				if (!provider.HasCustomAttributes)
					continue;


				var ass = provider as AssemblyDefinition;
				if (ass != null && !Driver.FollowForwarders)
					TypeForwardedToData.OutputForwarders (writer, ass);

				var attributes = provider.CustomAttributes.
					Where ((att) => !SkipAttribute (att)).
					OrderBy ((a) => a.Constructor.DeclaringType.FullName, StringComparer.Ordinal);
				
				foreach (var att in attributes) {
					string attName = Utils.CleanupTypeName (att.Constructor.DeclaringType);

					writer.WriteStartElement ("attribute");
					writer.WriteAttributeString ("name", attName);

					var attribute_mapping = CreateAttributeMapping (att);

					if (attribute_mapping != null) {
						var mapping = attribute_mapping.Where ((attr) => attr.Key != "TypeId");
						if (mapping.Any ()) {
							writer.WriteStartElement ("properties");
							foreach (var kvp in mapping) {
								string name = kvp.Key;
								object o = kvp.Value;

								writer.WriteStartElement ("property");
								writer.WriteAttributeString ("name", name);

								if (o == null) {
									writer.WriteAttributeString ("value", "null");
								} else {
									string value = o.ToString ();
									if (attName.EndsWith ("GuidAttribute", StringComparison.Ordinal))
										value = value.ToUpper ();
									writer.WriteAttributeString ("value", value);
								}

								writer.WriteEndElement (); // property
							}
							writer.WriteEndElement (); // properties
						}
					}
					writer.WriteEndElement (); // attribute
				}
			}

			writer.WriteEndElement (); // attributes
		}

		static Dictionary<string, object> CreateAttributeMapping (CustomAttribute attribute)
		{
			Dictionary<string, object> mapping = null;

			PopulateMapping (ref mapping, attribute);

			var constructor = attribute.Constructor.Resolve ();
			if (constructor == null || !constructor.HasParameters)
				return mapping;

			PopulateMapping (ref mapping, constructor, attribute);

			return mapping;
		}

		static void PopulateMapping (ref Dictionary<string, object> mapping, CustomAttribute attribute)
		{
			if (!attribute.HasProperties)
				return;
			
			foreach (var named_argument in attribute.Properties) {
				var name = named_argument.Name;
				var arg = named_argument.Argument;

				if (arg.Value is CustomAttributeArgument)
					arg = (CustomAttributeArgument) arg.Value;

				if (mapping == null)
					mapping = new Dictionary<string, object> (StringComparer.Ordinal);
				mapping.Add (name, GetArgumentValue (arg.Type, arg.Value));
			}
		}

		static Dictionary<FieldReference, int> CreateArgumentFieldMapping (MethodDefinition constructor)
		{
			Dictionary<FieldReference, int> field_mapping = null;

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
					argument = ((ParameterDefinition) instruction.Operand).Index + 1;
					break;

				case Code.Stfld:
					FieldReference field = (FieldReference) instruction.Operand;
					if (field.DeclaringType.FullName != constructor.DeclaringType.FullName)
						continue;

					if (!argument.HasValue)
						break;

					if (field_mapping == null)
						field_mapping = new Dictionary<FieldReference, int> ();
					
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
			Dictionary<PropertyDefinition, FieldReference> property_mapping = null;

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

					if (property_mapping == null)
						property_mapping = new Dictionary<PropertyDefinition, FieldReference> ();
					property_mapping.Add (property, field);
					break;
				}
			}

			return property_mapping;
		}

		static void PopulateMapping (ref Dictionary<string, object> mapping, MethodDefinition constructor, CustomAttribute attribute)
		{
			if (!constructor.HasBody)
				return;

			// Custom handling for attributes with arguments which cannot be easily extracted
			var ca = attribute.ConstructorArguments;
			switch (constructor.DeclaringType.FullName) {
			case "System.Runtime.CompilerServices.DecimalConstantAttribute":
				var dca = constructor.Parameters[2].ParameterType == constructor.Module.TypeSystem.Int32 ?
					new DecimalConstantAttribute ((byte) ca[0].Value, (byte) ca[1].Value, (int) ca[2].Value, (int) ca[3].Value, (int) ca[4].Value) :
					new DecimalConstantAttribute ((byte) ca[0].Value, (byte) ca[1].Value, (uint) ca[2].Value, (uint) ca[3].Value, (uint) ca[4].Value);

				if (mapping == null)
					mapping = new Dictionary<string, object> (StringComparer.Ordinal);
				mapping.Add ("Value", dca.Value);
				return;
			case "System.ComponentModel.BindableAttribute":
				if (ca.Count != 1)
					break;

				if (constructor.Parameters[0].ParameterType == constructor.Module.TypeSystem.Boolean) {
					if (mapping == null)
						mapping = new Dictionary<string, object> (StringComparer.Ordinal);
					mapping.Add ("Bindable", ca[0].Value);
				} else {
					throw new NotImplementedException ();
				}

				return;
			}

			var field_mapping = CreateArgumentFieldMapping (constructor);
			if (field_mapping != null) { 
				var property_mapping = CreatePropertyFieldMapping ((TypeDefinition) constructor.DeclaringType);

				if (property_mapping != null) {
					foreach (var pair in property_mapping) {
						int argument;
						if (!field_mapping.TryGetValue (pair.Value, out argument))
							continue;

						var ca_arg = ca [argument];
						if (ca_arg.Value is CustomAttributeArgument)
							ca_arg = (CustomAttributeArgument)ca_arg.Value;

						if (mapping == null)
							mapping = new Dictionary<string, object> (StringComparer.Ordinal);
						mapping.Add (pair.Key.Name, GetArgumentValue (ca_arg.Type, ca_arg.Value));
					}
				}
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

			if (!type.HasCustomAttributes)
				return false;

			foreach (CustomAttribute attribute in type.CustomAttributes)
				if (attribute.Constructor.DeclaringType.FullName == "System.FlagsAttribute")
					return true;

			return false;
		}

		static object GetFlaggedEnumValue (TypeDefinition type, object value)
		{
			if (value is ulong)
				return GetFlaggedEnumValue (type, (ulong)value);

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

		static object GetFlaggedEnumValue (TypeDefinition type, ulong flags)
		{
			var signature = new StringBuilder ();

			for (int i = type.Fields.Count - 1; i >= 0; i--) {
				FieldDefinition field = type.Fields [i];

				if (!field.HasConstant)
					continue;

				ulong flag = Convert.ToUInt64 (field.Constant);

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
			if (!TypeHelper.IsPublic (attribute))
				return true;
			
			return attribute.Constructor.DeclaringType.Name.EndsWith ("TODOAttribute", StringComparison.Ordinal);
		}

		public static void OutputAttributes (XmlWriter writer, params ICustomAttributeProvider[] providers)
		{
			AttributeData.DoOutput (writer, providers);
		}
	}

	static class Parameters {

		public static string GetSignature (IList<ParameterDefinition> infos)
		{
			if (infos == null || infos.Count == 0)
				return string.Empty;

			var signature = new StringBuilder ();
			for (int i = 0; i < infos.Count; i++) {

				if (i > 0)
					signature.Append (", ");

				ParameterDefinition info = infos [i];

				if (info.ParameterType.IsByReference) {
					string modifier;
					if ((info.Attributes & (ParameterAttributes.Out | ParameterAttributes.In)) == ParameterAttributes.Out)
						modifier = "out";
					else
						modifier = "ref";

					signature.Append (modifier);
					signature.Append (" ");
				}

				signature.Append (Utils.CleanupTypeName (info.ParameterType));
			}

			return signature.ToString ();
		}

	}

	class TypeReferenceComparer : IComparer<TypeReference>
	{
		public static TypeReferenceComparer Default = new TypeReferenceComparer ();

		public int Compare (TypeReference a, TypeReference b)
		{
			int result = String.Compare (a.Namespace, b.Namespace, StringComparison.Ordinal);
			if (result != 0)
				return result;

			return String.Compare (a.Name, b.Name, StringComparison.Ordinal);
		}
	}

	class MemberReferenceComparer : IComparer
	{
		public static MemberReferenceComparer Default = new MemberReferenceComparer ();

		public int Compare (object a, object b)
		{
			MemberReference ma = (MemberReference) a;
			MemberReference mb = (MemberReference) b;
			return String.Compare (ma.Name, mb.Name, StringComparison.Ordinal);
		}
	}

	class PropertyDefinitionComparer : IComparer<PropertyDefinition>
	{
		public static PropertyDefinitionComparer Default = new PropertyDefinitionComparer ();

		public int Compare (PropertyDefinition ma, PropertyDefinition mb)
		{
			int res = String.Compare (ma.Name, mb.Name, StringComparison.Ordinal);
			if (res != 0)
				return res;

			if (!ma.HasParameters && !mb.HasParameters)
				return 0;

			if (!ma.HasParameters)
				return -1;

			if (!mb.HasParameters)
				return 1;

			return MethodDefinitionComparer.Compare (ma.Parameters, mb.Parameters);
		}
	}

	class MethodDefinitionComparer : IComparer
	{
		public static MethodDefinitionComparer Default = new MethodDefinitionComparer ();

		public int Compare (object a, object b)
		{
			MethodDefinition ma = (MethodDefinition) a;
			MethodDefinition mb = (MethodDefinition) b;
			int res = String.Compare (ma.Name, mb.Name, StringComparison.Ordinal);
			if (res != 0)
				return res;

			if (!ma.HasParameters && !mb.HasParameters)
				return 0;

			if (!ma.HasParameters)
				return -1;

			if (!mb.HasParameters)
				return 1;

			res = Compare (ma.Parameters, mb.Parameters);
			if (res != 0)
				return res;

			if (ma.HasGenericParameters != mb.HasGenericParameters)
				return ma.HasGenericParameters ? -1 : 1;

			if (ma.HasGenericParameters && mb.HasGenericParameters) {
				res = ma.GenericParameters.Count - mb.GenericParameters.Count;
				if (res != 0)
					return res;
			}

			// operators can differ by only return type
			return string.CompareOrdinal (ma.ReturnType.FullName, mb.ReturnType.FullName);
		}

		public static int Compare (IList<ParameterDefinition> pia, IList<ParameterDefinition> pib)
		{
			var res = pia.Count - pib.Count;
			if (res != 0)
				return res;

			string siga = Parameters.GetSignature (pia);
			string sigb = Parameters.GetSignature (pib);
			return String.Compare (siga, sigb, StringComparison.Ordinal);
		}
	}
}

