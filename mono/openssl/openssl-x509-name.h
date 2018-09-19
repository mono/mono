//
//  openssl-x509-name.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/5/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_x509_name__
#define __openssl__openssl_x509_name__

#include <stdio.h>
#include "openssl-ssl.h"

typedef enum {
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_UNKNOWN = 0,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_COUNTRY_NAME,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_ORGANIZATION_NAME,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_ORGANIZATIONAL_UNIT_NAME,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_COMMON_NAME,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_LOCALITY_NAME,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_STATE_OR_PROVINCE_NAME,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_STREET_ADDRESS,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_SERIAL_NUMBER,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_DOMAIN_COMPONENT,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_USER_ID,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_EMAIL,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_DN_QUALIFIER,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_TITLE,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_SURNAME,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_GIVEN_NAME,
	MONO_OPENSSL_X509_NAME_ENTRY_TYPE_INITIAL
} MonoOpenSSLX509NameEntryType;

MonoOpenSSLX509Name *
mono_openssl_x509_name_from_name (X509_NAME *name);

MonoOpenSSLX509Name *
mono_openssl_x509_name_copy (X509_NAME *xn);

void
mono_openssl_x509_name_free (MonoOpenSSLX509Name *name);

X509_NAME *
mono_openssl_x509_name_peek_name (MonoOpenSSLX509Name *name);

MonoOpenSSLX509Name *
mono_openssl_x509_name_from_data (const void *data, int len, int use_canon_enc);

int
mono_openssl_x509_name_print_bio (MonoOpenSSLX509Name *name, BIO *bio);

int
mono_openssl_x509_name_print_string (MonoOpenSSLX509Name *name, char *buffer, int size);

int
mono_openssl_x509_name_get_raw_data (MonoOpenSSLX509Name *name, void **buffer, int use_canon_enc);

int64_t
mono_openssl_x509_name_hash (MonoOpenSSLX509Name *name);

int64_t
mono_openssl_x509_name_hash_old (MonoOpenSSLX509Name *name);

int
mono_openssl_x509_name_get_entry_count (MonoOpenSSLX509Name *name);

MonoOpenSSLX509NameEntryType
mono_openssl_x509_name_get_entry_type (MonoOpenSSLX509Name *name, int index);

int
mono_openssl_x509_name_get_entry_oid (MonoOpenSSLX509Name *name, int index, char *buffer, int size);

int
mono_openssl_x509_name_get_entry_oid_data (MonoOpenSSLX509Name *name, int index, const void **data);

int
mono_openssl_x509_name_get_entry_value (MonoOpenSSLX509Name *name, int index, int *tag, unsigned char **str);

#endif /* __openssl__openssl_x509_name__ */
