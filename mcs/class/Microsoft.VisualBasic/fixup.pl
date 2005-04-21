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

if ($#ARGV != 0) {
	print "Usage: fixup.pl filename.il\n";
	exit;
}

$file = $ARGV[0];
open (ORIG, $file) or die "Can't open file $file.";

while ($line = <ORIG>) {
	if ( $line =~ /(\.custom instance void)(class| .*)(Microsoft\.VisualBasic\.CompilerServices\.__DefaultArgumentValueAttribute::\.ctor)(.*)/) { 

		@type = split (/[()]/, $4);
		@str = split (/ /, $type[3]);
		if ($type[1] =~ /string/) {
			$size = hex $str [2];
			
			if ($size == 0) {
				print ("= \"\"\n");
			}elsif ($size == 255) {
				print ("= nullref\n");
			}else{	
				print " = bytearray ( ";
				for ($i = 3; $i < @str - 2; $i ++) {
					print $str [$i] . " 00 ";
				}
				print " )\n";
			}
		}elsif ($type [1] =~ /bool/) {
			print "= bool (" . (($str [2] == 01) ? "true" : "false") . ")\n";
		}else {
			print "= " . $type [1] ." (0x";
			
			for ($i = @str - 2 - 1; $i >= 2;$i --) {
				print $str [$i] ;
			}

			print ")\n";
		}
	}else{
		print $line;
	}
}

close (ORIG);
