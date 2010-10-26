use IO::File;

my $file = new IO::File("CoreClr.Tools/PlatformCode.cs", "w");

$file->print( <<END
namespace CoreClr.Tools {

	static public class PlatformCode {

		// Code adapted from ../../moon/class/tuning/SecurityAttributes/PlatformCode.cs.
		public static readonly string [] Assemblies = {
END
);

for (@ARGV) {
	$file->print("\t\t\t\"$_\",\n")
}

$file->print( <<END
		};
	}
}
END
);

$file->close();