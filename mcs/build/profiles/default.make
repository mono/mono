# -*- Makefile -*-
#
# The default profile.

# In this profile we compile everything relative to the already-installed
# runtime, so we use the bootstrap (external) compiler for everything and
# don't set MONO_PATH.

MCS = $(BOOTSTRAP_MCS)

# Causes some build errors
#PROFILE_MCS_FLAGS = /d:NET_1_1

# Exciting, no?
