//
// System.Runtime.Remoting.MetadataServices.MetaDataCodeGenerator
//
// Authors:
//		Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Novell, Inc
//

using System.Collections;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata;

namespace System.Runtime.Remoting.MetadataServices
{
	internal class MetaDataCodeGenerator
	{
		XmlDocument doc;
		CodeFile currentFile;
		XmlNamespaceManager nsManager;
		
		public void GenerateCode (bool clientProxy, string outputDirectory, Stream inputStream, 
			ArrayList outCodeStreamList, string proxyUrl, string proxyNamespace)
		{
			doc = new XmlDocument ();
			doc.Load (inputStream);
			
			nsManager = new XmlNamespaceManager (doc.NameTable);
			nsManager.AddNamespace ("wsdl", MetaData.WsdlNamespace);
			nsManager.AddNamespace ("s", MetaData.SchemaNamespace);
			nsManager.AddNamespace ("suds", MetaData.SudsNamespace);
		
			if (outputDirectory == null) outputDirectory = "";
			
			CodeFile mainFile = new CodeFile (outputDirectory);
			CodeFile interopFile = new CodeFile (outputDirectory);
			
			currentFile = mainFile;
			
			XmlNodeList nodes = doc.SelectNodes ("wsdl:definitions/wsdl:types/s:schema", nsManager);
			foreach (XmlElement schema in nodes)
				GenerateSchemaCode (schema);
			
			nodes = doc.SelectNodes ("wsdl:definitions/wsdl:service/wsdl:port", nsManager);
			foreach (XmlElement port in nodes)
				GeneratePortCode (port);
			
			mainFile.Write ();
			if (mainFile.FileName != null)
				outCodeStreamList.Add (mainFile.FileName);
		}
		
		void GeneratePortCode (XmlElement port)
		{
			XmlElement binding = GetBinding (port.GetAttribute ("binding"));
			XmlElement type = (XmlElement) binding.SelectSingleNode ("suds:class", nsManager);
			if (type == null) type = (XmlElement) binding.SelectSingleNode ("suds:interface", nsManager);
			
			string typeName = (type != null) ? type.GetAttribute ("type") : port.GetAttribute ("name");
			
			
			string name, ns;
			GetTypeQualifiedName (port, typeName, out name, out ns);
			currentFile.SetCurrentNamespace (ns);
			
			string cls = "public " + type.LocalName + " " + name;
			currentFile.WriteLine (cls);
			currentFile.WriteLineInd ("{");
			currentFile.WriteLineUni ("}");
			currentFile.WriteLine ("");
		}

		
		void GenerateSchemaCode (XmlElement schema)
		{
			string ns = schema.GetAttribute ("targetNamespace");
			string clrNs = DecodeNamespace (ns);
			currentFile.SetCurrentNamespace (clrNs);
			
			foreach (XmlNode node in schema)
			{
				XmlElement elem = node as XmlElement;
				if (elem == null) continue;
				
				if (elem.LocalName == "complexType")
					GenerateClassCode (ns, elem);
				else if (elem.LocalName == "simpleType")
					GenerateEnumCode (ns, elem);
			}
		}
		
		void GenerateClassCode (string ns, XmlElement elem)
		{
			if (elem.SelectSingleNode ("s:complexContent/s:restriction", nsManager) != null) return;
			
			currentFile.WriteLine ("[Serializable, SoapType (XmlNamespace = @\"" + ns + "\", XmlTypeNamespace = @\"" + ns + "\")]");
			
			string cls = "public class " + GetTypeName (elem.GetAttribute ("name"), ns);
			string baseType = elem.GetAttribute ("base");
			if (baseType != "") cls += ": " + GetTypeQualifiedName (elem, baseType);
			currentFile.WriteLine (cls);
			currentFile.WriteLineInd ("{");
			
			XmlNodeList elems = elem.GetElementsByTagName ("element", MetaData.SchemaNamespace);
			foreach (XmlElement elemField in elems)
				WriteField (elemField);
			
			elems = elem.GetElementsByTagName ("attribute", MetaData.SchemaNamespace);
			foreach (XmlElement elemField in elems)
				WriteField (elemField);
				
			currentFile.WriteLineUni ("}");
			currentFile.WriteLine ("");
		}
		
		void WriteField (XmlElement elemField)
		{
			bool isAttr = elemField.LocalName == "attribute";
			
			string type = elemField.GetAttribute ("type");
			
			if (isAttr)
				currentFile.WriteLine ("[SoapField (UseAttribute = true)]");
			else if (!IsPrimitive (elemField, type))
				currentFile.WriteLine ("[SoapField (Embedded = true)]");
			currentFile.WriteLine ("public " + GetTypeQualifiedName (elemField, type) + " " + elemField.GetAttribute ("name") + ";");
		}
		
		void GenerateEnumCode (string ns, XmlElement elem)
		{
		}
		
		bool IsPrimitive (XmlNode node, string qname)
		{
			string name = GetTypeQualifiedName (node, qname);
			return name.IndexOf ('.') == -1;
		}
		
		string GetTypeName (string localName, string ns)
		{
			return localName;
		}
		
		void GetTypeQualifiedName (XmlNode node, string qualifiedName, out string name, out string ns)
		{
			int i = qualifiedName.IndexOf (':');
			if (i == -1)
			{
				name = qualifiedName;
				ns = "";
				return;
			}
			
			string prefix = qualifiedName.Substring (0,i);
			name = qualifiedName.Substring (i+1);
			ns = node.GetNamespaceOfPrefix (prefix);
			
			string arrayType = GetArrayType (node, name, ns);
			if (arrayType != null) {
				name = arrayType;
				ns = "";
			}
			else if (ns != MetaData.SchemaNamespace) {
				ns = DecodeNamespace (ns);
			}
			else {
				ns = "";
				name = GetClrFromXsd (name);
			}
		}
		
		string GetClrFromXsd (string type)
		{
			switch (type)
			{
				case "boolean": return "bool";
				case "unsignedByte": return "byte";
				case "char": return "char";
				case "dateTime": return "DateTime";
				case "decimal": return "decimal";
				case "double": return "double";
				case "short": return "short";
				case "int": return "int";
				case "long": return "long";
				case "byte": return "sbyte";
				case "float": return "float";
				case "unsignedShort": return "ushort";
				case "unsignedInt": return "uint";
				case "unsignedLong": return "ulong";
				case "string": return "string";
				case "duration": return "TimeSpan";
				case "anyType": return "object";
			}
			throw new InvalidOperationException ("Unknown schema type: " + type);
		}
		
		string GetTypeQualifiedName (XmlNode node, string qualifiedName)
		{
			string name, ns;
			GetTypeQualifiedName (node, qualifiedName, out name, out ns);
			if (ns != "") return ns + "." + name;
			else return name;
		}
		
		string GetTypeNamespace (XmlNode node, string qualifiedName)
		{
			string name, ns;
			GetTypeQualifiedName (node, qualifiedName, out name, out ns);
			return ns;
		}
		
		string GetArrayType (XmlNode node, string name, string ns)
		{
			XmlNode anod = doc.SelectSingleNode ("wsdl:definitions/wsdl:types/s:schema[@targetNamespace='" + ns + "']/s:complexType[@name='" + name + "']/s:complexContent/s:restriction/s:attribute/@wsdl:arrayType", nsManager);
			if (anod == null) return null;
			
			string atype = anod.Value;
			int i = atype.IndexOf ('[');
			string itemType = GetTypeQualifiedName (node, atype.Substring (0,i));
			
			return itemType + atype.Substring (i);
		}
		
		XmlElement GetBinding (string name)
		{
			int i = name.IndexOf (':');
			name = name.Substring (i+1);
			return doc.SelectSingleNode ("wsdl:definitions/wsdl:binding[@name='" + name + "']", nsManager) as XmlElement;
		}
		
		string DecodeNamespace (string xmlNamespace)
		{
			string tns, tasm;
			
			if (!SoapServices.DecodeXmlNamespaceForClrTypeNamespace (xmlNamespace, out tns, out tasm))
				tns = xmlNamespace;
				
			return tns;
		}
		
		string GetLiteral (object ob)
		{
			if (ob == null) return "null";
			if (ob is string) return "\"" + ob.ToString().Replace("\"","\"\"") + "\"";
			if (ob is bool) return ((bool)ob) ? "true" : "false";
			if (ob is XmlQualifiedName) {
				XmlQualifiedName qn = (XmlQualifiedName)ob;
				return "new XmlQualifiedName (" + GetLiteral(qn.Name) + "," + GetLiteral(qn.Namespace) + ")";
			}
			else return ob.ToString ();
		}
		
		string Params (params string[] pars)
		{
			string res = "";
			foreach (string p in pars)
			{
				if (res != "") res += ", ";
				res += p;
			}
			return res;
		}
	}
	
	class CodeFile
	{
		public string FileName;
		public string Directory;
		Hashtable namespaces = new Hashtable ();
		public StringWriter writer;
		int indent;
		
		string currentNamespace;
		
		public CodeFile (string directory)
		{
			Directory = directory;
		}
		
		public void SetCurrentNamespace (string ns)
		{
			writer = namespaces [ns] as StringWriter;
			if (writer == null)
			{
				writer = new StringWriter ();
				namespaces [ns] = writer;
				WriteLine ("namespace " + ns);
				WriteLineInd ("{");
			}
			
			indent = 1;
			
			if (FileName == null)
				FileName = ns + ".cs";
		}
		
		public void WriteLineInd (string code)
		{
			WriteLine (code);
			indent++;
		}
		
		public void WriteLineUni (string code)
		{
			if (indent > 0) indent--;
			WriteLine (code);
		}
		
		public void WriteLine (string code)
		{
			if (code != "")	writer.Write (new String ('\t',indent));
			writer.WriteLine (code);
		}
		
		public void Write ()
		{
			if (FileName == null) return;
			
			string fn = Path.Combine (Directory, FileName);
			StreamWriter sw = new StreamWriter (fn);
			
			sw.WriteLine ("using System;");
			sw.WriteLine ("using System.Runtime.Remoting.Metadata;");
			sw.WriteLine ();
			
			foreach (StringWriter nsWriter in namespaces.Values)
			{
				sw.Write (nsWriter.ToString ());
				sw.WriteLine ("}");
				sw.WriteLine ();
			}
				
			sw.Close ();
		}
	}
	
	
}

