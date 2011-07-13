// 
// SubroutineFactory.cs
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

using System.Collections.Generic;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders {
	abstract class SubroutineFactory<Key, Data> : ICodeConsumer<Data, Subroutine> {
		protected readonly SubroutineFacade SubroutineFacade;
		private readonly Dictionary<Key, Subroutine> cache = new Dictionary<Key, Subroutine> ();

		protected SubroutineFactory (SubroutineFacade subroutineFacade)
		{
			this.SubroutineFacade = subroutineFacade;
		}

		protected IContractProvider ContractProvider
		{
			get { return this.SubroutineFacade.ContractProvider; }
		}

		protected IMetaDataProvider MetaDataProvider
		{
			get { return this.SubroutineFacade.MetaDataProvider; }
		}

		#region ICodeConsumer<Data,Subroutine> Members
		public Subroutine Accept<Label> (ICodeProvider<Label> codeProvider, Label entryPoint, Data data)
		{
			return Factory (new SimpleSubroutineBuilder<Label> (codeProvider, this.SubroutineFacade, entryPoint), entryPoint, data);
		}
		#endregion

		public Subroutine Get (Key key)
		{
			if (this.cache.ContainsKey (key))
				return this.cache [key];

			Subroutine sub = BuildNewSubroutine (key);
			this.cache.Add (key, sub);
			if (sub != null)
				sub.Initialize ();
			return sub;
		}

		protected abstract Subroutine BuildNewSubroutine (Key key);
		protected abstract Subroutine Factory<Label> (SimpleSubroutineBuilder<Label> builder, Label entry, Data data);
	}
}
