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
using System.Data;
using DbLinq.Schema.Dbml;
using DbLinq.Vendor;
using Type = System.Type;

namespace DbMetal.Generator.Implementation.CodeTextGenerator
{
    partial class CodeGenerator
    {
        protected virtual bool WriteDataContextCtor(CodeWriter writer, Database schema, Type contextBaseType,
            ParameterDefinition[] parameters, string[] baseCallParameterNames, Type[] baseCallParameterTypes,
            GenerationContext context)
        {
            // if we have a contextBaseType, then check that we can do it
            if (contextBaseType != null)
            {
                var ctor = contextBaseType.GetConstructor(baseCallParameterTypes);
                if (ctor == null)
                    return false;
            }
            using (writer.WriteCtor(SpecificationDefinition.Public, schema.Class, parameters, baseCallParameterNames))
            { }
            writer.WriteLine();
            return true;
        }

        protected virtual void WriteDataContextCtors(CodeWriter writer, Database schema, Type contextBaseType, GenerationContext context)
        {
            // the two constructors below have the same prototype, so they are mutually exclusive
            // the base class requires a IVendor
            if (!WriteDataContextCtor(writer, schema, contextBaseType,
                                 new[] { new ParameterDefinition { Name = "connection", Type = typeof(IDbConnection) } },
                                 new[] { "connection", writer.GetNewExpression(writer.GetMethodCallExpression(writer.GetLiteralFullType(context.SchemaLoader.Vendor.GetType()))) },
                                 new[] { typeof(IDbConnection), typeof(IVendor) },
                                 context))
            {
                // OR the base class requires no IVendor
                WriteDataContextCtor(writer, schema, contextBaseType,
                                     new[] { new ParameterDefinition { Name = "connection", Type = typeof(IDbConnection) } },
                                     new[] { "connection" },
                                     new[] { typeof(IDbConnection) },
                                     context);
            }
            // just in case you'd like to specify another vendor than the one who helped generating this file
            WriteDataContextCtor(writer, schema, contextBaseType,
                                 new[] { new ParameterDefinition { Name = "connection", Type = typeof(IDbConnection) } ,
                                 new ParameterDefinition { Name = "vendor", Type = typeof(IVendor) } },
                                 new[] { "connection", "vendor" }, new[] { typeof(IDbConnection), typeof(IVendor) },
                                 context);
        }
    }
}
