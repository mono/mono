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
    using System.Linq.Expressions;

    #endregion Namespaces.

    [DebuggerDisplay("Segment {ProjectionType} {Member}")]
    internal class ProjectionPathSegment
    {
        #region Constructors.

        internal ProjectionPathSegment(ProjectionPath startPath, string member, Type projectionType)
        {
            Debug.Assert(startPath != null, "startPath != null");
            
            this.Member = member;
            this.StartPath = startPath;
            this.ProjectionType = projectionType;
        }

        #endregion Constructors.

        #region Internal properties.

        internal string Member 
        { 
            get; 
            private set; 
        }

        internal Type ProjectionType 
        { 
            get; 
            set; 
        }

        internal ProjectionPath StartPath 
        { 
            get; 
            private set; 
        }

        #endregion Internal properties.
    }
}
