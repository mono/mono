using System.Security.Permissions;

namespace System.Net {
	internal static class ExceptionHelper
    {
        internal static readonly WebPermission WebPermissionUnrestricted = new WebPermission(NetworkAccess.Connect);
    }
}