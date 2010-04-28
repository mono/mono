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
    using System.Diagnostics;

    #endregion Namespaces.

    internal class ProjectionPlan
    {
#if DEBUG
        internal System.Linq.Expressions.Expression SourceProjection
        {
            get;
            set;
        }

        internal System.Linq.Expressions.Expression TargetProjection
        {
            get;
            set;
        }
#endif

        internal Type LastSegmentType
        {
            get;
            set;
        }

        internal Func<object, object, Type, object> Plan 
        { 
            get;
            set;
        }

        internal Type ProjectedType
        {
            get;
            set;
        }

#if DEBUG
        public override string ToString()
        {
            return "Plan - projection: " + this.SourceProjection + "\r\nBecomes: " + this.TargetProjection;
        }
#endif

        internal object Run(AtomMaterializer materializer, AtomEntry entry, Type expectedType)
        {
            Debug.Assert(materializer != null, "materializer != null");
            Debug.Assert(entry != null, "entry != null");

            return this.Plan(materializer, entry, expectedType);
        }
    }
}
