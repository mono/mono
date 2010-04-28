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
    #region Namespaces.

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    #endregion Namespaces.

    internal enum CountOption
    {
        None,

        ValueOnly,

        InlineAll
    }

    internal abstract class ResourceExpression : Expression
    {
        #region Fields.

        protected readonly Expression source;

        protected InputReferenceExpression inputRef;

        private List<string> expandPaths;

        private CountOption countOption;

        private Dictionary<ConstantExpression, ConstantExpression> customQueryOptions;

        private ProjectionQueryOptionExpression projection;

        #endregion Fields.

        internal ResourceExpression(Expression source, ExpressionType nodeType, Type type, List<string> expandPaths, CountOption countOption, Dictionary<ConstantExpression, ConstantExpression> customQueryOptions, ProjectionQueryOptionExpression projection)
            : base(nodeType, type)
        {
            this.expandPaths = expandPaths ?? new List<string>();
            this.countOption = countOption;
            this.customQueryOptions = customQueryOptions ?? new Dictionary<ConstantExpression, ConstantExpression>(ReferenceEqualityComparer<ConstantExpression>.Instance);
            this.projection = projection;
            this.source = source;
        }

        abstract internal ResourceExpression CreateCloneWithNewType(Type type);

        abstract internal bool HasQueryOptions { get; }

        internal abstract Type ResourceType { get; }

        abstract internal bool IsSingleton { get; }

        internal virtual List<string> ExpandPaths
        {
            get { return this.expandPaths; }
            set { this.expandPaths = value; }
        }

        internal virtual CountOption CountOption
        {
            get { return this.countOption; }
            set { this.countOption = value; }
        }

        internal virtual Dictionary<ConstantExpression, ConstantExpression> CustomQueryOptions
        {
            get { return this.customQueryOptions; }
            set { this.customQueryOptions = value; }
        }

        internal ProjectionQueryOptionExpression Projection
        {
            get { return this.projection; }
            set { this.projection = value; }
        }

        internal Expression Source
        {
            get
            {
                return this.source;
            }
        }

        internal InputReferenceExpression CreateReference()
        {
            if (this.inputRef == null)
            {
                this.inputRef = new InputReferenceExpression(this);
            }

            return this.inputRef;
        }
    }
}
