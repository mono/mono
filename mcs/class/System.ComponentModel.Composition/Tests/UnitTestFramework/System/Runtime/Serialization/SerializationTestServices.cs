// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
#if !SILVERLIGHT

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Runtime.Serialization
{
    public static class SerializationTestServices
    {
        /// <summary>
        ///     Serializes and then deserializes the specified value.
        /// </summary>
        public static T RoundTrip<T>(T value)
        {
            Assert.IsNotNull(value);

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, value);

                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        ///     Creates an instance of a type using the serialization constructor.
        /// </summary>
        public static T Create<T>(SerializationInfo info, StreamingContext context)
        {
            ConstructorInfo constructor = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                                                                   new Type[] { typeof(SerializationInfo), typeof(StreamingContext) },
                                                                   (ParameterModifier[])null);

            Assert.IsNotNull(constructor, "Type does not have a private or protected serialization constructor.");

            try
            {
                return (T)constructor.Invoke(new object[] { info, context });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>
        ///     Returns a new instance of <see cref="SerializationInfo"/> replacing the specified member name with the specified value.
        /// </summary>
        public static SerializationInfo CreateSerializationInfoReplacingMember<T>(string memberName, object value)
            where T : ISerializable, new()
        {
            return CreateSerializationInfoReplacingMember(memberName, value, () => new T()); 
        }

        /// <summary>
        ///     Returns a new instance of <see cref="SerializationInfo"/> replacing the specified member name with the specified value.
        /// </summary>
        public static SerializationInfo CreateSerializationInfoReplacingMember<T>(string memberName, object value, Func<T> creator)
            where T : ISerializable
        {
            T serializableObject = creator();

            var info = GetObjectDataFrom(serializableObject);

            return CloneReplacingMember<T>(info, memberName, value);
        }

        /// <summary>
        ///     Returns a new instance of <see cref="SerializationInfo"/> removing the specified member name.
        /// </summary>
        public static SerializationInfo CreateSerializationInfoRemovingMember<T>(string memberName)
            where T : ISerializable, new()
        {
            return CreateSerializationInfoRemovingMember(memberName, () => new T());
        }

        /// <summary>
        ///     Returns a new instance of <see cref="SerializationInfo"/> removing the specified member name.
        /// </summary>
        public static SerializationInfo CreateSerializationInfoRemovingMember<T>(string memberName, Func<T> creator)
            where T : ISerializable
        {
            T serializableObject = creator();

            var info = GetObjectDataFrom(serializableObject);

            return CloneRemovingMember<T>(info, memberName);
        }

        private static SerializationInfo CloneReplacingMember<T>(SerializationInfo info, string memberName, object value)
        {
            return Clone<T>(info, (entry, clone) =>
            {
                if (entry.Name != memberName)
                {
                    return true;
                }

                // Replace the entry
                clone.AddValue(entry.Name, value, value == null ? entry.ObjectType : value.GetType());
                return false;
            });
        }

        private static SerializationInfo CloneRemovingMember<T>(SerializationInfo info, string memberName)
        {
            return Clone<T>(info, (entry, clone) =>
            {
                // Add everything except the member we want to remove
                return entry.Name != memberName;
            });
        }

        private static SerializationInfo Clone<T>(SerializationInfo info, Func<SerializationEntry, SerializationInfo, bool> predicate)
        {
            var clone = GetEmptySerializationInfo<T>();

            foreach (var entry in info)
            {
                if (predicate(entry, clone))
                {
                    clone.AddValue(entry.Name, entry.Value, entry.ObjectType);
                }
            }

            return clone;
        }

        private static SerializationInfo GetObjectDataFrom<T>(T serializableObject) where T : ISerializable
        {
            var info = GetEmptySerializationInfo<T>();

            serializableObject.GetObjectData(info, new StreamingContext());

            return info;
        }

        private static SerializationInfo GetEmptySerializationInfo<T>()
        {
            StrictFormatterConverter converter = new StrictFormatterConverter();

            return new SerializationInfo(typeof(T), converter);
        }
    }
}

#endif // !SILVERLIGHT