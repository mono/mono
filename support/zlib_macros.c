/*
 * Helper routines to use Zlib
 *
 * Author:
 *   Christopher Lahey (clahey@ximian.co)
 *
 * (C) 2004 Novell, Inc.
 */
#include <zlib.h>
#include <stdlib.h>

z_stream *
create_z_stream(int compress, unsigned char gzip)
{
	z_stream *z;
	int retval;

	z = malloc (sizeof (z_stream));
	z->next_in = Z_NULL;
	z->avail_in = 0;
	z->next_out = Z_NULL;
	z->avail_out = 0;
	z->zalloc = Z_NULL;
	z->zfree = Z_NULL;
	z->opaque = NULL;
	if (compress) {
		retval = deflateInit2 (z, Z_DEFAULT_COMPRESSION, Z_DEFLATED, gzip ? 31 : -15, 8, Z_DEFAULT_STRATEGY);
	} else {
		retval = inflateInit2 (z, gzip ? 31 : -15);
	}

	if (retval == Z_OK)
		return z;

	free (z);
	return NULL;
}

void
free_z_stream(z_stream *z, int compress)
{
	if (compress) {
		deflateEnd (z);
	} else {
		inflateEnd (z);
	}
	free (z);
}

void
z_stream_set_next_in(z_stream *z, unsigned char *next_in)
{
	z->next_in = next_in;
}

void
z_stream_set_avail_in(z_stream *z, int avail_in)
{
	z->avail_in = avail_in;
}

int
z_stream_get_avail_in(z_stream *z)
{
	return z->avail_in;
}

void
z_stream_set_next_out(z_stream *z, unsigned char *next_out)
{
	z->next_out = next_out;
}

void
z_stream_set_avail_out(z_stream *z, int avail_out)
{
	z->avail_out = avail_out;
}

int
z_stream_deflate (z_stream *z, int flush, unsigned char *next_out, int *avail_out)
{
	int ret_val;

	z->next_out = next_out;
	z->avail_out = *avail_out;

	ret_val = deflate (z, flush);

	*avail_out = z->avail_out;

	return ret_val;
}

int
z_stream_inflate (z_stream *z, int *avail_out)
{
	int ret_val;

	z->avail_out = *avail_out;

	ret_val = inflate (z, Z_NO_FLUSH);

	*avail_out = z->avail_out;

	return ret_val;
}
