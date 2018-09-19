//
//  openssl-x509-store.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/3/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_x509_store__
#define __openssl__openssl_x509_store__

#include <stdio.h>
#include "openssl-ssl.h"

MonoOpenSSLX509Store *
mono_openssl_x509_store_new (void);

MonoOpenSSLX509Store *
mono_openssl_x509_store_from_store (X509_STORE *ctx);

MonoOpenSSLX509Store *
mono_openssl_x509_store_from_ctx (X509_STORE_CTX *ctx);

MonoOpenSSLX509Store *
mono_openssl_x509_store_from_ssl_ctx (MonoOpenSSLSslCtx *ctx);

MonoOpenSSLX509Store *
mono_openssl_x509_store_up_ref (MonoOpenSSLX509Store *store);

int
mono_openssl_x509_store_free (MonoOpenSSLX509Store *store);

X509_STORE *
mono_openssl_x509_store_peek_store (MonoOpenSSLX509Store *store);

int
mono_openssl_x509_store_add_cert (MonoOpenSSLX509Store *store, X509 *cert);

int
mono_openssl_x509_store_load_locations (MonoOpenSSLX509Store *store, const char *file, const char *path);

int
mono_openssl_x509_store_set_default_paths (MonoOpenSSLX509Store *store);

int
mono_openssl_x509_store_get_count (MonoOpenSSLX509Store *store);

#endif /* defined(__openssl__openssl_x509_store__) */

