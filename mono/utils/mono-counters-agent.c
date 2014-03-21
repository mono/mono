/*
 * Copyright 2014 Xamarin Inc
 */

#include <config.h>

#ifndef HOST_WIN32

#include <stdlib.h>
#include <stdio.h>
#include <errno.h>
#include <pthread.h>
#include <glib.h>
#include <unistd.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#include <netinet/tcp.h>
#include <sys/socket.h>
#include <sys/time.h>
#include <sys/types.h>
#include "mono-counters.h"
#include "mono-counters-internals.h"
#include "mono-counters-agent.h"
#include "mono-time.h"
#include "mono-threads.h"

#define MONO_COUNTERS_AGENT_PROTOCOL_VERSION 0x0001

enum {
	AGENT_STATUS_OK,
	AGENT_STATUS_NOK,
	AGENT_STATUS_NOTFOUND,
	AGENT_STATUS_EXISTING,
};

enum {
	SRV_HELLO,
	SRV_LIST_COUNTERS,
	SRV_ADD_COUNTERS,
	SRV_REMOTE_COUNTER,
	SVR_SAMPLES,
};
	
/**
 * Protocol :
 * | Headers | Values | Values | ... [Infinity]
 *
 * Headers :
 * | Count (2) | Counter | Counter | ... [Count]
 *
 * Counter :
 * | Category (4) | Name Length (8) | Name (Name Length) | Type (4) | Unit (4) | Variance (4) | Index (2) |
 *
 * Values :
 * | Timestamp (8) | Value | Value | ... | -1 (2) |
 *
 * Value :
 * | Index (2) | Size (2) | Value (Size) |
 */

typedef struct MonoCounterAgent {
	MonoCounter *counter;
	// MonoCounterAgent specific data :
	void *value;
	short index;
} MonoCounterAgent;

static const char *inspector_ip;
static int inspector_port;
static gint64 interval;

static GSList* counters;
static gboolean agent_running;
static short current_index = 0;

static void
free_agent_memory (void)
{
	GSList *counter = counters;
	while (counter) {
		MonoCounterAgent *ct = counter->data;

		mono_counters_free_counter (ct->counter);
		g_free (ct->value);
		g_free (ct);

		counter = counter->next;
	}
	g_slist_free (counters);
	g_free ((char*)inspector_ip);
	counters = NULL;
	inspector_ip = NULL;
	current_index = 0;
}

static MonoCounterAgent*
add_counter (MonoCounter *counter)
{
	MonoCounterAgent *agent_counter;

	agent_counter = g_new0 (MonoCounterAgent, 1);
	agent_counter->counter = counter;
	agent_counter->index = current_index++;
	counters = g_slist_append (counters, agent_counter);
	return agent_counter;
}

static void
parse_counters_names (const char *counters_names)
{
	gchar **names, **ptr;

	if (!counters_names)
		return;

	names = g_strsplit (counters_names, ";", -1);

	for (ptr = names; *ptr; ++ptr) {
		MonoCounterCategory category;
		MonoCounter *counter;
		gchar **split = g_strsplit (*ptr, "/", 2);

		if (!split [0] || !split [1]) {
			g_warning ("Bad counter format '%s' use Category/Name", *ptr);
			goto end;
		}
	
		category = mono_counters_category_name_to_id (split [0]);
		if (category < 0) {
			g_warning ("Category %s not found", split [0]);
			goto end;
		}

		counter  = mono_counters_get (category, split [1]);
		if (!counter) {
			g_warning ("Counter %s not found in category %s", split [1], split [0]);
			goto end;
		}
		add_counter (counter);

end:
		g_strfreev(split);
	}
	g_strfreev (names);
}

#define DEFAULT_ADDRESS "127.0.0.1"
#define DEFAULT_PORT 8888

static void
parse_address (const char *address)
{
	inspector_ip = NULL;
	inspector_port = DEFAULT_PORT;

	if (address) {
		gchar **split = g_strsplit (address, ":", 2);

		if (split [0])
			inspector_ip = g_strdup (split [0]);
		if (split [1])
			inspector_port = strtol (split [1], NULL, 10);

		g_strfreev(split);
	}
	if (!inspector_ip)
		inspector_ip = g_strdup (DEFAULT_ADDRESS);
}

static gboolean
write_buffer_to_socket (int fd, const char* buffer, size_t size)
{
	size_t sent = 0;
	
	while (sent < size) {
		ssize_t res = send (fd, buffer + sent, size - sent, 0);
		if (res <= 0)
			return FALSE;
		sent += res;
	}

	return TRUE;
}

static gboolean
read_socket_to_buffer (int fd, char* buffer, size_t size)
{
	size_t recvd = 0;

	while (recvd < size) {
		ssize_t res = recv (fd, buffer + recvd, size - recvd, 0);
		if (res <= 0)
			return FALSE;
		recvd += res;
	}

	return TRUE;
}

static gboolean
write_counter_agent_header (int socketfd, MonoCounterAgent *counter_agent)
{
	int len = strlen (counter_agent->counter->name);

	if (!write_buffer_to_socket(socketfd, (char*)&counter_agent->counter->category, 4) ||
		!write_buffer_to_socket (socketfd, (char*)&len, 4) ||
		!write_buffer_to_socket (socketfd, (char*) counter_agent->counter->name, len) ||
		!write_buffer_to_socket (socketfd, (char*)&counter_agent->counter->type, 4) ||
		!write_buffer_to_socket (socketfd, (char*)&counter_agent->counter->unit, 4) ||
		!write_buffer_to_socket (socketfd, (char*)&counter_agent->counter->variance, 4) ||
		!write_buffer_to_socket (socketfd, (char*)&counter_agent->index, 2))
		return FALSE;

	return TRUE;
}

static gboolean
write_counter_agent_headers (int socketfd)
{
	short count = g_slist_length (counters);
	GSList *item;

	if (!write_buffer_to_socket (socketfd, (char*)&count, 2))
		return FALSE;

	for (item = counters; item; item = item->next) {
		if (!write_counter_agent_header (socketfd, item->data))
			return FALSE;
	}

	return TRUE;
}

static gboolean
do_hello (int socketfd)
{
	char cmd = SRV_HELLO;
	short version = MONO_COUNTERS_AGENT_PROTOCOL_VERSION;

	if (!write_buffer_to_socket (socketfd, (char*)&cmd, 1))
		return FALSE;

	if (!write_buffer_to_socket (socketfd, (char*)&version, 2))
		return FALSE;

	if (!write_counter_agent_headers (socketfd))
		return FALSE;

	return TRUE;
}

static gboolean
do_list_counters (int socketfd)
{
	char cmd = SRV_LIST_COUNTERS;

	if (!write_buffer_to_socket (socketfd, (char*)&cmd, 1))
		return FALSE;

	if (!write_counter_agent_headers (socketfd))
		return FALSE;

	return TRUE;
}

static char*
read_string (int socketfd)
{
	int len;
	char *string;

	if (!read_socket_to_buffer (socketfd, (char*)&len, 4))
		return NULL;

	string = g_malloc(len + 1);
	if (!read_socket_to_buffer (socketfd, string, len))
		return FALSE;
	string [len] = '\0';
	return string;
}

static gboolean
do_add_counter (int socketfd)
{
	char cmd = SRV_ADD_COUNTERS;
	char *category = NULL;
	char *name = NULL;
	char status;
	MonoCounterCategory category_id;
	MonoCounter *counter;
	MonoCounterAgent *added_counter;
	GSList *item;

	if (!(category = read_string (socketfd)))
		goto fail;

	if (!(name = read_string (socketfd)))
		goto fail;

	category_id = mono_counters_category_name_to_id (category);
	if (category < 0) {
		status = AGENT_STATUS_NOTFOUND;
		goto done;
	}
	
	counter = mono_counters_get (category_id, name);
	if (!counter) {
		status = AGENT_STATUS_NOTFOUND;
		goto done;
	}
	
	for (item = counters; item; item = item->next) {
		MonoCounterAgent *counter_agent = item->data;
		if (counter_agent->counter->category == category_id && strcmp (counter_agent->counter->name, name) == 0) {
			status = AGENT_STATUS_EXISTING;
			goto done;
		}
	}

	added_counter = add_counter (counter);
	status = AGENT_STATUS_OK;

done:
	g_free (category);
	g_free (name);

	if (!write_buffer_to_socket (socketfd, (char*)&cmd, 1))
		return FALSE;

	if (!write_buffer_to_socket (socketfd, &status, 1))
		return FALSE;

	if (status == AGENT_STATUS_OK) {
		if (!write_counter_agent_header (socketfd, added_counter))
			return FALSE;
	}

	return TRUE;

fail:
	g_free(category);
	g_free(name);
	return FALSE;
}

static gboolean
do_remove_counter (int socketfd)
{
	char cmd = SRV_REMOTE_COUNTER;
	short index;
	char status;
	GSList *item;
	MonoCounterAgent *counter_agent = NULL, *counter_agent_tmp;

	if (!read_socket_to_buffer (socketfd, (char*)&index, 2))
		return FALSE;

	if (index < 0) {
		g_slist_free (counters);
		status = AGENT_STATUS_OK;
	} else {
		for (item = counters; item; item = item->next) {
			counter_agent_tmp = item->data;
			if (counter_agent_tmp->index == index) {
				counter_agent = counter_agent_tmp;
				break;
			}
		}

		if (counter_agent) {
			g_slist_remove (counters, counter_agent);
			g_free (counter_agent);

			status = AGENT_STATUS_OK;
		} else {
			status = AGENT_STATUS_NOTFOUND;
		}
	}

	if (!write_buffer_to_socket (socketfd, (char*)&cmd, 1))
		return FALSE;

	return write_buffer_to_socket (socketfd, &status, 1);
}

static gboolean
do_sampling (int socketfd)
{
	short end;
	char cmd = SVR_SAMPLES;
 	char buffer [8];
	GSList *item;
	MonoCounterAgent *counter_agent;

	if (!write_buffer_to_socket (socketfd, (char*)&cmd, 1))
		return FALSE;

	gint64 timestamp = mono_100ns_datetime ();
	if (!write_buffer_to_socket (socketfd, (char*)&timestamp, 8))
		return FALSE;

	for (item = counters; item; item = item->next) {
		counter_agent = item->data;

		if (!counter_agent)
			continue;

		int size = mono_counters_size (counter_agent->counter);
		if (size < 0)
			return FALSE;

		if (mono_counters_sample (counter_agent->counter, buffer, size) != size) {
			g_warning ("Sampling failed, bad size");
			return FALSE;
		}

		if (!counter_agent->value)
			counter_agent->value = g_malloc (size);
		else if (memcmp (counter_agent->value, buffer, size) == 0)
			continue;

		memcpy (counter_agent->value, buffer, size);

		if (!write_buffer_to_socket (socketfd, (char*)&counter_agent->index, 2) ||
			!write_buffer_to_socket (socketfd, (char*)&size, 2) ||
			!write_buffer_to_socket (socketfd, buffer, size))
			return FALSE;
	}

	end = -1;
	if (!write_buffer_to_socket (socketfd, (char*)&end, 2))
		return FALSE;

	return TRUE;
}

static void*
mono_counters_agent_sampling_thread (void* ptr)
{
	GSList *item;
	int ret, socketfd = -1;
	short count = 0;
	struct sockaddr_in inspector_addr;
	struct timeval timeout;
	fd_set socketfd_set;
	gint64 last_sampling = 0, now;
	char cmd;
	
	if ((socketfd = socket (AF_INET, SOCK_STREAM, 0)) < 0) {
		g_warning ("mono-counters-agent: error with socket : %s", strerror (errno));
		return NULL;
	}
	
	memset (&inspector_addr, 0, sizeof (inspector_addr));
	
	inspector_addr.sin_family = AF_INET;
	inspector_addr.sin_port = htons (inspector_port);
	
	if (!inet_pton (AF_INET, inspector_ip, &inspector_addr.sin_addr)) {
		g_warning ("mono-counters-agent: error with inet_pton : %s", strerror (errno));
		goto cleanup;
	}
	
	if (connect(socketfd, (struct sockaddr*)&inspector_addr, sizeof(inspector_addr)) < 0) {
		g_warning ("mono-counters-agent: error with connect : %s", strerror(errno));
		goto cleanup;
	}

	for (item = counters; item; item = item->next)
		++count;

	if (!write_buffer_to_socket (socketfd, (char*)&count, 2))
		goto cleanup;

	for (item = counters; item; item = item->next) {
		MonoCounterAgent* counter = item->data;

		int len = strlen (counter->counter->name);

		if (!write_buffer_to_socket(socketfd, (char*)&counter->counter->category, 4) ||
			!write_buffer_to_socket (socketfd, (char*)&len, 4) ||
			!write_buffer_to_socket (socketfd, (char*)counter->counter->name, len) ||
			!write_buffer_to_socket (socketfd, (char*)&counter->counter->type, 4) ||
			!write_buffer_to_socket (socketfd, (char*)&counter->counter->unit, 4) ||
			!write_buffer_to_socket (socketfd, (char*)&counter->counter->variance, 4) ||
			!write_buffer_to_socket (socketfd, (char*)&counter->index, 2))
			goto cleanup;
	}

	if (!do_hello(socketfd))
	 		goto cleanup;

 	for (;;) {
		now = mono_100ns_datetime ();
		if (last_sampling == 0 || now > last_sampling + interval * 10000) {
			timeout.tv_sec = 0;
			timeout.tv_usec = 0;
		} else {
			timeout.tv_sec = ((last_sampling + interval * 10000 - now) / 10) / 1000000;
			timeout.tv_usec = ((last_sampling + interval * 10000 - now) / 10) % 1000000;
		}

		FD_ZERO (&socketfd_set);
		FD_SET (socketfd, &socketfd_set);

		if ((ret = select (socketfd + 1, &socketfd_set, NULL, NULL, &timeout)) < 0) {
			if (errno == EINTR)
 				continue;
			g_warning ("mono-counters-agent: error with select : %s", strerror(errno));
			goto cleanup;
		}

		if (ret == 0) {
			if (!do_sampling (socketfd))
 				goto cleanup;

			last_sampling = mono_100ns_datetime ();
		} else {
			if (!read_socket_to_buffer (socketfd, (char*)&cmd, 1))
 				goto cleanup;

			switch (cmd) {
			case 0:
				if (!do_list_counters (socketfd))
					goto cleanup;
				break;
			case 1:
				if (!do_add_counter (socketfd))
					goto cleanup;
				break;
			case 2:
				if (!do_remove_counter (socketfd))
					goto cleanup;
				break;
			default:
				g_warning ("Unknown perf-agent command %d", cmd);
			}
		}
	}

cleanup:
	if (socketfd != -1)
		close (socketfd);
	free_agent_memory ();
	agent_running = FALSE;
	return NULL;
}

static void
parse_configuration (const gchar* configuration)
{
	const char *counters_names = NULL;
	const char *address = NULL;

	if (configuration == NULL)
		return;

	gchar **opts = g_strsplit (configuration, ",", -1), **ptr;
	for (ptr = opts; *ptr; ++ptr) {
		const char *opt = *ptr;
		if (g_str_has_prefix (opt, "address=")) {
			opt = strchr (opt, '=') + 1;
			address = g_strdup(opt);
		} else if (g_str_has_prefix (opt, "counters=")) {
			opt = strchr (opt, '=') + 1;
			counters_names = g_strdup(opt);
		} else if (g_str_has_prefix (opt, "interval=")) {
			opt = strchr (opt, '=') + 1;
			interval = strtoll (opt, NULL, 10);
		}
	}
	
	parse_counters_names(counters_names);
	parse_address(address);
	
	g_free((void*)counters_names);
	g_free((void*)address);
	g_strfreev(opts);
}

void
mono_counters_agent_start (const char *args)
{
	if (agent_running) {
		g_warning ("Agent already running");
		return;
	}
	agent_running = TRUE;
	parse_configuration(args);
	mono_threads_create_thread ((LPTHREAD_START_ROUTINE)&mono_counters_agent_sampling_thread, NULL, 0, 0, NULL);
}

#else

void
mono_counters_agent_start (const char *args)
{
	g_warning ("Perf agent not supported on windows");
}

#endif
