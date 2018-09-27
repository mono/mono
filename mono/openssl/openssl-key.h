//
//  openssl-key.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/7/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_key__
#define __openssl__openssl_key__

#include <stdio.h>
#include "openssl-ssl.h"
#include "openssl-rsa.h"
#include "openssl-x509.h"

EVP_PKEY *
mono_uxtls_key_new (void);

void
mono_uxtls_key_free (EVP_PKEY *pkey);

EVP_PKEY *
mono_uxtls_key_up_ref (EVP_PKEY *pkey);

int
mono_uxtls_key_get_bits (EVP_PKEY *pkey);

int
mono_uxtls_key_is_rsa (EVP_PKEY *pkey);

int
mono_uxtls_key_assign_rsa_private_key (EVP_PKEY *pkey, uint8_t *der_data, int der_length);

int
mono_uxtls_key_get_bytes (EVP_PKEY *pkey, uint8_t **buffer, int *size, int include_private_bits);

#endif /* __openssl__openssl_key__ */

