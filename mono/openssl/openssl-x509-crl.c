//
//  openssl-x509-crl.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/23/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include "openssl-x509-crl.h"
#include "openssl-x509-revoked.h"

struct MonoOpenSSLX509Crl {
	X509_CRL *crl;
	CRYPTO_refcount_t references;
};

MONO_API MonoOpenSSLX509Crl *
mono_tls_x509_crl_from_data (const void *buf, int len, MonoOpenSSLX509Format format)
{
	MonoOpenSSLX509Crl *crl;
	BIO *bio;

	crl = OPENSSL_malloc (sizeof (MonoOpenSSLX509Crl));
	memset (crl, 0, sizeof(MonoOpenSSLX509Crl));
	crl->references = 1;

	bio = BIO_new_mem_buf ((void *)buf, len);
	switch (format) {
		case MONO_OPENSSL_X509_FORMAT_DER:
			crl->crl = d2i_X509_CRL_bio (bio, NULL);
			break;
		case MONO_OPENSSL_X509_FORMAT_PEM:
			crl->crl = PEM_read_bio_X509_CRL (bio, NULL, NULL, NULL);
			break;
	}
	BIO_free (bio);

	if (!crl->crl) {
		OPENSSL_free (crl);
		return NULL;
	}

	return crl;
}

MONO_API MonoOpenSSLX509Crl *
mono_tls_x509_crl_ref (MonoOpenSSLX509Crl *crl)
{
	CRYPTO_refcount_inc (&crl->references);
	return crl;
}

MONO_API int
mono_tls_x509_crl_free (MonoOpenSSLX509Crl *crl)
{
	if (!CRYPTO_refcount_dec_and_test_zero (&crl->references))
		return 0;

	X509_CRL_free (crl->crl);
	OPENSSL_free (crl);
	return 1;
}

MONO_API MonoOpenSSLX509Revoked *
mono_tls_x509_crl_get_by_cert (MonoOpenSSLX509Crl *crl, X509 *x509)
{
	X509_REVOKED *revoked;
	int ret;

	revoked = NULL;
	ret = X509_CRL_get0_by_cert (crl->crl, &revoked, x509);
	fprintf (stderr, "mono_tls_x509_crl_get_by_cert: %d - %p\n", ret, revoked);

	if (!ret || !revoked)
		return NULL;

	return mono_tls_x509_revoked_new (crl, revoked);
}

MONO_API MonoOpenSSLX509Revoked *
mono_tls_x509_crl_get_by_serial (MonoOpenSSLX509Crl *crl, void *serial, int len)
{
	ASN1_INTEGER si;
	X509_REVOKED *revoked;
	int ret;

	si.type = V_ASN1_INTEGER;
	si.length = len;
	si.data = serial;

	revoked = NULL;
	ret = X509_CRL_get0_by_serial (crl->crl, &revoked, &si);
	fprintf (stderr, "mono_tls_x509_crl_get_by_serial: %d - %p\n", ret, revoked);

	if (!ret || !revoked)
		return NULL;

	return mono_tls_x509_revoked_new (crl, revoked);
}

MONO_API int
mono_tls_x509_crl_get_revoked_count (MonoOpenSSLX509Crl *crl)
{
	STACK_OF(X509_REVOKED) *stack;

	stack = X509_CRL_get_REVOKED (crl->crl);
	return (int)sk_X509_REVOKED_num (stack);
}

MONO_API MonoOpenSSLX509Revoked *
mono_tls_x509_crl_get_revoked (MonoOpenSSLX509Crl *crl, int index)
{
	STACK_OF(X509_REVOKED) *stack;
	X509_REVOKED *revoked;

	stack = X509_CRL_get_REVOKED (crl->crl);
	if ((ssize_t)index >= sk_X509_REVOKED_num (stack))
		return NULL;

	revoked = sk_X509_REVOKED_value (stack, index);
	if (!revoked)
		return NULL;

	return mono_tls_x509_revoked_new (crl, revoked);
}

MONO_API int64_t
mono_tls_x509_crl_get_last_update (MonoOpenSSLX509Crl *crl)
{
	return mono_tls_util_asn1_time_to_ticks (X509_CRL_get_lastUpdate (crl->crl));
}

MONO_API int64_t
mono_tls_x509_crl_get_next_update (MonoOpenSSLX509Crl *crl)
{
	return mono_tls_util_asn1_time_to_ticks (X509_CRL_get_nextUpdate (crl->crl));
}

MONO_API int64_t
mono_tls_x509_crl_get_version (MonoOpenSSLX509Crl *crl)
{
	return X509_CRL_get_version (crl->crl);
}

MONO_API MonoOpenSSLX509Name *
mono_tls_x509_crl_get_issuer (MonoOpenSSLX509Crl *crl)
{
	return mono_tls_x509_name_copy (X509_CRL_get_issuer (crl->crl));
}

