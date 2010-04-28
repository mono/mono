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
        
    [DebuggerDisplay("InputReferenceExpression -> {Type}")]
    internal sealed class InputReferenceExpression : Expression
    {
        private ResourceExpression target;

        internal InputReferenceExpression(ResourceExpression target)
            : base((ExpressionType)ResourceExpressionType.InputReference, target.ResourceType)
        {
            Debug.Assert(target != null, "Target resource set cannot be null");
            this.target = target;
        }

        internal ResourceExpression Target
        { 
            get { return this.target; }
        }

        internal void OverrideTarget(ResourceSetExpression newTarget)
        {
            Debug.Assert(newTarget != null, "Resource set cannot be null");
            Debug.Assert(newTarget.ResourceType.Equals(this.Type), "Cannot reference a resource set with a different resource type");

            this.target = newTarget;
        }
    }
}
