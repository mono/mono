// 
// System.Web.Services.Description.SoapProtocolReflector.cs
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

using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;

namespace System.Web.Services.Description {

	internal abstract class SoapProtocolReflector : ProtocolReflector 
	{
		#region Fields

		SoapBinding soapBinding;

		#endregion // Fields

		#region Constructors

		public SoapProtocolReflector ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public abstract SoapExtensionReflector ExtensionReflector { get; }

		#endregion

		#region Methods

		protected override void BeginClass ()
		{
			ExtensionReflector.ReflectDescription ();
		}

		protected override void EndClass ()
		{
		}

		protected override bool ReflectMethod ()
		{
			SoapMethodStubInfo method = (SoapMethodStubInfo) MethodStubInfo;
			bool existing = false;
#if NET_2_0
			if (Parent != null) {
				if (Parent.MappedMessagesIn.ContainsKey (method.MethodInfo))
					existing = true;
				else {
					Parent.MappedMessagesIn [method.MethodInfo] = InputMessage;
					Parent.MappedMessagesOut [method.MethodInfo] = OutputMessage;
				}
			}
#endif
			if (!existing)
				ImportMessageParts ();
			ExtensionReflector.ReflectMethod ();

			return !existing;
		}
		
		void ImportMessageParts ()
		{
			SoapMethodStubInfo method = (SoapMethodStubInfo) MethodStubInfo;
			ImportMessage (method.InputMembersMapping, InputMessage);
			ImportMessage (method.OutputMembersMapping, OutputMessage);
				

			foreach (SoapHeaderMapping hf in method.Headers)
			{
				if (hf.Custom) continue;
				
				Message msg = new Message ();
				msg.Name = Operation.Name + hf.HeaderType.Name;
				MessagePart part = new MessagePart ();
				part.Name = hf.HeaderType.Name;
				msg.Parts.Add (part);
				ServiceDescription.Messages.Add (msg);

				if (method.Use == SoapBindingUse.Literal)
				{
					// MS.NET reflects header classes in a weird way. The root element
					// name is the CLR class name unless it is specified in an XmlRootAttribute.
					// The usual is to use the xml type name by default, but not in this case.
				
					XmlRootAttribute root;
					XmlAttributes ats = new XmlAttributes (hf.HeaderType);
					if (ats.XmlRoot != null) root = ats.XmlRoot;
					else root = new XmlRootAttribute (hf.HeaderType.Name);
					
					if (root.Namespace == null) root.Namespace = TypeInfo.LogicalType.GetWebServiceLiteralNamespace (ServiceDescription.TargetNamespace);
					if (root.ElementName == null) root.ElementName = hf.HeaderType.Name;
					
					XmlTypeMapping mapping = ReflectionImporter.ImportTypeMapping (hf.HeaderType, root);
					part.Element = new XmlQualifiedName (mapping.ElementName, mapping.Namespace);
					SchemaExporter.ExportTypeMapping (mapping);
				}
				else
				{
					XmlTypeMapping mapping = SoapReflectionImporter.ImportTypeMapping (hf.HeaderType, TypeInfo.LogicalType.GetWebServiceEncodedNamespace (ServiceDescription.TargetNamespace));
					part.Type = new XmlQualifiedName (mapping.ElementName, mapping.Namespace);
					SoapSchemaExporter.ExportTypeMapping (mapping);
				}
			}
		}
		
		void ImportMessage (XmlMembersMapping members, Message msg)
		{
			SoapMethodStubInfo method = (SoapMethodStubInfo) MethodStubInfo;
			bool needsEnclosingElement = (method.ParameterStyle == SoapParameterStyle.Wrapped && 
											method.SoapBindingStyle == SoapBindingStyle.Document);

			if (needsEnclosingElement)
			{
				MessagePart part = new MessagePart ();
				part.Name = "parameters";
				XmlQualifiedName qname = new XmlQualifiedName (members.ElementName, members.Namespace);
				if (method.Use == SoapBindingUse.Literal) part.Element = qname;
				else part.Type = qname;
				msg.Parts.Add (part);
			}
			else
			{
				for (int n=0; n<members.Count; n++)
				{
					MessagePart part = new MessagePart ();
					part.Name = members[n].MemberName;
					
					if (method.Use == SoapBindingUse.Literal) {
						if (members[n].Any)
							part.Type = new XmlQualifiedName ("any", members[n].Namespace);
						else
							part.Element = new XmlQualifiedName (members[n].ElementName, members[n].Namespace);
					}
					else {
						string namesp = members[n].TypeNamespace;
						if (namesp == "") namesp = members[n].Namespace;
						part.Name = members[n].ElementName;
						part.Type = new XmlQualifiedName (members[n].TypeName, namesp);
					}
					msg.Parts.Add (part);
				}
			}
			
			
			if (method.Use == SoapBindingUse.Literal)
				SchemaExporter.ExportMembersMapping (members);
			else
				SoapSchemaExporter.ExportMembersMapping (members, needsEnclosingElement);
		}

		protected override string ReflectMethodBinding ()
		{
			return ((SoapMethodStubInfo)MethodStubInfo).Binding;
		}

		#endregion
	}

	internal class Soap11ProtocolReflector : SoapProtocolReflector
	{
		SoapExtensionReflector reflector;
		
		public Soap11ProtocolReflector ()
		{
			reflector = new Soap11BindingExtensionReflector ();
			reflector.ReflectionContext = this;
		}

		public override string ProtocolName {
			get { return "Soap"; }
		}

		public override SoapExtensionReflector ExtensionReflector {
			get { return reflector; }
		}
	}

#if NET_2_0
	internal class Soap12ProtocolReflector : SoapProtocolReflector
	{
		SoapExtensionReflector reflector;
		
		public Soap12ProtocolReflector ()
		{
			reflector = new Soap12BindingExtensionReflector ();
			reflector.ReflectionContext = this;
		}

		public override string ProtocolName {
			get { return "Soap12"; }
		}

		public override SoapExtensionReflector ExtensionReflector {
			get { return reflector; }
		}
	}
#endif
}
