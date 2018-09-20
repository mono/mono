//
//  openssl-ssl.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 14/11/15.
//  Copyright (c) 2015 Xamarin. All rights reserved.
//

#include "openssl-ssl.h"
#include "openssl-x509-verify-param.h"
#include "openssl/err.h"

struct MonoOpenSSLSsl {
	MonoOpenSSLSslCtx *ctx;
	SSL *ssl;
};

#define debug_print(ptr,message) \
do { if (mono_tls_ssl_ctx_is_debug_enabled(ptr->ctx)) \
mono_tls_ssl_ctx_debug_printf (ptr->ctx, "%s:%d:%s(): " message, __FILE__, __LINE__, \
__func__); } while (0)

#define debug_printf(ptr,fmt, ...) \
do { if (mono_tls_ssl_ctx_is_debug_enabled(ptr->ctx)) \
mono_tls_ssl_ctx_debug_printf (ptr->ctx, "%s:%d:%s(): " fmt, __FILE__, __LINE__, \
__func__, __VA_ARGS__); } while (0)

MONO_API MonoOpenSSLSsl *
mono_tls_ssl_new (MonoOpenSSLSslCtx *ctx)
{
	MonoOpenSSLSsl *ptr;

	ptr = calloc (1, sizeof (MonoOpenSSLSsl));

	ptr->ctx = mono_tls_ssl_ctx_up_ref (ctx);
	ptr->ssl = SSL_new (mono_tls_ssl_ctx_get_ctx (ptr->ctx));

	return ptr;
}

MONO_API void
mono_tls_ssl_destroy (MonoOpenSSLSsl *ptr)
{
	mono_tls_ssl_close (ptr);
	if (ptr->ssl) {
		SSL_free (ptr->ssl);
		ptr->ssl = NULL;
	}
	if (ptr->ctx) {
		mono_tls_ssl_ctx_free (ptr->ctx);
		ptr->ctx = NULL;
	}
	free (ptr);
}

MONO_API void
mono_tls_ssl_close (MonoOpenSSLSsl *ptr)
{
	;
}

MONO_API int
mono_tls_ssl_shutdown (MonoOpenSSLSsl *ptr)
{
    return SSL_shutdown (ptr->ssl);
}

MONO_API void
mono_tls_ssl_set_quiet_shutdown (MonoOpenSSLSsl *ptr, int mode)
{
    SSL_set_quiet_shutdown (ptr->ssl, mode);
}

MONO_API void
mono_tls_ssl_set_bio (MonoOpenSSLSsl *ptr, BIO *bio)
{
	BIO_up_ref (bio);
	SSL_set_bio (ptr->ssl, bio, bio);
}

MONO_API void
mono_tls_ssl_print_errors_cb (ERR_print_errors_callback_t callback, void *ctx)
{
	ERR_print_errors_cb (callback, ctx);
}

MONO_API int
mono_tls_ssl_use_certificate (MonoOpenSSLSsl *ptr, X509 *x509)
{
	return SSL_use_certificate (ptr->ssl, x509);
}

MONO_API int
mono_tls_ssl_use_private_key (MonoOpenSSLSsl *ptr, EVP_PKEY *key)
{
	return SSL_use_PrivateKey (ptr->ssl, key);
}

MONO_API int
mono_tls_ssl_add_chain_certificate (MonoOpenSSLSsl *ptr, X509 *x509)
{
	return SSL_add1_chain_cert (ptr->ssl, x509);
}

MONO_API int
mono_tls_ssl_accept (MonoOpenSSLSsl *ptr)
{
	return SSL_accept (ptr->ssl);
}

MONO_API int
mono_tls_ssl_connect (MonoOpenSSLSsl *ptr)
{
	return SSL_connect (ptr->ssl);
}

MONO_API int
mono_tls_ssl_handshake (MonoOpenSSLSsl *ptr)
{
	return SSL_do_handshake (ptr->ssl);
}

MONO_API int
mono_tls_ssl_read (MonoOpenSSLSsl *ptr, void *buf, int count)
{
	return SSL_read (ptr->ssl, buf, count);
}

MONO_API int
mono_tls_ssl_write (MonoOpenSSLSsl *ptr, void *buf, int count)
{
	return SSL_write (ptr->ssl, buf, count);
}

MONO_API int
mono_tls_ssl_get_version (MonoOpenSSLSsl *ptr)
{
	return SSL_version (ptr->ssl);
}

MONO_API void
mono_tls_ssl_set_min_version (MonoOpenSSLSsl *ptr, int version)
{

}

MONO_API void
mono_tls_ssl_set_max_version (MonoOpenSSLSsl *ptr, int version)
{

}

MONO_API int
mono_tls_ssl_get_cipher (MonoOpenSSLSsl *ptr)
{
	const SSL_CIPHER *cipher;

	cipher = SSL_get_current_cipher (ptr->ssl);
	if (!cipher)
		return 0;
	return (uint16_t)SSL_CIPHER_get_id (cipher);
}

MONO_API int
mono_tls_ssl_set_cipher_list (MonoOpenSSLSsl *ptr, const char *str)
{
	return SSL_set_cipher_list(ptr->ssl, str);
}

MONO_API int
mono_tls_ssl_get_ciphers (MonoOpenSSLSsl *ptr, uint16_t **data)
{
	STACK_OF(SSL_CIPHER) *ciphers;
	int count, i;

	*data = NULL;

	ciphers = SSL_get_ciphers (ptr->ssl);
	if (!ciphers)
		return 0;

	count = (int)sk_SSL_CIPHER_num (ciphers);

	*data = OPENSSL_malloc (2 * count);
	if (!*data)
		return 0;

	for (i = 0; i < count; i++) {
		const SSL_CIPHER *cipher = sk_SSL_CIPHER_value (ciphers, i);
		(*data) [i] = (uint16_t) SSL_CIPHER_get_id (cipher);
	}

	return count;
}

MONO_API X509 *
mono_tls_ssl_get_peer_certificate (MonoOpenSSLSsl *ptr)
{
	return SSL_get_peer_certificate (ptr->ssl);
}

MONO_API int
mono_tls_ssl_get_error (MonoOpenSSLSsl *ptr, int ret_code)
{
	return SSL_get_error (ptr->ssl, ret_code);
}

MONO_API int
mono_tls_ssl_set_verify_param (MonoOpenSSLSsl *ptr, const MonoOpenSSLX509VerifyParam *param)
{
	return SSL_set1_param (ptr->ssl, mono_tls_x509_verify_param_peek_param (param));
}

MONO_API int
mono_tls_ssl_set_server_name (MonoOpenSSLSsl *ptr, const char *name)
{
	return SSL_set_tlsext_host_name (ptr->ssl, name);
}

MONO_API const char *
mono_tls_ssl_get_server_name (MonoOpenSSLSsl *ptr)
{
	return SSL_get_servername (ptr->ssl, TLSEXT_NAMETYPE_host_name);
}

MONO_API void
mono_tls_ssl_set_renegotiate_mode (MonoOpenSSLSsl *ptr, MonoOpenSSLSslRenegotiateMode mode)
{
	switch(mode) {
	case MONO_OPENSSL_SSL_RENEGOTIATE_NEVER :
#ifdef SSL_OP_NO_RENEGOTIATION 
		SSL_set_options(ptr->ssl, SSL_OP_NO_RENEGOTIATION);
#else
		SSL_clear_options(ptr->ssl, SSL_OP_LEGACY_SERVER_CONNECT|SSL_OP_ALLOW_UNSAFE_LEGACY_RENEGOTIATION);
#endif
		break;
	case MONO_OPENSSL_SSL_RENEGOTIATE_ONCE :
		SSL_clear_options(ptr->ssl, SSL_OP_LEGACY_SERVER_CONNECT|SSL_OP_ALLOW_UNSAFE_LEGACY_RENEGOTIATION);
		break;
	default :
		SSL_set_options(ptr->ssl, SSL_OP_LEGACY_SERVER_CONNECT);
	}
}

MONO_API int
mono_tls_ssl_renegotiate_pending (MonoOpenSSLSsl *ptr)
{
    return (SSL_in_init(ptr->ssl) && 
	    (SSL_state(ptr->ssl) & SSL_CB_HANDSHAKE_START) && 
	    !(SSL_state(ptr->ssl) & SSL_CB_HANDSHAKE_DONE));
}
