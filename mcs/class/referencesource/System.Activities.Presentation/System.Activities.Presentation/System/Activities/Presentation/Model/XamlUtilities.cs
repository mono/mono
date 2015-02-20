//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------


namespace System.Activities.Presentation.Model
{

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;


    internal static class XamlUtilities
    {

        static Hashtable converterCache;
        static object converterCacheSyncObject = new object();

        public static TypeConverter GetConverter(Type itemType)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(itemType);

            if (converter == null || converter.GetType() == typeof(TypeConverter))
            {

                // We got an invalid converter.  WPF will do this if the converter
                // is internal, but we use internal converters all over the place
                // at design time.  Detect this and build the converter ourselves.

                if (converterCache != null)
                {
                    converter = (TypeConverter)converterCache[itemType];
                    if (converter != null)
                    {
                        return converter;
                    }
                }

                AttributeCollection attrs = TypeDescriptor.GetAttributes(itemType);
                TypeConverterAttribute tca = attrs[typeof(TypeConverterAttribute)] as TypeConverterAttribute;
                if (tca != null && tca.ConverterTypeName != null)
                {
                    Type type = Type.GetType(tca.ConverterTypeName);
                    if (type != null && !type.IsPublic && typeof(TypeConverter).IsAssignableFrom(type))
                    {
                        ConstructorInfo ctor = type.GetConstructor(new Type[] { typeof(Type) });
                        if (ctor != null)
                        {
                            converter = (TypeConverter)ctor.Invoke(new object[] { itemType });
                        }
                        else
                        {
                            converter = (TypeConverter)Activator.CreateInstance(type);
                        }

                        lock (converterCacheSyncObject)
                        {
                            if (converterCache == null)
                            {
                                converterCache = new Hashtable();

                                // Listen to type changes and clear the cache.
                                // This allows new metadata tables to be installed

                                TypeDescriptor.Refreshed += delegate(RefreshEventArgs args)
                                {
                                    converterCache.Remove(args.TypeChanged);
                                };
                            }

                            converterCache[itemType] = converter;
                        }
                    }
                }
            }

            return converter;
        }
    }
}
