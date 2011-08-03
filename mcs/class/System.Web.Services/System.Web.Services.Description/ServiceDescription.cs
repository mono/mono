// 
// System.Web.Services.Description.ServiceDescription.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
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
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Configuration;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

#if NET_2_0
using System.Collections.Generic;
#endif

namespace System.Web.Services.Description
{
	[XmlFormatExtensionPoint ("Extensions")]
	[XmlRoot ("definitions", Namespace = "http://schemas.xmlsoap.org/wsdl/")]
	public sealed class ServiceDescription :
#if NET_2_0
		NamedItem
#else
		DocumentableItem 
#endif
	{
		#region Fields

		public const string Namespace = "http://schemas.xmlsoap.org/wsdl/";

		BindingCollection bindings;
		ServiceDescriptionFormatExtensionCollection extensions;
		ImportCollection imports;
		MessageCollection messages;
#if !NET_2_0
		string name;
#endif
		PortTypeCollection portTypes;
		string retrievalUrl = String.Empty;
		ServiceDescriptionCollection serviceDescriptions;
		ServiceCollection services;
		string targetNamespace;
		Types types;
		static ServiceDescriptionSerializer serializer;
#if NET_2_0
		StringCollection validationWarnings;

		static XmlSchema schema;
#endif

		#endregion // Fields

		#region Constructors

		static ServiceDescription ()
		{
			serializer = new ServiceDescriptionSerializer ();
		}

		public ServiceDescription ()
		{
			bindings = new BindingCollection (this);
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
			imports = new ImportCollection (this);
			messages = new MessageCollection (this);
#if !NET_2_0
//			name = String.Empty;		
#endif
			portTypes = new PortTypeCollection (this);

			serviceDescriptions = null;
			services = new ServiceCollection (this);
			targetNamespace = null;
			types = new Types ();
		}
		
		#endregion // Constructors

		#region Properties

#if NET_2_0
		public static XmlSchema Schema {
			get {
				if (schema == null) {
					schema = XmlSchema.Read (typeof (ServiceDescription).Assembly.GetManifestResourceStream ("wsdl-1.1.xsd"), null);
				}
				return schema;
			}
		}
#endif

		[XmlElement ("import")]
		public ImportCollection Imports {
			get { return imports; }
		}

		[XmlElement ("types")]
		public Types Types {
			get { return types; }
			set { types = value; }
		}

		[XmlElement ("message")]
		public MessageCollection Messages {
			get { return messages; }
		}

		[XmlElement ("portType")]	
		public PortTypeCollection PortTypes {
			get { return portTypes; }
		}
	
		[XmlElement ("binding")]
		public BindingCollection Bindings {
			get { return bindings; }
		}

		[XmlIgnore]
		public 
#if NET_2_0
		override
#endif
		ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}

#if !NET_2_0
		[XmlAttribute ("name", DataType = "NMTOKEN")]	
		public string Name {
			get { return name; }
			set { name = value; }
		}
#endif

		[XmlIgnore]	
		public string RetrievalUrl {
			get { return retrievalUrl; }
			set { retrievalUrl = value; }
		}
	
		[XmlIgnore]	
		public static XmlSerializer Serializer {
			get { return serializer; }
		}

		[XmlIgnore]
		public ServiceDescriptionCollection ServiceDescriptions {
			get { 
				return serviceDescriptions; 
			}
		}

		[XmlElement ("service")]
		public ServiceCollection Services {
			get { return services; }
		}

		[XmlAttribute ("targetNamespace")]
		public string TargetNamespace {
			get { return targetNamespace; }
			set { targetNamespace = value; }
		}

#if NET_2_0
		[XmlIgnore]
		public StringCollection ValidationWarnings {
			get { return validationWarnings; }
		}
#endif

		#endregion // Properties

		#region Methods

		public static bool CanRead (XmlReader reader)
		{
			reader.MoveToContent ();
			return reader.LocalName == "definitions" && 
				reader.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/";
		}

#if NET_2_0
		public static ServiceDescription Read (string fileName, bool validate)
		{
			if (validate)
				using (XmlReader reader = XmlReader.Create (fileName)) {
					return Read (reader, true);
				}
			else
				return Read (fileName);
		}

		public static ServiceDescription Read (Stream stream, bool validate)
		{
			if (validate)
				return Read (XmlReader.Create (stream), true);
			else
				return Read (stream);
		}

		public static ServiceDescription Read (TextReader reader, bool validate)
		{
			if (validate)
				return Read (XmlReader.Create (reader), true);
			else
				return Read (reader);
		}

		public static ServiceDescription Read (XmlReader reader, bool validate)
		{
			if (validate) {
				StringCollection sc = new StringCollection ();
				XmlReaderSettings s = new XmlReaderSettings ();
				s.ValidationType = ValidationType.Schema;
				s.Schemas.Add (Schema);
				s.ValidationEventHandler += delegate (object o, ValidationEventArgs e) {
					sc.Add (e.Message);
				};

				ServiceDescription ret = Read (XmlReader.Create (reader, s));
				ret.validationWarnings = sc;
				return ret;
			}
			else
				return Read (reader);
		}
#endif

		public static ServiceDescription Read (Stream stream)
		{
			return (ServiceDescription) serializer.Deserialize (stream);
		}

		public static ServiceDescription Read (string fileName)
		{
			return Read (new FileStream (fileName, FileMode.Open, FileAccess.Read));
		}

		public static ServiceDescription Read (TextReader textReader)
		{
			return (ServiceDescription) serializer.Deserialize (textReader);
		}

		public static ServiceDescription Read (XmlReader reader)
		{
			return (ServiceDescription) serializer.Deserialize (reader);
		}

		public void Write (Stream stream)
		{
			serializer.Serialize (stream, this, GetNamespaceList ());
		}

		public void Write (string fileName)
		{
			Write (new FileStream (fileName, FileMode.Create));
		}

		public void Write (TextWriter writer)
		{
			serializer.Serialize (writer, this, GetNamespaceList ());
		}

		public void Write (XmlWriter writer)
		{
			serializer.Serialize (writer, this, GetNamespaceList ());
		}

		internal void SetParent (ServiceDescriptionCollection serviceDescriptions)
		{
			this.serviceDescriptions = serviceDescriptions; 
		}
		
		XmlSerializerNamespaces GetNamespaceList ()
		{
			XmlSerializerNamespaces ns;
			ns = new XmlSerializerNamespaces ();
			ns.Add ("soap", SoapBinding.Namespace);
#if NET_2_0
			ns.Add ("soap12", Soap12Binding.Namespace);
#endif
			ns.Add ("soapenc", "http://schemas.xmlsoap.org/soap/encoding/");
			ns.Add ("s", XmlSchema.Namespace);
			ns.Add ("http", HttpBinding.Namespace);
			ns.Add ("mime", MimeContentBinding.Namespace);
			ns.Add ("tm", MimeTextBinding.Namespace);
			ns.Add ("s0", TargetNamespace);
			
			AddExtensionNamespaces (ns, Extensions);
			
			if (Types != null) AddExtensionNamespaces (ns, Types.Extensions);
			
			foreach (Service ser in Services)
				foreach (Port port in ser.Ports)
					AddExtensionNamespaces (ns, port.Extensions);

			foreach (Binding bin in Bindings)
			{
				AddExtensionNamespaces (ns, bin.Extensions);
				foreach (OperationBinding op in bin.Operations)
				{
					AddExtensionNamespaces (ns, op.Extensions);
					if (op.Input != null) AddExtensionNamespaces (ns, op.Input.Extensions);
					if (op.Output != null) AddExtensionNamespaces (ns, op.Output.Extensions);
				}
			}
			return ns;
		}
		
		void AddExtensionNamespaces (XmlSerializerNamespaces ns, ServiceDescriptionFormatExtensionCollection extensions)
		{
			foreach (object o in extensions)
			{
				ServiceDescriptionFormatExtension ext = o as ServiceDescriptionFormatExtension;
				if (ext == null)
					// o can be XmlElement, skipping that
					continue;

				ExtensionInfo einf = ExtensionManager.GetFormatExtensionInfo (ext.GetType ());
				foreach (XmlQualifiedName qname in einf.NamespaceDeclarations)
					ns.Add (qname.Name, qname.Namespace);
			}
		}
		
		internal static void WriteExtensions (XmlWriter writer, object ob)
		{
			ServiceDescriptionFormatExtensionCollection extensions = ExtensionManager.GetExtensionPoint (ob);
			if (extensions != null)
			{
				foreach (object o in extensions) {
					if (o is ServiceDescriptionFormatExtension)
						WriteExtension (writer, (ServiceDescriptionFormatExtension)o);
					else if (o is XmlElement)
						((XmlElement)o).WriteTo (writer);
				}
			}
		}
		
		static void WriteExtension (XmlWriter writer, ServiceDescriptionFormatExtension ext)
		{
			Type type = ext.GetType ();
			ExtensionInfo info = ExtensionManager.GetFormatExtensionInfo (type);
			
//				if (prefix != null && prefix != "")
//					Writer.WriteStartElement (prefix, info.ElementName, info.Namespace);
//				else
//					WriteStartElement (info.ElementName, info.Namespace, false);

			XmlSerializerNamespaces ns = new XmlSerializerNamespaces ();
			ns.Add ("","");
			info.Serializer.Serialize (writer, ext, ns);
		}
		
		internal static void ReadExtension (XmlDocument doc, XmlReader reader, object ob)
		{
			ServiceDescriptionFormatExtensionCollection extensions = ExtensionManager.GetExtensionPoint (ob);
			if (extensions != null)
			{
				ExtensionInfo info = ExtensionManager.GetFormatExtensionInfo (reader.LocalName, reader.NamespaceURI);
				if (info != null)
				{
					object extension = info.Serializer.Deserialize (reader);
					extensions.Add ((ServiceDescriptionFormatExtension)extension);
					return;
				}
			}

			//No XmlFormatExtensionPoint attribute found

#if NET_2_0
			//Add to DocumentableItem.Extensions property
			DocumentableItem item = ob as DocumentableItem;
			if (item == null) {
				reader.Skip ();
				return;
			}

			item.Extensions.Add (doc.ReadNode (reader));
#else
			reader.Skip ();
#endif
		}

		#endregion

		internal class ServiceDescriptionSerializer : XmlSerializer 
		{
			protected override void Serialize (object o, XmlSerializationWriter writer)
			{
				ServiceDescriptionWriterBase xsWriter = writer as ServiceDescriptionWriterBase;
				xsWriter.WriteRoot_ServiceDescription (o);
			}
			
			protected override object Deserialize (XmlSerializationReader reader)
			{
				ServiceDescriptionReaderBase xsReader = reader as ServiceDescriptionReaderBase;
				return xsReader.ReadRoot_ServiceDescription ();
			}
			
			protected override XmlSerializationWriter CreateWriter ()
			{
				return new ServiceDescriptionWriterBase ();
			}
			
			protected override XmlSerializationReader CreateReader ()
			{
				return new ServiceDescriptionReaderBase ();
			}
		}		
	}
}
