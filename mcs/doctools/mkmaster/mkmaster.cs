//	mkmaster.cs
//
//	Adam Treat (manyoso@yahoo.com)
//	(C) 2002 Adam Treat
//
//	MkMaster is based heavily upon the NDoc project
//	ndoc.sourceforge.net
//
//	This program is free software; you can redistribute it and/or modify
//	it under the terms of the GNU General Public License as published by
//	the Free Software Foundation; either version 2 of the License, or
//	(at your option) any later version.


using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Text;

namespace Mono.Util {

	class MkMaster {

		Assembly assembly;
		bool nested;
		string assembly_file, classname, ctorname, currentNamespace;

		void Usage()
		{
			Console.WriteLine ("Usage: 'docstub Assembly.dll'");
		}

		public static void Main(string[] args)
		{
			MakeMasterDoc stub = new MakeMasterDoc (args);
		}

		public MakeMasterDoc (string[] args)
		{
			if (args.Length != 1) {
				Usage ();
				return;
			} else
				assembly_file = args[0];
			
			// Call the main driver to get some things done
			MakeXml();
		}

		// Builds an XmlDocument with the reflected metadata
		private void MakeXml()
		{
			try {
				assembly = LoadAssembly(Path.GetFullPath(assembly_file));
				//assembly = LoadAssembly(assembly_file); //This is for corlib. It's a special case :-)
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
			}
			Write();
		}

		private void Write()
		{
			foreach(Module module in assembly.GetModules()) {
				WriteNamespaces(module);
			}
		}

		private void WriteNamespaces(Module module)
		{
			Type[] types = GetMyTypes(module);
			StringCollection namespaceNames = GetNamespaceNames(types);
			XmlTextWriter writer = StartDocument();

			foreach (string namespaceName in namespaceNames) {
				currentNamespace = namespaceName;
				WriteClasses(writer, types);
				WriteInterfaces(writer, types);
				WriteStructures(writer, types);
				WriteDelegates(writer, types);
				WriteEnumerations(writer, types);
			}
			EndDocument(writer);
		}

		private XmlTextWriter StartDocument()
		{
			string filename = assembly.GetName().Name+ ".xml";
   			XmlTextWriter writer = new XmlTextWriter (filename, new UTF8Encoding());
			writer.Formatting = Formatting.Indented;
			writer.Indentation=4;
			writer.WriteStartDocument();
			writer.WriteStartElement("masterdoc");
			writer.WriteAttributeString("assembly", assembly.GetName().Name);
			return writer;
		}

		private void EndDocument(XmlTextWriter writer)
		{
			writer.WriteEndElement();
			writer.WriteEndDocument();
			nested = false;
			writer.Close();
		}

		private bool IsDelegate(Type type)
		{
			try {
				return type.BaseType.FullName == "System.Delegate" ||
				type.BaseType.FullName == "System.MulticastDelegate";
			} catch {
				return false;
			}
		}

		private string GetTypeName(Type type)
		{
			return type.FullName.Replace('+', '.');
		}

		private StringCollection GetNamespaceNames(Type[] types)
		{
			StringCollection namespaceNames = new StringCollection();
			foreach (Type type in types) {
				if (!namespaceNames.Contains(type.Namespace)) {
					namespaceNames.Add(type.Namespace);
				}
			}
			return namespaceNames;
		}

		private bool IsAlsoAnEvent(Type type, string fullName)
		{
			bool isEvent = false;

			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			foreach (EventInfo eventInfo in type.GetEvents(bindingFlags)) {

				if (eventInfo.EventHandlerType.FullName == fullName) {

					isEvent = true;
					break;
				}
			}

			return isEvent;
		}

		private bool IsAlsoAnEvent(FieldInfo field)
		{
			return IsAlsoAnEvent(field.DeclaringType, field.FieldType.FullName);
		}

		private bool IsAlsoAnEvent(PropertyInfo property)
		{
			return IsAlsoAnEvent(property.DeclaringType, property.PropertyType.FullName);
		}

		// Loads an assembly.
		public static Assembly LoadAssembly(string filename)
		{
			if (!File.Exists(filename)) {
				throw new ApplicationException("can't find assembly " + filename);
			}

			FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read);
			byte[] buffer = new byte[fs.Length];
			fs.Read(buffer, 0, (int)fs.Length);
			fs.Close();

			return Assembly.Load(buffer);
			//AppDomain app = AppDomain.CurrentDomain; //This is for corlib. It's a special case :-)
			//return app.Load (filename); //This is for corlib. It's a special case :-)
		}

		private Type[] GetMyTypes(Module module)
		{
			int i = 0;
			Type[] temp;
			try {
				temp = module.GetTypes();
			} catch (ReflectionTypeLoadException e) {
				temp = e.Types;
			}
			ArrayList list = new ArrayList();
			foreach (Type type in temp) {
				if (MustDocumentType(type)) {
     				list.Add(type);
				}
			}
			Type[] types = new Type[list.Count];
			foreach (Type type in list) {
				types[i++] = type;
			}
			return types;
		}

		private bool MustDocumentType(Type type)
		{
			if (type != null) {
				return ((type.IsPublic ||
						type.IsNestedPublic) &&
						!type.FullName.Equals("Driver") &&
						!type.FullName.Equals("Profile"));
			} else {
				return false;
			}
		}

		private bool MustDocumentMethod(MethodBase method)
		{
			return (method.IsPublic || method.IsFamily);
		}

		private bool MustDocumentField(FieldInfo field)
		{
			return (field.IsPublic || field.IsFamily);
		}

		private string GetParameterTypes(ParameterInfo[] parameters)
		{
			if (parameters.Length != 0) {
				StringBuilder sb = new StringBuilder();
				sb.Append("(");
				foreach (ParameterInfo parameter in parameters) {

					sb.Append(GetTypeName(parameter.ParameterType) + ", ");
				}
				sb.Remove(sb.Length-2, 2);
				sb.Append(")");
				return sb.ToString();
			} else {
				return "";
			}
		}

		private string GetParameterNames(ParameterInfo[] parameters)
		{
			if (parameters.Length != 0) {
				StringBuilder sb = new StringBuilder();
				foreach (ParameterInfo parameter in parameters) {

					sb.Append(parameter.Name + ", ");
				}
				sb.Remove(sb.Length-2, 2);
				return sb.ToString();
			} else {
				return "";
			}
		}

		private void WriteClasses(XmlTextWriter writer, Type[] types)
		{
			foreach (Type type in types) {
				if (type.IsClass && !IsDelegate(type) && type.Namespace.Equals(currentNamespace)) {
					classname = type.FullName;
					ctorname = type.Name;
					if (!nested) {
						WriteClass(writer, type);
					} else {
						WriteClass(writer, type);
					}
				}
			}
		}

		private void WriteInterfaces(XmlTextWriter writer, Type[] types)
		{
  			foreach (Type type in types) {

				if (type.IsInterface && type.Namespace.Equals(currentNamespace)) {

					classname = type.FullName;
					ctorname = type.Name;
					if (!nested) {
						WriteInterface(writer, type);
					} else {
						WriteInterface(writer, type);
					}
				}
			}
		}

		private void WriteStructures(XmlTextWriter writer, Type[] types)
		{
			foreach (Type type in types) {

				if (type.IsValueType && !type.IsEnum && type.Namespace.Equals(currentNamespace)) {

					classname = type.FullName;
					ctorname = type.Name;
					if (!nested) {
						WriteClass(writer, type);
					} else {
						WriteClass(writer, type);
					}
				}
			}
		}

		private void WriteDelegates(XmlTextWriter writer, Type[] types)
		{
			foreach (Type type in types) {

				if (type.IsClass && IsDelegate(type) && type.Namespace.Equals(currentNamespace)) {

					classname = type.FullName;
					ctorname = type.Name;
					if (!nested) {
						WriteDelegate(writer, type);
					} else {
						WriteDelegate(writer, type);
					}
				}
			}
		}

		private void WriteEnumerations(XmlTextWriter writer, Type[] types)
		{
			foreach (Type type in types) {

				if (type.IsEnum && type.Namespace.Equals(currentNamespace)) {

					classname = type.FullName;
					ctorname = type.Name;
					if (!nested) {
						WriteEnumeration(writer, type);
					} else {
						WriteEnumeration(writer, type);
					}
				}
			}
		}

		// Writes XML documenting a class or struct.
		private void WriteClass(XmlTextWriter writer, Type type)
		{
			Type[] types = type.GetNestedTypes();
			AssemblyName assemblyName = assembly.GetName();
			bool isStruct = type.IsValueType;
			nested = false;

			writer.WriteStartElement(isStruct ? "struct" : "class");
			writer.WriteAttributeString("name", type.Name);
			writer.WriteAttributeString("namespace", type.Namespace);
			WriteClasses(writer, types);
			WriteInterfaces(writer, types);
			WriteStructures(writer, types);
			WriteDelegates(writer, types);
			WriteEnumerations(writer, types);
			WriteConstructors(writer, type);
			WriteFields(writer, type);
			WriteProperties(writer, type);
			WriteMethods(writer, type);
			WriteOperators(writer, type);
			WriteEvents(writer, type);

			writer.WriteEndElement();
		}

		// Writes XML documenting an interface.
		private void WriteInterface(XmlTextWriter writer, Type type)
		{
			AssemblyName assemblyName = assembly.GetName();

			writer.WriteStartElement("interface");
			writer.WriteAttributeString("name", type.Name);
			writer.WriteAttributeString("namespace", type.Namespace);
			WriteConstructors(writer, type);
			WriteFields(writer, type);
			WriteProperties(writer, type);
			WriteMethods(writer, type);
			WriteOperators(writer, type);
			WriteEvents(writer, type);
			writer.WriteEndElement();
		}

		// Writes XML documenting a delegate.
		private void WriteDelegate(XmlTextWriter writer, Type type)
		{
			AssemblyName assemblyName = assembly.GetName();

			writer.WriteStartElement("delegate");
			writer.WriteAttributeString("name", type.Name);
			writer.WriteAttributeString("namespace", type.Namespace);
			WriteConstructors(writer, type);
			WriteFields(writer, type);
			WriteProperties(writer, type);
			WriteMethods(writer, type);
			WriteOperators(writer, type);
			WriteEvents(writer, type);
			writer.WriteEndElement();
		}

		// Writes XML documenting an enumeration.
		private void WriteEnumeration(XmlTextWriter writer, Type type)
		{
			Type[] types = type.GetNestedTypes();
			AssemblyName assemblyName = assembly.GetName();

			writer.WriteStartElement("enum");
			writer.WriteAttributeString("name", type.Name);
			writer.WriteAttributeString("namespace", type.Namespace);
			WriteConstructors(writer, type);
			WriteFields(writer, type);
			WriteProperties(writer, type);
			WriteMethods(writer, type);
			WriteOperators(writer, type);
			WriteEvents(writer, type);
			writer.WriteEndElement();
		}

		private void WriteConstructors(XmlTextWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			ConstructorInfo[] constructors = type.GetConstructors(bindingFlags);

			foreach (ConstructorInfo constructor in constructors) {

				if (MustDocumentMethod(constructor)) {

					WriteConstructor(writer, constructor, type);
				}
			}
		}

		private void WriteFields(XmlTextWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			foreach (FieldInfo field in type.GetFields(bindingFlags)) {

				if (!IsAlsoAnEvent(field) && MustDocumentField(field) && field.Name != "value__") {

					WriteField(writer, field, type);
				}
			}
		}

		private void WriteProperties(XmlTextWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			PropertyInfo[] properties = type.GetProperties(bindingFlags);

			foreach (PropertyInfo property in properties) {

				MethodInfo getMethod = property.GetGetMethod(true);
				MethodInfo setMethod = property.GetSetMethod(true);

				bool hasGetter = (getMethod != null);
				bool hasSetter = (setMethod != null);

				if ((hasGetter || hasSetter) && !IsAlsoAnEvent(property)) {

					WriteProperty(writer, property, type);
				}
			}
		}

		private void WriteMethods(XmlTextWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			MethodInfo[] methods = type.GetMethods(bindingFlags);

			foreach (MethodInfo method in methods) {

				if (!(method.Name.StartsWith("get_")) &&
					!(method.Name.StartsWith("set_")) &&
					!(method.Name.StartsWith("add_")) &&
					!(method.Name.StartsWith("remove_")) &&
					!(method.Name.StartsWith("op_")) && MustDocumentMethod(method))
				{
					WriteMethod(writer, method, type);
				}
			}
		}

		private void WriteOperators(XmlTextWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			MethodInfo[] methods = type.GetMethods(bindingFlags);

			foreach (MethodInfo method in methods) {

				if (method.Name.StartsWith("op_") && MustDocumentMethod(method)) {

					WriteOperator(writer, method, type);
				}
			}
		}

		private void WriteEvents(XmlTextWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			foreach (EventInfo eventInfo in type.GetEvents(bindingFlags)) {

				MethodInfo addMethod = eventInfo.GetAddMethod(true);

				if (addMethod != null && MustDocumentMethod(addMethod)) {

					WriteEvent(writer, eventInfo, type);
				}
			}
		}

		// Writes XML documenting a field.
		private void WriteField(XmlTextWriter writer, FieldInfo field, Type type)
		{
			writer.WriteStartElement("field");
			writer.WriteAttributeString("name", field.Name);
			if (field.DeclaringType.FullName != type.FullName)
				writer.WriteAttributeString("inherited", field.DeclaringType.FullName);
			writer.WriteEndElement();
		}

		// Writes XML documenting an event.
		private void WriteEvent(XmlTextWriter writer, EventInfo eventInfo, Type type)
		{
			writer.WriteStartElement("event");
			writer.WriteAttributeString("name", eventInfo.Name);
			if (eventInfo.DeclaringType.FullName != type.FullName)
				writer.WriteAttributeString("inherited", eventInfo.DeclaringType.FullName);
			writer.WriteEndElement();
		}

		// Writes XML documenting a constructor.
		private void WriteConstructor(XmlTextWriter writer, ConstructorInfo constructor, Type type)
		{
			writer.WriteStartElement("constructor");
			writer.WriteAttributeString("name", ctorname + GetParameterTypes(constructor.GetParameters()));
			writer.WriteAttributeString("argnames", GetParameterNames(constructor.GetParameters()));
			if (constructor.DeclaringType.FullName != type.FullName)
				writer.WriteAttributeString("inherited", constructor.DeclaringType.FullName);
			writer.WriteEndElement();
		}

		// Writes XML documenting a property.
		private void WriteProperty(XmlTextWriter writer, PropertyInfo property, Type type)
		{
			writer.WriteStartElement("property");
			writer.WriteAttributeString("name", property.Name);
			if (property.DeclaringType.FullName != type.FullName)
				writer.WriteAttributeString("inherited", property.DeclaringType.FullName);

			writer.WriteAttributeString("propertytype", property.PropertyType.FullName);
			writer.WriteEndElement();
		}

		// Writes XML documenting an operator.
		private void WriteOperator(XmlTextWriter writer, MethodInfo method, Type type)
		{
			if (method != null) {

				writer.WriteStartElement("operator");
				writer.WriteAttributeString("name", method.Name + GetParameterTypes(method.GetParameters()));
				writer.WriteAttributeString("argnames", GetParameterNames(method.GetParameters()));
			if (method.DeclaringType.FullName != type.FullName)
					writer.WriteAttributeString("inherited", method.DeclaringType.FullName);
				writer.WriteEndElement();
			}
		}

		// Writes XML documenting a method.
		private void WriteMethod(XmlTextWriter writer, MethodInfo method, Type type)
		{
			writer.WriteStartElement("method");
			writer.WriteAttributeString("name", method.Name + GetParameterTypes(method.GetParameters()));
			writer.WriteAttributeString("argnames", GetParameterNames(method.GetParameters()));
			if (method.DeclaringType.FullName != type.FullName)
				writer.WriteAttributeString("inherited", method.DeclaringType.FullName);
			writer.WriteAttributeString("returntype", method.ReturnType.FullName);
				
			writer.WriteEndElement();
		}
	}
}
