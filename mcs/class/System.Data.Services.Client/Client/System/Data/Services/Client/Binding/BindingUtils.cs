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
#region Namespaces
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Collections;
#endregion

    internal static class BindingUtils
    {
        internal static void ValidateEntitySetName(string entitySetName, object entity)
        {
            if (String.IsNullOrEmpty(entitySetName))
            {
                throw new InvalidOperationException(Strings.DataBinding_Util_UnknownEntitySetName(entity.GetType().FullName));
            }
        }
        
        internal static Type GetCollectionEntityType(Type collectionType)
        {
            while (collectionType != null)
            {
                if (collectionType.IsGenericType && WebUtil.IsDataServiceCollectionType(collectionType.GetGenericTypeDefinition()))
                {
                    return collectionType.GetGenericArguments()[0];
                }

                collectionType = collectionType.BaseType;
            }

            return null;
        }

        internal static void VerifyObserverNotPresent<T>(object oec, string sourceProperty, Type sourceType)
        {
            Debug.Assert(BindingEntityInfo.IsDataServiceCollection(oec.GetType()), "Must be an DataServiceCollection.");
            
            DataServiceCollection<T> typedCollection = oec as DataServiceCollection<T>;
            
            if (typedCollection.Observer != null)
            {
                throw new InvalidOperationException(Strings.DataBinding_CollectionPropertySetterValueHasObserver(sourceProperty, sourceType));
            }
        }
    }
}
