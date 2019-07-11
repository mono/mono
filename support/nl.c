/*
 * Network availability and change notifications for linux.
 *
 * Authors:
 *   Gonzalo Paniagua Javier (gonzalo@novell.com)
 *
 * Copyright (c) Novell, Inc. 2011
 */

#ifdef HAVE_CONFIG_H
#include <config.h>
#endif

#include "nl.h"

#if defined(HAVE_LINUX_NETLINK_H) && defined(HAVE_LINUX_RTNETLINK_H)

#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <linux/netlink.h>
#include <linux/rtnetlink.h>

#undef NL_DEBUG
#define NL_DEBUG_STMT(a) do { } while (0)
#define NL_DEBUG_PRINT(...)

/*
#define NL_DEBUG 1
#define NL_DEBUG_STMT(a) do { a } while (0)
#define NL_DEBUG_PRINT(...) g_message(__VA_ARGS__)
*/


#ifdef AF_INET6
#define ADDR_BYTE_LENGTH 16
#define ADDR_STR_LENGTH INET6_ADDRSTRLEN
#else
#define ADDR_LENGTH	 4
#define ADDR_STR_LENGTH INET_ADDRSTRLEN
#endif

enum event_type {
	EVT_NONE = 0,
#define EVT_NONE 0
	EVT_AVAILABILITY = 1 << 0,
#define EVT_AVAILABILITY (1 << 0)
	EVT_ADDRESS = 1 << 1,
#define EVT_ADDRESS (1 << 1)
	EVT_ALL = EVT_AVAILABILITY | EVT_ADDRESS
#define EVT_ALL (EVT_AVAILABILITY | EVT_ADDRESS)
};

#ifdef NL_DEBUG
typedef struct {
	int value;
	const char *name;
} value2name_t;

#define INIT(x) { x, #x }
#define FIND_NAME(a, b) value_to_name (a, b)

#define FIND_RT_TYPE_NAME(b) FIND_NAME (rt_types, b)
static value2name_t rt_types [] = {
	INIT (RTM_NEWROUTE),
	INIT (RTM_DELROUTE),
	INIT (RTM_GETROUTE),
	INIT (RTM_NEWADDR),
	INIT (RTM_DELADDR),
	INIT (RTM_GETADDR),
	INIT (RTM_NEWLINK),
	INIT (RTM_GETLINK),
	INIT (RTM_DELLINK),
	INIT (RTM_NEWNEIGH),
	INIT (RTM_GETNEIGH),
	INIT (RTM_DELNEIGH),
	{0, NULL}
};

#define FIND_RTM_TYPE_NAME(b) FIND_NAME (rtm_types, b)
static value2name_t rtm_types [] = {
	INIT (RTN_UNSPEC),
	INIT (RTN_UNICAST),
	INIT (RTN_LOCAL),
	INIT (RTN_BROADCAST),
	INIT (RTN_ANYCAST),
	INIT (RTN_MULTICAST),
	INIT (RTN_BLACKHOLE),
	INIT (RTN_UNREACHABLE),
	INIT (RTN_PROHIBIT),
	INIT (RTN_THROW),
	INIT (RTN_NAT),
	INIT (RTN_XRESOLVE),
	{0, NULL}
};

#define FIND_RTM_PROTO_NAME(b) FIND_NAME (rtm_protocols, b)
static value2name_t rtm_protocols[] = {
	INIT (RTPROT_UNSPEC),
	INIT (RTPROT_REDIRECT),
	INIT (RTPROT_KERNEL),
	INIT (RTPROT_BOOT),
	INIT (RTPROT_STATIC),
	{0, NULL}
};

#define FIND_RTM_SCOPE_NAME(b) FIND_NAME (rtm_scopes, b)
static value2name_t rtm_scopes [] = {
	INIT (RT_SCOPE_UNIVERSE),
	INIT (RT_SCOPE_SITE),
	INIT (RT_SCOPE_LINK),
	INIT (RT_SCOPE_HOST),
	INIT (RT_SCOPE_NOWHERE),
	{0, NULL}
};

#define FIND_RTM_ATTRS_NAME(b) FIND_NAME (rtm_attrs, b)
static value2name_t rtm_attrs [] = {
	INIT (RTA_UNSPEC),
	INIT (RTA_DST),
	INIT (RTA_SRC),
	INIT (RTA_IIF),
	INIT (RTA_OIF),
	INIT (RTA_GATEWAY),
	INIT (RTA_PRIORITY),
	INIT (RTA_PREFSRC),
	INIT (RTA_METRICS),
	INIT (RTA_MULTIPATH),
	INIT (RTA_PROTOINFO),
	INIT (RTA_FLOW),
	INIT (RTA_CACHEINFO),
	INIT (RTA_SESSION),
	INIT (RTA_MP_ALGO),
	INIT (RTA_TABLE),
	{0, NULL}
};

#define FIND_RT_TABLE_NAME(b) FIND_NAME (rtm_tables, b)
static value2name_t rtm_tables [] = {
        INIT (RT_TABLE_UNSPEC),
	INIT (RT_TABLE_COMPAT),
	INIT (RT_TABLE_DEFAULT),
	INIT (RT_TABLE_MAIN),
	INIT (RT_TABLE_LOCAL),
	{0,0}
};

static const char *
value_to_name (value2name_t *tbl, int value)
{
	static char auto_name [16];

	while (tbl->name) {
		if (tbl->value == value)
			return tbl->name;
		tbl++;
	}
	snprintf (auto_name, sizeof (auto_name), "#%d", value);
	return auto_name;
}
#endif /* NL_DEBUG */

gpointer
CreateNLSocket (void)
{
	int sock;
	struct sockaddr_nl sa;
	int ret;
	
	sock = socket (AF_NETLINK, SOCK_RAW, NETLINK_ROUTE);

	ret = fcntl (sock, F_GETFL, 0);
	if (ret != -1) {
		ret |= O_NONBLOCK;
		ret = fcntl (sock, F_SETFL, ret);
		if (ret < 0)
			return GINT_TO_POINTER (-1);
	}

	memset (&sa, 0, sizeof (sa));
	if (sock < 0)
		return GINT_TO_POINTER (-1);
	sa.nl_family = AF_NETLINK;
	sa.nl_pid = getpid ();
	sa.nl_groups = RTMGRP_IPV4_ROUTE | RTMGRP_IPV6_ROUTE | RTMGRP_NOTIFY;
	/* RTNLGRP_IPV4_IFADDR | RTNLGRP_IPV6_IFADDR
	 * RTMGRP_LINK */

	if (bind (sock, (struct sockaddr *) &sa, sizeof (sa)) < 0)
		return GINT_TO_POINTER (-1);

	return GINT_TO_POINTER (sock);
}

int
ReadEvents (gpointer sock, gpointer buffer, gint32 count, gint32 size)
{
	struct nlmsghdr *nlp;
	struct rtmsg *rtp;
	int rtl;
	struct rtattr *rtap;
	int result;
	int s;

	NL_DEBUG_PRINT ("ENTER ReadEvents()");
	result = EVT_NONE;
	s = GPOINTER_TO_INT (sock);
	/* This socket is not found by IO layer, so we do everything here */
	if (count == 0) {
		while ((count = recv (s, buffer, size, 0)) == -1 && errno == EINTR);
		if (count <= 0) {
			NL_DEBUG_PRINT ("EXIT ReadEvents()");
			return result;
		}
	}
	for (nlp = (struct nlmsghdr *) buffer; NLMSG_OK (nlp, count); nlp = NLMSG_NEXT (nlp, count)) {
		int family;
		int addr_length;
		int msg_type;
		int table;
#ifdef NL_DEBUG
		int protocol;
		int scope;
#endif
		int rtm_type;
		gboolean have_dst;
		gboolean have_src;
		gboolean have_pref_src;
		gboolean have_gw;
		char dst [ADDR_BYTE_LENGTH];
		char src [ADDR_BYTE_LENGTH];
		char pref_src [ADDR_BYTE_LENGTH];
		char gw [ADDR_BYTE_LENGTH];

		msg_type = nlp->nlmsg_type;
		NL_DEBUG_PRINT ("TYPE: %d %s", msg_type, FIND_RT_TYPE_NAME (msg_type));
		if (msg_type != RTM_NEWROUTE && msg_type != RTM_DELROUTE)
			continue;

		rtp = (struct rtmsg *) NLMSG_DATA (nlp);
		family = rtp->rtm_family;
#ifdef AF_INET6
		if (family != AF_INET && family != AF_INET6) {
#else
		if (family != AF_INET) {
#endif
			continue;
		}

		addr_length = (family == AF_INET) ? 4 : 16;
		table = rtp->rtm_table;
#ifdef NL_DEBUG
		protocol = rtp->rtm_protocol;
		scope = rtp->rtm_scope;
#endif
		rtm_type = rtp->rtm_type;
		NL_DEBUG_PRINT ("\tRTMSG table: %d %s", table, FIND_RT_TABLE_NAME (table));
		if (table != RT_TABLE_MAIN && table != RT_TABLE_LOCAL)
			continue;

		NL_DEBUG_PRINT ("\tRTMSG protocol: %d %s", protocol, FIND_RTM_PROTO_NAME (protocol));
		NL_DEBUG_PRINT ("\tRTMSG scope: %d %s", scope, FIND_RTM_SCOPE_NAME (scope));
		NL_DEBUG_PRINT ("\tRTMSG type: %d %s", rtm_type, FIND_RTM_TYPE_NAME (rtm_type));

		rtap = (struct rtattr *) RTM_RTA (rtp);
		rtl = RTM_PAYLOAD (nlp);
		// loop & get every attribute
		//
		// 
		// NEW_ROUTE
		// 	table = RT_TABLE_LOCAL, Scope = HOST + pref.src == src  + type=LOCAL -> new if addr
		// 	RT_TABLE_MAIN, Scope = Universe, unicast, gateway exists -> NEW default route
		// DEL_ROUTE
		// 	table = RT_TABLE_LOCAL, Scope = HOST, perfsrc = dst  + type=LOCAL -> if addr deleted
		// 	RT_TABLE_MAIN - DELROUTE + unicast -> event (gw down?)
		have_dst = have_src = have_pref_src = have_gw = FALSE;
		for(; RTA_OK (rtap, rtl); rtap = RTA_NEXT(rtap, rtl)) {
			char *data;
#ifdef NL_DEBUG
			char ip [ADDR_STR_LENGTH];
#endif

			NL_DEBUG_PRINT ("\tAttribute: %d %d (%s)", rtap->rta_len, rtap->rta_type, FIND_RTM_ATTRS_NAME (rtap->rta_type));
			data = RTA_DATA (rtap);
			switch (rtap->rta_type) {
			case RTA_DST:
				have_dst = TRUE;
				memcpy (dst, data, addr_length);
				NL_DEBUG_STMT (
					*ip = 0;
					inet_ntop (family, RTA_DATA (rtap), ip, sizeof (ip));
					NL_DEBUG_PRINT ("\t\tDst: %s", ip);
				);
				break;
			case RTA_PREFSRC:
				have_pref_src = TRUE;
				memcpy (pref_src, data, addr_length);
				NL_DEBUG_STMT (
					*ip = 0;
					inet_ntop (family, RTA_DATA (rtap), ip, sizeof (ip));
					NL_DEBUG_PRINT ("\t\tPref. Src.: %s", ip);
				);
				break;
			case RTA_SRC:
				have_src = TRUE;
				memcpy (src, data, addr_length);
				NL_DEBUG_STMT (
					*ip = 0;
					inet_ntop (family, RTA_DATA (rtap), ip, sizeof (ip));
					NL_DEBUG_PRINT ("\tSrc: %s", ip);
				);
				break;
			case RTA_GATEWAY:
				have_gw = TRUE;
				memcpy (gw, data, addr_length);
				NL_DEBUG_STMT (
					*ip = 0;
					inet_ntop (family, RTA_DATA (rtap), ip, sizeof (ip));
					NL_DEBUG_PRINT ("\t\tGateway: %s", ip);
				);
				break;
			default:
				break;
			}
		}
		if (msg_type == RTM_NEWROUTE) {
			if (table == RT_TABLE_MAIN) {
				NL_DEBUG_PRINT ("NEWROUTE: Availability changed");
				result |= EVT_AVAILABILITY;
			} else if (table == RT_TABLE_LOCAL) {
				NL_DEBUG_PRINT ("NEWROUTE: new IP");
				if (have_dst && have_pref_src && memcmp (dst, pref_src, addr_length) == 0)
					result |= EVT_ADDRESS;
			}
		} else if (msg_type == RTM_DELROUTE) {
			if (table == RT_TABLE_MAIN) {
				if (rtm_type == RTN_UNICAST && (have_dst || have_pref_src)) {
					result |= EVT_AVAILABILITY;
					NL_DEBUG_PRINT ("DELROUTE: Availability changed");
				}
			} else if (table == RT_TABLE_LOCAL) {
				if (have_dst && have_pref_src && memcmp (dst, pref_src, addr_length) == 0) {
					result |= EVT_ADDRESS;
					NL_DEBUG_PRINT ("DELROUTE: deleted IP");
				}
			}
		}
		while ((count = recv (s, buffer, size, 0)) == -1 && errno == EINTR);
		if (count <= 0) {
			NL_DEBUG_PRINT ("EXIT ReadEvents() -> %d", result);
			return result;
		}
		nlp = (struct nlmsghdr *) buffer;
	}
	NL_DEBUG_PRINT ("EXIT ReadEvents() -> %d", result);
	return result;
}

gpointer
CloseNLSocket (gpointer sock)
{
	return GINT_TO_POINTER (close (GPOINTER_TO_INT (sock)));
}
#else
int
ReadEvents (gpointer sock, gpointer buffer, gint32 count, gint32 size)
{
	return 0;
}

gpointer
CreateNLSocket (void)
{
	return GINT_TO_POINTER (-1);
}

gpointer
CloseNLSocket (gpointer sock)
{
	return GINT_TO_POINTER (-1);
}
#endif /* linux/netlink.h + linux/rtnetlink.h */

