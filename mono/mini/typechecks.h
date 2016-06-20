/*
 * Copyright 2016 Xamarin, Inc
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#ifndef __MINI_TYPECHECKS_H__
#define __MINI_TYPECHECKS_H__

#include "mini.h"

void mini_typechecks_emit_class_check (MonoCompile *cfg, int klass_reg, MonoClass *klass);

void mini_typechecks_save_cast_details (MonoCompile *cfg, MonoClass *klass, int obj_reg, gboolean null_check);
void mini_typechecks_reset_cast_details (MonoCompile *cfg);

MonoInst* mini_typechecks_decompose_instruction (MonoCompile *cfg, MonoInst *ins);

MonoInst* mini_typechecks_handle_ccastclass (MonoCompile *cfg, MonoClass *klass, MonoInst *src);
MonoInst* mini_typechecks_handle_cisinst (MonoCompile *cfg, MonoClass *klass, MonoInst *src);

#endif /* __MINI_TYPECHECKS_H__ */
