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
    using System.Collections;

    #endregion Namespaces.

    internal class ReferenceEqualityComparer : IEqualityComparer
    {
        #region Private fields.

#if NON_GENERIC_AVAILABLE
        private static ReferenceEqualityComparer nonGenericInstance;
#endif

        #endregion Private fields.

        #region Constructors.

        protected ReferenceEqualityComparer()
        {
        }

        #endregion Constructors.

        #region Properties.

        bool IEqualityComparer.Equals(object x, object y)
        {
            return object.ReferenceEquals(x, y);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return obj.GetHashCode();
        }

#if NON_GENERIC_AVAILABLE
        internal ReferenceEqualityComparer NonGenericInstance
        {
            get
            {
                if (nonGenericInstance == null)
                {
                    ReferenceEqualityComparer comparer = new ReferenceEqualityComparer();
                    System.Threading.Interlocked.CompareExchange(ref nonGenericInstance, comparer, null);
                }

                return nonGenericInstance;
            }
        }
#endif

        #endregion Properties.
    }

    internal sealed class ReferenceEqualityComparer<T> : ReferenceEqualityComparer, IEqualityComparer<T>
    {
        #region Private fields.

        private static ReferenceEqualityComparer<T> instance;

        #endregion Private fields.

        #region Constructors.

        private ReferenceEqualityComparer() : base()
        {
        }

        #endregion Constructors.

        #region Properties.

        internal static ReferenceEqualityComparer<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.Assert(!typeof(T).IsValueType, "!typeof(T).IsValueType -- can't use reference equality in a meaningful way with value types");
                    ReferenceEqualityComparer<T> newInstance = new ReferenceEqualityComparer<T>();
                    System.Threading.Interlocked.CompareExchange(ref instance, newInstance, null);
                }

                return instance;
            }
        }

        #endregion Properties.

        #region Methods.

        public bool Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return obj.GetHashCode();
        }

        #endregion Methods.
    }
}
