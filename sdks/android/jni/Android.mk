# Copyright (C) 2009 The Android Open Source Project
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#      http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#
LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_LDLIBS    := -llog
LOCAL_MODULE    := runtime-bootstrap
LOCAL_SRC_FILES := runtime-bootstrap.c

ifneq ($(MONO_BCL_TESTS),)
LOCAL_CFLAGS    += -DMONO_BCL_TESTS=1
endif

ifneq ($(MONO_DEBUGGER_TESTS),)
LOCAL_CFLAGS    += -DMONO_DEBUGGER_TESTS=1
endif

ifneq ($(RUN_WITH_MANAGED_DEBUGGER),)
LOCAL_CFLAGS    += -DRUN_WITH_MANAGED_DEBUGGER=1
endif

ifneq ($(MONO_WAIT_LLDB),)
LOCAL_CFLAGS    += -DMONO_WAIT_LLDB=1
endif

include $(BUILD_SHARED_LIBRARY)


include $(CLEAR_VARS)

LOCAL_MODULE    := lib-monosgen
LOCAL_SRC_FILES := $(TARGET_ARCH_ABI)/libmonosgen-2.0.so
include $(PREBUILT_SHARED_LIBRARY)


include $(CLEAR_VARS)

LOCAL_MODULE    := lib-suppport
LOCAL_SRC_FILES := $(TARGET_ARCH_ABI)/libMonoPosixHelper.so
include $(PREBUILT_SHARED_LIBRARY)
