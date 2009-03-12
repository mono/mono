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
using DbLinq.Util;
using DbMetal.Generator;

namespace DbMetal.Generator
{
    /// <summary>
    /// Base class for writing code.
    /// Divided in 3 Parts:
    /// - Code line writing (with indentation)
    /// - Code formatting (returning a literal type)
    /// - Code writing (comment line, field, property, event...)
    /// </summary>
    public abstract class CodeWriter : TextWriter
    {
        // required by TextWriter
        public override Encoding Encoding
        {
            get
            {
                return TextWriter.Encoding;
            }
        }

        public string IndentationPattern { get; set; }

        private readonly StringBuilder buffer = new StringBuilder(10 << 10);
        private int currentindentation = 0;

        protected TextWriter TextWriter;

        protected CodeWriter(TextWriter textWriter)
        {
            IndentationPattern = "\t";
            TextWriter = textWriter;
        }

        #region Writer

        protected bool IsFullLine()
        {
            int endIndex = buffer.Length - CoreNewLine.Length;
            if (endIndex < 0)
                return false;
            for (int i = 0; i < CoreNewLine.Length; i++)
            {
                if (buffer[endIndex + i] != CoreNewLine[i])
                    return false;
            }
            return true;
        }

        protected string GetLine()
        {
            string line = buffer.ToString();
            buffer.Remove(0, buffer.Length);
            return line;
        }

        protected abstract bool MustIndent(string line);
        protected abstract bool MustUnindent(string line);

        /// <summary>
        /// In the end, all output comes to this
        /// </summary>
        /// <param name="value"></param>
        public override void Write(char value)
        {
            buffer.Append(value);
            if (IsFullLine())
            {
                string line = GetLine();
                string rawLine = Trim(line);
                // unindent before...
                if (MustUnindent(rawLine))
                    currentindentation--;
                WriteLine(rawLine, currentindentation);
                // indent after
                if (MustIndent(rawLine))
                    currentindentation++;
            }
        }

        protected virtual string Trim(string line)
        {
            return line.Trim();
        }

        protected virtual void WriteLine(string rawLine, int indentation)
        {
            if (!string.IsNullOrEmpty(rawLine))
            {
                for (int indentationCount = 0; indentationCount < indentation; indentationCount++)
                {
                    TextWriter.Write(IndentationPattern);
                }
            }
            TextWriter.WriteLine(rawLine);
        }

        public virtual string GetEnumType(string name)
        {
            return name;
        }

        public abstract IDisposable WriteEnum(SpecificationDefinition specificationDefinition, string name);

        public virtual void WriteEnum(SpecificationDefinition specificationDefinition, string name, IDictionary<string, int> values)
        {
            using (WriteEnum(specificationDefinition, name))
            {
                var orderedValues = from nv in values orderby nv.Value select nv;
                int currentValue = 1;
                foreach (var nameValue in orderedValues)
                {
                    if (nameValue.Value == currentValue)
                        WriteLine(string.Format("{0},", nameValue.Key));
                    else
                    {
                        currentValue = nameValue.Value;
                        WriteLine(string.Format("{0} = {1},", nameValue.Key, nameValue.Value));
                    }
                    currentValue++;
                }
            }
        }

        #endregion

        #region Code generation

        // A language sometimes generates complementary text, such as "{" and "}", or "#region"/"#endregion"

        protected class NestedInstruction : IDisposable
        {
            private readonly Action endAction;

            public NestedInstruction(Action end)
            {
                endAction = end;
            }

            public void Dispose()
            {
                endAction();
            }
        }

        /// <summary>
        /// Registers an "end block" (written on Dispose() call)
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        protected IDisposable EndAction(Action end)
        {
            return new NestedInstruction(end);
        }

        #endregion

        #region Code generation - Language write

        public abstract void WriteCommentLine(string line);
        public virtual void WriteCommentLines(string comments)
        {
            string[] commentLines = comments.Split('\n');
            foreach (string commentLine in commentLines)
            {
                WriteCommentLine(commentLine.TrimEnd());
            }
        }

        /// <summary>
        /// Registers namespace to be written
        /// </summary>
        /// <param name="name"></param>
        public abstract void WriteUsingNamespace(string name);
        public abstract IDisposable WriteNamespace(string name);
        public abstract IDisposable WriteClass(SpecificationDefinition specificationDefinition, string name,
                                               string baseClass, params string[] interfaces);

        public abstract IDisposable WriteRegion(string name);
        public abstract IDisposable WriteAttribute(AttributeDefinition attributeDefinition);

        public abstract IDisposable WriteCtor(SpecificationDefinition specificationDefinition, string name,
                                              ParameterDefinition[] parameters, IList<string> baseCallParameters);
        public abstract IDisposable WriteMethod(SpecificationDefinition specificationDefinition, string name, Type returnType,
                                                params ParameterDefinition[] parameters);

        public abstract IDisposable WriteProperty(SpecificationDefinition specificationDefinition, string name, string propertyType);
        public abstract void WriteAutomaticPropertyGetSet();
        public abstract IDisposable WritePropertyGet();
        public abstract IDisposable WritePropertySet();

        public abstract void WritePropertyWithBackingField(SpecificationDefinition specificationDefinition, string name, string propertyType, bool privateSetter);
        public virtual void WritePropertyWithBackingField(SpecificationDefinition specificationDefinition, string name, string propertyType)
        {
            WritePropertyWithBackingField(specificationDefinition, name, propertyType, false);
        }

        public abstract void WriteField(SpecificationDefinition specificationDefinition, string name, string fieldType);

        public abstract void WriteEvent(SpecificationDefinition specificationDefinition, string name, string eventDelegate);

        public abstract IDisposable WriteIf(string expression);

        #endregion

        #region Code generation - Language construction

        public abstract string GetCastExpression(string value, string castType, bool hardCast);

        public virtual string GetLiteralValue(object value)
        {
            if (value == null)
                return GetNullExpression();
            if (value is string)
                return string.Format("\"{0}\"", value);
            return value.ToString();
        }

        public virtual string GetLiteralType(Type type)
        {
            return type.Name;
        }

        public virtual string GetLiteralFullType(Type type)
        {
            return type.FullName;
        }

        public virtual string GetMemberExpression(string obj, string member)
        {
            return string.Format("{0}.{1}", obj, member);
        }

        public virtual string GetReturnStatement(string expression)
        {
            if (expression == null)
                return GetStatement("return");
            return GetStatement(string.Format("return {0}", expression));
        }

        /// <summary>
        /// Returns the specified variable as a safe expression
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract string GetVariableExpression(string name);

        public virtual string GetNewExpression(string ctor)
        {
            return string.Format("new {0}", ctor);
        }

        public virtual string GetThisExpression()
        {
            return "this";
        }

        public virtual string GetDeclarationExpression(string variable, string type)
        {
            return string.Format("{0} {1}", type, variable);
        }

        public virtual string GetAssignmentExpression(string variable, string expression)
        {
            return string.Format("{0} = {1}", variable, expression);
        }

        public abstract string GetArray(string array, string literalIndex);

        public virtual string GetMethodCallExpression(string method, params string[] literalParameters)
        {
            return string.Format("{0}({1})", method, string.Join(", ", literalParameters));
        }

        public virtual string GetStatement(string expression)
        {
            return expression;
        }

        public abstract string GetPropertySetValueExpression();

        public abstract string GetNullExpression();

        public abstract string GetGenericName(string baseName, string type);

        public abstract string GetDifferentExpression(string a, string b);
        public abstract string GetEqualExpression(string a, string b);

        public abstract string GetXOrExpression(string a, string b);
        public abstract string GetAndExpression(string a, string b);

        public abstract string GetTernaryExpression(string conditionExpression, string trueExpression, string falseExpression);

        public abstract string GetNullValueExpression(string literalType);

        #endregion

        /// <summary>
        /// Returns a code that throw the given expression
        /// </summary>
        /// <param name="throwExpression"></param>
        /// <returns></returns>
        public abstract string GetThrowStatement(string throwExpression);

        /// <summary>
        /// Returns a declaration and assignement expression
        /// </summary>
        /// <param name="variableType"></param>
        /// <param name="variableName"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public abstract string GetVariableDeclarationInitialization(string variableType, string variableName, string expression);

        /// <summary>
        /// Writes the raw if.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public abstract void WriteRawIf(string expression);

        /// <summary>
        /// Writes the raw else.
        /// </summary>
        public abstract void WriteRawElse();

        /// <summary>
        /// Writes the raw endif.
        /// </summary>
        public abstract void WriteRawEndif();
    }
}
