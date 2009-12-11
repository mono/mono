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
using System.IO;
using DbLinq.Vendor;

namespace DbMetal.Generator
{
#if !MONO_STRICT
    public
#endif
    interface ISchemaLoaderFactory
    {
        /// <summary>
        /// the 'main entry point' into this class
        /// </summary>
        ISchemaLoader Load(Parameters parameters);

        /// <summary>
        /// loads a ISchemaLoader from a provider id string (used by schema loader)
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        ISchemaLoader Load(string provider);

        /// <summary>
        /// given a schemaLoaderType and dbConnType 
        /// (e.g. DbLinq.Oracle.OracleSchemaLoader and System.Data.OracleClient.OracleConnection),
        /// return an instance of the OracleSchemaLoader.
        /// </summary>
        ISchemaLoader Load(Parameters parameters, Type dbLinqSchemaLoaderType, Type databaseConnectionType, Type sqlDialectType);

        ISchemaLoader Load(Parameters parameters, string dbLinqSchemaLoaderTypeName, string databaseConnectionTypeName, string sqlDialectTypeName);

        /// <summary>
        /// Log output
        /// </summary>
        TextWriter Log { get; set; }
    }
}
