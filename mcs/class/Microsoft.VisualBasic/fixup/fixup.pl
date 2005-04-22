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

my $param = "";
my $output = "";
my $rest = "";

while (<>) {
    if (!/^\s*\.param\s*\[/) {
	print;
	next;
    }

    chomp;
    $param = $_;
    while (<>) {
	chomp;
	next if /^\s*$/;

	if (/^\s*\.param\s*\[/) {
	    print $param;
            print " = $output" if $output;
	    print "\n";
	    print "$rest\n" if $rest;
	    $output = ""; $rest = "";

	    $param = $_;
	    next;
	}
	last if !/.custom/;
	
	if (/\.custom instance void\s+(?:class\s+)?Microsoft\.VisualBasic\.CompilerServices\.__DefaultArgumentValueAttribute.*\.ctor\s*\(([^)]*)\)\s*=\s*\(\s*01\s+00\s+([^)]*)\s+00\s+00\s*\)/) {
    	    my @str = split (/ /, $2);
	    if ($1 =~ /string/) {
		# FIXME: Assumes length < 0x80.
		$size = hex (shift @str);
		$output = $size == 0 ? '""' : $size == 255 ? 'nullref' : "bytearray ( " . join (" 00 ", @str) . ' 00)';
	    } elsif ($1 =~ /bool/) {
		$output = "bool (" . ($str [0] == '00' ? "false" : "true") . ")";
	    } else {
	        @str = reverse @str;
	        $output = "$1 (0x" . join ("", @str) . ')';
	    }
	} else {
	    $rest .= "$_\n";
	}
    }

    print $param;
    print " = $output" if $output;
    print "\n";
    print "$rest\n" if $rest;
    $output = ""; $rest = "";
}
