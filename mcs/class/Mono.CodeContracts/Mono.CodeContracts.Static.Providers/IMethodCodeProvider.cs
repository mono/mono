// 
// IMethodCodeProvider.cs
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
using Mono.CodeContracts.Static.AST;

namespace Mono.CodeContracts.Static.Providers {
	interface IMethodCodeProvider<Label, Handler> : ICodeProvider<Label> {
		bool IsFaultHandler (Handler handler);
		bool IsFilterHandler (Handler handler);
		bool IsCatchHandler (Handler handler);
		bool IsCatchAllHandler (Handler handler);
		bool IsFinallyHandler (Handler handler);

		TypeNode CatchType (Handler handler);
		IEnumerable<Handler> GetTryBlocks (Method method);

		Label FilterExpressionStart (Handler handler);
		Label HandlerEnd (Handler handler);
		Label HandlerStart (Handler handler);
		Label TryStart (Handler handler);
		Label TryEnd (Handler handler);
	}
}
