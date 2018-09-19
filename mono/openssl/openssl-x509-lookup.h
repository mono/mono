//
//  openssl-x509-lookup.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/3/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_x509_lookup__
#define __openssl__openssl_x509_lookup__

#include <stdio.h>
#include <openssl-ssl.h>
#include <openssl-x509.h>
#include <openssl-x509-store.h>

typedef enum {
	MONO_OPENSSL_X509_LOOKUP_TYPE_UNKNOWN = 0,
	MONO_OPENSSL_X509_LOOKUP_TYPE_FILE,
	MONO_OPENSSL_X509_LOOKUP_TYPE_HASH_DIR,
	MONO_OPENSSL_X509_LOOKUP_TYPE_MONO
} MonoOpenSSLX509LookupType;

MonoOpenSSLX509Lookup *
mono_openssl_x509_lookup_new (MonoOpenSSLX509Store *store, MonoOpenSSLX509LookupType type);

int
mono_openssl_x509_lookup_load_file (MonoOpenSSLX509Lookup *lookup, const char *file, MonoOpenSSLX509FileType type);

int
mono_openssl_x509_lookup_add_dir (MonoOpenSSLX509Lookup *lookup, const char *dir, MonoOpenSSLX509FileType type);

MonoOpenSSLX509Lookup *
mono_openssl_x509_lookup_up_ref (MonoOpenSSLX509Lookup *lookup);

int
mono_openssl_x509_lookup_free (MonoOpenSSLX509Lookup *lookup);

int
mono_openssl_x509_lookup_init (MonoOpenSSLX509Lookup *lookup);

MonoOpenSSLX509LookupType
mono_openssl_x509_lookup_get_type (MonoOpenSSLX509Lookup *lookup);

X509_LOOKUP *
mono_openssl_x509_lookup_peek_lookup (MonoOpenSSLX509Lookup *lookup);

int
mono_openssl_x509_lookup_shutdown (MonoOpenSSLX509Lookup *lookup);

X509 *
mono_openssl_x509_lookup_by_subject (MonoOpenSSLX509Lookup *lookup, MonoOpenSSLX509Name *name);

X509 *
mono_openssl_x509_lookup_by_fingerprint (MonoOpenSSLX509Lookup *lookup, unsigned char *bytes, int len);

#endif /* defined(__openssl__openssl_x509_lookup__) */

