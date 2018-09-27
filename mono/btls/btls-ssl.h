//
//  btls-ssl.h
//  MonoBtls
//
//  Created by Martin Baulig on 14/11/15.
//  Copyright (c) 2015 Xamarin. All rights reserved.
//

#ifndef __btls__btls_ssl__
#define __btls__btls_ssl__

#include "btls-ssl-ctx.h"

MonoBtlsSsl *
mono_uxtls_ssl_new (MonoBtlsSslCtx *ctx);

int
mono_uxtls_ssl_use_certificate (MonoBtlsSsl *ptr, X509 *x509);

int
mono_uxtls_ssl_use_private_key (MonoBtlsSsl *ptr, EVP_PKEY *key);

int
mono_uxtls_ssl_add_chain_certificate (MonoBtlsSsl *ptr, X509 *x509);

int
mono_uxtls_ssl_accept (MonoBtlsSsl *ptr);

int
mono_uxtls_ssl_connect (MonoBtlsSsl *ptr);

int
mono_uxtls_ssl_handshake (MonoBtlsSsl *ptr);

void
mono_uxtls_ssl_print_errors_cb (ERR_print_errors_callback_t callback, void *ctx);

void
mono_uxtls_ssl_set_bio (MonoBtlsSsl *ptr, BIO *bio);

int
mono_uxtls_ssl_read (MonoBtlsSsl *ptr, void *buf, int count);

int
mono_uxtls_ssl_write (MonoBtlsSsl *ptr, void *buf, int count);

int
mono_uxtls_ssl_get_version (MonoBtlsSsl *ptr);

void
mono_uxtls_ssl_set_min_version (MonoBtlsSsl *ptr, int version);

void
mono_uxtls_ssl_set_max_version (MonoBtlsSsl *ptr, int version);

int
mono_uxtls_ssl_get_cipher (MonoBtlsSsl *ptr);

int
mono_uxtls_ssl_set_cipher_list (MonoBtlsSsl *ptr, const char *str);

int
mono_uxtls_ssl_get_ciphers (MonoBtlsSsl *ptr, uint16_t **data);

X509 *
mono_uxtls_ssl_get_peer_certificate (MonoBtlsSsl *ptr);

void
mono_uxtls_ssl_close (MonoBtlsSsl *ptr);

int
mono_uxtls_ssl_shutdown (MonoBtlsSsl *ptr);

MONO_API void
mono_uxtls_ssl_set_quiet_shutdown (MonoBtlsSsl *ptr, int mode);

int
mono_uxtls_ssl_get_error (MonoBtlsSsl *ptr, int ret_code);

int
mono_uxtls_ssl_set_verify_param (MonoBtlsSsl *ptr, const MonoBtlsX509VerifyParam *param);

int
mono_uxtls_ssl_set_server_name (MonoBtlsSsl *ptr, const char *name);

const char *
mono_uxtls_ssl_get_server_name (MonoBtlsSsl *ptr);

typedef enum {
    MONO_BTLS_SSL_RENEGOTIATE_NEVER = 0,
    MONO_BTLS_SSL_RENEGOTIATE_ONCE,
    MONO_BTLS_SSL_RENEGOTIATE_FREELY,
    MONO_BTLS_SSL_RENEGOTIATE_IGNORE
} MonoBtlsSslRenegotiateMode;

void
mono_uxtls_ssl_set_renegotiate_mode (MonoBtlsSsl *ptr, MonoBtlsSslRenegotiateMode mode);

int
mono_uxtls_ssl_renegotiate_pending (MonoBtlsSsl *ptr);

void
mono_uxtls_ssl_destroy (MonoBtlsSsl *ptr);

#endif /* defined(__btls__btls_ssl__) */
