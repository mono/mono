#if NET_2_0
/*
 Copyright (c) 2003-2004 Niels Kokholm <kokholm@itu.dk> and Peter Sestoft <sestoft@dina.kvl.dk>
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using C5;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
namespace C5.ComparerBuilder
{
    /// <summary>
    /// A default item comparer for an item type that is either generic (IComparable&lt;T&gt;)
    /// or ordinarily (System.IComparable) comparable.
    /// </summary>
    public class FromComparable<T>
    {
        static Type naturalComparerO = typeof(NaturalComparerO<>);

        static Type naturalComparer = typeof(NaturalComparer<>);


        /// <summary>
        /// Create a default comparer
        /// <exception cref="ArgumentException"/> if T is not comparable. 
        /// </summary>
        /// <returns>The comparer</returns>
        [Tested]
        public static IComparer<T> Examine()
        {
            Type t = typeof(T);

            if (t.Equals(typeof(int)))
                return (IComparer<T>)(new IC());

            if (typeof(IComparable<T>).IsAssignableFrom(t))
            {
                Type c = naturalComparer.BindGenericParameters(new Type[] { t });

                return (IComparer<T>)(c.GetConstructor(System.Type.EmptyTypes).Invoke(null));
            }

            if (t.GetInterface("System.IComparable") != null)
            {
                Type c = naturalComparerO.BindGenericParameters(new Type[] { t });

                return (IComparer<T>)(c.GetConstructor(System.Type.EmptyTypes).Invoke(null));
            }

            throw new ArgumentException(String.Format("Cannot make IComparer<{0}>", t));
        }
    }
}
namespace C5.HasherBuilder
{
    /// <summary>
    /// Prototype for an sequenced hasher for IIndexed[W]
    /// This will use the IIndexed[W] specific operations
    /// </summary>
    public class SequencedHasher<S, W> : IHasher<S>
        where S : ISequenced<W>
    {
        /// <summary>
        /// Get the hash code with respect to this sequenced hasher
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>The hash code</returns>
        [Tested]
        public int GetHashCode(S item) { return item.GetHashCode(); }


        /// <summary>
        /// Check if two items are equal with respect to this sequenced hasher
        /// </summary>
        /// <param name="i1">first item</param>
        /// <param name="i2">second item</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(S i1, S i2) { return i1 == null ? i2 == null : i1.Equals(i2); }
    }



    /// <summary>
    /// Prototype for an unsequenced hasher for ICollection[W]
    /// This will use the ICollection[W] specific operations
    /// </summary>
    public class UnsequencedHasher<S, W> : IHasher<S>
        where S : ICollection<W>
    {
        /// <summary>
        /// Get the hash code with respect to this unsequenced hasher
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>The hash code</returns>
        [Tested]
        public int GetHashCode(S item) { return item.GetHashCode(); }


        /// <summary>
        /// Check if two items are equal with respect to this unsequenced hasher
        /// </summary>
        /// <param name="i1">first item</param>
        /// <param name="i2">second item</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(S i1, S i2) { return i1 == null ? i2 == null : i1.Equals(i2); }
    }



    /// <summary>
    /// Create a hasher for T that is DefaultValueTypeHasher[T] 
    /// or DefaultReferenceTypeHasher[T] unless T has been 
    /// instatiated to a type of the exact form IIndexed[W] or ICollection[W]
    /// in which case Examine will return Sequenced- repectively UnsequencedHasher.
    /// </summary>
    public class ByPrototype<T>
    {
        static Type isequenced = typeof(ISequenced<>);

        static Type ieditable = typeof(ICollection<>);

        static Type orderedhasher = typeof(HasherBuilder.SequencedHasher<,>);

        static Type unorderedhasher = typeof(HasherBuilder.UnsequencedHasher<,>);
/*
        static Type isequenced = Type.GetType("C5.ISequenced");

        static Type ieditable = Type.GetType("C5.ICollection");

        static Type orderedhasher = Type.GetType("C5.HasherBuilder.SequencedHasher");

        static Type unorderedhasher = Type.GetType("C5.HasherBuilder.UnsequencedHasher");
*/

        /// <summary>
        /// See class description
        /// </summary>
        /// <returns>The hasher</returns>
        [Tested]
        public static IHasher<T> Examine()
        {
            Type t = typeof(T);

            if (!t.HasGenericArguments)
            {
                if (t.Equals(typeof(int)))
                    return (IHasher<T>)(new IntHasher());
                else if (t.IsValueType)
                    return new DefaultValueTypeHasher<T>();
                else
                    return new DefaultReferenceTypeHasher<T>();
            }

            Type s = t.GetGenericTypeDefinition();
            Type[] v = t.GetGenericArguments();
            Type b;

            if (s.Equals(isequenced))
                b = orderedhasher;
            else if (s.Equals(ieditable))
                b = unorderedhasher;
            else if (t.IsValueType)
                return new DefaultValueTypeHasher<T>();
            else
                return new DefaultReferenceTypeHasher<T>();

            Type c = b.BindGenericParameters(new Type[] { t, v[0] });

            return (IHasher<T>)(c.GetConstructor(System.Type.EmptyTypes).Invoke(null));
        }
    }


#if !EXPERIMENTAL

    /// <summary>
    /// IHasher factory class: examines at instatiation time if T is an
    /// interface implementing "int GetHashCode()" and "bool Equals(T)".
    /// If those are not present, MakeHasher will return a default hasher,
    /// else this class will implement Ihasher[T] via Invoke() on the
    /// reflected method infos.
    /// </summary>
    public class ByInvoke<T> : IHasher<T>
    {
        internal static readonly System.Reflection.MethodInfo hinfo, einfo;


        static ByInvoke()
        {
            Type t = typeof(T);

            if (!t.IsInterface) return;

            BindingFlags f = BindingFlags.Public | BindingFlags.Instance;

            hinfo = t.GetMethod("GetHashCode", f, null, new Type[0], null);
            einfo = t.GetMethod("Equals", f, null, new Type[1] { t }, null);
        }


        private ByInvoke() { }

/// <summary>
/// 
/// </summary>
/// <returns></returns>
        public static IHasher<T> MakeHasher()
        {
            if (hinfo != null && einfo != null)
                return new ByInvoke<T>();
            else
                return new DefaultReferenceTypeHasher<T>();
        }

/// <summary>
/// 
/// </summary>
/// <param name="item"></param>
/// <returns></returns>
        public int GetHashCode(T item)
        {
            return (int)(hinfo.Invoke(item, null));
        }

/// <summary>
/// 
/// </summary>
/// <param name="i1"></param>
/// <param name="i2"></param>
/// <returns></returns>
        public bool Equals(T i1, T i2)
        {
            return (bool)(einfo.Invoke(i1, new object[1] { i2 }));
        }
    }



    /// <summary>
    /// Like ByInvoke, but tries to build a hasher by RTCG to
    /// avoid the Invoke() overhead. Does not work as intended 
    /// because of a Whidbey RTCG bug.
    /// </summary>
    public class ByRTCG
    {
        private static ModuleBuilder moduleBuilder;

        private static AssemblyBuilder assemblyBuilder;

        private static int uid = 0;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hinfo"></param>
        /// <param name="einfo"></param>
        /// <returns></returns>
        public static /*ObjectHasher */ IHasher<T> CreateHasher<T>(MethodInfo hinfo, MethodInfo einfo)
        {
            if (moduleBuilder == null)
            {
                string assmname = "LeFake";
                string filename = assmname + ".dll";
                AssemblyName assemblyName = new AssemblyName("LeFake");
                AppDomain appdomain = AppDomain.CurrentDomain;
                AssemblyBuilderAccess acc = AssemblyBuilderAccess.RunAndSave;

                assemblyBuilder = appdomain.DefineDynamicAssembly(assemblyName, acc);
                moduleBuilder = assemblyBuilder.DefineDynamicModule(assmname, filename);
            }

            Type t = typeof(/*object*/ T);
            Type o_t = typeof(object);
            Type h_t = typeof(/*ObjectHasher*/ IHasher<T>);
            Type i_t = typeof(int);
            //TODO: protect uid for thread safety!
            string name = "C5.Dynamic.Hasher_" + uid++;
            TypeAttributes tatt = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;
            TypeBuilder tb = moduleBuilder.DefineType(name, tatt, o_t, new Type[1] { h_t });
            MethodAttributes matt = MethodAttributes.Public | MethodAttributes.Virtual;
            MethodBuilder mb = tb.DefineMethod("GetHashCode", matt, i_t, new Type[1] { t });
            ILGenerator ilg = mb.GetILGenerator();

            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Callvirt, hinfo);
            ilg.Emit(OpCodes.Ret);
            mb = tb.DefineMethod("Equals", matt, typeof(bool), new Type[2] { t, t });
            ilg = mb.GetILGenerator();
            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Ldarg_2);
            ilg.Emit(OpCodes.Callvirt, einfo);
            ilg.Emit(OpCodes.Ret);

            Type hasher_t = tb.CreateType();
            object hasher = hasher_t.GetConstructor(new Type[0]).Invoke(null);

            return (IHasher<T>)hasher;
        }

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <returns></returns>
        public static IHasher<T> build<T>()
        {
            MethodInfo hinfo = ByInvoke<T>.hinfo, einfo = ByInvoke<T>.einfo;

            if (hinfo != null && einfo != null)
                return CreateHasher<T>(hinfo, einfo);
            else
                return ByPrototype<T>.Examine();
        }

/// <summary>
/// 
/// </summary>
        public void dump()
        {
            assemblyBuilder.Save("LeFake.dll");
        }
    }
#endif
}
#endif
