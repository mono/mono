//
//  openssl-bio.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 14/11/15.
//  Copyright (c) 2015 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_bio__
#define __openssl__openssl_bio__

#include <stdio.h>
#include "openssl-ssl.h"

typedef enum {
	MONO_OPENSSL_CONTROL_COMMAND_FLUSH	= 1
} MonoOpenSSLControlCommand;

typedef int (* MonoOpenSSLReadFunc) (const void *instance, const void *buf, int size, int *wantMore);
typedef int (* MonoOpenSSLWriteFunc) (const void *instance, const void *buf, int size);
typedef int64_t (* MonoOpenSSLControlFunc) (const void *instance, MonoOpenSSLControlCommand command, int64_t arg);

BIO *
mono_openssl_bio_mono_new (void);

void
mono_openssl_bio_mono_initialize (BIO *bio, const void *instance,
			      MonoOpenSSLReadFunc read_func, MonoOpenSSLWriteFunc write_func,
			      MonoOpenSSLControlFunc control_func);

int
mono_openssl_bio_read (BIO *bio, void *data, int len);

int
mono_openssl_bio_write (BIO *bio, const void *data, int len);

int
mono_openssl_bio_flush (BIO *bio);

int
mono_openssl_bio_indent (BIO *bio, unsigned indent, unsigned max_indent);

int
mono_openssl_bio_hexdump (BIO *bio, const uint8_t *data, int len, unsigned indent);

void
mono_openssl_bio_print_errors (BIO *bio);

void
mono_openssl_bio_free (BIO *bio);

BIO *
mono_openssl_bio_mem_new (void);

int
mono_openssl_bio_mem_get_data (BIO *bio, void **data);

#endif /* defined(__openssl__openssl_bio__) */
