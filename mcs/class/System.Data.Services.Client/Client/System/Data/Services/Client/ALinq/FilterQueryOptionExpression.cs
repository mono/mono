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
    using System.Linq.Expressions;

    internal class FilterQueryOptionExpression : QueryOptionExpression
    {
        private Expression predicate;

        internal FilterQueryOptionExpression(Type type, Expression predicate)
            : base((ExpressionType)ResourceExpressionType.FilterQueryOption, type)
        {
            this.predicate = predicate;
        }

        internal Expression Predicate
        {
            get
            {
                return this.predicate;
            }
        }
    }
}