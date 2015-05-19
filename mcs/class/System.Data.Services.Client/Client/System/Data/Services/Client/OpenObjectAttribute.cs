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


#if ASTORIA_OPEN_OBJECT
namespace System.Data.Services.Client
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class OpenObjectAttribute : System.Attribute
    {
        private readonly string openObjectPropertyName;

        public OpenObjectAttribute(string openObjectPropertyName)
        {
            Util.CheckArgumentNotEmpty(openObjectPropertyName, "openObjectPropertyName");
            this.openObjectPropertyName = openObjectPropertyName;
        }

        public string OpenObjectPropertyName
        {
            get { return this.openObjectPropertyName; }
        }
    }
}
#endif
