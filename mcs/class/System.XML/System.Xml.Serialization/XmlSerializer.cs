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
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Configuration;

namespace System.Xml.Serialization
{

	public class XmlSerializer
	{
		internal const string WsdlNamespace = "http://schemas.xmlsoap.org/wsdl/";
		internal const string EncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		static int generationThreshold;
		static bool backgroundGeneration = true;
		static bool deleteTempFiles = true;

		bool customSerializer;
		XmlMapping typeMapping;
		
		SerializerData serializerData;
		Type writerType;
		Type readerType;
		
		static Hashtable serializerTypes = new Hashtable ();
		
		internal class SerializerData
		{
			public int UsageCount;
			public Type ReaderType;
			public MethodInfo ReaderMethod;
			public Type WriterType;
			public MethodInfo WriterMethod;
			public GenerationBatch Batch;
		}
		
		internal class GenerationBatch
		{
			public bool Done;
			public XmlMapping[] Maps;
			public SerializerData[] Datas;
		}
		
		static XmlSerializer ()
		{
			string db = Environment.GetEnvironmentVariable ("MONO_XMLSERIALIZER_DEBUG");
			deleteTempFiles = (db == null || db == "no");
			
			IDictionary table = (IDictionary) ConfigurationSettings.GetConfig("system.diagnostics");
			if (table != null) {
				table = (IDictionary) table["switches"];
				if (table != null) {
					string val = (string) table ["XmlSerialization.Compilation"];
					if (val == "1") deleteTempFiles = false;
				}
			}
			
			string th = Environment.GetEnvironmentVariable ("MONO_XMLSERIALIZER_THS");
			
			if (th == null) {
				generationThreshold = 50;
				backgroundGeneration = true;
			}
			else if (th.ToLower(CultureInfo.InvariantCulture) == "no") 
				generationThreshold = -1;
			else {
				generationThreshold = int.Parse (th, CultureInfo.InvariantCulture);
				backgroundGeneration = (generationThreshold != 0);
				if (generationThreshold < 1) generationThreshold = 1;
			}
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
			return Deserialize(xmlReader);
		}

		public object Deserialize (TextReader textReader)
		{
			XmlTextReader xmlReader = new XmlTextReader(textReader);
			xmlReader.Normalization = true;
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
			
			if (reader is XmlSerializationReaderInterpreter)
				return ((XmlSerializationReaderInterpreter)reader).ReadRoot ();
			else
				return serializerData.ReaderMethod.Invoke (reader, null);
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
			
			if (customSerializer)
				xsWriter = CreateWriter ();
			else
				xsWriter = CreateWriter (typeMapping);
				
			if (namespaces == null || namespaces.Count == 0)
			{
				namespaces = new XmlSerializerNamespaces ();
				namespaces.Add ("xsd", XmlSchema.Namespace);
				namespaces.Add ("xsi", XmlSchema.InstanceNamespace);
			}
			
			xsWriter.Initialize (writer, namespaces);
			Serialize (o, xsWriter);
			writer.Flush ();
		}
		
		XmlSerializationWriter CreateWriter (XmlMapping typeMapping)
		{
			lock (this) {
				if (serializerData != null && serializerData.WriterType != null)
					return (XmlSerializationWriter) Activator.CreateInstance (serializerData.WriterType);
			}
			
			if (!typeMapping.Source.CanBeGenerated || generationThreshold == -1)
				return new XmlSerializationWriterInterpreter (typeMapping);

			CheckGeneratedTypes (typeMapping);
			
			lock (this) {
				if (serializerData.WriterType != null)
					return (XmlSerializationWriter) Activator.CreateInstance (serializerData.WriterType);
			}
			
			return new XmlSerializationWriterInterpreter (typeMapping);
		}
		
		XmlSerializationReader CreateReader (XmlMapping typeMapping)
		{
			lock (this) {
				if (serializerData != null && serializerData.ReaderType != null)
					return (XmlSerializationReader) Activator.CreateInstance (serializerData.ReaderType);
			}
			
			if (!typeMapping.Source.CanBeGenerated || generationThreshold == -1)
				return new XmlSerializationReaderInterpreter (typeMapping);

			CheckGeneratedTypes (typeMapping);
			
			lock (this) {
				if (serializerData.ReaderType != null)
					return (XmlSerializationReader) Activator.CreateInstance (serializerData.ReaderType);
			}
			
			return new XmlSerializationReaderInterpreter (typeMapping);
		}
		
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
					GenerateSerializers (serializerData.Batch);
				else
				{
					GenerationBatch batch = new GenerationBatch ();
					batch.Maps = new XmlMapping[] {typeMapping};
					batch.Datas = new SerializerData[] {serializerData};
					GenerateSerializers (batch);
				}
			}
		}
		
		void GenerateSerializers (GenerationBatch batch)
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
				RunSerializerGenerationAux (obj);
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex);
			}
		}
		
		void RunSerializerGenerationAux (object obj)
		{
			DateTime tim = DateTime.Now;
			
			GenerationBatch batch = (GenerationBatch) obj;
			XmlMapping[] maps = batch.Maps;
			
			string file = Path.GetTempFileName ();
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
				
				if (deleteTempFiles)
					File.Delete (file);
				return;
			}
			sw.Close ();
			
			CSharpCodeProvider provider = new CSharpCodeProvider();
			ICodeCompiler comp = provider.CreateCompiler ();
			
			CompilerParameters cp = new CompilerParameters();
		    cp.GenerateExecutable = false;
			cp.IncludeDebugInformation = false;
		    cp.GenerateInMemory = true;
		    cp.ReferencedAssemblies.Add ("System.dll");
			cp.ReferencedAssemblies.Add ("System.Xml");
			cp.ReferencedAssemblies.Add ("System.Data");
			
			foreach (Type rtype in gen.ReferencedTypes)
			{
				if (!cp.ReferencedAssemblies.Contains (rtype.Assembly.Location))
					cp.ReferencedAssemblies.Add (rtype.Assembly.Location);
			}

			CompilerResults res = comp.CompileAssemblyFromFile (cp, file);
			if (res.Errors.Count > 0)
			{
				Console.WriteLine ("Error while compiling generated serializer");
				foreach (CompilerError error in res.Errors)
					Console.WriteLine (error);
					
				if (deleteTempFiles)
					File.Delete (file);
				return;
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
			
			if (deleteTempFiles)
				File.Delete (file);

			if (!deleteTempFiles)
				Console.WriteLine ("Generation finished - " + (DateTime.Now - tim).TotalMilliseconds + " ms");
		}
		
#endregion // Methods
	}
}
