# Makefile
#
#	Copyright (C) 2002 The Npgsql Development Team
#	npgsql-general@gborg.postgresql.org
#	http://gborg.postgresql.org/project/npgsql/projdisplay.php
#
#
# This library is free software; you can redistribute it and/or
# modify it under the terms of the GNU Lesser General Public
# License as published by the Free Software Foundation; either
# version 2.1 of the License, or (at your option) any later version.
# 
# This library is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# Lesser General Public License for more details.
# 
# You should have received a copy of the GNU Lesser General Public
# License along with this library; if not, write to the Free Software
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

NPGSQL_LIB=Npgsql.dll
# Allow compiling under Cygwin
ifeq ($(OS),"Windows_NT")
	CC=csc.exe
	LDFLAGS=/r:System.Data.dll /target:library /out:$(NPGSQL_LIB)
else
	CC=mcs
	LDFLAGS=-r /usr/lib/System.Data.dll --target library -o $(NPGSQL_LIB)
endif
CVS2CL=cvs2cl.pl
OBJECTS:=$(shell ls *.cs)

all: $(OBJECTS)
	$(CC) $(LDFLAGS) $(OBJECTS)

clean:
	rm -rf $(NPGSQL_LIB) *~

changelog:
	$(CVS2CL)
