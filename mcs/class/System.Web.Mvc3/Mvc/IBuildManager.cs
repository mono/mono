namespace System.Web.Mvc {
    using System.Collections;
    using System.IO;

    internal interface IBuildManager {
        bool FileExists(string virtualPath);
        Type GetCompiledType(string virtualPath);
        ICollection GetReferencedAssemblies();
        Stream ReadCachedFile(string fileName);
        Stream CreateCachedFile(string fileName);
    }
}
