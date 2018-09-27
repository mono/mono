//
//  openssl-x509-lookup.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/6/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include <openssl-x509-lookup.h>
#include <openssl-x509-lookup-mono.h>

struct MonoOpenSSLX509Lookup {
	MonoOpenSSLX509LookupType type;
	X509_LOOKUP *lookup;
	int owns_lookup;
	MonoOpenSSLX509Store *store;
	CRYPTO_refcount_t references;
};

static X509_LOOKUP_METHOD *
get_lookup_method (MonoOpenSSLX509LookupType type)
{
	switch (type) {
	case MONO_OPENSSL_X509_LOOKUP_TYPE_FILE:
		return X509_LOOKUP_file ();
	case MONO_OPENSSL_X509_LOOKUP_TYPE_HASH_DIR:
		return X509_LOOKUP_hash_dir ();
	case MONO_OPENSSL_X509_LOOKUP_TYPE_MONO:
		return mono_uxtls_x509_lookup_mono_method ();
	default:
		return NULL;
	}
}

MONO_API MonoOpenSSLX509Lookup *
mono_uxtls_x509_lookup_new (MonoOpenSSLX509Store *store, MonoOpenSSLX509LookupType type)
{
	MonoOpenSSLX509Lookup *lookup;
	X509_LOOKUP *store_lookup;
	X509_LOOKUP_METHOD *method;

	method = get_lookup_method (type);
	if (!method)
		return NULL;

	lookup = OPENSSL_malloc (sizeof(MonoOpenSSLX509Lookup));
	if (!lookup)
		return NULL;

	store_lookup = X509_STORE_add_lookup (mono_uxtls_x509_store_peek_store (store), method);
	if (!store_lookup) {
		OPENSSL_free (lookup);
		return NULL;
	}

	memset (lookup, 0, sizeof(MonoOpenSSLX509Lookup));
	// The X509_STORE owns the X509_LOOKUP.
	lookup->store = mono_uxtls_x509_store_up_ref (store);
	lookup->lookup = store_lookup;
	lookup->owns_lookup = 0;
	lookup->references = 1;
	lookup->type = type;
	return lookup;
}

MONO_API int
mono_uxtls_x509_lookup_load_file (MonoOpenSSLX509Lookup *lookup, const char *file, MonoOpenSSLX509FileType type)
{
	return X509_LOOKUP_load_file (lookup->lookup, file, type);
}

MONO_API int
mono_uxtls_x509_lookup_add_dir (MonoOpenSSLX509Lookup *lookup, const char *dir, MonoOpenSSLX509FileType type)
{
	return X509_LOOKUP_add_dir (lookup->lookup, dir, type);
}

MONO_API MonoOpenSSLX509Lookup *
mono_uxtls_x509_lookup_up_ref (MonoOpenSSLX509Lookup *lookup)
{
	CRYPTO_refcount_inc (&lookup->references);
	return lookup;
}

MONO_API int
mono_uxtls_x509_lookup_free (MonoOpenSSLX509Lookup *lookup)
{
	if (!CRYPTO_refcount_dec_and_test_zero (&lookup->references))
		return 0;

	if (lookup->store) {
		mono_uxtls_x509_store_free (lookup->store);
		lookup->store = NULL;
	}

	if (lookup->lookup) {
		if (lookup->owns_lookup)
			X509_LOOKUP_free (lookup->lookup);
		lookup->lookup = NULL;
	}

	OPENSSL_free (lookup);
	return 1;
}

MONO_API int
mono_uxtls_x509_lookup_init (MonoOpenSSLX509Lookup *lookup)
{
	return X509_LOOKUP_init (lookup->lookup);
}

MONO_API int
mono_uxtls_x509_lookup_shutdown (MonoOpenSSLX509Lookup *lookup)
{
	return X509_LOOKUP_shutdown (lookup->lookup);
}

MONO_API MonoOpenSSLX509LookupType
mono_uxtls_x509_lookup_get_type (MonoOpenSSLX509Lookup *lookup)
{
	return lookup->type;
}

MONO_API X509_LOOKUP *
mono_uxtls_x509_lookup_peek_lookup (MonoOpenSSLX509Lookup *lookup)
{
	return lookup->lookup;
}

MONO_API X509 *
mono_uxtls_x509_lookup_by_subject (MonoOpenSSLX509Lookup *lookup, MonoOpenSSLX509Name *name)
{
	X509_OBJECT obj;
	X509 *x509;
	int ret;

	ret = X509_LOOKUP_by_subject (lookup->lookup, X509_LU_X509, mono_uxtls_x509_name_peek_name (name), &obj);
	if (ret != X509_LU_X509) {
		X509_OBJECT_free_contents (&obj);
		return NULL;
	}

	x509 = X509_up_ref (obj.data.x509);
	return x509;
}

MONO_API X509 *
mono_uxtls_x509_lookup_by_fingerprint (MonoOpenSSLX509Lookup *lookup, unsigned char *bytes, int len)
{
	X509_OBJECT obj;
	X509 *x509;
	int ret;

	ret = X509_LOOKUP_by_fingerprint (lookup->lookup, X509_LU_X509, bytes, len, &obj);
	if (ret != X509_LU_X509) {
		X509_OBJECT_free_contents (&obj);
		return NULL;
	}

	x509 = X509_up_ref (obj.data.x509);
	return x509;
}
