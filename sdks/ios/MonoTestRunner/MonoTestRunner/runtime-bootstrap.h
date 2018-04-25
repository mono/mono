//
//  runtime-bootstrap.h
//  MonoTestRunner
//
//  Created by Rodrigo Kumpera on 3/30/17.
//  Copyright © 2017 Rodrigo Kumpera. All rights reserved.
//

#ifndef runtime_bootstrap_h
#define runtime_bootstrap_h

#include <stdio.h>

void init_runtime (void);
char* runtime_send_message (const char *key, const char *value);

#endif /* runtime_bootstrap_h */
