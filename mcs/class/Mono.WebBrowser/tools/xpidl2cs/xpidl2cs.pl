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




my $file;
my $path;
my $nosig;
my $_class;
my %opt=();

#open FILE, '<', $path.$file or die "Can't open file $path$file";

my %interface = (
		 properties => (), 
		 items => "", 
		 uuid => "", 
		 class => "", 
		 parent => ""
		 );
my %properties;
my %methods = {
	type => "",
	params => ()
};

my %types;
$types{"short"} = {name => "short", out => "out", marshal => ""};
$types{"PRUint8"} = {name => "char", out => "out", marshal => ""};
$types{"PRInt8"} = {name => "char", out => "out", marshal => ""};
$types{"unsigned,short"} = {name => "ushort", out => "out", marshal => ""};
$types{"PRUint16"} = {name => "ushort", out => "out", marshal => ""};
$types{"PRInt16"} = {name => "short", out => "out", marshal => ""};
$types{"int"} = {name => "int", out => "out", marshal => ""};
$types{"nsresult"} = {name => "int", out => "out", marshal => ""};
$types{"unsigned,int"} = {name => "uint", out => "out", marshal => ""};
$types{"PRUint32"} = {name => "UInt32", out => "out", marshal => ""};
$types{"PRInt32"} = {name => "Int32", out => "out", marshal => ""};
$types{"PRInt64"} = {name => "long", out => "out", marshal => ""};
$types{"long"} = {name => "int", out => "out", marshal => ""};
$types{"size_t"} = {name => "int", out => "out", marshal => ""};
$types{"unsigned,long"} = {name => "uint", out => "out", marshal => ""};
$types{"float"} = {name => "float", out => "out", marshal => ""};
$types{"boolean"} = {name => "bool", out => "out", marshal => ""};
$types{"PRBool"} = {name => "bool", out => "out", marshal => ""};
$types{"void"} = {name => "", out => "", marshal => ""};
$types{"octet"} = {name => "byte", out => "out", marshal => ""};
$types{"octet[]"} = {name => "IntPtr", out => "out", marshal => " "};
$types{"byte"} = {name => "byte", out => "out", marshal => ""};
$types{"DOMString"} = {name => "/*DOMString*/ HandleRef", out => "", marshal => ""};
$types{"AUTF8String"} = {name => "/*AUTF8String*/ HandleRef", out => "", marshal => ""};
$types{"ACString"} = {name => "/*ACString*/ HandleRef", out => "", marshal => ""};
$types{"AString"} = {name => "/*AString*/ HandleRef", out => "", marshal => ""};
$types{"wstring"} = {name => "string", out => "", marshal => "MarshalAs(UnmanagedType.LPWStr)"};
$types{"nsCIDRef"} = {name => "Guid", out => "out", marshal => "MarshalAs (UnmanagedType.LPStruct)"};
$types{"nsIIDRef"} = {name => "Guid", out => "out", marshal => "MarshalAs (UnmanagedType.LPStruct)"};
$types{"Guid"} = {name => "Guid", out => "out", marshal => "MarshalAs (UnmanagedType.LPStruct)"};
$types{"nsCID"} = {name => "Guid", out => "out", marshal => "MarshalAs (UnmanagedType.LPStruct)"};
$types{"nsCIDPtr"} = {name => "Guid", out => "out", marshal => "MarshalAs (UnmanagedType.LPStruct)"};
$types{"string"} = {name => "string", out => "ref", marshal => "MarshalAs (UnmanagedType.LPStr)"};
$types{"refstring"} = {name => "IntPtr", out => "ref", marshal => ""};
$types{"charPtr"} = {name => "StringBuilder", out => "", marshal => ""};
$types{"voidPtr"} = {name => "IntPtr", out => "", marshal => ""};
$types{"nsISupports"} = {name => "IntPtr", out => "out", marshal =>"MarshalAs (UnmanagedType.Interface)"};
$types{"DOMTimeStamp"} = {name => "int", out => "out", marshal => ""};
$types{"nsWriteSegmentFun"} = {name => "nsIWriteSegmentFunDelegate", out => "", marshal => ""};
$types{"nsReadSegmentFun"} = {name => "nsIReadSegmentFunDelegate", out => "", marshal => ""};
$types{"nsTimerCallbackFunc"} = {name => "nsITimerCallbackDelegate", out => "", marshal => ""};
$types{"nsLoadFlags"} = {name => "ulong", out => "out", marshal => ""};
$types{"nsQIResult"} = {name => "IntPtr", out => "out", marshal => ""};
$types{"nsIIDPtr[]"} = {name => "IntPtr", out => "out", marshal => ""};
$types{"PRFileDescStar"} = {name => "IntPtr", out => "out", marshal => ""};
$types{"PRLibraryStar"} = {name => "IntPtr", out => "out", marshal => ""};
$types{"FILE"} = {name => "IntPtr", out => "out", marshal => ""};
$types{"nsIPresShell"} = {name => "/*nsIPresShell*/ IntPtr", out => "out", marshal => ""};
$types{"nsIDocument"} = {name => "/*nsIDocument*/ IntPtr", out => "out", marshal => ""};
$types{"nsIFrame"} = {name => "/*nsIFrame*/ IntPtr", out => "out", marshal => ""};
$types{"nsObjectFrame"} = {name => "/*nsObjectFrame*/ IntPtr", out => "out", marshal => ""};
$types{"nsIContent"} = {name => "/*nsIContent*/ IntPtr", out => "out", marshal => ""};
$types{"others"} = {name => "", out => "out", marshal => "MarshalAs (UnmanagedType.Interface)"};

my %returnvalues;
$returnvalues{"short"} = {value => "0"};
$returnvalues{"ushort"} = {value => "0"};
$returnvalues{"int"} = {value => "0"};
$returnvalues{"uint"} = {value => "0"};
$returnvalues{"UInt32"} = {value => "0"};
$returnvalues{"Int32"} = {value => "0"};
$returnvalues{"long"} = {value => "0"};
$returnvalues{"ulong"} = {value => "0"};
$returnvalues{"IntPtr"} = {value => "0"};
$returnvalues{"float"} = {value => "0"};
$returnvalues{"byte"} = {value => "0"};
$returnvalues{"IntPtr"} = {value => "IntPtr.Zero"};
$returnvalues{"string"} = {value => "String.Empty"};
$returnvalues{"bool"} = {value => "false"};
$returnvalues{"/*DOMString*/ HandleRef"} = {value => "null"};
$returnvalues{"/*AUTF8String*/ HandleRef"} = {value => "null"};
$returnvalues{"ACString*/ HandleRef"} = {value => "null"};
$returnvalues{"/*AString*/ HandleRef"} = {value => "null"};
$returnvalues{""} = {value => ""};
$returnvalues{"others"} = {value => "null"};

my %names;
$names{"event"} = {name => "_event"};
$names{"lock"} = {name => "_lock"};

my %dependents;
   
my $class_implementation;



sub usage ()
{
	print STDERR << "EOF";
    Usage: xpidl2cs.pl -f file -p path/to/idl [-nh -c class]
    -h		: this help
    -f		: idl file to parse, with extension
    -p		: path to the idl file directory
    -n		: generate files with no PreserveSig attribute (optional, defaults to adding the attribute)
    -c		: specific class to use inside the idl file (optional)
EOF
	exit;
}

sub init ()
{
	use Getopt::Std;
	my $opts = 'f:p:c:n';
	getopts( "$opts", \%opt ) or usage();
	usage if $opt{h};

	usage() if !$opt{f} or !$opt{p};

	$file = $opt{f};
	$path = $opt{p};
	open FILE, '<', $path.$file or die "Can't open file $path$file";
	
	$nosig = 1 if $opt{n};
	$_class = $opt{c};
}


sub trim{
#print "trim\n";
    $_[0]=~s/^\s+//;
    $_[0]=~s/\s+$//;
    return;
}

sub parse_parent {
#print "parse_parent\n";
    my $x = shift;

	print "Parsing parent $x\n";
    `perl xpidl2cs.pl $x.idl $path $nosig`;

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
#print "has_setter\n";
    my $x = shift;
    return !$properties{$x}->{"setter"};
}

sub get_name {
#print "get_name\n";
    my $x = shift;

    if (exists $names{$x}) {
		return $names{$x}->{"name"};
    }
    return $x;
}

sub get_type {
#print "get_type\n";
    my $x = shift;
    my $out = shift;
    my $arr = shift;

#    print "arr = $arr ; out = $out ; name = $x\n";

    if ($out) {
		if ($arr && exists $types{"$out$x\[\]"}) {
			return $types{"$out$x\[\]"}->{"name"};
		} elsif ($arr && exists $types{"$out$x"}) {
			return $types{"$out$x"}->{"name"}."[]";
		} elsif (exists $types{"$out$x"}) {
			return $types{"$out$x"}->{"name"};
		}
    }

    if (exists $types{$x} || ($arr && exists $types{"$x\[\]"})) {
		if ($arr && exists $types{"$x\[\]"}) {
			return $types{"$x\[\]"}->{"name"};
		} elsif ($arr) {
			return $types{$x}->{"name"}."[]";
		} else {
			return $types{$x}->{"name"};
		}
    }
    return $x;
}

sub get_out {
#print "get_out\n";
    my $x = shift;
    if (exists $types{$x}) {
		return $types{$x}->{"out"};
    }
    return $types{"others"}->{"out"};
}

sub get_marshal {
#print "get_marshal\n";
    my $x = shift;
    my $out = shift;
    my $arr = shift;

    if ($out) {
		if ($arr && exists $types{"$out$x\[\]"}) {
			return $types{"$out$x\[\]"}->{"marshal"};
		} elsif (exists $types{"$out$x"}) {
			return $types{"$out$x"}->{"marshal"};
		}
    }

    if (exists $types{$x} || ($arr && exists $types{"$x\[\]"})) {
		if ($arr && exists $types{"$x\[\]"}) {
			return $types{"$x\[\]"}->{"marshal"};
		} else {
			return $types{$x}->{"marshal"};
		}
    }

    return $types{"others"}->{"marshal"};
}

sub get_return_value {
#print "get_return_value\n";
    my $x = shift;
    if (exists $returnvalues{$x}) {
		return $returnvalues{$x}->{"value"};
    }
    return $returnvalues{"others"}->{"value"};
}

		
sub is_property {
#print "is_property\n";
    my $x = shift;
    return (exists $properties{$x});
}

sub add_external {
#print "add_external\n";
    my $x = shift;
    if ($x !~ /nsISupports/ && !exists $types{$x} && !exists $dependents{$x}) {
		$dependents{$x} = $x;
    }
#    print "add_external $x\n";
}

sub get_params {
#print "get_params\n";
    my $x = shift;
    my %list;
#print $methods{$x}->{"params"}."\n";
    my @params = split /,/, $methods{$x}->{"params"};
	my $lastoutparam = "";
	my @ret = ();
	
#print "params:@params:\n";
    for my $param (@params) {
		my $marshal;
		my $name;
		my $type;
		my $out;
		my $isout;


#	print "param:$param:\n";
		my @p = split (" ", $param);
#	print "@p\n";
# need to backtrack to a previous parameter defined by iid_is(name) and 
# replace the type of this one with that. who the $%#@ came up with this idea? le sigh.

		if (@p[0] =~ m/iid_is/) {
		    shift @p;
		    $name = &get_name (@p[0]);
		    $name =~ s/ //;
		    $type = $list{$name}->{"type"};
	    	$marshal = $list{$name}->{"marshal"};
		    $marshal = " " if !$marshal;
		    $name = "";
		    until (scalar(@p) == 3) {
				shift @p;
		    }
		}
	
		if (@p[0] =~ m/array/ || @p[1] =~ m/array/) {
		    until (scalar(@p) == 3) {
				shift @p;
		    }
	    	$isout = 1 if (@p[0] =~ m/out/);
		    shift @p;
		    $marshal = &get_marshal (@p[0], "", 1);
	    	$type = &get_type(@p[0], "", 1);
		}

		shift @p unless @p[0] =~ /(in|out)/;
		$isout = 1 if (@p[0] =~ m/out/);
		shift @p unless scalar(@p) <= 2;

	# if an out parameter is of type nsQIResult, that means
	# it will return a pointer to an interface (that can be anything). 
	# That means we want to return an IntPtr, and later cast it to
	# the proper type, so reset type and marshalling
		if ($isout && @p[0] =~ /nsQIResult/) {
		    $marshal = "";
	    	$type = "";
		}

		if (!$type) {
	    	$type = join ",", @p[0..@p-2];
		    $type=~s/\[.*\],//;
		    until (scalar(@p) == 1) {
				shift @p;
			}

		    $marshal = &get_marshal ($type);
	    	$marshal = " " if !$marshal;
		    $type = &get_type ($type);
		    $name = &get_name (@p[0]);
		}
#print "marshal:$marshal\ttype:$type\tname:$name\n";
		$out = &get_out($type) if $isout;

		$type = &get_type (@p[0]) unless $type;
		shift @p unless scalar(@p) == 1;
		$marshal = &get_marshal ($type) unless $marshal;
		$name = &get_name (@p[0]) unless $name;

#print "marshal:$marshal\ttype:$type\tname:$name\n";

		$list{$name} = {
		    name => $name,
		    type => $type,
		    marshal => $marshal,
	    	out => $out,
			isout => $isout
		};

		&add_external ($type);

		$marshal = "" if $marshal eq " ";

#		my $tmp = "\n\t\t\t\t";
		my $tmp = "";
		$tmp .= "[$marshal] " if $marshal;
		$tmp .= "$out $type $name";
		push (@ret, $tmp);
		$lastoutparam = $name if $isout;
#print "tmp:$tmp\n";
    }

#print "$methods{$x}->{\"type\"}\n";
#print "nosig:$nosig;x:$x;type:" . &get_type ($methods{$x}->{"type"}) . ";\n";
	if (!$nosig && $x !~ /void/ && &get_type ($methods{$x}->{"type"}) ne "") {
		$type = $methods{$x}->{"type"};
		$type =~ s/\[.*\],//;
		$marshal = &get_marshal ($type);

		my $tmp = "";
		$tmp = "[$marshal] " if $marshal;
		$tmp .= &get_out($type);
		$tmp .= " " . &get_type ($type);
		$tmp .= " ret";
#print "tmp 2:$tmp\n";
		push (@ret, $tmp);
		
		&add_external ($type);
    }

	if ($nosig && &get_type ($methods{$x}->{"type"}) eq "" && $lastoutparam) {
		$methods{$x}->{"type"} = $list{$lastoutparam}->{"type"};
		pop (@ret);
	}
#print "@ret\n";
	
	return join (",\n\t\t\t\t", @ret);
}

sub parse_file {
#print "parse_file\n";
    my $method = 0;
    my $mname = '';
    my $mtype = '';
    my $mparams = '';
    my $start = 0;
	my $comment = 0;

    while (my $line = <FILE>) {
		chop $line;

		next if !$start && $line !~ /uuid\(/;
		$start = 1;
		last if $start && $line =~ /\};/;

		trim ($line);

		if (index($line, "/*") > -1) {
			$comment = 1;
			next;
		}
		if ($comment && index($line, "*/") > -1) {
			$comment = 0;
			next;
		}

		next if $comment;

		if (index($line, "*") == -1 && index ($line, "//") == -1 && index ($line, "#include") == -1) {

			$line =~ s/\[noscript\] //;
		
			if (index ($line, "uuid(") != -1) {
				my $uuid = $line;
				$uuid =~ s/\[.*uuid\((.*)\)\]/\1/;
				$interface->{"uuid"} = $uuid;
			}

			elsif (index($line, "interface") != -1) {
				my $class = $line;
				$class =~ s/interface ([^\:|\s]+)\s*:\s*(.*)/\1/;
#		print "\t\tclass:$class\n";
#		print "\t\t_class:$_class\n";
				if ($_class && $_class !~ $class) {
					$uuid = '';
					$class = '';
					$method = 0;
					$mname = '';
					$mtype = '';
					$mparams = '';
					$start = 0;
					$comment = 0;
					next;
				}

				my $parent = $line;
				$parent =~ s/([^\:]+):\s*(.*)[\s|\{]/\2/;
#		print "\t\tparent:$parent\n";
				$interface->{"class"} = $class;
				$interface->{"parent"} = $parent;
			}
			elsif (index ($line, "const") != -1 && index ($line, "[") == -1) {
				next;
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
#		print $line . "\n";
				if (!$method) {
					$method = 1;
					my  $m = substr($line, 0, index($line, "("));
					my @atts = split / /, $m;

#		    print "$m\n";
					$mname = pop @atts;
#		    print "name=$mname\n";
					my @nospaces = grep /[^ ]/, @atts;
					$mtype = join ",", @nospaces;
					$mtype =~ s/\[.*\],//;
#		    print "type=$mtype\n";
					$mparams .= substr($line, index($line, "(") + 1);
					$mparams =~ s/;//;
					$mparams =~ s/\)//;

					@atts = split / /, $mparams;
					@nospaces = grep /[^ ]/, @atts;
					$mparams = join " ", @nospaces;
#		    print "params=>$mparams\n";
					
				}
				elsif (index ($line, "raises") == -1) {
					$mparams .= $line;
					$mparams =~ s/;//;
					$mparams =~ s/\)//;
					my @atts = split / /, $mparams;
					my @nospaces = grep /[^ ]/, @atts;
					$mparams = join " ", @nospaces;
#		    print "params=>$mparams\n";
				}
				if (index ($line, ";") != -1) {
					$method = 0;
					$mparams =~ s/\[([^\]]+),([^\]]+),([^\]]+)\]/\1 \2 \3/;
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
#print "output\n";
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
    print X "// Copyright (c) 2007, 2008 Novell, Inc.\n";
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
    my $parent = $interface->{"parent"};
    print X "\t[Guid (\"$uuid\")]\n";
    print X "\t[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]\n";
    print X "\t[ComImport ()]\n";
    print X "\tinternal interface $name";
    print X " : $parent" if $parent !~ /nsISupports/;
    print X " {\n";

    if ($parent !~ /nsISupports/) {
		print X &parse_parent ($parent);
    }
    print X "\n";
    print X "#region $name\n";

    my @items = split ",", $interface->{"items"};
    for my $item (@items) {
	
		if (!$nosig) {
			print X "\t\t[PreserveSigAttribute]\n";
		}
		print X "\t\t[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]\n";

		if (&is_property ($item)) {
			my $out = &get_out($properties{$item}->{"type"});
			my $marshal = &get_marshal($properties{$item}->{"type"}, $out);
			my $type = &get_type ($properties{$item}->{"type"}, $out);
			my $name = ucfirst ($item);

			&add_external ($properties{$item}->{"type"});
## getter
			print X "\t\t";
			if ($nosig) {
				print X "[return: $marshal] " if $marshal;
				print X "$type get$name ();\n";
			} else {
				print X "int get$name (";
				print X "[$marshal] " if $marshal;
				print X "$out $type ret);\n";
			}
			print X "\n";

			$type = &get_type ($properties{$item}->{"type"});
			$marshal = &get_marshal($properties{$item}->{"type"});

## setter
			if (&has_setter($item)) {
				if (!$nosig) {
					print X "\t\t[PreserveSigAttribute]\n";
				}
				print X "\t\t[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]\n";
				if ($nosig) {
					print X "\t\tvoid";
				} else {
					print X "\t\tint";
				}
				print X " set$name (";
				print X "[$marshal] " if $marshal;
				print X "$type value);\n";
				print X "\n";
			}

		} else {
			my $type = &get_type ($methods{$item}->{"type"});
			my $out = &get_out($methods{$item}->{"type"}) if $type;
			my $marshal = &get_marshal($methods{$item}->{"type"}, $out) if $type;
			$type = "void" if !$type;

			print X "\t\t";
			if ($nosig) {
				print X "[return: $marshal] " if $marshal;
				print X "$type $item (";
				print X &get_params($item);
				print X ");";
			} else {
				print X "int $item (";
				print X &get_params($item);
				print X ");";
			}
			print X "\n\n";
		}
    }
    print X "#endregion\n";
    print X "\t}\n";


# mozilla-specific helper classes to proxy objects between threads
# remove if you're not running this for mono.mozilla

    print X "\n\n";
    $helpername = $name;
    $helpername =~ s/nsI/ns/;
    print X "\tinternal class $helpername";
    print X " {\n";
    print X "\t\tpublic static $name GetProxy (Mono.WebBrowser.IWebBrowser control, $name obj)\n";
    print X "\t\t{\n";
    print X "\t\t\tobject o = Base.GetProxyForObject (control, typeof($name).GUID, obj);\n";
    print X "\t\t\treturn o as $name;\n";
    print X "\t\t}\n";
    print X "\t}\n";

#end of mozilla-specific helper classes

    print X "}\n";
	
	
	&generate_class_implementation_example ();
	
	
    close X;
}

sub generate_dependents {
#print "generate_dependents\n";
    for my $dependent (keys %dependents) {
		if (! (-e "$dependent.cs") && -e "$path$dependent.idl" && $file != $dependent) {
			print "generating $path$dependent.idl\n";
			my $cmd = "perl xpidl2cs.pl -f $dependent.idl -p $path";
			$cmd .= "-n" if $nosig;
			my $ret = `$cmd`;
			print "\n$ret";
		}
    }
}

sub generate_class_implementation_example {
#print "generate_class_implementation_example\n";
    my $name = $interface->{"class"};
	my $interfacename = $interface->{"class"};
    my $helpername = $name;
    $helpername =~ s/nsI//;
    my $parent = $interface->{"parent"};

	print X "#if example\n\n";
    print X "using System;\n";
    print X "using System.Runtime.InteropServices;\n";
    print X "using System.Runtime.CompilerServices;\n";
    print X "using System.Text;\n";
    print X "\n";

    print X "\tinternal class $helpername";
    print X " : $interfacename";
    print X " {\n";

    print X "\n";
    print X "#region $interfacename\n";

    my @items = split ",", $interface->{"items"};
    for my $item (@items) {

		if (&is_property ($item)) {
			my $out = &get_out($properties{$item}->{"type"});
			my $marshal = &get_marshal($properties{$item}->{"type"}, $out);
			my $type = &get_type ($properties{$item}->{"type"}, $out);

			my $retval = &get_return_value($type);
			my $name = ucfirst ($item);

## getter
			print X "\t\t";
			if ($nosig) {
				print X "[return: $marshal] " if $marshal;
				print X "$type $interfacename.get$name ()\n";
			} else {
				print X "int $interfacename.get$name (";
				print X "[$marshal] " if $marshal;
				print X "$out $type ret)\n";
			}

			print X "\n\t\t{\n";
			print X "\t\t\t";

			print X "return $retval;\n";

			print X "\t\t";
			print X "}\n";
			print X "\n";

			$type = &get_type ($properties{$item}->{"type"});
			$retval = &get_return_value($type);
			$marshal = &get_marshal($properties{$item}->{"type"});

## setter
			if (&has_setter($item)) {
				if ($nosig) {
					print X "\t\tvoid";
				} else {
					print X "\t\tint";
				}
				print X " $interfacename.set$name (";
				print X "[$marshal] " if $marshal;
				print X "$type value)\n";
				print X "\n";

				print X "\n\t\t{\n";
				print X "\t\t\t";

				print X "return $retval;\n";

				print X "\t\t";
				print X "}\n";
				print X "\n";
			}

		} else {
			my $type = &get_type ($methods{$item}->{"type"});
			my $out = &get_out($methods{$item}->{"type"}) if $type;
			my $marshal = &get_marshal($methods{$item}->{"type"}, $out) if $type;
			$type = "void" if !$type;

			print X "\t\t";
			if ($nosig) {
				print X "[return: $marshal] " if $marshal;
				print X "$type $interfacename.$item (";
				print X &get_params($item);
				print X ")";
			} else {
				print X "int $interfacename.$item (";
				print X &get_params($item);
				print X ")";
			}

			print X "\n\t\t{\n";
			print X "\t\t\t";

			print X "return $retval;\n";

			print X "\t\t";
			print X "}\n";
			print X "\n";
			print X "\n\n";
		}
    }
    print X "#endregion\n";
    print X "\t}\n";


    print X "#endif\n";
	
}

&init();
&parse_file ();
&output ();
&generate_dependents ();
