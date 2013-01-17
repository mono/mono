// 
// ISyntheticILVisitor.cs
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

namespace Mono.CodeContracts.Static.AST.Visitors {
	interface ISyntheticILVisitor<Label, Source, Dest, Data, Result> {
		Result Entry (Label pc, Method method, Data data);
		Result Assume (Label pc, EdgeTag tag, Source condition, Data data);
		Result Assert (Label pc, EdgeTag tag, Source condition, Data data);
		Result LoadStack (Label pc, int offset, Dest dest, Source source, bool isOld, Data data);
		Result LoadStackAddress (Label pc, int offset, Dest dest, Source source, TypeNode type, bool isOld, Data data);
		Result LoadResult (Label pc, TypeNode type, Dest dest, Source source, Data data);
		Result BeginOld (Label pc, Label matchingEnd, Data data);
		Result EndOld (Label pc, Label matchingBegin, TypeNode type, Dest dest, Source source, Data data);
	}
}
