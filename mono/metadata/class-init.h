/**
 * \file
 * Copyright 2018 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_METADATA_CLASS_INIT_H__
#define __MONO_METADATA_CLASS_INIT_H__

#include <glib.h>
#include <mono/metadata/metadata.h>

MONO_BEGIN_DECLS

MonoClass *
mono_class_create_from_typedef (MonoImage *image, guint32 type_token, MonoError *error);

MonoClass*
mono_class_create_generic_inst (MonoGenericClass *gclass);

MonoClass *
mono_class_create_bounded_array (MonoClass *element_class, uint32_t rank, mono_bool bounded);

MonoClass *
mono_class_create_array (MonoClass *element_class, uint32_t rank);

MonoClass *
mono_class_create_ptr (MonoType *type);

MonoClass *
mono_class_create_fnptr (MonoMethodSignature *sig);


MONO_END_DECLS

#endif
