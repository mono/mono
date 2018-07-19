
using System.Diagnostics.CodeAnalysis;

namespace System.Net.NetworkInformation
{
    // See SCOPE_LEVEL
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags",
        Justification = "This enum does not represent combinable flags")]
    public enum ScopeLevel
    {
        None = 0,
        Interface = 1,
        Link = 2,
        Subnet = 3,
        Admin = 4,
        Site = 5,
        Organization = 8,
        Global = 14,
    }
}
