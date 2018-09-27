//
//  openssl-x509-store-ctx.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/5/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include "openssl-x509-store-ctx.h"

struct MonoOpenSSLX509StoreCtx {
	int owns;
	X509_STORE_CTX *ctx;
	CRYPTO_refcount_t references;
	MonoOpenSSLX509Store *store;
	MonoOpenSSLX509Chain *chain;
};

MONO_API MonoOpenSSLX509StoreCtx *
mono_uxtls_x509_store_ctx_from_ptr (X509_STORE_CTX *ptr)
{
	MonoOpenSSLX509StoreCtx *ctx;

	ctx = OPENSSL_malloc (sizeof(MonoOpenSSLX509StoreCtx));
	if (!ctx)
		return NULL;

	memset (ctx, 0, sizeof (MonoOpenSSLX509StoreCtx));
	ctx->ctx = ptr;
	ctx->references = 1;
	return ctx;
}

MONO_API MonoOpenSSLX509StoreCtx *
mono_uxtls_x509_store_ctx_new (void)
{
	MonoOpenSSLX509StoreCtx *ctx;

	ctx = OPENSSL_malloc (sizeof(MonoOpenSSLX509StoreCtx));
	if (!ctx)
		return NULL;

	memset (ctx, 0, sizeof (MonoOpenSSLX509StoreCtx));
	ctx->ctx = X509_STORE_CTX_new ();
	ctx->references = 1;
	ctx->owns = 1;
	return ctx;
}

MONO_API MonoOpenSSLX509StoreCtx *
mono_uxtls_x509_store_ctx_up_ref (MonoOpenSSLX509StoreCtx *ctx)
{
	return ctx;
}

MONO_API int
mono_uxtls_x509_store_ctx_free (MonoOpenSSLX509StoreCtx *ctx)
{
	if (ctx->owns == 0) 
		return 0;

	if (!CRYPTO_refcount_dec_and_test_zero (&ctx->references))
		return 0;

	if (ctx->owns) {
		X509_STORE_CTX_cleanup (ctx->ctx);
		X509_STORE_CTX_free (ctx->ctx);
		ctx->owns = 0;
	}
	if (ctx->store) {
		mono_uxtls_x509_store_free (ctx->store);
		ctx->store = NULL;
	}
	if (ctx->chain) {
		mono_uxtls_x509_chain_free (ctx->chain);
		ctx->chain = NULL;
	}
	OPENSSL_free (ctx);
	return 1;
}

MONO_API int
mono_uxtls_x509_store_ctx_get_error (MonoOpenSSLX509StoreCtx *ctx, const char **error_string)
{
	int error;

	error = X509_STORE_CTX_get_error (ctx->ctx);
	if (error_string)
		*error_string = X509_verify_cert_error_string (error);
	return error;
}

MONO_API int
mono_uxtls_x509_store_ctx_get_error_depth (MonoOpenSSLX509StoreCtx *ctx)
{
	return X509_STORE_CTX_get_error_depth (ctx->ctx);
}

MONO_API MonoOpenSSLX509Chain *
mono_uxtls_x509_store_ctx_get_chain (MonoOpenSSLX509StoreCtx *ctx)
{
	STACK_OF(X509) *certs;

	certs = X509_STORE_CTX_get_chain (ctx->ctx);
	if (!certs)
		return NULL;

	return mono_uxtls_x509_chain_from_certs (certs);
}

MONO_API MonoOpenSSLX509Chain *
mono_uxtls_x509_store_ctx_get_untrusted (MonoOpenSSLX509StoreCtx *ctx)
{
	STACK_OF(X509) *untrusted;

	/*
	 * Unfortunately, there is no accessor function for this.
	 *
	 * This is the set of certificate that's passed in by
	 * X509_STORE_CTX_init() and X509_STORE_CTX_set_chain().
	 */
	untrusted = ctx->ctx->untrusted;
	if (sk_X509_num (untrusted) == 0)
		return NULL;

	return mono_uxtls_x509_chain_from_certs (untrusted);
}

MONO_API int
mono_uxtls_x509_store_ctx_init (MonoOpenSSLX509StoreCtx *ctx,
				  MonoOpenSSLX509Store *store, 
				  MonoOpenSSLX509Chain *chain)
{
	STACK_OF(X509) *certs;
	X509 *leaf;
	int ret;

	if (ctx->store)
		return 0;

	certs = mono_uxtls_x509_chain_peek_certs (chain);
	if (!certs || !sk_X509_num (certs))
		return 0;

	ctx->store = mono_uxtls_x509_store_up_ref(store);
	ctx->chain = mono_uxtls_x509_chain_up_ref(chain);

	leaf = sk_X509_value (certs, 0);
	ret = X509_STORE_CTX_init (ctx->ctx, mono_uxtls_x509_store_peek_store (store), leaf, certs);
	if (ret != 1)
		return ret;

	X509_STORE_CTX_set_app_data (ctx->ctx, ctx);
	return 1;
}

MONO_API int
mono_uxtls_x509_store_ctx_set_param (MonoOpenSSLX509StoreCtx *ctx, MonoOpenSSLX509VerifyParam *param)
{
	return X509_VERIFY_PARAM_set1 (X509_STORE_CTX_get0_param (ctx->ctx), mono_uxtls_x509_verify_param_peek_param (param));
}

MONO_API int
mono_uxtls_x509_store_ctx_verify_cert (MonoOpenSSLX509StoreCtx *ctx)
{
	return X509_verify_cert (ctx->ctx);
}

MONO_API X509 *
mono_uxtls_x509_store_ctx_get_by_subject (MonoOpenSSLX509StoreCtx *ctx, MonoOpenSSLX509Name *name)
{
	X509_OBJECT obj;
	X509 *x509;
	int ret;

	ret = X509_STORE_get_by_subject (ctx->ctx, X509_LU_X509, mono_uxtls_x509_name_peek_name (name), &obj);
	if (ret != X509_LU_X509) {
		X509_OBJECT_free_contents (&obj);
		return NULL;
	}

	x509 = X509_up_ref (obj.data.x509);
	return x509;
}

MONO_API X509 *
mono_uxtls_x509_store_ctx_get_current_cert (MonoOpenSSLX509StoreCtx *ctx)
{
	X509 *x509 = X509_STORE_CTX_get_current_cert (ctx->ctx);
	if (!x509)
		return NULL;
	return X509_up_ref (x509);
}

MONO_API X509 *
mono_uxtls_x509_store_ctx_get_current_issuer (MonoOpenSSLX509StoreCtx *ctx)
{
	X509 *x509 = X509_STORE_CTX_get0_current_issuer (ctx->ctx);
	if (!x509)
		return NULL;
	return X509_up_ref (x509);
}

MONO_API MonoOpenSSLX509VerifyParam *
mono_uxtls_x509_store_ctx_get_verify_param (MonoOpenSSLX509StoreCtx *ctx)
{
	X509_VERIFY_PARAM *param;

	param = X509_STORE_CTX_get0_param (ctx->ctx);
	if (!param)
		return NULL;

	return mono_uxtls_x509_verify_param_from_store_ctx (ctx, param);
}

MONO_API int
mono_uxtls_x509_store_ctx_get_foo (MonoOpenSSLX509StoreCtx *ctx)
{
	return 0;
}
