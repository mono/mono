// 
// MethodContractSubroutine.cs
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
using Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines {
	abstract class MethodContractSubroutine<TLabel> : SubroutineBase<TLabel>, IMethodInfo {
		private readonly Method method;

		protected MethodContractSubroutine (SubroutineFacade subroutineFacade,
		                                    Method method) : base (subroutineFacade)
		{
			this.method = method;
		}

		protected MethodContractSubroutine (SubroutineFacade subroutineFacade,
		                                    Method method,
		                                    SimpleSubroutineBuilder<TLabel> builder,
		                                    TLabel startLabel) : base (subroutineFacade, startLabel, builder)
		{
			this.method = method;
		}

		protected new SimpleSubroutineBuilder<TLabel> Builder
		{
			get { return (SimpleSubroutineBuilder<TLabel>) base.Builder; }
			set { base.Builder = value; }
		}

		#region IMethodInfo Members
		public Method Method
		{
			get { return this.method; }
		}
		#endregion
	}
}
