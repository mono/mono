//
//  openssl-pkcs12.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/8/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_pkcs12__
#define __openssl__openssl_pkcs12__

#include <stdio.h>
#include "openssl-ssl.h"
#include "openssl-x509.h"

MonoOpenSSLPkcs12 *
mono_uxtls_pkcs12_new (void);

int
mono_uxtls_pkcs12_get_count (MonoOpenSSLPkcs12 *pkcs12);

X509 *
mono_uxtls_pkcs12_get_cert (MonoOpenSSLPkcs12 *pkcs12, int index);

STACK_OF(X509) *
mono_uxtls_pkcs12_get_certs (MonoOpenSSLPkcs12 *pkcs12);

int
mono_uxtls_pkcs12_free (MonoOpenSSLPkcs12 *pkcs12);

MonoOpenSSLPkcs12 *
mono_uxtls_pkcs12_up_ref (MonoOpenSSLPkcs12 *pkcs12);

void
mono_uxtls_pkcs12_add_cert (MonoOpenSSLPkcs12 *pkcs12, X509 *x509);

int
mono_uxtls_pkcs12_import (MonoOpenSSLPkcs12 *pkcs12, const void *data, int len, const void *password);

int
mono_uxtls_pkcs12_has_private_key (MonoOpenSSLPkcs12 *pkcs12);

EVP_PKEY *
mono_uxtls_pkcs12_get_private_key (MonoOpenSSLPkcs12 *pkcs12);

#endif /* __openssl__openssl_pkcs12__ */
