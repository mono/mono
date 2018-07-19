#!/bin/sh
(cd mcs/jay; make)
(cd msvc/scripts/; make prepare.exe; mono prepare.exe ../../mcs core)
msbuild net_4_x.sln
