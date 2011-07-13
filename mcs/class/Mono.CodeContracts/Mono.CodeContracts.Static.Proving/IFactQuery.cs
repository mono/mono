// 
// IFactQuery.cs
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

using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Proving {
	interface IFactQuery<Expression, Variable> : IFactBase<Variable> {
		ProofOutcome IsNull (APC pc, Expression expr);
		ProofOutcome IsNonNull (APC pc, Expression expr);
		ProofOutcome IsTrue (APC pc, Expression expr);
		ProofOutcome IsTrueImply (APC pc, LispList<Expression> positiveAssumptions, LispList<Expression> negativeAssumptions, Expression goal);
		ProofOutcome IsGreaterEqualToZero (APC pc, Expression expr);
		ProofOutcome IsLessThan (APC pc, Expression expr, Expression right);
		ProofOutcome IsNonZero (APC pc, Expression expr);
	}
}
