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
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

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
		public MoonlightChannelBaseContractExtension (MoonlightChannelBaseContext mlContext)
		{
			ml_context = mlContext;
		}

		MoonlightChannelBaseContext ml_context;

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
		}
	}

	class MoonlightChannelBaseOperationExtension : IOperationBehavior, IOperationContractGenerationExtension
	{
		public MoonlightChannelBaseOperationExtension (MoonlightChannelBaseContext mlContext)
		{
			ml_context = mlContext;
		}

		MoonlightChannelBaseContext ml_context;

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
			var type = ml_context.ChannelType;
			var od = context.Operation;

			CodeMemberMethod cm = new CodeMemberMethod ();
			type.Members.Add (cm);
			cm.Name = "Begin" + od.Name;
			cm.Attributes = MemberAttributes.Public 
					| MemberAttributes.Final;

			var inArgs = new List<CodeParameterDeclarationExpression > ();
			var outArgs = new List<CodeParameterDeclarationExpression > ();

			foreach (CodeParameterDeclarationExpression p in context.BeginMethod.Parameters) {
				inArgs.Add (p);
				cm.Parameters.Add (p);
			}
			inArgs.RemoveAt (inArgs.Count - 1);
			inArgs.RemoveAt (inArgs.Count - 1);

//			cm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (AsyncCallback)), "asyncCallback"));
//			cm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (object)), "userState"));
			cm.ReturnType = new CodeTypeReference (typeof (IAsyncResult));

			var argsDecl = new CodeVariableDeclarationStatement (
				typeof (object []),
				"args",
				new CodeArrayCreateExpression (typeof (object), inArgs.ConvertAll<CodeExpression> (decl => new CodeArgumentReferenceExpression (decl.Name)).ToArray ()));
			cm.Statements.Add (argsDecl);

			var args = new List<CodeExpression> ();
			args.Add (new CodePrimitiveExpression (od.Name));
			args.Add (new CodeVariableReferenceExpression ("args"));
			args.Add (new CodeArgumentReferenceExpression ("asyncCallback"));
			args.Add (new CodeArgumentReferenceExpression ("userState"));

			CodeExpression call = new CodeMethodInvokeExpression (
				new CodeBaseReferenceExpression (),
				"BeginInvoke",
				args.ToArray ());

			if (cm.ReturnType.BaseType == "System.Void")
				cm.Statements.Add (new CodeExpressionStatement (call));
			else
				cm.Statements.Add (new CodeMethodReturnStatement (call));

			// EndXxx() implementation

			cm = new CodeMemberMethod ();
			cm.Attributes = MemberAttributes.Public 
					| MemberAttributes.Final;
			type.Members.Add (cm);
			cm.Name = "End" + od.Name;

			var res = new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (IAsyncResult)), "result");
			cm.Parameters.Add (res);

			cm.ReturnType = context.EndMethod.ReturnType;

			string resultArgName = "result";
			argsDecl = new CodeVariableDeclarationStatement (
				typeof (object []),
				"args",
				new CodeArrayCreateExpression (typeof (object), new CodePrimitiveExpression (outArgs.Count)));
			cm.Statements.Add (argsDecl);

			call = new CodeCastExpression (
				context.EndMethod.ReturnType,
				new CodeMethodInvokeExpression (
				new CodeBaseReferenceExpression (),
				"EndInvoke",
				new CodePrimitiveExpression (od.Name),
				new CodeVariableReferenceExpression ("args"),
				new CodeArgumentReferenceExpression (resultArgName)));

			if (cm.ReturnType.BaseType == "System.Void")
				cm.Statements.Add (new CodeExpressionStatement (call));
			else
				cm.Statements.Add (new CodeMethodReturnStatement (call));
		}
	}
}
