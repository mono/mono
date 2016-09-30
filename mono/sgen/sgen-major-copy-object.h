/*
 * sgen-major-copy-object.h: Object copying in the major collectors.
 *
 * Copyright 2001-2003 Ximian, Inc
 * Copyright 2003-2010 Novell, Inc.
 * Copyright (C) 2012 Xamarin Inc
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#define COLLECTOR_SERIAL_ALLOC_FOR_PROMOTION sgen_minor_collector.alloc_for_promotion

#include "sgen-copy-object.h"
