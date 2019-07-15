// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#if MOBILE
namespace System.Data.Common
{
    public static partial class DbProviderFactories
    {
        public static System.Data.Common.DbProviderFactory GetFactory(System.Data.Common.DbConnection connection) => throw new PlatformNotSupportedException();
        public static System.Data.Common.DbProviderFactory GetFactory(System.Data.DataRow providerRow) => throw new PlatformNotSupportedException();
        public static System.Data.Common.DbProviderFactory GetFactory(string providerInvariantName) => throw new PlatformNotSupportedException();
        public static System.Data.DataTable GetFactoryClasses() => throw new PlatformNotSupportedException();
        public static System.Collections.Generic.IEnumerable<string> GetProviderInvariantNames() => throw new PlatformNotSupportedException();
        public static void RegisterFactory(string providerInvariantName, System.Data.Common.DbProviderFactory factory) => throw new PlatformNotSupportedException();
        public static void RegisterFactory(string providerInvariantName, string factoryTypeAssemblyQualifiedName) => throw new PlatformNotSupportedException();
        public static void RegisterFactory(string providerInvariantName, System.Type providerFactoryClass) => throw new PlatformNotSupportedException();
        public static bool TryGetFactory(string providerInvariantName, out System.Data.Common.DbProviderFactory factory) => throw new PlatformNotSupportedException();
        public static bool UnregisterFactory(string providerInvariantName) => throw new PlatformNotSupportedException();
    }
}
#endif