
using System.Security.Permissions;
using System.Threading.Tasks;

namespace System.Xml {

    [PermissionSetAttribute(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public partial class XmlSecureResolver : XmlResolver {
        public override Task<object> GetEntityAsync(Uri absoluteUri, string role, Type ofObjectToReturn) {
#if MONO_FEATURE_CAS
            permissionSet.PermitOnly();
#endif
            return resolver.GetEntityAsync(absoluteUri, role, ofObjectToReturn);
        }
    }
}
