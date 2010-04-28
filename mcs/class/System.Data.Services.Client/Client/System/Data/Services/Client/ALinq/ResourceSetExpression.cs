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
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    #endregion Namespaces.

    [DebuggerDisplay("ResourceSetExpression {Source}.{MemberExpression}")]
    internal class ResourceSetExpression : ResourceExpression
    {
        #region Private fields.

        private readonly Type resourceType;

        private readonly Expression member;

        private Dictionary<PropertyInfo, ConstantExpression> keyFilter;

        private List<QueryOptionExpression> sequenceQueryOptions;

        private TransparentAccessors transparentScope;

        #endregion Private fields.

        internal ResourceSetExpression(Type type, Expression source, Expression memberExpression, Type resourceType, List<string> expandPaths, CountOption countOption, Dictionary<ConstantExpression, ConstantExpression> customQueryOptions, ProjectionQueryOptionExpression projection)
            : base(source, source != null ? (ExpressionType)ResourceExpressionType.ResourceNavigationProperty : (ExpressionType)ResourceExpressionType.RootResourceSet, type, expandPaths, countOption, customQueryOptions, projection)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(memberExpression != null, "memberExpression != null");
            Debug.Assert(resourceType != null, "resourceType != null");
            Debug.Assert(
                (source == null && memberExpression is ConstantExpression) ||
                (source != null && memberExpression is MemberExpression),
                "source is null with constant entity set name, or not null with member expression");

            this.member = memberExpression;
            this.resourceType = resourceType;
            this.sequenceQueryOptions = new List<QueryOptionExpression>();
        }

        #region Internal properties.

        internal Expression MemberExpression
        {
            get { return this.member; }
        }

        internal override Type ResourceType
        {
            get { return this.resourceType; }
        }

        internal bool HasTransparentScope
        {
            get { return this.transparentScope != null; } 
        }

        internal TransparentAccessors TransparentScope
        {
            get { return this.transparentScope;  }
            set { this.transparentScope = value; }
        }

        internal bool HasKeyPredicate
        {
            get { return this.keyFilter != null; }
        }

        internal Dictionary<PropertyInfo, ConstantExpression> KeyPredicate
        {
            get { return this.keyFilter; }
            set { this.keyFilter = value; }
        }

        internal override bool IsSingleton
        {
            get { return this.HasKeyPredicate; }
        }

        internal override bool HasQueryOptions
        {
	        get 
            { 
                return this.sequenceQueryOptions.Count > 0 ||
                    this.ExpandPaths.Count > 0 ||
                    this.CountOption == CountOption.InlineAll ||                    this.CustomQueryOptions.Count > 0 ||
                    this.Projection != null;
            }
        }

        internal FilterQueryOptionExpression Filter
        {
            get
            {
                return this.sequenceQueryOptions.OfType<FilterQueryOptionExpression>().SingleOrDefault();
            }
        }

        internal OrderByQueryOptionExpression OrderBy
        {
            get { return this.sequenceQueryOptions.OfType<OrderByQueryOptionExpression>().SingleOrDefault(); }
        }

        internal SkipQueryOptionExpression Skip
        {
            get { return this.sequenceQueryOptions.OfType<SkipQueryOptionExpression>().SingleOrDefault(); }
        }

        internal TakeQueryOptionExpression Take
        {
            get { return this.sequenceQueryOptions.OfType<TakeQueryOptionExpression>().SingleOrDefault(); }
        }

        internal IEnumerable<QueryOptionExpression> SequenceQueryOptions
        {
            get { return this.sequenceQueryOptions.ToList(); }
        }

        internal bool HasSequenceQueryOptions
        {
            get { return this.sequenceQueryOptions.Count > 0; }
        }

        #endregion Internal properties.

        #region Internal methods.

        internal override ResourceExpression CreateCloneWithNewType(Type type)
        {
            ResourceSetExpression rse = new ResourceSetExpression(
                type, 
                this.source, 
                this.MemberExpression, 
                TypeSystem.GetElementType(type),
                this.ExpandPaths.ToList(),
                this.CountOption,
                this.CustomQueryOptions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                this.Projection);
            rse.keyFilter = this.keyFilter;
            rse.sequenceQueryOptions = this.sequenceQueryOptions;
            rse.transparentScope = this.transparentScope;
            return rse;
        }

        internal void AddSequenceQueryOption(QueryOptionExpression qoe)
        {
            Debug.Assert(qoe != null, "qoe != null");
            QueryOptionExpression old = this.sequenceQueryOptions.Where(o => o.GetType() == qoe.GetType()).FirstOrDefault();
            if (old != null)
            {
                qoe = qoe.ComposeMultipleSpecification(old);
                this.sequenceQueryOptions.Remove(old);
            }

            this.sequenceQueryOptions.Add(qoe);
        }

        internal void OverrideInputReference(ResourceSetExpression newInput)
        {
            Debug.Assert(newInput != null, "Original resource set cannot be null");
            Debug.Assert(this.inputRef == null, "OverrideInputReference cannot be called if the target has already been referenced");

            InputReferenceExpression inputRef = newInput.inputRef;
            if (inputRef != null)
            {
                this.inputRef = inputRef;
                inputRef.OverrideTarget(this);
            }
        }

        #endregion Internal methods.

        [DebuggerDisplay("{ToString()}")]
        internal class TransparentAccessors
        {
            #region Internal fields.

            internal readonly string Accessor;

            internal readonly Dictionary<string, Expression> SourceAccessors;

            #endregion Internal fields.

            internal TransparentAccessors(string acc, Dictionary<string, Expression> sourceAccesors)
            {
                Debug.Assert(!string.IsNullOrEmpty(acc), "Set accessor cannot be null or empty");
                Debug.Assert(sourceAccesors != null, "sourceAccesors != null");

                this.Accessor = acc;
                this.SourceAccessors = sourceAccesors;
            }

            public override string ToString()
            {
                string result = "SourceAccessors=[" + string.Join(",", this.SourceAccessors.Keys.ToArray());
                result += "] ->* Accessor=" + this.Accessor;
                return result;
            }
        }
    }
}
