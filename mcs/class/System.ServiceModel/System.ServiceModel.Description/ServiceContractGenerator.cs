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
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Threading;
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
		CodeIdentifiers identifiers = new CodeIdentifiers ();
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

		XsdDataContractImporter data_contract_importer;
		XmlSerializerMessageContractImporterInternal xml_serialization_importer;

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
			get { return GenerateEventBasedAsync || (options & ServiceContractGenerationOptions.AsynchronousMethods) != 0; }
		}

		bool GenerateEventBasedAsync {
			get { return (options & ServiceContractGenerationOptions.EventBasedAsynchronousMethods) != 0; }
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

			if (data_contract_importer != null)
				MergeCompileUnit (data_contract_importer.CodeCompileUnit, ccu);
			if (xml_serialization_importer != null)
				MergeCompileUnit (xml_serialization_importer.CodeCompileUnit, ccu);

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
			name = identifiers.AddUnique (name, null);
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

			if (GenerateEventBasedAsync)
				foreach (var od in cd.Operations)
					GenerateEventBasedAsyncSupport (type, od, cns);
		}

		void GenerateChannelInterface (ContractDescription cd, CodeNamespace cns)
		{
			string name = cd.Name + "Channel";
			name = identifiers.AddUnique (name, null);
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
			type.Name = identifiers.AddUnique (cd.Name, null);
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

				CodeTypeReference returnTypeFromMessageContract = null;
				syncMethod = GenerateOperationMethod (type, cd, od, false, out returnTypeFromMessageContract);
				type.Members.Add (syncMethod);

				if (GenerateAsync) {
					beginMethod = GenerateOperationMethod (type, cd, od, true, out returnTypeFromMessageContract);
					type.Members.Add (beginMethod);

					var cm = new CodeMemberMethod ();
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

		CodeMemberMethod GenerateOperationMethod (CodeTypeDeclaration type, ContractDescription cd, OperationDescription od, bool async, out CodeTypeReference returnType)
		{
			CodeMemberMethod cm = new CodeMemberMethod ();

			if (od.Behaviors.Find<XmlSerializerMappingBehavior> () != null)
				cm.CustomAttributes.Add (new CodeAttributeDeclaration (new CodeTypeReference (typeof (XmlSerializerFormatAttribute))));

			if (async)
				cm.Name = "Begin" + od.Name;
			else
				cm.Name = od.Name;

			if (od.SyncMethod != null) {
				ExportParameters (cm, od.SyncMethod.GetParameters ());
				if (async) {
					AddBeginAsyncArgs (cm);
					cm.ReturnType = new CodeTypeReference (typeof (IAsyncResult));
				}
				else
					cm.ReturnType = new CodeTypeReference (od.SyncMethod.ReturnType);
				returnType = new CodeTypeReference (od.SyncMethod.ReturnType);
			} else {
				ExportMessages (od.Messages, cm, false);
				returnType = cm.ReturnType;
				if (async) {
					AddBeginAsyncArgs (cm);
					cm.ReturnType = new CodeTypeReference (typeof (IAsyncResult));
				}
			}

			// [OperationContract (Action = "...", ReplyAction = "..")]
			var ad = new CodeAttributeDeclaration (new CodeTypeReference (typeof (OperationContractAttribute)));
			foreach (MessageDescription md in od.Messages) {
				if (md.Direction == MessageDirection.Input)
					ad.Arguments.Add (new CodeAttributeArgument ("Action", new CodePrimitiveExpression (md.Action)));
				else
					ad.Arguments.Add (new CodeAttributeArgument ("ReplyAction", new CodePrimitiveExpression (md.Action)));
			}
			if (async)
				ad.Arguments.Add (new CodeAttributeArgument ("AsyncPattern", new CodePrimitiveExpression (true)));
			cm.CustomAttributes.Add (ad);

			return cm;
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
				CodeMemberMethod cm;
				CodeTypeReference returnTypeFromMessageContract = null;
				cm = GenerateImplementationClientMethod (type, cd, od, false, out returnTypeFromMessageContract);
				type.Members.Add (cm);

				if (!GenerateAsync)
					continue;

				cm = GenerateImplementationClientMethod (type, cd, od, true, out returnTypeFromMessageContract);
				type.Members.Add (cm);

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

				var call = new CodeMethodInvokeExpression (
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

		CodeMemberMethod GenerateImplementationClientMethod (CodeTypeDeclaration type, ContractDescription cd, OperationDescription od, bool async, out CodeTypeReference returnTypeFromMessageContract)
		{
			CodeMemberMethod cm = new CodeMemberMethod ();
			if (async)
				cm.Name = "Begin" + od.Name;
			else
				cm.Name = od.Name;
			cm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			returnTypeFromMessageContract = null;

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
				if (async) {
					AddBeginAsyncArgs (cm);
					cm.ReturnType = new CodeTypeReference (typeof (IAsyncResult));
				}
			}
			if (async) {
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
			return cm;
		}

		CodeMemberMethod FindByName (CodeTypeDeclaration type, string name)
		{
			foreach (var m in type.Members) {
				var method = m as CodeMemberMethod;
				if (method != null && method.Name == name)
					return method;
			}
			return null;
		}

		void GenerateEventBasedAsyncSupport (CodeTypeDeclaration type, OperationDescription od, CodeNamespace cns)
		{
			var method = FindByName (type, od.Name) ?? FindByName (type, "Begin" + od.Name);
			var endMethod = method.Name == od.Name ? null : FindByName (type, "End" + od.Name);
			bool methodAsync = method.Name.StartsWith ("Begin", StringComparison.Ordinal);
			var resultType = endMethod != null ? endMethod.ReturnType : method.ReturnType;

			var thisExpr = new CodeThisReferenceExpression ();
			var baseExpr = new CodeBaseReferenceExpression ();
			var nullExpr = new CodePrimitiveExpression (null);
			var asyncResultType = new CodeTypeReference (typeof (IAsyncResult));

			// OnBeginXxx() implementation
			var cm = new CodeMemberMethod () {
				Name = "OnBegin" + od.Name,
				Attributes = MemberAttributes.Private | MemberAttributes.Final,
				ReturnType = asyncResultType
				};
			type.Members.Add (cm);

			AddMethodParam (cm, typeof (object []), "args");
			AddMethodParam (cm, typeof (AsyncCallback), "asyncCallback");
			AddMethodParam (cm, typeof (object), "userState");

			var call = new CodeMethodInvokeExpression (
				thisExpr,
				"Begin" + od.Name);
			for (int idx = 0; idx < method.Parameters.Count - (methodAsync ? 2 : 0); idx++) {
				var p = method.Parameters [idx];
				cm.Statements.Add (new CodeVariableDeclarationStatement (p.Type, p.Name, new CodeCastExpression (p.Type, new CodeArrayIndexerExpression (new CodeArgumentReferenceExpression ("args"), new CodePrimitiveExpression (idx)))));
				call.Parameters.Add (new CodeVariableReferenceExpression (p.Name));
			}
			call.Parameters.Add (new CodeArgumentReferenceExpression ("asyncCallback"));
			call.Parameters.Add (new CodeArgumentReferenceExpression ("userState"));
			cm.Statements.Add (new CodeMethodReturnStatement (call));

			// OnEndXxx() implementation
			cm = new CodeMemberMethod () {
				Name = "OnEnd" + od.Name,
				Attributes = MemberAttributes.Private | MemberAttributes.Final,
				ReturnType = new CodeTypeReference (typeof (object [])) };
			type.Members.Add (cm);

			AddMethodParam (cm, typeof (IAsyncResult), "result");

			var outArgRefs = new List<CodeVariableReferenceExpression> ();

			for (int idx = 0; idx < method.Parameters.Count; idx++) {
				var p = method.Parameters [idx];
				if (p.Direction != FieldDirection.In) {
					cm.Statements.Add (new CodeVariableDeclarationStatement (p.Type, p.Name));
					outArgRefs.Add (new CodeVariableReferenceExpression (p.Name)); // FIXME: should this work? They need "out" or "ref" modifiers.
				}
			}

			call = new CodeMethodInvokeExpression (
				thisExpr,
				"End" + od.Name,
				new CodeArgumentReferenceExpression ("result"));
			call.Parameters.AddRange (outArgRefs.Cast<CodeExpression> ().ToArray ()); // questionable

			var retCreate = new CodeArrayCreateExpression (typeof (object));
			if (resultType.BaseType == "System.Void")
				cm.Statements.Add (call);
			else {
				cm.Statements.Add (new CodeVariableDeclarationStatement (typeof (object), "__ret", call));
				retCreate.Initializers.Add (new CodeVariableReferenceExpression ("__ret"));
			}
			foreach (var outArgRef in outArgRefs)
				retCreate.Initializers.Add (new CodeVariableReferenceExpression (outArgRef.VariableName));

			cm.Statements.Add (new CodeMethodReturnStatement (retCreate));

			// OnXxxCompleted() implementation
			cm = new CodeMemberMethod () {
				Name = "On" + od.Name + "Completed",
				Attributes = MemberAttributes.Private | MemberAttributes.Final };
			type.Members.Add (cm);

			AddMethodParam (cm, typeof (object), "state");

			string argsname = identifiers.AddUnique (od.Name + "CompletedEventArgs", null);
			var iaargs = new CodeTypeReference ("InvokeAsyncCompletedEventArgs"); // avoid messy System.Type instance for generic nested type :|
			var iaref = new CodeVariableReferenceExpression ("args");
			var methodEventArgs = new CodeObjectCreateExpression (new CodeTypeReference (argsname),
				new CodePropertyReferenceExpression (iaref, "Results"),
				new CodePropertyReferenceExpression (iaref, "Error"),
				new CodePropertyReferenceExpression (iaref, "Cancelled"),
				new CodePropertyReferenceExpression (iaref, "UserState"));
			cm.Statements.Add (new CodeConditionStatement (
				new CodeBinaryOperatorExpression (
					new CodeEventReferenceExpression (thisExpr, od.Name + "Completed"), CodeBinaryOperatorType.IdentityInequality, nullExpr),
				new CodeVariableDeclarationStatement (iaargs, "args", new CodeCastExpression (iaargs, new CodeArgumentReferenceExpression ("state"))),
				new CodeExpressionStatement (new CodeMethodInvokeExpression (thisExpr, od.Name + "Completed", thisExpr, methodEventArgs))));

			// delegate fields
			type.Members.Add (new CodeMemberField (new CodeTypeReference ("BeginOperationDelegate"), "onBegin" + od.Name + "Delegate"));
			type.Members.Add (new CodeMemberField (new CodeTypeReference ("EndOperationDelegate"), "onEnd" + od.Name + "Delegate"));
			type.Members.Add (new CodeMemberField (new CodeTypeReference (typeof (SendOrPostCallback)), "on" + od.Name + "CompletedDelegate"));

			// XxxCompletedEventArgs class
			var argsType = new CodeTypeDeclaration (argsname);
			argsType.BaseTypes.Add (new CodeTypeReference (typeof (AsyncCompletedEventArgs)));
			cns.Types.Add (argsType);

			var argsCtor = new CodeConstructor () {
				Attributes = MemberAttributes.Public | MemberAttributes.Final };
			argsCtor.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object []), "results"));
			argsCtor.Parameters.Add (new CodeParameterDeclarationExpression (typeof (Exception), "error"));
			argsCtor.Parameters.Add (new CodeParameterDeclarationExpression (typeof (bool), "cancelled"));
			argsCtor.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object), "userState"));
			argsCtor.BaseConstructorArgs.Add (new CodeArgumentReferenceExpression ("error"));
			argsCtor.BaseConstructorArgs.Add (new CodeArgumentReferenceExpression ("cancelled"));
			argsCtor.BaseConstructorArgs.Add (new CodeArgumentReferenceExpression ("userState"));
			var resultsField = new CodeFieldReferenceExpression (thisExpr, "results");
			argsCtor.Statements.Add (new CodeAssignStatement (resultsField, new CodeArgumentReferenceExpression ("results")));
			argsType.Members.Add (argsCtor);

			argsType.Members.Add (new CodeMemberField (typeof (object []), "results"));

			if (resultType.BaseType != "System.Void") {
				var resultProp = new CodeMemberProperty {
					Name = "Result",
					Type = resultType,
					Attributes = MemberAttributes.Public | MemberAttributes.Final };
				resultProp.GetStatements.Add (new CodeMethodReturnStatement (new CodeCastExpression (resultProp.Type, new CodeArrayIndexerExpression (resultsField, new CodePrimitiveExpression (0)))));
				argsType.Members.Add (resultProp);
			}

			// event field
			var handlerType = new CodeTypeReference (typeof (EventHandler<>));
			handlerType.TypeArguments.Add (new CodeTypeReference (argsType.Name));
			type.Members.Add (new CodeMemberEvent () {
				Name = od.Name + "Completed",
				Type = handlerType,
				Attributes = MemberAttributes.Public | MemberAttributes.Final });

			// XxxAsync() implementations
			bool hasAsync = false;
			foreach (int __x in Enumerable.Range (0, 2)) {
				cm = new CodeMemberMethod ();
				type.Members.Add (cm);
				cm.Name = od.Name + "Async";
				cm.Attributes = MemberAttributes.Public 
						| MemberAttributes.Final;

				var inArgs = new List<CodeParameterDeclarationExpression > ();

				for (int idx = 0; idx < method.Parameters.Count - (methodAsync ? 2 : 0); idx++) {
					var pd = method.Parameters [idx];
					inArgs.Add (pd);
					cm.Parameters.Add (pd);
				}

				// First one is overload without asyncState arg.
				if (!hasAsync) {
					call = new CodeMethodInvokeExpression (thisExpr, cm.Name, inArgs.ConvertAll<CodeExpression> (decl => new CodeArgumentReferenceExpression (decl.Name)).ToArray ());
					call.Parameters.Add (nullExpr);
					cm.Statements.Add (new CodeExpressionStatement (call));
					hasAsync = true;
					continue;
				}

				// Second one is the primary one.

				cm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object), "userState"));

				// if (onBeginBarOperDelegate == null) onBeginBarOperDelegate = new BeginOperationDelegate (OnBeginBarOper);
				// if (onEndBarOperDelegate == null) onEndBarOperDelegate = new EndOperationDelegate (OnEndBarOper);
				// if (onBarOperCompletedDelegate == null) onBarOperCompletedDelegate = new BeginOperationDelegate (OnBarOperCompleted);
				var beginOperDelegateRef = new CodeFieldReferenceExpression (thisExpr, "onBegin" + od.Name + "Delegate");
				var endOperDelegateRef = new CodeFieldReferenceExpression (thisExpr, "onEnd" + od.Name + "Delegate");
				var operCompletedDelegateRef = new CodeFieldReferenceExpression (thisExpr, "on" + od.Name + "CompletedDelegate");

				var ifstmt = new CodeConditionStatement (
					new CodeBinaryOperatorExpression (beginOperDelegateRef, CodeBinaryOperatorType.IdentityEquality, nullExpr),
					new CodeAssignStatement (beginOperDelegateRef, new CodeDelegateCreateExpression (new CodeTypeReference ("BeginOperationDelegate"), thisExpr, "OnBegin" + od.Name)));
				cm.Statements.Add (ifstmt);
				ifstmt = new CodeConditionStatement (
					new CodeBinaryOperatorExpression (endOperDelegateRef, CodeBinaryOperatorType.IdentityEquality, nullExpr),
					new CodeAssignStatement (endOperDelegateRef, new CodeDelegateCreateExpression (new CodeTypeReference ("EndOperationDelegate"), thisExpr, "OnEnd" + od.Name)));
				cm.Statements.Add (ifstmt);
				ifstmt = new CodeConditionStatement (
					new CodeBinaryOperatorExpression (operCompletedDelegateRef, CodeBinaryOperatorType.IdentityEquality, nullExpr),
					new CodeAssignStatement (operCompletedDelegateRef, new CodeDelegateCreateExpression (new CodeTypeReference (typeof (SendOrPostCallback)), thisExpr, "On" + od.Name + "Completed")));
				cm.Statements.Add (ifstmt);

				// InvokeAsync (onBeginBarOperDelegate, inValues, onEndBarOperDelegate, onBarOperCompletedDelegate, userState);

				inArgs.Add (new CodeParameterDeclarationExpression (typeof (object), "userState"));

				var args = new List<CodeExpression> ();
				args.Add (beginOperDelegateRef);
				args.Add (new CodeArrayCreateExpression (typeof (object), inArgs.ConvertAll<CodeExpression> (decl => new CodeArgumentReferenceExpression (decl.Name)).ToArray ()));
				args.Add (endOperDelegateRef);
				args.Add (new CodeFieldReferenceExpression (thisExpr, "on" + od.Name + "CompletedDelegate"));
				args.Add (new CodeArgumentReferenceExpression ("userState"));
				call = new CodeMethodInvokeExpression (baseExpr, "InvokeAsync", args.ToArray ());
				cm.Statements.Add (new CodeExpressionStatement (call));
			}
		}

		void AddMethodParam (CodeMemberMethod cm, Type type, string name)
		{
			cm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (type), name));
		}

		const string ms_arrays_ns = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";

		private CodeExpression[] ExportMessages (MessageDescriptionCollection messages, CodeMemberMethod method, bool return_args)
		{
			CodeExpression [] args = null;
			foreach (MessageDescription md in messages) {
				if (md.Direction == MessageDirection.Output) {
					if (md.Body.ReturnValue != null) {
						ExportDataContract (md.Body.ReturnValue);
						method.ReturnType = md.Body.ReturnValue.CodeTypeReference;
					}
					continue;
				}

				if (return_args)
					args = new CodeExpression [md.Body.Parts.Count];

				MessagePartDescriptionCollection parts = md.Body.Parts;
				for (int i = 0; i < parts.Count; i++) {
					ExportDataContract (parts [i]);

					method.Parameters.Add (
						new CodeParameterDeclarationExpression (
							parts [i].CodeTypeReference,
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

		void MergeCompileUnit (CodeCompileUnit from, CodeCompileUnit to)
		{
			if (from == to)
				return;
			foreach (CodeNamespace fns in from.Namespaces) {
				bool merged = false;
				foreach (CodeNamespace tns in to.Namespaces)
					if (fns.Name == tns.Name) {
						// namespaces are merged.
						MergeNamespace (fns, tns);
						merged = true;
						break;
					}
				if (!merged)
					to.Namespaces.Add (fns);
			}
		}

		// existing type is skipped.
		void MergeNamespace (CodeNamespace from, CodeNamespace to)
		{
			foreach (CodeTypeDeclaration ftd in from.Types) {
				bool skip = false;
				foreach (CodeTypeDeclaration ttd in to.Types)
					if (ftd.Name == ttd.Name) {
						skip = true;
						break;
					}
				if (!skip)
					to.Types.Add (ftd);
			}
		}

		private void ExportDataContract (MessagePartDescription md)
		{
			if (data_contract_importer == null)
				data_contract_importer = md.DataContractImporter;
			else if (md.DataContractImporter != null && data_contract_importer != md.DataContractImporter)
				throw new Exception ("INTERNAL ERROR: should not happen");
			if (xml_serialization_importer == null)
				xml_serialization_importer = md.XmlSerializationImporter;
			else if (md.XmlSerializationImporter != null && xml_serialization_importer != md.XmlSerializationImporter)
				throw new Exception ("INTERNAL ERROR: should not happen");
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
