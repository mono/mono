//
//  openssl-ssl.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 14/11/15.
//  Copyright (c) 2015 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_ssl__
#define __openssl__openssl_ssl__

#include "openssl-ssl-ctx.h"

MonoOpenSSLSsl *
mono_openssl_ssl_new (MonoOpenSSLSslCtx *ctx);

int
mono_openssl_ssl_use_certificate (MonoOpenSSLSsl *ptr, X509 *x509);

int
mono_openssl_ssl_use_private_key (MonoOpenSSLSsl *ptr, EVP_PKEY *key);

int
mono_openssl_ssl_add_chain_certificate (MonoOpenSSLSsl *ptr, X509 *x509);

int
mono_openssl_ssl_accept (MonoOpenSSLSsl *ptr);

int
mono_openssl_ssl_connect (MonoOpenSSLSsl *ptr);

int
mono_openssl_ssl_handshake (MonoOpenSSLSsl *ptr);

void
mono_openssl_ssl_print_errors_cb (ERR_print_errors_callback_t callback, void *ctx);

void
mono_openssl_ssl_set_bio (MonoOpenSSLSsl *ptr, BIO *bio);

int
mono_openssl_ssl_read (MonoOpenSSLSsl *ptr, void *buf, int count);

int
mono_openssl_ssl_write (MonoOpenSSLSsl *ptr, void *buf, int count);

int
mono_openssl_ssl_get_version (MonoOpenSSLSsl *ptr);

void
mono_openssl_ssl_set_min_version (MonoOpenSSLSsl *ptr, int version);

void
mono_openssl_ssl_set_max_version (MonoOpenSSLSsl *ptr, int version);

int
mono_openssl_ssl_get_cipher (MonoOpenSSLSsl *ptr);

int
mono_openssl_ssl_set_cipher_list (MonoOpenSSLSsl *ptr, const char *str);

int
mono_openssl_ssl_get_ciphers (MonoOpenSSLSsl *ptr, uint16_t **data);

X509 *
mono_openssl_ssl_get_peer_certificate (MonoOpenSSLSsl *ptr);

void
mono_openssl_ssl_close (MonoOpenSSLSsl *ptr);

int
mono_openssl_ssl_shutdown (MonoOpenSSLSsl *ptr);

MONO_API void
mono_openssl_ssl_set_quiet_shutdown (MonoOpenSSLSsl *ptr, int mode);

int
mono_openssl_ssl_get_error (MonoOpenSSLSsl *ptr, int ret_code);

int
mono_openssl_ssl_set_verify_param (MonoOpenSSLSsl *ptr, const MonoOpenSSLX509VerifyParam *param);

int
mono_openssl_ssl_set_server_name (MonoOpenSSLSsl *ptr, const char *name);

const char *
mono_openssl_ssl_get_server_name (MonoOpenSSLSsl *ptr);

typedef enum {
    MONO_OPENSSL_SSL_RENEGOTIATE_NEVER = 0,
    MONO_OPENSSL_SSL_RENEGOTIATE_ONCE,
    MONO_OPENSSL_SSL_RENEGOTIATE_FREELY,
    MONO_OPENSSL_SSL_RENEGOTIATE_IGNORE
} MonoOpenSSLSslRenegotiateMode;

void
mono_openssl_ssl_set_renegotiate_mode (MonoOpenSSLSsl *ptr, MonoOpenSSLSslRenegotiateMode mode);

int
mono_openssl_ssl_renegotiate_pending (MonoOpenSSLSsl *ptr);

void
mono_openssl_ssl_destroy (MonoOpenSSLSsl *ptr);

#endif /* defined(__openssl__openssl_ssl__) */
