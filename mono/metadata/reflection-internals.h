/* 
 * Copyright 2014 Xamarin Inc
 */
#ifndef __MONO_METADATA_REFLECTION_INTERBALS_H__
#define __MONO_METADATA_REFLECTION_INTERBALS_H__

#include <mono/metadata/reflection.h>
#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-error.h>

MonoObject*
mono_custom_attrs_get_attr_checked (MonoCustomAttrInfo *ainfo, MonoClass *attr_klass, MonoError *error);

MonoType*
mono_reflection_get_type_checked (MonoImage* image, MonoTypeNameParse *info, gboolean ignorecase, gboolean *type_resolve, MonoError *error);

#endif
