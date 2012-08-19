// 
// ICFG.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow {
	interface ICFG {
		APC Entry { get; }
		APC EntryAfterRequires { get; }
		APC NormalExit { get; }
		APC ExceptionExit { get; }
		Subroutine Subroutine { get; }
		
		APC Next (APC pc);
		
		IEnumerable<APC> Successors (APC pc);
		bool HasSingleSuccessor (APC pc, out APC ifFound);

		IEnumerable<APC> Predecessors (APC pc);
		bool HasSinglePredecessor (APC pc, out APC ifFound);

		bool IsJoinPoint (APC pc);
		bool IsSplitPoint (APC pc);

		bool IsBlockStart (APC pc);
		bool IsBlockEnd (APC pc);

		IILDecoder<APC, Dummy, Dummy, IMethodContextProvider, Dummy> GetDecoder (IMetaDataProvider metaDataProvider);

		void Print (TextWriter tw, ILPrinter<APC> printer,
		            Func<CFGBlock, IEnumerable<Sequence<Edge<CFGBlock, EdgeTag>>>> contextLookup,
		            Sequence<Edge<CFGBlock, EdgeTag>> context);

	    bool IsForwardBackEdge (APC @from, APC to);
	        APC Post (APC pc);
	}
}
