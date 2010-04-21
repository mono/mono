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
using System.Text;

namespace DbMetal.Generator.Implementation.CodeTextGenerator
{
#if !MONO_STRICT
    public
#endif
    class CSCodeGenerator : CodeGenerator
    {
        public override string LanguageCode { get { return "obsolete-c#"; } }
        public override string Extension { get { return ".obsolete-cs"; } }

        protected override CodeWriter CreateCodeWriter(TextWriter textWriter)
        {
            return new CSCodeWriter(textWriter, false);
        }

        protected override void WriteDataContextTable(CodeWriter writer, DbLinq.Schema.Dbml.Table table)
        {
            writer.WriteLine("public Table<{1}> {0} {{ get {{ return GetTable<{1}>(); }} }}",
                             table.Member, table.Type.Name);
        }

        protected override string WriteProcedureBodyMethodCall(CodeWriter writer, DbLinq.Schema.Dbml.Function procedure, GenerationContext context)
        {
            // picrap: there may be some more elegant ways to invoke a stored procedure, because ExecuteMethodCall is 
            //         for internal use only
            const string result = "result";
            var parametersBuilder = new StringBuilder();
            foreach (var parameter in procedure.Parameters)
            {
                if (parameter.DirectionIn)
                    parametersBuilder.AppendFormat(", {0}", parameter.Name);
            }
            writer.WriteLine(string.Format("var {0} = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(){1});",
                                           result, parametersBuilder));
            return result;
        }
    }
}
