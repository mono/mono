//
//  openssl-x509-chain.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/3/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include "openssl-x509-chain.h"

struct MonoOpenSSLX509Chain {
	STACK_OF(X509) *certs;
	CRYPTO_refcount_t references;
};

MONO_API MonoOpenSSLX509Chain *
mono_uxtls_x509_chain_new (void)
{
	MonoOpenSSLX509Chain *chain = (MonoOpenSSLX509Chain *)OPENSSL_malloc (sizeof (MonoOpenSSLX509Chain));
	if (chain == NULL)
		return NULL;

	memset(chain, 0, sizeof(MonoOpenSSLX509Chain));
	chain->certs = sk_X509_new_null ();
	chain->references = 1;
	return chain;
}

MONO_API MonoOpenSSLX509Chain *
mono_uxtls_x509_chain_from_certs (STACK_OF(X509) *certs)
{
	MonoOpenSSLX509Chain *chain = (MonoOpenSSLX509Chain *)OPENSSL_malloc (sizeof (MonoOpenSSLX509Chain));
	if (chain == NULL)
		return NULL;

	memset(chain, 0, sizeof(MonoOpenSSLX509Chain));
	chain->certs = X509_chain_up_ref(certs);
	chain->references = 1;
	return chain;
}

MONO_API STACK_OF(X509) *
mono_uxtls_x509_chain_peek_certs (MonoOpenSSLX509Chain *chain)
{
	return chain->certs;
}

MONO_API int
mono_uxtls_x509_chain_get_count (MonoOpenSSLX509Chain *chain)
{
	return (int)sk_X509_num(chain->certs);
}

MONO_API X509 *
mono_uxtls_x509_chain_get_cert (MonoOpenSSLX509Chain *chain, int index)
{
	X509 *cert;

	if ((ssize_t)index >= sk_X509_num(chain->certs))
		return NULL;
	cert = sk_X509_value(chain->certs, index);
	if (cert)
		X509_up_ref(cert);
	return cert;
}

MONO_API STACK_OF(X509) *
mono_uxtls_x509_chain_get_certs (MonoOpenSSLX509Chain *chain)
{
	return chain->certs;
}

MONO_API int
mono_uxtls_x509_chain_free (MonoOpenSSLX509Chain *chain)
{
	if (!CRYPTO_refcount_dec_and_test_zero(&chain->references))
		return 0;

	sk_X509_pop_free(chain->certs, X509_free);
	OPENSSL_free (chain);
	return 1;
}

MONO_API MonoOpenSSLX509Chain *
mono_uxtls_x509_chain_up_ref (MonoOpenSSLX509Chain *chain)
{
	CRYPTO_refcount_inc(&chain->references);
	return chain;
}

MONO_API void
mono_uxtls_x509_chain_add_cert (MonoOpenSSLX509Chain *chain, X509 *x509)
{
	X509_up_ref(x509);
	sk_X509_push(chain->certs, x509);
}
