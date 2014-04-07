/*
 * Copyright 2014 Xamarin Inc
 *
 * Note : see the end of this file for a full description of the protocol.
 */

#include <config.h>

#ifndef HOST_WIN32

#include <stdlib.h>
#include <stdio.h>
#include <errno.h>
#include <fcntl.h>
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
	SRV_REMOVE_COUNTER,
	SVR_SAMPLES,
};

typedef struct MonoCounterAgent {
	MonoCounter *counter;
	// MonoCounterAgent specific data :
	void *value;
	short index;
} MonoCounterAgent;

static const char *inspector_dest;
static const char *inspector_type;
static int inspector_port;
static gint64 interval;

static GSList* counters;
static gboolean agent_running = FALSE;
static short current_index = 0;
static int fd = -1;

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
	g_free ((char*)inspector_type);
	g_free ((char*)inspector_dest);
	counters = NULL;
	inspector_type = NULL;
	inspector_dest = NULL;
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
		MonoCounter *counter;
		gchar **split = g_strsplit (*ptr, "/", 2);

		if (!split [0] || !split [1]) {
			g_warning ("Bad counter format '%s' use Category/Name", *ptr);
			goto end;
		}

		counter  = mono_counters_get (split [0], split [1]);
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

#define DEFAULT_TYPE "tcp"
#define DEFAULT_DEST "127.0.0.1"
#define DEFAULT_PORT 8888

static void
parse_tcp_address (const char *address)
{
	inspector_type = "tcp";

	gchar **split = g_strsplit (address, ":", 2);

	if (split [0])
		inspector_dest = g_strdup (split [0]);
	if (split [1])
		inspector_port = strtol (split [1], NULL, 10);

       g_strfreev (split);
}

static void
parse_file_address (const char *address)
{
	inspector_type = "file";
	inspector_dest = g_strdup (address);
}

static void
parse_address (const char *address)
{
	inspector_type = NULL;
	inspector_dest = NULL;
	inspector_port = DEFAULT_PORT;

	if (address) {
		gchar **split = g_strsplit (address, "://", 2);

		if (split [1] == NULL) {
			parse_tcp_address (split[0]);
		} else {
			if (strcmp (split [0], "file") == 0) {
				parse_file_address (split [1]);
			} else if (strcmp (split [0], "tcp") == 0) {
				parse_tcp_address (split [1]);
			} else {
				g_warning ("Unknown address type '%s'", split [0]);
			}
		}

		g_strfreev (split);
	}
	if (!inspector_type)
		inspector_type = g_strdup (DEFAULT_TYPE);
	if (!inspector_dest)
		inspector_dest = g_strdup (DEFAULT_DEST);
}

static gboolean
write_buffer (const char* buffer, size_t size)
{
	size_t sent = 0;

	while (sent < size) {
		ssize_t res = write (fd, buffer + sent, size - sent);
		if (res <= 0)
			return FALSE;
		sent += res;
	}

	return TRUE;
}

static gboolean
read_buffer (char* buffer, size_t size)
{
	size_t recvd = 0;

	while (recvd < size) {
		ssize_t res = read (fd, buffer + recvd, size - recvd);
		if (res <= 0)
			return FALSE;
		recvd += res;
	}

	return TRUE;
}

static gboolean
write_string (const char *str)
{
	int size = -1;

	if (!str)
		return write_buffer ((char*)&size, 4);

	size = strlen (str);
	if (!write_buffer ((char*)&size, 4))
		return FALSE;
	if (!write_buffer (str, size))
		return FALSE;

	return TRUE;
}

static gboolean
read_string (char **str)
{
	int len;

	if (!read_buffer ((char*)&len, 4))
		return FALSE;

	if (len < 0) {
		*str = NULL;
		return TRUE;
	}

	*str = g_malloc(len + 1);
	if (!read_buffer (*str, len))
		return FALSE;

	(*str) [len] = '\0';

	return TRUE;
}

static gboolean
write_counter_agent_header (MonoCounterAgent *counter_agent)
{
	if (!write_buffer((char*)&counter_agent->counter->category, 4) ||
		!write_string (counter_agent->counter->name) ||
		!write_buffer ((char*)&counter_agent->counter->type, 4) ||
		!write_buffer ((char*)&counter_agent->counter->unit, 4) ||
		!write_buffer ((char*)&counter_agent->counter->variance, 4) ||
		!write_buffer ((char*)&counter_agent->index, 2))
		return FALSE;

	return TRUE;
}

static gboolean
write_counter_agent_headers ()
{
	short count = g_slist_length (counters);
	GSList *item;

	if (!write_buffer ((char*)&count, 2))
		return FALSE;

	for (item = counters; item; item = item->next) {
		if (!write_counter_agent_header (item->data))
			return FALSE;
	}

	return TRUE;
}

static gboolean
do_hello ()
{
	char cmd = SRV_HELLO;
	short version = MONO_COUNTERS_AGENT_PROTOCOL_VERSION;

	if (!write_buffer ((char*)&cmd, 1))
		return FALSE;

	if (!write_buffer ((char*)&version, 2))
		return FALSE;

	if (!write_counter_agent_headers ())
		return FALSE;

	return TRUE;
}

static gboolean
write_counter (const char *category, const char *name)
{
	if (!write_string (category))
		return FALSE;
	if (!write_string (name))
		return FALSE;
	return TRUE;
}

static gboolean
do_list_counters ()
{
	char cmd = SRV_LIST_COUNTERS;

	if (!write_buffer ((char*)&cmd, 1))
		return FALSE;

	mono_counters_foreach (write_counter);
	write_string (NULL);

	return TRUE;
}


static gboolean
do_add_counter ()
{
	char cmd = SRV_ADD_COUNTERS;
	char *category = NULL;
	char *name = NULL;
	char *search_name = NULL;
	char status;
	MonoCounter *counter;
	GSList *list;
	MonoCounterCategory mcat;
	MonoCounterAgent *added_counter;

	if (!read_string (&category) || !category)
		goto fail;

	if (!read_string (&name) || !name)
		goto fail;

	mcat = mono_counters_category_name_to_id (category);

	search_name = name;
	if (mcat >= MONO_COUNTER_CAT_MAX) {
		mcat = MONO_COUNTER_CAT_CUSTOM;
		search_name = g_strdup_printf ("%s:%s", category, name);
	}

	for (list = counters; list; list = list->next) {
		MonoCounterAgent *ac = list->data;
		MonoCounter *c = ac->counter;
		if (c->category == mcat && !strcmp (c->name, search_name)) {
			status = AGENT_STATUS_EXISTING;
			goto fail;
		}
	}

	counter = mono_counters_get (category, name);
	if (!counter) {
		status = AGENT_STATUS_NOTFOUND;
		goto done;
	}

	added_counter = add_counter (counter);
	status = AGENT_STATUS_OK;

done:
	g_free (category);
	g_free (name);

	if (!write_buffer ((char*)&cmd, 1))
		return FALSE;

	if (!write_buffer (&status, 1))
		return FALSE;

	if (status == AGENT_STATUS_OK) {
		if (!write_counter_agent_header (added_counter))
			return FALSE;
	}

	return TRUE;

fail:
	g_free(category);
	g_free(name);
	if (search_name != name)
		g_free (search_name);
	return FALSE;
}

static gboolean
do_remove_counter ()
{
	char cmd = SRV_REMOVE_COUNTER;
	short index;
	char status;
	GSList *item;
	MonoCounterAgent *counter_agent = NULL, *counter_agent_tmp;

	if (!read_buffer ((char*)&index, 2))
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

	if (!write_buffer ((char*)&cmd, 1))
		return FALSE;

	return write_buffer (&status, 1);
}

static gboolean
do_sampling ()
{
	short end;
	char cmd = SVR_SAMPLES;
 	char buffer [8];
	GSList *item;
	MonoCounterAgent *counter_agent;

	if (!write_buffer ((char*)&cmd, 1))
		return FALSE;

	gint64 timestamp = mono_100ns_datetime ();
	if (!write_buffer ((char*)&timestamp, 8))
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

		if (!write_buffer ((char*)&counter_agent->index, 2) ||
			!write_buffer ((char*)&size, 2) ||
			!write_buffer (buffer, size))
			return FALSE;
	}

	end = -1;
	if (!write_buffer ((char*)&end, 2))
		return FALSE;

	return TRUE;
}

static void*
mono_counters_agent_tcp_sampling_thread (void* ptr)
{
	int ret;
	struct sockaddr_in inspector_addr;
	struct timeval timeout;
	fd_set socketfd_set;
	gint64 last_sampling = 0, now;
	char cmd;

	if ((fd = socket (AF_INET, SOCK_STREAM, 0)) < 0) {
		g_warning ("Error with socket : %s", strerror (errno));
		return NULL;
	}

	memset (&inspector_addr, 0, sizeof (inspector_addr));

	inspector_addr.sin_family = AF_INET;
	inspector_addr.sin_port = htons (inspector_port);

	if (!inet_pton (AF_INET, inspector_dest, &inspector_addr.sin_addr)) {
		g_warning ("Error with inet_pton : %s", strerror (errno));
		goto cleanup;
	}

	if (connect(fd, (struct sockaddr*)&inspector_addr, sizeof(inspector_addr)) < 0) {
		g_warning ("Error with connect : %s", strerror(errno));
		goto cleanup;
	}

	if (!do_hello ())
	 	goto cleanup;

	while (agent_running) {
		now = mono_100ns_datetime ();
		if (last_sampling == 0 || now > last_sampling + interval * 10000) {
			timeout.tv_sec = 0;
			timeout.tv_usec = 0;
		} else {
			timeout.tv_sec = ((last_sampling + interval * 10000 - now) / 10) / 1000000;
			timeout.tv_usec = ((last_sampling + interval * 10000 - now) / 10) % 1000000;
		}

		FD_ZERO (&socketfd_set);
		FD_SET (fd, &socketfd_set);

		if ((ret = select (fd + 1, &socketfd_set, NULL, NULL, &timeout)) < 0) {
			if (errno == EINTR)
 				continue;
			g_warning ("Error with select : %s", strerror(errno));
			goto cleanup;
		}

		if (ret == 0) {
			if (!do_sampling ())
 				goto cleanup;

			last_sampling = mono_100ns_datetime ();
		} else {
			if (!read_buffer ((char*)&cmd, 1))
 				goto cleanup;

			switch (cmd) {
			case 0:
				if (!do_list_counters ())
					goto cleanup;
				break;
			case 1:
				if (!do_add_counter ())
					goto cleanup;
				break;
			case 2:
				if (!do_remove_counter ())
					goto cleanup;
				break;
			case 127:
				g_warning ("Closing connection");
				goto cleanup;
			default:
				g_warning ("Unknown perf-agent command %d", cmd);
			}
		}
	}

cleanup:
	mono_counters_agent_stop ();
	return NULL;
}

static void*
mono_counters_agent_file_sampling_thread (void* ptr)
{
	if ((fd = open (inspector_dest, O_WRONLY | O_APPEND | O_CREAT | O_TRUNC, 0644)) < 0) {
		g_warning ("Impossible to open output file '%s'", inspector_dest);
		goto cleanup;
	}

	if (!do_hello ())
	 	goto cleanup;

	while (agent_running) {
		if (!do_sampling ())
			goto cleanup;

		usleep (interval * 1000);
	}

cleanup:
	mono_counters_agent_stop ();
	return NULL;
};

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

	if (strcmp (inspector_type, "tcp") == 0)
		mono_threads_create_thread ((LPTHREAD_START_ROUTINE)&mono_counters_agent_tcp_sampling_thread, NULL, 0, 0, NULL);
	else if (strcmp (inspector_type, "file") == 0)
		mono_threads_create_thread ((LPTHREAD_START_ROUTINE)&mono_counters_agent_file_sampling_thread, NULL, 0, 0, NULL);
}

void
mono_counters_agent_stop ()
{
	if (!agent_running) {
		g_warning ("Agent not running");
		return;
	}

	if (fd != -1) {
		close (fd);
		fd = -1;
	}
	free_agent_memory ();

	agent_running = FALSE;
}

#else

void
mono_counters_agent_start (const char *args)
{
	g_warning ("Perf agent not supported on windows");
}

void
mono_counters_agent_stop ()
{
	g_warning ("Perf agent not supported on windows");
}

#endif

/*
 * Full Protocol Description :
 *
 * DEFINITIONS:
 *
 * Agent : running in the Mono VM, tcp client or write to a file, in charge of : sending the samples to the inspector, execute add/remove/list counters commands.
 *
 * Inspector : tcp server or read a file, in charge of : displaying samples, send add/remove/lists counters commands. Can be written in any language that support socket or file streaming.
 *
 * TYPES:
 *
 * remark: there is two ways to send a list of values :
 *  - first, you can send the count first, and then the values
 *  - second, you send the values and finish the list by an end value ( marked as ‘type…endtype(value = end)’ )
 *
 * Char:
 * [value: 1 byte]
 *
 * Short:
 * [value: 2 bytes]
 *
 * Int:
 * [value: 4 bytes]
 *
 * Long:
 * [value: 8 bytes]
 *
 * Double:
 * [value: 8 bytes]
 *
 * String: if value == null, then length = -1 and there is no value;
 * [length: int]
 * [value: length * char]
 *
 * Header: describe a counter that is going to be sent during sampling
 * [category: int]
 * [name: string]
 * [type: int]
 * [unit: int]
 * [variance: int]
 * [index: short]
 *
 * Headers: list of header
 * [count: short] number of header
 * [value: count * header]
 *
 * Counter:
 * [name: string]
 * [category: string]
 *
 * Value:
 * [index: short]
 * [size: short]
 * [value: size * char]
 *
 * ResponseStatus:
 * [id: char] possible values : { 0x00: OK, 0x01: NOK, 0x02: NOTFOUND, 0x03: EXISTING }
 *
 * COMMANDS:
 *
 * Hello: first command of the protocol, sent by the agent to the inspector on connection. One way from agent to inspector.
 * [cmd: char] 0
 * [version: short] version of the protocol, currently 0.1 => 1
 * [headers: headers] the agent counters headers.
 *
 * List counters: list all the mono counters that are available for sampling. Request-Response from inspector to agent.
 * Request:
 * [cmd: char(value = 1)]
 * Response:
 * [cmd: char(value = 1)]
 * [mono counter: counter…string(value = null)]
 *
 * Add Counter: add a counter to the sample set. Request-Response from inspector to agent.
 * Request:
 * [cmd: char(value = 2)]
 * [name: string]
 * [category: string]
 * Response:
 * [cmd: char(value = 2)]
 * [status: response_status]
 * [header: header]
 *
 * Remove Counter: remove counter from the sample set. Request-Response from inspector to agent.
 * Request:
 * [cmd: char(value = 3)]
 * [index: short]
 * Response:
 * [cmd: char(value = 3)]
 * [status: response_status]
 *
 * Sampling: actual sampling of the data. One way from agent to inspector.
 * [cmd: 4]
 * [timestamp: long]
 * [values: value…short(value = -1)]
 *
 * Good Bye: close the connection. One way from inspector to agent.
 * [cmd: 127]
 */
