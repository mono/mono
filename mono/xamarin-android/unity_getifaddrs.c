#include "unity_getifaddrs.h"
#include "xamarin_getifaddrs.h"

void
ves_icall_System_Net_NetworkInformation_LinuxNetworkInterface_InitializeInterfaceAddresses ()
{
    _monodroid_getifaddrs_init ();
}

gint32
ves_icall_System_Net_NetworkInformation_LinuxNetworkInterface_GetInterfaceAddresses (gpointer* ptr)
{
    return _monodroid_getifaddrs ((struct _monodroid_ifaddrs **)ptr);
}

void
ves_icall_System_Net_NetworkInformation_LinuxNetworkInterface_FreeInterfaceAddresses (gpointer ptr)
{
    _monodroid_freeifaddrs ((struct _monodroid_ifaddrs *)ptr);
}
