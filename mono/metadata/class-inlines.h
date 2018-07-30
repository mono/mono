/**
 * \file
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_METADATA_CLASS_INLINES_H__
#define __MONO_METADATA_CLASS_INLINES_H__

#include <mono/metadata/class-internals.h>
#include <mono/metadata/tabledefs.h>

G_BEGIN_DECLS

static inline MonoType*
mono_get_void_type (void)
{
	return m_class_get_byval_arg (mono_defaults.void_class);
}

static inline MonoType*
mono_get_int32_type (void)
{
	return m_class_get_byval_arg (mono_defaults.int32_class);
}

static inline MonoType*
mono_get_int_type (void)
{
	return m_class_get_byval_arg (mono_defaults.int_class);
}

static inline MonoType*
mono_get_object_type (void)
{
	return m_class_get_byval_arg (mono_defaults.object_class);
}

static inline gboolean
mono_class_is_def (MonoClass *klass)
{
	return m_class_get_class_kind (klass) == MONO_CLASS_DEF;
}

static inline gboolean
mono_class_is_gtd (MonoClass *klass)
{
	return m_class_get_class_kind (klass) == MONO_CLASS_GTD;
}

static inline gboolean
mono_class_is_ginst (MonoClass *klass)
{
	return m_class_get_class_kind (klass) == MONO_CLASS_GINST;
}

static inline gboolean
mono_class_is_gparam (MonoClass *klass)
{
	return m_class_get_class_kind (klass) == MONO_CLASS_GPARAM;
}

static inline gboolean
mono_class_is_array (MonoClass *klass)
{
	return m_class_get_class_kind (klass) == MONO_CLASS_ARRAY;
}

static inline gboolean
mono_class_is_pointer (MonoClass *klass)
{
	return m_class_get_class_kind (klass) == MONO_CLASS_POINTER;
}

static inline gboolean
mono_class_is_abstract (MonoClass *klass)
{
	return mono_class_get_flags (klass) & TYPE_ATTRIBUTE_ABSTRACT;
}

static inline gboolean
mono_class_is_interface (MonoClass *klass)
{
	return mono_class_get_flags (klass) & TYPE_ATTRIBUTE_INTERFACE;
}

static inline gboolean
mono_class_is_sealed (MonoClass *klass)
{
	return mono_class_get_flags (klass) & TYPE_ATTRIBUTE_SEALED;
}

static inline gboolean
mono_class_is_before_field_init (MonoClass *klass)
{
	return mono_class_get_flags (klass) & TYPE_ATTRIBUTE_BEFORE_FIELD_INIT;
}

static inline gboolean
mono_class_is_auto_layout (MonoClass *klass)
{
	return (mono_class_get_flags (klass) & TYPE_ATTRIBUTE_LAYOUT_MASK) == TYPE_ATTRIBUTE_AUTO_LAYOUT;
}

static inline gboolean
mono_class_is_explicit_layout (MonoClass *klass)
{
	return (mono_class_get_flags (klass) & TYPE_ATTRIBUTE_LAYOUT_MASK) == TYPE_ATTRIBUTE_EXPLICIT_LAYOUT;
}

static inline gboolean
mono_class_is_public (MonoClass *klass)
{
	return mono_class_get_flags (klass) & TYPE_ATTRIBUTE_PUBLIC;
}

static inline gboolean
mono_class_has_static_metadata (MonoClass *klass)
{
	return m_class_get_type_token (klass) && !m_class_get_image (klass)->dynamic && !mono_class_is_ginst (klass);
}

G_END_DECLS

#endif
