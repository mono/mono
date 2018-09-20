//
//  btls-key.h
//  MonoBtls
//
//  Created by Martin Baulig on 3/7/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __btls__btls_key__
#define __btls__btls_key__

#include <stdio.h>
#include "btls-ssl.h"
#include "btls-x509.h"

EVP_PKEY *
mono_tls_key_new ();

void
mono_tls_key_free (EVP_PKEY *pkey);

EVP_PKEY *
mono_tls_key_up_ref (EVP_PKEY *pkey);

int
mono_tls_key_get_bits (EVP_PKEY *pkey);

int
mono_tls_key_is_rsa (EVP_PKEY *pkey);

int
mono_tls_key_assign_rsa_private_key (EVP_PKEY *pkey, uint8_t *der_data, int der_length);

int
mono_tls_key_get_bytes (EVP_PKEY *pkey, uint8_t **buffer, int *size, int include_private_bits);

#endif /* __btls__btls_key__ */

