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

namespace DbMetal.Generator.EntityInterface
{
    /// <summary>
    /// When asking for special interfaces implementation in generated entities,
    /// the following is used.
    /// It is identified by the target interface name, called 3 times,
    /// during the header generation, before and after the setter.
    /// </summary>
#if !MONO_STRICT
    public
#endif
    interface IImplementation
    {
        /// <summary>
        /// Interface name, in short format
        /// </summary>
        string InterfaceName { get; }

        /// <summary>
        /// Called at file top
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="context"></param>
        void WriteHeader(CodeWriter writer, GenerationContext context);

        /// <summary>
        /// Called at class top
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="table"></param>
        /// <param name="context"></param>
        void WriteClassHeader(CodeWriter writer, DbLinq.Schema.Dbml.Table table, GenerationContext context);

        /// <summary>
        /// Called before the value is assigned to backing field
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="property"></param>
        /// <param name="context"></param>
        void WritePropertyBeforeSet(CodeWriter writer, DbLinq.Schema.Dbml.Column property, GenerationContext context);

        /// <summary>
        /// Called after the value is assigned to backing field
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="property"></param>
        /// <param name="context"></param>
        void WritePropertyAfterSet(CodeWriter writer, DbLinq.Schema.Dbml.Column property, GenerationContext context);
    }
}
