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
    using System.Diagnostics;
    using System.Linq.Expressions;

    [DebuggerDisplay("TakeQueryOptionExpression {TakeAmount}")]
    internal class TakeQueryOptionExpression : QueryOptionExpression
    {
        private ConstantExpression takeAmount;

        internal TakeQueryOptionExpression(Type type, ConstantExpression takeAmount)
            : base((ExpressionType)ResourceExpressionType.TakeQueryOption, type)
        {
            this.takeAmount = takeAmount;
        }

        internal ConstantExpression TakeAmount
        {
            get
            {
                return this.takeAmount;
            }
        }

        internal override QueryOptionExpression ComposeMultipleSpecification(QueryOptionExpression previous)
        {
            Debug.Assert(previous != null, "other != null");
            Debug.Assert(previous.GetType() == this.GetType(), "other.GetType == this.GetType() -- otherwise it's not the same specification");
            Debug.Assert(this.takeAmount != null, "this.takeAmount != null");
            Debug.Assert(
                this.takeAmount.Type == typeof(int),
                "this.takeAmount.Type == typeof(int) -- otherwise it wouldn't have matched the Enumerable.Take(source, int count) signature");
            int thisValue = (int)this.takeAmount.Value;
            int previousValue = (int)((TakeQueryOptionExpression)previous).takeAmount.Value;
            return (thisValue < previousValue) ? this : previous;
        }
    }
}
