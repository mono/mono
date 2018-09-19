//
//  openssl-x509-verify-param.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/3/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_x509_verify_param__
#define __openssl__openssl_x509_verify_param__

#include <stdio.h>
#include "openssl-ssl.h"
#include "openssl-x509.h"

typedef enum {
	MONO_OPENSSL_X509_VERIFY_FLAGS_DEFAULT		= 0,
	MONO_OPENSSL_X509_VERIFY_FLAGS_CRL_CHECK	= 1,
	MONO_OPENSSL_X509_VERIFY_FLAGS_CRL_CHECK_ALL	= 2,
	MONO_OPENSSL_X509_VERIFY_FLAGS_X509_STRICT	= 4
} MonoOpenSSLX509VerifyFlags;

MonoOpenSSLX509VerifyParam *
mono_openssl_x509_verify_param_new (void);

MonoOpenSSLX509VerifyParam *
mono_openssl_x509_verify_param_from_store_ctx (MonoOpenSSLX509StoreCtx *ctx, X509_VERIFY_PARAM *param);

MonoOpenSSLX509VerifyParam *
mono_openssl_x509_verify_param_copy (const MonoOpenSSLX509VerifyParam *from);

void
mono_openssl_x509_verify_param_free (MonoOpenSSLX509VerifyParam *param);

X509_VERIFY_PARAM *
mono_openssl_x509_verify_param_peek_param (const MonoOpenSSLX509VerifyParam *param);

int
mono_openssl_x509_verify_param_can_modify (MonoOpenSSLX509VerifyParam *param);

MonoOpenSSLX509VerifyParam *
mono_openssl_x509_verify_param_lookup (const char *name);

int
mono_openssl_x509_verify_param_set_name (MonoOpenSSLX509VerifyParam *param, const char *name);

int
mono_openssl_x509_verify_param_set_host (MonoOpenSSLX509VerifyParam *param, const char *host, int namelen);

int
mono_openssl_x509_verify_param_add_host (MonoOpenSSLX509VerifyParam *param, const char *host, int namelen);

uint64_t
mono_openssl_x509_verify_param_get_flags (MonoOpenSSLX509VerifyParam *param);

int
mono_openssl_x509_verify_param_set_flags (MonoOpenSSLX509VerifyParam *param, uint64_t flags);

MonoOpenSSLX509VerifyFlags
mono_openssl_x509_verify_param_get_mono_flags (MonoOpenSSLX509VerifyParam *param);

int
mono_openssl_x509_verify_param_set_mono_flags (MonoOpenSSLX509VerifyParam *param, MonoOpenSSLX509VerifyFlags flags);

int
mono_openssl_x509_verify_param_set_purpose (MonoOpenSSLX509VerifyParam *param, MonoOpenSSLX509Purpose purpose);

int
mono_openssl_x509_verify_param_get_depth (MonoOpenSSLX509VerifyParam *param);

int
mono_openssl_x509_verify_param_set_depth (MonoOpenSSLX509VerifyParam *param, int depth);

int
mono_openssl_x509_verify_param_set_time (MonoOpenSSLX509VerifyParam *param, int64_t time);

char *
mono_openssl_x509_verify_param_get_peername (MonoOpenSSLX509VerifyParam *param);

#endif /* defined(__openssl__openssl_x509_verify_param__) */

