// 
// PathElement.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
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
using Mono.CodeContracts.Static.AST.Visitors;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.Paths
{
  abstract class PathElement
  {
    public virtual bool IsBooleanTyped { get { return false; } }
    public virtual bool IsDeref { get { return false; } }

    public virtual bool IsMethodCall { get { return false; } }
    public virtual bool IsGetter { get { return false; } }
    public virtual bool IsStatic { get { return false; } }
    public virtual bool IsUnmanagedPointer { get { return false; } }
    public virtual bool IsManagedPointer { get { return false; } }
    public virtual bool IsParameter { get { return false; } }
    public virtual bool IsParameterRef { get { return false; } }
    public virtual string CastTo  { get { return ""; } }
    public abstract bool IsAddressOf { get; }

    public virtual bool TryField(out Field f)
    {
      f = default(Field);
      return false;
    }

    public abstract bool TryGetResultType(out TypeNode type);

    public abstract TResult Decode<TData, TResult, TVisitor, TLabel>(TLabel label, TVisitor visitor, TData data)
      where TVisitor : IAggregateVisitor<TLabel, TData, TResult>;

    public abstract override string ToString();
  }
}