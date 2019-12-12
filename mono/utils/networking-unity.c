#include <config.h>
#include <glib.h>


#include <mono/utils/networking.h>

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

#include "Socket-c-api.h"

static void
add_hostent(MonoAddressInfo *info, int flags, const char* name, gint family, char** aliases, void** addresses, int32_t addressSize)
{
    MonoAddressEntry *cur, *prev = info->entries;
    int idx = 0;
    int address_length = 0;

    if (!info->aliases)
        info->aliases = g_strdupv(aliases);

    while (addresses[idx]) {
        cur = g_new0(MonoAddressEntry, 1);
        if (prev)
            prev->next = cur;
        else
            info->entries = cur;

        if (flags & MONO_HINT_CANONICAL_NAME && name)
            cur->canonical_name = g_strdup(name);

        cur->family = family;
        cur->socktype = SOCK_STREAM;
        cur->protocol = 0; /* Zero means the default stream protocol */
        address_length = addressSize;
        cur->address_len = address_length;
        memcpy(&cur->address, addresses[idx], address_length);

        prev = cur;
        ++idx;
    }
}

static void free_null_terminated_array (void** array)
{
    if (array != NULL)
    {
        int i = 0;
        while (array[i] != NULL)
        {
            g_free(array[i]);
            i++;
        }
    }
    g_free(array);
}

int
mono_get_address_info(const char *hostname, int port, int flags, MonoAddressInfo **result)
{
    MonoAddressInfo *addr_info;
    addr_info = g_new0(MonoAddressInfo, 1);

    char* name;
    gint family;
    char** aliases;
    void** addresses;
    int32_t addressSize;

    if (UnityPalGetHostByName(hostname, &name, &family, &aliases, &addresses, &addressSize) == kWaitStatusSuccess)
        add_hostent(addr_info, flags, name, family, aliases, addresses, addressSize);

    g_free(name);
    free_null_terminated_array(aliases);
    free_null_terminated_array(addresses);

    if (!addr_info->entries) {
        *result = NULL;
        mono_free_address_info(addr_info);
        return 1;
    }

    *result = addr_info;
    return 0;
}

void *
mono_get_local_interfaces (int family, int *interface_count)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

gboolean
mono_networking_addr_to_str (MonoAddress *address, char *buffer, socklen_t buflen)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

int
mono_networking_get_tcp_protocol (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

int
mono_networking_get_ip_protocol (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

int
mono_networking_get_ipv6_protocol (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

#endif /* PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS */
