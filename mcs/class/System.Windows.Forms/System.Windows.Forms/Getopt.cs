//
// System.Windows.Forms.Getopt.cs
//
// Author:
//   stubbed out by #!/usr/bin/perl -w
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {


	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

        use Getopt::Long;
        {
		 [MonoTODO]
		 use strict;
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		 #################################################################
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # Initialization #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #################################################################
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # Command-line options.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 my $o_comments = 0; # Defaults to 'false'.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 my $o_verbose = 0; # Defaults to 'flase'.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 my $o_namespace ='';
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 my $o_exception = 1;
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 my $o_shortcuts = 1;
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 GetOptions ( 'comments!' => \$o_comments, 
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  'v|verbose' => \$o_verbose,
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  'n|namespace=s' => \$o_namespace, 
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  'e|exception!' => \$o_exception,
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  's|shortcuts' => \$o_shortcuts );
		 {
			throw new NotImplementedException ();
		}


		 [MonoTODO]
		 our @lines;
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 our %config;
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 for my $file (@ARGV) {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  @lines = ();
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  next unless (prepare_file($file));
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		  obtain_configuration();
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		   "<$config>\n";
		 {
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  open_output_stream();
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  print_header();
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  parse_markup()
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 }
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		 #################################################################
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # F U N C T I O N S #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #################################################################
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		 #####
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # Run through the file reading off the '#' variables and the '\'
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #####
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 sub prepare_file {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  my $file = shift;
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		  # Shortcuts -- Unlike variables, these are first replaced and then processed
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  # like all other text. e.g. these are sensitive to '=' 
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  my %shortcuts = (
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  ctor => "//\n// --- Constructor\n//",
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  pub_methods => "//\n// --- Public Methods\n//",
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  pub_properties => "//\n// --- Public Properties\n//",
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  pub_events => "//\n// --- Public Events\n//",
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  pro_methods => "//\n// --- Protected Methods\n//",
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  pro_properties => "//\n// --- Protected Properties\n//",
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  dan => 'Daniel Carrera (dcarrera@math.toronto.edu)'
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  );
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		  unless(open(INPUT, "<$file")) {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  warn "Could not oppen $file. Skipping.\n";
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  return 0;
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  }
		 {
			throw new NotImplementedException ();
		}


		 [MonoTODO]
		  # First-run processing through the file:
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  # - Replace '#' shortcuts.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  # - Catenate spaces to one space.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  # - Wrap lines terminating in  #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  my $all_lines = '';
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  while (my $line = <INPUT>) { 
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  $line =~ s/[ \t]+/ /g; # catenate spaces to one space.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  $line =~ s/#(\w+)/$shortcuts/g if ($o_shortcuts);
		 {
		}

		 [MonoTODO]
		  if ( $line =~ /\\$/) { 
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  chomp($line); # Eliminate new-line character.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  chop($line); # Eliminate  }
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  $all_lines .= $line;
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  }
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  @lines = split(/\n/, $all_lines);
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  close(INPUT);
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  return 1;
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 }
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		 #####
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # Obtain Configuration
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #####
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 sub obtain_configuration {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  my @path;
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		  until( $config
		 {
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
			remove { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  until( $config
		 {
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
		}

		 [MonoTODO]
		  # The last entry before ':' is the class name.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  $config; # Start with the class definition.
		 {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  $config=~ s/\s*:.*//; # Chop everything after :
		 {
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  ($config=~ m/(\S+)$/; # Get the last word.
		 {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
		}

		 [MonoTODO]
		  unless ($o_namespace) {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  if ($^O eq 'linux') 
		 {
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  elsif ($^O eq 'MSWin32') 
		 {
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  else 
		 {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
			remove { throw new NotImplementedException (); }
		}

		 [MonoTODO]
		  chomp($o_namespace = $path[-1]);
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  }
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 }
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		 #####
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # Open OUTPUT stream.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #####
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 sub open_output_stream {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  if ($o_verbose) {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  open(OUT,"| tee $config.cs") or warn "Could not open pipe to 'tee'\n";
		 {
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  select OUT;
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  $| = 1; # this sets output to the last selected handle to be unbuffered
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  } else {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  if ( open(OUT,">$config.cs") ) {
		 {
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  select OUT
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  } else { 
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  warn "WARNING: Could not open $config.cs for writing\n".
		 {
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  " sending output to STDOUT\n";
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  }
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  }
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 }
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # Print the header.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 sub print_header {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  print <<EOH
		 {
			throw new NotImplementedException ();
		}
		//
		// $o_namespace.$config{class}
		//
		// Author:
		// stubbed out by $config{author}
		//
		// (C) 2002 Ximian, Inc
		//

		 [MonoTODO]
		 namespace $o_namespace
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 {
		 {
			throw new NotImplementedException ();
		}

		 // <summary>
		 // This is only a template. Nothing is implemented yet.
		 //
		 // </summary>

		 [MonoTODO]
		  $config
		 {
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 EOH
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 ;
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 }
		 {
			throw new NotImplementedException ();
		}


		 [MonoTODO]
		 #####
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # Parse the markup and take appropriate action.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #####
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		 sub parse_markup {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  ####
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  # Variables -- Shortcuts for common commands. These are not affected by '='.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  # They must be on a line of their own.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  ####
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  my %variables = (
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  equals => "public virtual bool Equals(object o);\n".
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  "public static bool Equals(object o1, object o2);\n",
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  ctor => "public $config()\n"
		 {
			set { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  );
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		  # Print the properties, classes, etc.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  my $append='';
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  foreach my $line (@lines) {
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		  unless ($line =~ /\S/) # skip blank lines.
		 {
			remove { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  if ($line =~ m[^\s*//]) # C# comments.
		 {
			remove { throw new NotImplementedException (); }
		}

		 [MonoTODO]
		  #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  # Prepend command: 
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  # '=public void' causes 'public void' to prepend the following code.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  if ($line =~ m[^=]) {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  $append = ( ($line =~ m[^=(.+)]) ? "$1 " : '');
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  next;
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  }
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		  #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  # Extra variables.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  # $var is replaced by the corresponding code ('=' option not applicable).
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  if ($line =~ m[^\$(\S+)]) {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  die "Variable $1 not recognized\n" unless ($variables);
		 {
		}
		 [MonoTODO]
		  my @lines = split(/\n/, $variables);
		 {
		}
		 [MonoTODO]
		  for my $line (@lines) 
		 {
			get { throw new NotImplementedException (); }
			add { throw new NotImplementedException (); }
			remove { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  next;
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  }
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		  # If we get this far than we have real code.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  generate_code( append => $append, line => $line);
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  }
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		  print "\t}\n}\n";
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 }
		 {
			throw new NotImplementedException ();
		}


		 [MonoTODO]
		 ####
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 # Parse the actual code.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 #
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 ####
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		 sub generate_code {
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  my %hash = @_;
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  my $append = $hash;
		 {
			add { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  my $line = $hash;
		 {
		}
		 [MonoTODO]
		  my $throw = "throw new NotImplementedException ();";
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		  my $contents = '';
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  # Contains , i.e. property or event.
		 {
		}
		 [MonoTODO]
		  if ( $line =~ s/
		 {
			set { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  else # \0 is a non-priting character. 
		 {
			set { throw new NotImplementedException (); }
		}

		 [MonoTODO]
		  my $get = "get ";
		 {
			remove { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  my $set = "set ";
		 {
			remove { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  my $add = "add ";
		 {
			remove { throw new NotImplementedException (); }
		}
		 [MonoTODO]
		  my $remove = "remove ";
		 {
			remove { throw new NotImplementedException (); }
		}

		 [MonoTODO]
		  my $c = ($o_comments? "//" : '');
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		  # \010 (octal 10) is the backspace character.
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  print <<EOH
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  $c \010[MonoTODO]
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  $c \010$append$line
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  $c \010{
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 EOH
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		 ;
		 {
			throw new NotImplementedException ();
		}

		 [MonoTODO]
		  print "\t\t$c\t$get\n" if ($contents =~ /g/i);
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  print "\t\t$c\t$set\n" if ($contents =~ /s/i);
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  print "\t\t$c\t$add\n" if ($contents =~ /a/i);
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  print "\t\t$c\t$remove\n" if ($contents =~ /r/i);
		 {
			throw new NotImplementedException ();
		}
		 [MonoTODO]
		  print "\t\t$c
