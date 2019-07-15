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

using System;

namespace DbMetal.Generator
{
    /// <summary>
    /// Parameter definition for code generation
    /// </summary>
    public class ParameterDefinition
    {
        /// <summary>
        /// Parameter attributes
        /// </summary>
        public AttributeDefinition Attribute;
        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Parameter CLR type
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// Parameter literal type
        /// If null, the Type is used
        /// </summary>
        public string LiteralType { get; set; }
        /// <summary>
        /// Parameter specifications, in, out, etc.
        /// </summary>
        public SpecificationDefinition SpecificationDefinition;
    }
}