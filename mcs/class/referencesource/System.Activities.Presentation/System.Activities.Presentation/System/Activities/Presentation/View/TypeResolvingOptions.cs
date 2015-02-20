//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.View
{
    using System.Runtime;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections.Generic;

    [Fx.Tag.XamlVisible(false)]
    public class TypeResolvingOptions
    {
        private IDictionary<string, string> hintTextMap;

        public TypeResolvingOptions() : this(null)
        {
        }

        public TypeResolvingOptions(IEnumerable<Type> defaultTypes)
        {
            if (defaultTypes != null)
            {
                this.MostRecentlyUsedTypes = new ObservableCollection<Type>();
                foreach (Type item in defaultTypes)
                {
                    if (item == null)
                    {
                        throw FxTrace.Exception.AsError(new ArgumentException(SR.TypeResolvingOptionsArgumentExceptionMessage));
                    }
                    this.MostRecentlyUsedTypes.Add(item);
                }
            }
        }

        public Func<Type, bool> Filter
        {
            get;
            set;
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.CollectionPropertiesShouldBeReadOnly,
            Justification = "Setter is provided to data binding on this property.")]
        internal ObservableCollection<Type> MostRecentlyUsedTypes
        {
            get;
            set;
        }

        /// <summary>
        /// A dictionary that maps the name of a generic type argument to a hint text.
        /// </summary>
        internal IDictionary<string, string> HintTextMap
        {
            get
            {
                if (this.hintTextMap == null)
                {
                    this.hintTextMap = new Dictionary<string, string>();
                }

                return this.hintTextMap;
            }
        }

        public bool BrowseTypeDirectly
        {
            get;
            set;
        }

        internal static TypeResolvingOptions Merge(TypeResolvingOptions lhs, TypeResolvingOptions rhs)
        {
            if (lhs == null)
            {
                return rhs;
            }
            else if (rhs == null)
            {
                return lhs;
            }

            TypeResolvingOptions result = new TypeResolvingOptions
            {
                Filter = FuncAnd(lhs.Filter, rhs.Filter),
                MostRecentlyUsedTypes = Intersect(lhs.MostRecentlyUsedTypes, rhs.MostRecentlyUsedTypes),
                BrowseTypeDirectly = lhs.BrowseTypeDirectly && rhs.BrowseTypeDirectly
            };

            // Pass in the fields directly to prevent triggering lazy initialization.
            IDictionary<string, string> mergedHintTextMap = MergeDictionaries<string, string>(lhs.hintTextMap, rhs.hintTextMap);
            if (mergedHintTextMap != null && mergedHintTextMap.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in mergedHintTextMap)
                {
                    result.HintTextMap.Add(kvp.Key, kvp.Value);
                }
            }

            return result;
        }

        static Func<Type, bool> FuncAnd(Func<Type, bool> lhs, Func<Type, bool> rhs)
        {
            if (lhs == null)
            {
                return rhs;
            }
            else if (rhs == null)
            {
                return lhs;
            }

            return new Func<Type, bool>((e) => lhs(e) && rhs(e));
        }

        static ObservableCollection<T> Intersect<T>(ObservableCollection<T> lhs, ObservableCollection<T> rhs)
        {
            if (lhs == null)
            {
                return rhs;
            }
            else if (rhs == null)
            {
                return lhs;
            }

            ObservableCollection<T> collection = new ObservableCollection<T>();
            foreach (T t in lhs)
            {
                if (rhs.Contains(t))
                {
                    collection.Add(t);
                }
            }

            return collection;
        }

        static IDictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(IDictionary<TKey, TValue> lhs, IDictionary<TKey, TValue> rhs)
        {
            if (lhs == null || lhs.Count == 0)
            {
                return rhs;
            }

            if (rhs == null || rhs.Count == 0)
            {
                return lhs;
            }

            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            foreach (KeyValuePair<TKey, TValue> kvp in lhs)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<TKey, TValue> kvp in rhs)
            {
                if (!result.ContainsKey(kvp.Key))
                {
                    result.Add(kvp.Key, kvp.Value);
                }
            }

            return result;
        }
    }
}
