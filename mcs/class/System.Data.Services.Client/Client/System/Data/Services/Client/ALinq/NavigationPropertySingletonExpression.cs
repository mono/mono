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
    #region Private fields.

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;

    #endregion Private fields.

    internal class NavigationPropertySingletonExpression : ResourceExpression
    {
        #region Private fields.

        private readonly Expression memberExpression;

        private readonly Type resourceType;

        #endregion Private fields.

        internal NavigationPropertySingletonExpression(Type type, Expression source, Expression memberExpression, Type resourceType, List<string> expandPaths, CountOption countOption, Dictionary<ConstantExpression, ConstantExpression> customQueryOptions, ProjectionQueryOptionExpression projection)
            : base(source, (ExpressionType)ResourceExpressionType.ResourceNavigationPropertySingleton, type, expandPaths, countOption, customQueryOptions, projection)
        {
            Debug.Assert(memberExpression != null, "memberExpression != null");
            Debug.Assert(resourceType != null, "resourceType != null");

            this.memberExpression = memberExpression;
            this.resourceType = resourceType;
        }

        internal MemberExpression MemberExpression
        {
            get
            {
                return (MemberExpression)this.memberExpression;
            }
        }

        internal override Type ResourceType
        {
            get { return this.resourceType; }
        }

        internal override bool IsSingleton
        {
            get { return true; }
        }

        internal override bool HasQueryOptions
        {
            get
            {
                return this.ExpandPaths.Count > 0 ||
                    this.CountOption == CountOption.InlineAll || 
                    this.CustomQueryOptions.Count > 0  || 
                    this.Projection != null;
            }
        }

        internal override ResourceExpression CreateCloneWithNewType(Type type)
        {
            return new NavigationPropertySingletonExpression(
                type, 
                this.source, 
                this.MemberExpression,
                TypeSystem.GetElementType(type),
                this.ExpandPaths.ToList(),
                this.CountOption,
                this.CustomQueryOptions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                this.Projection);
        }
    }
}