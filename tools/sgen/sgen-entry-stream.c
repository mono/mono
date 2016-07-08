/*
 * sgen-entry-stream.c: EntryStream implementation
 *
 * Copyright (C) 2016 Xamarin Inc
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <stdlib.h>
#include <unistd.h>
#include <glib.h>
#include "sgen-entry-stream.h"

#define BUFFER_SIZE (1 << 20)

void
init_stream (EntryStream *stream, int file)
{
	stream->file = file;
	stream->buffer = g_malloc0 (BUFFER_SIZE);
	stream->end = stream->buffer + BUFFER_SIZE;
	stream->pos = stream->end;
}

void
reset_stream (EntryStream *stream)
{
	stream->end = stream->buffer + BUFFER_SIZE;
	stream->pos = stream->end;
	lseek (stream->file, 0, SEEK_SET);
}

void
close_stream (EntryStream *stream)
{
	g_free (stream->buffer);
}

gboolean
refill_stream (EntryStream *in, size_t size)
{
	size_t remainder = in->end - in->pos;
	ssize_t refilled;
	g_assert (size > 0);
	g_assert (in->pos >= in->buffer);
	if (in->pos + size <= in->end)
		return TRUE;
	memmove (in->buffer, in->pos, remainder);
	in->pos = in->buffer;
	refilled = read (in->file, in->buffer + remainder, BUFFER_SIZE - remainder);
	if (refilled < 0)
		return FALSE;
	g_assert (refilled + remainder <= BUFFER_SIZE);
	in->end = in->buffer + refilled + remainder;
	return in->end - in->buffer >= size;
}

ssize_t
read_stream (EntryStream *stream, void *out, size_t size)
{
	if (refill_stream (stream, size)) {
		memcpy (out, stream->pos, size);
		stream->pos += size;
		return size;
	}
	return 0;
}
