// 
// CodeContractDecoder.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;

namespace Mono.CodeContracts.Static.Providers {
	class CodeContractDecoder : IContractProvider {
		public static readonly CodeContractDecoder Instance = new CodeContractDecoder ();

		#region IContractProvider Members
		public bool VerifyMethod (Method method, bool analyzeNonUserCode)
		{
			//todo: implement this
			return true;
		}

		public bool HasRequires (Method method)
		{
			method = GetMethodWithContractFor (method);
			if (method.MethodContract == null)
				return false;

			return method.MethodContract.RequiresCount > 0;
		}

		public Result AccessRequires<Data, Result> (Method method,
		                                            ICodeConsumer<Data, Result> consumer, Data data)
		{
			method = GetMethodWithContractFor (method);
			return consumer.Accept (CodeProviderImpl.Instance, new CodeProviderImpl.PC (method.MethodContract, 0), data);
		}

		public bool HasEnsures (Method method)
		{
			method = GetMethodWithContractFor (method);
			if (method.MethodContract == null)
				return false;

			return method.MethodContract.EnsuresCount > 0;
		}

		public Result AccessEnsures<Data, Result> (Method method, ICodeConsumer<Data, Result> consumer, Data data)
		{
			method = GetMethodWithContractFor (method);
			return consumer.Accept (CodeProviderImpl.Instance, new CodeProviderImpl.PC (method.MethodContract, method.MethodContract.RequiresCount + 2), data);
		}

		public bool CanInheritContracts (Method method)
		{
			return false;
		}
		#endregion

		private Method GetMethodWithContractFor (Method method)
		{
			//todo: implement this
			return method;
		}
	}
}
