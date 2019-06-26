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
using System.IO;
using System.Linq;
using System.Text;

namespace DbMetal.Generator.Implementation.CodeTextGenerator
{
    /// <summary>
    /// C# Code writer
    /// </summary>
    class CSCodeWriter : CodeWriter
    {
        public string Indent { get; set; }
        public string Unindent { get; set; }

        private readonly bool trimSpaces;

        public CSCodeWriter(TextWriter textWriter, bool trimSpaces)
            : base(textWriter)
        {
            this.trimSpaces = trimSpaces;
            Indent = "{";
            Unindent = "}";
        }

        public CSCodeWriter(TextWriter textWriter)
            : this(textWriter, true)
        {
        }

        protected override bool MustIndent(string line)
        {
            return line.StartsWith(Indent);
        }

        protected override bool MustUnindent(string line)
        {
            return line.StartsWith(Unindent);
        }

        protected override string Trim(string line)
        {
            if (trimSpaces)
                return line.Trim();
            return line.TrimEnd();
        }

        #region Code generation - Language write

        public override void WriteCommentLine(string line)
        {
            WriteLine("// {0}", line);
        }

        protected virtual IDisposable WriteBrackets()
        {
            WriteLine("{");
            return EndAction(() => WriteLine("}"));
        }

        IDisposable WriteBrackets(string endif)
        {
            WriteLine("{");
            return EndAction(() => { WriteLine("}"); WriteLine(endif); });
        }

        /// <summary>
        /// We keep track of namespaces written already once
        /// </summary>
        private readonly IList<string> writtenNamespaces = new List<string>();

        public override void WriteUsingNamespace(string name)
        {
            if (writtenNamespaces.Contains(name))
                return;

            writtenNamespaces.Add(name);
            WriteLine("using {0};", name);
        }

        public override IDisposable WriteNamespace(string name)
        {
            WriteLine("namespace {0}", name);
            return WriteBrackets();
        }

        public override IDisposable WriteClass(SpecificationDefinition specificationDefinition, string name, string baseClass, params string[] interfaces)
        {
            var classLineBuilder = new StringBuilder(1024);

            classLineBuilder.Append(GetProtectionSpecifications(specificationDefinition));
            classLineBuilder.Append(GetDomainSpecifications(specificationDefinition));
            classLineBuilder.Append(GetInheritanceSpecifications(specificationDefinition));

            var bases = new List<string>();
            if (!string.IsNullOrEmpty(baseClass))
                bases.Add(baseClass);
            bases.AddRange(interfaces);

            classLineBuilder.AppendFormat("class {0}", name);
            if (bases.Count > 0)
            {
                classLineBuilder.Append(" : ");
                classLineBuilder.Append(string.Join(", ", bases.ToArray()));
            }
            WriteLine(classLineBuilder.ToString());

            return WriteBrackets();
        }

        public override IDisposable WriteRegion(string name)
        {
            WriteLine("#region {0}", name);
            WriteLine();
            return EndAction(delegate { WriteLine(); WriteLine("#endregion"); WriteLine(); });
        }

        public override IDisposable WriteAttribute(AttributeDefinition attributeDefinition)
        {
            if (attributeDefinition != null)
                WriteLine(GetAttribute(attributeDefinition));
            return null;
        }

        protected virtual IDisposable WriteGeneralMethod(SpecificationDefinition specificationDefinition, string name,
                                                bool hasReturnType, Type returnType,
                                                IList<ParameterDefinition> parameters, IList<string> baseCallParameters)
        {
            bool monoStrict = baseCallParameters != null && baseCallParameters.Count > 0 &&
                parameters.Any(p => p.Type == typeof(DbLinq.Vendor.IVendor));

            var methodLineBuilder = new StringBuilder(1024);

            if (monoStrict)
                methodLineBuilder.Append("#if !MONO_STRICT")
                    .AppendLine();
            methodLineBuilder.Append(GetProtectionSpecifications(specificationDefinition));
            methodLineBuilder.Append(GetDomainSpecifications(specificationDefinition));
            methodLineBuilder.Append(GetInheritanceSpecifications(specificationDefinition));

            if (hasReturnType)
                methodLineBuilder.AppendFormat("{0} {1}(", GetLiteralType(returnType) ?? "void", name);
            else
                methodLineBuilder.AppendFormat("{0}(", name);
            var literalParameters = new List<string>();
            foreach (var parameter in parameters)
            {
                string literalParameter = string.Format("{0}{3}{1} {2}",
                                                        parameter.Attribute != null ? GetAttribute(parameter.Attribute) + " " : string.Empty,
                                                        parameter.LiteralType ?? GetLiteralType(parameter.Type),
                                                        parameter.Name,
                                                        GetDirectionSpecifications(parameter.SpecificationDefinition));
                literalParameters.Add(literalParameter);
            }
            methodLineBuilder.AppendFormat("{0})", string.Join(", ", literalParameters.ToArray()));
            if (baseCallParameters != null && baseCallParameters.Count > 0)
            {
                methodLineBuilder.AppendLine();
                bool strictArgs = parameters.Count != baseCallParameters.Count;
                if (strictArgs)
                    methodLineBuilder.Append("#if MONO_STRICT")
                        .AppendLine()
                        .AppendFormat("\t: base({0})", string.Join(", ", baseCallParameters.Take(parameters.Count).ToArray()))
                        .AppendLine()
                        .Append("#else   // MONO_STRICT")
                        .AppendLine();
                methodLineBuilder.AppendFormat("\t: base({0})", string.Join(", ", baseCallParameters.ToArray()));
                if (strictArgs)
                    methodLineBuilder.AppendLine()
                        .Append("#endif  // MONO_STRICT");
            }
            WriteLine(methodLineBuilder.ToString());
            return monoStrict ? WriteBrackets("#endif  // !MONO_STRICT") : WriteBrackets();
        }

        public override IDisposable WriteCtor(SpecificationDefinition specificationDefinition, string name,
                                                ParameterDefinition[] parameters, IList<string> baseCallParameters)
        {
            return WriteGeneralMethod(specificationDefinition, name, false, null, parameters, baseCallParameters);
        }

        public override IDisposable WriteMethod(SpecificationDefinition specificationDefinition, string name, Type returnType,
                                                params ParameterDefinition[] parameters)
        {
            return WriteGeneralMethod(specificationDefinition, name, true, returnType, parameters, null);
        }

        protected void WriteFieldOrProperty(SpecificationDefinition specificationDefinition, string name, string memberType)
        {
            var methodLineBuilder = new StringBuilder(1024);

            methodLineBuilder.Append(GetProtectionSpecifications(specificationDefinition));
            methodLineBuilder.Append(GetDomainSpecifications(specificationDefinition));
            methodLineBuilder.Append(GetInheritanceSpecifications(specificationDefinition));
            methodLineBuilder.Append(GetSpecifications(specificationDefinition & SpecificationDefinition.Event));

            methodLineBuilder.AppendFormat("{0} {1}", memberType, GetVariableExpression(name));

            Write(methodLineBuilder.ToString());
        }

        public override IDisposable WriteProperty(SpecificationDefinition specificationDefinition, string name, string propertyType)
        {
            WriteFieldOrProperty(specificationDefinition, name, propertyType);
            WriteLine();
            return WriteBrackets();
        }

        public override void WriteAutomaticPropertyGetSet()
        {
            WriteLine("get; set;");
        }
        public override IDisposable WritePropertyGet()
        {
            WriteLine("get");
            return WriteBrackets();
        }

        public override IDisposable WritePropertySet()
        {
            WriteLine("set");
            return WriteBrackets();
        }

        public override void WriteField(SpecificationDefinition specificationDefinition, string name, string fieldType)
        {
            WriteFieldOrProperty(specificationDefinition, name, fieldType);
            WriteLine(";");
        }

        public override void WritePropertyWithBackingField(SpecificationDefinition specificationDefinition, string name, string propertyType, bool privateSetter)
        {
            WriteFieldOrProperty(specificationDefinition, name, propertyType);
            WriteLine("{{ get; {0}set; }}", privateSetter ? "private " : string.Empty);
        }

        public override void WriteEvent(SpecificationDefinition specificationDefinition, string name, string eventDelegate)
        {
            WriteFieldOrProperty(specificationDefinition | SpecificationDefinition.Event, name, eventDelegate);
            WriteLine(";");
        }

        public override IDisposable WriteIf(string expression)
        {
            WriteLine("if ({0})", expression);
            return WriteBrackets();
        }


        /// <summary>
        /// Writes the raw if.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public override void WriteRawIf(string expression)
        {
            WriteLine("if ({0})", expression);
            WriteLine("{");
        }

        /// <summary>
        /// Writes the raw else.
        /// </summary>
        public override void WriteRawElse()
        {
            WriteLine("}");
            WriteLine("else");
            WriteLine("{");
        }

        /// <summary>
        /// Writes the raw endif.
        /// </summary>
        public override void WriteRawEndif()
        {
            WriteLine("}");
        }

        /// <summary>
        /// Writes the enum.
        /// </summary>
        /// <param name="specificationDefinition">The specification definition.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public override IDisposable WriteEnum(SpecificationDefinition specificationDefinition, string name)
        {
            WriteLine("{0}enum {1}", GetProtectionSpecifications(specificationDefinition), name);
            return WriteBrackets();
        }

        #endregion

        #region Code generation - Language construction

        protected bool HasSpecification(SpecificationDefinition specificationDefinition, SpecificationDefinition test)
        {
            return (specificationDefinition & test) != 0;
        }

        protected virtual string GetSpecifications(SpecificationDefinition specificationDefinition)
        {
            var literalSpecifications = new List<string>();
            if (HasSpecification(specificationDefinition, SpecificationDefinition.Internal))
                literalSpecifications.Add("internal");
            if (HasSpecification(specificationDefinition, SpecificationDefinition.Private))
                literalSpecifications.Add("private");
            if (HasSpecification(specificationDefinition, SpecificationDefinition.Protected))
                literalSpecifications.Add("protected");

            if (HasSpecification(specificationDefinition, SpecificationDefinition.Abstract))
                literalSpecifications.Add("abstract");
            if (HasSpecification(specificationDefinition, SpecificationDefinition.Virtual))
                literalSpecifications.Add("virtual");
            if (HasSpecification(specificationDefinition, SpecificationDefinition.Override))
                literalSpecifications.Add("override");
            if (HasSpecification(specificationDefinition, SpecificationDefinition.New))
                literalSpecifications.Add("new");
            if (HasSpecification(specificationDefinition, SpecificationDefinition.Static))
                literalSpecifications.Add("static");

            if (HasSpecification(specificationDefinition, SpecificationDefinition.Partial))
                literalSpecifications.Add("partial");

            if (HasSpecification(specificationDefinition, SpecificationDefinition.Out))
            {
                if (HasSpecification(specificationDefinition, SpecificationDefinition.In))
                    literalSpecifications.Add("ref");
                else
                    literalSpecifications.Add("out");
            }

            if (HasSpecification(specificationDefinition, SpecificationDefinition.Event))
                literalSpecifications.Add("event");

            string result = string.Join(" ", literalSpecifications.ToArray());
            if (!string.IsNullOrEmpty(result))
                result += " ";
            return result;
        }

        protected virtual string GetProtectionSpecifications(SpecificationDefinition specificationDefinition)
        {
            string literalSpecifications = GetSpecifications(specificationDefinition & SpecificationDefinition.ProtectionClass);
            if (string.IsNullOrEmpty(literalSpecifications))
                literalSpecifications = "public ";
            return literalSpecifications;
        }

        protected virtual string GetInheritanceSpecifications(SpecificationDefinition specificationDefinition)
        {
            return GetSpecifications(specificationDefinition & SpecificationDefinition.InheritanceClass);
        }

        protected virtual string GetDomainSpecifications(SpecificationDefinition specificationDefinition)
        {
            return GetSpecifications(specificationDefinition & SpecificationDefinition.DomainClass);
        }

        protected virtual string GetDirectionSpecifications(SpecificationDefinition specificationDefinition)
        {
            return GetSpecifications(specificationDefinition & SpecificationDefinition.DirectionClass);
        }

        protected virtual string GetAttribute(AttributeDefinition attributeDefinition)
        {
            if (attributeDefinition.Members.Count == 0)
                return string.Format("[{0}]", attributeDefinition.Name);
            var attributeLineBuilder = new StringBuilder(1024);

            attributeLineBuilder.AppendFormat("[{0}(", attributeDefinition.Name);
            var members = new List<string>();
            foreach (var keyValue in attributeDefinition.Members)
                members.Add(string.Format("{0} = {1}", keyValue.Key, GetLiteralValue(keyValue.Value)));
            attributeLineBuilder.Append(string.Join(", ", members.ToArray()));
            attributeLineBuilder.Append(")]");

            return attributeLineBuilder.ToString();
        }

        public override string GetGenericName(string baseName, string type)
        {
            return string.Format("{0}<{1}>", baseName, type);
        }

        public override string GetCastExpression(string value, string castType, bool hardCast)
        {
            string format = hardCast ? "({1}){0}" : "{0} as {1}";
            string literalCast = string.Format(format, value, castType);
            return literalCast;
        }

        public override string GetLiteralValue(object value)
        {
            if (value is bool)
                return ((bool)value) ? "true" : "false";
            if (value is string)
                return string.Format("\"{0}\"", ((string)value).Replace("\"", "\\\""));
            return base.GetLiteralValue(value);
        }

        public virtual string GetTypeNickName(Type type)
        {
            if (type == typeof(char))
                return "char";
            if (type == typeof(string))
                return "string";

            if (type == typeof(byte))
                return "byte";
            if (type == typeof(sbyte))
                return "sbyte";
            if (type == typeof(short))
                return "short";
            if (type == typeof(ushort))
                return "ushort";
            if (type == typeof(int))
                return "int";
            if (type == typeof(uint))
                return "uint";
            if (type == typeof(long))
                return "long";
            if (type == typeof(ulong))
                return "ulong";

            if (type == typeof(float))
                return "float";
            if (type == typeof(double))
                return "double";
            if (type == typeof(decimal))
                return "decimal";

            if (type == typeof(bool))
                return "bool";
            if (type == typeof(object))
                return "object";

            return type.Name;
        }

        protected static readonly string[] Keywords = 
            {
                "int", "uint","ubyte", "byte", "short", "ushort", "char"
                ,"decimal", "float", "double"
                ,"string", "DateTime"
                , "void", "object"

                ,"private", "protected", "public", "internal"
                ,"override", "virtual", "abstract", "partial", "static", "sealed", "readonly"
                ,"class", "struct", "namespace", "enum", "interface", "using", "const", "enum"

                ,"return", "if", "while", "for", "foreach"
                ,"yield", "break", "goto", "switch", "case", "default"

                , "as", "catch", "continue", "default", "delegate", "do"
                , "else", "false", "true", "fixed", "finally", "in", "is", "lock"
                , "new", "null", "out", "ref", "sizeof", "stackalloc", "throw", "typeof"
            };

        protected virtual bool IsKeyword(string name)
        {
            return Keywords.Contains(name);
        }

        public override string GetVariableExpression(string name)
        {
            if (IsKeyword(name))
                return "@" + name;
            return name;
        }

        public override string GetLiteralType(Type type)
        {
            if (type == null)
                return null;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return string.Format("{0}?", GetTypeNickName(type.GetGenericArguments()[0]));
            return GetTypeNickName(type);
        }

        public override string GetArray(string array, string literalIndex)
        {
            return string.Format("{0}[{1}]", array, literalIndex);
        }

        public override string GetStatement(string expression)
        {
            return expression + ";";
        }

        public override string GetXOrExpression(string a, string b)
        {
            return string.Format("{0} ^ {1}", a, b);
        }

        public override string GetAndExpression(string a, string b)
        {
            return string.Format("{0} && {1}", a, b);
        }

        public override string GetPropertySetValueExpression()
        {
            return "value";
        }

        public override string GetNullExpression()
        {
            return "null";
        }

        public override string GetDifferentExpression(string a, string b)
        {
            return string.Format("{0} != {1}", a, b);
        }

        /// <summary>
        /// Gets the equal expression.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public override string GetEqualExpression(string a, string b)
        {
            return string.Format("{0} == {1}", a, b);
        }

        /// <summary>
        /// Gets the ternary expression.
        /// </summary>
        /// <param name="conditionExpression">The condition expression.</param>
        /// <param name="trueExpression">The true expression.</param>
        /// <param name="falseExpression">The false expression.</param>
        /// <returns></returns>
        public override string GetTernaryExpression(string conditionExpression, string trueExpression, string falseExpression)
        {
            return string.Format("{0} ? {1} : {2}", conditionExpression, trueExpression, falseExpression);
        }

        public override string GetNullValueExpression(string literalType)
        {
            return string.Format("default({0})", literalType);
        }

        /// <summary>
        /// Returns a code that throw the given expression
        /// </summary>
        /// <param name="throwExpression"></param>
        /// <returns></returns>
        public override string GetThrowStatement(string throwExpression)
        {
            return string.Format("throw {0};", throwExpression);
        }

        /// <summary>
        /// Returns a declaration and assignement expression
        /// </summary>
        /// <param name="variableType"></param>
        /// <param name="variableName"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override string GetVariableDeclarationInitialization(string variableType, string variableName, string expression)
        {
            //return string.Format("{0} {1} = {2}", variableType, variableName, expression);
            // we can do this since we generate for C# 3
            return string.Format("var {0} = {1}", variableName, expression);
        }

        #endregion
    }
}
