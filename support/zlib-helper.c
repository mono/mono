/*
 * Used by System.IO.Compression.DeflateStream
 *
 * Author:
 *   Gonzalo Paniagua Javier (gonzalo@novell.com)
 *
 * (c) Copyright 2009 Novell, Inc.
 */
#include <config.h>
#if defined (HAVE_ZLIB)
#include <zlib.h>
#else
#include "zlib.h"
#endif

#include <glib.h>
#include <string.h>
#include <stdlib.h>

#ifndef TRUE
#define FALSE 0
#define TRUE 1
#endif

#define BUFFER_SIZE 4096
#define ARGUMENT_ERROR -10
#define IO_ERROR -11

typedef gint (*read_write_func) (guchar *buffer, gint length, void *gchandle);
struct _ZStream {
	z_stream *stream;
	guchar *buffer;
	read_write_func func;
	void *gchandle;
	guchar compress;
	guchar eof;
};
typedef struct _ZStream ZStream;

ZStream *CreateZStream (gint compress, guchar gzip, read_write_func func, void *gchandle);
gint CloseZStream (ZStream *zstream);
gint Flush (ZStream *stream);
gint ReadZStream (ZStream *stream, guchar *buffer, gint length);
gint WriteZStream (ZStream *stream, guchar *buffer, gint length);
static gint flush_internal (ZStream *stream, gboolean is_final);

static void *
z_alloc (void *opaque, gsize nitems, gsize item_size)
{
	return g_malloc0 (nitems * item_size);
}

static void
z_free (void *opaque, void *ptr)
{
	g_free (ptr);
}

ZStream *
CreateZStream (gint compress, guchar gzip, read_write_func func, void *gchandle)
{
	z_stream *z;
	gint retval;
	ZStream *result;

	if (func == NULL)
		return NULL;

#if !defined(ZLIB_VERNUM) || (ZLIB_VERNUM < 0x1204)
	/* Older versions of zlib do not support raw deflate or gzip */
	return NULL;
#endif

	z = g_new0 (z_stream, 1);
	if (compress) {
		retval = deflateInit2 (z, Z_DEFAULT_COMPRESSION, Z_DEFLATED, gzip ? 31 : -15, 8, Z_DEFAULT_STRATEGY);
	} else {
		retval = inflateInit2 (z, gzip ? 31 : -15);
	}

	if (retval != Z_OK) {
		g_free (z);
		return NULL;
	}
	z->zalloc = z_alloc;
	z->zfree = z_free;
	result = g_new0 (ZStream, 1);
	result->stream = z;
	result->func = func;
	result->gchandle = gchandle;
	result->compress = compress;
	result->buffer = g_new (guchar, BUFFER_SIZE);
	return result;
}

gint
CloseZStream (ZStream *zstream)
{
	gint status;
	gint flush_status;

	if (zstream == NULL)
		return ARGUMENT_ERROR;

	status = 0;
	if (zstream->compress) {
		if (zstream->stream->total_in > 0) {
			do {
				status = deflate (zstream->stream, Z_FINISH);
				flush_status = flush_internal (zstream, TRUE);
			} while (status == Z_OK); /* We want Z_STREAM_END or error here here */
			if (status == Z_STREAM_END)
				status = flush_status;
		}
		deflateEnd (zstream->stream);
	} else {
		inflateEnd (zstream->stream);
	}
	g_free (zstream->buffer);
	g_free (zstream->stream);
	memset (zstream, 0, sizeof (ZStream));
	g_free (zstream);
	return status;
}

static gint
write_to_managed (ZStream *stream)
{
	gint n;
	z_stream *zs;

	zs = stream->stream;
	if (zs->avail_out != BUFFER_SIZE) {
		n = stream->func (stream->buffer, BUFFER_SIZE - zs->avail_out, stream->gchandle);
		zs->next_out = stream->buffer;
		zs->avail_out = BUFFER_SIZE;
		if (n < 0)
			return IO_ERROR;
	}
	return 0;
}

static gint
flush_internal (ZStream *stream, gboolean is_final)
{
	gint status;

	if (!stream->compress)
		return 0;

	if (!is_final) {
		status = deflate (stream->stream, Z_PARTIAL_FLUSH);
		if (status != Z_OK && status != Z_STREAM_END)
			return status;
	}
	return write_to_managed (stream);
}

gint
Flush (ZStream *stream)
{
	return flush_internal (stream, FALSE);
}

gint
ReadZStream (ZStream *stream, guchar *buffer, gint length)
{
	gint n;
	gint status;
	z_stream *zs;

	if (stream == NULL || buffer == NULL || length < 0)
		return ARGUMENT_ERROR;

	if (stream->eof)
		return 0;

	zs = stream->stream;
	zs->next_out = buffer;
	zs->avail_out = length;
	while (zs->avail_out > 0) {
		if (zs->avail_in == 0) {
			n = stream->func (stream->buffer, BUFFER_SIZE, stream->gchandle);
			if (n <= 0) {
				stream->eof = TRUE;
				break;
			}
			zs->next_in = stream->buffer;
			zs->avail_in = n;
		}

		status = inflate (stream->stream, Z_SYNC_FLUSH);
		if (status == Z_STREAM_END) {
			stream->eof = TRUE;
			break;
		} else if (status != Z_OK) {
			return status;
		}
	}
	return length - zs->avail_out;
}

gint
WriteZStream (ZStream *stream, guchar *buffer, gint length)
{
	gint n;
	gint status;
	z_stream *zs;

	if (stream == NULL || buffer == NULL || length < 0)
		return ARGUMENT_ERROR;

	if (stream->eof)
		return IO_ERROR;

	zs = stream->stream;
	zs->next_in = buffer;
	zs->avail_in = length;
	while (zs->avail_in > 0) {
		if (zs->avail_out == 0) {
			zs->next_out = stream->buffer;
			zs->avail_out = BUFFER_SIZE;
		}
		status = deflate (stream->stream, Z_NO_FLUSH);
		if (status != Z_OK && status != Z_STREAM_END)
			return status;

		if (zs->avail_out == 0) {
			n = write_to_managed (stream);
			if (n < 0)
				return n;
		}
	}
	return length;
}

