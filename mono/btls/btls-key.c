//
//  btls-key.c
//  MonoBtls
//
//  Created by Martin Baulig on 3/7/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include <btls-key.h>

void
mono_btls_key_free (EVP_PKEY *pkey)
{
	EVP_PKEY_free (pkey);
}

EVP_PKEY *
mono_btls_key_up_ref (EVP_PKEY *pkey)
{
	return EVP_PKEY_up_ref (pkey);
}

int
mono_btls_key_get_bits (EVP_PKEY *pkey)
{
	return EVP_PKEY_bits (pkey);
}

int
mono_btls_key_is_rsa (EVP_PKEY *pkey)
{
	return pkey->type == EVP_PKEY_RSA;
}

int
mono_btls_key_get_bytes (EVP_PKEY *pkey, uint8_t **buffer, int *size, int include_private_bits)
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

	if (ret != 1)
		return 0;

	*size = (int)len;
	return 1;
}

int
mono_btls_key_test (EVP_PKEY *pkey)
{
	RSA *rsa;
	unsigned char *p = NULL;
	int ret;

	if (pkey->type != EVP_PKEY_RSA)
		return 0;

	rsa = EVP_PKEY_get1_RSA (pkey);
	if (!rsa)
		return 0;

	ret = i2d_RSA_PUBKEY (rsa, &p);

	RSA_free (rsa);
	return ret;

}
