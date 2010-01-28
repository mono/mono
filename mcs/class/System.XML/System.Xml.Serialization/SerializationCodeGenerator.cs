//
// System.Xml.Serialization.SerializationCodeGenerator.cs: 
//
// Authors:
//   Lluis Sanchez Gual (lluis@ximian.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2006 Novell, Inc.
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

using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Collections;
using System.Globalization;
using System.Text;

using HookDir = System.Xml.Serialization.XmlMappingAccess;

namespace System.Xml.Serialization
{
	internal class SerializationCodeGenerator
	{
		XmlMapping _typeMap;
		SerializationFormat _format;
		TextWriter _writer;
		int _tempVarId = 0;
		int _indent = 0;
		Hashtable _uniqueNames = new Hashtable();
		int _methodId = 0;
		SerializerInfo _config;
		ArrayList _mapsToGenerate = new ArrayList ();
		ArrayList _fixupCallbacks;
		ArrayList _referencedTypes = new ArrayList ();
		GenerationResult[] _results;
		GenerationResult _result;
		XmlMapping[] _xmlMaps;
		
		CodeIdentifiers classNames = new CodeIdentifiers ();

		public SerializationCodeGenerator (XmlMapping[] xmlMaps): this (xmlMaps, null)
		{
		}
		
		public SerializationCodeGenerator (XmlMapping[] xmlMaps, SerializerInfo config)
		{
			_xmlMaps = xmlMaps;
			_config = config;
		}
		
		public SerializationCodeGenerator (XmlMapping xmlMap, SerializerInfo config)
		{
			_xmlMaps = new XmlMapping [] {xmlMap};
			_config = config;
		}
		
		public static void Generate (string configFileName, string outputPath)
		{
			SerializationCodeGeneratorConfiguration cnf = null;
			StreamReader sr = new StreamReader (configFileName);
			try
			{
				XmlReflectionImporter ri = new XmlReflectionImporter ();
				ri.AllowPrivateTypes = true;
				XmlSerializer ser = new XmlSerializer (ri.ImportTypeMapping (typeof (SerializationCodeGeneratorConfiguration)));
				cnf = (SerializationCodeGeneratorConfiguration) ser.Deserialize (sr);
			}
			finally
			{
				sr.Close ();
			}
			
			if (outputPath == null) outputPath = "";
			
			CodeIdentifiers ids = new CodeIdentifiers ();
			if (cnf.Serializers != null)
			{
				foreach (SerializerInfo info in cnf.Serializers)
				{					
					Type type;
					if (info.Assembly != null)
					{
						Assembly asm;
						try {
							asm = Assembly.Load (info.Assembly);
						} catch {
							asm = Assembly.LoadFrom (info.Assembly);
						}
						type = asm.GetType (info.ClassName, true);
					}
					else
						type = Type.GetType (info.ClassName);
					
					if (type == null) throw new InvalidOperationException ("Type " + info.ClassName + " not found");
					
					string file = info.OutFileName;
					if (file == null || file.Length == 0) {
						int i = info.ClassName.LastIndexOf (".");
						if (i != -1) file = info.ClassName.Substring (i+1);
						else file = info.ClassName;
						file = ids.AddUnique (file, type) + "Serializer.cs";
					}
					StreamWriter writer = new StreamWriter (Path.Combine (outputPath, file));
					
					try
					{
						XmlTypeMapping map;
						
						if (info.SerializationFormat == SerializationFormat.Literal) {
							XmlReflectionImporter ri = new XmlReflectionImporter ();
							map = ri.ImportTypeMapping (type);
						}
						else {
							SoapReflectionImporter ri = new SoapReflectionImporter ();
							map = ri.ImportTypeMapping (type);
						}
						
						SerializationCodeGenerator gen = new SerializationCodeGenerator (map, info);
						gen.GenerateSerializers (writer);
					}
					finally
					{
						writer.Close ();
					}
				}
			}
		}
		
		public void GenerateSerializers (TextWriter writer)
		{
			_writer = writer;
			_results = new GenerationResult [_xmlMaps.Length];
			
			WriteLine ("// It is automatically generated");
			WriteLine ("using System;");
			WriteLine ("using System.Xml;");
			WriteLine ("using System.Xml.Schema;");
			WriteLine ("using System.Xml.Serialization;");
			WriteLine ("using System.Text;");
			WriteLine ("using System.Collections;");
			WriteLine ("using System.Globalization;");
			if (_config != null && _config.NamespaceImports != null && _config.NamespaceImports.Length > 0) {
				foreach (string ns in _config.NamespaceImports)
					WriteLine ("using " + ns + ";");
			}
			WriteLine ("");
			
			string readerClassName = null;
			string writerClassName = null;
			string baseClassName = null;
			string implClassName = null;
			string namspace = null;
			
			if (_config != null)
			{
				readerClassName = _config.ReaderClassName;
				writerClassName = _config.WriterClassName;
				baseClassName = _config.BaseSerializerClassName;
				implClassName = _config.ImplementationClassName;
				namspace = _config.Namespace;
			}

			if (readerClassName == null || readerClassName.Length == 0)
				readerClassName = "GeneratedReader";

			if (writerClassName == null || writerClassName.Length == 0)
				writerClassName = "GeneratedWriter";
			if (baseClassName == null || baseClassName.Length == 0)
				baseClassName = "BaseXmlSerializer";
			if (implClassName == null || implClassName.Length == 0)
				implClassName = "XmlSerializerContract";
			readerClassName = GetUniqueClassName (readerClassName);
			writerClassName = GetUniqueClassName (writerClassName);
			baseClassName = GetUniqueClassName (baseClassName);
			implClassName = GetUniqueClassName (implClassName);
			Hashtable mapsByNamespace = new Hashtable ();
			Hashtable generatedMaps = new Hashtable ();
			
			for (int n=0; n<_xmlMaps.Length; n++)
			{
				_typeMap = _xmlMaps [n];
				if (_typeMap == null) continue;
				
				_result = generatedMaps [_typeMap] as GenerationResult;
				if (_result != null) {
					_results[n] = _result;
					continue;
				}
				
				_result = new GenerationResult ();
				_results[n] = _result;
				
				generatedMaps [_typeMap] = _result;
				
				string typeName;
				if (_typeMap is XmlTypeMapping) typeName = CodeIdentifier.MakeValid (((XmlTypeMapping)_typeMap).TypeData.CSharpName);
				else typeName = ((XmlMembersMapping)_typeMap).ElementName;
				
				_result.ReaderClassName = readerClassName;
				_result.WriterClassName = writerClassName;
				_result.BaseSerializerClassName = baseClassName;
				_result.ImplementationClassName = implClassName;

				if (namspace == null || namspace.Length == 0)
					_result.Namespace = "Mono.GeneratedSerializers." + _typeMap.Format;
				else
					_result.Namespace = namspace;
				
				_result.WriteMethodName = GetUniqueName ("rwo", _typeMap, "WriteRoot_" + typeName);
				_result.ReadMethodName = GetUniqueName ("rro", _typeMap, "ReadRoot_" + typeName);

				_result.Mapping = _typeMap;
				
				ArrayList maps = (ArrayList) mapsByNamespace [_result.Namespace];
				if (maps == null) {
					maps = new ArrayList ();
					mapsByNamespace [_result.Namespace] = maps;
				}
				maps.Add (_result);
			}
			
			foreach (DictionaryEntry entry in mapsByNamespace)
			{
				ArrayList maps = (ArrayList) entry.Value;
				
				WriteLine ("namespace " + entry.Key);
				WriteLineInd ("{");
				
				if (_config == null || !_config.NoReader)
					GenerateReader (readerClassName, maps);
				WriteLine ("");
				if (_config == null || !_config.NoWriter)
					GenerateWriter (writerClassName, maps);
				WriteLine ("");
				
#if NET_2_0
				GenerateContract (maps);
#endif

				WriteLineUni ("}");
				WriteLine ("");
			}
		}
		
		public GenerationResult[] GenerationResults
		{
			get { return _results; }
		}
		
		public ArrayList ReferencedTypes
		{
			get { return _referencedTypes; }
		}
		
		void UpdateGeneratedTypes (ArrayList list)
		{
			for (int n=0; n<list.Count; n++)
			{
				XmlTypeMapping map = list[n] as XmlTypeMapping;
				if (map != null && !_referencedTypes.Contains (map.TypeData.Type))
					_referencedTypes.Add (map.TypeData.Type);
			}
		}

		static string ToCSharpFullName (Type type)
		{
			return TypeData.ToCSharpName (type, true);
		}

		#region Writer Generation
		
		//*******************************************************
		// Contract generation
		//
		
#if NET_2_0
		public void GenerateContract (ArrayList generatedMaps)
		{
			// Write the base serializer
			
			if (generatedMaps.Count == 0) return;
			
			GenerationResult main = (GenerationResult) generatedMaps[0];
			
			string baseSerializerName = main.BaseSerializerClassName;
			string access_mod = (_config == null || !_config.GenerateAsInternal) ? "public" : "internal";
			
			WriteLine ("");
			WriteLine (access_mod + " class " + baseSerializerName + " : System.Xml.Serialization.XmlSerializer");
			WriteLineInd ("{");
			WriteLineInd ("protected override System.Xml.Serialization.XmlSerializationReader CreateReader () {");
			WriteLine ("return new " + main.ReaderClassName + " ();");
			WriteLineUni ("}");
			WriteLine ("");
			
			WriteLineInd ("protected override System.Xml.Serialization.XmlSerializationWriter CreateWriter () {");
			WriteLine ("return new " + main.WriterClassName + " ();");
			WriteLineUni ("}");
			WriteLine ("");
			
			WriteLineInd ("public override bool CanDeserialize (System.Xml.XmlReader xmlReader) {");
			WriteLine ("return true;");
			WriteLineUni ("}");
			
			WriteLineUni ("}");
			WriteLine ("");
			
			// Write a serializer for each imported map
			
			foreach (GenerationResult res in generatedMaps)
			{
				res.SerializerClassName = GetUniqueClassName (res.Mapping.ElementName + "Serializer");
				
				WriteLine (access_mod + " sealed class " + res.SerializerClassName + " : " + baseSerializerName);
				WriteLineInd ("{");
				WriteLineInd ("protected override void Serialize (object obj, System.Xml.Serialization.XmlSerializationWriter writer) {");
				WriteLine ("((" + res.WriterClassName + ")writer)." + res.WriteMethodName + "(obj);");
				WriteLineUni ("}");
				WriteLine ("");
				
				WriteLineInd ("protected override object Deserialize (System.Xml.Serialization.XmlSerializationReader reader) {");
				WriteLine ("return ((" + res.ReaderClassName + ")reader)." + res.ReadMethodName + "();");
				WriteLineUni ("}");
				
				WriteLineUni ("}");
				WriteLine ("");
			}

			WriteLine ("#if !TARGET_JVM"); // does it make sense? ;-)
			WriteLine (access_mod + " class " + main.ImplementationClassName + " : System.Xml.Serialization.XmlSerializerImplementation");
			WriteLineInd ("{");
			
			WriteLine ("System.Collections.Hashtable readMethods = null;");
			WriteLine ("System.Collections.Hashtable writeMethods = null;");
			WriteLine ("System.Collections.Hashtable typedSerializers = null;");
			WriteLine ("");
		
			WriteLineInd ("public override System.Xml.Serialization.XmlSerializationReader Reader {");
			WriteLineInd ("get {");
			WriteLine ("return new " + main.ReaderClassName + "();");
			WriteLineUni ("}");
			WriteLineUni ("}");
			WriteLine ("");
			
			WriteLineInd ("public override System.Xml.Serialization.XmlSerializationWriter Writer {");
			WriteLineInd ("get {");
			WriteLine ("return new " + main.WriterClassName + "();");
			WriteLineUni ("}");
			WriteLineUni ("}");
			WriteLine ("");
			
			WriteLineInd ("public override System.Collections.Hashtable ReadMethods {");
			WriteLineInd ("get {");
			WriteLineInd ("lock (this) {");
			WriteLineInd ("if (readMethods == null) {");
			WriteLine ("readMethods = new System.Collections.Hashtable ();");
			foreach (GenerationResult res in generatedMaps)
				WriteLine ("readMethods.Add (@\"" + res.Mapping.GetKey () + "\", @\"" + res.ReadMethodName + "\");");
			WriteLineUni ("}");
			WriteLine ("return readMethods;");
			WriteLineUni ("}");
			WriteLineUni ("}");
			WriteLineUni ("}");
			WriteLine ("");
			
			WriteLineInd ("public override System.Collections.Hashtable WriteMethods {");
			WriteLineInd ("get {");
			WriteLineInd ("lock (this) {");
			WriteLineInd ("if (writeMethods == null) {");
			WriteLine ("writeMethods = new System.Collections.Hashtable ();");
			foreach (GenerationResult res in generatedMaps)
				WriteLine ("writeMethods.Add (@\"" + res.Mapping.GetKey () + "\", @\"" + res.WriteMethodName + "\");");
			WriteLineUni ("}");
			WriteLine ("return writeMethods;");
			WriteLineUni ("}");
			WriteLineUni ("}");
			WriteLineUni ("}");
			WriteLine ("");
			
			WriteLineInd ("public override System.Collections.Hashtable TypedSerializers {");
			WriteLineInd ("get {");
			WriteLineInd ("lock (this) {");
			WriteLineInd ("if (typedSerializers == null) {");
			WriteLine ("typedSerializers = new System.Collections.Hashtable ();");
			foreach (GenerationResult res in generatedMaps)
				WriteLine ("typedSerializers.Add (@\"" + res.Mapping.GetKey () + "\", new " + res.SerializerClassName + "());");
			WriteLineUni ("}");
			WriteLine ("return typedSerializers;");
			WriteLineUni ("}");
			WriteLineUni ("}");
			WriteLineUni ("}");
			WriteLine ("");

			WriteLine ("public override XmlSerializer GetSerializer (Type type)");
			WriteLineInd ("{");
			WriteLine ("switch (type.FullName) {");
			foreach (GenerationResult res in generatedMaps) {
				if (res.Mapping is XmlTypeMapping) {
					WriteLineInd ("case \"" + ((XmlTypeMapping) res.Mapping).TypeData.CSharpFullName + "\":");
					WriteLine ("return (XmlSerializer) TypedSerializers [\"" + res.Mapping.GetKey () + "\"];");
					WriteLineUni ("");
				}
			}
			WriteLine ("}");
			WriteLine ("return base.GetSerializer (type);");
			WriteLineUni ("}");
			WriteLine ("");
			
			WriteLineInd ("public override bool CanSerialize (System.Type type) {");
			foreach (GenerationResult res in generatedMaps) {
				if (res.Mapping is XmlTypeMapping)
					WriteLine ("if (type == typeof(" + (res.Mapping as XmlTypeMapping).TypeData.CSharpFullName +  ")) return true;");
			}
			WriteLine ("return false;");
			WriteLineUni ("}");
			
			WriteLineUni ("}");
			WriteLine ("");
			WriteLine ("#endif");
		}
#endif


		//*******************************************************
		// Writer generation
		//

		public void GenerateWriter (string writerClassName, ArrayList maps)
		{
			_mapsToGenerate = new ArrayList ();
			
			InitHooks ();
			
			if (_config == null || !_config.GenerateAsInternal)
				WriteLine ("public class " + writerClassName + " : XmlSerializationWriter");
			else
				WriteLine ("internal class " + writerClassName + " : XmlSerializationWriter");
			WriteLineInd ("{");
			WriteLine ("const string xmlNamespace = \"http://www.w3.org/2000/xmlns/\";");
			// ToBinHexString() is not public, so use reflection here.
			WriteLine ("static readonly System.Reflection.MethodInfo toBinHexStringMethod = typeof (XmlConvert).GetMethod (\"ToBinHexString\", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new Type [] {typeof (byte [])}, null);");
			WriteLine ("static string ToBinHexString (byte [] input)");
			WriteLineInd ("{");
			WriteLine ("return input == null ? null : (string) toBinHexStringMethod.Invoke (null, new object [] {input});");
			WriteLineUni ("}");
			
			for (int n=0; n<maps.Count; n++)
			{
				GenerationResult res = (GenerationResult) maps [n];
				_typeMap = res.Mapping;
				_format = _typeMap.Format;
				_result = res;
				
				GenerateWriteRoot ();
			}
			
			for (int n=0; n<_mapsToGenerate.Count; n++)
			{
				XmlTypeMapping map = (XmlTypeMapping) _mapsToGenerate[n];
				GenerateWriteObject (map);
				if (map.TypeData.SchemaType == SchemaTypes.Enum)
					GenerateGetXmlEnumValue (map);
			}
			
			GenerateWriteInitCallbacks ();
			UpdateGeneratedTypes (_mapsToGenerate);
			
			WriteLineUni ("}");
		}
		
		void GenerateWriteRoot ()
		{
			WriteLine ("public void " +_result.WriteMethodName + " (object o)");
			WriteLineInd ("{");
			WriteLine ("WriteStartDocument ();");
			
			if (_typeMap is XmlTypeMapping)
			{
				WriteLine (GetRootTypeName () + " ob = (" + GetRootTypeName () + ") o;");
				XmlTypeMapping mp = (XmlTypeMapping) _typeMap;
				if (mp.TypeData.SchemaType == SchemaTypes.Class || mp.TypeData.SchemaType == SchemaTypes.Array) 
					WriteLine ("TopLevelElement ();");

				if (_format == SerializationFormat.Literal) {
					WriteLine (GetWriteObjectName (mp) + " (ob, " + GetLiteral(mp.ElementName) + ", " + GetLiteral(mp.Namespace) + ", true, false, true);");
				}
				else {
					RegisterReferencingMap (mp);
					WriteLine ("WritePotentiallyReferencingElement (" + GetLiteral(mp.ElementName) + ", " + GetLiteral(mp.Namespace) + ", ob, " + GetTypeOf(mp.TypeData) + ", true, false);");
				}
			}
			else if (_typeMap is XmlMembersMapping) {
				WriteLine ("object[] pars = (object[]) o;");
				GenerateWriteMessage ((XmlMembersMapping) _typeMap);
			}
			else
				throw new InvalidOperationException ("Unknown type map");

			if (_format == SerializationFormat.Encoded)
				WriteLine ("WriteReferencedElements ();");

			WriteLineUni ("}");
			WriteLine ("");
		}
		
		void GenerateWriteMessage (XmlMembersMapping membersMap)
		{
			if (membersMap.HasWrapperElement) {
				WriteLine ("TopLevelElement ();");
				WriteLine ("WriteStartElement (" + GetLiteral (membersMap.ElementName) + ", " + GetLiteral (membersMap.Namespace) + ", (" + GetLiteral(_format == SerializationFormat.Encoded) + "));");

/*				WriteLineInd ("if (Writer.LookupPrefix (XmlSchema.Namespace) == null)");
				WriteLine ("WriteAttribute (\"xmlns\",\"xsd\",XmlSchema.Namespace,XmlSchema.Namespace);");
				Unindent ();
	
				WriteLineInd ("if (Writer.LookupPrefix (XmlSchema.InstanceNamespace) == null)");
				WriteLine ("WriteAttribute (\"xmlns\",\"xsi\",XmlSchema.InstanceNamespace,XmlSchema.InstanceNamespace);");
				Unindent ();
*/
			}
			
			GenerateWriteObjectElement (membersMap, "pars", true);

			if (membersMap.HasWrapperElement)
				WriteLine ("WriteEndElement();");
		}
		
		void GenerateGetXmlEnumValue (XmlTypeMapping map)
		{
			EnumMap emap = (EnumMap) map.ObjectMap;

			string xmlNamesArray = null;
			string valuesArray = null;

			if (emap.IsFlags) {
				// create static string[] holding XML names of enum constants
				xmlNamesArray = GetUniqueName ("gxe", map, "_xmlNames" + map.XmlType);
				Write ("static readonly string[] " + xmlNamesArray + " = { ");
				for (int i = 0; i < emap.XmlNames.Length; i++) {
					if (i > 0)
						_writer.Write (',');
					_writer.Write ('"');
					_writer.Write (emap.XmlNames[i]);
					_writer.Write ('\"');
				}
				_writer.WriteLine (" };");

				// create const long[] holding values of enum constants
				valuesArray = GetUniqueName ("gve", map, "_values" + map.XmlType);
				Write ("static readonly long[] " + valuesArray + " = { ");
				for (int i = 0; i < emap.Values.Length; i++) {
					if (i > 0)
						_writer.Write (',');
					_writer.Write (emap.Values[i].ToString (CultureInfo.InvariantCulture));
					_writer.Write ("L");
				}
				_writer.WriteLine (" };");
				WriteLine (string.Empty);
			}

			WriteLine ("string " + GetGetEnumValueName (map) + " (" + map.TypeData.CSharpFullName + " val)");
			WriteLineInd ("{");


			WriteLineInd ("switch (val) {");
			for (int i = 0; i < emap.EnumNames.Length; i++)
				WriteLine ("case " + map.TypeData.CSharpFullName + ".@" + emap.EnumNames[i] + ": return " + GetLiteral (emap.XmlNames[i]) + ";");

			if (emap.IsFlags) {
				WriteLineInd ("default:");
				// FromEnum actually covers this case too, but we save some cycles here
				WriteLine ("if (val.ToString () == \"0\") return string.Empty;");
				Write ("return FromEnum ((long) val, " + xmlNamesArray + ", " + valuesArray);
#if NET_2_0
				_writer.Write (", typeof (");
				_writer.Write (map.TypeData.CSharpFullName);
				_writer.Write (").FullName");
#endif
				_writer.Write (')'); // close FromEnum method call
				WriteUni (";"); // end statement
			} else {
#if NET_2_0
				WriteLine ("default: throw CreateInvalidEnumValueException ((long) val, typeof (" + map.TypeData.CSharpFullName + ").FullName);");
#else
				WriteLine ("default: return ((long)val).ToString(CultureInfo.InvariantCulture);");
#endif
			}
			
			WriteLineUni ("}");
			
			WriteLineUni ("}");
			WriteLine ("");
		}
		
		void GenerateWriteObject (XmlTypeMapping typeMap)
		{
			WriteLine ("void " + GetWriteObjectName (typeMap) + " (" + typeMap.TypeData.CSharpFullName + " ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)");
			WriteLineInd ("{");
			
			PushHookContext ();
			
			SetHookVar ("$TYPE", typeMap.TypeData.CSharpName);
			SetHookVar ("$FULLTYPE", typeMap.TypeData.CSharpFullName);
			SetHookVar ("$OBJECT", "ob");
			SetHookVar ("$NULLABLE", "isNullable");

			if (GenerateWriteHook (HookType.type, typeMap.TypeData.Type))
			{
				WriteLineUni ("}");
				WriteLine ("");
				PopHookContext ();
				return;
			}
			
			if (!typeMap.TypeData.IsValueType)
			{
				WriteLine ("if (((object)ob) == null)");
				WriteLineInd ("{");
				WriteLineInd ("if (isNullable)");
				
				if (_format == SerializationFormat.Literal) 
					WriteLine ("WriteNullTagLiteral(element, namesp);");
				else 
					WriteLine ("WriteNullTagEncoded (element, namesp);");
				
				WriteLineUni ("return;");
				WriteLineUni ("}");
				WriteLine ("");
			}

			if (typeMap.TypeData.SchemaType == SchemaTypes.XmlNode)
			{
				if (_format == SerializationFormat.Literal)
					WriteLine ("WriteElementLiteral (ob, \"\", \"\", true, false);");
				else 
					WriteLine ("WriteElementEncoded (ob, \"\", \"\", true, false);");
					
				GenerateEndHook ();
				WriteLineUni ("}");
				WriteLine ("");
				PopHookContext ();
				return;
			}

			if (typeMap.TypeData.SchemaType == SchemaTypes.XmlSerializable)
			{
				WriteLine ("WriteSerializable (ob, element, namesp, isNullable);");
				
				GenerateEndHook ();
				WriteLineUni ("}");
				WriteLine ("");
				PopHookContext ();
				return;
			}

			ArrayList types = typeMap.DerivedTypes;
			
			WriteLine ("System.Type type = ob.GetType ();");
			WriteLine ("if (type == typeof(" + typeMap.TypeData.CSharpFullName + "))");
			WriteLine ("{ }");
				
			for (int n=0; n<types.Count; n++)
			{
				XmlTypeMapping map = (XmlTypeMapping)types[n];
				
				WriteLineInd ("else if (type == typeof(" + map.TypeData.CSharpFullName + ")) { ");
				WriteLine (GetWriteObjectName (map) + "((" + map.TypeData.CSharpFullName + ")ob, element, namesp, isNullable, true, writeWrappingElem);");
				WriteLine ("return;");
				WriteLineUni ("}");
			}
			
			if (typeMap.TypeData.Type == typeof (object)) {
				WriteLineInd ("else {");
				WriteLineInd ("if (ob.GetType().IsArray && typeof(XmlNode).IsAssignableFrom(ob.GetType().GetElementType())) {");
				WriteLine ("Writer.WriteStartElement (" + GetLiteral (typeMap.ElementName) + ", " + GetLiteral (typeMap.Namespace) + ");");
				WriteLineInd ("foreach (XmlNode node in (System.Collections.IEnumerable) ob)");
				WriteLineUni ("node.WriteTo (Writer);");
				WriteLineUni ("Writer.WriteEndElement ();");
				WriteLine ("}");
				WriteLineInd ("else");
				WriteLineUni ("WriteTypedPrimitive (element, namesp, ob, true);");
				WriteLine ("return;");
				WriteLineUni ("}");
			}
			else
			{
				WriteLineInd ("else {");
				WriteLine ("throw CreateUnknownTypeException (ob);");
				WriteLineUni ("}");
				WriteLine ("");
				
				WriteLineInd ("if (writeWrappingElem) {");
				if (_format == SerializationFormat.Encoded) WriteLine ("needType = true;");
				WriteLine ("WriteStartElement (element, namesp, ob);");
				WriteLineUni ("}");
				WriteLine ("");
	
				WriteLine ("if (needType) WriteXsiType(" + GetLiteral(typeMap.XmlType) + ", " + GetLiteral(typeMap.XmlTypeNamespace) + ");");
				WriteLine ("");
	
				switch (typeMap.TypeData.SchemaType)
				{
					case SchemaTypes.Class: GenerateWriteObjectElement (typeMap, "ob", false); break;
					case SchemaTypes.Array: GenerateWriteListElement (typeMap, "ob"); break;
					case SchemaTypes.Primitive: GenerateWritePrimitiveElement (typeMap, "ob"); break;
					case SchemaTypes.Enum: GenerateWriteEnumElement (typeMap, "ob"); break;
				}
	
				WriteLine ("if (writeWrappingElem) WriteEndElement (ob);");
			}
			
			GenerateEndHook ();
			WriteLineUni ("}");
			WriteLine ("");
			PopHookContext ();
		}

		void GenerateWriteObjectElement (XmlMapping xmlMap, string ob, bool isValueList)
		{
			XmlTypeMapping typeMap = xmlMap as XmlTypeMapping;
			Type xmlMapType = (typeMap != null) ? typeMap.TypeData.Type : typeof(object[]);
			
			ClassMap map = (ClassMap)xmlMap.ObjectMap;
			if (!GenerateWriteHook (HookType.attributes, xmlMapType))
			{
				if (map.NamespaceDeclarations != null) {
					WriteLine ("WriteNamespaceDeclarations ((XmlSerializerNamespaces) " + ob + ".@" + map.NamespaceDeclarations.Name + ");");
					WriteLine ("");
				}
	
				XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
				if (anyAttrMember != null) 
				{
					if (!GenerateWriteMemberHook (xmlMapType, anyAttrMember))
					{
						string cond = GenerateMemberHasValueCondition (anyAttrMember, ob, isValueList);
						if (cond != null) WriteLineInd ("if (" + cond + ") {");
		
						string tmpVar = GetObTempVar ();
						WriteLine ("ICollection " + tmpVar + " = " + GenerateGetMemberValue (anyAttrMember, ob, isValueList) + ";");
						WriteLineInd ("if (" + tmpVar + " != null) {");
		
						string tmpVar2 = GetObTempVar ();
						WriteLineInd ("foreach (XmlAttribute " + tmpVar2 + " in " + tmpVar + ")");
						WriteLineInd ("if (" + tmpVar2 + ".NamespaceURI != xmlNamespace)");
						WriteLine ("WriteXmlAttribute (" + tmpVar2 + ", " + ob + ");");
						Unindent ();
						Unindent ();
						WriteLineUni ("}");
		
						if (cond != null) WriteLineUni ("}");
						WriteLine ("");
						GenerateEndHook ();
					}
				}
				
				ICollection attributes = map.AttributeMembers;
				if (attributes != null)
				{
					foreach (XmlTypeMapMemberAttribute attr in attributes) 
					{
						if (GenerateWriteMemberHook (xmlMapType, attr)) continue;
					
						string val = GenerateGetMemberValue (attr, ob, isValueList);
						string cond = GenerateMemberHasValueCondition (attr, ob, isValueList);
						
						if (cond != null) WriteLineInd ("if (" + cond + ") {");
						
						string strVal = GenerateGetStringValue (attr.MappedType, attr.TypeData, val, false);
						WriteLine ("WriteAttribute (" + GetLiteral(attr.AttributeName) + ", " + GetLiteral(attr.Namespace) + ", " + strVal + ");");
	
						if (cond != null) WriteLineUni ("}");
						GenerateEndHook ();
					}
					WriteLine ("");
				}
				GenerateEndHook ();
			}
			
			if (!GenerateWriteHook (HookType.elements, xmlMapType))
			{
				ICollection members = map.ElementMembers;
				if (members != null)
				{
					foreach (XmlTypeMapMemberElement member in members)
					{
						if (GenerateWriteMemberHook (xmlMapType, member)) continue;
						
						string cond = GenerateMemberHasValueCondition (member, ob, isValueList);
						if (cond != null) WriteLineInd ("if (" + cond + ") {");
						
						string memberValue = GenerateGetMemberValue (member, ob, isValueList);
						Type memType = member.GetType();
	
						if (memType == typeof(XmlTypeMapMemberList))
						{
							GenerateWriteMemberElement ((XmlTypeMapElementInfo) member.ElementInfo[0], memberValue);
						}
						else if (memType == typeof(XmlTypeMapMemberFlatList))
						{
							WriteLineInd ("if (" + memberValue + " != null) {"); 
							GenerateWriteListContent (ob, member.TypeData, ((XmlTypeMapMemberFlatList)member).ListMap, memberValue, false);
							WriteLineUni ("}");
						}
						else if (memType == typeof(XmlTypeMapMemberAnyElement))
						{
							WriteLineInd ("if (" + memberValue + " != null) {"); 
							GenerateWriteAnyElementContent ((XmlTypeMapMemberAnyElement)member, memberValue);
							WriteLineUni ("}");
						}
						else if (memType == typeof(XmlTypeMapMemberAnyElement))
						{
							WriteLineInd ("if (" + memberValue + " != null) {"); 
							GenerateWriteAnyElementContent ((XmlTypeMapMemberAnyElement)member, memberValue);
							WriteLineUni ("}");
						}
						else if (memType == typeof(XmlTypeMapMemberAnyAttribute))
						{
							// Ignore
						}
						else if (memType == typeof(XmlTypeMapMemberElement))
						{
							if (member.ElementInfo.Count == 1) {
								GenerateWriteMemberElement ((XmlTypeMapElementInfo)member.ElementInfo[0], memberValue);
							}
							else if (member.ChoiceMember != null)
							{
								string choiceValue = ob + ".@" + member.ChoiceMember;
								foreach (XmlTypeMapElementInfo elem in member.ElementInfo) {
									WriteLineInd ("if (" + choiceValue + " == " + GetLiteral(elem.ChoiceValue) + ") {");
									GenerateWriteMemberElement (elem, GetCast(elem.TypeData, member.TypeData, memberValue));
									WriteLineUni ("}");
								}
							}
							else
							{
	//							WriteLineInd ("if (" + memberValue + " == null) {");
	//							GenerateWriteMemberElement ((XmlTypeMapElementInfo)member.ElementInfo[0], memberValue);
	//							WriteLineUni ("}");
									
								bool first = true;
								foreach (XmlTypeMapElementInfo elem in member.ElementInfo)
								{
									WriteLineInd ((first?"":"else ") + "if (" + memberValue + " is " + elem.TypeData.CSharpFullName + ") {");
									GenerateWriteMemberElement (elem, GetCast(elem.TypeData, member.TypeData, memberValue));
									WriteLineUni ("}");
									first = false;
								}
							}
						}
						else
							throw new InvalidOperationException ("Unknown member type");
							
						if (cond != null)
							WriteLineUni ("}");
							
						GenerateEndHook ();
					}
				}
				GenerateEndHook ();
			}
		}
		
		void GenerateWriteMemberElement (XmlTypeMapElementInfo elem, string memberValue)
		{
			switch (elem.TypeData.SchemaType)
			{
				case SchemaTypes.XmlNode:
					string elemName = elem.WrappedElement ? elem.ElementName : "";
					if (_format == SerializationFormat.Literal) 
						WriteMetCall ("WriteElementLiteral", memberValue, GetLiteral(elemName), GetLiteral(elem.Namespace), GetLiteral(elem.IsNullable), "false");
					else 
						WriteMetCall ("WriteElementEncoded", memberValue, GetLiteral(elemName), GetLiteral(elem.Namespace), GetLiteral(elem.IsNullable), "false");
					break;

				case SchemaTypes.Enum:
				case SchemaTypes.Primitive:
					if (_format == SerializationFormat.Literal) 
						GenerateWritePrimitiveValueLiteral (memberValue, elem.ElementName, elem.Namespace, elem.MappedType, elem.TypeData, elem.WrappedElement, elem.IsNullable);
					else
						GenerateWritePrimitiveValueEncoded (memberValue, elem.ElementName, elem.Namespace, new XmlQualifiedName (elem.TypeData.XmlType, elem.DataTypeNamespace), elem.MappedType, elem.TypeData, elem.WrappedElement, elem.IsNullable);
					break;

				case SchemaTypes.Array:
					WriteLineInd ("if (" + memberValue + " != null) {");
					
					if (elem.MappedType.MultiReferenceType) {
						WriteMetCall ("WriteReferencingElement", GetLiteral(elem.ElementName), GetLiteral(elem.Namespace), memberValue, GetLiteral(elem.IsNullable));
						RegisterReferencingMap (elem.MappedType);
					}
					else {
						WriteMetCall ("WriteStartElement", GetLiteral(elem.ElementName), GetLiteral(elem.Namespace), memberValue);
						GenerateWriteListContent (null, elem.TypeData, (ListMap) elem.MappedType.ObjectMap, memberValue, false);
						WriteMetCall ("WriteEndElement", memberValue);
					}
					WriteLineUni ("}");
					
					if (elem.IsNullable) {
						WriteLineInd ("else");
						if (_format == SerializationFormat.Literal) 
							WriteMetCall ("WriteNullTagLiteral", GetLiteral(elem.ElementName), GetLiteral(elem.Namespace));
						else
							WriteMetCall ("WriteNullTagEncoded", GetLiteral(elem.ElementName), GetLiteral(elem.Namespace));
						Unindent ();
					}
					
					break;

				case SchemaTypes.Class:
					if (elem.MappedType.MultiReferenceType)	{
						RegisterReferencingMap (elem.MappedType);
						if (elem.MappedType.TypeData.Type == typeof(object))
							WriteMetCall ("WritePotentiallyReferencingElement", GetLiteral(elem.ElementName), GetLiteral(elem.Namespace), memberValue, "null", "false", GetLiteral(elem.IsNullable));
						else
							WriteMetCall ("WriteReferencingElement", GetLiteral(elem.ElementName), GetLiteral(elem.Namespace), memberValue, GetLiteral(elem.IsNullable));
					}
					else 
						WriteMetCall (GetWriteObjectName(elem.MappedType), memberValue, GetLiteral(elem.ElementName), GetLiteral(elem.Namespace), GetLiteral(elem.IsNullable), "false", "true");
					break;

				case SchemaTypes.XmlSerializable:
					WriteMetCall ("WriteSerializable",  "(" + ToCSharpFullName (elem.MappedType.TypeData.Type) + ") " + memberValue, GetLiteral(elem.ElementName), GetLiteral(elem.Namespace), GetLiteral(elem.IsNullable));
					break;

				default:
					throw new NotSupportedException ("Invalid value type");
			}
		}		

		void GenerateWriteListElement (XmlTypeMapping typeMap, string ob)
		{
			if (_format == SerializationFormat.Encoded)
			{
				string n, ns;
				string itemCount = GenerateGetListCount (typeMap.TypeData, ob);
				GenerateGetArrayType ((ListMap) typeMap.ObjectMap, itemCount, out n, out ns);
				
				string arrayType;
				if (ns != string.Empty)
					arrayType = "FromXmlQualifiedName (new XmlQualifiedName(" + n + "," + ns + "))";
				else
					arrayType = GetLiteral (n);
				
				WriteMetCall ("WriteAttribute", GetLiteral("arrayType"), GetLiteral(XmlSerializer.EncodingNamespace), arrayType);
			}
			GenerateWriteListContent (null, typeMap.TypeData, (ListMap) typeMap.ObjectMap, ob, false);
		}
		
		void GenerateWriteAnyElementContent (XmlTypeMapMemberAnyElement member, string memberValue)
		{
			bool singleElement = (member.TypeData.Type == typeof (XmlElement));
			string var, var2;
			
			var2 = GetObTempVar ();
			if (singleElement)
				var = memberValue;
			else {
				var = GetObTempVar ();
				WriteLineInd ("foreach (object " + var2 + " in " + memberValue + ") {");
			}
			WriteLine ("XmlNode " + var + " = " + var2 + " as XmlNode;");
			WriteLine ("if (" + var + " == null && " + var2 + "!= null) throw new InvalidOperationException (\"A member with XmlAnyElementAttribute can only serialize and deserialize certain XmlNode types.");
			WriteLineUni ("}");

			string elem = GetObTempVar ();
			WriteLine ("XmlNode " + elem + " = " + var + ";");
			WriteLine ("if (" + elem + " is XmlElement) {");
			
			if (!member.IsDefaultAny) {
				for (int n=0; n<member.ElementInfo.Count; n++) {
					XmlTypeMapElementInfo info = (XmlTypeMapElementInfo)member.ElementInfo[n];
					string txt = "(" + elem + ".LocalName == " + GetLiteral(info.ElementName) + " && " + elem + ".NamespaceURI == " + GetLiteral(info.Namespace) + ")";
					if (n == member.ElementInfo.Count-1) txt += ") {";
					if (n == 0) WriteLineInd ("if (" + txt);
					else WriteLine ("|| " + txt);
				}				
			}
			WriteLine ("}");
			WriteLine ("else " + elem + ".WriteTo (Writer);");

			if (_format == SerializationFormat.Literal) 
				WriteLine ("WriteElementLiteral (" + elem + ", \"\", \"\", false, true);");
			else 
				WriteLine ("WriteElementEncoded (" + elem + ", \"\", \"\", false, true);");

			if (!member.IsDefaultAny) {
				WriteLineUni ("}");
				WriteLineInd ("else");
				WriteLine ("throw CreateUnknownAnyElementException (" + elem + ".Name, " + elem + ".NamespaceURI);");
				Unindent ();
			}
			
			if (!singleElement)
				WriteLineUni ("}");
		}

		void GenerateWritePrimitiveElement (XmlTypeMapping typeMap, string ob)
		{
			string strVal = GenerateGetStringValue (typeMap, typeMap.TypeData, ob, false);
			WriteLine ("Writer.WriteString (" + strVal + ");");
		}

		void GenerateWriteEnumElement (XmlTypeMapping typeMap, string ob)
		{
			string strVal = GenerateGetEnumXmlValue (typeMap, ob);
			WriteLine ("Writer.WriteString (" + strVal + ");");
		}

		string GenerateGetStringValue (XmlTypeMapping typeMap, TypeData type, string value, bool isNullable)
		{
			if (type.SchemaType == SchemaTypes.Array) {
				string str = GetStrTempVar ();
				WriteLine ("string " + str + " = null;");
				WriteLineInd ("if (" + value + " != null) {");
				string res = GenerateWriteListContent (null, typeMap.TypeData, (ListMap)typeMap.ObjectMap, value, true);
				WriteLine (str + " = " + res + ".ToString ().Trim ();");
				WriteLineUni ("}");
				return str;
			}
			else if (type.SchemaType == SchemaTypes.Enum) {
				if (isNullable)
					return "(" + value + ").HasValue ? " + GenerateGetEnumXmlValue (typeMap, "(" + value + ").Value") + " : null";
				else
					return GenerateGetEnumXmlValue (typeMap, value);
			}
			else if (type.Type == typeof (XmlQualifiedName))
				return "FromXmlQualifiedName (" + value + ")";
			else if (value == null)
				return null;
			else
				return XmlCustomFormatter.GenerateToXmlString (type, value);
		}

		string GenerateGetEnumXmlValue (XmlTypeMapping typeMap, string ob)
		{
			return GetGetEnumValueName (typeMap) + " (" + ob + ")";
		}

		string GenerateGetListCount (TypeData listType, string ob)
		{
			if (listType.Type.IsArray)
				return "ob.Length";
			else
				return "ob.Count";
		}

		void GenerateGetArrayType (ListMap map, string itemCount, out string localName, out string ns)
		{
			string arrayDim;
			if (itemCount != "") arrayDim = "";
			else arrayDim = "[]";

			XmlTypeMapElementInfo info = (XmlTypeMapElementInfo) map.ItemInfo[0];
			if (info.TypeData.SchemaType == SchemaTypes.Array)
			{
				string nm;
				GenerateGetArrayType ((ListMap)info.MappedType.ObjectMap, "", out nm, out ns);
				localName = nm + arrayDim;
			}
			else 
			{
				if (info.MappedType != null)
				{
					localName = info.MappedType.XmlType + arrayDim;
					ns = info.MappedType.Namespace;
				}
				else 
				{
					localName = info.TypeData.XmlType + arrayDim;
					ns = info.DataTypeNamespace;
				}
			}
			if (itemCount != "") {
				localName = "\"" + localName + "[\" + " + itemCount + " + \"]\"";
				ns = GetLiteral (ns);
			}
		}

		string GenerateWriteListContent (string container, TypeData listType, ListMap map, string ob, bool writeToString)
		{
			string targetString = null;
			
			if (writeToString)
			{
				targetString = GetStrTempVar ();
				WriteLine ("System.Text.StringBuilder " + targetString + " = new System.Text.StringBuilder();");
			}
			
			if (listType.Type.IsArray)
			{
				string itemVar = GetNumTempVar ();
				WriteLineInd ("for (int "+itemVar+" = 0; "+itemVar+" < " + ob + ".Length; "+itemVar+"++) {");
				GenerateListLoop (container, map, ob + "["+itemVar+"]", itemVar, listType.ListItemTypeData, targetString);
				WriteLineUni ("}");
			}
			else if (typeof(ICollection).IsAssignableFrom (listType.Type))
			{
				string itemVar = GetNumTempVar ();
				WriteLineInd ("for (int "+itemVar+" = 0; "+itemVar+" < " + ob + ".Count; "+itemVar+"++) {");
				GenerateListLoop (container, map, ob + "["+itemVar+"]", itemVar, listType.ListItemTypeData, targetString);
				WriteLineUni ("}");
			}
			else if (typeof(IEnumerable).IsAssignableFrom (listType.Type))
			{
				string itemVar = GetObTempVar ();
				WriteLineInd ("foreach (" + listType.ListItemTypeData.CSharpFullName + " " + itemVar + " in " + ob + ") {");
				GenerateListLoop (container, map, itemVar, null, listType.ListItemTypeData, targetString);
				WriteLineUni ("}");
			}
			else
				throw new Exception ("Unsupported collection type");

			return targetString;
		}
		
		void GenerateListLoop (string container, ListMap map, string item, string index, TypeData itemTypeData, string targetString)
		{
			bool multichoice = (map.ItemInfo.Count > 1);

			if (map.ChoiceMember != null && container != null && index != null) {
				WriteLineInd ("if ((" + container + ".@" + map.ChoiceMember + " == null) || (" + index + " >= " + container + ".@" + map.ChoiceMember + ".Length))");
				WriteLine ("throw CreateInvalidChoiceIdentifierValueException (" + container + ".GetType().ToString(), \"" + map.ChoiceMember + "\");");
				Unindent ();
			}
			
			if (multichoice)
				WriteLine ("if (((object)" + item + ") == null) { }");
				
			foreach (XmlTypeMapElementInfo info in map.ItemInfo)
			{
				if (map.ChoiceMember != null && multichoice)
					WriteLineInd ("else if (" + container + ".@" + map.ChoiceMember + "[" + index + "] == " + GetLiteral (info.ChoiceValue) + ") {");
				else if (multichoice)
					WriteLineInd ("else if (" + item + ".GetType() == typeof(" + info.TypeData.CSharpFullName + ")) {");
				
				if (targetString == null) 
					GenerateWriteMemberElement (info, GetCast (info.TypeData, itemTypeData, item));
				else
				{
					string strVal = GenerateGetStringValue (info.MappedType, info.TypeData, GetCast (info.TypeData, itemTypeData, item), false);
					WriteLine (targetString + ".Append (" + strVal + ").Append (\" \");");
				}

				if (multichoice)
					WriteLineUni ("}");
			}
			
			if (multichoice)
				WriteLine ("else throw CreateUnknownTypeException (" + item + ");");
		}

		void GenerateWritePrimitiveValueLiteral (string memberValue, string name, string ns, XmlTypeMapping mappedType, TypeData typeData, bool wrapped, bool isNullable)
		{
			if (!wrapped) {
				string strVal = GenerateGetStringValue (mappedType, typeData, memberValue, false);
				WriteMetCall ("WriteValue", strVal);
			}
			else if (isNullable) {
				if (typeData.Type == typeof(XmlQualifiedName)) 
					WriteMetCall ("WriteNullableQualifiedNameLiteral", GetLiteral(name), GetLiteral(ns), memberValue);
				else  {
					string strVal = GenerateGetStringValue (mappedType, typeData, memberValue, true);
					WriteMetCall ("WriteNullableStringLiteral", GetLiteral(name), GetLiteral(ns), strVal);
				}
			}
			else {
				if (typeData.Type == typeof(XmlQualifiedName))
					WriteMetCall ("WriteElementQualifiedName", GetLiteral(name), GetLiteral(ns), memberValue);
				else {
					string strVal = GenerateGetStringValue (mappedType, typeData, memberValue, false);
					WriteMetCall ("WriteElementString", GetLiteral(name),GetLiteral(ns), strVal);
				}
			}
		}
		
		void GenerateWritePrimitiveValueEncoded (string memberValue, string name, string ns, XmlQualifiedName xsiType, XmlTypeMapping mappedType, TypeData typeData, bool wrapped, bool isNullable)
		{
			if (!wrapped) {
				string strVal = GenerateGetStringValue (mappedType, typeData, memberValue, false);
				WriteMetCall ("WriteValue", strVal);
			}
			else if (isNullable) {
				if (typeData.Type == typeof(XmlQualifiedName)) 
					WriteMetCall ("WriteNullableQualifiedNameEncoded", GetLiteral(name), GetLiteral(ns), memberValue, GetLiteral(xsiType));
				else  {
					string strVal = GenerateGetStringValue (mappedType, typeData, memberValue, true);
					WriteMetCall ("WriteNullableStringEncoded", GetLiteral(name), GetLiteral(ns), strVal, GetLiteral(xsiType));
				}
			}
			else {
				if (typeData.Type == typeof(XmlQualifiedName))
					WriteMetCall ("WriteElementQualifiedName", GetLiteral(name), GetLiteral(ns), memberValue, GetLiteral(xsiType));
				else {
					string strVal = GenerateGetStringValue (mappedType, typeData, memberValue, false);
					WriteMetCall ("WriteElementString", GetLiteral(name),GetLiteral(ns), strVal, GetLiteral(xsiType));
				}
			}
		}

		string GenerateGetMemberValue (XmlTypeMapMember member, string ob, bool isValueList)
		{
			if (isValueList) return GetCast (member.TypeData, TypeTranslator.GetTypeData (typeof(object)), ob + "[" + member.GlobalIndex + "]");
			else return ob + ".@" + member.Name;
		}
		
		string GenerateMemberHasValueCondition (XmlTypeMapMember member, string ob, bool isValueList)
		{
			if (isValueList) {
				return ob + ".Length > " + member.GlobalIndex;
			}
			else if (member.DefaultValue != System.DBNull.Value) {
				string mem = ob + ".@" + member.Name;
				if (member.DefaultValue == null) 
					return mem + " != null";
				else if (member.TypeData.SchemaType == SchemaTypes.Enum)
					return mem + " != " + GetCast (member.TypeData, GetLiteral (member.DefaultValue));
				else 
					return mem + " != " + GetLiteral (member.DefaultValue);
			}
			else if (member.IsOptionalValueType)
				return ob + ".@" + member.Name + "Specified";
			return null;
		}

		void GenerateWriteInitCallbacks ()
		{
			WriteLine ("protected override void InitCallbacks ()");
			WriteLineInd ("{");
			
			if (_format == SerializationFormat.Encoded)
			{
				foreach (XmlMapping xmap in _mapsToGenerate)  {
					XmlTypeMapping map = xmap as XmlTypeMapping;
					if (map != null)
						WriteMetCall ("AddWriteCallback", GetTypeOf(map.TypeData), GetLiteral(map.XmlType), GetLiteral(map.Namespace), "new XmlSerializationWriteCallback (" + GetWriteObjectCallbackName (map) + ")");
				}
			}	
			
			WriteLineUni ("}");
			WriteLine ("");
				
			if (_format == SerializationFormat.Encoded)
			{
				foreach (XmlTypeMapping xmap in _mapsToGenerate)  {
					XmlTypeMapping map = xmap as XmlTypeMapping;
					if (map == null) continue;
					if (map.TypeData.SchemaType == SchemaTypes.Enum)
						WriteWriteEnumCallback (map);
					else
						WriteWriteObjectCallback (map);
				}
			}
		}
		
		void WriteWriteEnumCallback (XmlTypeMapping map)
		{
			WriteLine ("void " + GetWriteObjectCallbackName (map) + " (object ob)");
			WriteLineInd ("{");
			WriteMetCall (GetWriteObjectName(map), GetCast (map.TypeData, "ob"), GetLiteral(map.ElementName), GetLiteral(map.Namespace), "false", "true", "false");
			WriteLineUni ("}");
			WriteLine ("");
		}
		
		void WriteWriteObjectCallback (XmlTypeMapping map)
		{
			WriteLine ("void " + GetWriteObjectCallbackName (map) + " (object ob)");
			WriteLineInd ("{");
			WriteMetCall (GetWriteObjectName(map), GetCast (map.TypeData, "ob"), GetLiteral(map.ElementName), GetLiteral(map.Namespace), "false", "false", "false");
			WriteLineUni ("}");
			WriteLine ("");
		}
		
		#endregion
		
		#region Reader Generation

		//*******************************************************
		// Reader generation
		//
		
		public void GenerateReader (string readerClassName, ArrayList maps)
		{
			if (_config == null || !_config.GenerateAsInternal)
				WriteLine ("public class " + readerClassName + " : XmlSerializationReader");
			else
				WriteLine ("internal class " + readerClassName + " : XmlSerializationReader");
			WriteLineInd ("{");
			// FromBinHexString() is not public, so use reflection here.
			WriteLine ("static readonly System.Reflection.MethodInfo fromBinHexStringMethod = typeof (XmlConvert).GetMethod (\"FromBinHexString\", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new Type [] {typeof (string)}, null);");
			WriteLine ("static byte [] FromBinHexString (string input)");
			WriteLineInd ("{");
			WriteLine ("return input == null ? null : (byte []) fromBinHexStringMethod.Invoke (null, new object [] {input});");
			WriteLineUni ("}");

			_mapsToGenerate = new ArrayList ();
			_fixupCallbacks = new ArrayList ();
			InitHooks ();
			
			for (int n=0; n<maps.Count; n++)
			{
				GenerationResult res = (GenerationResult) maps [n];
				_typeMap = res.Mapping;
				_format = _typeMap.Format;
				_result = res;
				
				GenerateReadRoot ();
			}
			
			for (int n=0; n<_mapsToGenerate.Count; n++)
			{
				XmlTypeMapping map = _mapsToGenerate [n] as XmlTypeMapping;
				if (map == null) continue;
				
				GenerateReadObject (map);
				if (map.TypeData.SchemaType == SchemaTypes.Enum)
					GenerateGetEnumValueMethod (map);
			}
			
			GenerateReadInitCallbacks ();
			
			if (_format == SerializationFormat.Encoded)
			{
				GenerateFixupCallbacks ();
				GenerateFillerCallbacks ();
			}
			
			WriteLineUni ("}");
			UpdateGeneratedTypes (_mapsToGenerate);
		}
		
		void GenerateReadRoot ()
		{
			WriteLine ("public object " + _result.ReadMethodName + " ()");
			WriteLineInd ("{");
			WriteLine ("Reader.MoveToContent();");
			
			if (_typeMap is XmlTypeMapping)
			{
				XmlTypeMapping typeMap = (XmlTypeMapping) _typeMap;

				if (_format == SerializationFormat.Literal)
				{
					if (typeMap.TypeData.SchemaType == SchemaTypes.XmlNode) {
						if (typeMap.TypeData.Type == typeof (XmlDocument))
							WriteLine ("return ReadXmlDocument (false);");
						else
							WriteLine ("return ReadXmlNode (false);");
					} else {
						WriteLineInd ("if (Reader.LocalName != " + GetLiteral (typeMap.ElementName) + " || Reader.NamespaceURI != " + GetLiteral (typeMap.Namespace) + ")");
						WriteLine ("throw CreateUnknownNodeException();");
						Unindent ();

						WriteLine ("return " + GetReadObjectCall (typeMap, GetLiteral(typeMap.IsNullable), "true") + ";");
					}
				}
				else
				{
					WriteLine ("object ob = null;");
					WriteLine ("Reader.MoveToContent();");
					WriteLine ("if (Reader.NodeType == System.Xml.XmlNodeType.Element) ");
					WriteLineInd ("{");
					WriteLineInd ("if (Reader.LocalName == " + GetLiteral(typeMap.ElementName) + " && Reader.NamespaceURI == " + GetLiteral (typeMap.Namespace) + ")");
					WriteLine ("ob = ReadReferencedElement();");
					Unindent ();
					WriteLineInd ("else ");
					WriteLine ("throw CreateUnknownNodeException();");
					Unindent ();
					WriteLineUni ("}");
					WriteLineInd ("else ");
					WriteLine ("UnknownNode(null);");
					Unindent ();
					WriteLine ("");
					WriteLine ("ReadReferencedElements();");
					WriteLine ("return ob;");
					RegisterReferencingMap (typeMap);
				}
			}
			else {
				WriteLine ("return " + GenerateReadMessage ((XmlMembersMapping)_typeMap) + ";");
			}

			WriteLineUni ("}");
			WriteLine ("");
		}
		
		string GenerateReadMessage (XmlMembersMapping typeMap)
		{
			WriteLine ("object[] parameters = new object[" + typeMap.Count + "];");
			WriteLine ("");

			if (typeMap.HasWrapperElement)
			{
				if (_format == SerializationFormat.Encoded)
				{
					WriteLine ("while (Reader.NodeType == System.Xml.XmlNodeType.Element)");
					WriteLineInd ("{");
					WriteLine ("string root = Reader.GetAttribute (\"root\", " + GetLiteral(XmlSerializer.EncodingNamespace) + ");");
					WriteLine ("if (root == null || System.Xml.XmlConvert.ToBoolean(root)) break;");
					WriteLine ("ReadReferencedElement ();");
					WriteLine ("Reader.MoveToContent ();");
					WriteLineUni ("}");
					WriteLine ("");
					WriteLine ("if (Reader.NodeType != System.Xml.XmlNodeType.EndElement)");
					WriteLineInd ("{");
					WriteLineInd ("if (Reader.IsEmptyElement) {");
					WriteLine ("Reader.Skip();");
					WriteLine ("Reader.MoveToContent();");
					WriteLineUni ("}");
					WriteLineInd ("else {");
					WriteLine ("Reader.ReadStartElement();");
					GenerateReadMembers (typeMap, (ClassMap)typeMap.ObjectMap, "parameters", true, false);
					WriteLine ("ReadEndElement();");
					WriteLineUni ("}");
					WriteLine ("");
					WriteLine ("Reader.MoveToContent();");
					WriteLineUni ("}");
				}
				else
				{
					// bug #79988: out parameters need to be initialized if 
					// they are value types
					ClassMap classMap = (ClassMap) typeMap.ObjectMap;
					ArrayList members = classMap.AllMembers;
					for (int n = 0; n < members.Count; n++) {
						XmlTypeMapMember mem = (XmlTypeMapMember) members [n];
						if (!mem.IsReturnValue && mem.TypeData.IsValueType)
							GenerateSetMemberValueFromAttr (mem, "parameters",
								String.Format ("({0}) Activator.CreateInstance(typeof({0}), true)", mem.TypeData.FullTypeName), true);
					}

					WriteLine ("while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.ReadState == ReadState.Interactive)");
					WriteLineInd ("{");
					WriteLine ("if (Reader.IsStartElement(" + GetLiteral(typeMap.ElementName) + ", " + GetLiteral(typeMap.Namespace) + "))");
					WriteLineInd ("{");
					bool dummy = false;
					GenerateReadAttributeMembers (typeMap, (ClassMap)typeMap.ObjectMap, "parameters", true, ref dummy);
					WriteLine ("if (Reader.IsEmptyElement)");
					WriteLineInd ("{");
					WriteLine ("Reader.Skip(); Reader.MoveToContent(); continue;");
					WriteLineUni ("}");
					WriteLine ("Reader.ReadStartElement();");
					GenerateReadMembers (typeMap, (ClassMap)typeMap.ObjectMap, "parameters", true, false);
					WriteLine ("ReadEndElement();");
					WriteLine ("break;");
					WriteLineUni ("}");
					WriteLineInd ("else ");
					WriteLine ("UnknownNode(null);");
					Unindent ();
					WriteLine ("");
					WriteLine ("Reader.MoveToContent();");
					WriteLineUni ("}");
				}
			}
			else
				GenerateReadMembers (typeMap, (ClassMap)typeMap.ObjectMap, "parameters", true, _format == SerializationFormat.Encoded);

			if (_format == SerializationFormat.Encoded)
				WriteLine ("ReadReferencedElements();");

			return "parameters";
		}
		
		void GenerateReadObject (XmlTypeMapping typeMap)
		{
			string isNullable;
			if (_format == SerializationFormat.Literal) {
				WriteLine ("public " + typeMap.TypeData.CSharpFullName + " " + GetReadObjectName (typeMap) + " (bool isNullable, bool checkType)");
				isNullable = "isNullable";
			}
			else {
				WriteLine ("public object " + GetReadObjectName (typeMap) + " ()");
				isNullable = "true";
			}
			
			WriteLineInd ("{");

			PushHookContext ();
			
			SetHookVar ("$TYPE", typeMap.TypeData.CSharpName);
			SetHookVar ("$FULLTYPE", typeMap.TypeData.CSharpFullName);
			SetHookVar ("$NULLABLE", "isNullable");
			
			switch (typeMap.TypeData.SchemaType)
			{
				case SchemaTypes.Class: GenerateReadClassInstance (typeMap, isNullable, "checkType"); break;
				case SchemaTypes.Array: 
					string list = GenerateReadListElement (typeMap, null, isNullable, true);
					if (list != null)
						WriteLine ("return " + list + ";"); 
					break;
				case SchemaTypes.XmlNode: GenerateReadXmlNodeElement (typeMap, isNullable); break;
				case SchemaTypes.Primitive: GenerateReadPrimitiveElement (typeMap, isNullable); break;
				case SchemaTypes.Enum: GenerateReadEnumElement (typeMap, isNullable); break;
				case SchemaTypes.XmlSerializable: GenerateReadXmlSerializableElement (typeMap, isNullable); break;
				default: throw new Exception ("Unsupported map type");
			}
			
			WriteLineUni ("}");
			WriteLine ("");
			PopHookContext ();
		}
				
		void GenerateReadClassInstance (XmlTypeMapping typeMap, string isNullable, string checkType)
		{
			SetHookVar ("$OBJECT", "ob");
			if (!typeMap.TypeData.IsValueType)
			{
				WriteLine (typeMap.TypeData.CSharpFullName + " ob = null;");
			
				if (GenerateReadHook (HookType.type, typeMap.TypeData.Type)) {
					WriteLine ("return ob;");
					return;
				}
				
				if (_format == SerializationFormat.Literal) {
					WriteLine ("if (" + isNullable + " && ReadNull()) return null;");
					WriteLine ("");
					WriteLine ("if (checkType) ");
					WriteLineInd ("{");
				}
				else {
					WriteLine ("if (ReadNull()) return null;");
					WriteLine ("");
				}
			}
			else
			{
				WriteLine (typeMap.TypeData.CSharpFullName + String.Format (" ob = ({0}) Activator.CreateInstance(typeof({0}), true);", typeMap.TypeData.CSharpFullName));
			
				if (GenerateReadHook (HookType.type, typeMap.TypeData.Type)) {
					WriteLine ("return ob;");
					return;
				}
			}
			
			WriteLine ("System.Xml.XmlQualifiedName t = GetXsiType();");
			WriteLine ("if (t == null)");
			if (typeMap.TypeData.Type != typeof(object))
				WriteLine ("{ }");
			else
				WriteLine ("\treturn " + GetCast (typeMap.TypeData, "ReadTypedPrimitive (new System.Xml.XmlQualifiedName(\"anyType\", System.Xml.Schema.XmlSchema.Namespace))") + ";");
			
			foreach (XmlTypeMapping realMap in typeMap.DerivedTypes)
			{
				WriteLineInd ("else if (t.Name == " + GetLiteral (realMap.XmlType) + " && t.Namespace == " + GetLiteral (realMap.XmlTypeNamespace) + ")");
				WriteLine ("return " + GetReadObjectCall(realMap, isNullable, checkType) + ";");
				Unindent ();
			}

			WriteLine ("else if (t.Name != " + GetLiteral (typeMap.XmlType) + " || t.Namespace != " + GetLiteral (typeMap.XmlTypeNamespace) + ")");
			if (typeMap.TypeData.Type == typeof(object))
				WriteLine ("\treturn " + GetCast (typeMap.TypeData, "ReadTypedPrimitive (t)") + ";");
			else
				WriteLine ("\tthrow CreateUnknownTypeException(t);");

			if (!typeMap.TypeData.IsValueType)
			{
				if (_format == SerializationFormat.Literal)
					WriteLineUni ("}");

				if (typeMap.TypeData.Type.IsAbstract) {
					GenerateEndHook ();
					WriteLine ("return ob;");
					return;
				}
	
				WriteLine ("");
				WriteLine (String.Format ("ob = ({0}) Activator.CreateInstance(typeof({0}), true);", typeMap.TypeData.CSharpFullName));
			}
			
			WriteLine ("");
			
			WriteLine ("Reader.MoveToElement();");
			WriteLine ("");
			
			GenerateReadMembers (typeMap, (ClassMap)typeMap.ObjectMap, "ob", false, false);
			
			WriteLine ("");
			
			GenerateEndHook ();
			WriteLine ("return ob;");
		}

		void GenerateReadMembers (XmlMapping xmlMap, ClassMap map, string ob, bool isValueList, bool readByOrder)
		{
			XmlTypeMapping typeMap = xmlMap as XmlTypeMapping;
			Type xmlMapType = (typeMap != null) ? typeMap.TypeData.Type : typeof(object[]);
			
			bool first = false;
			// Read attributes
			GenerateReadAttributeMembers (xmlMap, map, ob, isValueList, ref first);

			if (!isValueList)
			{
				WriteLine ("Reader.MoveToElement();");
				WriteLineInd ("if (Reader.IsEmptyElement) {"); 
				WriteLine ("Reader.Skip ();");
				GenerateSetListMembersDefaults (typeMap, map, ob, isValueList);
				WriteLine ("return " + ob + ";");
				WriteLineUni ("}");
				WriteLine ("");
	
				WriteLine ("Reader.ReadStartElement();");
			}
			
			// Reads elements

			WriteLine("Reader.MoveToContent();");
			WriteLine ("");

			if (!GenerateReadHook (HookType.elements, xmlMapType))
			{
				string[] readFlag = null;
				if (map.ElementMembers != null && !readByOrder)
				{
					string readFlagsVars = string.Empty;
					readFlag = new string[map.ElementMembers.Count];
					int n=0;
					foreach (XmlTypeMapMember mem in map.ElementMembers) {
						// The text collector doesn't need a flag
						if (!((mem is XmlTypeMapMemberElement) && ((XmlTypeMapMemberElement)mem).IsXmlTextCollector)) {
							readFlag[n] = GetBoolTempVar ();
							if (readFlagsVars.Length > 0) readFlagsVars += ", ";
							readFlagsVars += readFlag[n] + "=false";
						}
						n++;
					}
					if (readFlagsVars.Length > 0) {
						readFlagsVars = "bool " + readFlagsVars;
						WriteLine (readFlagsVars + ";");
					}
					WriteLine ("");
				}
				
				string[] indexes = null;
				string[] flatLists = null;
				string[] flatListsChoices = null;
	
				if (map.FlatLists != null) 
				{
					indexes = new string[map.FlatLists.Count];
					flatLists = new string[map.FlatLists.Count];
					
					string code = "int ";
					for (int n=0; n<map.FlatLists.Count; n++) 
					{
						XmlTypeMapMemberElement mem = (XmlTypeMapMemberElement)map.FlatLists[n];
						indexes[n] = GetNumTempVar ();
						if (n > 0) code += ", ";
						code += indexes[n] + "=0";
						if (!MemberHasReadReplaceHook (xmlMapType, mem)) {
							flatLists[n] = GetObTempVar ();
							string rval;
							WriteLine (mem.TypeData.CSharpFullName + " " + flatLists[n] + ";");
							if (IsReadOnly (typeMap, mem, mem.TypeData, isValueList)) {
								rval = GenerateGetMemberValue (mem, ob, isValueList);
								WriteLine (flatLists[n] + " = " + rval + ";");
							} else if (mem.TypeData.Type.IsArray) {
								rval = GenerateInitializeList (mem.TypeData);
								WriteLine (flatLists[n] + " = " + rval + ";");
							} else {
								WriteLine (flatLists[n] + " = " + GenerateGetMemberValue (mem, ob, isValueList) + ";");
								WriteLineInd ("if (((object)" + flatLists[n] + ") == null) {");
								WriteLine (flatLists[n] + " = " + GenerateInitializeList (mem.TypeData) + ";");
								GenerateSetMemberValue (mem, ob, flatLists[n], isValueList);
								WriteLineUni ("}");
							}
						}
						
						if (mem.ChoiceMember != null) {
							if (flatListsChoices == null)
								flatListsChoices = new string [map.FlatLists.Count];
							flatListsChoices[n] = GetObTempVar ();
							string rval = GenerateInitializeList (mem.ChoiceTypeData);
							WriteLine (mem.ChoiceTypeData.CSharpFullName + " " + flatListsChoices[n] + " = " + rval + ";");
						}
					}
					WriteLine (code + ";");
					WriteLine ("");
				}
				
				if (_format == SerializationFormat.Encoded && map.ElementMembers != null)
				{
					_fixupCallbacks.Add (xmlMap);
					WriteLine ("Fixup fixup = new Fixup(" + ob + ", new XmlSerializationFixupCallback(" + GetFixupCallbackName (xmlMap) + "), " + map.ElementMembers.Count + ");");
					WriteLine ("AddFixup (fixup);");
					WriteLine ("");
				}
	
				ArrayList infos = null;
				
				int maxInd;
				if (readByOrder) {
					if (map.ElementMembers != null) maxInd = map.ElementMembers.Count;
					else maxInd = 0;
				}
				else
				{
					infos = new ArrayList ();
					infos.AddRange (map.AllElementInfos);
					maxInd = infos.Count;
					
					WriteLine ("while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) ");
					WriteLineInd ("{");
					WriteLine ("if (Reader.NodeType == System.Xml.XmlNodeType.Element) ");
					WriteLineInd ("{");
				}
				
				first = true;
				for (int ind = 0; ind < maxInd; ind++)
				{
					XmlTypeMapElementInfo info = readByOrder ? map.GetElement (ind) : (XmlTypeMapElementInfo) infos [ind];
					
					if (!readByOrder)
					{
						if (info.IsTextElement || info.IsUnnamedAnyElement) continue;
						string elemCond = first ? "" : "else ";
						elemCond += "if (";
						if (!(info.Member.IsReturnValue && _format == SerializationFormat.Encoded)) {
							elemCond += "Reader.LocalName == " + GetLiteral (info.ElementName);
							if (!map.IgnoreMemberNamespace) elemCond += " && Reader.NamespaceURI == " + GetLiteral (info.Namespace);
							elemCond += " && ";
						}
						elemCond += "!" + readFlag[info.Member.Index] + ") {";
						WriteLineInd (elemCond);
					}
	
					if (info.Member.GetType() == typeof (XmlTypeMapMemberList))
					{
						if (_format == SerializationFormat.Encoded && info.MultiReferenceType)
						{
							string list = GetObTempVar ();
							WriteLine ("object " + list + " = ReadReferencingElement (out fixup.Ids[" + info.Member.Index + "]);");
							RegisterReferencingMap (info.MappedType);

							WriteLineInd ("if (fixup.Ids[" + info.Member.Index + "] == null) {");	// Already read
							if (IsReadOnly (typeMap, info.Member, info.TypeData, isValueList)) 
								WriteLine ("throw CreateReadOnlyCollectionException (" + GetLiteral(info.TypeData.CSharpFullName) + ");");
							else 
								GenerateSetMemberValue (info.Member, ob, GetCast (info.Member.TypeData,list), isValueList);
							WriteLineUni ("}");
	
							if (!info.MappedType.TypeData.Type.IsArray)
							{
								WriteLineInd ("else {");
								if (IsReadOnly (typeMap, info.Member, info.TypeData, isValueList)) 
									WriteLine (list + " = " + GenerateGetMemberValue (info.Member, ob, isValueList) + ";");
								else { 
									WriteLine (list + " = " + GenerateCreateList (info.MappedType.TypeData.Type) + ";");
									GenerateSetMemberValue (info.Member, ob, GetCast (info.Member.TypeData,list), isValueList);
								}
								WriteLine ("AddFixup (new CollectionFixup (" + list + ", new XmlSerializationCollectionFixupCallback (" + GetFillListName(info.Member.TypeData) + "), fixup.Ids[" + info.Member.Index + "]));");
								WriteLine ("fixup.Ids[" + info.Member.Index + "] = null;");		// The member already has the value, no further fix needed.
								WriteLineUni ("}");
							}
						}
						else
						{
							if (!GenerateReadMemberHook (xmlMapType, info.Member)) {
								if (IsReadOnly (typeMap, info.Member, info.TypeData, isValueList)) {
									GenerateReadListElement (info.MappedType, GenerateGetMemberValue (info.Member, ob, isValueList), GetLiteral(info.IsNullable), false);
								} else if (info.MappedType.TypeData.Type.IsArray) {
									if (info.IsNullable)
										GenerateSetMemberValue (info.Member, ob, GenerateReadListElement (info.MappedType, null, GetLiteral(info.IsNullable), true), isValueList);
									else {
										string list = GetObTempVar ();
										WriteLine (info.MappedType.TypeData.CSharpFullName + " " + list + " = " + GenerateReadListElement (info.MappedType, null, GetLiteral(info.IsNullable), true) + ";");
										WriteLineInd ("if (((object)" + list + ") != null) {");
										GenerateSetMemberValue (info.Member, ob, list, isValueList);
										WriteLineUni ("}");
									}
								} else {
									string list = GetObTempVar ();
									WriteLine (info.MappedType.TypeData.CSharpFullName + " " + list + " = " + GenerateGetMemberValue (info.Member, ob, isValueList) + ";");
									WriteLineInd ("if (((object)" + list + ") == null) {");
									WriteLine (list + " = " + GenerateCreateList (info.MappedType.TypeData.Type) + ";");
									GenerateSetMemberValue (info.Member, ob, list, isValueList);
									WriteLineUni ("}");
									GenerateReadListElement (info.MappedType, list, GetLiteral(info.IsNullable), true);
								}
								GenerateEndHook ();
							}
						}
						if (!readByOrder)
							WriteLine (readFlag[info.Member.Index] + " = true;");
					}
					else if (info.Member.GetType() == typeof (XmlTypeMapMemberFlatList))
					{
						XmlTypeMapMemberFlatList mem = (XmlTypeMapMemberFlatList)info.Member;
						if (!GenerateReadArrayMemberHook (xmlMapType, info.Member, indexes[mem.FlatArrayIndex])) {
							GenerateAddListValue (mem.TypeData, flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex], GenerateReadObjectElement (info), !IsReadOnly (typeMap, info.Member, info.TypeData, isValueList));
							if (mem.ChoiceMember != null) {
								GenerateAddListValue (mem.ChoiceTypeData, flatListsChoices[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex], GetLiteral (info.ChoiceValue), true);
							}
							GenerateEndHook ();
						}
						WriteLine (indexes[mem.FlatArrayIndex] + "++;");
					}
					else if (info.Member.GetType() == typeof (XmlTypeMapMemberAnyElement))
					{
						XmlTypeMapMemberAnyElement mem = (XmlTypeMapMemberAnyElement)info.Member;
						if (mem.TypeData.IsListType) { 
							if (!GenerateReadArrayMemberHook (xmlMapType, info.Member, indexes[mem.FlatArrayIndex])) {
								GenerateAddListValue (mem.TypeData, flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex], GetReadXmlNode (mem.TypeData.ListItemTypeData, false), true);
								GenerateEndHook ();
							}
							WriteLine (indexes[mem.FlatArrayIndex] + "++;");
						}
						else {
							if (!GenerateReadMemberHook (xmlMapType, info.Member)) {
								GenerateSetMemberValue (mem, ob, GetReadXmlNode(mem.TypeData, false), isValueList);
								GenerateEndHook ();
							}
						}
					}
					else if (info.Member.GetType() == typeof(XmlTypeMapMemberElement))
					{
						if (!readByOrder)
							WriteLine (readFlag[info.Member.Index] + " = true;");
						if (_format == SerializationFormat.Encoded)
						{
							string val = GetObTempVar ();
							RegisterReferencingMap (info.MappedType);
							
							if (info.Member.TypeData.SchemaType != SchemaTypes.Primitive)
								WriteLine ("object " + val + " = ReadReferencingElement (out fixup.Ids[" + info.Member.Index + "]);");
							else
								WriteLine ("object " + val + " = ReadReferencingElement (" + GetLiteral(info.Member.TypeData.XmlType) + ", " + GetLiteral(System.Xml.Schema.XmlSchema.Namespace) + ", out fixup.Ids[" + info.Member.Index + "]);");
							
							if (info.MultiReferenceType)
								WriteLineInd ("if (fixup.Ids[" + info.Member.Index + "] == null) {");	// already read
							else
								WriteLineInd ("if (" + val + " != null) {");	// null value
								
							GenerateSetMemberValue (info.Member, ob, GetCast (info.Member.TypeData,val), isValueList);
							WriteLineUni ("}");
						}
						else if (!GenerateReadMemberHook (xmlMapType, info.Member)) {
							if (info.ChoiceValue != null) {
								XmlTypeMapMemberElement imem = (XmlTypeMapMemberElement) info.Member;
								WriteLine (ob + ".@" + imem.ChoiceMember + " = " + GetLiteral(info.ChoiceValue) + ";");
							}
							GenerateSetMemberValue (info.Member, ob, GenerateReadObjectElement (info), isValueList);
							GenerateEndHook ();
						}
					}
					else
						throw new InvalidOperationException ("Unknown member type");
	
					if (!readByOrder)
						WriteLineUni ("}");
					else
						WriteLine ("Reader.MoveToContent();");
					first = false;
				}
				
				if (!readByOrder)
				{
					if (!first) WriteLineInd ("else {");
					
					if (map.DefaultAnyElementMember != null)
					{
						XmlTypeMapMemberAnyElement mem = map.DefaultAnyElementMember;
						if (mem.TypeData.IsListType) {
							if (!GenerateReadArrayMemberHook (xmlMapType, mem, indexes[mem.FlatArrayIndex])) {
								GenerateAddListValue (mem.TypeData, flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex], GetReadXmlNode(mem.TypeData.ListItemTypeData, false), true);
								GenerateEndHook ();
							}
							WriteLine (indexes[mem.FlatArrayIndex] + "++;");
						}
						else if (! GenerateReadMemberHook (xmlMapType, mem)) {
							GenerateSetMemberValue (mem, ob, GetReadXmlNode(mem.TypeData, false), isValueList);
							GenerateEndHook ();
						}
					}
					else {
						if (!GenerateReadHook (HookType.unknownElement, xmlMapType)) {
							WriteLine ("UnknownNode (" + ob + ");");
							GenerateEndHook ();
						}
					}
					
					if (!first) WriteLineUni ("}");
		
					WriteLineUni ("}");
					
					if (map.XmlTextCollector != null)
					{
						WriteLine ("else if (Reader.NodeType == System.Xml.XmlNodeType.Text || Reader.NodeType == System.Xml.XmlNodeType.CDATA)");
						WriteLineInd ("{");
		
						if (map.XmlTextCollector is XmlTypeMapMemberExpandable)
						{
							XmlTypeMapMemberExpandable mem = (XmlTypeMapMemberExpandable)map.XmlTextCollector;
							XmlTypeMapMemberFlatList flatl = mem as XmlTypeMapMemberFlatList;
							TypeData itype = (flatl == null) ? mem.TypeData.ListItemTypeData : flatl.ListMap.FindTextElement().TypeData;
							
							if (!GenerateReadArrayMemberHook (xmlMapType, map.XmlTextCollector, indexes[mem.FlatArrayIndex])) {
								string val = (itype.Type == typeof (string)) ? "Reader.ReadString()" : GetReadXmlNode (itype, false);
								GenerateAddListValue (mem.TypeData, flatLists[mem.FlatArrayIndex], indexes[mem.FlatArrayIndex], val, true);
								GenerateEndHook ();
							}
							WriteLine (indexes[mem.FlatArrayIndex] + "++;");
						}
						else if (!GenerateReadMemberHook (xmlMapType, map.XmlTextCollector))
						{
							XmlTypeMapMemberElement mem = (XmlTypeMapMemberElement) map.XmlTextCollector;
							XmlTypeMapElementInfo info = (XmlTypeMapElementInfo) mem.ElementInfo [0];
							if (info.TypeData.Type == typeof (string))
								GenerateSetMemberValue (mem, ob, "ReadString (" + GenerateGetMemberValue (mem, ob, isValueList) + ")", isValueList);
							else {
								WriteLineInd ("{");
								string str = GetStrTempVar ();
								WriteLine ("string " + str + " = Reader.ReadString();");
								GenerateSetMemberValue (mem, ob, GenerateGetValueFromXmlString (str, info.TypeData, info.MappedType, info.IsNullable), isValueList);
								WriteLineUni ("}");
							}
							GenerateEndHook ();
						}
						WriteLineUni ("}");
					}
						
					WriteLine ("else");
					WriteLine ("\tUnknownNode(" + ob + ");");
					WriteLine ("");
					WriteLine ("Reader.MoveToContent();");
					WriteLineUni ("}");
				}
				else
					WriteLine ("Reader.MoveToContent();");
	
				if (flatLists != null)
				{
					WriteLine ("");
					foreach (XmlTypeMapMemberExpandable mem in map.FlatLists)
					{
						if (MemberHasReadReplaceHook (xmlMapType, mem)) continue;
						
						string list = flatLists[mem.FlatArrayIndex];
						if (mem.TypeData.Type.IsArray)
							WriteLine (list + " = (" + mem.TypeData.CSharpFullName + ") ShrinkArray (" + list + ", " + indexes[mem.FlatArrayIndex] + ", " + GetTypeOf(mem.TypeData.Type.GetElementType()) + ", true);");
						if (!IsReadOnly (typeMap, mem, mem.TypeData, isValueList) && mem.TypeData.Type.IsArray)
							GenerateSetMemberValue (mem, ob, list, isValueList);
					}
				}
				
				if (flatListsChoices != null)
				{
					WriteLine ("");
					foreach (XmlTypeMapMemberExpandable mem in map.FlatLists)
					{
						if (MemberHasReadReplaceHook (xmlMapType, mem)) continue;
						if (mem.ChoiceMember == null) continue;
						
						string list = flatListsChoices[mem.FlatArrayIndex];
						WriteLine (list + " = (" + mem.ChoiceTypeData.CSharpFullName + ") ShrinkArray (" + list + ", " + indexes[mem.FlatArrayIndex] + ", " + GetTypeOf(mem.ChoiceTypeData.Type.GetElementType()) + ", true);");
						WriteLine (ob + ".@" + mem.ChoiceMember + " = " + list + ";");
					}
				}
				
				GenerateSetListMembersDefaults (typeMap, map, ob, isValueList);
				
				GenerateEndHook ();
			}			

			if (!isValueList)
			{
				WriteLine ("");
				WriteLine ("ReadEndElement();");
			}
		}

		void GenerateReadAttributeMembers (XmlMapping xmlMap, ClassMap map, string ob, bool isValueList, ref bool first)
		{
			XmlTypeMapping typeMap = xmlMap as XmlTypeMapping;
			Type xmlMapType = (typeMap != null) ? typeMap.TypeData.Type : typeof(object[]);

			if (GenerateReadHook (HookType.attributes, xmlMapType))
				return;

			XmlTypeMapMember anyAttrMember = map.DefaultAnyAttributeMember;
			
			if (anyAttrMember != null)
			{
				WriteLine ("int anyAttributeIndex = 0;");
				WriteLine (anyAttrMember.TypeData.CSharpFullName + " anyAttributeArray = null;");
			}
			
			WriteLine ("while (Reader.MoveToNextAttribute())");
			WriteLineInd ("{");
			first = true;
			if (map.AttributeMembers != null) {
				foreach (XmlTypeMapMemberAttribute at in map.AttributeMembers)
				{
					WriteLineInd ((first?"":"else ") + "if (Reader.LocalName == " + GetLiteral (at.AttributeName) + " && Reader.NamespaceURI == " + GetLiteral (at.Namespace) + ") {");
					if (!GenerateReadMemberHook (xmlMapType, at)) {
						GenerateSetMemberValue (at, ob, GenerateGetValueFromXmlString ("Reader.Value", at.TypeData, at.MappedType, false), isValueList);
						GenerateEndHook ();
					}
					WriteLineUni ("}");
					first = false;
				}
			}
			WriteLineInd ((first?"":"else ") + "if (IsXmlnsAttribute (Reader.Name)) {");

			// If the map has NamespaceDeclarations,
			// then store this xmlns to the given member.
			// If the instance doesn't exist, then create.
			
			if (map.NamespaceDeclarations != null) {
				if (!GenerateReadMemberHook (xmlMapType, map.NamespaceDeclarations)) {
					string nss = ob + ".@" + map.NamespaceDeclarations.Name;
					WriteLine ("if (" + nss + " == null) " + nss + " = new XmlSerializerNamespaces ();");
					WriteLineInd ("if (Reader.Prefix == \"xmlns\")");
					WriteLine (nss + ".Add (Reader.LocalName, Reader.Value);");
					Unindent ();
					WriteLineInd ("else");
					WriteLine (nss + ".Add (\"\", Reader.Value);");
					Unindent ();
					GenerateEndHook ();
				}
			}
			
			WriteLineUni ("}");
			WriteLineInd ("else {");

			if (anyAttrMember != null) 
			{
				if (!GenerateReadArrayMemberHook (xmlMapType, anyAttrMember, "anyAttributeIndex")) {
					WriteLine ("System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) Document.ReadNode(Reader);");
					if (typeof(System.Xml.Schema.XmlSchemaAnnotated).IsAssignableFrom (xmlMapType)) 
						WriteLine ("ParseWsdlArrayType (attr);");
					GenerateAddListValue (anyAttrMember.TypeData, "anyAttributeArray", "anyAttributeIndex", GetCast (anyAttrMember.TypeData.ListItemTypeData, "attr"), true);
					GenerateEndHook ();
				}
				WriteLine ("anyAttributeIndex++;");
			}
			else {
				if (!GenerateReadHook (HookType.unknownAttribute, xmlMapType)) {
					WriteLine ("UnknownNode (" + ob + ");");
					GenerateEndHook ();
				}
			}

			WriteLineUni ("}");
			WriteLineUni ("}");

			if (anyAttrMember != null && !MemberHasReadReplaceHook (xmlMapType, anyAttrMember))
			{
				WriteLine ("");
				WriteLine("anyAttributeArray = (" + anyAttrMember.TypeData.CSharpFullName + ") ShrinkArray (anyAttributeArray, anyAttributeIndex, " + GetTypeOf(anyAttrMember.TypeData.Type.GetElementType()) + ", true);");
				GenerateSetMemberValue (anyAttrMember, ob, "anyAttributeArray", isValueList);
			}
			WriteLine ("");
			WriteLine ("Reader.MoveToElement ();");

			GenerateEndHook ();
		}
		
		void GenerateSetListMembersDefaults (XmlTypeMapping typeMap, ClassMap map, string ob, bool isValueList)
		{
			if (map.ListMembers != null)
			{
				ArrayList members = map.ListMembers;
				for (int n=0; n<members.Count; n++) {
					XmlTypeMapMember mem = (XmlTypeMapMember) members[n];
					if (IsReadOnly (typeMap, mem, mem.TypeData, isValueList))
						continue;
					WriteLineInd ("if (" + GenerateGetMemberValue (mem, ob, isValueList) + " == null) {");
					GenerateSetMemberValue (mem, ob, GenerateInitializeList (mem.TypeData), isValueList);
					WriteLineUni ("}");
				}
			}
		}
		
		bool IsReadOnly (XmlTypeMapping map, XmlTypeMapMember member, TypeData memType, bool isValueList)
		{
			if (isValueList) return !memType.HasPublicConstructor;
			else return member.IsReadOnly (map.TypeData.Type) || !memType.HasPublicConstructor;
		}

		void GenerateSetMemberValue (XmlTypeMapMember member, string ob, string value, bool isValueList)
		{
			if (isValueList) WriteLine (ob + "[" + member.GlobalIndex + "] = " + value + ";");
			else {
				WriteLine (ob + ".@" + member.Name + " = " + value + ";");
				if (member.IsOptionalValueType)
					WriteLine (ob + "." + member.Name + "Specified = true;");
			}
		}

		void GenerateSetMemberValueFromAttr (XmlTypeMapMember member, string ob, string value, bool isValueList)
		{
			// Enumeration values specified in custom attributes are stored as integer
			// values if the custom attribute property is of type object. So, it is
			// necessary to convert to the enum type before asigning the value to the field.
			
			if (member.TypeData.Type.IsEnum)
				value = GetCast (member.TypeData.Type, value);
			GenerateSetMemberValue (member, ob, value, isValueList);
		}

		string GenerateReadObjectElement (XmlTypeMapElementInfo elem)
		{
			switch (elem.TypeData.SchemaType)
			{
				case SchemaTypes.XmlNode:
					return GetReadXmlNode (elem.TypeData, true);

				case SchemaTypes.Primitive:
				case SchemaTypes.Enum:
					return GenerateReadPrimitiveValue (elem);

				case SchemaTypes.Array:
					return GenerateReadListElement (elem.MappedType, null, GetLiteral(elem.IsNullable), true);

				case SchemaTypes.Class:
					return GetReadObjectCall (elem.MappedType, GetLiteral(elem.IsNullable), "true");

				case SchemaTypes.XmlSerializable:
					return GetCast (elem.TypeData, String.Format ("({0}) ReadSerializable (({0}) Activator.CreateInstance(typeof({0}), true))", elem.TypeData.CSharpFullName));

				default:
					throw new NotSupportedException ("Invalid value type");
			}
		}

		string GenerateReadPrimitiveValue (XmlTypeMapElementInfo elem)
		{
			if (elem.TypeData.Type == typeof (XmlQualifiedName)) {
				if (elem.IsNullable) return "ReadNullableQualifiedName ()";
				else return "ReadElementQualifiedName ()";
			}
			else if (elem.IsNullable) {
				string str = GetStrTempVar ();
				WriteLine ("string " + str + " = ReadNullableString ();");
				return GenerateGetValueFromXmlString (str, elem.TypeData, elem.MappedType, true);
			}
			else {
				string str = GetStrTempVar ();
				WriteLine ("string " + str + " = Reader.ReadElementString ();");
				return GenerateGetValueFromXmlString (str, elem.TypeData, elem.MappedType, false);
			}
		}
		
		string GenerateGetValueFromXmlString (string value, TypeData typeData, XmlTypeMapping typeMap, bool isNullable)
		{
			if (typeData.SchemaType == SchemaTypes.Array)
				return GenerateReadListString (typeMap, value);
			else if (typeData.SchemaType == SchemaTypes.Enum)
				return GenerateGetEnumValue (typeMap, value, isNullable);
			else if (typeData.Type == typeof (XmlQualifiedName))
				return "ToXmlQualifiedName (" + value + ")";
			else 
				return XmlCustomFormatter.GenerateFromXmlString (typeData, value);
		}
		
		string GenerateReadListElement (XmlTypeMapping typeMap, string list, string isNullable, bool canCreateInstance)
		{
			Type listType = typeMap.TypeData.Type;
			ListMap listMap = (ListMap)typeMap.ObjectMap;
			bool doNullCheck = typeMap.TypeData.Type.IsArray;

			if (canCreateInstance && typeMap.TypeData.HasPublicConstructor) 
			{
				if (list == null) {
					list = GetObTempVar ();
					WriteLine (typeMap.TypeData.CSharpFullName + " " + list + " = null;");
					if (doNullCheck)
						WriteLineInd ("if (!ReadNull()) {");
					WriteLine (list + " = " + GenerateCreateList (listType) + ";");
				} else {
					if (doNullCheck)
						WriteLineInd ("if (!ReadNull()) {");
				}
			}
			else
			{
				if (list != null) {
					WriteLineInd ("if (((object)" + list + ") == null)");
					WriteLine ("throw CreateReadOnlyCollectionException (" + GetLiteral (typeMap.TypeData.CSharpFullName) + ");");
					Unindent ();
					doNullCheck = false;
				}
				else {
					WriteLine ("throw CreateReadOnlyCollectionException (" + GetLiteral (typeMap.TypeData.CSharpFullName) + ");");
					return list;
				}
			}
				
			WriteLineInd ("if (Reader.IsEmptyElement) {");
			WriteLine ("Reader.Skip();");
			if (listType.IsArray)
				WriteLine (list + " = (" + typeMap.TypeData.CSharpFullName + ") ShrinkArray (" + list + ", 0, " + GetTypeOf(listType.GetElementType()) + ", false);");

			Unindent ();
			WriteLineInd ("} else {");

			string index = GetNumTempVar ();
			WriteLine ("int " + index + " = 0;");
			WriteLine ("Reader.ReadStartElement();");
			WriteLine ("Reader.MoveToContent();");
			WriteLine ("");

			WriteLine ("while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) ");
			WriteLineInd ("{");
			WriteLine ("if (Reader.NodeType == System.Xml.XmlNodeType.Element) ");
			WriteLineInd ("{");

			bool first = true;
			foreach (XmlTypeMapElementInfo elemInfo in listMap.ItemInfo)
			{
				WriteLineInd ((first?"":"else ") + "if (Reader.LocalName == " + GetLiteral (elemInfo.ElementName) + " && Reader.NamespaceURI == " + GetLiteral (elemInfo.Namespace) + ") {");
				GenerateAddListValue (typeMap.TypeData, list, index, GenerateReadObjectElement (elemInfo), false);
				WriteLine (index + "++;");
				WriteLineUni ("}");
				first = false;
			}
			if (!first) WriteLine ("else UnknownNode (null);");
			else WriteLine ("UnknownNode (null);");
			
			WriteLineUni ("}");
			WriteLine ("else UnknownNode (null);");
			WriteLine ("");
			WriteLine ("Reader.MoveToContent();");
			WriteLineUni ("}");
			
			WriteLine ("ReadEndElement();");

			if (listType.IsArray)
				WriteLine (list + " = (" + typeMap.TypeData.CSharpFullName + ") ShrinkArray (" + list + ", " + index + ", " + GetTypeOf(listType.GetElementType()) + ", false);");

			WriteLineUni ("}");
			if (doNullCheck)
				WriteLineUni ("}");

			return list;
		}

		string GenerateReadListString (XmlTypeMapping typeMap, string values)
		{
			Type listType = typeMap.TypeData.Type;
			ListMap listMap = (ListMap)typeMap.ObjectMap;
			string itemType = ToCSharpFullName (listType.GetElementType());
			
			string list = GetObTempVar ();
			WriteLine (itemType + "[] " + list + ";");
			
			string var = GetStrTempVar ();
			WriteLine ("string " + var + " = " + values + ".Trim();");
			WriteLineInd ("if (" + var + " != string.Empty) {");
			
			string valueArray = GetObTempVar ();
			WriteLine ("string[] " + valueArray + " = " + var + ".Split (' ');");
			
			WriteLine (list + " = new " + GetArrayDeclaration (listType, valueArray + ".Length") + ";");
			
			XmlTypeMapElementInfo info = (XmlTypeMapElementInfo)listMap.ItemInfo[0];

			string index = GetNumTempVar ();
			WriteLineInd ("for (int " + index + " = 0; " + index + " < " + valueArray + ".Length; " + index + "++)");
			WriteLine (list + "[" + index + "] = " + GenerateGetValueFromXmlString (valueArray + "[" + index + "]", info.TypeData, info.MappedType, info.IsNullable) + ";");
			Unindent ();
			WriteLineUni ("}");
			WriteLine ("else");
			WriteLine ("\t" + list + " = new " + GetArrayDeclaration (listType, "0") + ";");
			
			return list;
		}
		
		string GetArrayDeclaration (Type type, string length)
		{
			Type t = type.GetElementType();
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			sb.Append ('[').Append (length).Append (']');
			while (t.IsArray) {
				sb.Append ("[]");
				t = t.GetElementType();
			}
			sb.Insert (0, ToCSharpFullName (t));
			return sb.ToString ();
		}

		void GenerateAddListValue (TypeData listType, string list, string index, string value, bool canCreateInstance)
		{
			Type type = listType.Type;
			if (type.IsArray)
			{
				WriteLine (list + " = (" + ToCSharpFullName (type) + ") EnsureArrayIndex (" + list + ", " + index + ", " + GetTypeOf(type.GetElementType()) + ");");
				WriteLine (list + "[" + index + "] = " + value + ";");
			}
			else	// Must be IEnumerable
			{
				WriteLine ("if (((object)" + list + ") == null)");
				if (canCreateInstance) 
					WriteLine ("\t" + list + String.Format (" = ({0}) Activator.CreateInstance(typeof({0}), true);", listType.CSharpFullName));
				else 
					WriteLine ("\tthrow CreateReadOnlyCollectionException (" + GetLiteral (listType.CSharpFullName) + ");");
				
				WriteLine (list + ".Add (" + value + ");");
			}
		}

		string GenerateCreateList (Type listType)
		{
			if (listType.IsArray)
				return "(" + ToCSharpFullName (listType) + ") EnsureArrayIndex (null, 0, " + GetTypeOf(listType.GetElementType()) + ")";
			else
				return "new " + ToCSharpFullName (listType) + "()";
		}
		
		string GenerateInitializeList (TypeData listType)
		{
			if (listType.Type.IsArray)
				return "null";
			else
				return "new " + listType.CSharpFullName + "()";
		}
		
		void GenerateFillerCallbacks ()
		{
			foreach (TypeData td in _listsToFill)
			{
				string metName = GetFillListName (td);
				WriteLine ("void " + metName + " (object list, object source)");
				WriteLineInd ("{");
				WriteLine ("if (((object)list) == null) throw CreateReadOnlyCollectionException (" + GetLiteral (td.CSharpFullName) + ");");
				WriteLine ("");

				WriteLine (td.CSharpFullName + " dest = (" + td.CSharpFullName + ") list;");
				WriteLine ("foreach (object ob in (IEnumerable)source)");
				WriteLine ("\tdest.Add (" + GetCast (td.ListItemTypeData, "ob") + ");");
				WriteLineUni ("}");
				WriteLine ("");
			}
		}

		void GenerateReadXmlNodeElement (XmlTypeMapping typeMap, string isNullable)
		{
			WriteLine ("return " + GetReadXmlNode (typeMap.TypeData, false) + ";");
		}

		void GenerateReadPrimitiveElement (XmlTypeMapping typeMap, string isNullable)
		{
			WriteLine ("XmlQualifiedName t = GetXsiType();");
			WriteLine ("if (t == null) t = new XmlQualifiedName (" + GetLiteral(typeMap.XmlType) + ", " + GetLiteral(typeMap.Namespace) + ");");
			WriteLine ("return " + GetCast (typeMap.TypeData, "ReadTypedPrimitive (t)") + ";");
		}

		void GenerateReadEnumElement (XmlTypeMapping typeMap, string isNullable)
		{
			WriteLine ("Reader.ReadStartElement ();");
			WriteLine (typeMap.TypeData.CSharpFullName + " res = " + GenerateGetEnumValue (typeMap, "Reader.ReadString()", false) + ";");
			WriteLineInd ("if (Reader.NodeType != XmlNodeType.None)");
			WriteLineUni ("Reader.ReadEndElement ();");
			WriteLine ("return res;");
		}

		string GenerateGetEnumValue (XmlTypeMapping typeMap, string val, bool isNullable)
		{
			if (isNullable)
				return "(" + val + ") != null ? " + GetGetEnumValueName (typeMap) + " (" + val + ") : (" + typeMap.TypeData.CSharpFullName + "?) null";
			else
				return GetGetEnumValueName (typeMap) + " (" + val + ")";
		}
		
		void GenerateGetEnumValueMethod (XmlTypeMapping typeMap)
		{
			string metName = GetGetEnumValueName (typeMap);
			EnumMap map = (EnumMap) typeMap.ObjectMap;

			if (map.IsFlags)
			{
				string switchMethod =  metName + "_Switch";
				WriteLine (typeMap.TypeData.CSharpFullName + " " + metName + " (string xmlName)");
				WriteLineInd ("{");
				WriteLine ("xmlName = xmlName.Trim();");
				WriteLine ("if (xmlName.Length == 0) return (" + typeMap.TypeData.CSharpFullName + ")0;");
				WriteLine (typeMap.TypeData.CSharpFullName + " sb = (" + typeMap.TypeData.CSharpFullName + ")0;");
				WriteLine ("string[] enumNames = xmlName.Split (null);");
				WriteLine ("foreach (string name in enumNames)");
				WriteLineInd ("{");
				WriteLine ("if (name == string.Empty) continue;");
				WriteLine ("sb |= " + switchMethod + " (name); ");
				WriteLineUni ("}");
				WriteLine ("return sb;");
				WriteLineUni ("}");
				WriteLine ("");
				metName = switchMethod;
			}

			WriteLine (typeMap.TypeData.CSharpFullName + " " + metName + " (string xmlName)");
			WriteLineInd ("{");
			GenerateGetSingleEnumValue (typeMap, "xmlName");
			WriteLineUni ("}");
			WriteLine ("");
		}
		
		void GenerateGetSingleEnumValue (XmlTypeMapping typeMap, string val)
		{
			EnumMap map = (EnumMap) typeMap.ObjectMap;
			WriteLine ("switch (" + val + ")");
			WriteLineInd ("{");
			foreach (EnumMap.EnumMapMember mem in map.Members)
			{
				WriteLine ("case " + GetLiteral (mem.XmlName) + ": return " + typeMap.TypeData.CSharpFullName + ".@" + mem.EnumName + ";");
			}
			WriteLineInd ("default:");
			WriteLine ("throw CreateUnknownConstantException (" + val + ", typeof(" + typeMap.TypeData.CSharpFullName + "));");
			Unindent ();
			WriteLineUni ("}");
		}
		
		void GenerateReadXmlSerializableElement (XmlTypeMapping typeMap, string isNullable)
		{
			WriteLine ("Reader.MoveToContent ();");
			WriteLine ("if (Reader.NodeType == XmlNodeType.Element)");
			WriteLineInd ("{");
			WriteLine ("if (Reader.LocalName == " + GetLiteral (typeMap.ElementName) + " && Reader.NamespaceURI == " + GetLiteral (typeMap.Namespace) + ")");
			WriteLine (String.Format ("\treturn ({0}) ReadSerializable (({0}) Activator.CreateInstance(typeof({0}), true));", typeMap.TypeData.CSharpFullName));
			WriteLine ("else");
			WriteLine ("\tthrow CreateUnknownNodeException ();");
			WriteLineUni ("}");
			WriteLine ("else UnknownNode (null);");
			WriteLine ("");
			WriteLine ("return default (" + typeMap.TypeData.CSharpFullName + ");");
		}

		void GenerateReadInitCallbacks ()
		{
			WriteLine ("protected override void InitCallbacks ()");
			WriteLineInd ("{");

			if (_format == SerializationFormat.Encoded)
			{
				foreach (XmlMapping xmap in _mapsToGenerate)  
				{
					XmlTypeMapping map = xmap as XmlTypeMapping;
					if (map == null) continue;
					if (map.TypeData.SchemaType == SchemaTypes.Class || map.TypeData.SchemaType == SchemaTypes.Enum)
						WriteMetCall ("AddReadCallback", GetLiteral (map.XmlType), GetLiteral(map.Namespace), GetTypeOf(map.TypeData.Type), "new XmlSerializationReadCallback (" + GetReadObjectName (map) + ")");
				}
			}
			
			WriteLineUni ("}");
			WriteLine ("");

			WriteLine ("protected override void InitIDs ()");
			WriteLine ("{");
			WriteLine ("}");
			WriteLine ("");
		}

		void GenerateFixupCallbacks ()
		{
			foreach (XmlMapping map in _fixupCallbacks)
			{
				bool isList = map is XmlMembersMapping;
				string tname = !isList ? ((XmlTypeMapping)map).TypeData.CSharpFullName : "object[]";
				WriteLine ("void " + GetFixupCallbackName (map) + " (object obfixup)");
				WriteLineInd ("{");					
				WriteLine ("Fixup fixup = (Fixup)obfixup;");
				WriteLine (tname + " source = (" + tname + ") fixup.Source;");
				WriteLine ("string[] ids = fixup.Ids;");
				WriteLine ("");

				ClassMap cmap = (ClassMap)map.ObjectMap;
				ICollection members = cmap.ElementMembers;
				if (members != null) {
					foreach (XmlTypeMapMember member in members)
					{
						WriteLineInd ("if (ids[" + member.Index + "] != null)");
						string val = "GetTarget(ids[" + member.Index + "])";
						if (!isList) val = GetCast (member.TypeData, val);
						GenerateSetMemberValue (member, "source", val, isList);
						Unindent ();
					}
				}
				WriteLineUni ("}");
				WriteLine ("");
			}
		}

		string GetReadXmlNode (TypeData type, bool wrapped)
		{
			if (type.Type == typeof (XmlDocument))
				return GetCast (type, TypeTranslator.GetTypeData (typeof(XmlDocument)), "ReadXmlDocument (" + GetLiteral(wrapped) + ")");
			else
				return GetCast (type, TypeTranslator.GetTypeData (typeof(XmlNode)), "ReadXmlNode (" + GetLiteral(wrapped) + ")");
		}
		
		#endregion
		
		#region Helper methods

		//*******************************************************
		// Helper methods
		//
		
		ArrayList _listsToFill = new ArrayList ();
		Hashtable _hookVariables;
		Stack _hookContexts;
		Stack _hookOpenHooks;
		
		class HookInfo {
			public HookType HookType;
			public Type Type;
			public string Member;
			public HookDir Direction;
		}

		void InitHooks ()
		{
			_hookContexts = new Stack ();
			_hookOpenHooks = new Stack ();
			_hookVariables = new Hashtable ();
		}
		
		void PushHookContext ()
		{
			_hookContexts.Push (_hookVariables);
			_hookVariables = (Hashtable) _hookVariables.Clone ();
		}
		
		void PopHookContext ()
		{
			_hookVariables = (Hashtable) _hookContexts.Pop ();
		}
		
		void SetHookVar (string var, string value)
		{
			_hookVariables [var] = value;
		}

		bool GenerateReadHook (HookType hookType, Type type)
		{
			return GenerateHook (hookType, HookDir.Read, type, null);
		}

		bool GenerateWriteHook (HookType hookType, Type type)
		{
			return GenerateHook (hookType, HookDir.Write, type, null);
		}
		
		bool GenerateWriteMemberHook (Type type, XmlTypeMapMember member)
		{
			SetHookVar ("$MEMBER", member.Name);
			return GenerateHook (HookType.member, HookDir.Write, type, member.Name);
		}
		
		bool GenerateReadMemberHook (Type type, XmlTypeMapMember member)
		{
			SetHookVar ("$MEMBER", member.Name);
			return GenerateHook (HookType.member, HookDir.Read, type, member.Name);
		}
		
		bool GenerateReadArrayMemberHook (Type type, XmlTypeMapMember member, string index)
		{
			SetHookVar ("$INDEX", index);
			return GenerateReadMemberHook (type, member);
		}
	
		bool MemberHasReadReplaceHook (Type type, XmlTypeMapMember member)
		{
			if (_config == null) return false;
			return _config.GetHooks (HookType.member, HookDir.Read, HookAction.Replace, type, member.Name).Count > 0;
		}
		
		bool GenerateHook (HookType hookType, HookDir dir, Type type, string member)
		{
			GenerateHooks (hookType, dir, type, null, HookAction.InsertBefore);
			if (GenerateHooks (hookType, dir, type, null, HookAction.Replace))
			{
				GenerateHooks (hookType, dir, type, null, HookAction.InsertAfter);
				return true;
			}
			else
			{
				HookInfo hi = new HookInfo ();
				hi.HookType = hookType;
				hi.Type = type;
				hi.Member = member;
				hi.Direction = dir;
				_hookOpenHooks.Push (hi);
				return false;
			}
		}
		
		void GenerateEndHook ()
		{
			HookInfo hi = (HookInfo) _hookOpenHooks.Pop();
			GenerateHooks (hi.HookType, hi.Direction, hi.Type, hi.Member, HookAction.InsertAfter);
		}
		
		bool GenerateHooks (HookType hookType, HookDir dir, Type type, string member, HookAction action)
		{
			if (_config == null) return false;
			ArrayList hooks = _config.GetHooks (hookType, dir, action, type, null);
			if (hooks.Count == 0) return false;			
			foreach (Hook hook in hooks)
			{
				string code = hook.GetCode (action);
				foreach (DictionaryEntry de in _hookVariables)
					code = code.Replace ((string)de.Key, (string)de.Value);
				WriteMultilineCode (code);
			}
			return true;
		}
		
		string GetRootTypeName ()
		{
			if (_typeMap is XmlTypeMapping) return ((XmlTypeMapping)_typeMap).TypeData.CSharpFullName;
			else return "object[]";
		}

		string GetNumTempVar ()
		{
			return "n" + (_tempVarId++);
		}
		
		string GetObTempVar ()
		{
			return "o" + (_tempVarId++);
		}
		
		string GetStrTempVar ()
		{
			return "s" + (_tempVarId++);
		}
		
		string GetBoolTempVar ()
		{
			return "b" + (_tempVarId++);
		}
		
		string GetUniqueName (string uniqueGroup, object ob, string name)
		{
			name = CodeIdentifier.MakeValid (name.Replace ("[]","_array"));
			Hashtable names = (Hashtable) _uniqueNames [uniqueGroup];
			if (names == null) {
				names = new Hashtable ();
				_uniqueNames [uniqueGroup] = names; 
			}
			
			string res = (string) names [ob];
			if (res != null) return res;

			foreach (string n in names.Values)
				if (n == name) return GetUniqueName (uniqueGroup, ob, name + (_methodId++));
				
			names [ob] = name;
			return name;
		}
		
		void RegisterReferencingMap (XmlTypeMapping typeMap)
		{
			if (typeMap != null && !_mapsToGenerate.Contains (typeMap))
				_mapsToGenerate.Add (typeMap);
		}
		
		string GetWriteObjectName (XmlTypeMapping typeMap)
		{
			if (!_mapsToGenerate.Contains (typeMap)) _mapsToGenerate.Add (typeMap);
			return GetUniqueName ("rw", typeMap, "WriteObject_" + typeMap.XmlType);
		}
		
		string GetReadObjectName (XmlTypeMapping typeMap)
		{
			if (!_mapsToGenerate.Contains (typeMap)) _mapsToGenerate.Add (typeMap);
			return GetUniqueName ("rr", typeMap, "ReadObject_" + typeMap.XmlType);
		}
		
		string GetGetEnumValueName (XmlTypeMapping typeMap)
		{
			if (!_mapsToGenerate.Contains (typeMap)) _mapsToGenerate.Add (typeMap);
			return GetUniqueName ("ge", typeMap, "GetEnumValue_" + typeMap.XmlType);
		}

		string GetWriteObjectCallbackName (XmlTypeMapping typeMap)
		{
			if (!_mapsToGenerate.Contains (typeMap)) _mapsToGenerate.Add (typeMap);
			return GetUniqueName ("wc", typeMap, "WriteCallback_" + typeMap.XmlType);
		}
		
		string GetFixupCallbackName (XmlMapping typeMap)
		{
			if (!_mapsToGenerate.Contains (typeMap)) _mapsToGenerate.Add (typeMap);
			
			if (typeMap is XmlTypeMapping)
				return GetUniqueName ("fc", typeMap, "FixupCallback_" + ((XmlTypeMapping)typeMap).XmlType);
			else
				return GetUniqueName ("fc", typeMap, "FixupCallback__Message");
		}
		
		string GetUniqueClassName (string s)
		{
			return classNames.AddUnique (CodeIdentifier.MakeValid (s), null);
		}
		
		string GetReadObjectCall (XmlTypeMapping typeMap, string isNullable, string checkType)
		{
			if (_format == SerializationFormat.Literal)
				return GetReadObjectName (typeMap) + " (" + isNullable + ", " + checkType + ")";
			else
				return GetCast (typeMap.TypeData, GetReadObjectName (typeMap) + " ()");
		}
		
		string GetFillListName (TypeData td)
		{
			if (!_listsToFill.Contains (td)) _listsToFill.Add (td);
			return GetUniqueName ("fl", td, "Fill_" + CodeIdentifier.MakeValid (td.CSharpName));
		}
		
		string GetCast (TypeData td, TypeData tdval, string val)
		{
			if (td.CSharpFullName == tdval.CSharpFullName) return val;
			else return GetCast (td, val);
		}

		string GetCast (TypeData td, string val)
		{
			return "((" + td.CSharpFullName + ") " + val + ")";
		}

		string GetCast (Type td, string val)
		{
			return "((" + ToCSharpFullName (td) + ") " + val + ")";
		}

		string GetTypeOf (TypeData td)
		{
			return "typeof(" + td.CSharpFullName + ")";
		}
		
		string GetTypeOf (Type td)
		{
			return "typeof(" + ToCSharpFullName (td) + ")";
		}
		
		string GetLiteral (object ob)
		{
			if (ob == null) return "null";
			if (ob is string) return "\"" + ob.ToString().Replace("\"","\"\"") + "\"";
			if (ob is DateTime) return "new DateTime (" + ((DateTime) ob).Ticks + ")";
#if NET_2_0
			if (ob is DateTimeOffset) return "new DateTimeOffset (" + ((DateTimeOffset) ob).Ticks + ")";
#endif
			if (ob is TimeSpan) return "new TimeSpan (" + ((TimeSpan) ob).Ticks + ")";
			if (ob is bool) return ((bool)ob) ? "true" : "false";
			if (ob is XmlQualifiedName) {
				XmlQualifiedName qn = (XmlQualifiedName)ob;
				return "new XmlQualifiedName (" + GetLiteral(qn.Name) + "," + GetLiteral(qn.Namespace) + ")";
			}
			if (ob is Enum) {
				string typeName = ToCSharpFullName (ob.GetType ());
				StringBuilder sb = new StringBuilder ();
				string namedValue = Enum.Format (ob.GetType (), ob, "g");
				string[] names = namedValue.Split (',');
				foreach (string name in names) {
					// individual named constants can be seperated by a comma
					// combined with some additional whitespace characters
					string cleanName = name.Trim ();
					if (cleanName.Length == 0)
						continue;

					if (sb.Length > 0)
						sb.Append (" | ");

					sb.Append (typeName);
					sb.Append ('.');
					sb.Append (cleanName);
				}
				return sb.ToString ();
			}

			return (ob is IFormattable) ? ((IFormattable) ob).ToString (null, CultureInfo.InvariantCulture) : ob.ToString ();
		}
		
		void WriteLineInd (string code)
		{
			WriteLine (code);
			_indent++;
		}
		
		void WriteLineUni (string code)
		{
			if (_indent > 0) _indent--;
			WriteLine (code);
		}

		void Write (string code)
		{
			if (code.Length > 0) 
				_writer.Write (new String ('\t', _indent));
			_writer.Write (code);
		}

		void WriteUni (string code)
		{
			if (_indent > 0) _indent--;
			_writer.Write (code);
			_writer.WriteLine (string.Empty);
		}

		void WriteLine (string code)
		{
			if (code.Length > 0)
				_writer.Write (new String ('\t',_indent));
			_writer.WriteLine (code);
		}
		
		void WriteMultilineCode (string code)
		{
			string tabs = new string ('\t',_indent);
			code = code.Replace ("\r","");
			code = code.Replace ("\t","");
			while (code.StartsWith ("\n")) code = code.Substring (1);
			while (code.EndsWith ("\n")) code = code.Substring (0, code.Length - 1);
			code = code.Replace ("\n", "\n" + tabs);
			WriteLine (code);
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
		
		void WriteMetCall (string method, params string[] pars)
		{
			WriteLine (method + " (" + Params (pars) + ");");
		}
		
		void Unindent ()
		{
			_indent--;
		}

		#endregion

	}
	
	internal class GenerationResult
	{
		public XmlMapping Mapping;
		public string ReaderClassName;
		public string ReadMethodName;
		public string WriterClassName;
		public string WriteMethodName;
		public string Namespace;
#if NET_2_0		
		public string SerializerClassName;
#endif		
		public string BaseSerializerClassName;
		public string ImplementationClassName;
	}
	
}
