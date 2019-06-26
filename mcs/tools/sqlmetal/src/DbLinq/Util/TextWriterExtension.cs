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
using System.Linq.Expressions;

namespace DbLinq.Util
{
    internal static class TextWriterExtension
    {
        /// <summary>
        /// Writes an Expression to the given textWriter
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="expression"></param>
        public static void WriteExpression(this TextWriter textWriter, Expression expression)
        {
            try
            {
                var rawLines = new List<string>(Write(expression, string.Empty, 0));
                rawLines.Insert(0, string.Empty);
                var lines = rawLines.ToArray();
                textWriter.WriteLine(string.Join(Environment.NewLine, lines));
            }
            // we just ignore NREs
            catch (NullReferenceException)
            {
            }
        }

        /// <summary>
        /// Expression visitor. Calls correct method, depending on expression real type
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="header"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private static IList<string> Write(Expression expression, string header, int depth)
        {
            if (expression == null)
                return new[] { WriteLiteral("(null)", header, depth) };
            if (expression is BinaryExpression)
                return WriteEx((BinaryExpression)expression, header, depth);
            if (expression is ConditionalExpression)
                return WriteEx((ConditionalExpression)expression, header, depth);
            if (expression is ConstantExpression)
                return WriteEx((ConstantExpression)expression, header, depth);
            if (expression is InvocationExpression)
                return WriteEx((InvocationExpression)expression, header, depth);
            if (expression is LambdaExpression)
                return WriteEx((LambdaExpression)expression, header, depth);
            if (expression is MemberExpression)
                return WriteEx((MemberExpression)expression, header, depth);
            if (expression is MethodCallExpression)
                return WriteEx((MethodCallExpression)expression, header, depth);
            if (expression is NewExpression)
                return WriteEx((NewExpression)expression, header, depth);
            if (expression is NewArrayExpression)
                return WriteEx((NewArrayExpression)expression, header, depth);
            if (expression is MemberInitExpression)
                return WriteEx((MemberInitExpression)expression, header, depth);
            if (expression is ListInitExpression)
                return WriteEx((ListInitExpression)expression, header, depth);
            if (expression is ParameterExpression)
                return WriteEx((ParameterExpression)expression, header, depth);
            if (expression is TypeBinaryExpression)
                return WriteEx((TypeBinaryExpression)expression, header, depth);
            if (expression is UnaryExpression)
                return WriteEx((UnaryExpression)expression, header, depth);

            return new[] { WriteHeader(expression, header, depth) };
        }

        #region typed Expression writer

        private static IList<string> WriteEx(BinaryExpression expression, string header, int depth)
        {
            var lines = new List<string> { WriteHeader(expression, header, depth++) };
            lines.AddRange(Write(expression.Left, "Left ", depth));
            lines.AddRange(Write(expression.Right, "Right", depth));
            return lines;
        }

        private static IList<string> WriteEx(ConditionalExpression expression, string header, int depth)
        {
            var lines = new List<string> { WriteHeader(expression, header, depth++) };
            lines.AddRange(Write(expression.Test, "If  ", depth));
            lines.AddRange(Write(expression.IfTrue, "Then", depth));
            lines.AddRange(Write(expression.IfFalse, "Else", depth));
            return lines;
        }

        private static IList<string> WriteEx(ConstantExpression expression, string header, int depth)
        {
            var lines = new List<string>
                            {
                                WriteHeader(expression, header, depth++),
                                WriteLiteral(expression.Value, "Value", depth)
                            };
            return lines;
        }

        private static IList<string> WriteEx(InvocationExpression expression, string header, int depth)
        {
            var lines = new List<string> { WriteHeader(expression, header, depth++) };
            lines.AddRange(Write(expression.Expression, "Call", depth));
            for (int i = 0; i < expression.Arguments.Count; i++)
                lines.AddRange(Write(expression.Arguments[i], string.Format("#{0:0##}", i), depth));
            return lines;
        }

        private static IList<string> WriteEx(LambdaExpression expression, string header, int depth)
        {
            var lines = new List<string> { WriteHeader(expression, header, depth++) };
            lines.AddRange(Write(expression.Body, "Call", depth));
            for (int i = 0; i < expression.Parameters.Count; i++)
                lines.AddRange(Write(expression.Parameters[i], string.Format("#{0:0##}", i), depth));
            return lines;
        }

        private static IList<string> WriteEx(MemberExpression expression, string header, int depth)
        {
            var lines = new List<string> { WriteHeader(expression, header, depth++) };
            lines.AddRange(Write(expression.Expression, "Object", depth));
            lines.Add(WriteLiteral(expression.Member.Name, "Member", depth));
            return lines;
        }

        private static IList<string> WriteEx(MethodCallExpression expression, string header, int depth)
        {
            var lines = new List<string> { WriteHeader(expression, header, depth++) };
            lines.AddRange(Write(expression.Object, "Object", depth));
            lines.Add(WriteLiteral(expression.Method.Name, "Method", depth));
            for (int i = 0; i < expression.Arguments.Count; i++)
                lines.AddRange(Write(expression.Arguments[i], string.Format("#{0:0####}", i), depth));
            return lines;
        }

        private static IList<string> WriteEx(NewExpression expression, string header, int depth)
        {
            var lines = new List<string>
                            {
                                WriteHeader(expression, header, depth++),
                                WriteLiteral(expression.Constructor.Name, "Ctor", depth)
                            };
            for (int i = 0; i < expression.Arguments.Count; i++)
                lines.AddRange(Write(expression.Arguments[i], string.Format("#{0:0##}", i), depth));
            if (expression.Members != null)
            {
                for (int i = 0; i < expression.Members.Count; i++)
                    lines.Add(WriteLiteral(expression.Members[i].Name, string.Format("M{0:0##}", i), depth));
            }
            return lines;
        }

        private static IList<string> WriteEx(NewArrayExpression expression, string header, int depth)
        {
            var lines = new List<string> { WriteHeader(expression, header, depth++) };
            for (int i = 0; i < expression.Expressions.Count; i++)
                lines.AddRange(Write(expression.Expressions[i], string.Format("#{0:0##}", i), depth));
            return lines;
        }

        private static IList<string> WriteEx(MemberInitExpression expression, string header, int depth)
        {
            var lines = new List<string> { WriteHeader(expression, header, depth++) };
            lines.AddRange(Write(expression.NewExpression, "New", depth));
            for (int i = 0; i < expression.Bindings.Count; i++)
                lines.Add(WriteLiteral(expression.Bindings[i].Member.Name, string.Format("B{0:0##}", i), depth));
            return lines;
        }

        private static IList<string> WriteEx(ListInitExpression expression, string header, int depth)
        {
            var lines = new List<string> { WriteHeader(expression, header, depth++) };
            lines.AddRange(Write(expression.NewExpression, "New", depth));
            for (int i = 0; i < expression.Initializers.Count; i++)
            {
                lines.Add(WriteLiteral(expression.Initializers[i].AddMethod.Name, string.Format("Method{0:0##}", i), depth));
                for (int j = 0; j < expression.Initializers[i].Arguments.Count; j++)
                {
                    lines.AddRange(Write(expression.Initializers[i].Arguments[j], string.Format("#{0:0##}", j), depth + 1));
                }
            }
            return lines;
        }

        private static IList<string> WriteEx(ParameterExpression expression, string header, int depth)
        {
            var lines = new List<string>
                            {
                                WriteHeader(expression, header, depth++),
                                WriteLiteral(expression.Name, "Parameter", depth)
                            };
            return lines;
        }

        private static IList<string> WriteEx(TypeBinaryExpression expression, string header, int depth)
        {
            var lines = new List<string> { WriteHeader(expression, header, depth++) };
            lines.AddRange(Write(expression.Expression, "Expression", depth));
            lines.Add(WriteLiteral(expression.TypeOperand.Name, "Type      ", depth));
            return lines;
        }

        private static IList<string> WriteEx(UnaryExpression expression, string header, int depth)
        {
            var lines = new List<string>
                            {
                                WriteHeader(expression, header, depth++),
                                WriteLiteral(expression.Method != null ? expression.Method.Name : null, "Method ", depth)
                            };
            lines.AddRange(Write(expression.Operand, "Operand", depth));
            return lines;
        }

        private static string WriteHeader(Expression expression, string header, int depth)
        {
            return string.Format("{0}{1} {2} ({3})", GetPrefix(depth), header, expression.NodeType, expression.GetType().Name);
        }

        private static string WriteLiteral(object value, string header, int depth)
        {
            return string.Format("{0}{1}: {2}", GetPrefix(depth), header, value);
        }

        private static string GetPrefix(int depth)
        {
            return string.Empty.PadRight(depth * 2, '.');
        }

        #endregion
    }
}
