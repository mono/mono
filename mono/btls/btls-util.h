//
//  btls-util.h
//  MonoBtls
//
//  Created by Martin Baulig on 3/23/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __btls__btls_util__
#define __btls__btls_util__

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <openssl/ssl.h>

#ifndef MONO_API
#if defined(_MSC_VER)

// MONO_API is not used consistently and therefore errors.
// .def file is preferred.
//#define MONO_API __declspec(dllexport)
#define MONO_API /* nothing */

#else

#ifdef __GNUC__
#define MONO_API __attribute__ ((__visibility__ ("default")))
#else
#define MONO_API
#endif

#endif
#endif

void
mono_tls_free (void *data);

int64_t
mono_tls_util_asn1_time_to_ticks (ASN1_TIME *time);

int
mono_tls_debug_printf (BIO *bio, const char *format, va_list args);

OPENSSL_EXPORT void CRYPTO_refcount_inc(CRYPTO_refcount_t *count);
OPENSSL_EXPORT int CRYPTO_refcount_dec_and_test_zero(CRYPTO_refcount_t *count);

#endif /* __btls__btls_util__ */
