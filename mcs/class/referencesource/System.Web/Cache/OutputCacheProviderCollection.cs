using System;
using System.Configuration.Provider;
using System.Security.Permissions;
using System.Web;

namespace System.Web.Caching {
    public sealed class OutputCacheProviderCollection : ProviderCollection {

        new public OutputCacheProvider this[string name] {
            get {
                return (OutputCacheProvider) base[name];
            }
        }
        
        public override void Add(ProviderBase provider) {
            if (provider == null) {
                throw new ArgumentNullException( "provider" );
            }
            
            if (!(provider is OutputCacheProvider)) {
                throw new ArgumentException(SR.GetString(SR.Provider_must_implement_type, typeof(OutputCacheProvider).Name),
                                            "provider");
            }
            
            base.Add(provider);
        }
        
        public void CopyTo(OutputCacheProvider[] array, int index) {
            base.CopyTo(array, index);
        }
    }
}
