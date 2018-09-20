//
//  openssl-key.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/7/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include "openssl-key.h"

MONO_API EVP_PKEY *
mono_tls_key_new ()
{
	return EVP_PKEY_new ();
}

MONO_API void
mono_tls_key_free (EVP_PKEY *pkey)
{
	EVP_PKEY_free (pkey);
}

MONO_API EVP_PKEY *
mono_tls_key_up_ref (EVP_PKEY *pkey)
{
	return(EVP_PKEY_up_ref(pkey));
}

MONO_API int
mono_tls_key_get_bits (EVP_PKEY *pkey)
{
	return EVP_PKEY_bits (pkey);
}

MONO_API int
mono_tls_key_is_rsa (EVP_PKEY *pkey)
{
	return pkey->type == EVP_PKEY_RSA;
}

MONO_API int
mono_tls_key_assign_rsa_private_key (EVP_PKEY *pkey, uint8_t *der_data, int der_length)
{
	RSA *rsa;

	rsa = RSA_private_key_from_bytes (der_data, der_length);
	if (!rsa)
		return 0;

	return EVP_PKEY_assign_RSA (pkey, rsa);
}

MONO_API int
mono_tls_key_get_bytes (EVP_PKEY *pkey, uint8_t **buffer, int *size, int include_private_bits)
{
	size_t len;
	RSA *rsa;
	int ret;

	*size = 0;
	*buffer = NULL;

	if (pkey->type != EVP_PKEY_RSA)
		return 0;

	rsa = EVP_PKEY_get1_RSA (pkey);
	if (!rsa)
		return 0;

	if (include_private_bits)
		ret = RSA_private_key_to_bytes (buffer, &len, rsa);
	else
		ret = RSA_public_key_to_bytes (buffer, &len, rsa);

	RSA_free (rsa);

	if (ret != 1)
		return 0;

	*size = (int)len;
	return 1;
}
