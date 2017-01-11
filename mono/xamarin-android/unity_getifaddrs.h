#ifndef __UNITY_MONO_GETIFADDRS_H
#define __UNITY_MONO_GETIFADDRS_H

#include <glib.h>
#include "mono/utils/mono-compiler.h"

void
ves_icall_System_Net_NetworkInformation_LinuxNetworkInterface_InitializeInterfaceAddresses () MONO_INTERNAL;

gint32
ves_icall_System_Net_NetworkInformation_LinuxNetworkInterface_GetInterfaceAddresses (gpointer* ptr) MONO_INTERNAL;

void
ves_icall_System_Net_NetworkInformation_LinuxNetworkInterface_FreeInterfaceAddresses (gpointer ptr) MONO_INTERNAL;

#endif
