
using System.Threading.Tasks;

namespace System.Xml {

    public abstract partial class XmlResolver {

        public virtual Task<Object> GetEntityAsync(Uri absoluteUri,
                                             string role,
                                             Type ofObjectToReturn) {

            throw new NotImplementedException();
        }
    }
}
