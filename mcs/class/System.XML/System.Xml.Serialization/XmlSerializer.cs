//
// XmlSerializer.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
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
using System.Threading;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Text;
#if !TARGET_JVM && !MOBILE
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
#endif
using System.Configuration;
using System.Security.Policy;

namespace System.Xml.Serialization
{

	public class XmlSerializer
	{
		internal const string WsdlNamespace = "http://schemas.xmlsoap.org/wsdl/";
		internal const string EncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		internal const string WsdlTypesNamespace = "http://microsoft.com/wsdl/types/";
		static int generationThreshold;
		static bool backgroundGeneration = true;
		static bool deleteTempFiles = true;
		static bool generatorFallback = true;

		bool customSerializer;
		XmlMapping typeMapping;
		
		SerializerData serializerData;
		
		static Hashtable serializerTypes = new Hashtable ();
		
		internal class SerializerData
		{
			public int UsageCount;
			public Type ReaderType;
			public MethodInfo ReaderMethod;
			public Type WriterType;
			public MethodInfo WriterMethod;
			public GenerationBatch Batch;
			public XmlSerializerImplementation Implementation = null;
			
			public XmlSerializationReader CreateReader () {
				if (ReaderType != null)
					return (XmlSerializationReader) Activator.CreateInstance (ReaderType);
				else if (Implementation != null)
					return Implementation.Reader;
				else
					return null;
			}
			
			public XmlSerializationWriter CreateWriter () {
				if (WriterType != null)
					return (XmlSerializationWriter) Activator.CreateInstance (WriterType);
				else if (Implementation != null)
					return Implementation.Writer;
				else
					return null;
			}
		}
		
		internal class GenerationBatch
		{
			public bool Done;
			public XmlMapping[] Maps;
			public SerializerData[] Datas;
		}
		
		static XmlSerializer ()
		{
			// The following options are available:
			// MONO_XMLSERIALIZER_DEBUG: when set to something != "no", it will
			//       it will print the name of the generated file, and it won't
			//       be deleted.
			// MONO_XMLSERIALIZER_THS: The code generator threshold. It can be:
			//       no: does not use the generator, always the interpreter.
			//       0: always use the generator, wait until the generation is done.
			//       any number: use the interpreted serializer until the specified
			//       number of serializations is reached. At this point the generation
			//       of the serializer will start in the background. The interpreter
			//       will be used while the serializer is being generated.
			//
			//       XmlSerializer will fall back to the interpreted serializer if
			//       the code generation somehow fails. This can be avoided for
			//       debugging pourposes by adding the "nofallback" option.
			//       For example: MONO_XMLSERIALIZER_THS=0,nofallback
			
#if TARGET_JVM || MOBILE
			string db = null;
			string th = null;
			generationThreshold = -1;
			backgroundGeneration = false;
#else
			string db = Environment.GetEnvironmentVariable ("MONO_XMLSERIALIZER_DEBUG");
			string th = Environment.GetEnvironmentVariable ("MONO_XMLSERIALIZER_THS");
			
			if (th == null) {
				generationThreshold = 50;
				backgroundGeneration = true;
			} else {
				int i = th.IndexOf (',');
				if (i != -1) {
					if (th.Substring (i+1) == "nofallback")
						generatorFallback = false;
					th = th.Substring (0, i);
				}
				
				if (th.ToLower(CultureInfo.InvariantCulture) == "no") 
					generationThreshold = -1;
				else {
					generationThreshold = int.Parse (th, CultureInfo.InvariantCulture);
					backgroundGeneration = (generationThreshold != 0);
					if (generationThreshold < 1) generationThreshold = 1;
				}
			}
#endif
			deleteTempFiles = (db == null || db == "no");
#if !MOBILE
			IDictionary table = (IDictionary) ConfigurationSettings.GetConfig("system.diagnostics");
			if (table != null) 
			{
				table = (IDictionary) table["switches"];
				if (table != null) 
				{
					string val = (string) table ["XmlSerialization.Compilation"];
					if (val == "1") deleteTempFiles = false;
				}
			}
#endif
		}

#region Constructors

		protected XmlSerializer ()
		{
			customSerializer = true;
		}

		public XmlSerializer (Type type)
			: this (type, null, null, null, null)
		{
		}

		public XmlSerializer (XmlTypeMapping xmlTypeMapping)
		{
			typeMapping = xmlTypeMapping;
		}

		internal XmlSerializer (XmlMapping mapping, SerializerData data)
		{
			typeMapping = mapping;
			serializerData = data;
		}

		public XmlSerializer (Type type, string defaultNamespace)
			: this (type, null, null, null, defaultNamespace)
		{
		}

		public XmlSerializer (Type type, Type[] extraTypes)
			: this (type, null, extraTypes, null, null)
		{
		}

		public XmlSerializer (Type type, XmlAttributeOverrides overrides)
			: this (type, overrides, null, null, null)
		{
		}

		public XmlSerializer (Type type, XmlRootAttribute root)
			: this (type, null, null, root, null)
		{
		}

		public XmlSerializer (Type type,
			XmlAttributeOverrides overrides,
			Type [] extraTypes,
			XmlRootAttribute root,
			string defaultNamespace)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			XmlReflectionImporter importer = new XmlReflectionImporter (overrides, defaultNamespace);

			if (extraTypes != null) 
			{
				foreach (Type intype in extraTypes)
					importer.IncludeType (intype);
			}

			typeMapping = importer.ImportTypeMapping (type, root, defaultNamespace);
		}
		
		internal XmlMapping Mapping
		{
			get { return typeMapping; }
		}

#if NET_2_0

		[MonoTODO]
		public XmlSerializer (Type type,
			XmlAttributeOverrides overrides,
			Type [] extraTypes,
			XmlRootAttribute root,
			string defaultNamespace,
			string location,
			Evidence evidence)
		{
		}
#endif

#endregion // Constructors

#region Events

		private XmlAttributeEventHandler onUnknownAttribute;
		private XmlElementEventHandler onUnknownElement;
		private XmlNodeEventHandler onUnknownNode;
		private UnreferencedObjectEventHandler onUnreferencedObject;

		public event XmlAttributeEventHandler UnknownAttribute 
		{
			add { onUnknownAttribute += value; } remove { onUnknownAttribute -= value; }
		}

		public event XmlElementEventHandler UnknownElement 
		{
			add { onUnknownElement += value; } remove { onUnknownElement -= value; }
		}

		public event XmlNodeEventHandler UnknownNode 
		{
			add { onUnknownNode += value; } remove { onUnknownNode -= value; }
		}

		public event UnreferencedObjectEventHandler UnreferencedObject 
		{
			add { onUnreferencedObject += value; } remove { onUnreferencedObject -= value; }
		}


		internal virtual void OnUnknownAttribute (XmlAttributeEventArgs e) 
		{
			if (onUnknownAttribute != null) onUnknownAttribute(this, e);
		}

		internal virtual void OnUnknownElement (XmlElementEventArgs e) 
		{
			if (onUnknownElement != null) onUnknownElement(this, e);
		}

		internal virtual void OnUnknownNode (XmlNodeEventArgs e) 
		{
			if (onUnknownNode != null) onUnknownNode(this, e);
		}

		internal virtual void OnUnreferencedObject (UnreferencedObjectEventArgs e) 
		{
			if (onUnreferencedObject != null) onUnreferencedObject(this, e);
		}


#endregion // Events

#region Methods

		public virtual bool CanDeserialize (XmlReader xmlReader)
		{
			xmlReader.MoveToContent	();
			if (typeMapping is XmlMembersMapping) 
				return true;
			else
				return ((XmlTypeMapping)typeMapping).ElementName == xmlReader.LocalName;
		}

		protected virtual XmlSerializationReader CreateReader ()
		{
			// Must be implemented in derived class
			throw new NotImplementedException ();
		}

		protected virtual XmlSerializationWriter CreateWriter ()
		{
			// Must be implemented in derived class
			throw new NotImplementedException ();
		}

		public object Deserialize (Stream stream)
		{
			XmlTextReader xmlReader = new XmlTextReader(stream);
			xmlReader.Normalization = true;
			xmlReader.WhitespaceHandling = WhitespaceHandling.Significant;
			return Deserialize(xmlReader);
		}

		public object Deserialize (TextReader textReader)
		{
			XmlTextReader xmlReader = new XmlTextReader(textReader);
			xmlReader.Normalization = true;
			xmlReader.WhitespaceHandling = WhitespaceHandling.Significant;
			return Deserialize(xmlReader);
		}

		public object Deserialize (XmlReader xmlReader)
		{
			XmlSerializationReader xsReader;
			if (customSerializer)
				xsReader = CreateReader ();
			else
				xsReader = CreateReader (typeMapping);
				
			xsReader.Initialize (xmlReader, this);
			return Deserialize (xsReader);
		}

		protected virtual object Deserialize (XmlSerializationReader reader)
		{
			if (customSerializer)
				// Must be implemented in derived class
				throw new NotImplementedException ();
			
			try {
				if (reader is XmlSerializationReaderInterpreter)
					return ((XmlSerializationReaderInterpreter) reader).ReadRoot ();
				else
					return serializerData.ReaderMethod.Invoke (reader, null);
			} catch (Exception ex) {
				if (ex is InvalidOperationException || ex is InvalidCastException)
					throw new InvalidOperationException ("There is an error in"
						+ " XML document.", ex);
				throw;
			}
		}

		public static XmlSerializer [] FromMappings (XmlMapping	[] mappings)
		{
			XmlSerializer[] sers = new XmlSerializer [mappings.Length];
			SerializerData[] datas = new SerializerData [mappings.Length];
			GenerationBatch batch = new GenerationBatch ();
			batch.Maps = mappings;
			batch.Datas = datas;
			
			for (int n=0; n<mappings.Length; n++)
			{
				if (mappings[n] != null)
				{
					SerializerData data = new SerializerData ();
					data.Batch = batch;
					sers[n] = new XmlSerializer (mappings[n], data);
					datas[n] = data;
				}
			}
			
			return sers;
		}

		public static XmlSerializer [] FromTypes (Type [] mappings)
		{
			XmlSerializer [] sers = new XmlSerializer [mappings.Length];
			for (int n=0; n<mappings.Length; n++)
				sers[n] = new XmlSerializer (mappings[n]);
			return sers;
		}

		protected virtual void Serialize (object o, XmlSerializationWriter writer)
		{
			if (customSerializer)
				// Must be implemented in derived class
				throw new NotImplementedException ();
				
			if (writer is XmlSerializationWriterInterpreter)
				((XmlSerializationWriterInterpreter)writer).WriteRoot (o);
			else
				serializerData.WriterMethod.Invoke (writer, new object[] {o});
		}

		public void Serialize (Stream stream, object o)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (stream, System.Text.Encoding.Default);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize (xmlWriter, o, null);
		}

		public void Serialize (TextWriter textWriter, object o)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (textWriter);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize (xmlWriter, o, null);
		}

		public void Serialize (XmlWriter xmlWriter, object o)
		{
			Serialize (xmlWriter, o, null);
		}

		public void Serialize (Stream stream, object o, XmlSerializerNamespaces	namespaces)
		{
			XmlTextWriter xmlWriter	= new XmlTextWriter (stream, System.Text.Encoding.Default);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize (xmlWriter, o, namespaces);
		}

		public void Serialize (TextWriter textWriter, object o, XmlSerializerNamespaces	namespaces)
		{
			XmlTextWriter xmlWriter	= new XmlTextWriter (textWriter);
			xmlWriter.Formatting = Formatting.Indented;
			Serialize (xmlWriter, o, namespaces);
			xmlWriter.Flush();
		}

		public void Serialize (XmlWriter writer, object o, XmlSerializerNamespaces namespaces)
		{
			XmlSerializationWriter xsWriter;

			try {
				if (customSerializer)
					xsWriter = CreateWriter ();
				else
					xsWriter = CreateWriter (typeMapping);

				if (namespaces == null || namespaces.Count == 0) {
					namespaces = new XmlSerializerNamespaces ();
#if NET_2_0
					namespaces.Add ("xsi", XmlSchema.InstanceNamespace);
					namespaces.Add ("xsd", XmlSchema.Namespace);
#else
					namespaces.Add ("xsd", XmlSchema.Namespace);
					namespaces.Add ("xsi", XmlSchema.InstanceNamespace);
#endif
				}

				xsWriter.Initialize (writer, namespaces);
				Serialize (o, xsWriter);
				writer.Flush ();
			} catch (Exception ex) {
				if (ex is TargetInvocationException)
					ex = ex.InnerException;

				if (ex is InvalidOperationException || ex is InvalidCastException)
					throw new InvalidOperationException ("There was an error generating" +
						" the XML document.", ex);

				throw;
			}
		}
		
#if NET_2_0
		
		[MonoTODO]
		public object Deserialize (XmlReader xmlReader, string encodingStyle, XmlDeserializationEvents events)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object Deserialize (XmlReader xmlReader, string encodingStyle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object Deserialize (XmlReader xmlReader, XmlDeserializationEvents events)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static XmlSerializer[] FromMappings (XmlMapping[] mappings, Evidence evidence)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static XmlSerializer[] FromMappings (XmlMapping[] mappings, Type type)
		{
			throw new NotImplementedException ();
		}

#if !TARGET_JVM && !MOBILE
		public static Assembly GenerateSerializer (Type[] types, XmlMapping[] mappings)
		{
			return GenerateSerializer (types, mappings, null);
		}
		
		[MonoTODO]
		public static Assembly GenerateSerializer (Type[] types, XmlMapping[] mappings, CompilerParameters parameters)
		{
			GenerationBatch batch = new GenerationBatch ();
			batch.Maps = mappings;
			batch.Datas = new SerializerData [mappings.Length];
			
			for (int n=0; n<mappings.Length; n++) {
				SerializerData data = new SerializerData ();
				data.Batch = batch;
				batch.Datas [n] = data;
			}
			
			return GenerateSerializers (batch, parameters);
		}
#endif

		public static string GetXmlSerializerAssemblyName (Type type)
		{
			return type.Assembly.GetName().Name + ".XmlSerializers";
		}

		public static string GetXmlSerializerAssemblyName (Type type, string defaultNamespace)
		{
			return GetXmlSerializerAssemblyName (type) + "." + defaultNamespace.GetHashCode ();
		}
		
		[MonoTODO]
		public void Serialize (XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces, string encodingStyle)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported("")]
		public void Serialize (XmlWriter xmlWriter, Object o, XmlSerializerNamespaces namespaces, string encodingStyle, string id)
		{
			throw new NotImplementedException ();
		}
#endif
		
		XmlSerializationWriter CreateWriter (XmlMapping typeMapping)
		{
			XmlSerializationWriter writer;
			
			lock (this) {
				if (serializerData != null) {
					lock (serializerData) {
						writer = serializerData.CreateWriter ();
					}
					if (writer != null) return writer;
				}
			}
			
			if (!typeMapping.Source.CanBeGenerated || generationThreshold == -1)
				return new XmlSerializationWriterInterpreter (typeMapping);

			CheckGeneratedTypes (typeMapping);
			
			lock (this) {
				lock (serializerData) {
					writer = serializerData.CreateWriter ();
				}
				if (writer != null) return writer;
				if (!generatorFallback)
					throw new InvalidOperationException ("Error while generating serializer");
			}
			
			return new XmlSerializationWriterInterpreter (typeMapping);
		}
		
		XmlSerializationReader CreateReader (XmlMapping typeMapping)
		{
			XmlSerializationReader reader;
			
			lock (this) {
				if (serializerData != null) {
					lock (serializerData) {
						reader = serializerData.CreateReader ();
					}
					if (reader != null) return reader;
				}
			}
			
			if (!typeMapping.Source.CanBeGenerated || generationThreshold == -1)
				return new XmlSerializationReaderInterpreter (typeMapping);

			CheckGeneratedTypes (typeMapping);
			
			lock (this) {
				lock (serializerData) {
					reader = serializerData.CreateReader ();
				}
				if (reader != null) return reader;
				if (!generatorFallback)
					throw new InvalidOperationException ("Error while generating serializer");
			}
			
			return new XmlSerializationReaderInterpreter (typeMapping);
		}
		
#if TARGET_JVM || MOBILE
 		void CheckGeneratedTypes (XmlMapping typeMapping)
 		{
			throw new NotImplementedException();
		}
		void GenerateSerializersAsync (GenerationBatch batch)
		{
			throw new NotImplementedException();
		}
		void RunSerializerGeneration (object obj)
		{
			throw new NotImplementedException();
		}
#else
		void CheckGeneratedTypes (XmlMapping typeMapping)
		{
			lock (this)
			{
				if (serializerData == null) 
				{
					lock (serializerTypes)
					{
						serializerData = (SerializerData) serializerTypes [typeMapping.Source];
						if (serializerData == null) {
							serializerData = new SerializerData();
							serializerTypes [typeMapping.Source] = serializerData;
						}
					}
				}
			}
			
			bool generate = false;
			lock (serializerData)
			{
				generate = (++serializerData.UsageCount == generationThreshold);
			}
			
			if (generate)
			{
				if (serializerData.Batch != null)
					GenerateSerializersAsync (serializerData.Batch);
				else
				{
					GenerationBatch batch = new GenerationBatch ();
					batch.Maps = new XmlMapping[] {typeMapping};
					batch.Datas = new SerializerData[] {serializerData};
					GenerateSerializersAsync (batch);
				}
			}
		}
		
		void GenerateSerializersAsync (GenerationBatch batch)
		{
			if (batch.Maps.Length != batch.Datas.Length)
				throw new ArgumentException ("batch");

			lock (batch)
			{
				if (batch.Done) return;
				batch.Done = true;
			}
			
			if (backgroundGeneration)
				ThreadPool.QueueUserWorkItem (new WaitCallback (RunSerializerGeneration), batch);
			else
				RunSerializerGeneration (batch);
		}
		
		void RunSerializerGeneration (object obj)
		{
			try
			{
				GenerationBatch batch = (GenerationBatch) obj;
				batch = LoadFromSatelliteAssembly (batch);
				
				if (batch != null)
					GenerateSerializers (batch, null);
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex);
			}
		}
		
		static Assembly GenerateSerializers (GenerationBatch batch, CompilerParameters cp)
		{
			DateTime tim = DateTime.Now;
			
			XmlMapping[] maps = batch.Maps;
			
			if (cp == null) {
				cp = new CompilerParameters();
				cp.IncludeDebugInformation = false;
				cp.GenerateInMemory = true;
				cp.TempFiles.KeepFiles = !deleteTempFiles;
			}
			
			string file = cp.TempFiles.AddExtension ("cs");
			StreamWriter sw = new StreamWriter (file);
			
			if (!deleteTempFiles)
				Console.WriteLine ("Generating " + file);
			
			SerializationCodeGenerator gen = new SerializationCodeGenerator (maps);
			
			try
			{
				gen.GenerateSerializers (sw);
			}
			catch (Exception ex)
			{
				Console.WriteLine ("Serializer could not be generated");
				Console.WriteLine (ex);
				cp.TempFiles.Delete ();
				return null;
			}
			sw.Close ();
			
			CSharpCodeProvider provider = new CSharpCodeProvider();
			ICodeCompiler comp = provider.CreateCompiler ();
			
			cp.GenerateExecutable = false;
			
			foreach (Type rtype in gen.ReferencedTypes)
			{
				string path = new Uri (rtype.Assembly.CodeBase).LocalPath;
				if (!cp.ReferencedAssemblies.Contains (path))
					cp.ReferencedAssemblies.Add (path);
			}
				
			if (!cp.ReferencedAssemblies.Contains ("System.dll"))
				cp.ReferencedAssemblies.Add ("System.dll");
			if (!cp.ReferencedAssemblies.Contains ("System.Xml"))
				cp.ReferencedAssemblies.Add ("System.Xml");
			if (!cp.ReferencedAssemblies.Contains ("System.Data"))
				cp.ReferencedAssemblies.Add ("System.Data");
			
			CompilerResults res = comp.CompileAssemblyFromFile (cp, file);
			if (res.Errors.HasErrors || res.CompiledAssembly == null) {
				Console.WriteLine ("Error while compiling generated serializer");
				foreach (CompilerError error in res.Errors)
					Console.WriteLine (error);
					
				cp.TempFiles.Delete ();
				return null;
			}
			
			GenerationResult[] results = gen.GenerationResults;
			for (int n=0; n<results.Length; n++)
			{
				GenerationResult gres = results[n];
				SerializerData sd = batch.Datas [n];
				lock (sd)
				{
					sd.WriterType = res.CompiledAssembly.GetType (gres.Namespace + "." + gres.WriterClassName);
					sd.ReaderType = res.CompiledAssembly.GetType (gres.Namespace + "." + gres.ReaderClassName);
					sd.WriterMethod = sd.WriterType.GetMethod (gres.WriteMethodName);
					sd.ReaderMethod = sd.ReaderType.GetMethod (gres.ReadMethodName);
					sd.Batch = null;
				}
			}
			
			cp.TempFiles.Delete ();

			if (!deleteTempFiles)
				Console.WriteLine ("Generation finished - " + (DateTime.Now - tim).TotalMilliseconds + " ms");
				
			return res.CompiledAssembly;
		}
#endif
		
#if NET_2_0
		GenerationBatch LoadFromSatelliteAssembly (GenerationBatch batch)
		{
			return batch;
		}
#else
		GenerationBatch LoadFromSatelliteAssembly (GenerationBatch batch)
		{
			return batch;
		}
#endif
		
#endregion // Methods
	}
}
