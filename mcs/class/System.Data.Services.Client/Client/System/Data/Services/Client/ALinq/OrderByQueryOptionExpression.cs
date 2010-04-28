//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.

namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class OrderByQueryOptionExpression : QueryOptionExpression
    {
        private List<Selector> selectors;

        internal OrderByQueryOptionExpression(Type type, List<Selector> selectors)
            : base((ExpressionType)ResourceExpressionType.OrderByQueryOption, type)
        {
            this.selectors = selectors; 
        }

        internal List<Selector> Selectors
        {
            get
            {
                return this.selectors;
            }
        }

        internal struct Selector
        {
            internal readonly Expression Expression;

            internal readonly bool Descending;

            internal Selector(Expression e, bool descending)
            {
                this.Expression = e;
                this.Descending = descending;
            }
        }
    }
}
