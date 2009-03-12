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
using System.IO;
using DbLinq.Schema.Dbml;
using DbLinq.Vendor;

namespace DbMetal.Generator
{
    public interface IProcessor
    {
        /// <summary>
        /// The SchemaLoadFactory is used to create the ISchemaLoader (who loads the schema from the database)
        /// Automatically created by IProcessor implementation
        /// </summary>
        ISchemaLoaderFactory SchemaLoaderFactory { get; set; }

        /// <summary>
        /// Log output
        /// </summary>
        TextWriter Log { get; set; }

        /// <summary>
        /// Whole process, given application parameters
        /// </summary>
        /// <param name="args"></param>
        void Process(string[] args);
        /// <summary>
        /// Generates a code file, given parameters
        /// </summary>
        /// <param name="parameters">Application parameters</param>
        /// <param name="dbSchema">Loaded schema (from file or database)</param>
        /// <param name="schemaLoader">Schema loader related to dbSchema</param>
        /// <param name="filename">Output filename</param>
        void GenerateCode(Parameters parameters, Database dbSchema, ISchemaLoader schemaLoader, string filename);
        /// <summary>
        /// Loads a schema from a database
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="schemaLoader"></param>
        /// <returns></returns>
        Database ReadSchema(Parameters parameters, out ISchemaLoader schemaLoader);
        /// <summary>
        /// Loads a schema from a given file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
		Database ReadSchema(Parameters parameters, string filename);
        /// <summary>
        /// Lists all code generators available in DbMetal
        /// </summary>
        /// <returns></returns>
        IEnumerable<ICodeGenerator> EnumerateCodeGenerators();
        /// <summary>
        /// Find a code generator given the /language or filename extension
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        ICodeGenerator FindCodeGenerator(Parameters parameters, string filename);
    }
}
