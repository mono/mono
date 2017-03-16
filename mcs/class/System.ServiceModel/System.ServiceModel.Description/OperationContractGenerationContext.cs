//
// OperationContractGenerationContext.cs
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
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Web.Services.Description;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public class OperationContractGenerationContext
	{
		public OperationContractGenerationContext (
			ServiceContractGenerator serviceContractGenerator,
			ServiceContractGenerationContext contract,
			OperationDescription operation,
			CodeTypeDeclaration declaringType,
			CodeMemberMethod method)
			: this (serviceContractGenerator, contract, operation,
				declaringType, method, null, null)
		{
		}

		public OperationContractGenerationContext (
			ServiceContractGenerator serviceContractGenerator,
			ServiceContractGenerationContext contract,
			OperationDescription operation,
			CodeTypeDeclaration declaringType,
			CodeMemberMethod syncMethod,
			CodeMemberMethod beginMethod,
			CodeMemberMethod endMethod)
		{
			generator = serviceContractGenerator;
			this.contract = contract;
			this.operation = operation;
			declaring_type = declaringType;
			this.method = syncMethod;
			this.begin_method = beginMethod;
			this.end_method = endMethod;
		}

		ServiceContractGenerator generator;
		ServiceContractGenerationContext contract;
		OperationDescription operation;
		CodeTypeDeclaration declaring_type;
		CodeMemberMethod method, begin_method, end_method;

		public ServiceContractGenerator ServiceContractGenerator {
			get { return generator; }
		}
		public ServiceContractGenerationContext Contract {
			get { return contract; }
		}
		public OperationDescription Operation {
			get { return operation; }
		}
		public CodeTypeDeclaration DeclaringType {
			get { return declaring_type; }
		}
		public CodeMemberMethod SyncMethod {
			get { return method; }
		}
		public CodeMemberMethod BeginMethod {
			get { return begin_method; }
		}
		public CodeMemberMethod EndMethod {
			get { return end_method; }
		}
		public bool IsAsync {
			get { return begin_method != null; }
		}
	}
}
