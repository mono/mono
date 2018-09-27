//
//  btls-bio.h
//  MonoBtls
//
//  Created by Martin Baulig on 14/11/15.
//  Copyright (c) 2015 Xamarin. All rights reserved.
//

#ifndef __btls__btls_bio__
#define __btls__btls_bio__

#include <stdio.h>
#include "btls-ssl.h"

typedef enum {
	MONO_BTLS_CONTROL_COMMAND_FLUSH	= 1
} MonoBtlsControlCommand;

typedef int (* MonoBtlsReadFunc) (const void *instance, const void *buf, int size, int *wantMore);
typedef int (* MonoBtlsWriteFunc) (const void *instance, const void *buf, int size);
typedef int64_t (* MonoBtlsControlFunc) (const void *instance, MonoBtlsControlCommand command, int64_t arg);

BIO *
mono_uxtls_bio_mono_new (void);

void
mono_uxtls_bio_mono_initialize (BIO *bio, const void *instance,
			      MonoBtlsReadFunc read_func, MonoBtlsWriteFunc write_func,
			      MonoBtlsControlFunc control_func);

int
mono_uxtls_bio_read (BIO *bio, void *data, int len);

int
mono_uxtls_bio_write (BIO *bio, const void *data, int len);

int
mono_uxtls_bio_flush (BIO *bio);

int
mono_uxtls_bio_indent (BIO *bio, unsigned indent, unsigned max_indent);

int
mono_uxtls_bio_hexdump (BIO *bio, const uint8_t *data, int len, unsigned indent);

void
mono_uxtls_bio_print_errors (BIO *bio);

void
mono_uxtls_bio_free (BIO *bio);

BIO *
mono_uxtls_bio_mem_new (void);

int
mono_uxtls_bio_mem_get_data (BIO *bio, void **data);

#endif /* defined(__btls__btls_bio__) */
