//
//  btls-rand.c
//  MonoBtls
//
//  Created by Martin Baulig on 5/29/18.
//  Copyright © 2018 Xamarin. All rights reserved.
//

#include "btls-rand.h"

int
mono_btls_get_random_bytes (void *buffer, int num)
{
	int ret = RAND_bytes (buffer, num);
	return ret == 1;
}
