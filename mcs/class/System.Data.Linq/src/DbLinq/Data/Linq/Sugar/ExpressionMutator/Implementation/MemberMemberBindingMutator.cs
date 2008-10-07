#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#if MONO_STRICT
namespace System.Data.Linq.Sugar.ExpressionMutator.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.ExpressionMutator.Implementation
#endif
{
    internal class MemberMemberBindingMutator : IMemberBindingMutator
    {
        protected MemberMemberBinding MemberMemberBinding { get; private set; }

        public IEnumerable<Expression> Operands
        {
            get
            {
                // a MemberMemberBindings is recursive, so let's recurse
                foreach (var memberBinding in MemberMemberBinding.Bindings)
                {
                    foreach (Expression operand in MemberBindingMutatorFactory.GetMutator(memberBinding).Operands)
                        yield return operand;
                }
            }
        }

        public MemberBinding Mutate(IList<Expression> operands)
        {
            var bindings = new List<MemberBinding>();
            int operandsIndex = 0;
            // same thing here. The difficulty is that we have to split out operands
            foreach (var memberBinding in MemberMemberBinding.Bindings)
            {
                int operandsCount = MemberBindingMutatorFactory.GetMutator(memberBinding).Operands.Count();
                var subOperands = operands.Skip(operandsIndex).Take(operandsCount).ToList();
                bindings.Add(MemberBindingMutatorFactory.GetMutator(memberBinding).Mutate(subOperands));
                operandsIndex += operandsCount;
            }
            return Expression.MemberBind(MemberMemberBinding.Member, bindings);
        }

        public MemberMemberBindingMutator(MemberMemberBinding memberMemberBinding)
        {
            MemberMemberBinding = memberMemberBinding;
        }
    }
}