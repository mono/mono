# -*- Makefile -*-
# 
# Only build .NET 1.0 classes.
# (will probably not work)

include $(topdir)/build/profiles/default.make

PROFILE_MCS_FLAGS = /d:NET_1_0 /d:ONLY_1_0
FRAMEWORK_VERSION = 1.0

# done
