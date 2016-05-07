using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;
using System.Web;

namespace System.Web.Caching {
    public interface IOutputCacheEntry {

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This can be set by the user to support inline dictionary intializers")]
        List<HeaderElement>        HeaderElements    { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This can be set by the user to support inline dictionary intializers")]
        List<ResponseElement>      ResponseElements  { get; set; }
    }
}

