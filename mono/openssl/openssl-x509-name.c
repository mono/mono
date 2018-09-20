//
//  openssl-x509-name.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/5/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include "openssl-x509-name.h"

struct MonoOpenSSLX509Name {
	int owns;
	X509_NAME *name;
};

MONO_API MonoOpenSSLX509Name *
mono_tls_x509_name_from_name (X509_NAME *xn)
{
	MonoOpenSSLX509Name *name;

	name = OPENSSL_malloc (sizeof (MonoOpenSSLX509Name));
	if (!name)
		return NULL;

	memset(name, 0, sizeof(MonoOpenSSLX509Name));
	name->name = xn;
	return name;
}

MONO_API MonoOpenSSLX509Name *
mono_tls_x509_name_copy (X509_NAME *xn)
{
	MonoOpenSSLX509Name *name;

	name = OPENSSL_malloc (sizeof (MonoOpenSSLX509Name));
	if (!name)
		return NULL;

	memset(name, 0, sizeof(MonoOpenSSLX509Name));
	name->name = X509_NAME_dup(xn);
	name->owns = 1;
	return name;
}

MONO_API void
mono_tls_x509_name_free (MonoOpenSSLX509Name *name)
{
	if (name->owns) {
		if (name->name) {
			X509_NAME_free(name->name);
			name->name = NULL;
		}
	}
	OPENSSL_free(name);
}

MONO_API X509_NAME *
mono_tls_x509_name_peek_name (MonoOpenSSLX509Name *name)
{
	return name->name;
}

MONO_API int
mono_tls_x509_name_print_bio (MonoOpenSSLX509Name *name, BIO *bio)
{
	return X509_NAME_print_ex (bio, name->name, 0, ASN1_STRFLGS_RFC2253 | XN_FLAG_FN_SN | XN_FLAG_SEP_CPLUS_SPC | XN_FLAG_DN_REV);
}

MONO_API int
mono_tls_x509_name_get_raw_data (MonoOpenSSLX509Name *name, void **buffer, int use_canon_enc)
{
	int len;
	void *ptr;

	if (use_canon_enc) {
		// make sure canon_enc is initialized.
		i2d_X509_NAME (name->name, NULL);

		len = name->name->canon_enclen;
		ptr = name->name->canon_enc;
	} else {
		len = (int)name->name->bytes->length;
		ptr = name->name->bytes->data;
	}

	*buffer = OPENSSL_malloc (len);
	if (!*buffer)
		return 0;

	memcpy (*buffer, ptr, len);
	return len;
}

MONO_API MonoOpenSSLX509Name *
mono_tls_x509_name_from_data (const void *data, int len, int use_canon_enc)
{
	MonoOpenSSLX509Name *name;
	uint8_t *buf;
	const unsigned char *ptr;
	X509_NAME *ret;

	name = OPENSSL_malloc (sizeof (MonoOpenSSLX509Name));
	if (!name)
		return NULL;

	memset (name, 0, sizeof(MonoOpenSSLX509Name));
	name->owns = 1;

	name->name = X509_NAME_new ();
	if (!name->name) {
		OPENSSL_free (name);
		return NULL;
	}

	if (use_canon_enc) {
		CBB cbb, contents;
		size_t buf_len;

		// re-add ASN1 SEQUENCE header.
		CBB_init(&cbb, 0);
		if (!CBB_add_asn1(&cbb, &contents, 0x30) ||
		    !CBB_add_bytes(&contents, data, len) ||
		    !CBB_finish(&cbb, &buf, &buf_len)) {
			CBB_cleanup (&cbb);
			mono_tls_x509_name_free (name);
			return NULL;
		}

		ptr = buf;
		len = (int)buf_len;
	} else {
		ptr = data;
		buf = NULL;
	}

	ret = d2i_X509_NAME (&name->name, &ptr, len);

	if (buf)
		OPENSSL_free (buf);

	if (ret != name->name) {
		mono_tls_x509_name_free (name);
		return NULL;
	}

	return name;
}

MONO_API int
mono_tls_x509_name_print_string (MonoOpenSSLX509Name *name, char *buffer, int size)
{
	*buffer = 0;
	return X509_NAME_oneline (name->name, buffer, size) != NULL;
}

MONO_API int64_t
mono_tls_x509_name_hash (MonoOpenSSLX509Name *name)
{
	return X509_NAME_hash (name->name);
}

MONO_API int64_t
mono_tls_x509_name_hash_old (MonoOpenSSLX509Name *name)
{
	return X509_NAME_hash_old (name->name);
}

MONO_API int
mono_tls_x509_name_get_entry_count (MonoOpenSSLX509Name *name)
{
	return X509_NAME_entry_count (name->name);
}

static MonoOpenSSLX509NameEntryType
nid2mono (int nid)
{
	switch (nid) {
	case NID_countryName:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_COUNTRY_NAME;
	case NID_organizationName:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_ORGANIZATION_NAME;
	case NID_organizationalUnitName:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_ORGANIZATIONAL_UNIT_NAME;
	case NID_commonName:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_COMMON_NAME;
	case NID_localityName:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_LOCALITY_NAME;
	case NID_stateOrProvinceName:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_STATE_OR_PROVINCE_NAME;
	case NID_streetAddress:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_STREET_ADDRESS;
	case NID_serialNumber:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_SERIAL_NUMBER;
	case NID_domainComponent:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_DOMAIN_COMPONENT;
	case NID_userId:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_USER_ID;
	case NID_dnQualifier:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_DN_QUALIFIER;
	case NID_title:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_TITLE;
	case NID_surname:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_SURNAME;
	case NID_givenName:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_GIVEN_NAME;
	case NID_initials:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_INITIAL;
	default:
		return MONO_OPENSSL_X509_NAME_ENTRY_TYPE_UNKNOWN;
	}
}

MONO_API MonoOpenSSLX509NameEntryType
mono_tls_x509_name_get_entry_type (MonoOpenSSLX509Name *name, int index)
{
	X509_NAME_ENTRY *entry;
	ASN1_OBJECT *obj;

	if (index >= X509_NAME_entry_count (name->name))
		return -1;

	entry = X509_NAME_get_entry (name->name, index);
	if (!entry)
		return -1;

	obj = X509_NAME_ENTRY_get_object (entry);
	if (!obj)
		return -1;

	return nid2mono (OBJ_obj2nid (obj));
}

MONO_API int
mono_tls_x509_name_get_entry_oid (MonoOpenSSLX509Name *name, int index, char *buffer, int size)
{
	X509_NAME_ENTRY *entry;
	ASN1_OBJECT *obj;

	if (index >= X509_NAME_entry_count (name->name))
		return 0;

	entry = X509_NAME_get_entry (name->name, index);
	if (!entry)
		return 0;

	obj = X509_NAME_ENTRY_get_object (entry);
	if (!obj)
		return 0;

	return OBJ_obj2txt (buffer, size, obj, 1);
}

MONO_API int
mono_tls_x509_name_get_entry_oid_data (MonoOpenSSLX509Name *name, int index, const void **data)
{
	X509_NAME_ENTRY *entry;
	ASN1_OBJECT *obj;

	if (index >= X509_NAME_entry_count (name->name))
		return -1;

	entry = X509_NAME_get_entry (name->name, index);
	if (!entry)
		return -1;

	obj = X509_NAME_ENTRY_get_object (entry);
	if (!obj)
		return -1;

	*data = obj->data;
	return obj->length;
}

MONO_API int
mono_tls_x509_name_get_entry_value (MonoOpenSSLX509Name *name, int index, int *tag, unsigned char **str)
{
	X509_NAME_ENTRY *entry;
	ASN1_STRING *data;

	*str = NULL;
	*tag = 0;

	if (index >= X509_NAME_entry_count (name->name))
		return 0;

	entry = X509_NAME_get_entry (name->name, index);
	if (!entry)
		return 0;

	data = X509_NAME_ENTRY_get_data (entry);
	if (!data)
		return 0;

	*tag = data->type;
	return ASN1_STRING_to_UTF8 (str, data);
}
