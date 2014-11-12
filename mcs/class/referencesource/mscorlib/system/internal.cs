// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** This file exists to contain miscellaneous module-level attributes
** and other miscellaneous stuff.
**
**
** 
===========================================================*/
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Reflection;
using System.Security;

#if FEATURE_COMINTEROP

using System.Runtime.InteropServices.WindowsRuntime;

[assembly:Guid("BED7F4EA-1A96-11d2-8F08-00A0C9A6186D")]

// The following attribute are required to ensure COM compatibility.
[assembly:System.Runtime.InteropServices.ComCompatibleVersion(1, 0, 3300, 0)]
[assembly:System.Runtime.InteropServices.TypeLibVersion(2, 4)]

#endif // FEATURE_COMINTEROP

[assembly:DefaultDependencyAttribute(LoadHint.Always)]
// mscorlib would like to have its literal strings frozen if possible
[assembly: System.Runtime.CompilerServices.StringFreezingAttribute()]

namespace System
{
    static class Internal
    {
        // This method is purely an aid for NGen to statically deduce which
        // instantiations to save in the ngen image.
        // Otherwise, the JIT-compiler gets used, which is bad for working-set.
        // Note that IBC can provide this information too.
        // However, this helps in keeping the JIT-compiler out even for
        // test scenarios which do not use IBC.
        // This can be removed after V2, when we implement other schemes
        // of keeping the JIT-compiler out for generic instantiations.

        static void CommonlyUsedGenericInstantiations_HACK()
        {
            // Make absolutely sure we include some of the most common 
            // instantiations here in mscorlib's ngen image.
            // Note that reference type instantiations are already included
            // automatically for us.

            System.Array.Sort<double>(null);
            System.Array.Sort<int>(null);
            System.Array.Sort<IntPtr>(null);
            
            new ArraySegment<byte>(new byte[1], 0, 0);

            new Dictionary<Char, Object>();
            new Dictionary<Guid, Byte>();
            new Dictionary<Guid, Object>();
            new Dictionary<Guid, Guid>(); // Added for Visual Studio 2010
            new Dictionary<Int16, IntPtr>();
            new Dictionary<Int32, Byte>();
            new Dictionary<Int32, Int32>();
            new Dictionary<Int32, Object>();
            new Dictionary<IntPtr, Boolean>();
            new Dictionary<IntPtr, Int16>();
            new Dictionary<Object, Boolean>();
            new Dictionary<Object, Char>();
            new Dictionary<Object, Guid>();
            new Dictionary<Object, Int32>();
            new Dictionary<Object, Int64>(); // Added for Visual Studio 2010
            new Dictionary<uint, WeakReference>();  // NCL team needs this
            new Dictionary<Object, UInt32>();
            new Dictionary<UInt32, Object>();
            new Dictionary<Int64, Object>();

        // Microsoft.Windows.Design
            new Dictionary<System.Reflection.MemberTypes, Object>();
            new EnumEqualityComparer<System.Reflection.MemberTypes>();

        // Microsoft.Expression.DesignModel
            new Dictionary<Object, KeyValuePair<Object,Object>>();
            new Dictionary<KeyValuePair<Object,Object>, Object>();

            NullableHelper_HACK<Boolean>();
            NullableHelper_HACK<Byte>();
            NullableHelper_HACK<Char>();
            NullableHelper_HACK<DateTime>(); 
            NullableHelper_HACK<Decimal>(); 
            NullableHelper_HACK<Double>();
            NullableHelper_HACK<Guid>();
            NullableHelper_HACK<Int16>();
            NullableHelper_HACK<Int32>();
            NullableHelper_HACK<Int64>();
            NullableHelper_HACK<Single>();
            NullableHelper_HACK<TimeSpan>();
            NullableHelper_HACK<DateTimeOffset>();  // For SQL

            new List<Boolean>();
            new List<Byte>();
            new List<Char>();
            new List<DateTime>();
            new List<Decimal>();
            new List<Double>();
            new List<Guid>();
            new List<Int16>();
            new List<Int32>();
            new List<Int64>();
            new List<TimeSpan>();
            new List<SByte>();
            new List<Single>();
            new List<UInt16>();
            new List<UInt32>();
            new List<UInt64>();
            new List<IntPtr>();
            new List<KeyValuePair<Object, Object>>();
            new List<GCHandle>();  // NCL team needs this
            new List<DateTimeOffset>();

            new KeyValuePair<Char, UInt16>('\0', UInt16.MinValue);
            new KeyValuePair<UInt16, Double>(UInt16.MinValue, Double.MinValue);
            new KeyValuePair<Object, Int32>(String.Empty, Int32.MinValue);
            new KeyValuePair<Int32, Int32>(Int32.MinValue, Int32.MinValue);            
            SZArrayHelper_HACK<Boolean>(null);
            SZArrayHelper_HACK<Byte>(null);
            SZArrayHelper_HACK<DateTime>(null);
            SZArrayHelper_HACK<Decimal>(null);
            SZArrayHelper_HACK<Double>(null);
            SZArrayHelper_HACK<Guid>(null);
            SZArrayHelper_HACK<Int16>(null);
            SZArrayHelper_HACK<Int32>(null);
            SZArrayHelper_HACK<Int64>(null);
            SZArrayHelper_HACK<TimeSpan>(null);
            SZArrayHelper_HACK<SByte>(null);
            SZArrayHelper_HACK<Single>(null);
            SZArrayHelper_HACK<UInt16>(null);
            SZArrayHelper_HACK<UInt32>(null);
            SZArrayHelper_HACK<UInt64>(null);
            SZArrayHelper_HACK<DateTimeOffset>(null);

            SZArrayHelper_HACK<CustomAttributeTypedArgument>(null);
            SZArrayHelper_HACK<CustomAttributeNamedArgument>(null);
        }

        static T NullableHelper_HACK<T>() where T : struct
        {
            Nullable.Compare<T>(null, null);    
            Nullable.Equals<T>(null, null); 
            Nullable<T> nullable = new Nullable<T>();
            return nullable.GetValueOrDefault();
        }       

        static void SZArrayHelper_HACK<T>(SZArrayHelper oSZArrayHelper)
        {
            // Instantiate common methods for IList implementation on Array
            oSZArrayHelper.get_Count<T>();
            oSZArrayHelper.get_Item<T>(0);
            oSZArrayHelper.GetEnumerator<T>();
        }

#if FEATURE_COMINTEROP

        // Similar to CommonlyUsedGenericInstantiations_HACK but for instantiations of marshaling stubs used
        // for WinRT redirected interfaces. Note that we do care about reference types here as well because,
        // say, IList<string> and IList<object> cannot share marshaling stubs.
        // The methods below "call" most commonly used stub methods on redirected interfaces and take arguments
        // typed as matching instantiations of mscorlib copies of WinRT interfaces (IIterable<T>, IVector<T>,
        // IMap<K, V>, ...) which is necessary to generate all required IL stubs.

        [SecurityCritical]
        static void CommonlyUsedWinRTRedirectedInterfaceStubs_HACK()
        {
            WinRT_IEnumerable_HACK<byte>(null, null, null);
            WinRT_IEnumerable_HACK<char>(null, null, null);
            WinRT_IEnumerable_HACK<short>(null, null, null);
            WinRT_IEnumerable_HACK<ushort>(null, null, null);
            WinRT_IEnumerable_HACK<int>(null, null, null);
            WinRT_IEnumerable_HACK<uint>(null, null, null);
            WinRT_IEnumerable_HACK<long>(null, null, null);
            WinRT_IEnumerable_HACK<ulong>(null, null, null);
            WinRT_IEnumerable_HACK<float>(null, null, null);
            WinRT_IEnumerable_HACK<double>(null, null, null);
            WinRT_IEnumerable_HACK<string>(null, null, null);
            WinRT_IEnumerable_HACK<object>(null, null, null);

            WinRT_IList_HACK<int>(null, null, null, null);
            WinRT_IList_HACK<string>(null, null, null, null);
            WinRT_IList_HACK<object>(null, null, null, null);

            WinRT_IReadOnlyList_HACK<int>(null, null, null);
            WinRT_IReadOnlyList_HACK<string>(null, null, null);
            WinRT_IReadOnlyList_HACK<object>(null, null, null);

            WinRT_IDictionary_HACK<string, int>(null, null, null, null);
            WinRT_IDictionary_HACK<string, string>(null, null, null, null);
            WinRT_IDictionary_HACK<string, object>(null, null, null, null);
            WinRT_IDictionary_HACK<object, object>(null, null, null, null);

            WinRT_IReadOnlyDictionary_HACK<string, int>(null, null, null, null);
            WinRT_IReadOnlyDictionary_HACK<string, string>(null, null, null, null);
            WinRT_IReadOnlyDictionary_HACK<string, object>(null, null, null, null);
            WinRT_IReadOnlyDictionary_HACK<object, object>(null, null, null, null);
        }

        [SecurityCritical]
        static void WinRT_IEnumerable_HACK<T>(IterableToEnumerableAdapter iterableToEnumerableAdapter, EnumerableToIterableAdapter enumerableToIterableAdapter, IIterable<T> iterable)
        {
            // instantiate stubs for the one method on IEnumerable<T> and the one method on IIterable<T>
            iterableToEnumerableAdapter.GetEnumerator_Stub<T>();
            enumerableToIterableAdapter.First_Stub<T>();
        }

        [SecurityCritical]
        static void WinRT_IList_HACK<T>(VectorToListAdapter vectorToListAdapter, VectorToCollectionAdapter vectorToCollectionAdapter, ListToVectorAdapter listToVectorAdapter, IVector<T> vector)
        {
            WinRT_IEnumerable_HACK<T>(null, null, null);

            // instantiate stubs for commonly used methods on IList<T> and ICollection<T>
            vectorToListAdapter.Indexer_Get<T>(0);
            vectorToListAdapter.Indexer_Set<T>(0, default(T));
            vectorToListAdapter.Insert<T>(0, default(T));
            vectorToListAdapter.RemoveAt<T>(0);
            vectorToCollectionAdapter.Count<T>();
            vectorToCollectionAdapter.Add<T>(default(T));
            vectorToCollectionAdapter.Clear<T>();

            // instantiate stubs for commonly used methods on IVector<T>
            listToVectorAdapter.GetAt<T>(0);
            listToVectorAdapter.Size<T>();
            listToVectorAdapter.SetAt<T>(0, default(T));
            listToVectorAdapter.InsertAt<T>(0, default(T));
            listToVectorAdapter.RemoveAt<T>(0);
            listToVectorAdapter.Append<T>(default(T));
            listToVectorAdapter.RemoveAtEnd<T>();
            listToVectorAdapter.Clear<T>();
        }

        [SecurityCritical]
        static void WinRT_IReadOnlyCollection_HACK<T>(VectorViewToReadOnlyCollectionAdapter vectorViewToReadOnlyCollectionAdapter)
        {
            WinRT_IEnumerable_HACK<T>(null, null, null);

            // instantiate stubs for commonly used methods on IReadOnlyCollection<T>
            vectorViewToReadOnlyCollectionAdapter.Count<T>();
        }

        [SecurityCritical]
        static void WinRT_IReadOnlyList_HACK<T>(IVectorViewToIReadOnlyListAdapter vectorToListAdapter, IReadOnlyListToIVectorViewAdapter listToVectorAdapter, IVectorView<T> vectorView)
        {
            WinRT_IEnumerable_HACK<T>(null, null, null);
            WinRT_IReadOnlyCollection_HACK<T>(null);

            // instantiate stubs for commonly used methods on IReadOnlyList<T>
            vectorToListAdapter.Indexer_Get<T>(0);

            // instantiate stubs for commonly used methods on IVectorView<T>
            listToVectorAdapter.GetAt<T>(0);
            listToVectorAdapter.Size<T>();
        }

        [SecurityCritical]
        static void WinRT_IDictionary_HACK<K, V>(MapToDictionaryAdapter mapToDictionaryAdapter, MapToCollectionAdapter mapToCollectionAdapter, DictionaryToMapAdapter dictionaryToMapAdapter, IMap<K, V> map)
        {
            WinRT_IEnumerable_HACK<KeyValuePair<K, V>>(null, null, null);

            // instantiate stubs for commonly used methods on IDictionary<K, V> and ICollection<KeyValuePair<K, V>>
            V dummy;
            mapToDictionaryAdapter.Indexer_Get<K, V>(default(K));
            mapToDictionaryAdapter.Indexer_Set<K, V>(default(K), default(V));
            mapToDictionaryAdapter.ContainsKey<K, V>(default(K));
            mapToDictionaryAdapter.Add<K, V>(default(K), default(V));
            mapToDictionaryAdapter.Remove<K, V>(default(K));
            mapToDictionaryAdapter.TryGetValue<K, V>(default(K), out dummy);
            mapToCollectionAdapter.Count<K, V>();
            mapToCollectionAdapter.Add<K, V>(new KeyValuePair<K, V>(default(K), default(V)));
            mapToCollectionAdapter.Clear<K, V>();

            // instantiate stubs for commonly used methods on IMap<K, V>
            dictionaryToMapAdapter.Lookup<K, V>(default(K));
            dictionaryToMapAdapter.Size<K, V>();
            dictionaryToMapAdapter.HasKey<K, V>(default(K));
            dictionaryToMapAdapter.Insert<K, V>(default(K), default(V));
            dictionaryToMapAdapter.Remove<K, V>(default(K));
            dictionaryToMapAdapter.Clear<K, V>();
        }

        [SecurityCritical]
        static void WinRT_IReadOnlyDictionary_HACK<K, V>(IMapViewToIReadOnlyDictionaryAdapter mapToDictionaryAdapter, IReadOnlyDictionaryToIMapViewAdapter dictionaryToMapAdapter, IMapView<K, V> mapView, MapViewToReadOnlyCollectionAdapter mapViewToReadOnlyCollectionAdapter)
        {
            WinRT_IEnumerable_HACK<KeyValuePair<K, V>>(null, null, null);
            WinRT_IReadOnlyCollection_HACK<KeyValuePair<K, V>>(null);

            // instantiate stubs for commonly used methods on IReadOnlyDictionary<K, V>
            V dummy;
            mapToDictionaryAdapter.Indexer_Get<K, V>(default(K));
            mapToDictionaryAdapter.ContainsKey<K, V>(default(K));
            mapToDictionaryAdapter.TryGetValue<K, V>(default(K), out dummy);

            // instantiate stubs for commonly used methods in IReadOnlyCollection<T>
            mapViewToReadOnlyCollectionAdapter.Count<K, V>();

            // instantiate stubs for commonly used methods on IMapView<K, V>
            dictionaryToMapAdapter.Lookup<K, V>(default(K));
            dictionaryToMapAdapter.Size<K, V>();
            dictionaryToMapAdapter.HasKey<K, V>(default(K));
        }

#endif // FEATURE_COMINTEROP
    }
}
