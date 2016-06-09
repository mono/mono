/*
 * gmem.c: memory utility functions
 *
 * Author:
 * 	Gonzalo Paniagua Javier (gonzalo@novell.com)
 *
 * (C) 2006 Novell, Inc.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
#include <stdio.h>
#include <string.h>
#include <glib.h>

volatile gint64 mono_stat_malloc_memory = 0;

#ifdef MONO_ENABLE_ALLOG

#include <inttypes.h>
#include <pthread.h>

typedef struct {
	gpointer address;
	gsize size;
} Allocation;

#define ALLOG_SIZE (1024 * 1024)
static Allocation allog [ALLOG_SIZE];
static Allocation *allog_current;
static pthread_mutex_t allog_lock;
static gboolean allog_inited = FALSE;

static gsize
allog_find (const gpointer address)
{
	for (Allocation *p = allog; p < &allog [0] + ALLOG_SIZE; ++p)
		if (p->address == address)
			return p->size;
	return 0;
}

static void
allog_insert (const gpointer address, const gsize size)
{
	for (Allocation *p = allog_current; p < &allog [0] + ALLOG_SIZE; ++p) {
		if (!p->address) {
			p->address = address;
			p->size = size;
			allog_current = p;
			return;
		}
	}
	for (Allocation *p = &allog [0]; p < allog_current; ++p) {
		if (!p->address) {
			p->address = address;
			p->size = size;
			allog_current = p;
			return;
		}
	}
	g_error ("Allocation log full.\n");
}

static void
allog_remove (const gpointer address)
{
	for (Allocation *p = &allog [0]; p < &allog [0] + ALLOG_SIZE; ++p) {
		if (p->address == address) {
			p->address = NULL;
			p->size = 0;
			allog_current = p;
			return;
		}
	}
	g_error ("Freeing unallocated pointer %p\n", address);
}

static void
allog_init (void)
{
	if (allog_inited)
		return;
	allog_current = &allog [0];
	allog_inited = TRUE;
}

void
mono_allog_alloc (const gpointer address, const gsize size)
{
	pthread_mutex_lock (&allog_lock);
	allog_init ();
	const gsize existing = allog_find (address);
	if (existing)
		g_error ("Address %p already in use with size %zu when allocating %zu bytes\n", address, existing, size);
	allog_insert (address, size);
	pthread_mutex_unlock (&allog_lock);
}

void
mono_allog_free (const gpointer address, const gsize size)
{
	/* free(null) is fine. */
	if (!address)
		return;
	pthread_mutex_lock (&allog_lock);
	if (!allog_inited)
		g_error ("Freeing %zu bytes at %p before allocating anything\n", size, address);
	allog_init ();
	gsize existing = allog_find (address);
	if (!existing)
		g_error ("Freeing pointer %p of size %zu that was not allocated\n", address, size);
	if (existing != size)
		g_error ("Allocated size %zu did not match expected size %zu when freeing %p\n", existing, size, address);
	allog_remove (address);
	pthread_mutex_unlock (&allog_lock);
}

#else

void
mono_allog_alloc (const gpointer address, const gsize size)
{
}

void
mono_allog_free (const gpointer address, const gsize size)
{
}

#endif /* MONO_ENABLE_ALLOG */

void
g_free (void *ptr)
{
	if (ptr != NULL) {
		gsize size = g_malloc_size (ptr);
		g_atomic_pointer_add (&mono_stat_malloc_memory, -(gssize)size);
		mono_allog_free (ptr, size);
		free (ptr);
	}
}

gpointer
g_memdup (gconstpointer mem, guint byte_size)
{
	gpointer ptr;

	if (mem == NULL)
		return NULL;

	ptr = g_malloc (byte_size);
	if (ptr != NULL)
		memcpy (ptr, mem, byte_size);

	return ptr;
}

gpointer g_realloc (gpointer obj, gsize size)
{
	gpointer ptr;
	if (!size) {
		g_free (obj);
		return 0;
	}
	gsize old_size = g_malloc_size (obj);
	ptr = realloc (obj, size);
	if (ptr) {
		gsize new_size = g_malloc_size (ptr);
		g_atomic_pointer_add (&mono_stat_malloc_memory, new_size - old_size);
		mono_allog_free (obj, old_size);
		mono_allog_alloc (ptr, new_size);
		return ptr;
	}
	g_error ("Could not allocate %i bytes", size);
}

gpointer 
g_malloc (gsize x) 
{ 
	gpointer ptr;
	if (!x)
		return 0;
	ptr = malloc (x);
	if (ptr) {
		gsize size = g_malloc_size (ptr);
		g_atomic_pointer_add (&mono_stat_malloc_memory, size);
		mono_allog_alloc (ptr, size);
		return ptr;
	}
	g_error ("Could not allocate %i bytes", x);
}

gpointer g_malloc0 (gsize x) 
{ 
	gpointer ptr; 
	if (!x) 
		return 0; 
	ptr = calloc(1,x); 
	if (ptr) {
		gsize size = g_malloc_size (ptr);
		g_atomic_pointer_add (&mono_stat_malloc_memory, size);
		mono_allog_alloc (ptr, size);
		return ptr;
	}
	g_error ("Could not allocate %i bytes", x);
}

gpointer g_try_malloc (gsize x) 
{
	if (x) {
		gpointer ptr = malloc (x);
		if (ptr) {
			gsize size = g_malloc_size (ptr);
			g_atomic_pointer_add (&mono_stat_malloc_memory, size);
			mono_allog_alloc (ptr, size);
		}
		return ptr;
	}
	return 0;
}


gpointer g_try_realloc (gpointer obj, gsize size)
{ 
	gpointer ptr;
	if (!size) {
		g_free (obj);
		return 0;
	} 
	gsize old_size = g_malloc_size (obj);
	ptr = realloc (obj, size);
	if (ptr) {
		gsize new_size = g_malloc_size (ptr);
		g_atomic_pointer_add (&mono_stat_malloc_memory, new_size - old_size);
		mono_allog_free (obj, old_size);
		mono_allog_alloc (ptr, new_size);
	}
	return ptr;
}
