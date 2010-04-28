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
    using System.Linq.Expressions;

    #endregion Namespaces.

    internal class ProjectionQueryOptionExpression : QueryOptionExpression
    {
        #region Private fields.

        private readonly LambdaExpression lambda;

        private readonly List<string> paths;

        #endregion Private fields.

        internal ProjectionQueryOptionExpression(Type type, LambdaExpression lambda, List<string> paths)
            : base((ExpressionType)ResourceExpressionType.ProjectionQueryOption, type)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(lambda != null, "lambda != null");
            Debug.Assert(paths != null, "paths != null");

            this.lambda = lambda;
            this.paths = paths;
        }

        #region Internal properties.

        internal LambdaExpression Selector
        {
            get
            {
                return this.lambda;
            }
        }

        internal List<string> Paths
        {
            get
            {
                return this.paths;
            }
        }

        #endregion Internal properties.
    }
}