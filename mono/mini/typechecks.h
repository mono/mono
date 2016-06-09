/*
 * Copyright 2016 Xamarin, Inc
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#ifndef __MINI_TYPECHECKS_H__
#define __MINI_TYPECHECKS_H__

#include "mini.h"

void typechecks_mini_emit_class_check (MonoCompile *cfg, int klass_reg, MonoClass *klass);

void typechecks_save_cast_details (MonoCompile *cfg, MonoClass *klass, int obj_reg, gboolean null_check);
void typechecks_reset_cast_details (MonoCompile *cfg);

MonoInst* typechecks_decompose_instruction (MonoCompile *cfg, MonoInst *ins);

MonoInst* typechecks_handle_ccastclass (MonoCompile *cfg, MonoClass *klass, MonoInst *src);
MonoInst* typechecks_handle_cisinst (MonoCompile *cfg, MonoClass *klass, MonoInst *src);

#endif /* __MINI_TYPECHECKS_H__ */
