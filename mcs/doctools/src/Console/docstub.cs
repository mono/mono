//	docstub.cs
//
//	Adam Treat (manyoso@yahoo.com)
//	(C) 2002 Adam Treat
//
//	DocStub is based heavily upon the NDoc project
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

	class DocStub {

		Assembly assembly;
		bool nested;
		string assembly_file, directory, language, classname, currentNamespace, docname;

		void Usage()
		{
			Console.Write (
				"docstub -l <lang> -d <directory> -a <assembly>\n\n" +
				"   -d || /-d || --dir       <directory>             The directory to write the xml files to.\n" +
				"   -a || /-a || --assembly  <assembly>              Specifies the target assembly to load and parse.\n" +
				"   -l || /-l || --language  <two-letter ISO code>   Specifies the language encoding.\n\n");
		}

		public static void Main(string[] args)
		{
			DocStub stub = new DocStub(args);
		}

		public DocStub(string [] args)
		{
			assembly_file = null;
			directory = null;
			int argc = args.Length;

			for(int i = 0; i < argc; i++) {

				string arg = args[i];

				// The "/" switch is there for wine users, like me ;-)
				if(arg.StartsWith("-") || arg.StartsWith("/")) {

					switch(arg) {


					case "-d": case "/-d": case "--directory":
						if((i + 1) >= argc) {

							Usage();
							return;
						}
						directory = args[++i];
						continue;

					case "-a": case "/-a": case "--assembly":
						if((i + 1) >= argc) {

							Usage();
							return;
						}
						assembly_file = args[++i];
						continue;
					case "-l": case "/-l": case "--language":
						if((i + 1) >= argc) {

							Usage();
							return;
						}
						language = args[++i];
						continue;

					default:
						Usage();
						return;
					}
				}
			}

			if(assembly_file == null) {

				Usage();
				return;
			} else if(directory == null) {

				Usage();
				return;
			}

			if (!Directory.Exists(directory) && directory != null) {

                Directory.CreateDirectory(directory);
            }

			// Call the main driver to get some things done
			MakeXml();
		}

		// Builds an XmlDocument with the reflected metadata
		private void MakeXml()
		{
			try {

				assembly = LoadAssembly(Path.GetFullPath(assembly_file));
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
			Type[] types = module.GetTypes();
			StringCollection namespaceNames = GetNamespaceNames(types);
			XmlTextWriter dummy = new XmlTextWriter(".temp.xml", null);
			foreach (string namespaceName in namespaceNames) {

				currentNamespace = namespaceName;
				WriteClasses(dummy, types);
				WriteInterfaces(dummy, types);
				WriteStructures(dummy, types);
				WriteDelegates(dummy, types);
				WriteEnumerations(dummy, types);
			}

            dummy.Close();
			File.Delete(".temp.xml");
		}

		private XmlTextWriter StartDocument()
		{
			if (!Directory.Exists(directory+"/"+currentNamespace) && directory != null) {

                Directory.CreateDirectory(directory+"/"+currentNamespace);
            }
			string filename = directory+"/"+currentNamespace+"/"+docname+".xml";
   			XmlTextWriter writer = new XmlTextWriter (filename, null);
			writer.Formatting = Formatting.Indented;
			writer.Indentation=4;
			writer.WriteStartDocument();
			writer.WriteDocType("monodoc", null, "http://www.go-mono.org/monodoc.dtd", null);
			writer.WriteStartElement("monodoc");
			writer.WriteAttributeString("language",language);
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
			return type.BaseType.FullName == "System.Delegate" ||
				type.BaseType.FullName == "System.MulticastDelegate";
		}

		private string GetTypeName(Type type)
		{
			return type.FullName.Replace('+', '.');
		}

		private StringCollection GetNamespaceNames(Type[] types)
		{
			StringCollection namespaceNames = new StringCollection();

			foreach (Type type in types) {

				if (namespaceNames.Contains(type.Namespace) == false) {

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
		}

		private bool MustDocumentType(Type type)
		{
			return (type.IsPublic || type.IsNestedPublic);
		}

		private bool MustDocumentMethod(MethodBase method)
		{
			return (method.IsPublic);
		}

		private bool MustDocumentField(FieldInfo field)
		{
			return (field.IsPublic);
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

		private void WriteClasses(XmlTextWriter writer, Type[] types)
		{
			foreach (Type type in types) {

				if (type.IsClass && !IsDelegate(type) && type.Namespace.Equals(currentNamespace) && MustDocumentType(type)) {

					classname = type.FullName;
					docname = type.Name;
					if (!nested) {

						writer = StartDocument();
						WriteClass(writer, type);
						EndDocument(writer);
					} else {
						WriteClass(writer, type);
					}
				}
			}
		}

		private void WriteInterfaces(XmlTextWriter writer, Type[] types)
		{
  			foreach (Type type in types) {

				if (type.IsInterface && type.Namespace.Equals(currentNamespace) && MustDocumentType(type)) {

					classname = type.FullName;
					docname = type.Name;
					if (!nested) {

						writer = StartDocument();
						WriteInterface(writer, type);
						EndDocument(writer);
					} else {
						WriteInterface(writer, type);
					}
				}
			}
		}

		private void WriteStructures(XmlTextWriter writer, Type[] types)
		{
			foreach (Type type in types) {

				if (type.IsValueType && !type.IsEnum && type.Namespace.Equals(currentNamespace) && MustDocumentType(type)) {

					classname = type.FullName;
					docname = type.Name;
					if (!nested) {

						writer = StartDocument();
						WriteClass(writer, type);
						EndDocument(writer);
					} else {
						WriteClass(writer, type);
					}
				}
			}
		}

		private void WriteDelegates(XmlTextWriter writer, Type[] types)
		{
			foreach (Type type in types) {

				if (type.IsClass && IsDelegate(type) && type.Namespace.Equals(currentNamespace) && MustDocumentType(type)) {

					classname = type.FullName;
					docname = type.Name;
					if (!nested) {

						writer = StartDocument();
						WriteDelegate(writer, type);
						EndDocument(writer);
					} else {
						WriteDelegate(writer, type);
					}
				}
			}
		}

		private void WriteEnumerations(XmlTextWriter writer, Type[] types)
		{
			foreach (Type type in types) {

				if (type.IsEnum && type.Namespace.Equals(currentNamespace) && MustDocumentType(type)) {

					classname = type.FullName;
					docname = type.Name;
					if (!nested) {

						writer = StartDocument();
						WriteEnumeration(writer, type);
						EndDocument(writer);
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
			writer.WriteAttributeString("name", type.FullName);
			writer.WriteAttributeString("assembly", assemblyName.Name);
			writer.WriteElementString("summary","TODO");
			writer.WriteElementString("remarks","TODO");

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
			Type[] types = type.GetNestedTypes();
			AssemblyName assemblyName = assembly.GetName();

			writer.WriteStartElement("interface");
			writer.WriteAttributeString("name", type.FullName);
			writer.WriteAttributeString("assembly", assemblyName.Name);
			writer.WriteElementString("summary","TODO");
			writer.WriteElementString("remarks","TODO");
			writer.WriteEndElement();
		}

		// Writes XML documenting a delegate.
		private void WriteDelegate(XmlTextWriter writer, Type type)
		{
			Type[] types = type.GetNestedTypes();
			AssemblyName assemblyName = assembly.GetName();

			writer.WriteStartElement("delegate");
			writer.WriteAttributeString("name", type.FullName);
			writer.WriteAttributeString("assembly", assemblyName.Name);
			writer.WriteElementString("summary","TODO");
			writer.WriteElementString("remarks","TODO");

			//param

			writer.WriteEndElement();
		}

		// Writes XML documenting an enumeration.
		private void WriteEnumeration(XmlTextWriter writer, Type type)
		{
			Type[] types = type.GetNestedTypes();
			AssemblyName assemblyName = assembly.GetName();

			writer.WriteStartElement("enum");
			writer.WriteAttributeString("name", type.FullName);
			writer.WriteAttributeString("assembly", assemblyName.Name);
			writer.WriteElementString("summary","TODO");
			writer.WriteElementString("remarks","TODO");

			writer.WriteStartElement("member");
			writer.WriteAttributeString("name", "TODO");
			writer.WriteEndElement();

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

					WriteConstructor(writer, constructor);
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

				if (!IsAlsoAnEvent(field) && MustDocumentField(field)) {

					WriteField(writer, field);
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

					WriteProperty(writer, property, property.DeclaringType.FullName != type.FullName);
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
					WriteMethod(writer, method, method.DeclaringType.FullName != type.FullName);
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

					WriteOperator(writer, method);
				}
			}
		}

		private void WriteEvents(XmlTextWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			foreach (EventInfo eventInfo in type.GetEvents(bindingFlags)) {

				MethodInfo addMethod = eventInfo.GetAddMethod(true);

				if (addMethod != null && MustDocumentMethod(addMethod)) {

					WriteEvent(writer, eventInfo);
				}
			}
		}

		// Writes XML documenting a field.
		private void WriteField(XmlTextWriter writer, FieldInfo field)
		{
			writer.WriteStartElement("field");
			writer.WriteAttributeString("name", field.Name);
			writer.WriteElementString("summary","TODO");
			writer.WriteElementString("remarks","TODO");
			writer.WriteEndElement();
		}

		// Writes XML documenting an event.
		private void WriteEvent(XmlTextWriter writer, EventInfo eventInfo)
		{
			writer.WriteStartElement("event");
			writer.WriteAttributeString("name", eventInfo.Name);
			writer.WriteElementString("summary","TODO");
			writer.WriteElementString("remarks","TODO");
			writer.WriteElementString("data","TODO");

			writer.WriteEndElement();
		}

		// Writes XML documenting a constructor.
		private void WriteConstructor(XmlTextWriter writer, ConstructorInfo constructor)
		{
			writer.WriteStartElement("constructor");
			writer.WriteAttributeString("name", docname + GetParameterTypes(constructor.GetParameters()));
			writer.WriteElementString("summary","TODO");
			writer.WriteElementString("remarks","TODO");

			foreach (ParameterInfo parameter in constructor.GetParameters()) {

				WriteParameter(writer, parameter);
			}


			writer.WriteEndElement();
		}

		// Writes XML documenting a property.
		private void WriteProperty(XmlTextWriter writer, PropertyInfo property, bool inherited )
		{
			if (!inherited) {

				writer.WriteStartElement("property");
				writer.WriteAttributeString("name", property.Name);
				writer.WriteElementString("summary","TODO");
				writer.WriteElementString("remarks","TODO");
				writer.WriteElementString("value","TODO");

				writer.WriteEndElement();
			}
		}

		// Writes XML documenting an operator.
		private void WriteOperator(XmlTextWriter writer, MethodInfo method)
		{
			if (method != null) {

				writer.WriteStartElement("operator");
				writer.WriteAttributeString("name", method.Name + GetParameterTypes(method.GetParameters()));
				writer.WriteElementString("summary","TODO");
				writer.WriteElementString("remarks","TODO");

				foreach (ParameterInfo parameter in method.GetParameters()) {

					WriteParameter(writer, parameter);
				}

				writer.WriteElementString("returns", "TODO");

				writer.WriteEndElement();
			}
		}

		// Writes XML documenting a method.
		private void WriteMethod(XmlTextWriter writer, MethodInfo method, bool inherited)
		{
			if (!inherited && method != null) {

				writer.WriteStartElement("method");
				writer.WriteAttributeString("name", method.Name + GetParameterTypes(method.GetParameters()));
				writer.WriteElementString("summary","TODO");
				writer.WriteElementString("remarks","TODO");

				foreach (ParameterInfo parameter in method.GetParameters()) {

					WriteParameter(writer, parameter);
				}

				writer.WriteElementString("returns", "TODO");

				writer.WriteEndElement();
			}
		}

		private void WriteParameter(XmlTextWriter writer, ParameterInfo parameter)
		{
			writer.WriteStartElement("param");
			writer.WriteAttributeString("name", parameter.Name);
			writer.WriteString("TODO");
			writer.WriteEndElement();
		}
	}
}
