using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Permissions;
using System.Security;
using System.Runtime.CompilerServices;

namespace System.Data.Linq.Mapping {
    using System.Data.Linq.Provider;
    using System.Diagnostics.CodeAnalysis;

    internal delegate V DGet<T, V>(T t);
    internal delegate void DSet<T, V>(T t, V v);
    internal delegate void DRSet<T, V>(ref T t, V v);

    internal static class FieldAccessor {

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static MetaAccessor Create(Type objectType, FieldInfo fi) {
            if (!fi.ReflectedType.IsAssignableFrom(objectType))
                throw Error.InvalidFieldInfo(objectType, fi.FieldType, fi);
            Delegate dget = null;
            Delegate drset = null;

            if (!objectType.IsGenericType) {
                DynamicMethod mget = new DynamicMethod(
                    "xget_" + fi.Name,
                    fi.FieldType,
                    new Type[] { objectType },
                    true
                    );
                ILGenerator gen = mget.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, fi);
                gen.Emit(OpCodes.Ret);
                dget = mget.CreateDelegate(typeof(DGet<,>).MakeGenericType(objectType, fi.FieldType));

                DynamicMethod mset = new DynamicMethod(
                    "xset_" + fi.Name,
                    typeof(void),
                    new Type[] { objectType.MakeByRefType(), fi.FieldType },
                    true
                    );
                gen = mset.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                if (!objectType.IsValueType) {
                    gen.Emit(OpCodes.Ldind_Ref);
                }
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, fi);
                gen.Emit(OpCodes.Ret);
                drset = mset.CreateDelegate(typeof(DRSet<,>).MakeGenericType(objectType, fi.FieldType));
            }
            return (MetaAccessor)Activator.CreateInstance(
                typeof(Accessor<,>).MakeGenericType(objectType, fi.FieldType),
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] { fi, dget, drset }, null
                );
        }

        class Accessor<T, V> : MetaAccessor<T, V> { 
            DGet<T, V> dget;
            DRSet<T, V> drset;
            FieldInfo fi;

            internal Accessor(FieldInfo fi, DGet<T, V> dget, DRSet<T, V> drset) {
                this.fi = fi;
                this.dget = dget;
                this.drset = drset;
            }
            public override V GetValue(T instance) {
                if (this.dget != null)
                    return this.dget(instance);
                return (V)fi.GetValue(instance);
            }
            public override void SetValue(ref T instance, V value) {
                if (this.drset != null)
                    this.drset(ref instance, value);
                else
                    this.fi.SetValue(instance, value);
            }
        }
    }

    internal static class PropertyAccessor {

        internal static MetaAccessor Create(Type objectType, PropertyInfo pi, MetaAccessor storageAccessor) {
            Delegate dset = null;
            Delegate drset = null;
            Type dgetType = typeof(DGet<,>).MakeGenericType(objectType, pi.PropertyType);
            MethodInfo getMethod = pi.GetGetMethod(true);

            Delegate dget = Delegate.CreateDelegate(dgetType, getMethod, true);
            if (dget == null) {
                throw Error.CouldNotCreateAccessorToProperty(objectType, pi.PropertyType, pi);
            }

            if (pi.CanWrite) {
                if (!objectType.IsValueType) {
                    dset = Delegate.CreateDelegate(typeof(DSet<,>).MakeGenericType(objectType, pi.PropertyType), pi.GetSetMethod(true), true);
                }
                else {
                    DynamicMethod mset = new DynamicMethod(
                        "xset_" + pi.Name,
                        typeof(void),
                        new Type[] { objectType.MakeByRefType(), pi.PropertyType },
                        true
                        );
                    ILGenerator gen = mset.GetILGenerator();
                    gen.Emit(OpCodes.Ldarg_0);
                    if (!objectType.IsValueType) {
                        gen.Emit(OpCodes.Ldind_Ref);
                    }
                    gen.Emit(OpCodes.Ldarg_1);
                    gen.Emit(OpCodes.Call, pi.GetSetMethod(true));
                    gen.Emit(OpCodes.Ret);
                    drset = mset.CreateDelegate(typeof(DRSet<,>).MakeGenericType(objectType, pi.PropertyType));
                }
            }

            Type saType = (storageAccessor != null) ? storageAccessor.Type : pi.PropertyType;
            return (MetaAccessor)Activator.CreateInstance(
                typeof(Accessor<,,>).MakeGenericType(objectType, pi.PropertyType, saType),
                BindingFlags.Instance|BindingFlags.NonPublic, null, 
                new object[] {pi, dget, dset, drset, storageAccessor}, null        
                );
            }


        class Accessor<T,V, V2> : MetaAccessor<T,V> where V2 : V {
            PropertyInfo pi;
            DGet<T, V> dget;
            DSet<T, V> dset;
            DRSet<T, V> drset;
            MetaAccessor<T, V2> storage;

            internal Accessor(PropertyInfo pi, DGet<T, V> dget, DSet<T, V> dset, DRSet<T, V> drset, MetaAccessor<T,V2> storage) {
                this.pi = pi;
                this.dget = dget;
                this.dset = dset;
                this.drset = drset;
                this.storage = storage;
            }
            public override V GetValue(T instance) {
                return this.dget(instance);
            }
            public override void SetValue(ref T instance, V value) {
                if (this.dset != null) {
                    this.dset(instance, value);
                }
                else if (this.drset != null) {
                    this.drset(ref instance, value);
                }
                else if (this.storage != null) {
                    this.storage.SetValue(ref instance, (V2)value);
                }
                else {
                    throw Error.UnableToAssignValueToReadonlyProperty(this.pi);
                }
            }
        }
    }
    
    // deferred type accessors 
    internal class LinkValueAccessor<T, V> : MetaAccessor<T, V> {
        MetaAccessor<T, Link<V>> acc;
        internal LinkValueAccessor(MetaAccessor<T, Link<V>> acc) {
            this.acc = acc;
        }
        public override bool HasValue(object instance) {
            Link<V> link = this.acc.GetValue((T)instance);
            return link.HasValue;
        }
        public override bool HasAssignedValue(object instance) {
            Link<V> link = this.acc.GetValue((T)instance);
            return link.HasAssignedValue;
        }
        public override bool HasLoadedValue(object instance) {
            Link<V> link = this.acc.GetValue((T)instance);
            return link.HasLoadedValue;
        }
        public override V GetValue(T instance) {
            Link<V> link = this.acc.GetValue(instance);
            return link.Value;
        }
        public override void SetValue(ref T instance, V value) {
            this.acc.SetValue(ref instance, new Link<V>(value));
        }
    }

    internal class LinkDefValueAccessor<T, V> : MetaAccessor<T, V> {
        MetaAccessor<T, Link<V>> acc;
        internal LinkDefValueAccessor(MetaAccessor<T, Link<V>> acc) {
            this.acc = acc;
        }
        public override V GetValue(T instance) {
            Link<V> link = this.acc.GetValue(instance);
            return link.UnderlyingValue;
        }
        public override void SetValue(ref T instance, V value) {
            this.acc.SetValue(ref instance, new Link<V>(value));
        }
    }

    internal class LinkDefSourceAccessor<T, V> : MetaAccessor<T, IEnumerable<V>> {
        MetaAccessor<T, Link<V>> acc;
        internal LinkDefSourceAccessor(MetaAccessor<T, Link<V>> acc) {
            this.acc = acc;
        }
        public override IEnumerable<V> GetValue(T instance) {
            Link<V> link = this.acc.GetValue(instance);
            return (IEnumerable<V>)link.Source;
        }
        public override void SetValue(ref T instance, IEnumerable<V> value) {
            Link<V> link = this.acc.GetValue(instance);
            if (link.HasAssignedValue || link.HasLoadedValue) {
                throw Error.LinkAlreadyLoaded();
            }
            this.acc.SetValue(ref instance, new Link<V>(value));
        }
    }

    internal class EntityRefValueAccessor<T, V> : MetaAccessor<T, V> where V : class {
        MetaAccessor<T, EntityRef<V>> acc;
        internal EntityRefValueAccessor(MetaAccessor<T, EntityRef<V>> acc) {
            this.acc = acc;
        }
        public override V GetValue(T instance) {
            EntityRef<V> er = this.acc.GetValue(instance);
            return er.Entity;
        }
        public override void SetValue(ref T instance, V value) {
            this.acc.SetValue(ref instance, new EntityRef<V>(value));
        }
        public override bool HasValue(object instance) {
            EntityRef<V> er = this.acc.GetValue((T)instance);
            return er.HasValue;
        }
        public override bool HasAssignedValue(object instance) {
            EntityRef<V> er = this.acc.GetValue((T)instance);
            return er.HasAssignedValue;
        }
        public override bool HasLoadedValue(object instance) {
            EntityRef<V> er = this.acc.GetValue((T)instance);
            return er.HasLoadedValue;
        }
    }

    internal class EntityRefDefValueAccessor<T, V> : MetaAccessor<T, V> where V : class {
        MetaAccessor<T, EntityRef<V>> acc;
        internal EntityRefDefValueAccessor(MetaAccessor<T, EntityRef<V>> acc) {
            this.acc = acc;
        }
        public override V GetValue(T instance) {
            EntityRef<V> er = this.acc.GetValue(instance);
            return er.UnderlyingValue;
        }
        public override void SetValue(ref T instance, V value) {
            this.acc.SetValue(ref instance, new EntityRef<V>(value));
        }
    }

    internal class EntityRefDefSourceAccessor<T, V> : MetaAccessor<T, IEnumerable<V>> where V : class {
        MetaAccessor<T, EntityRef<V>> acc;
        internal EntityRefDefSourceAccessor(MetaAccessor<T, EntityRef<V>> acc) {
            this.acc = acc;
        }
        public override IEnumerable<V> GetValue(T instance) {
            EntityRef<V> er = this.acc.GetValue(instance);
            return (IEnumerable<V>)er.Source;
        }
        public override void SetValue(ref T instance, IEnumerable<V> value) {
            EntityRef<V> er = this.acc.GetValue(instance);
            if (er.HasAssignedValue || er.HasLoadedValue) {
                throw Error.EntityRefAlreadyLoaded();
            }
            this.acc.SetValue(ref instance, new EntityRef<V>(value));
        }
    }

    internal class EntitySetValueAccessor<T, V> : MetaAccessor<T, EntitySet<V>> where V : class {
        MetaAccessor<T, EntitySet<V>> acc;
        internal EntitySetValueAccessor(MetaAccessor<T, EntitySet<V>> acc) {
            this.acc = acc;
        }
        public override EntitySet<V> GetValue(T instance) {
            return this.acc.GetValue(instance);
        }
        public override void SetValue(ref T instance, EntitySet<V> value) {
            EntitySet<V> eset = this.acc.GetValue(instance);
            if (eset == null) {
                eset = new EntitySet<V>();
                this.acc.SetValue(ref instance, eset);
            }
            eset.Assign(value);
        }
        public override bool HasValue(object instance) {
            EntitySet<V> es = this.acc.GetValue((T)instance);
            return es != null && es.HasValues;
        }
        public override bool HasAssignedValue(object instance) {
            EntitySet<V> es = this.acc.GetValue((T)instance);
            return es != null && es.HasAssignedValues;
        }
        public override bool HasLoadedValue(object instance) {
            EntitySet<V> es = this.acc.GetValue((T)instance);
            return es != null && es.HasLoadedValues;
        }
    }

    internal class EntitySetDefValueAccessor<T, V> : MetaAccessor<T, IEnumerable<V>> where V : class {
        MetaAccessor<T, EntitySet<V>> acc;
        internal EntitySetDefValueAccessor(MetaAccessor<T, EntitySet<V>> acc) {
            this.acc = acc;
        }
        public override IEnumerable<V> GetValue(T instance) {
            EntitySet<V> eset = this.acc.GetValue(instance);
            return eset.GetUnderlyingValues();
        }
        public override void SetValue(ref T instance, IEnumerable<V> value) {
            EntitySet<V> eset = this.acc.GetValue(instance);
            if (eset == null) {
                eset = new EntitySet<V>();
                this.acc.SetValue(ref instance, eset);
            }
            eset.Assign(value);
        }
    }

    internal class EntitySetDefSourceAccessor<T, V> : MetaAccessor<T, IEnumerable<V>> where V : class {
        MetaAccessor<T, EntitySet<V>> acc;
        internal EntitySetDefSourceAccessor(MetaAccessor<T, EntitySet<V>> acc) {
            this.acc = acc;
        }
        public override IEnumerable<V> GetValue(T instance) {
            EntitySet<V> eset = this.acc.GetValue(instance);
            return (IEnumerable<V>)eset.Source;
        }
        public override void SetValue(ref T instance, IEnumerable<V> value) {
            EntitySet<V> eset = this.acc.GetValue(instance);
            if (eset == null) {
                eset = new EntitySet<V>();
                this.acc.SetValue(ref instance, eset);
            }
            eset.SetSource(value);
        }
    }
}
