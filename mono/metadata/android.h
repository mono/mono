
#ifndef __MONO_METADATA_ANDROID_H__
#define __MONO_METADATA_ANDROID_H__

#include <glib.h>

#include <jni.h>

#include "object.h"

#include "utils/mono-publib.h"

/* We're implementing getifaddrs behavior, this is the structure we use. It is exactly the same as
 * struct ifaddrs defined in ifaddrs.h but since bionics doesn't have it we need to mirror it here.
 */
struct _monodroid_ifaddrs {
	struct _monodroid_ifaddrs *ifa_next; /* Pointer to the next structure.      */

	gchar *ifa_name;                      /* Name of this network interface.     */
	guint ifa_flags;              /* Flags as from SIOCGIFFLAGS ioctl.   */

	struct sockaddr *ifa_addr;           /* Network address of this interface.  */
	struct sockaddr *ifa_netmask;        /* Netmask of this interface.          */
	union {
		/* At most one of the following two is valid.  If the IFF_BROADCAST
		   bit is set in `ifa_flags', then `ifa_broadaddr' is valid.  If the
		   IFF_POINTOPOINT bit is set, then `ifa_dstaddr' is valid.
		   It is never the case that both these bits are set at once.  */
		struct sockaddr *ifu_broadaddr;  /* Broadcast address of this interface. */
		struct sockaddr *ifu_dstaddr;    /* Point-to-point destination address.  */
	} ifa_ifu;
	/* These very same macros are defined by <net/if.h> for `struct ifaddr'.
	   So if they are defined already, the existing definitions will be fine.  */
# ifndef _monodroid_ifa_broadaddr
#  define _monodroid_ifa_broadaddr ifa_ifu.ifu_broadaddr
# endif
# ifndef _monodroid_ifa_dstaddr
#  define _monodroid_ifa_dstaddr   ifa_ifu.ifu_dstaddr
# endif

	gpointer ifa_data;               /* Address-specific data (may be unused).  */
};

MONO_API void
mono_jvm_initialize (JavaVM *vm);

JNIEnv*
mono_jvm_get_jnienv (void);

MONO_API void
monodroid_add_system_property (const gchar *name, const gchar *value);

MONO_API gint32
monodroid_get_system_property (const gchar *name, gchar **value);

gpointer
ves_icall_System_TimezoneInfo_AndroidTimeZones_GetDefaultTimeZoneId (void);

gint32
ves_icall_System_Net_NetworkInformation_NetworkInterfaceFactory_UnixNetworkInterfaceAPI_getifaddrs (struct _monodroid_ifaddrs **ifap);

void
ves_icall_System_Net_NetworkInformation_NetworkInterfaceFactory_UnixNetworkInterfaceAPI_freeifaddrs (struct _monodroid_ifaddrs *ifa);

void
ves_icall_Mono_Unix_Android_AndroidUtils_DetectCpuAndArchitecture (guint16 *built_for_cpu, guint16 *running_on_cpu, MonoBoolean *is64bit);

gint32
ves_icall_System_TimezoneInfo_AndroidTimeZones_GetSystemProperty (const gchar *name, gchar **value);

gint32
ves_icall_System_Net_NetworkInformation_UnixIPInterfaceProperties_GetDNSServers (gpointer *dns_servers_array);

#endif /* __MONO_METADATA_ANDROID_H__ */
