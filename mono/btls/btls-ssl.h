//
//  btls-ssl.h
//  MonoBtls
//
//  Created by Martin Baulig on 14/11/15.
//  Copyright (c) 2015 Xamarin. All rights reserved.
//

#ifndef __btls__btls_ssl__
#define __btls__btls_ssl__

#include <btls-ssl-ctx.h>

MonoBtlsSsl *
mono_btls_ssl_new (MonoBtlsSslCtx *ctx);

int
mono_btls_ssl_use_certificate (MonoBtlsSsl *ptr, X509 *x509);

int
mono_btls_ssl_use_private_key (MonoBtlsSsl *ptr, EVP_PKEY *key);

int
mono_btls_ssl_add_chain_certificate (MonoBtlsSsl *ptr, X509 *x509);

int
mono_btls_ssl_accept (MonoBtlsSsl *ptr);

int
mono_btls_ssl_connect (MonoBtlsSsl *ptr);

int
mono_btls_ssl_handshake (MonoBtlsSsl *ptr);

void
mono_btls_ssl_print_errors_cb (ERR_print_errors_callback_t callback, void *ctx);

void
mono_btls_ssl_set_bio (MonoBtlsSsl *ptr, BIO *bio);

int
mono_btls_ssl_read (MonoBtlsSsl *ptr, void *buf, int count);

int
mono_btls_ssl_write (MonoBtlsSsl *ptr, void *buf, int count);

int
mono_btls_ssl_get_version (MonoBtlsSsl *ptr);

void
mono_btls_ssl_set_min_version (MonoBtlsSsl *ptr, int version);

void
mono_btls_ssl_set_max_version (MonoBtlsSsl *ptr, int version);

int
mono_btls_ssl_get_cipher (MonoBtlsSsl *ptr);

int
mono_btls_ssl_set_cipher_list (MonoBtlsSsl *ptr, const char *str);

int
mono_btls_ssl_get_ciphers (MonoBtlsSsl *ptr, uint16_t **data);

X509 *
mono_btls_ssl_get_peer_certificate (MonoBtlsSsl *ptr);

void
mono_btls_ssl_close (MonoBtlsSsl *ptr);

int
mono_btls_ssl_get_error (MonoBtlsSsl *ptr, int ret_code);

int
mono_btls_ssl_set_verify_param (MonoBtlsSsl *ptr, const MonoBtlsX509VerifyParam *param);

void
mono_btls_ssl_destroy (MonoBtlsSsl *ptr);

void
mono_btls_ssl_test (MonoBtlsSsl *ptr);

#endif /* defined(__btls__btls_ssl__) */
