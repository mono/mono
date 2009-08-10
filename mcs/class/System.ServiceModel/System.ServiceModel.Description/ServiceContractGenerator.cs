//
// ServiceContractGenerator.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Xml.Schema;
using System.Xml.Serialization;

using ConfigurationType = System.Configuration.Configuration;
using QName = System.Xml.XmlQualifiedName;
using OPair = System.Collections.Generic.KeyValuePair<System.ServiceModel.Description.IOperationContractGenerationExtension,System.ServiceModel.Description.OperationContractGenerationContext>;

namespace System.ServiceModel.Description
{
	public class ServiceContractGenerator
	{
		CodeCompileUnit ccu;
		ConfigurationType config;
		Collection<MetadataConversionError> errors
			= new Collection<MetadataConversionError> ();
		Dictionary<string,string> nsmappings
			= new Dictionary<string,string> ();
		Dictionary<ContractDescription,Type> referenced_types
			= new Dictionary<ContractDescription,Type> ();
		ServiceContractGenerationOptions options;
		Dictionary<QName, QName> imported_names = null;
		ServiceContractGenerationContext contract_context;
		List<OPair> operation_contexts = new List<OPair> ();

		public ServiceContractGenerator ()
			: this (null, null)
		{
		}

		public ServiceContractGenerator (CodeCompileUnit ccu)
			: this (ccu, null)
		{
		}

		public ServiceContractGenerator (ConfigurationType config)
			: this (null, config)
		{
		}

		public ServiceContractGenerator (CodeCompileUnit ccu, ConfigurationType config)
		{
			if (ccu == null)
				this.ccu = new CodeCompileUnit ();
			else
				this.ccu = ccu;
			this.config = config;
			Options |= ServiceContractGenerationOptions.ChannelInterface | 
				ServiceContractGenerationOptions.ClientClass;
		}

		public ConfigurationType Configuration {
			get { return config; }
		}

		public Collection<MetadataConversionError> Errors {
			get { return errors; }
		}

		public Dictionary<string,string> NamespaceMappings {
			get { return nsmappings; }
		}

		public ServiceContractGenerationOptions Options {
			get { return options; }
			set { options = value; }
		}

		bool GenerateAsync {
			get { return (options & ServiceContractGenerationOptions.AsynchronousMethods) != 0; }
		}

		public Dictionary<ContractDescription,Type> ReferencedTypes {
			get { return referenced_types; }
		}

		public CodeCompileUnit TargetCompileUnit {
			get { return ccu; }
		}

		[MonoTODO]
		public void GenerateBinding (Binding binding,
			out string bindingSectionName,
			out string configurationName)
		{
			throw new NotImplementedException ();
		}

		#region Service Contract Type

		// Those implementation classes are very likely to be split
		// into different classes.

		[MonoTODO]
		public CodeTypeReference GenerateServiceContractType (
			ContractDescription contractDescription)
		{
			CodeNamespace cns = GetNamespace (contractDescription.Namespace);
			imported_names = new Dictionary<QName, QName> ();
			var ret = ExportInterface (contractDescription, cns);

			// FIXME: handle duplex callback

			if ((Options & ServiceContractGenerationOptions.ChannelInterface) != 0)
				GenerateChannelInterface (contractDescription, cns);

			if ((Options & ServiceContractGenerationOptions.ClientClass) != 0)
				GenerateProxyClass (contractDescription, cns);

			// Process extensions. Class first, then methods.
			// (built-in ones must present before processing class extensions).
			foreach (var cb in contractDescription.Behaviors) {
				var gex = cb as IServiceContractGenerationExtension;
				if (gex != null)
					gex.GenerateContract (contract_context);
			}
			foreach (var opair in operation_contexts)
				opair.Key.GenerateOperation (opair.Value);

			return ret;
		}

		CodeNamespace GetNamespace (string ns)
		{
			if (ns == null)
				ns = String.Empty;
			foreach (CodeNamespace cns in ccu.Namespaces)
				if (cns.Name == ns)
					return cns;
			CodeNamespace ncns = new CodeNamespace ();
			//ncns.Name = ns;
			ccu.Namespaces.Add (ncns);
			return ncns;
		}

		CodeTypeDeclaration GetTypeDeclaration (CodeNamespace cns, string name)
		{
			foreach (CodeTypeDeclaration type in cns.Types)
				if (type.Name == name)
					return type;
			return null;
		}

		void GenerateProxyClass (ContractDescription cd, CodeNamespace cns)
		{
			string name = cd.Name + "Client";
			if (name [0] == 'I')
				name = name.Substring (1);
			CodeTypeDeclaration type = GetTypeDeclaration (cns, name);
			if (type != null)
				return; // already imported
			CodeTypeReference clientBase = new CodeTypeReference (typeof (ClientBase<>));
			clientBase.TypeArguments.Add (new CodeTypeReference (cd.Name));
			type = new CodeTypeDeclaration (name);
			cns.Types.Add (type);
			type.TypeAttributes = TypeAttributes.Public;
			type.BaseTypes.Add (clientBase);
			type.BaseTypes.Add (new CodeTypeReference (cd.Name));

			// .ctor()
			CodeConstructor ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.Public;
			type.Members.Add (ctor);

			// .ctor(string endpointConfigurationName)
			ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.Public;
			ctor.Parameters.Add (
				new CodeParameterDeclarationExpression (
					new CodeTypeReference (typeof (string)), "endpointConfigurationName"));
			ctor.BaseConstructorArgs.Add (
				new CodeArgumentReferenceExpression ("endpointConfigurationName"));
			type.Members.Add (ctor);

			// .ctor(string endpointConfigurationName, string remoteAddress)
			ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.Public;
			ctor.Parameters.Add (
				new CodeParameterDeclarationExpression (
					new CodeTypeReference (typeof (string)), "endpointConfigurationName"));
			ctor.Parameters.Add (
				new CodeParameterDeclarationExpression (
					new CodeTypeReference (typeof (string)), "remoteAddress"));
			ctor.BaseConstructorArgs.Add (
				new CodeArgumentReferenceExpression ("endpointConfigurationName"));
			ctor.BaseConstructorArgs.Add (
				new CodeArgumentReferenceExpression ("remoteAddress"));
			type.Members.Add (ctor);

			// .ctor(string endpointConfigurationName, EndpointAddress remoteAddress)
			ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.Public;
			ctor.Parameters.Add (
				new CodeParameterDeclarationExpression (
					new CodeTypeReference (typeof (string)), "endpointConfigurationName"));
			ctor.Parameters.Add (
				new CodeParameterDeclarationExpression (
					new CodeTypeReference (typeof (EndpointAddress)), "remoteAddress"));
			ctor.BaseConstructorArgs.Add (
				new CodeArgumentReferenceExpression ("endpointConfigurationName"));
			ctor.BaseConstructorArgs.Add (
				new CodeArgumentReferenceExpression ("remoteAddress"));
			type.Members.Add (ctor);

			// .ctor(Binding,EndpointAddress)
			ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.Public;
			ctor.Parameters.Add (
				new CodeParameterDeclarationExpression (
					new CodeTypeReference (typeof (Binding)), "binding"));
			ctor.Parameters.Add (
				new CodeParameterDeclarationExpression (
					new CodeTypeReference (typeof (EndpointAddress)), "endpoint"));
			ctor.BaseConstructorArgs.Add (
				new CodeArgumentReferenceExpression ("binding"));
			ctor.BaseConstructorArgs.Add (
				new CodeArgumentReferenceExpression ("endpoint"));
			type.Members.Add (ctor);

			// service contract methods
			AddImplementationClientMethods (type, cd);
		}

		void GenerateChannelInterface (ContractDescription cd, CodeNamespace cns)
		{
			string name = cd.Name + "Channel";
			CodeTypeDeclaration type = GetTypeDeclaration (cns, name);
			if (type != null)
				return;

			type = new CodeTypeDeclaration ();
			type.Name = name;
			type.TypeAttributes = TypeAttributes.Interface | TypeAttributes.Public;
			cns.Types.Add (type);
			
			type.BaseTypes.Add (ExportInterface (cd, cns));
			type.BaseTypes.Add (new CodeTypeReference (typeof (System.ServiceModel.IClientChannel)));
		}

		CodeTypeReference ExportInterface (ContractDescription cd, CodeNamespace cns)
		{
			CodeTypeDeclaration type = GetTypeDeclaration (cns, cd.Name);
			if (type != null)
				return new CodeTypeReference (type.Name);
			type = new CodeTypeDeclaration ();
			type.TypeAttributes = TypeAttributes.Interface;
			type.TypeAttributes |= TypeAttributes.Public;
			cns.Types.Add (type);
			type.Name = cd.Name;
			CodeAttributeDeclaration ad = 
				new CodeAttributeDeclaration (
					new CodeTypeReference (
						typeof (ServiceContractAttribute)));
			ad.Arguments.Add (new CodeAttributeArgument ("Namespace", new CodePrimitiveExpression (cd.Namespace)));
			type.CustomAttributes.Add (ad);
			contract_context = new ServiceContractGenerationContext (this, cd, type);

			AddOperationMethods (type, cd);

			return new CodeTypeReference (type.Name);
		}

		void AddBeginAsyncArgs (CodeMemberMethod cm)
		{
			var acb = new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (AsyncCallback)), "asyncCallback");
			cm.Parameters.Add (acb);
			var us = new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (object)), "userState");
			cm.Parameters.Add (us);
		}

		void AddOperationMethods (CodeTypeDeclaration type, ContractDescription cd)
		{
			foreach (OperationDescription od in cd.Operations) {
				CodeMemberMethod syncMethod = null, beginMethod = null, endMethod = null;

				CodeMemberMethod cm = new CodeMemberMethod ();
				type.Members.Add (cm);
				if (GenerateAsync) {
					cm.Name = "Begin" + od.Name;
					beginMethod = cm;
				} else {
					cm.Name = od.Name;
					syncMethod = cm;
				}
				CodeTypeReference returnTypeFromMessageContract = null;

				if (od.SyncMethod != null) {
					ExportParameters (cm, od.SyncMethod.GetParameters ());
					if (GenerateAsync) {
						AddBeginAsyncArgs (cm);
						cm.ReturnType = new CodeTypeReference (typeof (IAsyncResult));
					}
					else
						cm.ReturnType = new CodeTypeReference (od.SyncMethod.ReturnType);
				} else {
					ExportMessages (od.Messages, cm, false);
					returnTypeFromMessageContract = cm.ReturnType;
					if (GenerateAsync) {
						AddBeginAsyncArgs (cm);
						cm.ReturnType = new CodeTypeReference (typeof (IAsyncResult));
					}
				}

				// [OperationContract (Action = "...", ReplyAction = "..")]
				CodeAttributeDeclaration ad =
					new CodeAttributeDeclaration (
						new CodeTypeReference (
							typeof (OperationContractAttribute)));
				foreach (MessageDescription md in od.Messages) {
					if (md.Direction == MessageDirection.Input)
						ad.Arguments.Add (new CodeAttributeArgument ("Action", new CodePrimitiveExpression (md.Action)));
					else
						ad.Arguments.Add (new CodeAttributeArgument ("ReplyAction", new CodePrimitiveExpression (md.Action)));
				}
				if (GenerateAsync)
					ad.Arguments.Add (new CodeAttributeArgument ("AsyncPattern", new CodePrimitiveExpression (true)));
				cm.CustomAttributes.Add (ad);

				// For async mode, add EndXxx() too.
				if (GenerateAsync) {

					cm = new CodeMemberMethod ();
					type.Members.Add (cm);
					cm.Name = "End" + od.Name;
					endMethod = cm;

					var res = new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (IAsyncResult)), "result");
					cm.Parameters.Add (res);

					if (od.SyncMethod != null) // FIXME: it depends on sync method!
						cm.ReturnType = new CodeTypeReference (od.SyncMethod.ReturnType);
					else
						cm.ReturnType = returnTypeFromMessageContract;
				}

				foreach (var ob in od.Behaviors) {
					var gex = ob as IOperationContractGenerationExtension;
					if (gex != null)
						operation_contexts.Add (new OPair (gex, new OperationContractGenerationContext (this, contract_context, od, type, syncMethod, beginMethod, endMethod)));
				}
			}
		}

		void ExportParameters (CodeMemberMethod method, ParameterInfo [] parameters)
		{
			foreach (ParameterInfo pi in parameters)
				method.Parameters.Add (
					new CodeParameterDeclarationExpression (
						new CodeTypeReference (pi.ParameterType),
						pi.Name));
		}

		void AddImplementationClientMethods (CodeTypeDeclaration type, ContractDescription cd)
		{
			foreach (OperationDescription od in cd.Operations) {
				CodeMemberMethod cm = new CodeMemberMethod ();
				type.Members.Add (cm);
				if (GenerateAsync)
					cm.Name = "Begin" + od.Name;
				else
					cm.Name = od.Name;
				cm.Attributes = MemberAttributes.Public 
						| MemberAttributes.Final;
				CodeTypeReference returnTypeFromMessageContract = null;

				List<CodeExpression> args = new List<CodeExpression> ();
				if (od.SyncMethod != null) {
					ParameterInfo [] pars = od.SyncMethod.GetParameters ();
					ExportParameters (cm, pars);
					cm.ReturnType = new CodeTypeReference (od.SyncMethod.ReturnType);
					int i = 0;
					foreach (ParameterInfo pi in pars)
						args.Add (new CodeArgumentReferenceExpression (pi.Name));
				} else {
					args.AddRange (ExportMessages (od.Messages, cm, true));
					returnTypeFromMessageContract = cm.ReturnType;
					if (GenerateAsync) {
						AddBeginAsyncArgs (cm);
						cm.ReturnType = new CodeTypeReference (typeof (IAsyncResult));
					}
				}
				if (GenerateAsync) {
					args.Add (new CodeArgumentReferenceExpression ("asyncCallback"));
					args.Add (new CodeArgumentReferenceExpression ("userState"));
				}

				CodeExpression call = new CodeMethodInvokeExpression (
					new CodePropertyReferenceExpression (
						new CodeBaseReferenceExpression (),
						"Channel"),
					cm.Name,
					args.ToArray ());

				if (cm.ReturnType.BaseType == "System.Void")
					cm.Statements.Add (new CodeExpressionStatement (call));
				else
					cm.Statements.Add (new CodeMethodReturnStatement (call));

				// For async mode, add EndXxx() too.
				if (!GenerateAsync)
					return;

				// EndXxx() implementation

				cm = new CodeMemberMethod ();
				cm.Attributes = MemberAttributes.Public 
						| MemberAttributes.Final;
				type.Members.Add (cm);
				cm.Name = "End" + od.Name;

				var res = new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (IAsyncResult)), "result");
				cm.Parameters.Add (res);

				if (od.SyncMethod != null) // FIXME: it depends on sync method!
					cm.ReturnType = new CodeTypeReference (od.SyncMethod.ReturnType);
				else
					cm.ReturnType = returnTypeFromMessageContract;

				string resultArgName = "result";
				if (od.EndMethod != null)
					resultArgName = od.EndMethod.GetParameters () [0].Name;

				call = new CodeMethodInvokeExpression (
					new CodePropertyReferenceExpression (
						new CodeBaseReferenceExpression (),
						"Channel"),
					cm.Name,
					new CodeArgumentReferenceExpression (resultArgName));

				if (cm.ReturnType.BaseType == "System.Void")
					cm.Statements.Add (new CodeExpressionStatement (call));
				else
					cm.Statements.Add (new CodeMethodReturnStatement (call));
			}
		}

		private CodeExpression[] ExportMessages (MessageDescriptionCollection messages, CodeMemberMethod method, bool return_args)
		{
			CodeExpression [] args = null;
			foreach (MessageDescription md in messages) {
				if (md.Direction == MessageDirection.Output) {
					if (md.Body.ReturnValue != null) {
						ExportDataContract (md.Body.ReturnValue.XmlTypeMapping);	
						method.ReturnType = new CodeTypeReference (md.Body.ReturnValue.TypeName.Name);
					}
					continue;
				}

				if (return_args)
					args = new CodeExpression [md.Body.Parts.Count];

				MessagePartDescriptionCollection parts = md.Body.Parts;
				for (int i = 0; i < parts.Count; i++) {
					ExportDataContract (parts [i].XmlTypeMapping);	

					method.Parameters.Add (
						new CodeParameterDeclarationExpression (
							new CodeTypeReference (parts [i].TypeName.Name),
							parts [i].Name));

					if (return_args)
						args [i] = new CodeArgumentReferenceExpression (parts [i].Name);
				}
			}

			return args;
		}

		#endregion

		[MonoTODO]
		public CodeTypeReference GenerateServiceEndpoint (
			ServiceEndpoint endpoint,
			out ChannelEndpointElement channelElement)
		{
			throw new NotImplementedException ();
		}

		private void ExportDataContract (XmlTypeMapping mapping)
		{
			if (mapping == null)
				return;

			QName qname = new QName (mapping.TypeName, mapping.Namespace);
			if (imported_names.ContainsKey (qname))
				return;

			CodeNamespace cns = new CodeNamespace ();

			XmlCodeExporter xce = new XmlCodeExporter (cns);
			xce.ExportTypeMapping (mapping);

			List <CodeTypeDeclaration> to_remove = new List <CodeTypeDeclaration> ();
			
			//Process the types just generated
			//FIXME: Iterate and assign the types to correct namespaces
			//At the end, add all those namespaces to the ccu
			foreach (CodeTypeDeclaration type in cns.Types) {
				string ns = GetXmlNamespace (type);
				if (ns == null)
					//FIXME: do what here?
					continue;

				QName type_name = new QName (type.Name, ns);
				if (imported_names.ContainsKey (type_name)) {
					//Type got reemitted, so remove it!
					to_remove.Add (type);
					continue;
				}

				imported_names [type_name] = type_name;

				type.Comments.Clear ();
				//Custom Attributes
				type.CustomAttributes.Clear ();

				if (type.IsEnum)
					continue;
	
				type.CustomAttributes.Add (
					new CodeAttributeDeclaration (
						new CodeTypeReference ("System.CodeDom.Compiler.GeneratedCodeAttribute"),
						new CodeAttributeArgument (new CodePrimitiveExpression ("System.Runtime.Serialization")),
						new CodeAttributeArgument (new CodePrimitiveExpression ("3.0.0.0"))));
			
				type.CustomAttributes.Add (
					new CodeAttributeDeclaration (
						new CodeTypeReference ("System.Runtime.Serialization.DataContractAttribute")));

				//BaseType and interface
				type.BaseTypes.Add (new CodeTypeReference (typeof (object)));
				type.BaseTypes.Add (new CodeTypeReference ("System.Runtime.Serialization.IExtensibleDataObject"));

				foreach (CodeTypeMember mbr in type.Members) {
					CodeMemberProperty p = mbr as CodeMemberProperty;
					if (p == null)
						continue;

					if ((p.Attributes & MemberAttributes.Public) == MemberAttributes.Public) {
						//FIXME: Clear all attributes or only XmlElementAttribute?
						p.CustomAttributes.Clear ();
						p.CustomAttributes.Add (new CodeAttributeDeclaration (
							new CodeTypeReference ("System.Runtime.Serialization.DataMemberAttribute")));

						p.Comments.Clear ();
					}
				}

				//Fields
				CodeMemberField field = new CodeMemberField (
					new CodeTypeReference ("System.Runtime.Serialization.ExtensionDataObject"),
					"extensionDataField");
				field.Attributes = MemberAttributes.Private | MemberAttributes.Final;
				type.Members.Add (field);

				//Property 
				CodeMemberProperty prop = new CodeMemberProperty ();
				prop.Type = new CodeTypeReference ("System.Runtime.Serialization.ExtensionDataObject");
				prop.Name = "ExtensionData";
				prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;

				//Get
				prop.GetStatements.Add (new CodeMethodReturnStatement (
					new CodeFieldReferenceExpression (
					new CodeThisReferenceExpression (),
					"extensionDataField")));

				//Set
				prop.SetStatements.Add (new CodeAssignStatement (
					new CodeFieldReferenceExpression (
					new CodeThisReferenceExpression (),
					"extensionDataField"),
					new CodePropertySetValueReferenceExpression ()));

				type.Members.Add (prop);
			}

			foreach (CodeTypeDeclaration type in to_remove)
				cns.Types.Remove (type);

			ccu.Namespaces.Add (cns);
		}
		
		private string GetXmlNamespace (CodeTypeDeclaration type)
		{
			foreach (CodeAttributeDeclaration attr in type.CustomAttributes) {
				if (attr.Name == "System.Xml.Serialization.XmlTypeAttribute" ||
					attr.Name == "System.Xml.Serialization.XmlRootAttribute") {

					foreach (CodeAttributeArgument arg in attr.Arguments)
						if (arg.Name == "Namespace")
							return ((CodePrimitiveExpression)arg.Value).Value as string;

					//Could not find Namespace arg!
					return null;	
				}
			}
			
			return null;
		}


	}
}
