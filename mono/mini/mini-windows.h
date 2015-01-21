#ifndef __MONO_MINI_WINDOWS_H__
#define __MONO_MINI_WINDOWS_H__

#include "mini.h"

gboolean win32_stack_overflow_walk (StackFrameInfo *frame, MonoContext *ctx, gpointer data) MONO_INTERNAL;

#endif
