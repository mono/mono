#!/usr/bin/perl
#
# xpidl2cs.pl : Generates C# interfaces from idl
#
# Author: Andreia Gaita <shana.ufie@gmail.com>
#
# Copyright (c) 2007 Novell, Inc.
#
# This program is free software; you can redistribute it and/or
# modify it under the terms of version 2 of the GNU General Public
# License as published by the Free Software Foundation.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# General Public License for more details.
#
# You should have received a copy of the GNU General Public
# License along with this program; if not, write to the
# Free Software Foundation, Inc., 59 Temple Place - Suite 330,
# Boston, MA 02111-1307, USA.
##############################################################

die "Usage: xpidl2cs.pl file.idl [/path/to/idl/]" if length @ARGV == 0;

my $file = shift;
my $path = shift if length @ARGV > 0;


open FILE, '<', $path.$file or die "Can't open file $path$file";

my %interface = (
		 properties => (), 
		 items => "", 
		 uuid => "", 
		 class => "", 
		 parent => ""
		 );
my %properties;
my %methods;

my %types;
$types{"short"} = {name => "short", out => "out", marshal => ""};
$types{"unsigned,short"} = {name => "ushort", out => "out", marshal => ""};
$types{"int"} = {name => "int", out => "out", marshal => ""};
$types{"nsresult"} = {name => "int", out => "out", marshal => ""};
$types{"unsigned,int"} = {name => "uint", out => "out", marshal => ""};
$types{"PRUint32"} = {name => "UInt32", out => "out", marshal => ""};
$types{"long"} = {name => "int", out => "out", marshal => ""};
$types{"unsigned,long"} = {name => "uint", out => "out", marshal => ""};
$types{"float"} = {name => "float", out => "out", marshal => ""};
$types{"boolean"} = {name => "bool", out => "out", marshal => ""};
$types{"PRBool"} = {name => "bool", out => "out", marshal => ""};
$types{"void"} = {name => "", out => "", marshal => ""};
$types{"DOMString"} = {name => "HandleRef", out => "", marshal => ""};
$types{"nsCIDRef"} = {name => "Guid", out => "out", marshal => "[MarshalAs (UnmanagedType.LPStruct)] "};
$types{"nsIIDRef"} = {name => "Guid", out => "out", marshal => "[MarshalAs (UnmanagedType.LPStruct)] "};
$types{"Guid"} = {name => "Guid", out => "out", marshal => "[MarshalAs (UnmanagedType.LPStruct)] "};
$types{"string"} = {name => "string", out => "", marshal => "[MarshalAs (UnmanagedType.LPStr)] "};
$types{"charPtr"} = {name => "StringBuilder", out => "", marshal => ""};
$types{"voidPtr"} = {name => "IntPtr", out => "", marshal => ""};
$types{"others"} = {name => "", out => "out", marshal => "[MarshalAs (UnmanagedType.Interface)] "};


my %dependents;

sub parse_parent {
    my $x = shift;

    `perl xpidl2cs.pl $x.idl $path`;

    open my $f, '<', "$x.cs";
    my $start = 0;
    my $out;
    while (my $line = <$f>) {
	chop $line;
	if (!$start) {
	    if ($line =~ /#region/) {
		$start = 1;
		$out .= $line . "\n";
	    }
	}
	elsif ($line =~ /\}/) {
	    last;
	}
	else {
	    $out .= $line . "\n";
	}
    }

    return $out;
}

sub has_setter {
    my $x = shift;
    return !$properties{$x}->{"setter"};
}

sub get_type {
    my $x = shift;
    if (exists $types{$x}) {
	return $types{$x}->{"name"};
    }
    return $x;
}

sub get_out {
    my $x = shift;
    if (exists $types{$x}) {
	return $types{$x}->{"out"};
    }
    return $types{"others"}->{"out"};
}

sub get_marshal {
    my $x = shift;
    if (exists $types{$x}) {
	return $types{$x}->{"marshal"};
    }
    return $types{"others"}->{"marshal"};
}

sub is_property {
    my $x = shift;
    return (exists $properties{$x});
}

sub add_external {
    my $x = shift;
    if ($x !~ /nsISupports/ && !exists $types{$x} && !exists $dependents{$x}) {
	$dependents{$x} = $x;
    }
#    print "add_external $x\n";
}

sub get_params {
    my $x = shift;
    my $ret = '';
    my %list;
    my @params = split /,/, $methods{$x}->{"params"};

#    print "params:@params:\n";
    for my $param (@params) {
	my $marshal;
	my $name;
	my $type;
	my $out;
	my $isout;


#	print "param:$param:\n";
	my @p = split (" ", $param);

# need to backtrack to a previous parameter defined by iid_is(name) and 
# replace the type of this one with that. who the $%#@ came up with this idea? le sigh.

	if (@p[0] =~ m/iid_is/) {
	    shift @p;
	    $name = @p[0];
	    $name =~ s/ //;
	    $type = $list{$name}->{"type"};
	    $marshal = $list{$name}->{"marshal"};
	    shift @p unless @p == 3;
	}

	shift @p unless @p[0] =~ /(in|out)/;

	if (@p[0] =~ m/out/) {
	    $isout = 1;
	}
	shift @p;

	if (!$type) {
	    $type = join ",", @p[0..@p-2];
	    $type=~s/\[.*\],//;
	}

	$out = &get_out($type) if $isout;



	$type = &get_type (@p[0]) unless $type;
	$marshal = &get_marshal ($type) unless $marshal;
	$name = @p[1];

#	print "=======$marshal===$type======\n";

	$list{$name} = {
	    name => $name,
	    type => $type,
	    marshal => $marshal,
	    out => $out
	};

	&add_external ($type);

	$ret .= "\n\t\t\t\t$marshal $out $type $name,";
    }

    if (&get_type ($methods{$x}->{"type"}) ne "") {
	$type = $methods{$x}->{"type"};
	$type =~ s/\[.*\],//;
	$marshal = &get_marshal ($type);

	$ret .= "$marshal ";
	$ret .= &get_out($type);
	$ret .= " " . &get_type ($type);
	$ret .= " ret";
	&add_external ($type);
    }
    $ret =~ s/,$//;
    return $ret;
}

sub parse_file {
    my $method = 0;
    my $mname = '';
    my $mtype = '';
    my $mparams = '';
    my $start = 0;

    while (my $line = <FILE>) {
	chop $line;

	next if !$start && $line !~ /\[scriptable/;
	$start = 1;
	last if $start && $line =~ /\};/;
	
	if (index($line, "*") == -1 && index ($line, "//") == -1 && index ($line, "#include") == -1) {
	    
	    if (index ($line, "uuid") != -1) {
		my $uuid = $line;
		$uuid =~ s/\[.*uuid\((.*)\)\]/\1/;
		$interface->{"uuid"} = $uuid;
	    }

	    elsif (index($line, "interface") != -1) {
		my $class = $line;
		$class =~ s/interface ([^\:|\s]+)\s:\s+(.*)/\1/;
#		print "\t\t==============$class\n";
		my $parent = $line;
		$parent =~ s/interface ([^\:|\s]+)\s:\s+(.*)/\2/;
		$interface->{"class"} = $class;
		$interface->{"parent"} = $parent;

	    }
	    elsif (index ($line, "const") != -1) {
	    }
	    elsif (index ($line, "attribute") != -1) {
		my $att = substr($line, index($line, "attribute") + 10);

		my @atts = split / /, $att;

		my $name = pop @atts;
		$name =~ s/;//;
#	    print $name . "\n";
		my @nospaces = grep /[^ ]/, @atts;
		my $type = join ",", @nospaces;

		my $setter = 0;
		if (index ($line, "readonly") != -1) {
		    $setter = 1;
		}
#            print $type . "\n";
		$properties{$name} = {type => $type, setter => $setter};
		$interface->{"items"} .= $name . ",";
	    }
	    elsif ($line !~ m/[{|}]/ && $line =~ m/./) {
#	    print $line . "\n";
		if (!$method) {
		    $method = 1;
		    my  $m = substr($line, 0, index($line, "("));
		    my @atts = split / /, $m;

#		print $m;
		    $mname = pop @atts;
#		print "name=$mname\n";
		    my @nospaces = grep /[^ ]/, @atts;
		    $mtype = join ",", @nospaces;
#		print "type=$mtype\n";
		    $mparams .= substr($line, index($line, "(") + 1);
		    $mparams =~ s/;//;
		    $mparams =~ s/\)//;

		    @atts = split / /, $mparams;
		    @nospaces = grep /[^ ]/, @atts;
		    $mparams = join " ", @nospaces;

		}
		elsif (index ($line, "raises") == -1) {
		    $mparams .= $line;
		    $mparams =~ s/;//;
		    $mparams =~ s/\)//;
		    my @atts = split / /, $mparams;
		    my @nospaces = grep /[^ ]/, @atts;
		    $mparams = join " ", @nospaces;
		}
		if (index ($line, ";") != -1) {
		    $method = 0;
		    $mparams =~ s/\[([^\]]+),([^\]]+)\]/\1 \2/;
		    $mparams =~ s/\(/ /;
		    $mparams =~ s/\)//;
		    $mparams =~ s/retval//;

		    $methods{$mname} = {type => $mtype, params => $mparams};
		    $interface->{"items"} .= $mname . ",";
#		    print "params=>$mparams\n";
		    $mname = '';
		    $mtype = '';
		    $mparams = '';
		}
	    }	
	}
    }
}

sub output {

    my $name = $interface->{"class"};
    print "$name.cs\n";
    open X, ">$name.cs";
    print X "// THIS FILE AUTOMATICALLY GENERATED BY xpidl2cs.pl\n";
    print X "// EDITING IS PROBABLY UNWISE\n";
    print X "// Permission is hereby granted, free of charge, to any person obtaining\n";
    print X "// a copy of this software and associated documentation files (the\n";
    print X "// \"Software\"), to deal in the Software without restriction, including\n";
    print X "// without limitation the rights to use, copy, modify, merge, publish,\n";
    print X "// distribute, sublicense, and/or sell copies of the Software, and to\n";
    print X "// permit persons to whom the Software is furnished to do so, subject to\n";
    print X "// the following conditions:\n";
    print X "// \n";
    print X "// The above copyright notice and this permission notice shall be\n";
    print X "// included in all copies or substantial portions of the Software.\n";
    print X "// \n";
    print X "// THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND,\n";
    print X "// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF\n";
    print X "// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND\n";
    print X "// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE\n";
    print X "// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION\n";
    print X "// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION\n";
    print X "// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.\n";
    print X "//\n";
    print X "// Copyright (c) 2007 Novell, Inc.\n";
    print X "//\n";
    print X "// Authors:\n";
    print X "//	Andreia Gaita (avidigal\@novell.com)\n";
    print X "//\n";
    print X "\n";
    print X "using System;\n";
    print X "using System.Runtime.InteropServices;\n";
    print X "using System.Runtime.CompilerServices;\n";
    print X "using System.Text;\n";
    print X "\n";
    print X "namespace Mono.Mozilla {\n";
    print X "\n";

    my $uuid = $interface->{"uuid"};
    print X "\t[Guid (\"$uuid\")]\n";
    print X "\t[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]\n";
    print X "\t[ComImport ()]\n";
    print X "\tinternal interface $name {\n";


    if ($interface->{"parent"} ne "nsISupports") {
	print X &parse_parent ($interface->{"parent"});
    }
    print X "\n";
    print X "#region $name\n";

    my @items = split ",", $interface->{"items"};
    for my $item (@items) {

	print X "\t\t[PreserveSigAttribute]\n";
	print X "\t\t[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]\n";

	if (&is_property ($item)) {
	    my $marshal = &get_marshal($properties{$item}->{"type"});
	    my $out = &get_out($properties{$item}->{"type"});
	    my $type = &get_type ($properties{$item}->{"type"});
	    my $name = ucfirst($item);

	    &add_external ($properties{$item}->{"type"});
## getter

	    print X "\t\tint get$name ($marshal $out $type ret);\n";
	    print X "\n";

## setter
	    if (&has_setter($item)) {
		print X "\t\t[PreserveSigAttribute]\n";
		print X "\t\t[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]\n";
		print X "\t\tint set$name ($marshal $type value);\n";
		print X "\n";
	    }

	} else {
	    print X "\t\t";
	    print X "int " . $item . " (";
	    print X &get_params($item);
	    print X ");";
	    print X "\n\n";
	}
    }
    print X "#endregion\n";
    print X "\t}\n";
    print X "}\n";
    close X;
}

sub generate_dependents {
    for my $file (keys %dependents) {
	print "generating $path$file.idl\n";
	if (! (-e "$file.cs") && -e "$path$file.idl") {
	    my $ret = `perl xpidl2cs.pl $file.idl $path`;
	    print "\n$ret";
	}
    }
}

&parse_file ();
&output ();
&generate_dependents ();
