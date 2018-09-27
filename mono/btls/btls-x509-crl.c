//
//  btls-x509-crl.c
//  MonoBtls
//
//  Created by Martin Baulig on 3/23/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include "btls-x509-crl.h"
#include "btls-x509-revoked.h"

struct MonoBtlsX509Crl {
	X509_CRL *crl;
	CRYPTO_refcount_t references;
};

MONO_API MonoBtlsX509Crl *
mono_uxtls_x509_crl_from_data (const void *buf, int len, MonoBtlsX509Format format)
{
	MonoBtlsX509Crl *crl;
	BIO *bio;

	crl = OPENSSL_malloc (sizeof (MonoBtlsX509Crl));
	memset (crl, 0, sizeof(MonoBtlsX509Crl));
	crl->references = 1;

	bio = BIO_new_mem_buf ((void *)buf, len);
	switch (format) {
		case MONO_BTLS_X509_FORMAT_DER:
			crl->crl = d2i_X509_CRL_bio (bio, NULL);
			break;
		case MONO_BTLS_X509_FORMAT_PEM:
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

MONO_API MonoBtlsX509Crl *
mono_uxtls_x509_crl_ref (MonoBtlsX509Crl *crl)
{
	CRYPTO_refcount_inc (&crl->references);
	return crl;
}

MONO_API int
mono_uxtls_x509_crl_free (MonoBtlsX509Crl *crl)
{
	if (!CRYPTO_refcount_dec_and_test_zero (&crl->references))
		return 0;

	X509_CRL_free (crl->crl);
	OPENSSL_free (crl);
	return 1;
}

MONO_API MonoBtlsX509Revoked *
mono_uxtls_x509_crl_get_by_cert (MonoBtlsX509Crl *crl, X509 *x509)
{
	X509_REVOKED *revoked;
	int ret;

	revoked = NULL;
	ret = X509_CRL_get0_by_cert (crl->crl, &revoked, x509);
	fprintf (stderr, "mono_uxtls_x509_crl_get_by_cert: %d - %p\n", ret, revoked);

	if (!ret || !revoked)
		return NULL;

	return mono_uxtls_x509_revoked_new (crl, revoked);
}

MONO_API MonoBtlsX509Revoked *
mono_uxtls_x509_crl_get_by_serial (MonoBtlsX509Crl *crl, void *serial, int len)
{
	ASN1_INTEGER si;
	X509_REVOKED *revoked;
	int ret;

	si.type = V_ASN1_INTEGER;
	si.length = len;
	si.data = serial;

	revoked = NULL;
	ret = X509_CRL_get0_by_serial (crl->crl, &revoked, &si);
	fprintf (stderr, "mono_uxtls_x509_crl_get_by_serial: %d - %p\n", ret, revoked);

	if (!ret || !revoked)
		return NULL;

	return mono_uxtls_x509_revoked_new (crl, revoked);
}

MONO_API int
mono_uxtls_x509_crl_get_revoked_count (MonoBtlsX509Crl *crl)
{
	STACK_OF(X509_REVOKED) *stack;

	stack = X509_CRL_get_REVOKED (crl->crl);
	return (int)sk_X509_REVOKED_num (stack);
}

MONO_API MonoBtlsX509Revoked *
mono_uxtls_x509_crl_get_revoked (MonoBtlsX509Crl *crl, int index)
{
	STACK_OF(X509_REVOKED) *stack;
	X509_REVOKED *revoked;

	stack = X509_CRL_get_REVOKED (crl->crl);
	if ((size_t)index >= sk_X509_REVOKED_num (stack))
		return NULL;

	revoked = sk_X509_REVOKED_value (stack, index);
	if (!revoked)
		return NULL;

	return mono_uxtls_x509_revoked_new (crl, revoked);
}

MONO_API int64_t
mono_uxtls_x509_crl_get_last_update (MonoBtlsX509Crl *crl)
{
	return mono_uxtls_util_asn1_time_to_ticks (X509_CRL_get_lastUpdate (crl->crl));
}

MONO_API int64_t
mono_uxtls_x509_crl_get_next_update (MonoBtlsX509Crl *crl)
{
	return mono_uxtls_util_asn1_time_to_ticks (X509_CRL_get_nextUpdate (crl->crl));
}

MONO_API int64_t
mono_uxtls_x509_crl_get_version (MonoBtlsX509Crl *crl)
{
	return X509_CRL_get_version (crl->crl);
}

MONO_API MonoBtlsX509Name *
mono_uxtls_x509_crl_get_issuer (MonoBtlsX509Crl *crl)
{
	return mono_uxtls_x509_name_copy (X509_CRL_get_issuer (crl->crl));
}

