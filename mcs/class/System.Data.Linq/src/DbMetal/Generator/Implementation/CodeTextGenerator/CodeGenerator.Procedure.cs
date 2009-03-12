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
using System.Collections.Generic;
using System.Data;
using System.Data.Linq.Mapping;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using Type = System.Type;

namespace DbMetal.Generator.Implementation.CodeTextGenerator
{
    public partial class CodeGenerator
    {
        protected virtual void WriteDataContextProcedures(CodeWriter writer, DbLinq.Schema.Dbml.Database schema, GenerationContext context)
        {
            foreach (var procedure in schema.Functions)
            {
                WriteDataContextProcedure(writer, procedure, context);
            }
        }

        private void WriteDataContextProcedure(CodeWriter writer, Function procedure, GenerationContext context)
        {
            if (procedure == null || procedure.Name == null)
            {
                //Logger.Write(Level.Error, "CodeGenStoredProc: Error L33 Invalid storedProcedure object");
                writer.WriteCommentLine("error L33 Invalid storedProcedure object");
                return;
            }

            var functionAttribute = NewAttributeDefinition<FunctionAttribute>();
            functionAttribute["Name"] = procedure.Name;
            functionAttribute["IsComposable"] = procedure.IsComposable;

            SpecificationDefinition specifications;
            if (procedure.AccessModifierSpecified)
                specifications = GetSpecificationDefinition(procedure.AccessModifier);
            else
                specifications = SpecificationDefinition.Public;
            if (procedure.ModifierSpecified)
                specifications |= GetSpecificationDefinition(procedure.Modifier);

            using (writer.WriteAttribute(functionAttribute))
            using (writer.WriteMethod(specifications, GetProcedureName(procedure),
                                      GetProcedureType(procedure), GetProcedureParameters(procedure)))
            {
                string result = WriteProcedureBodyMethodCall(writer, procedure, context);
                WriteProcedureBodyOutParameters(writer, procedure, result, context);
                WriteProcedureBodyReturnValue(writer, procedure, result, context);
            }
            writer.WriteLine();
        }

        protected virtual void WriteProcedureBodyReturnValue(CodeWriter writer, DbLinq.Schema.Dbml.Function procedure, string result, GenerationContext context)
        {
            Type returnType = GetProcedureType(procedure);
            if (returnType != null)
                writer.WriteLine(writer.GetReturnStatement(writer.GetCastExpression(writer.GetMemberExpression(result, "ReturnValue"), writer.GetLiteralType(returnType), true)));
        }

        protected virtual void WriteProcedureBodyOutParameters(CodeWriter writer, DbLinq.Schema.Dbml.Function procedure, string result, GenerationContext context)
        {
            int parameterIndex = 0;
            foreach (var parameter in procedure.Parameters)
            {
                if (parameter.DirectionOut)
                    WriteProcedureBodyOutParameter(writer, parameter, result, parameterIndex, context);

                parameterIndex++;
            }
        }

        protected virtual void WriteProcedureBodyOutParameter(CodeWriter writer, DbLinq.Schema.Dbml.Parameter parameter, string result, int parameterIndex, GenerationContext context)
        {
            string p = writer.GetMethodCallExpression(writer.GetMemberExpression(result, "GetParameterValue"), parameterIndex.ToString());
            string cp = writer.GetCastExpression(p, parameter.Type, true);
            writer.WriteLine(writer.GetStatement(writer.GetAssignmentExpression(parameter.Name, cp)));
        }

        protected abstract string WriteProcedureBodyMethodCall(CodeWriter writer, DbLinq.Schema.Dbml.Function procedure, GenerationContext context);

        protected virtual string GetProcedureName(DbLinq.Schema.Dbml.Function procedure)
        {
            return procedure.Method ?? procedure.Name;
        }

        protected virtual Type GetProcedureType(DbLinq.Schema.Dbml.Function procedure)
        {
            Type type = null;
            if (procedure.Return != null)
            {
                type = GetType(procedure.Return.Type, false);
            }

            bool isDataShapeUnknown = procedure.ElementType == null
                                      && procedure.BodyContainsSelectStatement
                                      && !procedure.IsComposable;
            if (isDataShapeUnknown)
            {
                //if we don't know the shape of results, and the proc body contains some selects,
                //we have no choice but to return an untyped DataSet.
                //
                //TODO: either parse proc body like microsoft, 
                //or create a little GUI tool which would call the proc with test values, to determine result shape.
                type = typeof(DataSet);
            }
            return type;
        }

        protected virtual ParameterDefinition[] GetProcedureParameters(DbLinq.Schema.Dbml.Function procedure)
        {
            var parameters = new List<ParameterDefinition>();
            foreach (var parameter in procedure.Parameters)
                parameters.Add(GetProcedureParameter(parameter));
            return parameters.ToArray();
        }

        protected virtual ParameterDefinition GetProcedureParameter(DbLinq.Schema.Dbml.Parameter parameter)
        {
            var parameterDefinition = new ParameterDefinition();
            parameterDefinition.Name = parameter.Name;
            parameterDefinition.Type = GetType(parameter.Type, false);
            switch (parameter.Direction)
            {
            case DbLinq.Schema.Dbml.ParameterDirection.In:
                parameterDefinition.SpecificationDefinition |= SpecificationDefinition.In;
                break;
            case DbLinq.Schema.Dbml.ParameterDirection.Out:
                parameterDefinition.SpecificationDefinition |= SpecificationDefinition.Out;
                break;
            case DbLinq.Schema.Dbml.ParameterDirection.InOut:
                parameterDefinition.SpecificationDefinition |= SpecificationDefinition.Ref;
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
            parameterDefinition.Attribute = NewAttributeDefinition<ParameterAttribute>();
            parameterDefinition.Attribute["Name"] = parameter.Name;
            parameterDefinition.Attribute["DbType"] = parameter.DbType;
            return parameterDefinition;
        }
    }
}