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

    internal class QueryComponents
    {
        #region Private fields.

        private readonly Uri uri;

        private readonly Type lastSegmentType;

        private readonly Dictionary<Expression, Expression> normalizerRewrites;

        private readonly LambdaExpression projection;

        private Version version;

        #endregion Private fields.

        internal QueryComponents(Uri uri, Version version, Type lastSegmentType, LambdaExpression projection, Dictionary<Expression, Expression> normalizerRewrites)
        {
            this.projection = projection;
            this.normalizerRewrites = normalizerRewrites;
            this.lastSegmentType = lastSegmentType;
            this.uri = uri;
            this.version = version;
        }

        #region Internal properties.

        internal Uri Uri
        {
            get
            {
                return this.uri;
            }
        }

        internal Dictionary<Expression, Expression> NormalizerRewrites
        {
            get 
            { 
                return this.normalizerRewrites; 
            }
        }

        internal LambdaExpression Projection
        {
            get
            {
                return this.projection;
            }
        }

        internal Type LastSegmentType
        {
            get
            {
                return this.lastSegmentType;
            }
        }

        internal Version Version
        {
            get
            {
                return this.version;
            }

#if !ASTORIA_LIGHT            
            
           set
            {
                this.version = value;
            }
#endif
        }

        #endregion Internal properties.
    }
}
