using System;
using System.Web;
using Microsoft.Web.Services;

namespace Microsoft.Web.Services.Messaging {

        public abstract class SoapReceiver : SoapPort, IHttpHandler
        {
		[MonoTODO]
                public bool IsReusable {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                [MonoTODO]
                public void ProcessRequest (HttpContext context)
                {
                        throw new NotImplementedException ();
                }
        }
}
