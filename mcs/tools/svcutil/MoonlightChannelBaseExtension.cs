//
// MoonlightChannelBaseExtension.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;

namespace Mono.ServiceContractTool
{
	class MoonlightChannelBaseContext
	{
		public MoonlightChannelBaseContractExtension Contract;
		public List<MoonlightChannelBaseOperationExtension> Operations = new List<MoonlightChannelBaseOperationExtension> ();

		public CodeTypeDeclaration ClientType { get; set; }
		public CodeTypeDeclaration ChannelType { get; set; }

		public void FindClientType (ServiceContractGenerationContext context)
		{
			var cd = context.Contract;
			string name = cd.Name + "Client";
			if (name [0] == 'I')
				name = name.Substring (1);

			foreach (CodeNamespace cns in context.ServiceContractGenerator.TargetCompileUnit.Namespaces)
				foreach (CodeTypeDeclaration ct in cns.Types)
					if (ct == context.ContractType)
						foreach (CodeTypeDeclaration ct2 in cns.Types)
							if (ct2.Name == name) {
								ClientType = ct2;
								return;
							}
			throw new Exception (String.Format ("Contract '{0}' not found", name));
		}

		public void Fixup ()
		{
			Contract.Fixup ();
			foreach (var op in Operations)
				op.Fixup ();
		}
	}

	class MoonlightChannelBaseContractExtension : IContractBehavior, IServiceContractGenerationExtension
	{
		public MoonlightChannelBaseContractExtension (MoonlightChannelBaseContext mlContext, bool generateSync)
		{
			ml_context = mlContext;
			generate_sync = generateSync;
		}

		MoonlightChannelBaseContext ml_context;
		bool generate_sync;

		// IContractBehavior
		public void AddBindingParameters (ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
			throw new NotSupportedException ();
		}

		public void ApplyClientBehavior (
			ContractDescription description,
			ServiceEndpoint endpoint,
			ClientRuntime proxy)
		{
			throw new NotSupportedException ();
		}

		public void ApplyDispatchBehavior (
			ContractDescription description,
			ServiceEndpoint endpoint,
			DispatchRuntime dispatch)
		{
			throw new NotSupportedException ();
		}

		public void Validate (
			ContractDescription description,
			ServiceEndpoint endpoint)
		{
			throw new NotSupportedException ();
		}

		// IServiceContractGenerationExtensions

		public void GenerateContract (
			ServiceContractGenerationContext context)
		{
			this.context = context;
			ml_context.Contract = this;
		}

		ServiceContractGenerationContext context;

		public void Fixup ()
		{
			ContractDescription cd = context.Contract;
			ml_context.FindClientType (context);
			var parentClass = ml_context.ClientType;

			if (!generate_sync)
				EliminateSync ();

			string name = cd.Name + "Channel";
			if (name [0] == 'I')
				name = name.Substring (1);

			var gt = new CodeTypeReference (cd.Name);
			var clientBaseType = new CodeTypeReference ("System.ServiceModel.ClientBase", gt);
			// this omits namespace, but should compile
			var channelBase = new CodeTypeReference ("ChannelBase", gt);
			var type = new CodeTypeDeclaration (name);
			parentClass.Members.Add (type);
			type.BaseTypes.Add (channelBase);
			type.BaseTypes.Add (new CodeTypeReference (cd.Name));
			type.TypeAttributes |= TypeAttributes.NestedPrivate;

			ml_context.ChannelType = type;

			// .ctor(ClientBase<T> client)
			var ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.Public;
			ctor.Parameters.Add (
				new CodeParameterDeclarationExpression (
					clientBaseType, "client"));
			ctor.BaseConstructorArgs.Add (
				new CodeArgumentReferenceExpression ("client"));
			type.Members.Add (ctor);

			// In Client type:
			// protected override TChannel CreateChannel()
			var creator = new CodeMemberMethod ();
			creator.Name = "CreateChannel";
			creator.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			creator.ReturnType = gt;
			creator.Statements.Add (
				new CodeMethodReturnStatement (
					new CodeCastExpression (
						gt,
						new CodeObjectCreateExpression (
							new CodeTypeReference (name),
							new CodeThisReferenceExpression ()))));
			parentClass.Members.Add (creator);

			// clear IExtensibleDataObject. Since there is *no* way 
			// to identify the type of a TypeReference, I cannot do 
			// anything but this brutal removal.
			// Also clear ExtensionDataObject members.
			foreach (CodeNamespace cns in context.ServiceContractGenerator.TargetCompileUnit.Namespaces) {
				foreach (CodeTypeDeclaration ct in cns.Types) {
					if (!ShouldPreserveBaseTypes (ct))
						ct.BaseTypes.Clear ();
					CodeTypeMember cp = null, cf = null;
					foreach (CodeTypeMember cm in ct.Members) {
						if (cm is CodeMemberProperty && cm.Name == "ExtensionData")
							cp = cm;
						else if (cm is CodeMemberField && cm.Name == "extensionDataField")
							cf = cm;
					}
					if (cf != null)
						ct.Members.Remove (cf);
					if (cp != null)
						ct.Members.Remove (cp);
				}
			}
		}

		bool ShouldPreserveBaseTypes (CodeTypeDeclaration ct)
		{
			foreach (CodeTypeReference cr in ct.BaseTypes) {
				if (cr.BaseType == "System.ServiceModel.ClientBase`1")
					return true;
				if (cr.BaseType == "System.ComponentModel.AsyncCompletedEventArgs")
					return true;
			}
			return false;
		}

		void EliminateSync ()
		{
			var type = context.ContractType;

			// remove such OperationContract methods that do not have AsyncPattern parameter. It is sort of hack as it does not check the value (it might be "false").
			var l = new List<CodeMemberMethod> ();
			foreach (CodeMemberMethod cm in type.Members) {
				bool isOperation = false, isAsync = false;
				foreach (CodeAttributeDeclaration att in cm.CustomAttributes) {
					if (att.Name == "System.ServiceModel.OperationContractAttribute")
						isOperation = true;
					else
						continue;
					foreach (CodeAttributeArgument aa in att.Arguments) {
						if (aa.Name == "AsyncPattern") {
							isAsync = true;
							break;
						}
					}
					if (isAsync)
						break;
				}
				if (isOperation && !isAsync)
					l.Add (cm);
			}
			foreach (var cm in l)
				type.Members.Remove (cm);

			// remove corresponding client implementation methods. 
			// It is sort of hack as it only checks method and 
			// parameter names (ideally we want to check parameter
			// types, but there is no way to compare 
			// CodeTypeReferences).
			var lc = new List<CodeMemberMethod> ();
			foreach (var cm_ in ml_context.ClientType.Members) {
				var cm = cm_ as CodeMemberMethod;
				if (cm == null)
					continue;
				foreach (var sm in l) {
					if (cm.Name != sm.Name || cm.Parameters.Count != sm.Parameters.Count)
						continue;
					bool diff = false;
					for (int i = 0; i < cm.Parameters.Count; i++) {
						var cp = cm.Parameters [i];
						var sp = sm.Parameters [i];
						if (cp.Direction != sp.Direction || cp.Name != sp.Name) {
							diff = true;
							break;
						}
					}
					if (diff)
						continue;
					lc.Add (cm);
					break;
				}
			}
			foreach (var cm in lc)
				ml_context.ClientType.Members.Remove (cm);
		}
	}

	class MoonlightChannelBaseOperationExtension : IOperationBehavior, IOperationContractGenerationExtension
	{
		public MoonlightChannelBaseOperationExtension (MoonlightChannelBaseContext mlContext, bool generateSync)
		{
			ml_context = mlContext;
			generate_sync = generateSync;
		}

		MoonlightChannelBaseContext ml_context;
		bool generate_sync;

		// IOperationBehavior

		public void AddBindingParameters (
			OperationDescription description,
			BindingParameterCollection parameters)
		{
			throw new NotSupportedException ();
		}

		public void ApplyDispatchBehavior (
			OperationDescription description,
			DispatchOperation dispatch)
		{
			throw new NotSupportedException ();
		}

		public void ApplyClientBehavior (
			OperationDescription description,
			ClientOperation proxy)
		{
			throw new NotSupportedException ();
		}

		public void Validate (
			OperationDescription description)
		{
			throw new NotSupportedException ();
		}

		// IOperationContractGenerationContext

		public void GenerateOperation (OperationContractGenerationContext context)
		{
			this.context = context;
			ml_context.Operations.Add (this);
		}

		OperationContractGenerationContext context;

		public void Fixup ()
		{
			if (generate_sync)
				FixupSync ();
			FixupAsync ();
		}

		void FixupSync ()
		{
			var type = ml_context.ChannelType;
			var od = context.Operation;

			// sync method implementation
			CodeMemberMethod cm = new CodeMemberMethod ();
			type.Members.Add (cm);
			cm.Name = od.Name;
			cm.Attributes = MemberAttributes.Public 
					| MemberAttributes.Final;

			var inArgs = new List<CodeParameterDeclarationExpression > ();
			var outArgs = new List<CodeParameterDeclarationExpression > ();

			foreach (CodeParameterDeclarationExpression p in context.SyncMethod.Parameters) {
				inArgs.Add (p);
				cm.Parameters.Add (p);
			}

			cm.ReturnType = context.SyncMethod.ReturnType;

			var argsDecl = new CodeVariableDeclarationStatement (
				typeof (object []),
				"args",
				new CodeArrayCreateExpression (typeof (object), inArgs.ConvertAll<CodeExpression> (decl => new CodeArgumentReferenceExpression (decl.Name)).ToArray ()));
			cm.Statements.Add (argsDecl);

			var args = new List<CodeExpression> ();
			args.Add (new CodePrimitiveExpression (od.Name));
			args.Add (new CodeVariableReferenceExpression ("args"));

			CodeExpression call = new CodeMethodInvokeExpression (
				new CodeBaseReferenceExpression (),
				"Invoke",
				args.ToArray ());

			if (cm.ReturnType.BaseType == "System.Void")
				cm.Statements.Add (new CodeExpressionStatement (call));
			else
				cm.Statements.Add (new CodeMethodReturnStatement (new CodeCastExpression (context.SyncMethod.ReturnType, call)));
		}

		public void FixupAsync ()
		{
			var type = ml_context.ChannelType;
			var od = context.Operation;

			var baseExpr = new CodeBaseReferenceExpression ();
			var asyncResultType = new CodeTypeReference (typeof (IAsyncResult));

			// BeginXxx() implementation
			CodeMemberMethod cm = new CodeMemberMethod () {
				Name = "Begin" + od.Name,
				Attributes = MemberAttributes.Public | MemberAttributes.Final,
				ReturnType = asyncResultType
				};
			type.Members.Add (cm);

			var inArgs = new List<CodeParameterDeclarationExpression > ();
			foreach (CodeParameterDeclarationExpression p in context.BeginMethod.Parameters) {
				inArgs.Add (p);
				cm.Parameters.Add (p);
			}
			inArgs.RemoveAt (inArgs.Count - 1);
			inArgs.RemoveAt (inArgs.Count - 1);

			var call = new CodeMethodInvokeExpression (
				baseExpr,
				"BeginInvoke",
				new CodePrimitiveExpression (od.Name),
				new CodeArrayCreateExpression (typeof (object), inArgs.ConvertAll<CodeExpression> (decl => new CodeArgumentReferenceExpression (decl.Name)).ToArray ()),
				new CodeArgumentReferenceExpression ("asyncCallback"),
				new CodeArgumentReferenceExpression ("userState"));
			cm.Statements.Add (new CodeMethodReturnStatement (call));

			// EndXxx() implementation

			cm = new CodeMemberMethod () {
				Name = "End" + od.Name,
				Attributes = MemberAttributes.Public | MemberAttributes.Final,
				ReturnType = context.EndMethod.ReturnType };
			type.Members.Add (cm);

			AddMethodParam (cm, typeof (IAsyncResult), "result");

			var outArgs = new List<CodeParameterDeclarationExpression > ();

			string resultArgName = "result";
			var argsDecl = new CodeVariableDeclarationStatement (
				typeof (object []),
				"args",
				new CodeArrayCreateExpression (typeof (object), new CodePrimitiveExpression (outArgs.Count)));
			cm.Statements.Add (argsDecl);

			var ret = new CodeMethodInvokeExpression (
				baseExpr,
				"EndInvoke",
				new CodePrimitiveExpression (od.Name),
				new CodeVariableReferenceExpression ("args"),
				new CodeArgumentReferenceExpression (resultArgName));
			if (cm.ReturnType.BaseType == "System.Void")
				cm.Statements.Add (new CodeExpressionStatement (ret));
			else
				cm.Statements.Add (new CodeMethodReturnStatement (new CodeCastExpression (context.EndMethod.ReturnType, ret)));
		}

		void AddMethodParam (CodeMemberMethod cm, Type type, string name)
		{
			cm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (type), name));
		}
	}
}
