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
using System.Linq.Expressions;

#if MONO_STRICT
namespace System.Data.Linq.Sugar
#else
namespace DbLinq.Data.Linq.Sugar
#endif
{
    internal interface IExpressionDispatcher
    {
        /// <summary>
        /// Registers the first table. Extracts the table type and registeres the piece
        /// </summary>
        /// <param name="requestingExpression"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        Expression CreateTableExpression(Expression requestingExpression, BuilderContext builderContext);

        /// <summary>
        /// Entry point for Analyzis
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="parameter"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        Expression Analyze(Expression expression, Expression parameter, BuilderContext builderContext);

        /// <summary>
        /// Builds the upper select clause
        /// </summary>
        /// <param name="selectExpression"></param>
        /// <param name="builderContext"></param>
        void BuildSelect(Expression selectExpression, BuilderContext builderContext);

        /// <summary>
        /// Builds a Row builder, based on a given list of parameters
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="parameters"></param>
        /// <param name="builderContext"></param>
        /// <returns></returns>
        LambdaExpression BuildTableReader(Type tableType, IList<string> parameters, BuilderContext builderContext);
    }
}