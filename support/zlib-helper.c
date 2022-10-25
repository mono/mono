/*
 * Used by System.IO.Compression.DeflateStream
 *
 * Author:
 *   Gonzalo Paniagua Javier (gonzalo@novell.com)
 *
 * (c) Copyright 2009 Novell, Inc.
 */
#include <config.h>
#if defined (HAVE_SYS_ZLIB)
#include <zlib.h>
#else
#include "../mono/zlib/zlib.h"
#endif

#include <glib.h>
#include <string.h>
#include <stdlib.h>

#ifndef MONO_API
#define MONO_API
#endif

#ifndef TRUE
#define FALSE 0
#define TRUE 1
#endif

#define BUFFER_SIZE 4096
#define ARGUMENT_ERROR -10
#define IO_ERROR -11
#define MONO_EXCEPTION -12

#define z_new0(type)  ((type *) calloc (sizeof (type), 1))

typedef gint (*read_write_func) (guchar *buffer, gint length, void *gchandle);
struct _ZStream {
	z_stream *stream;
	guchar *buffer;
	read_write_func func;
	void *gchandle;
	guchar compress;
	guchar eof;
	guint32 total_in;
};
typedef struct _ZStream ZStream;

// FIXME? Names should start "mono"?
MONO_API ZStream *CreateZStream (gint compress, guchar gzip, read_write_func func, void *gchandle);
MONO_API gint CloseZStream (ZStream *zstream);
MONO_API gint Flush (ZStream *stream);
MONO_API gint ReadZStream (ZStream *stream, guchar *buffer, gint length);
MONO_API gint WriteZStream (ZStream *stream, guchar *buffer, gint length);
static gint flush_internal (ZStream *stream, gboolean is_final);

static void *
z_alloc (void *opaque, unsigned int nitems, unsigned int item_size)
{
	return calloc (nitems, item_size);
}

static void
z_free (void *opaque, void *ptr)
{
	free (ptr);
}

ZStream *
CreateZStream (gint compress, guchar gzip, read_write_func func, void *gchandle)
{
	z_stream *z;
	gint retval;
	ZStream *result;

	if (func == NULL)
		return NULL;

	z = z_new0 (z_stream);
	if (compress) {
		retval = deflateInit2 (z, Z_DEFAULT_COMPRESSION, Z_DEFLATED, gzip ? 31 : -15, 8, Z_DEFAULT_STRATEGY);
	} else {
		retval = inflateInit2 (z, gzip ? 31 : -15);
	}

	if (retval != Z_OK) {
		free (z);
		return NULL;
	}
	z->zalloc = z_alloc;
	z->zfree = z_free;
	result = z_new0 (ZStream);
	result->stream = z;
	result->func = func;
	result->gchandle = gchandle;
	result->compress = compress;
	result->buffer = (guchar*)malloc (BUFFER_SIZE);
	result->stream->next_out = result->buffer;
	result->stream->avail_out = BUFFER_SIZE;
	result->stream->total_in = 0;
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
				if (flush_status == MONO_EXCEPTION) {
					status = flush_status;
					break;
				}
			} while (status == Z_OK); /* We want Z_STREAM_END or error here here */
			if (status == Z_STREAM_END)
				status = flush_status;
		}
		deflateEnd (zstream->stream);
	} else {
		inflateEnd (zstream->stream);
	}
	free (zstream->buffer);
	free (zstream->stream);
	memset (zstream, 0, sizeof (ZStream));
	free (zstream);
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
		if (n == MONO_EXCEPTION)
			return n;
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

	if (!is_final && stream->stream->avail_in != 0) {
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
			if (n == MONO_EXCEPTION)
				return n;
			n = n < 0 ? 0 : n;
			stream->total_in += n;
			zs->next_in = stream->buffer;
			zs->avail_in = n;
		}

		if (zs->avail_in == 0 && zs->total_in == 0)
			return 0;

		status = inflate (stream->stream, Z_SYNC_FLUSH);
		if (status == Z_STREAM_END) {
			stream->eof = TRUE;
			break;
		} else if (status == Z_BUF_ERROR && stream->total_in == zs->total_in) {
			if (zs->avail_in != 0) {
				stream->eof = TRUE;
			}
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

