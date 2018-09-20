//
//  openssl-bio.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 14/11/15.
//  Copyright (c) 2015 Xamarin. All rights reserved.
//

#include "openssl-ssl.h"
#include "openssl-bio.h"
#include <openssl/err.h>
#include <errno.h>

struct MonoOpenSSLBio {
	const void *instance;
	MonoOpenSSLReadFunc read_func;
	MonoOpenSSLWriteFunc write_func;
	MonoOpenSSLControlFunc control_func;
};

#if 0
static void
mono_debug (const char *message)
{
	BIO *bio_err;
	bio_err = BIO_new_fp (stderr, BIO_NOCLOSE);
	fprintf (stderr, "DEBUG: %s\n", message);
	ERR_print_errors (bio_err);
}
#endif

static int
mono_read (BIO *bio, char *out, int outl)
{
	MonoOpenSSLBio *mono = (MonoOpenSSLBio *)bio->ptr;
	int ret, wantMore;

	if (!mono)
		return -1;

	ret = mono->read_func (mono->instance, out, outl, &wantMore);

	if (ret < 0) {
		errno = EIO;
		return -1;
	}
	if (ret > 0)
		return ret;

	if (wantMore) {
		errno = EAGAIN;
		BIO_set_retry_read (bio);
		return -1;
	}

	return 0;
}

static int
mono_write (BIO *bio, const char *in, int inl)
{
	MonoOpenSSLBio *mono = (MonoOpenSSLBio *)bio->ptr;

	if (!mono)
		return -1;

	return mono->write_func (mono->instance, in, inl);
}

static long
mono_ctrl (BIO *bio, int cmd, long num, void *ptr)
{
	MonoOpenSSLBio *mono = (MonoOpenSSLBio *)bio->ptr;

	if (!mono)
		return -1;

	// fprintf (stderr, "mono_ctrl: %x - %lx - %p\n", cmd, num, ptr);
	switch (cmd) {
		case BIO_CTRL_FLUSH:
			return mono->control_func (mono->instance, MONO_OPENSSL_CONTROL_COMMAND_FLUSH, 0);
		default:
			return -1;
	}
	return -1;
}

static int
mono_new (BIO *bio)
{
	// mono_debug("mono_new!\n");
	bio->init = 0;
	bio->num = -1;
	bio->flags = 0;
	return 1;
}

static int
mono_free (BIO *bio)
{
	// mono_debug ("mono_free!\n");
	if (bio->ptr) {
		MonoOpenSSLBio *mono = (MonoOpenSSLBio *)bio->ptr;

		bio->ptr = NULL;
		mono->instance = NULL;
		mono->read_func = NULL;
		mono->write_func = NULL;
		mono->control_func = NULL;
		free (mono);
	}
	return 1;
}

static const BIO_METHOD mono_method = {
	BIO_TYPE_NONE, "mono", mono_write, mono_read,
	NULL, NULL, mono_ctrl, mono_new, mono_free, NULL
};

MONO_API BIO *
mono_tls_bio_mono_new (void)
{
	BIO *bio;
	MonoOpenSSLBio *monoBio;

	bio = BIO_new ((BIO_METHOD *) &mono_method);
	if (!bio)
		return NULL;

	monoBio = calloc (1, sizeof (MonoOpenSSLBio));
	if (!monoBio) {
		BIO_free (bio);
		return NULL;
	}

	bio->ptr = monoBio;
	bio->init = 0;

	return bio;
}

MONO_API void
mono_tls_bio_mono_initialize (BIO *bio, const void *instance,
			      MonoOpenSSLReadFunc read_func, MonoOpenSSLWriteFunc write_func,
			      MonoOpenSSLControlFunc control_func)
{
	MonoOpenSSLBio *monoBio = bio->ptr;

	monoBio->instance = instance;
	monoBio->read_func = read_func;
	monoBio->write_func = write_func;
	monoBio->control_func = control_func;

	bio->init = 1;
}

MONO_API int
mono_tls_bio_read (BIO *bio, void *data, int len)
{
	return BIO_read (bio, data, len);
}

MONO_API int
mono_tls_bio_write (BIO *bio, const void *data, int len)
{
	return BIO_write (bio, data, len);
}

MONO_API int
mono_tls_bio_flush (BIO *bio)
{
	return BIO_flush (bio);
}

MONO_API int
mono_tls_bio_indent (BIO *bio, unsigned indent, unsigned max_indent)
{
	return BIO_indent (bio, indent, max_indent);
}

MONO_API int
mono_tls_bio_hexdump (BIO *bio, const uint8_t *data, int len, unsigned indent)
{
	return (int) BIO_dump (bio, (char *) data, len);
}

MONO_API void
mono_tls_bio_print_errors (BIO *bio)
{
	ERR_print_errors (bio);
}

MONO_API void
mono_tls_bio_free (BIO *bio)
{
	BIO_free (bio);
}

MONO_API BIO *
mono_tls_bio_mem_new (void)
{
	return BIO_new (BIO_s_mem ());
}

MONO_API int
mono_tls_bio_mem_get_data (BIO *bio, void **data)
{
	return (int)BIO_get_mem_data (bio, (char**)data);
}
