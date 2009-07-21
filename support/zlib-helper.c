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

#include <string.h>
#include <stdlib.h>

#ifndef TRUE
#define FALSE 0
#define TRUE 1
#endif

#define BUFFER_SIZE 4096
#define ARGUMENT_ERROR -10
#define IO_ERROR -11

typedef int (*read_write_func) (unsigned char *buffer, int length);
struct _ZStream {
	z_stream *stream;
	unsigned char *buffer;
	read_write_func func;
	unsigned char compress;
	unsigned char eof;
};
typedef struct _ZStream ZStream;

ZStream *CreateZStream (int compress, unsigned char gzip, read_write_func func);
int CloseZStream (ZStream *zstream);
int Flush (ZStream *stream);
int ReadZStream (ZStream *stream, unsigned char *buffer, int length);
int WriteZStream (ZStream *stream, unsigned char *buffer, int length);


ZStream *
CreateZStream (int compress, unsigned char gzip, read_write_func func)
{
	z_stream *z;
	int retval;
	ZStream *result;

	if (func == NULL)
		return NULL;

#if !defined(ZLIB_VERNUM) || (ZLIB_VERNUM < 0x1204)
	/* Older versions of zlib do not support raw deflate or gzip */
	return NULL;
#endif

	z = (z_stream *) malloc (sizeof (z_stream));
	if (z == NULL)
		return NULL;
	memset (z, 0, sizeof (z_stream));
	if (compress) {
		retval = deflateInit2 (z, Z_DEFAULT_COMPRESSION, Z_DEFLATED, gzip ? 31 : -15, 8, Z_DEFAULT_STRATEGY);
	} else {
		retval = inflateInit2 (z, gzip ? 31 : -15);
	}

	if (retval != Z_OK) {
		free (z);
		return NULL;
	}
	result = malloc (sizeof (ZStream));
	memset (result, 0, sizeof (ZStream));
	result->stream = z;
	result->func = func;
	result->compress = compress;
	result->buffer = (unsigned char *) malloc (BUFFER_SIZE);
	if (result->buffer == NULL) {
		free (result);
		if (compress) {
			deflateEnd (z);
		} else {
			inflateEnd (z);
		}
		free (z);
		return NULL;
	}
	memset (result->buffer, 0, BUFFER_SIZE);
	return result;
}

int
CloseZStream (ZStream *zstream)
{
	int status;
	int flush_status;

	if (zstream == NULL)
		return ARGUMENT_ERROR;

	status = 0;
	if (zstream->compress) {
		status = deflate (zstream->stream, Z_FINISH);
		flush_status = Flush (zstream);
		if (status == Z_OK || status == Z_STREAM_END)
			status = flush_status;
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

static int
write_to_managed (ZStream *stream)
{
	int n;
	z_stream *zs;

	zs = stream->stream;
	if (zs->avail_out != BUFFER_SIZE) {
		n = stream->func (stream->buffer, BUFFER_SIZE - zs->avail_out);
		zs->next_out = stream->buffer;
		zs->avail_out =  BUFFER_SIZE;
		if (n < 0)
			return IO_ERROR;
	}
	return 0;
}

int
Flush (ZStream *stream)
{
	if (!stream->compress)
		return 0;

	return write_to_managed (stream);
}

int
ReadZStream (ZStream *stream, unsigned char *buffer, int length)
{
	int n;
	int status;
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
			n = stream->func (stream->buffer, BUFFER_SIZE);
			if (n <= 0) {
				stream->eof = TRUE;
				break;
			}
			zs->next_in = stream->buffer;
			zs->avail_in = n;
		}

		status = inflate (stream->stream, Z_SYNC_FLUSH);
		if (status != Z_OK && status != Z_STREAM_END)
			return status;
	}
	return length - zs->avail_out;
}

int
WriteZStream (ZStream *stream, unsigned char *buffer, int length)
{
	int n;
	int status;
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
		status = deflate (stream->stream, Z_SYNC_FLUSH);
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

