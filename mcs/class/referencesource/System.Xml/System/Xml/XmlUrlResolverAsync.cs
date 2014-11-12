
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace System.Xml
{
    public partial class XmlUrlResolver : XmlResolver {

        // Maps a URI to an Object containing the actual resource.
        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        public override async Task<Object> GetEntityAsync(Uri absoluteUri, string role, Type ofObjectToReturn) {
            if (ofObjectToReturn == null || ofObjectToReturn == typeof(System.IO.Stream) || ofObjectToReturn == typeof(System.Object)) {
                return await DownloadManager.GetStreamAsync(absoluteUri, _credentials, _proxy, _cachePolicy).ConfigureAwait(false);
            }
            else {
                throw new XmlException(Res.Xml_UnsupportedClass, string.Empty);
            }
        }
    }
}
