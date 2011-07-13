// 
// EnsuresFactory.cs
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
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders {
	class EnsuresFactory : SubroutineFactory<Method, Pair<Method, IImmutableSet<Subroutine>>> {
		public EnsuresFactory (SubroutineFacade subroutineFacade) : base (subroutineFacade)
		{
		}

		#region Overrides of SubroutineFactory<Method,Pair<Method,IImmutableSet<Subroutine>>>
		protected override Subroutine BuildNewSubroutine (Method method)
		{
			if (ContractProvider != null) {
				IImmutableSet<Subroutine> inheritedEnsures = GetInheritedEnsures (method);
				if (ContractProvider.HasEnsures (method))
					return ContractProvider.AccessEnsures (method, this, new Pair<Method, IImmutableSet<Subroutine>> (method, inheritedEnsures));
				if (inheritedEnsures.Count > 0) {
					if (inheritedEnsures.Count > 1)
						return new EnsuresSubroutine<Dummy> (this.SubroutineFacade, method, inheritedEnsures);
					return inheritedEnsures.Any;
				}
			}
			return new EnsuresSubroutine<Dummy> (this.SubroutineFacade, method, null);
		}

		private IImmutableSet<Subroutine> GetInheritedEnsures (Method method)
		{
			IImmutableSet<Subroutine> result = ImmutableSet<Subroutine>.Empty ();
			if (MetaDataProvider.IsVirtual (method) && ContractProvider.CanInheritContracts (method)) {
				foreach (Method implementedMethod in MetaDataProvider.OverridenAndImplementedMethods (method)) {
					Subroutine subroutine = Get (MetaDataProvider.Unspecialized (implementedMethod));
					if (subroutine != null)
						result = result.Add (subroutine);
				}
			}
			return result;
		}

		protected override Subroutine Factory<Label> (SimpleSubroutineBuilder<Label> builder, Label entry, Pair<Method, IImmutableSet<Subroutine>> data)
		{
			return new EnsuresSubroutine<Label> (this.SubroutineFacade, data.Key, builder, entry, data.Value);
		}
		#endregion
	}
}
