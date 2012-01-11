//
// System.Runtime.Remoting.MetadataServices.MetaDataCodeGenerator
//
// Authors:
//		Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Novell, Inc
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
		Hashtable sudsTypes;
		
		public void GenerateCode (bool clientProxy, string outputDirectory, Stream inputStream, 
			ArrayList outCodeStreamList, string proxyUrl, string proxyNamespace)
		{
			doc = new XmlDocument ();
			doc.Load (inputStream);
			
			nsManager = new XmlNamespaceManager (doc.NameTable);
			nsManager.AddNamespace ("wsdl", MetaData.WsdlNamespace);
			nsManager.AddNamespace ("s", MetaData.SchemaNamespace);
			nsManager.AddNamespace ("suds", MetaData.SudsNamespace);
		
			if (outputDirectory == null) outputDirectory = Directory.GetCurrentDirectory();
			
			CodeFile mainFile = new CodeFile (outputDirectory);
			
			currentFile = mainFile;
			
			// Suds types
			
			sudsTypes = new Hashtable ();
			XmlNodeList nodes = doc.DocumentElement.SelectNodes ("wsdl:binding/suds:class|wsdl:binding/suds:interface|wsdl:binding/suds:struct", nsManager);
			foreach (XmlElement node in nodes)
				sudsTypes [GetTypeQualifiedName (node, node.GetAttribute ("type"))] = node;
			
			// Data types
			
			nodes = doc.SelectNodes ("wsdl:definitions/wsdl:types/s:schema", nsManager);
			foreach (XmlElement schema in nodes)
				GenerateSchemaCode (schema);
			
			// Services
			
			nodes = doc.SelectNodes ("wsdl:definitions/wsdl:service/wsdl:port", nsManager);
			foreach (XmlElement port in nodes)
				GeneratePortCode (port);
			
			mainFile.Write ();
			if (mainFile.FileName != null)
				outCodeStreamList.Add (mainFile.FilePath);
		}
		
		void GeneratePortCode (XmlElement port)
		{
			XmlElement binding = GetBinding (port.GetAttribute ("binding"));
			XmlElement type = null;
			foreach (XmlNode node in binding)
				if ((node is XmlElement) && ((XmlElement)node).NamespaceURI == MetaData.SudsNamespace) 
				{ type = (XmlElement) node; break; }
					
			string rootType = type.GetAttribute ("rootType");
			if (rootType == "Delegate")
				GenerateServiceDelegateCode (port, binding, type);
			else
				GenerateServiceClassCode (port, binding, type);
		}
		
		void GenerateServiceDelegateCode (XmlElement port, XmlElement binding, XmlElement type)
		{
			string typeName = (type != null) ? type.GetAttribute ("type") : port.GetAttribute ("name");
			string portName = GetNameFromQn (binding.GetAttribute ("type"));
			
			string name, ns;
			GetTypeQualifiedName (port, typeName, out name, out ns);
			currentFile.SetCurrentNamespace (ns);
			
			XmlElement oper = (XmlElement) binding.SelectSingleNode ("wsdl:operation[@name='Invoke']", nsManager);
			if (oper == null) throw new InvalidOperationException ("Invalid delegate schema");
			
			string parsDec;
			string returnType;
			GetParameters (oper, portName, "Invoke", out parsDec, out returnType);

			currentFile.WriteLine ("public delegate " + returnType + " " + name + " (" + parsDec + ");");
			currentFile.WriteLine ("");
		}
		
		void GenerateServiceClassCode (XmlElement port, XmlElement binding, XmlElement type)
		{
			string typeName = (type != null) ? type.GetAttribute ("type") : port.GetAttribute ("name");
			
			string name, ns;
			GetTypeQualifiedName (port, typeName, out name, out ns);
			currentFile.SetCurrentNamespace (ns);
			
			string cls = "public " + type.LocalName + " " + name;
			string baset = type.GetAttribute ("extends");
			if (baset != "") cls += ": " + GetTypeQualifiedName (port, baset);
			
			// Interfaces
			
			XmlNodeList interfaces = type.SelectNodes ("suds:implements",nsManager);
			if (interfaces.Count == 0) interfaces = type.SelectNodes ("suds:extends",nsManager);
			
			foreach (XmlElement interf in interfaces)
			{
				string iname = GetTypeQualifiedName (interf, interf.GetAttribute ("type"));
				if (cls.IndexOf (':') == -1)  cls += ": " + iname;
				else cls += ", " + iname;
			}
			
			currentFile.WriteLine (cls);
			currentFile.WriteLineInd ("{");
			
			string portName = GetNameFromQn (binding.GetAttribute ("type"));
			bool isInterface = type.LocalName == "interface";
			
			string vis = isInterface? "":"public ";
			
			ArrayList mets = GetMethods (portName, binding);
			foreach (MethodData met in mets)
			{
				if (met.IsProperty)
				{
					string prop = vis + met.ReturnType + " ";
					if (met.Signature != "") prop += "this [" + met.Signature + "]";
					else prop += met.Name;
					
					if (isInterface)
					{
						prop += " { ";
						if (met.HasGet) prop += "get; ";
						if (met.HasSet) prop += "set; ";
						prop += "}";
						currentFile.WriteLine (prop);
					}
					else
					{
						currentFile.WriteLine (prop);
						currentFile.WriteLineInd ("{");
						if (met.HasGet) currentFile.WriteLine ("get { throw new NotImplementedException (); }");
						if (met.HasSet) currentFile.WriteLine ("set { throw new NotImplementedException (); }");
						currentFile.WriteLineUni ("}");
						currentFile.WriteLine ("");
					}
				}
				else
				{
					currentFile.WriteLine (vis + met.ReturnType + " " + met.Name + " (" + met.Signature + ")" + (isInterface?";":""));
					if (!isInterface)
					{
						currentFile.WriteLineInd ("{");
						currentFile.WriteLine ("throw new NotImplementedException ();");
						currentFile.WriteLineUni ("}");
						currentFile.WriteLine ("");
					}
				}
			}
			
			currentFile.WriteLineUni ("}");
			currentFile.WriteLine ("");
		}
		
		class MethodData
		{
			public string ReturnType;
			public string Signature;
			public string Name;
			public bool HasSet;
			public bool HasGet;
			
			public bool IsProperty { get { return HasGet || HasSet; } }
		}
		
		ArrayList GetMethods (string portName, XmlElement binding)
		{
			ArrayList mets = new ArrayList ();
			
			XmlNodeList nodes = binding.SelectNodes ("wsdl:operation", nsManager);
			foreach (XmlElement oper in nodes)
			{
				MethodData md = new MethodData ();
				md.Name = oper.GetAttribute ("name");
				
				GetParameters (oper, portName, md.Name, out md.Signature, out md.ReturnType);
				
				if (md.Name.StartsWith ("set_") || md.Name.StartsWith ("get_"))
				{
					string tmp = ", " + md.Signature;
					if (tmp.IndexOf (", out ") == -1 && tmp.IndexOf (", ref ") == -1)
					{
						bool isSet = md.Name[0]=='s';
						md.Name = md.Name.Substring (4);
						MethodData previousProp = null;
						
						foreach (MethodData fmd in mets)
							if (fmd.Name == md.Name && fmd.IsProperty)
								previousProp = fmd;
								
						if (previousProp != null) {
							if (isSet) previousProp.HasSet = true;
							else { previousProp.HasGet = true; previousProp.Signature = md.Signature; }
							continue;
						}
						else {
							if (isSet) { md.HasSet = true; md.Signature = ""; }
							else md.HasGet = true;
						}
					}
				}
				
				mets.Add (md);
			}
			return mets;
		}
		
		void GetParameters (XmlElement oper, string portName, string operName, out string signature, out string returnType)
		{
			returnType = null;
			
			XmlElement portType = (XmlElement) doc.SelectSingleNode ("wsdl:definitions/wsdl:portType[@name='" + portName + "']", nsManager);
			XmlElement portOper = (XmlElement) portType.SelectSingleNode ("wsdl:operation[@name='" + operName + "']", nsManager);
			string[] parNames = portOper.GetAttribute ("parameterOrder").Split (' ');
			
			XmlElement inPortMsg = (XmlElement) portOper.SelectSingleNode ("wsdl:input", nsManager);
			XmlElement inMsg = FindMessageFromPortMessage (inPortMsg);
			
			XmlElement outPortMsg = (XmlElement) portOper.SelectSingleNode ("wsdl:output", nsManager);
			XmlElement outMsg = FindMessageFromPortMessage (outPortMsg);

			string[] parameters;
			if (parNames [0] != "") parameters = new string [parNames.Length];
			else parameters = new string [0];
			
			foreach (XmlElement part in inMsg.SelectNodes ("wsdl:part",nsManager))
			{
				int i = Array.IndexOf (parNames, part.GetAttribute ("name"));
				string type = GetTypeQualifiedName (part, part.GetAttribute ("type"));
				parameters [i] = type + " " + parNames [i];
			}
			
			foreach (XmlElement part in outMsg.SelectNodes ("wsdl:part",nsManager))
			{
				string pn = part.GetAttribute ("name");
				string type = GetTypeQualifiedName (part, part.GetAttribute ("type"));
				
				if (pn == "return") 
					returnType = type;
				else {
					int i = Array.IndexOf (parNames, pn);
					if (parameters [i] != null) parameters [i] = "ref " + parameters [i];
					else parameters [i] = "out " + type + " " + pn;
				}
			}
			
			signature = string.Join (", ", parameters);
			if (returnType == null) returnType = "void";
		}
		
		XmlElement FindMessageFromPortMessage (XmlElement portMsg)
		{
			string msgName = portMsg.GetAttribute ("message");
			msgName = GetNameFromQn (msgName);
			return (XmlElement) doc.SelectSingleNode ("wsdl:definitions/wsdl:message[@name='" + msgName + "']", nsManager);
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
			string clrNs = DecodeNamespace (ns);
			string typeName = GetTypeName (elem.GetAttribute ("name"), ns);
			
			XmlElement sudsType = (XmlElement) sudsTypes [clrNs + "." + typeName];
			
			string typetype = "class";
			if (sudsType != null) typetype = sudsType.LocalName;
			
			currentFile.WriteLine ("[Serializable, SoapType (XmlNamespace = @\"" + ns + "\", XmlTypeNamespace = @\"" + ns + "\")]");
			
			string cls = "public " + typetype + " " + typeName;
			string baseType = elem.GetAttribute ("base");
			if (baseType != "") cls += ": " + GetTypeQualifiedName (elem, baseType);
			
			bool isSerializable = (sudsType.GetAttribute ("rootType") == "ISerializable");
			
			if (isSerializable)
			{
				if (cls.IndexOf (':') == -1) cls += ": ";
				else cls += ", ";
				cls += "System.Runtime.Serialization.ISerializable";
			}
			
			currentFile.WriteLine (cls);
			currentFile.WriteLineInd ("{");
			
			XmlNodeList elems = elem.GetElementsByTagName ("element", MetaData.SchemaNamespace);
			foreach (XmlElement elemField in elems)
				WriteField (elemField);
			
			elems = elem.GetElementsByTagName ("attribute", MetaData.SchemaNamespace);
			foreach (XmlElement elemField in elems)
				WriteField (elemField);
			
			if (isSerializable)
			{
				currentFile.WriteLine ("");
				currentFile.WriteLine ("public " + typeName + " ()");
				currentFile.WriteLineInd ("{");
				currentFile.WriteLineUni ("}");
				currentFile.WriteLine ("");
				
				currentFile.WriteLine ("public " + typeName + " (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)");
				currentFile.WriteLineInd ("{");
				currentFile.WriteLine ("throw new NotImplementedException ();");
				currentFile.WriteLineUni ("}");
				currentFile.WriteLine ("");
				
				currentFile.WriteLine ("public void GetObjectData (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)");
				currentFile.WriteLineInd ("{");
				currentFile.WriteLine ("throw new NotImplementedException ();");
				currentFile.WriteLineUni ("}");
			}
			
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
			currentFile.WriteLine ("public enum " + GetTypeName (elem.GetAttribute ("name"), ns));
			currentFile.WriteLineInd ("{");
			
			XmlNodeList nodes = elem.SelectNodes ("s:restriction/s:enumeration/@value", nsManager);
			foreach (XmlNode node in nodes)
				currentFile.WriteLine (node.Value + ",");
				
			currentFile.WriteLineUni ("}");
			currentFile.WriteLine ("");
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
		
		string GetNameFromQn (string qn)
		{
			int i = qn.IndexOf (':');
			if (i == -1) return qn;
			else return qn.Substring (i+1);
		}
	}
	
	class CodeFile
	{
		public string FileName;
		public string Directory;
		public string FilePath;
		Hashtable namespaces = new Hashtable ();
		public StringWriter writer;
		int indent;

		public CodeFile (string directory)
		{
			Directory = directory;
		}
		
		public void SetCurrentNamespace (string ns)
		{
			writer = namespaces [ns] as StringWriter;
			if (writer == null)
			{
				indent = 0;
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
			
			FilePath = Path.Combine (Directory, FileName);
			StreamWriter sw = new StreamWriter (FilePath);
			
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

