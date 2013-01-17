// 
// BasicAnalysisDriver.cs
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
using Mono.CodeContracts.Static.ControlFlow.Subroutines;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.Drivers {
	class BasicAnalysisDriver : IBasicAnalysisDriver {
		private readonly IContractProvider contract_provider;
		private readonly IMetaDataProvider meta_data_provider;

		#region Implementation of IBasicAnalysisDriver<Local,Parameter,Method,Field,Property,Event,Type,Attribute,Assembly>
		public IMetaDataProvider MetaDataProvider
		{
			get { return this.meta_data_provider; }
		}


		public IContractProvider ContractProvider
		{
			get { return this.contract_provider; }
		}

		public SubroutineFacade SubroutineFacade { get; private set; }
		#endregion

		public BasicAnalysisDriver (IMetaDataProvider metaDataProvider,
		                            IContractProvider contractProvider)
		{
			SubroutineFacade = new SubroutineFacade (metaDataProvider, contractProvider);
			this.meta_data_provider = metaDataProvider;
			this.contract_provider = contractProvider;
		}

		public BasicMethodDriver CreateMethodDriver (Method method)
		{
			return new BasicMethodDriver (method, this);
		}
	}
}
