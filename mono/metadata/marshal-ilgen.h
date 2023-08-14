/**
 * \file
 * Copyright 2018 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_MARSHAL_ILGEN_H__
#define __MONO_MARSHAL_ILGEN_H__

MONO_API void
mono_marshal_ilgen_init (void);

gboolean
get_fixed_buffer_attr (MonoClassField *field, MonoType **out_etype, int *out_len);

#endif
