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

    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Text;
    using System.Reflection;

    #endregion Namespaces.

    [DebuggerDisplay("{NextLinkUri}")]
    public abstract class DataServiceQueryContinuation
    {
        #region Private fields.

        private readonly Uri nextLinkUri;
        
        private readonly ProjectionPlan plan;

        #endregion Private fields.

        #region Constructors.

        internal DataServiceQueryContinuation(Uri nextLinkUri, ProjectionPlan plan)
        {
            Debug.Assert(nextLinkUri != null, "nextLinkUri != null");
            Debug.Assert(plan != null, "plan != null");

            this.nextLinkUri = nextLinkUri;
            this.plan = plan;
        }

        #endregion Contructors.

        #region Properties.

        public Uri NextLinkUri
        {
            get { return this.nextLinkUri; }
        }

        internal abstract Type ElementType
        {
            get;
        }

        internal ProjectionPlan Plan
        {
            get { return this.plan; }
        }

        #endregion Properties.

        #region Methods.

        public override string ToString()
        {
            return this.NextLinkUri.ToString();
        }

        internal static DataServiceQueryContinuation Create(Uri nextLinkUri, ProjectionPlan plan)
        {
            Debug.Assert(plan != null || nextLinkUri == null, "plan != null || nextLinkUri == null");

            if (nextLinkUri == null)
            {
                return null;
            }

            var constructors = typeof(DataServiceQueryContinuation<>).MakeGenericType(plan.ProjectedType).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(constructors.Length == 1, "constructors.Length == 1");
            object result = Util.ConstructorInvoke(constructors[0], new object[] { nextLinkUri, plan });
            return (DataServiceQueryContinuation)result;
        }

        internal QueryComponents CreateQueryComponents()
        {
            QueryComponents result = new QueryComponents(this.NextLinkUri, Util.DataServiceVersionEmpty, this.Plan.LastSegmentType, null, null);
            return result;
        }

        #endregion Methods.
    }

    public sealed class DataServiceQueryContinuation<T> : DataServiceQueryContinuation
    {
        #region Contructors.

        internal DataServiceQueryContinuation(Uri nextLinkUri, ProjectionPlan plan)
            : base(nextLinkUri, plan)
        {
        }

        #endregion Contructors.

        #region Properties.

        internal override Type ElementType
        {
            get { return typeof(T); }
        }

        #endregion Properties.
    }
}
