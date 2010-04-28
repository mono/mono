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


namespace System.Data.Services.Common
{
    using System.Collections.Generic;

    internal class EpmSourcePathSegment
    {
        #region Fields

        private String propertyName;

        private List<EpmSourcePathSegment> subProperties;

        #endregion

        internal EpmSourcePathSegment(String propertyName)
        {
            this.propertyName = propertyName;
            this.subProperties = new List<EpmSourcePathSegment>();
        }

        #region Properties

        internal String PropertyName
        {
            get
            {
                return this.propertyName;
            }
        }

        internal List<EpmSourcePathSegment> SubProperties
        {
            get
            {
                return this.subProperties;
            }
        }

        internal EntityPropertyMappingInfo EpmInfo
        {
            get;
            set;
        }

        #endregion
    }
}