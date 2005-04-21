#!/usr/bin/perl -w
#
# fixup.pl
#
# Authors:
#   Ankit Jain (ankit@corewars.org)
#
# Copyright (C) 2005 Novell, Inc (http://www.novell.com)
#
# Permission is hereby granted, free of charge, to any person obtaining
# a copy of this software and associated documentation files (the
# "Software"), to deal in the Software without restriction, including
# without limitation the rights to use, copy, modify, merge, publish,
# distribute, sublicense, and/or sell copies of the Software, and to
# permit persons to whom the Software is furnished to do so, subject to
# the following conditions:
# 
# The above copyright notice and this permission notice shall be
# included in all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
# MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
# LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
# OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
# WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#

while (<>) {
	if (/\.custom instance void\s+(?:class\s+)?Microsoft\.VisualBasic\.CompilerServices\.__DefaultArgumentValueAttribute::\.ctor\s*\(([^)]*)\)\s*=\s*\(([^)]*)\)/) { 

		my @str = split (/ /, $2);
		if ($1 =~ /string/) {
		        # FIXME: Assumes length < 0x80.
			$size = hex $str [2];
			
			if ($size == 0) {
				print ("= \"\"\n");
			}elsif ($size == 255) {
				print ("= nullref\n");
			}else{	
			        # FIXME: Should be a UTF-8 to UCS-2 translator.  However, we only use ASCII.
				print "= bytearray ( ";
				for ($i = 3; $i < @str - 2; $i ++) {
					print $str [$i] . " 00 ";
				}
				print " )\n";
			}
		}elsif ($1 =~ /bool/) {
			print "= bool (" . ($str [2] == '00' ? "false" : "true") . ")\n";
		}else {
			print "= $1 (0x";
			
			for ($i = @str - 2 - 1; $i >= 2;$i --) {
				print $str [$i] ;
			}

			print ")\n";
		}
	}else{
		print;
	}
}
