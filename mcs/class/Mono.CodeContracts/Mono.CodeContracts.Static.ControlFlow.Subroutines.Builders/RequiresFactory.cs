// 
// RequiresFactory.cs
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
	class RequiresFactory : SubroutineFactory<Method, Pair<Method, IImmutableSet<Subroutine>>> {
		public RequiresFactory (SubroutineFacade subroutineFacade)
			: base (subroutineFacade)
		{
		}

		#region Overrides of SubroutineFactory<Method,Pair<Method,IImmutableSet<Subroutine>>>
		protected override Subroutine BuildNewSubroutine (Method method)
		{
			if (ContractProvider != null) {
				IImmutableSet<Subroutine> inheritedRequires = GetInheritedRequires (method);
				if (ContractProvider.HasRequires (method))
					return ContractProvider.AccessRequires (method, this, new Pair<Method, IImmutableSet<Subroutine>> (method, inheritedRequires));
				if (inheritedRequires.Count > 0) {
					if (inheritedRequires.Count == 1)
						return inheritedRequires.Any;

					return new RequiresSubroutine<Dummy> (this.SubroutineFacade, method, inheritedRequires);
				}
			}
			return null;
		}

		private IImmutableSet<Subroutine> GetInheritedRequires (Method method)
		{
			IImmutableSet<Subroutine> result = ImmutableSet<Subroutine>.Empty ();

			if (MetaDataProvider.IsVirtual (method) && ContractProvider.CanInheritContracts (method)) {
				Method rootMethod;
				if (MetaDataProvider.TryGetRootMethod (method, out rootMethod)) {
					Subroutine sub = Get (MetaDataProvider.Unspecialized (method));
					if (sub != null)
						result = result.Add (sub);
				}
				foreach (Method implMethod in MetaDataProvider.ImplementedMethods (method)) {
					Subroutine sub = Get (MetaDataProvider.Unspecialized (implMethod));
					if (sub != null)
						result = result.Add (sub);
				}
			}

			return result;
		}

		protected override Subroutine Factory<Label> (SimpleSubroutineBuilder<Label> builder, Label entry, Pair<Method, IImmutableSet<Subroutine>> data)
		{
			return new RequiresSubroutine<Label> (this.SubroutineFacade, data.Key, builder, entry, data.Value);
		}
		#endregion
	}
}
