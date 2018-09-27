//
//  openssl-x509-lookup-mono.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/3/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_x509_lookup_mono__
#define __openssl__openssl_x509_lookup_mono__

#include <stdio.h>
#include "openssl-ssl.h"
#include "openssl-x509.h"
#include "openssl-x509-store.h"

typedef int (* MonoOpenSSLX509LookupMono_BySubject) (const void *instance, MonoOpenSSLX509Name *name, X509 **ret);

MonoOpenSSLX509LookupMono *
mono_uxtls_x509_lookup_mono_new (void);

int
mono_uxtls_x509_lookup_mono_free (MonoOpenSSLX509LookupMono *mono);

void
mono_uxtls_x509_lookup_mono_init (MonoOpenSSLX509LookupMono *mono, const void *instance,
				 MonoOpenSSLX509LookupMono_BySubject by_subject_func);

int
mono_uxtls_x509_lookup_add_mono (MonoOpenSSLX509Lookup *lookup, MonoOpenSSLX509LookupMono *mono);

X509_LOOKUP_METHOD *
mono_uxtls_x509_lookup_mono_method (void);

#endif /* defined(__openssl__openssl_x509_lookup_mono__) */

