//
// ServiceContractGenerationContext.cs
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Web.Services.Description;

namespace System.ServiceModel.Description
{
	public class ServiceContractGenerationContext
	{
		public ServiceContractGenerationContext (
			ServiceContractGenerator serviceContractGenerator,
			ContractDescription contract,
			CodeTypeDeclaration contractType)
			: this (serviceContractGenerator, contract, contractType, null)
		{
		}

		public ServiceContractGenerationContext (
			ServiceContractGenerator serviceContractGenerator,
			ContractDescription contract,
			CodeTypeDeclaration contractType,
			CodeTypeDeclaration duplexCallbackType)
		{
			generator = serviceContractGenerator;
			this.contract = contract;
			contract_type = contractType;
			duplex_callback_type = duplexCallbackType;
		}

		ServiceContractGenerator generator;
		ContractDescription contract;
		CodeTypeDeclaration contract_type;
		CodeTypeDeclaration duplex_callback_type;
		Collection<OperationContractGenerationContext> operations
			= new Collection<OperationContractGenerationContext> ();

		public ServiceContractGenerator ServiceContractGenerator {
			get { return generator; }
		}

		public ContractDescription Contract {
			get { return contract; }
		}

		public CodeTypeDeclaration ContractType {
			get { return contract_type; }
		}

		public CodeTypeDeclaration DuplexCallbackType {
			get { return duplex_callback_type; }
		}

		public Collection<OperationContractGenerationContext> Operations {
			get { return operations; }
		}
	}
}
