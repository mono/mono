//
// MasterMetadata.cs
//
// (C) 2007 - 2008 Novell, Inc. (http://www.novell.com)
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
using System.Collections.Generic;
using System.Text;

namespace GuiCompare {

	static class MasterUtils {
		public static void PopulateMethodList (XMLMethods methods, List<CompNamed> method_list)
		{
			foreach (object key in methods.keys.Keys) {
				XMLMethods.SignatureFlags signatureFlags = (methods.signatureFlags != null &&
				                                            methods.signatureFlags.ContainsKey (key) ?
				                                            (XMLMethods.SignatureFlags) methods.signatureFlags [key] :
				                                            XMLMethods.SignatureFlags.None);

				XMLParameters parameters = (methods.parameters == null ? null
				                            : (XMLParameters)methods.parameters[key]);
				XMLGenericParameters genericParameters = (methods.genericParameters == null ? null
				                                                  : (XMLGenericParameters)methods.genericParameters[key]);
				XMLAttributes attributes = (methods.attributeMap == null ? null
				                            : (XMLAttributes)methods.attributeMap[key]);
				string returnType = (methods.returnTypes == null ? null
				                     : (string)methods.returnTypes[key]);
				method_list.Add (new MasterMethod ((string)methods.keys[key],
				                                   signatureFlags,
				                                   returnType,
				                                   parameters,
				                                   genericParameters,
				                                   methods.ConvertToString (Int32.Parse ((string)methods.access[key])),
				                                   attributes));
			}
		}
		                                       
		public static void PopulateMemberLists (XMLClass xml_cls,
		                                        List<CompNamed> interface_list,
		                                        List<CompNamed> constructor_list,
		                                        List<CompNamed> method_list,
		                                        List<CompNamed> property_list,
		                                        List<CompNamed> field_list,
		                                        List<CompNamed> event_list)
		{
			if (interface_list != null && xml_cls.interfaces != null) {
				foreach (object i in xml_cls.interfaces.keys.Keys) {
					interface_list.Add (new MasterInterface ((string)xml_cls.interfaces.keys[i]));
				}
			}
			
			if (constructor_list != null && xml_cls.constructors != null) {
				PopulateMethodList (xml_cls.constructors, constructor_list);
			}
			
			if (method_list != null && xml_cls.methods != null) {
				PopulateMethodList (xml_cls.methods, method_list);
			}
			
			if (property_list != null && xml_cls.properties != null) {
				foreach (object key in xml_cls.properties.keys.Keys) {
					XMLAttributes attributes = (xml_cls.properties.attributeMap == null ? null
					                            : (XMLAttributes)xml_cls.properties.attributeMap[key]);

					property_list.Add (new MasterProperty ((string)key,
					                                       xml_cls.properties.ConvertToString (Int32.Parse ((string)xml_cls.properties.access[key])),
					                                       (XMLMethods)xml_cls.properties.nameToMethod[key],
					                                       attributes));
				}
			}
			
			if (field_list != null && xml_cls.fields != null) {
				foreach (object key in xml_cls.fields.keys.Keys) {
					string type = (xml_cls.fields.fieldTypes == null || !xml_cls.fields.fieldTypes.ContainsKey(key)) ? null : (string)xml_cls.fields.fieldTypes[key];
					string fvalue = (xml_cls.fields.fieldValues == null || !xml_cls.fields.fieldValues.ContainsKey(key)) ? null : (string)xml_cls.fields.fieldValues[key];
					XMLAttributes attributes = (xml_cls.fields.attributeMap == null ? null
					                            : (XMLAttributes)xml_cls.fields.attributeMap[key]);

					field_list.Add (new MasterField ((string)xml_cls.fields.keys[key],
					                                 type, fvalue,
					                                 xml_cls.fields.ConvertToString(Int32.Parse ((string)xml_cls.fields.access[key])),
					                                 attributes));
				}
			}
			
			if (event_list != null && xml_cls.events != null) {
				foreach (object key in xml_cls.events.keys.Keys) {
					XMLAttributes attributes = (xml_cls.events.attributeMap == null ? null
					                            : (XMLAttributes)xml_cls.events.attributeMap[key]);
					event_list.Add (new MasterEvent ((string)xml_cls.events.keys[key],
					                                 (string)xml_cls.events.eventTypes[key],
					                                 xml_cls.events.ConvertToString (Int32.Parse ((string)xml_cls.events.access[key])),
					                                 attributes));
				}
			}
		}
		

		public static void PopulateTypeLists (XMLClass fromDef,
		                                      List<CompNamed> class_list,
		                                      List<CompNamed> enum_list,
		                                      List<CompNamed> delegate_list,
		                                      List<CompNamed> interface_list,
		                                      List<CompNamed> struct_list)
		{
			if (fromDef.nested == null)
				return;
			
			foreach (XMLClass cls in fromDef.nested) {
				if (cls.type == "class")
					class_list.Add (new MasterClass (cls, CompType.Class));
				else if (cls.type == "enum")
					enum_list.Add (new MasterEnum (cls));
				else if (cls.type == "delegate")
					delegate_list.Add (new MasterDelegate (cls));
				else if (cls.type == "interface")
					interface_list.Add (new MasterInterface (cls));
				else if (cls.type == "struct")
					struct_list.Add (new MasterClass (cls, CompType.Struct));
			}
		}
		
		public static bool IsImplementationSpecificAttribute (string name)
		{
			if (name.IndexOf (';') > 0) {
				if (name.StartsWith ("System.Runtime.CompilerServices.InternalsVisibleToAttribute"))
					return true;
			}
		
			switch (name) {
				case "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute":
				case "System.NonSerializedAttribute":
				case "System.Runtime.CompilerServices.CompilerGeneratedAttribute":
				case "System.Security.SecuritySafeCriticalAttribute":
				case "System.Security.SecurityCriticalAttribute":
				case "System.Diagnostics.DebuggerHiddenAttribute":
				case "System.Diagnostics.DebuggerStepThroughAttribute":
				case "System.Runtime.CompilerServices.InternalsVisibleToAttribute":
				case "System.Runtime.TargetedPatchingOptOutAttribute":
				case "System.Runtime.InteropServices.ComVisibleAttribute":
				return true;
			}
				
			return false;
		}
		
		public static List<CompNamed> GetAttributes (XMLAttributes attributes)
		{
			List<CompNamed> rv = new List<CompNamed>();
			if (attributes != null) {
				XMLAttributeProperties properties;
				
				foreach (string key in attributes.keys.Keys) {
					if (IsImplementationSpecificAttribute (key))
						continue;

					if (!attributes.Properties.TryGetValue (key, out properties))
						properties = null;
					rv.Add (new MasterAttribute ((string)attributes.keys[key], properties));
				}
			}
			return rv;
		}

		public static List<CompGenericParameter> GetTypeParameters (XMLGenericParameters gparams)
		{
			if (gparams == null)
				return null;

			var list = new List<CompGenericParameter> ();
			foreach (string key in gparams.keys.Keys) {
				XMLAttributes attributes = gparams.attributeMap == null ?
					null : (XMLAttributes) gparams.attributeMap [key];
					
				var constraints = gparams.constraints [key];
				
				list.Add (new MasterGenericTypeParameter (key, constraints.attributes, attributes));
			}
			
			return list;
		}
	}
	
	public class MasterAssembly : CompAssembly {
		public MasterAssembly (string path)
			: base (path)
		{
			masterinfo = XMLAssembly.CreateFromFile (path);
			if (masterinfo == null)
				throw new ArgumentException ("Error loading masterinfo from " + path);
			attributes = MasterUtils.GetAttributes (masterinfo.attributes);
		}

		public override List<CompNamed> GetNamespaces ()
		{
			List<CompNamed> namespaces = new List<CompNamed>();
			if (masterinfo != null && masterinfo.namespaces != null) {
				foreach (XMLNamespace ns in masterinfo.namespaces)
					namespaces.Add (new MasterNamespace (ns));
			}

			return namespaces;
		}

		public override List<CompNamed> GetAttributes ()
		{
			return attributes;
		}

		XMLAssembly masterinfo;
		List<CompNamed> attributes;
	}

	public class MasterNamespace : CompNamespace {
		public MasterNamespace (XMLNamespace ns)
			: base (ns.name)
		{
			this.ns = ns;

			delegate_list = new List<CompNamed>();
			enum_list = new List<CompNamed>();
			class_list = new List<CompNamed>();
			struct_list = new List<CompNamed>();
			interface_list = new List<CompNamed>();

			foreach (XMLClass cls in ns.types) {
				if (cls.type == "class")
					class_list.Add (new MasterClass (cls, CompType.Class));
				else if (cls.type == "enum")
					enum_list.Add (new MasterEnum (cls));
				else if (cls.type == "delegate")
					delegate_list.Add (new MasterDelegate (cls));
				else if (cls.type == "interface")
					interface_list.Add (new MasterInterface (cls));
				else if (cls.type == "struct")
					struct_list.Add (new MasterClass (cls, CompType.Struct));
			}
		}

		public override List<CompNamed> GetNestedClasses ()
		{
			return class_list;
		}

		public override List<CompNamed> GetNestedInterfaces ()
		{
			return interface_list;
		}

		public override List<CompNamed> GetNestedStructs ()
		{
			return struct_list;
		}

		public override List<CompNamed> GetNestedEnums ()
		{
			return enum_list;
		}

		public override List<CompNamed> GetNestedDelegates ()
		{
			return delegate_list;
		}

		XMLNamespace ns;
		List<CompNamed> delegate_list;
		List<CompNamed> enum_list;
		List<CompNamed> class_list;
		List<CompNamed> struct_list;
		List<CompNamed> interface_list;
	}

	public class MasterInterface : CompInterface {
		public MasterInterface (XMLClass xml_cls)
			: base (xml_cls.name)
		{
			this.xml_cls = xml_cls;
			
			interfaces = new List<CompNamed>();
			constructors = new List<CompNamed>();
			methods = new List<CompNamed>();
			properties = new List<CompNamed>();
			fields = new List<CompNamed>();
			events = new List<CompNamed>();
			
			MasterUtils.PopulateMemberLists (xml_cls,
			                                 interfaces,
			                                 constructors,
			                                 methods,
			                                 properties,
			                                 fields,
			                                 events);
			
			attributes = MasterUtils.GetAttributes (xml_cls.attributes);
		}
		
		public MasterInterface (string name)
			: base (name)
		{
			interfaces = new List<CompNamed>();
			constructors = new List<CompNamed>();
			methods = new List<CompNamed>();
			properties = new List<CompNamed>();
			fields = new List<CompNamed>();
			events = new List<CompNamed>();
			attributes = new List<CompNamed>();
		}

		public override string GetBaseType()
		{
			return xml_cls == null ? null : xml_cls.baseName;
		}
		
		public override List<CompNamed> GetInterfaces ()
		{
			return interfaces;
		}

		public override List<CompNamed> GetMethods ()
		{
			return methods;
		}

		public override List<CompNamed> GetConstructors ()
		{
			return constructors;
		}

 		public override List<CompNamed> GetProperties()
		{
			return properties;
		}

 		public override List<CompNamed> GetFields()
		{
			return fields;
		}

 		public override List<CompNamed> GetEvents()
		{
			return events;
		}

		public override List<CompNamed> GetAttributes ()
		{
			return attributes;
		}
		
		public override List<CompGenericParameter> GetTypeParameters ()
		{
			return xml_cls.GetTypeParameters ();
		}
		
		XMLClass xml_cls;
		List<CompNamed> interfaces;
		List<CompNamed> constructors;
		List<CompNamed> methods;
		List<CompNamed> properties;
		List<CompNamed> fields;
		List<CompNamed> events;
		List<CompNamed> attributes;
	}

	public class MasterDelegate : CompDelegate {
		public MasterDelegate (XMLClass cls)
			: base (cls.name)
		{
			xml_cls = cls;
		}
		
		public override List<CompNamed> GetAttributes ()
		{
			return MasterUtils.GetAttributes (xml_cls.attributes);
		}

		public override string GetBaseType ()
		{
			return xml_cls.baseName;
		}
		
		public override List<CompGenericParameter> GetTypeParameters ()
		{
			return xml_cls.GetTypeParameters ();
		}
		
		XMLClass xml_cls;
	}

	public class MasterEnum : CompEnum {
		public MasterEnum (XMLClass cls)
			: base (cls.name)
		{
			xml_cls = cls;
			
			fields = new List<CompNamed>();

			MasterUtils.PopulateMemberLists (xml_cls,
			                                 null,
			                                 null,
			                                 null,
			                                 null,
			                                 fields,
			                                 null);

		}

		public override string GetBaseType()
		{
			return xml_cls.baseName;
		}
		
 		public override List<CompNamed> GetFields()
		{
			return fields;
		}

		public override List<CompNamed> GetAttributes ()
		{
			return MasterUtils.GetAttributes (xml_cls.attributes);
		}

		List<CompNamed> fields;
		XMLClass xml_cls;
	}

	public class MasterClass : CompClass {
		public MasterClass (XMLClass cls, CompType type)
			: base (cls.name, type)
		{
			xml_cls = cls;

			interfaces = new List<CompNamed>();
			constructors = new List<CompNamed>();
			methods = new List<CompNamed>();
			properties = new List<CompNamed>();
			fields = new List<CompNamed>();
			events = new List<CompNamed>();

			MasterUtils.PopulateMemberLists (xml_cls,
			                                 interfaces,
			                                 constructors,
			                                 methods,
			                                 properties,
			                                 fields,
			                                 events);
			
			delegate_list = new List<CompNamed>();
			enum_list = new List<CompNamed>();
			class_list = new List<CompNamed>();
			struct_list = new List<CompNamed>();
			interface_list = new List<CompNamed>();

			MasterUtils.PopulateTypeLists (xml_cls,
			                               class_list,
			                               enum_list,
			                               delegate_list,
			                               interface_list,
			                               struct_list);
		}

		public override string GetBaseType ()
		{
			return xml_cls.baseName;
		}
		
		public override bool IsSealed { get { return xml_cls.isSealed; } }
		public override bool IsAbstract { get { return xml_cls.isAbstract; } }		
		
		public override List<CompNamed> GetInterfaces ()
		{
			return interfaces;
		}

		public override List<CompNamed> GetMethods()
		{
			return methods;
		}

		public override List<CompNamed> GetConstructors()
		{
			return constructors;
		}

 		public override List<CompNamed> GetProperties()
		{
			return properties;
		}

 		public override List<CompNamed> GetFields()
		{
			return fields;
		}

 		public override List<CompNamed> GetEvents()
		{
			return events;
		}

		public override List<CompNamed> GetAttributes ()
		{
			return MasterUtils.GetAttributes (xml_cls.attributes);
		}

		public override List<CompNamed> GetNestedClasses()
		{
			return class_list;
		}

		public override List<CompNamed> GetNestedInterfaces ()
		{
			return interface_list;
		}

		public override List<CompNamed> GetNestedStructs ()
		{
			return struct_list;
		}

		public override List<CompNamed> GetNestedEnums ()
		{
			return enum_list;
		}

		public override List<CompNamed> GetNestedDelegates ()
		{
			return delegate_list;
		}
		
		public override List<CompGenericParameter> GetTypeParameters ()
		{
			return xml_cls.GetTypeParameters ();
		}

		XMLClass xml_cls;

		List<CompNamed> interfaces;
		List<CompNamed> constructors;
		List<CompNamed> methods;
		List<CompNamed> properties;
		List<CompNamed> fields;
		List<CompNamed> events;
		
		List<CompNamed> delegate_list;
		List<CompNamed> enum_list;
		List<CompNamed> class_list;
		List<CompNamed> struct_list;
		List<CompNamed> interface_list;
}

	public class MasterEvent : CompEvent {
		public MasterEvent (string name,
		                    string eventType,
		                    string eventAccess,
		                    XMLAttributes attributes)
			: base (name)
		{
			this.eventType = eventType;
			this.eventAccess = eventAccess;
			this.attributes = attributes;
		}

		public override string GetMemberType ()
		{
			return eventType;
		}

		public override string GetMemberAccess ()
		{
			return eventAccess;
		}
		
		public override List<CompNamed> GetAttributes ()
		{
			return MasterUtils.GetAttributes (attributes);
		}
		
		string eventType;
		string eventAccess;
		XMLAttributes attributes;
	}
	

	public class MasterField : CompField {
		public MasterField (string name,
		                    string fieldType,
		                    string fieldValue,
		                    string fieldAccess,
		                    XMLAttributes attributes)
			: base (name)
		{
			this.fieldType = fieldType;
			this.fieldValue = fieldValue;
			// we don't care about the Assembly (internal) part
			this.fieldAccess = fieldAccess.Replace ("FamORAssem", "Family");
			this.attributes = attributes;
		}

		public override string GetMemberType ()
		{
			return fieldType;
		}
		
		public override string GetMemberAccess ()
		{
			return fieldAccess;
		}
		
		public override List<CompNamed> GetAttributes ()
		{
			return MasterUtils.GetAttributes (attributes);
		}
		
		public override string GetLiteralValue ()
		{
			return fieldValue;
		}

		string fieldType;
		string fieldValue;
		string fieldAccess;
		XMLAttributes attributes;
	}
	
	public class MasterProperty : CompProperty {
		public MasterProperty (string key, string propertyAccess, XMLMethods xml_methods, XMLAttributes attributes)
			: base (FormatName (key))
		{
			string[] keyparts = key.Split(new char[] {':'}, 3);
			
			this.propertyType = keyparts[1];
			this.propertyAccess = propertyAccess;
			
			methods = new List<CompNamed>();			
			
			MasterUtils.PopulateMethodList (xml_methods, methods);
			this.attributes = attributes;
		}

		public override List<CompNamed> GetAttributes ()
		{
			return MasterUtils.GetAttributes (attributes);
		}
		
		public override string GetMemberType()
		{
			return propertyType;
		}
		
		public override string GetMemberAccess()
		{
			return propertyAccess;
		}
		
		public override List<CompNamed> GetMethods()
		{
			return methods;
		}

		static string FormatName (string key)
		{
			string[] keyparts = key.Split(new char[] {':'}, 3);

			StringBuilder sb = new StringBuilder ();
			sb.Append (keyparts[1]);
			sb.Append (" ");
			sb.Append (keyparts[0]);

			if (keyparts[2] != "")
				sb.AppendFormat ("[{0}]", keyparts[2]);

			return sb.ToString ();
		}
	
		List<CompNamed> methods;
		XMLAttributes attributes;
		string propertyType;
		string propertyAccess;
	}
	
	public class MasterMethod : CompMethod {
		public MasterMethod (string name,
		                     XMLMethods.SignatureFlags signatureFlags,
		                     string returnType,
		                     XMLParameters parameters,
		                     XMLGenericParameters genericParameters,
		                     string methodAccess,
		                     XMLAttributes attributes)
			: base (String.Format ("{0} {1}", returnType, name))
		{
			this.signatureFlags = signatureFlags;
			this.returnType = returnType;
			this.parameters = parameters;
			this.genericParameters = genericParameters;
			// we don't care about the Assembly (internal) part
			this.methodAccess = methodAccess.Replace ("FamORAssem", "Family");
			this.attributes = attributes;
		}

		public override string GetMemberType()
		{
			return returnType;
		}

		public override bool ThrowsNotImplementedException ()
		{
			return false;
		}
		
		public override string GetMemberAccess()
		{
			return methodAccess;
		}
		
		public override List<CompNamed> GetAttributes ()
		{
			return MasterUtils.GetAttributes (attributes);
		}
		
		public override List<CompGenericParameter> GetTypeParameters ()
		{
			return MasterUtils.GetTypeParameters (genericParameters);
		}

		XMLMethods.SignatureFlags signatureFlags;
		string returnType;
		XMLParameters parameters;
		XMLGenericParameters genericParameters;
		string methodAccess;
		XMLAttributes attributes;
	}
			         
	public class MasterAttribute : CompAttribute
	{
		string FormatStringValue (string value)
		{
			if (value == null)
				return "null";

			return "\"" + value + "\"";
		}

		public MasterAttribute (string name, XMLAttributeProperties properties) : base(name)
		{
			var sb = new StringBuilder ();

			if (properties == null)
				return;

			IDictionary<string, string> props = properties.Properties;
			if (props.Count == 0)
				return;

			if (name == "System.Runtime.CompilerServices.TypeForwardedToAttribute") {
				string dest;
				if (props.TryGetValue ("Destination", out dest) && !String.IsNullOrEmpty (dest))
					sb.AppendFormat ("[assembly: TypeForwardedToAttribute (typeof ({0}))]", dest);
			} else {
				sb.Append ("<b>Properties:</b>\n");
				foreach (var prop in props)
					sb.AppendFormat ("\t\t<i>{0}</i> == {1}\n", prop.Key, FormatStringValue (prop.Value));
				sb.Append ('\n');
			}

			ExtraInfo = sb.ToString ();
		}
	}
	
	public class MasterGenericTypeParameter : CompGenericParameter {
		XMLAttributes attributes;
		
		public MasterGenericTypeParameter (string name, string genericAttribute, XMLAttributes attributes)
			: base (name, (Mono.Cecil.GenericParameterAttributes)Enum.Parse (typeof (Mono.Cecil.GenericParameterAttributes), genericAttribute))
		{
			this.attributes = attributes;
		}
		
		public override List<CompNamed> GetAttributes ()
		{
			return MasterUtils.GetAttributes (attributes);
		}
	}
}
