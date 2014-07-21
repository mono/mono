#!/bin/sh
perl build_runtime_osx.pl --debug=1 \
	&& perl build_classlibs_osx.pl cleanbuild=0
