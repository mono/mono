//
//  btls-rand.h
//  MonoBtls
//
//  Created by Martin Baulig on 5/29/18.
//  Copyright © 2018 Xamarin. All rights reserved.
//

#ifndef __btls__btls_rand__
#define __btls__btls_rand__

#include <openssl/rand.h>

int
mono_btls_get_random_bytes (void *buffer, int num);

#endif /* __btls__btls_rand__ */
