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
using System.IO;
using DbLinq.Schema;
using DbLinq.Vendor;
using DbMetal.Generator;

namespace DbMetal.Generator
{
    /// <summary>
    /// Generates a code file fro a DBML schema
    /// </summary>
#if !MONO_STRICT
    public
#endif
    interface ICodeGenerator
    {
        /// <summary>
        /// Used to match /language parameter
        /// </summary>
        string LanguageCode { get;  }

        /// <summary>
        /// Default file extension with leading dot (for example '.cs')
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// Writes DBML file to stream
        /// </summary>
        /// <param name="textWriter">Output text stream</param>
        /// <param name="dbSchema">DBML schema</param>
        /// <param name="context">Holds parameters, variables and ISchemaLoader</param>
        void Write(TextWriter textWriter, DbLinq.Schema.Dbml.Database dbSchema, GenerationContext context);
    }
}
