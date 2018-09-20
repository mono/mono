//
//  openssl-x509-store-ctx.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/3/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_x509_store_ctx__
#define __openssl__openssl_x509_store_ctx__

#include <stdio.h>
#include "openssl-ssl.h"
#include "openssl-x509-chain.h"
#include "openssl-x509-name.h"
#include "openssl-x509-store.h"
#include "openssl-x509-verify-param.h"

MonoOpenSSLX509StoreCtx *
mono_tls_x509_store_ctx_from_ptr (X509_STORE_CTX *ptr);

MonoOpenSSLX509StoreCtx *
mono_tls_x509_store_ctx_new (void);

MonoOpenSSLX509StoreCtx *
mono_tls_x509_store_ctx_up_ref (MonoOpenSSLX509StoreCtx *ctx);

int
mono_tls_x509_store_ctx_free (MonoOpenSSLX509StoreCtx *ctx);

int
mono_tls_x509_store_ctx_get_error (MonoOpenSSLX509StoreCtx *ctx, const char **error_string);

int
mono_tls_x509_store_ctx_get_error_depth (MonoOpenSSLX509StoreCtx *ctx);

MonoOpenSSLX509Chain *
mono_tls_x509_store_ctx_get_chain (MonoOpenSSLX509StoreCtx *ctx);

X509 *
mono_tls_x509_store_ctx_get_current_cert (MonoOpenSSLX509StoreCtx *ctx);

X509 *
mono_tls_x509_store_ctx_get_current_issuer (MonoOpenSSLX509StoreCtx *ctx);

int
mono_tls_x509_store_ctx_init (MonoOpenSSLX509StoreCtx *ctx,
				   MonoOpenSSLX509Store *store, MonoOpenSSLX509Chain *chain);

int
mono_tls_x509_store_ctx_set_param (MonoOpenSSLX509StoreCtx *ctx, MonoOpenSSLX509VerifyParam *param);

X509 *
mono_tls_x509_store_ctx_get_by_subject (MonoOpenSSLX509StoreCtx *ctx, MonoOpenSSLX509Name *name);

int
mono_tls_x509_store_ctx_verify_cert (MonoOpenSSLX509StoreCtx *ctx);

MonoOpenSSLX509VerifyParam *
mono_tls_x509_store_ctx_get_verify_param (MonoOpenSSLX509StoreCtx *ctx);

MonoOpenSSLX509Chain *
mono_tls_x509_store_ctx_get_untrusted (MonoOpenSSLX509StoreCtx *ctx);

int
mono_tls_x509_store_ctx_get_foo (MonoOpenSSLX509StoreCtx *ctx);

#endif /* defined(__openssl__openssl_x509_store_ctx__) */

