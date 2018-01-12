
#include <glib.h>

#include "android.h"

#if LINUX

#include <errno.h>
#include <dlfcn.h>
#include <limits.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>

#include <sys/types.h>
#include <sys/socket.h>

#include <unistd.h>

#include <linux/netlink.h>
#include <linux/rtnetlink.h>
#include <linux/if_arp.h>

#include <netinet/in.h>

#include "utils/mono-lazy-init.h"
#include "utils/mono-logger-internals.h"

/* Some of these aren't defined in android's rtnetlink.h (as of ndk 16). We define values for all of
 * them if they aren't found so that the debug code works properly. We could skip them but future
 * versions of the NDK might include definitions for them.
 * Values are taken from Linux headers shipped with glibc
 */
#ifndef IFLA_UNSPEC
#define IFLA_UNSPEC 0
#endif

#ifndef IFLA_ADDRESS
#define IFLA_ADDRESS 1
#endif

#ifndef IFLA_BROADCAST
#define IFLA_BROADCAST 2
#endif

#ifndef IFLA_IFNAME
#define IFLA_IFNAME 3
#endif

#ifndef IFLA_MTU
#define IFLA_MTU 4
#endif

#ifndef IFLA_LINK
#define IFLA_LINK 5
#endif

#ifndef IFLA_QDISC
#define IFLA_QDISC 6
#endif

#ifndef IFLA_STATS
#define IFLA_STATS 7
#endif

#ifndef IFLA_COST
#define IFLA_COST 8
#endif

#ifndef IFLA_PRIORITY
#define IFLA_PRIORITY 9
#endif

#ifndef IFLA_MASTER
#define IFLA_MASTER 10
#endif

#ifndef IFLA_WIRELESS
#define IFLA_WIRELESS 11
#endif

#ifndef IFLA_PROTINFO
#define IFLA_PROTINFO 12
#endif

#ifndef IFLA_TXQLEN
#define IFLA_TXQLEN 13
#endif

#ifndef IFLA_MAP
#define IFLA_MAP 14
#endif

#ifndef IFLA_WEIGHT
#define IFLA_WEIGHT 15
#endif

#ifndef IFLA_OPERSTATE
#define IFLA_OPERSTATE 16
#endif

#ifndef IFLA_LINKMODE
#define IFLA_LINKMODE 17
#endif

#ifndef IFLA_LINKINFO
#define IFLA_LINKINFO 18
#endif

#ifndef IFLA_NET_NS_PID
#define IFLA_NET_NS_PID 19
#endif

#ifndef IFLA_IFALIAS
#define IFLA_IFALIAS 20
#endif

#ifndef IFLA_NUM_VF
#define IFLA_NUM_VF 21
#endif

#ifndef IFLA_VFINFO_LIST
#define IFLA_VFINFO_LIST 22
#endif

#ifndef IFLA_STATS64
#define IFLA_STATS64 23
#endif

#ifndef IFLA_VF_PORTS
#define IFLA_VF_PORTS 24
#endif

#ifndef IFLA_PORT_SELF
#define IFLA_PORT_SELF 25
#endif

#ifndef IFLA_AF_SPEC
#define IFLA_AF_SPEC 26
#endif

#ifndef IFLA_GROUP
#define IFLA_GROUP 27
#endif

#ifndef IFLA_NET_NS_FD
#define IFLA_NET_NS_FD 28
#endif

#ifndef IFLA_EXT_MASK
#define IFLA_EXT_MASK 29
#endif

#ifndef IFLA_PROMISCUITY
#define IFLA_PROMISCUITY 30
#endif

#ifndef IFLA_NUM_TX_QUEUES
#define IFLA_NUM_TX_QUEUES 31
#endif

#ifndef IFLA_NUM_RX_QUEUES
#define IFLA_NUM_RX_QUEUES 32
#endif

#ifndef IFLA_CARRIER
#define IFLA_CARRIER 33
#endif

#ifndef IFLA_PHYS_PORT_ID
#define IFLA_PHYS_PORT_ID 34
#endif

#ifndef IFLA_CARRIER_CHANGES
#define IFLA_CARRIER_CHANGES 35
#endif

#ifndef IFLA_PHYS_SWITCH_ID
#define IFLA_PHYS_SWITCH_ID 36
#endif

#ifndef IFLA_LINK_NETNSID
#define IFLA_LINK_NETNSID 37
#endif

#ifndef IFLA_PHYS_PORT_NAME
#define IFLA_PHYS_PORT_NAME 38
#endif

#ifndef IFLA_PROTO_DOWN
#define IFLA_PROTO_DOWN 39
#endif

#ifndef IFLA_GSO_MAX_SEGS
#define IFLA_GSO_MAX_SEGS 40
#endif

#ifndef IFLA_GSO_MAX_SIZE
#define IFLA_GSO_MAX_SIZE 41
#endif

#ifndef IFLA_PAD
#define IFLA_PAD 42
#endif

#ifndef IFLA_XDP
#define IFLA_XDP 43
#endif

/* Maximum interface address label size, should be more than enough */
#define MAX_IFA_LABEL_SIZE 1024

/* This is the message we send to the kernel */
typedef struct {
	struct nlmsghdr header;
	struct rtgenmsg message;
} netlink_request;

typedef struct {
	gint sock_fd;
	gint seq;
	struct sockaddr_nl them; /* kernel end */
	struct sockaddr_nl us; /* our end */
	struct msghdr message_header; /* for use with sendmsg */
	struct iovec payload_vector; /* Used to send netlink_request */
} netlink_session;

/* Turns out that quite a few link types have address length bigger than the 8 bytes allocated in
 * this structure as defined by the OS. Examples are Infiniband or ipv6 tunnel devices
 */
struct sockaddr_ll_extended {
    gushort sll_family;
    gushort sll_protocol;
    gint sll_ifindex;
    gushort sll_hatype;
    guchar sll_pkttype;
    guchar sll_halen;
    guchar sll_addr[24];
};

static gint
parse_netlink_reply (netlink_session *session, struct _monodroid_ifaddrs **ifaddrs_head, struct _monodroid_ifaddrs **last_ifaddr);

static struct _monodroid_ifaddrs*
get_link_info (const struct nlmsghdr *message);

static struct _monodroid_ifaddrs *
get_link_address (const struct nlmsghdr *message, struct _monodroid_ifaddrs **ifaddrs_head);

static gint
open_netlink_session (netlink_session *session);

static gint
send_netlink_dump_request (netlink_session *session, gint type);

static gint
append_ifaddr (struct _monodroid_ifaddrs *addr, struct _monodroid_ifaddrs **ifaddrs_head, struct _monodroid_ifaddrs **last_ifaddr);

static gint
fill_ll_address (struct sockaddr_ll_extended **sa, struct ifinfomsg *net_interface, gpointer rta_data, gint rta_payload_length);

static gint
fill_sa_address (struct sockaddr **sa, struct ifaddrmsg *net_address, gpointer rta_data, gint rta_payload_length);

static void
free_single_xamarin_ifaddrs (struct _monodroid_ifaddrs **ifap);

static void
get_ifaddrs_impl (gint (**getifaddrs_impl) (struct _monodroid_ifaddrs **ifap), void (**freeifaddrs_impl) (struct _monodroid_ifaddrs *ifa));

static struct _monodroid_ifaddrs*
find_interface_by_index (gint index, struct _monodroid_ifaddrs **ifaddrs_head);

static gchar*
get_interface_name_by_index (gint index, struct _monodroid_ifaddrs **ifaddrs_head);

static gint
get_interface_flags_by_index (gint index, struct _monodroid_ifaddrs **ifaddrs_head);

static gint
calculate_address_netmask (struct _monodroid_ifaddrs *ifa, struct ifaddrmsg *net_address);

#if DEBUG
static void
print_ifla_name (gint id);

static void
print_address_list (gchar *title, struct _monodroid_ifaddrs *list);
#endif

/* We don't use 'struct ifaddrs' since that doesn't exist in Android's bionic, but since our
 * version of the structure is 100% compatible we can just use it instead
 */
typedef gint (*getifaddrs_impl_fptr)(struct _monodroid_ifaddrs **);
typedef void (*freeifaddrs_impl_fptr)(struct _monodroid_ifaddrs *ifa);

static mono_lazy_init_t initialized = MONO_LAZY_INIT_STATUS_NOT_INITIALIZED;

static getifaddrs_impl_fptr getifaddrs_impl = NULL;
static freeifaddrs_impl_fptr freeifaddrs_impl = NULL;

static void
initialize (void)
{
	get_ifaddrs_impl (&getifaddrs_impl, &freeifaddrs_impl);
}

static void
_monodroid_freeifaddrs (struct _monodroid_ifaddrs *ifa)
{
	struct _monodroid_ifaddrs *cur, *next;

	if (!ifa)
		return;

	if (freeifaddrs_impl) {
		(*freeifaddrs_impl)(ifa);
		return;
	}

#if DEBUG
	print_address_list ("List passed to freeifaddrs", ifa);
#endif
	cur = ifa;
	while (cur) {
		next = cur->ifa_next;
		free_single_xamarin_ifaddrs (&cur);
		cur = next;
	}
}

gint
ves_icall_System_Net_NetworkInformation_NetworkInterfaceFactory_UnixNetworkInterfaceAPI_getifaddrs (struct _monodroid_ifaddrs **ifap)
{
	gint ret = -1;

	mono_lazy_initialize (&initialized, initialize);

	if (getifaddrs_impl)
		return (*getifaddrs_impl)(ifap);

	if (!ifap)
		return ret;

	*ifap = NULL;
	struct _monodroid_ifaddrs *ifaddrs_head = 0;
	struct _monodroid_ifaddrs *last_ifaddr = 0;
	netlink_session session;

	if (open_netlink_session (&session) < 0) {
		goto cleanup;
	}

	/* Request information about the specified link. In our case it will be all of them since we
	   request the root of the link tree below
	*/
	if ((send_netlink_dump_request (&session, RTM_GETLINK) < 0) ||
			(parse_netlink_reply (&session, &ifaddrs_head, &last_ifaddr) < 0) ||
			(send_netlink_dump_request (&session, RTM_GETADDR) < 0) ||
			(parse_netlink_reply (&session, &ifaddrs_head, &last_ifaddr) < 0)) {
		_monodroid_freeifaddrs (ifaddrs_head);
		goto cleanup;
	}

	ret = 0;
	*ifap = ifaddrs_head;
#if DEBUG
	print_address_list ("Initial interfaces list", *ifap);
#endif

  cleanup:
	if (session.sock_fd >= 0) {
		close (session.sock_fd);
		session.sock_fd = -1;
	}

	return ret;
}

void
ves_icall_System_Net_NetworkInformation_NetworkInterfaceFactory_UnixNetworkInterfaceAPI_freeifaddrs (struct _monodroid_ifaddrs *ifa)
{
	_monodroid_freeifaddrs (ifa);
}

static void
get_ifaddrs_impl (gint (**getifaddrs_impl) (struct _monodroid_ifaddrs **ifap), void (**freeifaddrs_impl) (struct _monodroid_ifaddrs *ifa))
{
	gpointer libc;

	g_assert (getifaddrs_impl);
	g_assert (freeifaddrs_impl);

	libc = dlopen ("libc.so", RTLD_NOW);
	if (libc) {
		*getifaddrs_impl = dlsym (libc, "getifaddrs");
		if (*getifaddrs_impl)
			*freeifaddrs_impl = dlsym (libc, "freeifaddrs");
	}

	if (!*getifaddrs_impl)
		mono_trace (G_LOG_LEVEL_MESSAGE, MONO_TRACE_ANDROID_NET, "This libc does not have getifaddrs/freeifaddrs, using Xamarin's");
	else
		mono_trace (G_LOG_LEVEL_MESSAGE, MONO_TRACE_ANDROID_NET, "This libc has getifaddrs/freeifaddrs");
}

static void
free_single_xamarin_ifaddrs (struct _monodroid_ifaddrs **ifap)
{
	struct _monodroid_ifaddrs *ifa = ifap ? *ifap : NULL;
	if (!ifa)
		return;

	if (ifa->ifa_name)
		free (ifa->ifa_name);

	if (ifa->ifa_addr)
		free (ifa->ifa_addr);

	if (ifa->ifa_netmask)
		free (ifa->ifa_netmask);

	if (ifa->_monodroid_ifa_broadaddr)
		free (ifa->_monodroid_ifa_broadaddr);

	if (ifa->ifa_data)
		free (ifa->ifa_data);

	free (ifa);
	*ifap = NULL;
}

static gint
open_netlink_session (netlink_session *session)
{
	g_assert (session != 0);

	memset (session, 0, sizeof (*session));
	session->sock_fd = socket (AF_NETLINK, SOCK_RAW, NETLINK_ROUTE);
	if (session->sock_fd == -1) {
		mono_trace (G_LOG_LEVEL_WARNING, MONO_TRACE_ANDROID_NETLINK, "Failed to create a netlink socket. %s\n", strerror (errno));
		return -1;
	}

	/* Fill out addresses */
	session->us.nl_family = AF_NETLINK;

	/* We have previously used `getpid()` here but it turns out that WebView/Chromium does the same
	   and there can only be one session with the same PID. Setting it to 0 will cause the kernel to
	   assign some PID that's unique and valid instead.

	   See: https://bugzilla.xamarin.com/show_bug.cgi?id=41860
	*/
	session->us.nl_pid = 0;
	session->us.nl_groups = 0;

	session->them.nl_family = AF_NETLINK;

	if (bind (session->sock_fd, (struct sockaddr *)&session->us, sizeof (session->us)) < 0) {
		mono_trace (G_LOG_LEVEL_WARNING, MONO_TRACE_ANDROID_NETLINK, "Failed to bind to the netlink socket. %s\n", strerror (errno));
		return -1;
	}

	return 0;
}

static gint
send_netlink_dump_request (netlink_session *session, gint type)
{
	netlink_request request;

	memset (&request, 0, sizeof (request));
	request.header.nlmsg_len = NLMSG_LENGTH (sizeof (struct rtgenmsg));
	/* Flags (from netlink.h):
	   NLM_F_REQUEST - it's a request message
	   NLM_F_DUMP - gives us the root of the link tree and returns all links matching our requested
	   AF, which in our case means all of them (AF_PACKET)
	*/
	request.header.nlmsg_flags = NLM_F_REQUEST | NLM_F_ROOT | NLM_F_MATCH;
	request.header.nlmsg_seq = ++session->seq;
	request.header.nlmsg_pid = session->us.nl_pid;
	request.header.nlmsg_type = type;

	/* AF_PACKET means we want to see everything */
	request.message.rtgen_family = AF_PACKET;

	memset (&session->payload_vector, 0, sizeof (session->payload_vector));
	session->payload_vector.iov_len = request.header.nlmsg_len;
	session->payload_vector.iov_base = &request;

	memset (&session->message_header, 0, sizeof (session->message_header));
	session->message_header.msg_namelen = sizeof (session->them);
	session->message_header.msg_name = &session->them;
	session->message_header.msg_iovlen = 1;
	session->message_header.msg_iov = &session->payload_vector;

	if (sendmsg (session->sock_fd, (const struct msghdr*)&session->message_header, 0) < 0) {
		mono_trace (G_LOG_LEVEL_WARNING, MONO_TRACE_ANDROID_NETLINK, "Failed to send netlink message. %s\n", strerror (errno));
		return -1;
	}

	return 0;
}

static gint
append_ifaddr (struct _monodroid_ifaddrs *addr, struct _monodroid_ifaddrs **ifaddrs_head, struct _monodroid_ifaddrs **last_ifaddr)
{
	g_assert (addr);
	g_assert (ifaddrs_head);
	g_assert (last_ifaddr);

	if (!*ifaddrs_head) {
		*ifaddrs_head = *last_ifaddr = addr;
		if (!*ifaddrs_head)
			return -1;
	} else if (!*last_ifaddr) {
		struct _monodroid_ifaddrs *last = *ifaddrs_head;

		while (last->ifa_next)
			last = last->ifa_next;
		*last_ifaddr = last;
	}

	addr->ifa_next = NULL;
	if (addr == *last_ifaddr)
		return 0;

	g_assert (addr != *last_ifaddr);
	(*last_ifaddr)->ifa_next = addr;
	*last_ifaddr = addr;
	g_assert ((*last_ifaddr)->ifa_next == NULL);

	return 0;
}

static gint
parse_netlink_reply (netlink_session *session, struct _monodroid_ifaddrs **ifaddrs_head, struct _monodroid_ifaddrs **last_ifaddr)
{
	struct msghdr netlink_reply;
	struct iovec reply_vector;
	struct nlmsghdr *current_message;
	struct _monodroid_ifaddrs *addr;
	gint ret = -1;
	guchar *response = NULL;

	g_assert (session);
	g_assert (ifaddrs_head);
	g_assert (last_ifaddr);

	gint buf_size = getpagesize ();
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "receive buffer size == %d", buf_size);

	response = (guchar*)malloc (sizeof (*response) * buf_size);
	if (!response) {
		goto cleanup;
	}

	ssize_t length = 0;
	while (1) {
		memset (response, 0, buf_size);
		memset (&reply_vector, 0, sizeof (reply_vector));
		reply_vector.iov_len = buf_size;
		reply_vector.iov_base = response;

		memset (&netlink_reply, 0, sizeof (netlink_reply));
		netlink_reply.msg_namelen = sizeof (&session->them);
		netlink_reply.msg_name = &session->them;
		netlink_reply.msg_iovlen = 1;
		netlink_reply.msg_iov = &reply_vector;

		length = recvmsg (session->sock_fd, &netlink_reply, 0);
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "  length == %d\n", (gint)length);

		if (length < 0) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "Failed to receive reply from netlink. %s\n", strerror (errno));
			goto cleanup;
		}

#if DEBUG
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "response flags:");
		if (netlink_reply.msg_flags == 0)
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   [NONE]");
		else {
			if (netlink_reply.msg_flags & MSG_EOR)
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   MSG_EOR");
			if (netlink_reply.msg_flags & MSG_TRUNC)
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   MSG_TRUNC");
			if (netlink_reply.msg_flags & MSG_CTRUNC)
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   MSG_CTRUNC");
			if (netlink_reply.msg_flags & MSG_OOB)
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   MSG_OOB");
			if (netlink_reply.msg_flags & MSG_ERRQUEUE)
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   MSG_ERRQUEUE");
		}
#endif

		if (length == 0)
			break;

		for (current_message = (struct nlmsghdr*)response; current_message && NLMSG_OK (current_message, length); current_message = NLMSG_NEXT (current_message, length)) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "next message... (type: %u)\n", current_message->nlmsg_type);
			switch (current_message->nlmsg_type) {
				/* See rtnetlink.h */
				case RTM_NEWLINK:
					mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "  dumping link...\n");
					addr = get_link_info (current_message);
					if (!addr || append_ifaddr (addr, ifaddrs_head, last_ifaddr) < 0) {
						ret = -1;
						goto cleanup;
					}
					mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "  done\n");
					break;

				case RTM_NEWADDR:
					mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "  got an address\n");
					addr = get_link_address (current_message, ifaddrs_head);
					if (!addr || append_ifaddr (addr, ifaddrs_head, last_ifaddr) < 0) {
						ret = -1;
						goto cleanup;
					}
					break;

				case NLMSG_DONE:
					mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "  message done\n");
					ret = 0;
					goto cleanup;
					break;

				default:
					mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "  message type: %u", current_message->nlmsg_type);
					break;
			}
		}
	}

  cleanup:
	if (response)
		free (response);
	return ret;
}

static gint
fill_sa_address (struct sockaddr **sa, struct ifaddrmsg *net_address, gpointer rta_data, gint rta_payload_length)
{
	g_assert (sa);
	g_assert (net_address);
	g_assert (rta_data);

	switch (net_address->ifa_family) {
		case AF_INET: {
			struct sockaddr_in *sa4;
			g_assert (rta_payload_length == 4); /* IPv4 address length */
			sa4 = (struct sockaddr_in*)calloc (1, sizeof (*sa4));
			if (!sa4)
				return -1;

			sa4->sin_family = AF_INET;
			memcpy (&sa4->sin_addr, rta_data, rta_payload_length);
			*sa = (struct sockaddr*)sa4;
			break;
		}

		case AF_INET6: {
			struct sockaddr_in6 *sa6;
			g_assert (rta_payload_length == 16); /* IPv6 address length */
			sa6 = (struct sockaddr_in6*)calloc (1, sizeof (*sa6));
			if (!sa6)
				return -1;

			sa6->sin6_family = AF_INET6;
			memcpy (&sa6->sin6_addr, rta_data, rta_payload_length);
			if (IN6_IS_ADDR_LINKLOCAL (&sa6->sin6_addr) || IN6_IS_ADDR_MC_LINKLOCAL (&sa6->sin6_addr))
				sa6->sin6_scope_id = net_address->ifa_index;
			*sa = (struct sockaddr*)sa6;
			break;
		}

		default: {
			struct sockaddr *sagen;
			g_assert (rta_payload_length <= sizeof (sagen->sa_data));
			*sa = sagen = (struct sockaddr*)calloc (1, sizeof (*sagen));
			if (sagen)
				return -1;

			sagen->sa_family = net_address->ifa_family;
			memcpy (&sagen->sa_data, rta_data, rta_payload_length);
			break;
		}
	}

	return 0;
}

static gint
fill_ll_address (struct sockaddr_ll_extended **sa, struct ifinfomsg *net_interface, gpointer rta_data, gint rta_payload_length)
{
	g_assert (sa);
	g_assert (net_interface);

	/* Always allocate, do not free - caller may reuse the same variable */
	*sa = calloc (1, sizeof (**sa));
	if (!*sa)
		return -1;

	(*sa)->sll_family = AF_PACKET; /* Always for physical links */

	/* The g_assert can only fail for Iniband links, which are quite unlikely to be found
	 * in any mobile devices
	 */
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "rta_payload_length == %d; sizeof sll_addr == %d; hw type == 0x%X\n", rta_payload_length, sizeof ((*sa)->sll_addr), net_interface->ifi_type);
	if (rta_payload_length > sizeof ((*sa)->sll_addr)) {
		mono_trace (G_LOG_LEVEL_MESSAGE, MONO_TRACE_ANDROID_NETLINK, "Address is too long to place in sockaddr_ll (%d > %d)", rta_payload_length, sizeof ((*sa)->sll_addr));
		free (*sa);
		*sa = NULL;
		return -1;
	}

	(*sa)->sll_ifindex = net_interface->ifi_index;
	(*sa)->sll_hatype = net_interface->ifi_type;
	(*sa)->sll_halen = rta_payload_length;
	memcpy ((*sa)->sll_addr, rta_data, rta_payload_length);

	return 0;
}

static struct _monodroid_ifaddrs *
find_interface_by_index (gint index, struct _monodroid_ifaddrs **ifaddrs_head)
{
	struct _monodroid_ifaddrs *cur;
	if (!ifaddrs_head || !*ifaddrs_head)
		return NULL;

	/* Normally expensive, but with the small amount of links in the chain we'll deal with it's not
	 * worth the extra houskeeping and memory overhead
	 */
	cur = *ifaddrs_head;
	while (cur) {
		if (cur->ifa_addr && cur->ifa_addr->sa_family == AF_PACKET && ((struct sockaddr_ll_extended*)cur->ifa_addr)->sll_ifindex == index)
			return cur;
		if (cur == cur->ifa_next)
			break;
		cur = cur->ifa_next;
	}

	return NULL;
}

static gchar *
get_interface_name_by_index (gint index, struct _monodroid_ifaddrs **ifaddrs_head)
{
	struct _monodroid_ifaddrs *iface = find_interface_by_index (index, ifaddrs_head);
	if (!iface || !iface->ifa_name)
		return NULL;

	return iface->ifa_name;
}

static gint
get_interface_flags_by_index (gint index, struct _monodroid_ifaddrs **ifaddrs_head)
{
	struct _monodroid_ifaddrs *iface = find_interface_by_index (index, ifaddrs_head);
	if (!iface)
		return 0;

	return iface->ifa_flags;
}

static gint
calculate_address_netmask (struct _monodroid_ifaddrs *ifa, struct ifaddrmsg *net_address)
{
	if (ifa->ifa_addr && ifa->ifa_addr->sa_family != AF_UNSPEC && ifa->ifa_addr->sa_family != AF_PACKET) {
		uint32_t prefix_length = 0;
		uint32_t data_length = 0;
		guchar *netmask_data = NULL;

		switch (ifa->ifa_addr->sa_family) {
			case AF_INET: {
				struct sockaddr_in *sa = (struct sockaddr_in*)calloc (1, sizeof (struct sockaddr_in));
				if (!sa)
					return -1;

				ifa->ifa_netmask = (struct sockaddr*)sa;
				prefix_length = net_address->ifa_prefixlen;
				if (prefix_length > 32)
					prefix_length = 32;
				data_length = sizeof (sa->sin_addr);
				netmask_data = (guchar*)&sa->sin_addr;
				break;
			}

			case AF_INET6: {
				struct sockaddr_in6 *sa = (struct sockaddr_in6*)calloc (1, sizeof (struct sockaddr_in6));
				if (!sa)
					return -1;

				ifa->ifa_netmask = (struct sockaddr*)sa;
				prefix_length = net_address->ifa_prefixlen;
				if (prefix_length > 128)
					prefix_length = 128;
				data_length = sizeof (sa->sin6_addr);
				netmask_data = (guchar*)&sa->sin6_addr;
				break;
			}
		}

		if (ifa->ifa_netmask && netmask_data) {
			/* Fill the first X bytes with 255 */
			uint32_t prefix_bytes = prefix_length / 8;
			uint32_t postfix_bytes;
			gint i;

			if (prefix_bytes > data_length) {
				errno = EINVAL;
				return -1;
			}
			postfix_bytes = data_length - prefix_bytes;
			memset (netmask_data, 0xFF, prefix_bytes);
			if (postfix_bytes > 0)
				memset (netmask_data + prefix_bytes + 1, 0x00, postfix_bytes);
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   calculating netmask, prefix length is %u bits (%u bytes), data length is %u bytes\n", prefix_length, prefix_bytes, data_length);
			if (prefix_bytes + 2 < data_length)
				/* Set the rest of the mask bits in the byte following the last 0xFF value */
				netmask_data [prefix_bytes + 1] = 0xff << (8 - (prefix_length % 8));
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   netmask is: ");
			for (i = 0; i < data_length; i++) {
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "%s%u", i == 0 ? " " : ".", (guchar)ifa->ifa_netmask->sa_data [i]);
			}
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "\n");
		}
	}

	return 0;
}

static struct _monodroid_ifaddrs *
get_link_address (const struct nlmsghdr *message, struct _monodroid_ifaddrs **ifaddrs_head)
{
	size_t length = 0;
	struct rtattr *attribute;
	struct ifaddrmsg *net_address;
	struct _monodroid_ifaddrs *ifa = NULL;
	struct sockaddr **sa;
	gint payload_size;

	g_assert (message);
	net_address = NLMSG_DATA (message);
	length = IFA_PAYLOAD (message);
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   address data length: %u", length);
	if (length <= 0) {
		goto error;
	}

	ifa = calloc (1, sizeof (*ifa));
	if (!ifa) {
		goto error;
	}

	ifa->ifa_flags = get_interface_flags_by_index (net_address->ifa_index, ifaddrs_head);

	attribute = IFA_RTA (net_address);
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   reading attributes");
	while (RTA_OK (attribute, length)) {
		payload_size = RTA_PAYLOAD (attribute);
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "     attribute payload_size == %u\n", payload_size);
		sa = NULL;

		switch (attribute->rta_type) {
			case IFA_LABEL: {
				gint room_for_trailing_null = 0;

				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "     attribute type: LABEL");
				if (payload_size > MAX_IFA_LABEL_SIZE) {
					payload_size = MAX_IFA_LABEL_SIZE;
					room_for_trailing_null = 1;
				}

				if (payload_size > 0) {
					ifa->ifa_name = (gchar*)malloc (payload_size + room_for_trailing_null);
					if (!ifa->ifa_name) {
						goto error;
					}

					memcpy (ifa->ifa_name, RTA_DATA (attribute), payload_size);
					if (room_for_trailing_null)
						ifa->ifa_name [payload_size] = '\0';
				}
				break;
			}

			case IFA_LOCAL:
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "     attribute type: LOCAL");
				if (ifa->ifa_addr) {
					/* P2P protocol, set the dst/broadcast address union from the original address.
					 * Since ifa_addr is set it means IFA_ADDRESS occured earlier and that address
					 * is indeed the P2P destination one.
					 */
					ifa->_monodroid_ifa_dstaddr = ifa->ifa_addr;
					ifa->ifa_addr = 0;
				}
				sa = &ifa->ifa_addr;
				break;

			case IFA_BROADCAST:
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "     attribute type: BROADCAST");
				if (ifa->_monodroid_ifa_dstaddr) {
					/* IFA_LOCAL happened earlier, undo its effect here */
					free (ifa->_monodroid_ifa_dstaddr);
					ifa->_monodroid_ifa_dstaddr = NULL;
				}
				sa = &ifa->_monodroid_ifa_broadaddr;
				break;

			case IFA_ADDRESS:
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "     attribute type: ADDRESS");
				if (ifa->ifa_addr) {
					/* Apparently IFA_LOCAL occured earlier and we have a P2P connection
					 * here. IFA_LOCAL carries the destination address, move it there
					 */
					ifa->_monodroid_ifa_dstaddr = ifa->ifa_addr;
					ifa->ifa_addr = NULL;
				}
				sa = &ifa->ifa_addr;
				break;

			case IFA_UNSPEC:
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "     attribute type: UNSPEC");
				break;

			case IFA_ANYCAST:
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "     attribute type: ANYCAST");
				break;

			case IFA_CACHEINFO:
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "     attribute type: CACHEINFO");
				break;

			case IFA_MULTICAST:
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "     attribute type: MULTICAST");
				break;

			default:
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "     attribute type: %u", attribute->rta_type);
				break;
		}

		if (sa) {
			if (fill_sa_address (sa, net_address, RTA_DATA (attribute), RTA_PAYLOAD (attribute)) < 0) {
				goto error;
			}
		}

		attribute = RTA_NEXT (attribute, length);
	}

	/* glibc stores the associated interface name in the address if IFA_LABEL never occured */
	if (!ifa->ifa_name) {
		gchar *name = get_interface_name_by_index (net_address->ifa_index, ifaddrs_head);
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   address has no name/label, getting one from interface\n");
		ifa->ifa_name = name ? strdup (name) : NULL;
	}
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   address label: %s\n", ifa->ifa_name);

	if (calculate_address_netmask (ifa, net_address) < 0) {
		goto error;
	}

	return ifa;

  error:
	{
		/* errno may be modified by free, or any other call inside the free_single_xamarin_ifaddrs
		 * function. We don't care about errors in there since it is more important to know how we
		 * failed to obtain the link address and not that we went OOM. Save and restore the value
		 * after the resources are freed.
		 */
		gint errno_save = errno;
		free_single_xamarin_ifaddrs (&ifa);
		errno = errno_save;
		return NULL;
	}

}

static struct _monodroid_ifaddrs *
get_link_info (const struct nlmsghdr *message)
{
	ssize_t length;
	struct rtattr *attribute;
	struct ifinfomsg *net_interface;
	struct _monodroid_ifaddrs *ifa = NULL;
	struct sockaddr_ll_extended *sa = NULL;

	g_assert (message);
	net_interface = NLMSG_DATA (message);
	length = message->nlmsg_len - NLMSG_LENGTH (sizeof (*net_interface));
	if (length <= 0) {
		goto error;
	}

	ifa = calloc (1, sizeof (*ifa));
	if (!ifa) {
		goto error;
	}

	ifa->ifa_flags = net_interface->ifi_flags;
	attribute = IFLA_RTA (net_interface);
	while (RTA_OK (attribute, length)) {
		switch (attribute->rta_type) {
			case IFLA_IFNAME:
				ifa->ifa_name = strdup (RTA_DATA (attribute));
				if (!ifa->ifa_name) {
					goto error;
				}
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   interface name (payload length: %d; string length: %d)\n", RTA_PAYLOAD (attribute), strlen (ifa->ifa_name));
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "     %s\n", ifa->ifa_name);
				break;

			case IFLA_BROADCAST:
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   interface broadcast (%d bytes)\n", RTA_PAYLOAD (attribute));
				if (fill_ll_address (&sa, net_interface, RTA_DATA (attribute), RTA_PAYLOAD (attribute)) < 0) {
					goto error;
				}
				ifa->_monodroid_ifa_broadaddr = (struct sockaddr*)sa;
				break;

			case IFLA_ADDRESS:
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "   interface address (%d bytes)\n", RTA_PAYLOAD (attribute));
				if (fill_ll_address (&sa, net_interface, RTA_DATA (attribute), RTA_PAYLOAD (attribute)) < 0) {
					goto error;
				}
				ifa->ifa_addr = (struct sockaddr*)sa;
				break;

			default:
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "     rta_type: ");
#if DEBUG
				print_ifla_name (attribute->rta_type);
#endif
				break;
		}

		attribute = RTA_NEXT (attribute, length);
	}
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ANDROID_NETLINK, "link flags: 0x%X", ifa->ifa_flags);
	return ifa;

  error:
	if (sa)
		free (sa);
	free_single_xamarin_ifaddrs (&ifa);

	return NULL;
}

#if DEBUG

static void
print_ifla_name (gint id)
{
	static struct {
		gint   value;
		gchar *name;
	} iflas[] = {
#define ENUM_VALUE_ENTRY(enumvalue) { enumvalue, #enumvalue }
		ENUM_VALUE_ENTRY (IFLA_UNSPEC),
		ENUM_VALUE_ENTRY (IFLA_ADDRESS),
		ENUM_VALUE_ENTRY (IFLA_BROADCAST),
		ENUM_VALUE_ENTRY (IFLA_IFNAME),
		ENUM_VALUE_ENTRY (IFLA_MTU),
		ENUM_VALUE_ENTRY (IFLA_LINK),
		ENUM_VALUE_ENTRY (IFLA_QDISC),
		ENUM_VALUE_ENTRY (IFLA_STATS),
		ENUM_VALUE_ENTRY (IFLA_COST),
		ENUM_VALUE_ENTRY (IFLA_PRIORITY),
		ENUM_VALUE_ENTRY (IFLA_MASTER),
		ENUM_VALUE_ENTRY (IFLA_WIRELESS),
		ENUM_VALUE_ENTRY (IFLA_PROTINFO),
		ENUM_VALUE_ENTRY (IFLA_TXQLEN),
		ENUM_VALUE_ENTRY (IFLA_MAP),
		ENUM_VALUE_ENTRY (IFLA_WEIGHT),
		ENUM_VALUE_ENTRY (IFLA_OPERSTATE),
		ENUM_VALUE_ENTRY (IFLA_LINKMODE),
		ENUM_VALUE_ENTRY (IFLA_LINKINFO),
		ENUM_VALUE_ENTRY (IFLA_NET_NS_PID),
		ENUM_VALUE_ENTRY (IFLA_IFALIAS),
		ENUM_VALUE_ENTRY (IFLA_NUM_VF),
		ENUM_VALUE_ENTRY (IFLA_VFINFO_LIST),
		ENUM_VALUE_ENTRY (IFLA_STATS64),
		ENUM_VALUE_ENTRY (IFLA_VF_PORTS),
		ENUM_VALUE_ENTRY (IFLA_PORT_SELF),
		ENUM_VALUE_ENTRY (IFLA_AF_SPEC),
		ENUM_VALUE_ENTRY (IFLA_GROUP),
		ENUM_VALUE_ENTRY (IFLA_NET_NS_FD),
		ENUM_VALUE_ENTRY (IFLA_EXT_MASK),
		ENUM_VALUE_ENTRY (IFLA_PROMISCUITY),
		ENUM_VALUE_ENTRY (IFLA_NUM_TX_QUEUES),
		ENUM_VALUE_ENTRY (IFLA_NUM_RX_QUEUES),
		ENUM_VALUE_ENTRY (IFLA_CARRIER),
		ENUM_VALUE_ENTRY (IFLA_PHYS_PORT_ID),
		ENUM_VALUE_ENTRY (IFLA_CARRIER_CHANGES),
		ENUM_VALUE_ENTRY (IFLA_PHYS_SWITCH_ID),
		ENUM_VALUE_ENTRY (IFLA_LINK_NETNSID),
		ENUM_VALUE_ENTRY (IFLA_PHYS_PORT_NAME),
		ENUM_VALUE_ENTRY (IFLA_PROTO_DOWN),
		ENUM_VALUE_ENTRY (IFLA_GSO_MAX_SEGS),
		ENUM_VALUE_ENTRY (IFLA_GSO_MAX_SIZE),
		ENUM_VALUE_ENTRY (IFLA_PAD),
		ENUM_VALUE_ENTRY (IFLA_XDP),
		{ -1, 0 }
#undef ENUM_VALUE_ENTRY
	};

	for (gint i = 0; iflas [i].value != -1 && iflas [i].name != 0; ++i) {
		if (iflas [i].value == id) {
			mono_trace (G_LOG_LEVEL_MESSAGE, MONO_TRACE_ANDROID_NETLINK, "ifla->name: %s (%d)\n", iflas [i].name, iflas [i].value);
			return;
		}
	}

	mono_trace (G_LOG_LEVEL_MESSAGE, MONO_TRACE_ANDROID_NETLINK, "Unknown ifla->name: unknown id %d\n", id);
}

static void
print_address_list (gchar *title, struct _monodroid_ifaddrs *list)
{
	struct _monodroid_ifaddrs *cur;
	gchar *msg, *tmp;

	if (!list) {
		mono_trace (G_LOG_LEVEL_MESSAGE, MONO_TRACE_ANDROID_NETLINK, "monodroid-net", "No list to print in %s", __FUNCTION__);
		return;
	}

	cur = list;
	msg = NULL;
	while (cur) {
		tmp = NULL;
		asprintf (&tmp, "%s%s%p (%s; %p)", msg ? msg : "", msg ? " -> " : "", cur, cur->ifa_name, cur->ifa_name);
		if (msg)
			free (msg);
		msg = tmp;
		cur = cur->ifa_next;
	}

	mono_trace (G_LOG_LEVEL_MESSAGE, MONO_TRACE_ANDROID_NETLINK, "%s: %s", title, msg ? msg : "[no addresses]");
	free (msg);
}

#endif // DEBUG

#else // LINUX

gint
ves_icall_System_Net_NetworkInformation_NetworkInterfaceFactory_UnixNetworkInterfaceAPI_getifaddrs (struct _monodroid_ifaddrs **ifap)
{
	*ifap = NULL;
	return 0;
}

void
ves_icall_System_Net_NetworkInformation_NetworkInterfaceFactory_UnixNetworkInterfaceAPI_freeifaddrs (struct _monodroid_ifaddrs *ifa)
{
}

#endif // LINUX
