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
static int frequency;

static GSList* counters;
static gboolean agent_running;

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
}

static void
parse_counters_names (const char *counters_names)
{
	short index = 0;
	gchar **names, **ptr;

	if (!counters_names)
		return;

	names = g_strsplit (counters_names, ";", -1);

	for (ptr = names; *ptr; ++ptr) {
		MonoCounterCategory category;
		MonoCounter *counter;
		MonoCounterAgent *agent_counter;
		gchar **split = g_strsplit(*ptr, "/", 2);
		if (!split[0] || !split[1]) {
			g_warning ("Bad counter format '%s' use Category/Name", *ptr);
			goto end;
		}
	
		category = mono_counters_category_name_to_id (split [0]);
		if (!category) {
			g_warning ("Category %s not found", split [0]);
			goto end;
		}

		counter  = mono_counters_get (category, split [1]);
		if (!counter) {
			g_warning ("Counter %s not found in category %s", split [1], split [0]);
			goto end;
		}

		agent_counter = g_new0 (MonoCounterAgent, 1);
		agent_counter->counter = counter;
		agent_counter->index   = index++;

		counters = g_slist_append (counters, agent_counter);

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
	inspector_ip = DEFAULT_ADDRESS;
	inspector_port = DEFAULT_PORT;

	if (address) {
		gchar **split = g_strsplit (address, ":", 2);

		if (split [0])
			inspector_ip = g_strdup (split [0]);
		if (split [1])
			inspector_port = strtol (split [1], NULL, 10);

		g_strfreev(split);
	}
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

static void*
mono_counters_agent_sampling_thread (void* ptr)
{
	GSList *item;
	char buffer[8];
	int socketfd = -1;
	short count = 0;
	struct sockaddr_in inspector_addr;
	
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

		if (!write_buffer_to_socket(socketfd, (char*)&counter->counter->category, 4)
				|| !write_buffer_to_socket (socketfd, (char*)&len, 4)
				|| !write_buffer_to_socket (socketfd, (char*)counter->counter->name, len)
				|| !write_buffer_to_socket (socketfd, (char*)&counter->counter->type, 4)
				|| !write_buffer_to_socket (socketfd, (char*)&counter->counter->unit, 4)
				|| !write_buffer_to_socket (socketfd, (char*)&counter->counter->variance, 4)
				|| !write_buffer_to_socket (socketfd, (char*)&counter->index, 2))
			goto cleanup;
	}

	for (;;) {
		gint64 timestamp = mono_100ns_ticks ();
		if (!write_buffer_to_socket (socketfd, (char*)&timestamp, 8))
			goto cleanup;

		for (item = counters; item; item = item->next) {
			MonoCounterAgent* counter = item->data;

			if (!counter)
				continue;

			int size = mono_counters_size (counter->counter);
			if (size < 0)
				goto cleanup;
	
			if (mono_counters_sample (counter->counter, buffer, size) != size)
				goto cleanup;
	
			if (!counter->value)
				counter->value = g_malloc (size);
			else if (memcmp (counter->value, buffer, size) == 0)
				continue;
	
			memcpy (counter->value, buffer, size);
	
			if (!write_buffer_to_socket (socketfd, (char*)&counter->index, 2)
					|| !write_buffer_to_socket (socketfd, (char*)&size, 2)
					|| !write_buffer_to_socket (socketfd, (char*) buffer, size))
				goto cleanup;
		}
		
		short end = -1;
		if (!write_buffer_to_socket (socketfd, (char*)&end, 2))
			goto cleanup;

		usleep (1000000 / frequency);
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
		} else if (g_str_has_prefix (opt, "frequency=")) {
			opt = strchr (opt, '=') + 1;
			frequency = strtol (opt, NULL, 10);
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
