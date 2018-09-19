//
//  openssl-ssl-ctx.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 4/11/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include "openssl-ssl-ctx.h"
#include "openssl-x509-verify-param.h"
#include <openssl/err.h>
#include <string.h>

struct MonoOpenSSLSslCtx {
	CRYPTO_refcount_t references;
	SSL_CTX *ctx;
	BIO *bio;
	BIO *debug_bio;
	void *instance;
	MonoOpenSSLVerifyFunc verify_func;
	MonoOpenSSLSelectFunc select_func;
};

#define debug_print(ptr,message) \
do { if (mono_openssl_ssl_ctx_is_debug_enabled(ptr)) \
mono_openssl_ssl_ctx_debug_printf (ptr, "%s:%d:%s(): " message, __FILE__, __LINE__, \
	__func__); } while (0)

#define debug_printf(ptr,fmt, ...) \
do { if (mono_openssl_ssl_ctx_is_debug_enabled(ptr)) \
mono_openssl_ssl_ctx_debug_printf (ptr, "%s:%d:%s(): " fmt, __FILE__, __LINE__, \
	__func__, __VA_ARGS__); } while (0)

MONO_API int
mono_openssl_ssl_ctx_is_debug_enabled (MonoOpenSSLSslCtx *ctx)
{
	return ctx->debug_bio != NULL;
}

MONO_API int
mono_openssl_ssl_ctx_debug_printf (MonoOpenSSLSslCtx *ctx, const char *format, ...)
{
	va_list args;
	int ret;

	if (!ctx->debug_bio)
		return 0;

	va_start (args, format);
	ret = mono_openssl_debug_printf (ctx->debug_bio, format, args);
	va_end (args);
	return ret;
}

MONO_API MonoOpenSSLSslCtx *
mono_openssl_ssl_ctx_new (void)
{
	MonoOpenSSLSslCtx *ctx;
	const SSL_METHOD* method = TLSv1_2_method();
	int err;

	ctx = OPENSSL_malloc (sizeof (MonoOpenSSLSslCtx));
	if (!ctx)
		return NULL;

	memset (ctx, 0, sizeof (MonoOpenSSLSslCtx));
	SSL_load_error_strings();
        ERR_load_crypto_strings();
	SSL_library_init();
	OpenSSL_add_all_ciphers();
	ctx->references = 1;
	ctx->ctx = SSL_CTX_new (method);
	if (ctx->ctx == NULL) {
		err = ERR_get_error();
		fprintf(stderr, "%s\n", ERR_error_string(err, NULL));
		abort();
	}

	// enable the default ciphers but disable any RC4 based ciphers
	// since they're insecure: RFC 7465 "Prohibiting RC4 Cipher Suites"
	SSL_CTX_set_cipher_list (ctx->ctx, "DEFAULT:!RC4");

	// disable SSLv2 and SSLv3 by default, they are deprecated
	// and should generally not be used according to the openssl docs
	SSL_CTX_set_options (ctx->ctx, SSL_OP_NO_SSLv2 | SSL_OP_NO_SSLv3);

	return ctx;
}

MONO_API MonoOpenSSLSslCtx *
mono_openssl_ssl_ctx_up_ref (MonoOpenSSLSslCtx *ctx)
{
	CRYPTO_refcount_inc (&ctx->references);
	return ctx;
}

MONO_API int
mono_openssl_ssl_ctx_free (MonoOpenSSLSslCtx *ctx)
{
	if (!CRYPTO_refcount_dec_and_test_zero (&ctx->references))
		return 0;
	SSL_CTX_free (ctx->ctx);
	ctx->instance = NULL;
	ctx->ctx = NULL;
	OPENSSL_free (ctx);
	return 1;
}

MONO_API SSL_CTX *
mono_openssl_ssl_ctx_get_ctx (MonoOpenSSLSslCtx *ctx)
{
	return ctx->ctx;
}

MONO_API void
mono_openssl_ssl_ctx_set_debug_bio (MonoOpenSSLSslCtx *ctx, BIO *debug_bio)
{
	if (debug_bio) {
		ctx->debug_bio = BIO_up_ref(debug_bio);
	} else
		ctx->debug_bio = NULL;
}

MONO_API void
mono_openssl_ssl_ctx_initialize (MonoOpenSSLSslCtx *ctx, void *instance)
{
	ctx->instance = instance;
}

static int
cert_verify_callback (X509_STORE_CTX *storeCtx, void *arg)
{
	MonoOpenSSLSslCtx *ptr = (MonoOpenSSLSslCtx*)arg;
	int ret, err;

	debug_printf (ptr, "cert_verify_callback(): %p\n", ptr->verify_func);
	ret = X509_verify_cert (storeCtx);
	if (ret != 1) {
		err = X509_STORE_CTX_get_error(storeCtx);
		if (err == X509_V_ERR_SELF_SIGNED_CERT_IN_CHAIN)
			ret = 1;
	}
	debug_printf (ptr, "cert_verify_callback() #1: %d\n", ret);

	if (ptr->verify_func)
		ret = ptr->verify_func (ptr->instance, ret, storeCtx);

	return ret;
}

MONO_API void
mono_openssl_ssl_ctx_set_cert_verify_callback (MonoOpenSSLSslCtx *ptr, MonoOpenSSLVerifyFunc func, int cert_required)
{
	int mode;

	ptr->verify_func = func;
	SSL_CTX_set_cert_verify_callback (ptr->ctx, cert_verify_callback, ptr);

	mode = SSL_VERIFY_PEER;
	if (cert_required)
		mode |= SSL_VERIFY_FAIL_IF_NO_PEER_CERT;

	SSL_CTX_set_verify (ptr->ctx, mode, NULL);
}

static int
cert_select_callback (SSL *ssl, void *arg)
{
	MonoOpenSSLSslCtx *ptr = (MonoOpenSSLSslCtx*)arg;
	STACK_OF(X509_NAME) *ca_list;
	int *sizes = NULL;
	void **cadata = NULL;
	int count = 0,
	    ret = 1,
	    i;

	debug_printf (ptr, "cert_select_callback(): %p\n", ptr->select_func);

	// SSL_get_client_CA_list() may only be called during this callback.
	ca_list = SSL_get_client_CA_list (ssl);
	if (ca_list) {
		count = sk_X509_NAME_num (ca_list);
		cadata = OPENSSL_malloc (sizeof (void *) * (count + 1));
		sizes = OPENSSL_malloc (sizeof (int) * (count + 1));
		if (!cadata || !sizes) {
			ret = 0;
			goto out;
		}
		for (i = 0; i < count; i++) {
			X509_NAME *name = sk_X509_NAME_value (ca_list, i);
			cadata[i] = name->bytes->data;
			sizes[i] = name->bytes->length;
		}
	}

	debug_printf (ptr, "cert_select_callback() #1: %p\n", ca_list);

	if (ptr->select_func)
		ret = ptr->select_func (ptr->instance, count, sizes, cadata);
	debug_printf (ptr, "cert_select_callback() #1: %d\n", ret);

out:
	if (cadata)
		OPENSSL_free (cadata);
	if (sizes)
		OPENSSL_free (sizes);

	return ret;
}

MONO_API void
mono_openssl_ssl_ctx_set_cert_select_callback (MonoOpenSSLSslCtx *ptr, MonoOpenSSLSelectFunc func)
{
	ptr->select_func = func;
	SSL_CTX_set_cert_cb (ptr->ctx, cert_select_callback, ptr);
}

MONO_API X509_STORE *
mono_openssl_ssl_ctx_peek_store (MonoOpenSSLSslCtx *ctx)
{
	return SSL_CTX_get_cert_store (ctx->ctx);
}

MONO_API void
mono_openssl_ssl_ctx_set_min_version (MonoOpenSSLSslCtx *ctx, int version)
{

}

MONO_API void
mono_openssl_ssl_ctx_set_max_version (MonoOpenSSLSslCtx *ctx, int version)
{

}

MONO_API int
mono_openssl_ssl_ctx_is_cipher_supported (MonoOpenSSLSslCtx *ctx, uint16_t value)
{
	STACK_OF(SSL_CIPHER) *sk;
	SSL_CIPHER *cipher;
	unsigned long id = 0x03000000 | value;
	int i;

	sk = ctx->ctx->cipher_list_by_id;
	for (i = 0; i < sk_SSL_CIPHER_num(sk); i++) {
		cipher = sk_SSL_CIPHER_value(sk,i);
		if (cipher->id == id)
			return(1);
	}
	return (0);
}

MONO_API int
mono_openssl_ssl_ctx_set_ciphers (MonoOpenSSLSslCtx *ctx, int count, const uint16_t *data,
				  int allow_unsupported)
{
	STACK_OF(SSL_CIPHER) *sk;
	SSL_CIPHER *cipher;
	int i, j, found;
	size_t lStr = 0;
	char *cipherList;
	unsigned long id;

	for (i = 0; i < count; i++) {
		id = 0x03000000 | data[i];
		found = 0;
		sk = ctx->ctx->cipher_list_by_id;
		for (j = 0; j < sk_SSL_CIPHER_num(sk); j++) {
			cipher = sk_SSL_CIPHER_value(sk,j);
			if (cipher->id == id) {
				found = 1;
				lStr += (strlen(cipher->name) + 1);
				break;
			}
		}
		if (!found && !allow_unsupported)
			return (0);
	}

	cipherList = malloc(lStr+1);
	if (cipherList != NULL) {
		cipherList[0] = 0;

		for (i = 0; i < count; i++) {
			id = 0x03000000 | data[i];
			sk = ctx->ctx->cipher_list_by_id;
			for (j = 0; j < sk_SSL_CIPHER_num(sk); j++) {
				cipher = sk_SSL_CIPHER_value(sk,j);
				if (cipher->id == id) {
					strcat(cipherList, cipher->name);
					strcat(cipherList, ":");
					break;
				}
			}
		}
		cipherList[lStr] = 0;

		if (SSL_CTX_set_cipher_list (ctx->ctx, cipherList) == -1)
			count = 0;

		free (cipherList);
	} else {
		count = 0;
	}

	return count;
}

MONO_API int
mono_openssl_ssl_ctx_set_verify_param (MonoOpenSSLSslCtx *ctx, const MonoOpenSSLX509VerifyParam *param)
{
	return SSL_CTX_set1_param (ctx->ctx, mono_openssl_x509_verify_param_peek_param (param));
}

MONO_API int
mono_openssl_ssl_ctx_set_client_ca_list (MonoOpenSSLSslCtx *ctx, int count, int *sizes, const void **data)
{
	STACK_OF(X509_NAME) *name_list;
	int i;

	name_list = sk_X509_NAME_new_null ();
	if (!name_list)
		return 0;

	for (i = 0; i < count; i++) {
		X509_NAME *name;
		const unsigned char *ptr = (const unsigned char*)data[i];

		name = d2i_X509_NAME (NULL, &ptr, sizes[i]);
		if (!name) {
			sk_X509_NAME_pop_free (name_list, X509_NAME_free);
			return 0;
		}
		sk_X509_NAME_push (name_list, name);
	}

	// Takes ownership of the list.
	SSL_CTX_set_client_CA_list (ctx->ctx, name_list);
	return 1;
}
