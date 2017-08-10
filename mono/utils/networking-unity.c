#include <mono/utils/networking.h>

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

int
mono_get_address_info (const char *hostname, int port, int flags, MonoAddressInfo **result)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
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
