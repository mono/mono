# -*- Makefile -*-
# 
# Only build .NET 1.0 classes.
#
# If we want to combine this with, say, the atomic profile,
# we should create 'atomic-net_1_0.make' which includes both.
#
# Ideally you could say 'make PROFILE="bootstrap net_1_0"' but
# that would be pretty hard to code.

include $(topdir)/build/profiles/default.make

PROFILE_MCS_FLAGS = /d:NET_1_0

# done
