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
using DbLinq.Schema;

namespace DbMetal.Generator.EntityInterface.Implementation
{
    /// <summary>
    /// Helper to make default IImplementation implementation
    /// </summary>
#if !MONO_STRICT
    public
#endif
    abstract class InterfaceImplementation : IImplementation
    {
        public abstract string InterfaceName { get; }

        public virtual void WriteHeader(CodeWriter writer, GenerationContext context)
        {
        }

        public virtual void WriteClassHeader(CodeWriter writer, DbLinq.Schema.Dbml.Table table, GenerationContext context)
        {
        }

        public virtual void WritePropertyBeforeSet(CodeWriter writer, DbLinq.Schema.Dbml.Column property, GenerationContext context)
        {
        }

        public virtual void WritePropertyAfterSet(CodeWriter writer, DbLinq.Schema.Dbml.Column property, GenerationContext context)
        {
        }
    }
}
