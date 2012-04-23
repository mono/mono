namespace System.Web.Mvc {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal static class TypeCacheUtil {

        private static IEnumerable<Type> FilterTypesInAssemblies(IBuildManager buildManager, Predicate<Type> predicate) {
            // Go through all assemblies referenced by the application and search for types matching a predicate
            IEnumerable<Type> typesSoFar = Type.EmptyTypes;

            ICollection assemblies = buildManager.GetReferencedAssemblies();
            foreach (Assembly assembly in assemblies) {
                Type[] typesInAsm;
                try {
                    typesInAsm = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex) {
                    typesInAsm = ex.Types;
                }
                typesSoFar = typesSoFar.Concat(typesInAsm);
            }
            return typesSoFar.Where(type => TypeIsPublicClass(type) && predicate(type));
        }

        public static List<Type> GetFilteredTypesFromAssemblies(string cacheName, Predicate<Type> predicate, IBuildManager buildManager) {
            TypeCacheSerializer serializer = new TypeCacheSerializer();

            // first, try reading from the cache on disk
            List<Type> matchingTypes = ReadTypesFromCache(cacheName, predicate, buildManager, serializer);
            if (matchingTypes != null) {
                return matchingTypes;
            }

            // if reading from the cache failed, enumerate over every assembly looking for a matching type
            matchingTypes = FilterTypesInAssemblies(buildManager, predicate).ToList();

            // finally, save the cache back to disk
            SaveTypesToCache(cacheName, matchingTypes, buildManager, serializer);

            return matchingTypes;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Cache failures are not fatal, and the code should continue executing normally.")]
        internal static List<Type> ReadTypesFromCache(string cacheName, Predicate<Type> predicate, IBuildManager buildManager, TypeCacheSerializer serializer) {
            try {
                Stream stream = buildManager.ReadCachedFile(cacheName);
                if (stream != null) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        List<Type> deserializedTypes = serializer.DeserializeTypes(reader);
                        if (deserializedTypes != null && deserializedTypes.All(type => TypeIsPublicClass(type) && predicate(type))) {
                            // If all read types still match the predicate, success!
                            return deserializedTypes;
                        }
                    }
                }
            }
            catch {
            }

            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Cache failures are not fatal, and the code should continue executing normally.")]
        internal static void SaveTypesToCache(string cacheName, IList<Type> matchingTypes, IBuildManager buildManager, TypeCacheSerializer serializer) {
            try {
                Stream stream = buildManager.CreateCachedFile(cacheName);
                if (stream != null) {
                    using (StreamWriter writer = new StreamWriter(stream)) {
                        serializer.SerializeTypes(matchingTypes, writer);
                    }
                }
            }
            catch {
            }
        }

        private static bool TypeIsPublicClass(Type type) {
            return (type != null && type.IsPublic && type.IsClass && !type.IsAbstract);
        }

    }
}
