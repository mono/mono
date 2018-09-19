//
//  openssl-x509-revoked.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/23/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include "openssl-x509-revoked.h"

struct MonoOpenSSLX509Revoked {
	MonoOpenSSLX509Crl *owner;
	X509_REVOKED *revoked;
};

MONO_API MonoOpenSSLX509Revoked *
mono_openssl_x509_revoked_new (MonoOpenSSLX509Crl *owner, X509_REVOKED *revoked)
{
	MonoOpenSSLX509Revoked *instance;

	instance = OPENSSL_malloc (sizeof (MonoOpenSSLX509Revoked));
	memset (instance, 0, sizeof (MonoOpenSSLX509Revoked));

	instance->owner = mono_openssl_x509_crl_ref (owner);
	instance->revoked = revoked;
	return instance;
}

MONO_API void
mono_openssl_x509_revoked_free (MonoOpenSSLX509Revoked *revoked)
{
	mono_openssl_x509_crl_free (revoked->owner);
	OPENSSL_free (revoked);
}

MONO_API int
mono_openssl_x509_revoked_get_serial_number (MonoOpenSSLX509Revoked *revoked, char *buffer, int size)
{
	ASN1_INTEGER *serial;

	serial = revoked->revoked->serialNumber;
	if (serial->length == 0 || serial->length+1 > size)
		return 0;

	memcpy (buffer, serial->data, serial->length);
	return serial->length;
}

MONO_API int64_t
mono_openssl_x509_revoked_get_revocation_date (MonoOpenSSLX509Revoked *revoked)
{
	ASN1_TIME *date;

	date = revoked->revoked->revocationDate;
	if (!date)
		return 0;

	return mono_openssl_util_asn1_time_to_ticks (date);
}

MONO_API int
mono_openssl_x509_revoked_get_reason (MonoOpenSSLX509Revoked *revoked)
{
	return revoked->revoked->reason;
}

MONO_API int
mono_openssl_x509_revoked_get_sequence (MonoOpenSSLX509Revoked *revoked)
{
	return revoked->revoked->sequence;
}

