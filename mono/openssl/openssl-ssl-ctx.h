//
//  openssl-ssl-ctx.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 4/11/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __openssl_ssl_ctx__openssl_ssl_ctx__
#define __openssl_ssl_ctx__openssl_ssl_ctx__

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <openssl/ssl.h>
#include "openssl-util.h"

typedef struct MonoOpenSSLBio MonoOpenSSLBio;
typedef struct MonoOpenSSLX509Chain MonoOpenSSLX509Chain;
typedef struct MonoOpenSSLX509Crl MonoOpenSSLX509Crl;
typedef struct MonoOpenSSLX509Lookup MonoOpenSSLX509Lookup;
typedef struct MonoOpenSSLX509LookupMono MonoOpenSSLX509LookupMono;
typedef struct MonoOpenSSLX509Name MonoOpenSSLX509Name;
typedef struct MonoOpenSSLX509Store MonoOpenSSLX509Store;
typedef struct MonoOpenSSLX509StoreCtx MonoOpenSSLX509StoreCtx;
typedef struct MonoOpenSSLX509Revoked MonoOpenSSLX509Revoked;
typedef struct MonoOpenSSLX509VerifyParam MonoOpenSSLX509VerifyParam;
typedef struct MonoOpenSSLPkcs12 MonoOpenSSLPkcs12;
typedef struct MonoOpenSSLSsl MonoOpenSSLSsl;
typedef struct MonoOpenSSLSslCtx MonoOpenSSLSslCtx;

typedef int (* MonoOpenSSLVerifyFunc) (void *instance, int preverify_ok, X509_STORE_CTX *ctx);
typedef int (* MonoOpenSSLSelectFunc) (void *instance, int countIssuers, const int *sizes, void **issuerData);

MonoOpenSSLSslCtx *
mono_uxtls_ssl_ctx_new (void);

MonoOpenSSLSslCtx *
mono_uxtls_ssl_ctx_up_ref (MonoOpenSSLSslCtx *ctx);

int
mono_uxtls_ssl_ctx_free (MonoOpenSSLSslCtx *ctx);

void
mono_uxtls_ssl_ctx_initialize (MonoOpenSSLSslCtx *ctx, void *instance);

SSL_CTX *
mono_uxtls_ssl_ctx_get_ctx (MonoOpenSSLSslCtx *ctx);

int
mono_uxtls_ssl_ctx_debug_printf (MonoOpenSSLSslCtx *ctx, const char *format, ...);

int
mono_uxtls_ssl_ctx_is_debug_enabled (MonoOpenSSLSslCtx *ctx);

void
mono_uxtls_ssl_ctx_set_cert_verify_callback (MonoOpenSSLSslCtx *ptr, MonoOpenSSLVerifyFunc func, int cert_required);

void
mono_uxtls_ssl_ctx_set_cert_select_callback (MonoOpenSSLSslCtx *ptr, MonoOpenSSLSelectFunc func);

void
mono_uxtls_ssl_ctx_set_debug_bio (MonoOpenSSLSslCtx *ctx, BIO *debug_bio);

X509_STORE *
mono_uxtls_ssl_ctx_peek_store (MonoOpenSSLSslCtx *ctx);

void
mono_uxtls_ssl_ctx_set_min_version (MonoOpenSSLSslCtx *ctx, int version);

void
mono_uxtls_ssl_ctx_set_max_version (MonoOpenSSLSslCtx *ctx, int version);

int
mono_uxtls_ssl_ctx_is_cipher_supported (MonoOpenSSLSslCtx *ctx, uint16_t value);

int
mono_uxtls_ssl_ctx_set_ciphers (MonoOpenSSLSslCtx *ctx, int count, const uint16_t *data,
				   int allow_unsupported);

int
mono_uxtls_ssl_ctx_set_verify_param (MonoOpenSSLSslCtx *ctx, const MonoOpenSSLX509VerifyParam *param);

int
mono_uxtls_ssl_ctx_set_client_ca_list (MonoOpenSSLSslCtx *ctx, int count, int *sizes, const void **data);

#endif /* __openssl_ssl_ctx__openssl_ssl_ctx__ */
