//
//  openssl-x509-verify-param.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/5/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include "openssl-x509-verify-param.h"
#include "openssl-x509-store-ctx.h"

struct MonoOpenSSLX509VerifyParam {
	int owns;
	MonoOpenSSLX509StoreCtx *owner;
	X509_VERIFY_PARAM *param;
};

MONO_API MonoOpenSSLX509VerifyParam *
mono_openssl_x509_verify_param_new (void)
{
	MonoOpenSSLX509VerifyParam *param;

	param = OPENSSL_malloc (sizeof(MonoOpenSSLX509VerifyParam));
	if (!param)
		return NULL;
	memset (param, 0, sizeof (MonoOpenSSLX509VerifyParam));
	param->param = X509_VERIFY_PARAM_new();
	param->owns = 1;
	return param;
}

MONO_API MonoOpenSSLX509VerifyParam *
mono_openssl_x509_verify_param_from_store_ctx (MonoOpenSSLX509StoreCtx *ctx, X509_VERIFY_PARAM *param)
{
	MonoOpenSSLX509VerifyParam *instance;

	instance = OPENSSL_malloc (sizeof(MonoOpenSSLX509VerifyParam));
	if (!instance)
		return NULL;
	memset (instance, 0, sizeof (MonoOpenSSLX509VerifyParam));
	instance->param = param;
	instance->owner = mono_openssl_x509_store_ctx_up_ref (ctx);
	return instance;
}

MONO_API MonoOpenSSLX509VerifyParam *
mono_openssl_x509_verify_param_copy (const MonoOpenSSLX509VerifyParam *from)
{
	MonoOpenSSLX509VerifyParam *param;

	param = mono_openssl_x509_verify_param_new ();
	if (!param)
		return NULL;

	X509_VERIFY_PARAM_set1 (param->param, from->param);
	return param;
}

MONO_API X509_VERIFY_PARAM *
mono_openssl_x509_verify_param_peek_param (const MonoOpenSSLX509VerifyParam *param)
{
	return param->param;
}

MONO_API int
mono_openssl_x509_verify_param_can_modify (MonoOpenSSLX509VerifyParam *param)
{
	return param->owns;
}

MONO_API MonoOpenSSLX509VerifyParam *
mono_openssl_x509_verify_param_lookup (const char *name)
{
	MonoOpenSSLX509VerifyParam *param;
	const X509_VERIFY_PARAM *p;

	p = X509_VERIFY_PARAM_lookup(name);
	if (!p)
		return NULL;

	param = OPENSSL_malloc (sizeof(MonoOpenSSLX509VerifyParam));
	if (!param)
		return NULL;
	memset (param, 0, sizeof (MonoOpenSSLX509VerifyParam));
	param->param = (X509_VERIFY_PARAM *)p;
	return param;
}

MONO_API void
mono_openssl_x509_verify_param_free (MonoOpenSSLX509VerifyParam *param)
{
	if (param->owns) {
		if (param->param) {
			X509_VERIFY_PARAM_free (param->param);
			param->param = NULL;
		}
	}
	if (param->owner) {
		mono_openssl_x509_store_ctx_free (param->owner);
		param->owner = NULL;
	}
	OPENSSL_free (param);
}

MONO_API int
mono_openssl_x509_verify_param_set_name (MonoOpenSSLX509VerifyParam *param, const char *name)
{
	if (!param->owns)
		return -1;
	return X509_VERIFY_PARAM_set1_name (param->param, name);
}

MONO_API int
mono_openssl_x509_verify_param_set_host (MonoOpenSSLX509VerifyParam *param, const char *host, int namelen)
{
	if (!param->owns)
		return -1;
	return X509_VERIFY_PARAM_set1_host (param->param, host, namelen);
}

MONO_API int
mono_openssl_x509_verify_param_add_host (MonoOpenSSLX509VerifyParam *param, const char *host, int namelen)
{
	if (!param->owns)
		return -1;
	return X509_VERIFY_PARAM_set1_host (param->param, host, namelen);
}

MONO_API uint64_t
mono_openssl_x509_verify_param_get_flags (MonoOpenSSLX509VerifyParam *param)
{
	return X509_VERIFY_PARAM_get_flags (param->param);
}

MONO_API int
mono_openssl_x509_verify_param_set_flags (MonoOpenSSLX509VerifyParam *param, uint64_t flags)
{
	if (!param->owns)
		return -1;
	return X509_VERIFY_PARAM_set_flags (param->param, flags);
}

MONO_API MonoOpenSSLX509VerifyFlags
mono_openssl_x509_verify_param_get_mono_flags (MonoOpenSSLX509VerifyParam *param)
{
	MonoOpenSSLX509VerifyFlags current;
	uint64_t flags;

	if (!param->owns)
		return -1;

	current = 0;
	flags = X509_VERIFY_PARAM_get_flags (param->param);

	if (flags & X509_V_FLAG_CRL_CHECK)
		current |= MONO_OPENSSL_X509_VERIFY_FLAGS_CRL_CHECK;
	if (flags & X509_V_FLAG_CRL_CHECK_ALL)
		current |= MONO_OPENSSL_X509_VERIFY_FLAGS_CRL_CHECK_ALL;
	if (flags & X509_V_FLAG_X509_STRICT)
		current |= MONO_OPENSSL_X509_VERIFY_FLAGS_X509_STRICT;

	return current;
}

MONO_API int
mono_openssl_x509_verify_param_set_mono_flags (MonoOpenSSLX509VerifyParam *param, MonoOpenSSLX509VerifyFlags flags)
{
	uint64_t current;

	if (!param->owns)
		return -1;

	current = X509_VERIFY_PARAM_get_flags (param->param);
	if (flags & MONO_OPENSSL_X509_VERIFY_FLAGS_CRL_CHECK)
		current |= X509_V_FLAG_CRL_CHECK;
	if (flags & MONO_OPENSSL_X509_VERIFY_FLAGS_CRL_CHECK_ALL)
		current |= X509_V_FLAG_CRL_CHECK_ALL;
	if (flags & MONO_OPENSSL_X509_VERIFY_FLAGS_X509_STRICT)
		current |= X509_V_FLAG_X509_STRICT;

	return X509_VERIFY_PARAM_set_flags (param->param, current);
}

MONO_API int
mono_openssl_x509_verify_param_set_purpose (MonoOpenSSLX509VerifyParam *param, MonoOpenSSLX509Purpose purpose)
{
	if (!param->owns)
		return -1;
	return X509_VERIFY_PARAM_set_purpose (param->param, purpose);
}

MONO_API int
mono_openssl_x509_verify_param_get_depth (MonoOpenSSLX509VerifyParam *param)
{
	return X509_VERIFY_PARAM_get_depth (param->param);
}

MONO_API int
mono_openssl_x509_verify_param_set_depth (MonoOpenSSLX509VerifyParam *param, int depth)
{
	if (!param->owns)
		return -1;
	X509_VERIFY_PARAM_set_depth (param->param, depth);
	return 1;
}

MONO_API int
mono_openssl_x509_verify_param_set_time (MonoOpenSSLX509VerifyParam *param, int64_t time)
{
	if (!param->owns)
		return -1;
	X509_VERIFY_PARAM_set_time (param->param, time);
	return 1;
}

MONO_API char *
mono_openssl_x509_verify_param_get_peername (MonoOpenSSLX509VerifyParam *param)
{
	char *peer = X509_VERIFY_PARAM_get0_peername (param->param);
	return peer;
}
