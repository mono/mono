#region generated code 11.06.2005 12:21:32

using System;
using System.Collections;
using System.Collections.Specialized;

namespace System.Windows.Forms
{
	internal struct MimeGenerated
	{
		public static NameValueCollection Aliases = new NameValueCollection( new CaseInsensitiveHashCodeProvider(), new Comparer( System.Globalization.CultureInfo.CurrentUICulture ) );
		public static NameValueCollection SubClasses = new NameValueCollection( new CaseInsensitiveHashCodeProvider(), new Comparer( System.Globalization.CultureInfo.CurrentUICulture ) );

		public static NameValueCollection GlobalPatternsShort = new NameValueCollection( new CaseInsensitiveHashCodeProvider(), new Comparer( System.Globalization.CultureInfo.CurrentUICulture ) );
		public static NameValueCollection GlobalPatternsLong = new NameValueCollection( new CaseInsensitiveHashCodeProvider(), new Comparer( System.Globalization.CultureInfo.CurrentUICulture ) );
		public static NameValueCollection GlobalLiterals = new NameValueCollection( new CaseInsensitiveHashCodeProvider(), new Comparer( System.Globalization.CultureInfo.CurrentUICulture ) );
		public static NameValueCollection GlobalSufPref = new NameValueCollection( new CaseInsensitiveHashCodeProvider(), new Comparer( System.Globalization.CultureInfo.CurrentUICulture ) );
		public static Hashtable MimeTypes = new Hashtable();

		public static ArrayList Matches80Plus = new ArrayList();
		public static ArrayList MatchesBelow80 = new ArrayList();

		public static void Init()
		{

			Aliases.Add( "application/pdf", "application/x-pdf" );
			Aliases.Add( "application/stuffit", "application/x-stuffit" );
			Aliases.Add( "application/stuffit", "application/x-sit" );
			Aliases.Add( "application/vnd.lotus-1-2-3", "application/x-lotus123" );
			Aliases.Add( "application/vnd.lotus-1-2-3", "application/x-123" );
			Aliases.Add( "application/vnd.lotus-1-2-3", "application/lotus123" );
			Aliases.Add( "application/vnd.lotus-1-2-3", "application/wk1" );
			Aliases.Add( "application/vnd.ms-excel", "application/msexcel" );
			Aliases.Add( "application/msword", "application/vnd.ms-word" );
			Aliases.Add( "application/vnd.wordperfect", "application/x-wordperfect" );
			Aliases.Add( "application/vnd.wordperfect", "application/wordperfect" );
			Aliases.Add( "application/x-dbf", "application/x-dbase" );
			Aliases.Add( "application/x-dbf", "application/dbf" );
			Aliases.Add( "application/x-dbf", "application/dbase" );
			Aliases.Add( "application/x-desktop", "application/x-gnome-app-info" );
			Aliases.Add( "text/x-lyx", "application/x-lyx" );
			Aliases.Add( "application/zip", "application/x-zip-compressed" );
			Aliases.Add( "audio/midi", "audio/x-midi" );
			Aliases.Add( "audio/mp4", "audio/x-m4a" );
			Aliases.Add( "audio/mpeg", "audio/x-mp3" );
			Aliases.Add( "audio/x-pn-realaudio", "audio/vnd.rn-realaudio" );
			Aliases.Add( "audio/x-wav", "audio/wav" );
			Aliases.Add( "audio/x-wav", "audio/vnd.wave" );
			Aliases.Add( "image/vnd.djvu", "image/x-djvu" );
			Aliases.Add( "text/calendar", "text/x-vcalendar" );
			Aliases.Add( "text/directory", "text/x-vcard" );

			SubClasses.Add( "application/rtf", "text/plain" );
			SubClasses.Add( "application/smil", "text/plain" );
			SubClasses.Add( "application/vnd.mozilla.xul+xml", "text/xml" );
			SubClasses.Add( "application/vnd.sun.xml.calc", "application/zip" );
			SubClasses.Add( "application/x-xbel", "text/plain" );
			SubClasses.Add( "application/x-7z-compressed", "application/octet-stream" );
			SubClasses.Add( "application/x-asp", "text/plain" );
			SubClasses.Add( "application/x-awk", "text/plain" );
			SubClasses.Add( "application/x-cgi", "text/plain" );
			SubClasses.Add( "application/x-csh", "application/x-shellscript" );
			SubClasses.Add( "application/x-csh", "text/plain" );
			SubClasses.Add( "application/x-desktop", "text/plain" );
			SubClasses.Add( "application/x-glade", "text/xml" );
			SubClasses.Add( "application/x-java-jnlp-file", "text/xml" );
			SubClasses.Add( "application/x-javascript", "text/plain" );
			SubClasses.Add( "application/x-magicpoint", "text/plain" );
			SubClasses.Add( "application/x-nautilus-link", "text/plain" );
			SubClasses.Add( "application/x-netscape-bookmarks", "text/plain" );
			SubClasses.Add( "application/x-perl", "application/x-executable" );
			SubClasses.Add( "application/x-perl", "text/plain" );
			SubClasses.Add( "application/x-php", "text/plain" );
			SubClasses.Add( "application/x-profile", "text/plain" );
			SubClasses.Add( "application/x-reject", "text/plain" );
			SubClasses.Add( "application/x-ruby", "application/x-executable" );
			SubClasses.Add( "application/x-ruby", "text/plain" );
			SubClasses.Add( "application/x-shar", "text/plain" );
			SubClasses.Add( "application/x-shellscript", "text/plain" );
			SubClasses.Add( "application/x-theme", "text/plain" );
			SubClasses.Add( "application/x-troff", "text/plain" );
			SubClasses.Add( "application/x-troff-man", "text/plain" );
			SubClasses.Add( "application/xhtml+xml", "text/xml" );
			SubClasses.Add( "image/svg+xml", "text/xml" );
			SubClasses.Add( "image/x-eps", "application/postscript" );
			SubClasses.Add( "inode/mount-point", "inode/directory" );
			SubClasses.Add( "message/delivery-status", "text/plain" );
			SubClasses.Add( "message/news", "text/plain" );
			SubClasses.Add( "message/partial", "text/plain" );
			SubClasses.Add( "message/rfc822", "text/plain" );
			SubClasses.Add( "text/mathml", "text/xml" );
			SubClasses.Add( "text/rdf", "text/plain" );
			SubClasses.Add( "text/richtext", "text/plain" );
			SubClasses.Add( "text/rss", "text/plain" );
			SubClasses.Add( "text/tab-separated-values", "text/plain" );
			SubClasses.Add( "text/x-adasrc", "text/plain" );
			SubClasses.Add( "text/x-authors", "text/plain" );
			SubClasses.Add( "text/x-bibtex", "text/plain" );
			SubClasses.Add( "text/x-c++hdr", "text/plain" );
			SubClasses.Add( "text/x-c++src", "text/plain" );
			SubClasses.Add( "text/x-chdr", "text/plain" );
			SubClasses.Add( "text/x-comma-separated-values", "text/plain" );
			SubClasses.Add( "text/x-copying", "text/plain" );
			SubClasses.Add( "text/x-credits", "text/plain" );
			SubClasses.Add( "text/x-csrc", "text/plain" );
			SubClasses.Add( "text/x-csharp", "text/plain" );
			SubClasses.Add( "text/x-dcl", "text/plain" );
			SubClasses.Add( "text/x-dsrc", "text/plain" );
			SubClasses.Add( "text/x-dtd", "text/plain" );
			SubClasses.Add( "text/x-emacs-lisp", "text/plain" );
			SubClasses.Add( "text/x-fortran", "text/plain" );
			SubClasses.Add( "text/x-gettext-translation", "text/plain" );
			SubClasses.Add( "text/x-gettext-translation-template", "text/plain" );
			SubClasses.Add( "text/x-gtkrc", "text/plain" );
			SubClasses.Add( "text/x-haskell", "text/plain" );
			SubClasses.Add( "text/x-idl", "text/plain" );
			SubClasses.Add( "text/x-install", "text/plain" );
			SubClasses.Add( "text/x-java", "text/plain" );
			SubClasses.Add( "text/x-ksysv-log", "text/plain" );
			SubClasses.Add( "text/x-literate-haskell", "text/plain" );
			SubClasses.Add( "text/x-log", "text/plain" );
			SubClasses.Add( "text/x-makefile", "text/plain" );
			SubClasses.Add( "text/x-moc", "text/plain" );
			SubClasses.Add( "text/x-objcsrc", "text/plain" );
			SubClasses.Add( "text/x-pascal", "text/plain" );
			SubClasses.Add( "text/x-patch", "text/plain" );
			SubClasses.Add( "text/x-python", "application/x-executable" );
			SubClasses.Add( "text/x-python", "text/plain" );
			SubClasses.Add( "text/x-readme", "text/plain" );
			SubClasses.Add( "text/x-scheme", "text/plain" );
			SubClasses.Add( "text/x-setext", "text/plain" );
			SubClasses.Add( "text/x-sql", "text/plain" );
			SubClasses.Add( "text/x-tcl", "text/plain" );
			SubClasses.Add( "text/x-tex", "text/plain" );
			SubClasses.Add( "text/x-texinfo", "text/plain" );
			SubClasses.Add( "application/xml", "text/plain" );
			SubClasses.Add( "text/xml", "text/plain" );
			SubClasses.Add( "application/x-gdesklets-display", "text/xml" );

			GlobalPatternsShort.Add( ".bfproject", "application/bluefish-project" );
			GlobalPatternsShort.Add( ".ez", "application/andrew-inset" );
			GlobalPatternsShort.Add( ".ai", "application/illustrator" );
			GlobalPatternsShort.Add( ".nb", "application/mathematica" );
			GlobalPatternsShort.Add( ".bin", "application/octet-stream" );
			GlobalPatternsShort.Add( ".oda", "application/oda" );
			GlobalPatternsShort.Add( ".pdf", "application/pdf" );
			GlobalPatternsShort.Add( ".pgp", "application/pgp" );
			GlobalPatternsShort.Add( ".p7s", "application/pkcs7-signature" );
			GlobalPatternsShort.Add( ".ps", "application/postscript" );
			GlobalPatternsShort.Add( ".rtf", "application/rtf" );
			GlobalPatternsShort.Add( ".smil", "application/smil" );
			GlobalPatternsShort.Add( ".smi", "application/smil" );
			GlobalPatternsShort.Add( ".sml", "application/smil" );
			GlobalPatternsShort.Add( ".kino", "application/smil" );
			GlobalPatternsShort.Add( ".sit", "application/stuffit" );
			GlobalPatternsShort.Add( ".ged", "application/x-gedcom" );
			GlobalPatternsShort.Add( ".gedcom", "application/x-gedcom" );
			GlobalPatternsShort.Add( ".cdr", "application/vnd.corel-draw" );
			GlobalPatternsShort.Add( ".hpgl", "application/vnd.hp-hpgl" );
			GlobalPatternsShort.Add( ".pcl", "application/vnd.hp-pcl" );
			GlobalPatternsShort.Add( ".123", "application/vnd.lotus-1-2-3" );
			GlobalPatternsShort.Add( ".wk1", "application/vnd.lotus-1-2-3" );
			GlobalPatternsShort.Add( ".wk3", "application/vnd.lotus-1-2-3" );
			GlobalPatternsShort.Add( ".wk4", "application/vnd.lotus-1-2-3" );
			GlobalPatternsShort.Add( ".wks", "application/vnd.lotus-1-2-3" );
			GlobalPatternsShort.Add( ".xul", "application/vnd.mozilla.xul+xml" );
			GlobalPatternsShort.Add( ".mdb", "application/vnd.ms-access" );
			GlobalPatternsShort.Add( ".xls", "application/vnd.ms-excel" );
			GlobalPatternsShort.Add( ".xlc", "application/vnd.ms-excel" );
			GlobalPatternsShort.Add( ".xll", "application/vnd.ms-excel" );
			GlobalPatternsShort.Add( ".xlm", "application/vnd.ms-excel" );
			GlobalPatternsShort.Add( ".xlw", "application/vnd.ms-excel" );
			GlobalPatternsShort.Add( ".xla", "application/vnd.ms-excel" );
			GlobalPatternsShort.Add( ".xlt", "application/vnd.ms-excel" );
			GlobalPatternsShort.Add( ".xld", "application/vnd.ms-excel" );
			GlobalPatternsShort.Add( ".ppz", "application/vnd.ms-powerpoint" );
			GlobalPatternsShort.Add( ".ppt", "application/vnd.ms-powerpoint" );
			GlobalPatternsShort.Add( ".pps", "application/vnd.ms-powerpoint" );
			GlobalPatternsShort.Add( ".pot", "application/vnd.ms-powerpoint" );
			GlobalPatternsShort.Add( ".doc", "application/msword" );
			GlobalPatternsShort.Add( ".pdb", "application/vnd.palm" );
			GlobalPatternsShort.Add( ".sdc", "application/vnd.stardivision.calc" );
			GlobalPatternsShort.Add( ".sds", "application/vnd.stardivision.chart" );
			GlobalPatternsShort.Add( ".sda", "application/vnd.stardivision.draw" );
			GlobalPatternsShort.Add( ".sdd", "application/vnd.stardivision.impress" );
			GlobalPatternsShort.Add( ".sdp", "application/vnd.stardivision.impress" );
			GlobalPatternsShort.Add( ".smd", "application/vnd.stardivision.mail" );
			GlobalPatternsShort.Add( ".smf", "application/vnd.stardivision.math" );
			GlobalPatternsShort.Add( ".sdw", "application/vnd.stardivision.writer" );
			GlobalPatternsShort.Add( ".vor", "application/vnd.stardivision.writer" );
			GlobalPatternsShort.Add( ".sgl", "application/vnd.stardivision.writer" );
			GlobalPatternsShort.Add( ".sxc", "application/vnd.sun.xml.calc" );
			GlobalPatternsShort.Add( ".stc", "application/vnd.sun.xml.calc.template" );
			GlobalPatternsShort.Add( ".sxd", "application/vnd.sun.xml.draw" );
			GlobalPatternsShort.Add( ".std", "application/vnd.sun.xml.draw.template" );
			GlobalPatternsShort.Add( ".sxi", "application/vnd.sun.xml.impress" );
			GlobalPatternsShort.Add( ".sti", "application/vnd.sun.xml.impress.template" );
			GlobalPatternsShort.Add( ".sxm", "application/vnd.sun.xml.math" );
			GlobalPatternsShort.Add( ".sxw", "application/vnd.sun.xml.writer" );
			GlobalPatternsShort.Add( ".sxg", "application/vnd.sun.xml.writer.global" );
			GlobalPatternsShort.Add( ".stw", "application/vnd.sun.xml.writer.template" );
			GlobalPatternsShort.Add( ".odt", "application/vnd.oasis.opendocument.text" );
			GlobalPatternsShort.Add( ".ott", "application/vnd.oasis.opendocument.text-template" );
			GlobalPatternsShort.Add( ".oth", "application/vnd.oasis.opendocument.text-web" );
			GlobalPatternsShort.Add( ".odm", "application/vnd.oasis.opendocument.text-master" );
			GlobalPatternsShort.Add( ".odg", "application/vnd.oasis.opendocument.graphics" );
			GlobalPatternsShort.Add( ".otg", "application/vnd.oasis.opendocument.graphics-template" );
			GlobalPatternsShort.Add( ".odp", "application/vnd.oasis.opendocument.presentation" );
			GlobalPatternsShort.Add( ".otp", "application/vnd.oasis.opendocument.presentation-template" );
			GlobalPatternsShort.Add( ".ods", "application/vnd.oasis.opendocument.spreadsheet" );
			GlobalPatternsShort.Add( ".ots", "application/vnd.oasis.opendocument.spreadsheet-template" );
			GlobalPatternsShort.Add( ".odc", "application/vnd.oasis.opendocument.chart" );
			GlobalPatternsShort.Add( ".odf", "application/vnd.oasis.opendocument.formula" );
			GlobalPatternsShort.Add( ".odb", "application/vnd.oasis.opendocument.database" );
			GlobalPatternsShort.Add( ".odi", "application/vnd.oasis.opendocument.image" );
			GlobalPatternsShort.Add( ".wpd", "application/vnd.wordperfect" );
			GlobalPatternsShort.Add( ".xbel", "application/x-xbel" );
			GlobalPatternsShort.Add( ".7z", "application/x-7z-compressed" );
			GlobalPatternsShort.Add( ".abw", "application/x-abiword" );
			GlobalPatternsLong.Add( ".abw.CRASHED", "application/x-abiword" );
			GlobalPatternsLong.Add( ".abw.gz", "application/x-abiword" );
			GlobalPatternsShort.Add( ".zabw", "application/x-abiword" );
			GlobalPatternsShort.Add( ".cue", "application/x-cue" );
			GlobalPatternsShort.Add( ".sam", "application/x-amipro" );
			GlobalPatternsShort.Add( ".as", "application/x-applix-spreadsheet" );
			GlobalPatternsShort.Add( ".aw", "application/x-applix-word" );
			GlobalPatternsShort.Add( ".a", "application/x-archive" );
			GlobalPatternsShort.Add( ".arj", "application/x-arj" );
			GlobalPatternsShort.Add( ".asp", "application/x-asp" );
			GlobalPatternsShort.Add( ".bcpio", "application/x-bcpio" );
			GlobalPatternsShort.Add( ".torrent", "application/x-bittorrent" );
			GlobalPatternsShort.Add( ".blender", "application/x-blender" );
			GlobalPatternsShort.Add( ".blend", "application/x-blender" );
			GlobalPatternsShort.Add( ".BLEND", "application/x-blender" );
			GlobalPatternsShort.Add( ".bz", "application/x-bzip" );
			GlobalPatternsShort.Add( ".bz2", "application/x-bzip" );
			GlobalPatternsLong.Add( ".tar.bz", "application/x-bzip-compressed-tar" );
			GlobalPatternsLong.Add( ".tar.bz2", "application/x-bzip-compressed-tar" );
			GlobalPatternsShort.Add( ".iso", "application/x-cd-image" );
			GlobalPatternsShort.Add( ".cgi", "application/x-cgi" );
			GlobalPatternsShort.Add( ".pgn", "application/x-chess-pgn" );
			GlobalPatternsShort.Add( ".chm", "application/x-chm" );
			GlobalPatternsShort.Add( ".Z", "application/x-compress" );
			GlobalPatternsLong.Add( ".tar.gz", "application/x-compressed-tar" );
			GlobalPatternsShort.Add( ".tgz", "application/x-compressed-tar" );
			GlobalLiterals.Add( "core", "application/x-core" );
			GlobalPatternsShort.Add( ".cpio", "application/x-cpio" );
			GlobalPatternsLong.Add( ".cpio.gz", "application/x-cpio-compressed" );
			GlobalPatternsShort.Add( ".csh", "application/x-csh" );
			GlobalPatternsShort.Add( ".dbf", "application/x-dbf" );
			GlobalPatternsShort.Add( ".dc", "application/x-dc-rom" );
			GlobalPatternsShort.Add( ".deb", "application/x-deb" );
			GlobalPatternsShort.Add( ".ui", "application/x-designer" );
			GlobalPatternsShort.Add( ".desktop", "application/x-desktop" );
			GlobalPatternsShort.Add( ".kdelnk", "application/x-desktop" );
			GlobalPatternsShort.Add( ".dia", "application/x-dia-diagram" );
			GlobalPatternsShort.Add( ".dvi", "application/x-dvi" );
			GlobalPatternsShort.Add( ".etheme", "application/x-e-theme" );
			GlobalPatternsShort.Add( ".egon", "application/x-egon" );
			GlobalPatternsShort.Add( ".exe", "application/x-executable" );
			GlobalPatternsShort.Add( ".pfa", "application/x-font-type1" );
			GlobalPatternsShort.Add( ".pfb", "application/x-font-type1" );
			GlobalPatternsShort.Add( ".gsf", "application/x-font-type1" );
			GlobalPatternsLong.Add( ".pcf.Z", "application/x-font-type1" );
			GlobalPatternsShort.Add( ".afm", "application/x-font-afm" );
			GlobalPatternsShort.Add( ".bdf", "application/x-font-bdf" );
			GlobalPatternsShort.Add( ".psf", "application/x-font-linux-psf" );
			GlobalPatternsShort.Add( ".pcf", "application/x-font-pcf" );
			GlobalPatternsShort.Add( ".spd", "application/x-font-speedo" );
			GlobalPatternsShort.Add( ".ttf", "application/x-font-ttf" );
			GlobalPatternsShort.Add( ".gb", "application/x-gameboy-rom" );
			GlobalPatternsShort.Add( ".gen", "application/x-genesis-rom" );
			GlobalPatternsShort.Add( ".md", "application/x-genesis-rom" );
			GlobalPatternsShort.Add( ".gmo", "application/x-gettext-translation" );
			GlobalPatternsShort.Add( ".glade", "application/x-glade" );
			GlobalPatternsShort.Add( ".gnucash", "application/x-gnucash" );
			GlobalPatternsShort.Add( ".gnc", "application/x-gnucash" );
			GlobalPatternsShort.Add( ".xac", "application/x-gnucash" );
			GlobalPatternsShort.Add( ".gnumeric", "application/x-gnumeric" );
			GlobalPatternsShort.Add( ".gra", "application/x-graphite" );
			GlobalPatternsShort.Add( ".gtar", "application/x-gtar" );
			GlobalPatternsShort.Add( ".gz", "application/x-gzip" );
			GlobalPatternsLong.Add( ".ps.gz", "application/x-gzpostscript" );
			GlobalPatternsShort.Add( ".hdf", "application/x-hdf" );
			GlobalPatternsShort.Add( ".jar", "application/x-jar" );
			GlobalPatternsShort.Add( ".class", "application/x-java" );
			GlobalPatternsShort.Add( ".jnlp", "application/x-java-jnlp-file" );
			GlobalPatternsShort.Add( ".js", "application/x-javascript" );
			GlobalPatternsShort.Add( ".jpr", "application/x-jbuilder-project" );
			GlobalPatternsShort.Add( ".jpx", "application/x-jbuilder-project" );
			GlobalPatternsShort.Add( ".karbon", "application/x-karbon" );
			GlobalPatternsShort.Add( ".chrt", "application/x-kchart" );
			GlobalPatternsShort.Add( ".kfo", "application/x-kformula" );
			GlobalPatternsShort.Add( ".kil", "application/x-killustrator" );
			GlobalPatternsShort.Add( ".flw", "application/x-kivio" );
			GlobalPatternsShort.Add( ".kon", "application/x-kontour" );
			GlobalPatternsShort.Add( ".kpm", "application/x-kpovmodeler" );
			GlobalPatternsShort.Add( ".kpr", "application/x-kpresenter" );
			GlobalPatternsShort.Add( ".kpt", "application/x-kpresenter" );
			GlobalPatternsShort.Add( ".kra", "application/x-krita" );
			GlobalPatternsShort.Add( ".ksp", "application/x-kspread" );
			GlobalPatternsShort.Add( ".kud", "application/x-kugar" );
			GlobalPatternsShort.Add( ".kwd", "application/x-kword" );
			GlobalPatternsShort.Add( ".kwt", "application/x-kword" );
			GlobalPatternsShort.Add( ".lha", "application/x-lha" );
			GlobalPatternsShort.Add( ".lzh", "application/x-lha" );
			GlobalPatternsShort.Add( ".lhz", "application/x-lhz" );
			GlobalPatternsShort.Add( ".ts", "application/x-linguist" );
			GlobalPatternsShort.Add( ".lyx", "text/x-lyx" );
			GlobalPatternsShort.Add( ".lzo", "application/x-lzop" );
			GlobalPatternsShort.Add( ".mgp", "application/x-magicpoint" );
			GlobalPatternsShort.Add( ".mkv", "application/x-matroska" );
			GlobalPatternsShort.Add( ".mif", "application/x-mif" );
			GlobalPatternsShort.Add( ".exe", "application/x-ms-dos-executable" );
			GlobalPatternsShort.Add( ".wri", "application/x-mswrite" );
			GlobalPatternsShort.Add( ".msx", "application/x-msx-rom" );
			GlobalPatternsShort.Add( ".n64", "application/x-n64-rom" );
			GlobalPatternsShort.Add( ".nes", "application/x-nes-rom" );
			GlobalPatternsShort.Add( ".cdf", "application/x-netcdf" );
			GlobalPatternsShort.Add( ".nc", "application/x-netcdf" );
			GlobalPatternsShort.Add( ".o", "application/x-object" );
			GlobalPatternsShort.Add( ".ogg", "application/ogg" );
			GlobalPatternsShort.Add( ".oleo", "application/x-oleo" );
			GlobalPatternsShort.Add( ".PAR2", "application/x-par2" );
			GlobalPatternsShort.Add( ".par2", "application/x-par2" );
			GlobalPatternsShort.Add( ".pl", "application/x-perl" );
			GlobalPatternsShort.Add( ".pm", "application/x-perl" );
			GlobalPatternsShort.Add( ".al", "application/x-perl" );
			GlobalPatternsShort.Add( ".perl", "application/x-perl" );
			GlobalPatternsShort.Add( ".php", "application/x-php" );
			GlobalPatternsShort.Add( ".php3", "application/x-php" );
			GlobalPatternsShort.Add( ".php4", "application/x-php" );
			GlobalPatternsShort.Add( ".p12", "application/x-pkcs12" );
			GlobalPatternsShort.Add( ".pfx", "application/x-pkcs12" );
			GlobalLiterals.Add( "gmon.out", "application/x-profile" );
			GlobalPatternsShort.Add( ".pw", "application/x-pw" );
			GlobalPatternsShort.Add( ".pyc", "application/x-python-bytecode" );
			GlobalPatternsShort.Add( ".pyo", "application/x-python-bytecode" );
			GlobalPatternsShort.Add( ".wb1", "application/x-quattropro" );
			GlobalPatternsShort.Add( ".wb2", "application/x-quattropro" );
			GlobalPatternsShort.Add( ".wb3", "application/x-quattropro" );
			GlobalPatternsShort.Add( ".qif", "application/x-qw" );
			GlobalPatternsShort.Add( ".rar", "application/x-rar" );
			GlobalPatternsShort.Add( ".rej", "application/x-reject" );
			GlobalPatternsShort.Add( ".rpm", "application/x-rpm" );
			GlobalPatternsShort.Add( ".rb", "application/x-ruby" );
			GlobalPatternsShort.Add( ".shar", "application/x-shar" );
			GlobalPatternsShort.Add( ".la", "application/x-shared-library-la" );
			GlobalPatternsShort.Add( ".so", "application/x-sharedlib" );
			GlobalPatternsShort.Add( ".sh", "application/x-shellscript" );
			GlobalPatternsShort.Add( ".swf", "application/x-shockwave-flash" );
			GlobalPatternsShort.Add( ".siag", "application/x-siag" );
			GlobalPatternsShort.Add( ".sms", "application/x-sms-rom" );
			GlobalPatternsShort.Add( ".gg", "application/x-sms-rom" );
			GlobalPatternsShort.Add( ".sv4cpio", "application/x-sv4cpio" );
			GlobalPatternsShort.Add( ".sv4crc", "application/x-sv4crc" );
			GlobalPatternsShort.Add( ".tar", "application/x-tar" );
			GlobalPatternsLong.Add( ".tar.Z", "application/x-tarz" );
			GlobalPatternsShort.Add( ".gf", "application/x-tex-gf" );
			GlobalSufPref.Add( "*pk", "application/x-tex-pk" );
			GlobalPatternsShort.Add( ".obj", "application/x-tgif" );
			GlobalPatternsShort.Add( ".theme", "application/x-theme" );
			GlobalSufPref.Add( "*~", "application/x-trash" );
			GlobalSufPref.Add( "*%", "application/x-trash" );
			GlobalPatternsShort.Add( ".bak", "application/x-trash" );
			GlobalPatternsShort.Add( ".old", "application/x-trash" );
			GlobalPatternsShort.Add( ".sik", "application/x-trash" );
			GlobalPatternsShort.Add( ".tr", "application/x-troff" );
			GlobalPatternsShort.Add( ".roff", "application/x-troff" );
			GlobalPatternsShort.Add( ".t", "application/x-troff" );
			GlobalPatternsShort.Add( ".man", "application/x-troff-man" );
			GlobalPatternsLong.Add( ".tar.lzo", "application/x-tzo" );
			GlobalPatternsShort.Add( ".tzo", "application/x-tzo" );
			GlobalPatternsShort.Add( ".ustar", "application/x-ustar" );
			GlobalPatternsShort.Add( ".src", "application/x-wais-source" );
			GlobalPatternsShort.Add( ".wpg", "application/x-wpg" );
			GlobalPatternsShort.Add( ".der", "application/x-x509-ca-cert" );
			GlobalPatternsShort.Add( ".cer", "application/x-x509-ca-cert" );
			GlobalPatternsShort.Add( ".crt", "application/x-x509-ca-cert" );
			GlobalPatternsShort.Add( ".cert", "application/x-x509-ca-cert" );
			GlobalPatternsShort.Add( ".pem", "application/x-x509-ca-cert" );
			GlobalPatternsShort.Add( ".zoo", "application/x-zoo" );
			GlobalPatternsShort.Add( ".xhtml", "application/xhtml+xml" );
			GlobalPatternsShort.Add( ".zip", "application/zip" );
			GlobalPatternsShort.Add( ".ac3", "audio/ac3" );
			GlobalPatternsShort.Add( ".au", "audio/basic" );
			GlobalPatternsShort.Add( ".snd", "audio/basic" );
			GlobalPatternsShort.Add( ".sid", "audio/prs.sid" );
			GlobalPatternsShort.Add( ".psid", "audio/prs.sid" );
			GlobalPatternsShort.Add( ".aiff", "audio/x-aiff" );
			GlobalPatternsShort.Add( ".aif", "audio/x-aiff" );
			GlobalPatternsShort.Add( ".aifc", "audio/x-aiff" );
			GlobalPatternsShort.Add( ".it", "audio/x-it" );
			GlobalPatternsShort.Add( ".flac", "audio/x-flac" );
			GlobalPatternsShort.Add( ".mid", "audio/midi" );
			GlobalPatternsShort.Add( ".midi", "audio/midi" );
			GlobalPatternsShort.Add( ".m4a", "audio/mp4" );
			GlobalPatternsShort.Add( ".mp4", "video/mp4" );
			GlobalPatternsShort.Add( ".mod", "audio/x-mod" );
			GlobalPatternsShort.Add( ".ult", "audio/x-mod" );
			GlobalPatternsShort.Add( ".uni", "audio/x-mod" );
			GlobalPatternsShort.Add( ".XM", "audio/x-mod" );
			GlobalPatternsShort.Add( ".m15", "audio/x-mod" );
			GlobalPatternsShort.Add( ".mtm", "audio/x-mod" );
			GlobalPatternsShort.Add( ".669", "audio/x-mod" );
			GlobalPatternsShort.Add( ".mp3", "audio/mpeg" );
			GlobalPatternsShort.Add( ".m3u", "audio/x-mpegurl" );
			GlobalPatternsShort.Add( ".ra", "audio/x-pn-realaudio" );
			GlobalPatternsShort.Add( ".ram", "audio/x-pn-realaudio" );
			GlobalPatternsShort.Add( ".rm", "audio/x-pn-realaudio" );
			GlobalPatternsShort.Add( ".rmvb", "audio/x-pn-realaudio" );
			GlobalPatternsShort.Add( ".s3m", "audio/x-s3m" );
			GlobalPatternsShort.Add( ".pls", "audio/x-scpls" );
			GlobalPatternsShort.Add( ".stm", "audio/x-stm" );
			GlobalPatternsShort.Add( ".voc", "audio/x-voc" );
			GlobalPatternsShort.Add( ".wav", "audio/x-wav" );
			GlobalPatternsShort.Add( ".xi", "audio/x-xi" );
			GlobalPatternsShort.Add( ".xm", "audio/x-xm" );
			GlobalPatternsShort.Add( ".bmp", "image/bmp" );
			GlobalPatternsShort.Add( ".cgm", "image/cgm" );
			GlobalPatternsShort.Add( ".g3", "image/fax-g3" );
			GlobalPatternsShort.Add( ".gif", "image/gif" );
			GlobalPatternsShort.Add( ".ief", "image/ief" );
			GlobalPatternsShort.Add( ".jpeg", "image/jpeg" );
			GlobalPatternsShort.Add( ".jpg", "image/jpeg" );
			GlobalPatternsShort.Add( ".jpe", "image/jpeg" );
			GlobalPatternsShort.Add( ".jp2", "image/jpeg2000" );
			GlobalPatternsShort.Add( ".pict", "image/x-pict" );
			GlobalPatternsShort.Add( ".pict1", "image/x-pict" );
			GlobalPatternsShort.Add( ".pict2", "image/x-pict" );
			GlobalPatternsShort.Add( ".png", "image/png" );
			GlobalPatternsShort.Add( ".rle", "image/rle" );
			GlobalPatternsShort.Add( ".svg", "image/svg+xml" );
			GlobalPatternsShort.Add( ".tif", "image/tiff" );
			GlobalPatternsShort.Add( ".tiff", "image/tiff" );
			GlobalPatternsShort.Add( ".dwg", "image/vnd.dwg" );
			GlobalPatternsShort.Add( ".dxf", "image/vnd.dxf" );
			GlobalPatternsShort.Add( ".3ds", "image/x-3ds" );
			GlobalPatternsShort.Add( ".ag", "image/x-applix-graphics" );
			GlobalPatternsShort.Add( ".ras", "image/x-cmu-raster" );
			GlobalPatternsLong.Add( ".xcf.gz", "image/x-compressed-xcf" );
			GlobalPatternsLong.Add( ".xcf.bz2", "image/x-compressed-xcf" );
			GlobalPatternsShort.Add( ".dcm", "application/dicom" );
			GlobalPatternsShort.Add( ".djvu", "image/vnd.djvu" );
			GlobalPatternsShort.Add( ".djv", "image/vnd.djvu" );
			GlobalPatternsShort.Add( ".eps", "image/x-eps" );
			GlobalPatternsShort.Add( ".epsi", "image/x-eps" );
			GlobalPatternsShort.Add( ".epsf", "image/x-eps" );
			GlobalPatternsShort.Add( ".fits", "image/x-fits" );
			GlobalPatternsShort.Add( ".icb", "image/x-icb" );
			GlobalPatternsShort.Add( ".ico", "image/x-ico" );
			GlobalPatternsShort.Add( ".iff", "image/x-iff" );
			GlobalPatternsShort.Add( ".ilbm", "image/x-ilbm" );
			GlobalPatternsShort.Add( ".jng", "image/x-jng" );
			GlobalPatternsShort.Add( ".lwo", "image/x-lwo" );
			GlobalPatternsShort.Add( ".lwob", "image/x-lwo" );
			GlobalPatternsShort.Add( ".lws", "image/x-lws" );
			GlobalPatternsShort.Add( ".msod", "image/x-msod" );
			GlobalPatternsShort.Add( ".pcd", "image/x-photo-cd" );
			GlobalPatternsShort.Add( ".pnm", "image/x-portable-anymap" );
			GlobalPatternsShort.Add( ".pbm", "image/x-portable-bitmap" );
			GlobalPatternsShort.Add( ".pgm", "image/x-portable-graymap" );
			GlobalPatternsShort.Add( ".ppm", "image/x-portable-pixmap" );
			GlobalPatternsShort.Add( ".psd", "image/x-psd" );
			GlobalPatternsShort.Add( ".rgb", "image/x-rgb" );
			GlobalPatternsShort.Add( ".sgi", "image/x-sgi" );
			GlobalPatternsShort.Add( ".sun", "image/x-sun-raster" );
			GlobalPatternsShort.Add( ".tga", "image/x-tga" );
			GlobalPatternsShort.Add( ".cur", "image/x-win-bitmap" );
			GlobalPatternsShort.Add( ".wmf", "image/x-wmf" );
			GlobalPatternsShort.Add( ".xbm", "image/x-xbitmap" );
			GlobalPatternsShort.Add( ".xcf", "image/x-xcf" );
			GlobalPatternsShort.Add( ".fig", "image/x-xfig" );
			GlobalPatternsShort.Add( ".xpm", "image/x-xpixmap" );
			GlobalPatternsShort.Add( ".xwd", "image/x-xwindowdump" );
			GlobalLiterals.Add( "RMAIL", "message/x-gnu-rmail" );
			GlobalPatternsShort.Add( ".wrl", "model/vrml" );
			GlobalPatternsShort.Add( ".vcs", "text/calendar" );
			GlobalPatternsShort.Add( ".ics", "text/calendar" );
			GlobalPatternsShort.Add( ".css", "text/css" );
			GlobalPatternsShort.Add( ".CSSL", "text/css" );
			GlobalPatternsShort.Add( ".vcf", "text/directory" );
			GlobalPatternsShort.Add( ".vct", "text/directory" );
			GlobalPatternsShort.Add( ".gcrd", "text/directory" );
			GlobalPatternsShort.Add( ".mml", "text/mathml" );
			GlobalPatternsShort.Add( ".txt", "text/plain" );
			GlobalPatternsShort.Add( ".asc", "text/plain" );
			GlobalPatternsShort.Add( ".rdf", "text/rdf" );
			GlobalPatternsShort.Add( ".rtx", "text/richtext" );
			GlobalPatternsShort.Add( ".rss", "text/rss" );
			GlobalPatternsShort.Add( ".sgml", "text/sgml" );
			GlobalPatternsShort.Add( ".sgm", "text/sgml" );
			GlobalPatternsShort.Add( ".sylk", "text/spreadsheet" );
			GlobalPatternsShort.Add( ".slk", "text/spreadsheet" );
			GlobalPatternsShort.Add( ".tsv", "text/tab-separated-values" );
			GlobalPatternsShort.Add( ".wml", "text/vnd.wap.wml" );
			GlobalPatternsShort.Add( ".adb", "text/x-adasrc" );
			GlobalPatternsShort.Add( ".ads", "text/x-adasrc" );
			GlobalLiterals.Add( "AUTHORS", "text/x-authors" );
			GlobalPatternsShort.Add( ".bib", "text/x-bibtex" );
			GlobalPatternsShort.Add( ".hh", "text/x-c++hdr" );
			GlobalPatternsShort.Add( ".hpp", "text/x-c++hdr" );
			GlobalPatternsShort.Add( ".cpp", "text/x-c++src" );
			GlobalPatternsShort.Add( ".cxx", "text/x-c++src" );
			GlobalPatternsShort.Add( ".cc", "text/x-c++src" );
			GlobalPatternsShort.Add( ".C", "text/x-c++src" );
			GlobalPatternsShort.Add( ".c++", "text/x-c++src" );
			GlobalPatternsShort.Add( ".h", "text/x-chdr" );
			GlobalPatternsShort.Add( ".h++", "text/x-chdr" );
			GlobalPatternsShort.Add( ".hp", "text/x-chdr" );
			GlobalPatternsShort.Add( ".csv", "text/x-comma-separated-values" );
			GlobalLiterals.Add( "COPYING", "text/x-copying" );
			GlobalLiterals.Add( "CREDITS", "text/x-credits" );
			GlobalPatternsShort.Add( ".c", "text/x-csrc" );
			GlobalPatternsShort.Add( ".cs", "text/x-csharp" );
			GlobalPatternsShort.Add( ".dcl", "text/x-dcl" );
			GlobalPatternsShort.Add( ".dsl", "text/x-dsl" );
			GlobalPatternsShort.Add( ".d", "text/x-dsrc" );
			GlobalPatternsShort.Add( ".dtd", "text/x-dtd" );
			GlobalPatternsShort.Add( ".el", "text/x-emacs-lisp" );
			GlobalPatternsShort.Add( ".f", "text/x-fortran" );
			GlobalPatternsShort.Add( ".f9[05]", "text/x-fortran" );
			GlobalPatternsShort.Add( ".for", "text/x-fortran" );
			GlobalPatternsShort.Add( ".po", "text/x-gettext-translation" );
			GlobalPatternsShort.Add( ".pot", "text/x-gettext-translation-template" );
			GlobalPatternsShort.Add( ".html", "text/html" );
			GlobalPatternsShort.Add( ".htm", "text/html" );
			GlobalLiterals.Add( "gtkrc", "text/x-gtkrc" );
			GlobalLiterals.Add( ".gtkrc", "text/x-gtkrc" );
			GlobalPatternsShort.Add( ".hs", "text/x-haskell" );
			GlobalPatternsShort.Add( ".idl", "text/x-idl" );
			GlobalLiterals.Add( "INSTALL", "text/x-install" );
			GlobalPatternsShort.Add( ".java", "text/x-java" );
			GlobalPatternsShort.Add( ".lhs", "text/x-literate-haskell" );
			GlobalPatternsShort.Add( ".log", "text/x-log" );
			GlobalSufPref.Add( "makefile*", "text/x-makefile" );
			GlobalLiterals.Add( "[Mm]akefile", "text/x-makefile" );
			GlobalPatternsShort.Add( ".moc", "text/x-moc" );
			GlobalPatternsShort.Add( ".m", "text/x-objcsrc" );
			GlobalPatternsShort.Add( ".p", "text/x-pascal" );
			GlobalPatternsShort.Add( ".pas", "text/x-pascal" );
			GlobalPatternsShort.Add( ".diff", "text/x-patch" );
			GlobalPatternsShort.Add( ".patch", "text/x-patch" );
			GlobalPatternsShort.Add( ".py", "text/x-python" );
			GlobalSufPref.Add( "README*", "text/x-readme" );
			GlobalPatternsShort.Add( ".scm", "text/x-scheme" );
			GlobalPatternsShort.Add( ".etx", "text/x-setext" );
			GlobalPatternsShort.Add( ".sql", "text/x-sql" );
			GlobalPatternsShort.Add( ".tcl", "text/x-tcl" );
			GlobalPatternsShort.Add( ".tk", "text/x-tcl" );
			GlobalPatternsShort.Add( ".tex", "text/x-tex" );
			GlobalPatternsShort.Add( ".ltx", "text/x-tex" );
			GlobalPatternsShort.Add( ".sty", "text/x-tex" );
			GlobalPatternsShort.Add( ".cls", "text/x-tex" );
			GlobalPatternsShort.Add( ".texi", "text/x-texinfo" );
			GlobalPatternsShort.Add( ".texinfo", "text/x-texinfo" );
			GlobalPatternsShort.Add( ".me", "text/x-troff-me" );
			GlobalPatternsShort.Add( ".mm", "text/x-troff-mm" );
			GlobalPatternsShort.Add( ".ms", "text/x-troff-ms" );
			GlobalPatternsShort.Add( ".uil", "text/x-uil" );
			GlobalPatternsShort.Add( ".uri", "text/x-uri" );
			GlobalPatternsShort.Add( ".url", "text/x-uri" );
			GlobalPatternsShort.Add( ".xmi", "text/x-xmi" );
			GlobalPatternsShort.Add( ".fo", "text/x-xslfo" );
			GlobalPatternsShort.Add( ".xslfo", "text/x-xslfo" );
			GlobalPatternsShort.Add( ".xslt", "application/xml" );
			GlobalPatternsShort.Add( ".xsl", "application/xml" );
			GlobalPatternsShort.Add( ".xml", "text/xml" );
			GlobalPatternsShort.Add( ".mpeg", "video/mpeg" );
			GlobalPatternsShort.Add( ".mpg", "video/mpeg" );
			GlobalPatternsShort.Add( ".mp2", "video/mpeg" );
			GlobalPatternsShort.Add( ".mpe", "video/mpeg" );
			GlobalPatternsShort.Add( ".vob", "video/mpeg" );
			GlobalPatternsShort.Add( ".dat", "video/mpeg" );
			GlobalPatternsShort.Add( ".qt", "video/quicktime" );
			GlobalPatternsShort.Add( ".mov", "video/quicktime" );
			GlobalPatternsShort.Add( ".moov", "video/quicktime" );
			GlobalPatternsShort.Add( ".qtvr", "video/quicktime" );
			GlobalPatternsShort.Add( ".anim[1-9j]", "video/x-anim" );
			GlobalPatternsShort.Add( ".fli", "video/x-flic" );
			GlobalPatternsShort.Add( ".flc", "video/x-flic" );
			GlobalPatternsShort.Add( ".mng", "video/x-mng" );
			GlobalPatternsShort.Add( ".asf", "video/x-ms-asf" );
			GlobalPatternsShort.Add( ".asx", "video/x-ms-asf" );
			GlobalPatternsShort.Add( ".wmv", "video/x-ms-wmv" );
			GlobalPatternsShort.Add( ".avi", "video/x-msvideo" );
			GlobalPatternsShort.Add( ".nsv", "video/x-nsv" );
			GlobalPatternsShort.Add( ".NSV", "video/x-nsv" );
			GlobalPatternsShort.Add( ".movie", "video/x-sgi-movie" );
			GlobalPatternsShort.Add( ".crw", "application/x-crw" );
			GlobalPatternsShort.Add( ".cr2", "application/x-crw" );
			GlobalPatternsShort.Add( ".pln", "application/x-planperfect" );
			GlobalPatternsShort.Add( ".mps", "application/x-mps" );
			GlobalPatternsShort.Add( ".display", "application/x-gdesklets-display" );
			GlobalPatternsShort.Add( ".il", "text/x-msil" );
			GlobalPatternsShort.Add( ".n", "text/x-nemerle" );
			GlobalPatternsShort.Add( ".vb", "text/x-vb" );
			GlobalPatternsShort.Add( ".js", "text/x-js" );
			GlobalPatternsShort.Add( ".aspx", "application/x-aspx" );
			GlobalPatternsShort.Add( ".ashx", "application/x-ashx" );
			GlobalPatternsShort.Add( ".ascx", "application/x-ascx" );
			GlobalPatternsShort.Add( ".asix", "application/x-asix" );
			GlobalPatternsShort.Add( ".axd", "application/x-axd" );
			GlobalLiterals.Add( "web.config", "application/x-web-config" );
			GlobalLiterals.Add( "machine.config", "application/x-machine-config" );
			GlobalPatternsShort.Add( ".config", "application/x-config" );
			GlobalPatternsShort.Add( ".master", "application/x-master-page" );
			GlobalPatternsShort.Add( ".resources", "application/x-resources" );
			GlobalPatternsShort.Add( ".resx", "application/x-resourcesx" );
			GlobalPatternsShort.Add( ".rem", "application/x-remoting" );
			GlobalPatternsShort.Add( ".soap", "application/x-soap-remoting" );
			GlobalPatternsShort.Add( ".asmx", "application/x-asmx" );
			GlobalPatternsShort.Add( ".prjx", "application/x-prjx" );
			GlobalPatternsShort.Add( ".cmbx", "application/x-cmbx" );
			GlobalPatternsShort.Add( ".mdsx", "application/x-mdsx" );
			GlobalPatternsShort.Add( ".mdp", "application/x-mdp" );
			GlobalPatternsShort.Add( ".mds", "application/x-mds" );
			GlobalPatternsShort.Add( ".disco", "application/x-disco" );
			GlobalPatternsShort.Add( ".asax", "application/x-asax" );
			GlobalPatternsShort.Add( ".wsdl", "application/x-wsdl" );
			GlobalPatternsShort.Add( ".planner", "application/x-planner" );
			GlobalPatternsShort.Add( ".mrproject", "application/x-planner" );
			GlobalPatternsShort.Add( ".sla", "application/x-scribus" );
			GlobalPatternsLong.Add( ".sla.gz", "application/x-scribus" );
			GlobalPatternsShort.Add( ".scd", "application/x-scribus" );
			GlobalPatternsLong.Add( ".scd.gz", "application/x-scribus" );
			GlobalPatternsShort.Add( ".vmx", "application/x-vmware-vm" );
			GlobalPatternsShort.Add( ".vmdk", "application/x-vmware-vmdisk" );
			GlobalPatternsShort.Add( ".vmtm", "application/x-vmware-team" );
			GlobalPatternsShort.Add( ".vmsn", "application/x-vmware-snapshot" );
			GlobalPatternsShort.Add( ".vmxf", "application/x-vmware-vmfoundry" );

			MimeType mt = null;

			mt = new MimeType();
			mt.Comment = "Bluefish project file";
			MimeTypes.Add( "application/bluefish-project", mt );

			mt = new MimeType();
			mt.Comment = "Andrew Toolkit inset";
			MimeTypes.Add( "application/andrew-inset", mt );

			mt = new MimeType();
			mt.Comment = "Adobe Illustrator document";
			MimeTypes.Add( "application/illustrator", mt );

			mt = new MimeType();
			mt.Comment = "Macintosh BinHex-encoded file";
			MimeTypes.Add( "application/mac-binhex40", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/mathematica", mt );

			mt = new MimeType();
			mt.Comment = "unknown";
			MimeTypes.Add( "application/octet-stream", mt );

			mt = new MimeType();
			mt.Comment = "ODA document";
			MimeTypes.Add( "application/oda", mt );

			mt = new MimeType();
			mt.Comment = "PDF document";
			MimeTypes.Add( "application/pdf", mt );

			mt = new MimeType();
			mt.Comment = "PGP message";
			MimeTypes.Add( "application/pgp", mt );

			mt = new MimeType();
			mt.Comment = "PGP/MIME-encrypted message header";
			MimeTypes.Add( "application/pgp-encrypted", mt );

			mt = new MimeType();
			mt.Comment = "PGP keys";
			MimeTypes.Add( "application/pgp-keys", mt );

			mt = new MimeType();
			mt.Comment = "detached OpenPGP signature";
			MimeTypes.Add( "application/pgp-signature", mt );

			mt = new MimeType();
			mt.Comment = "S/MIME file";
			MimeTypes.Add( "application/pkcs7-mime", mt );

			mt = new MimeType();
			mt.Comment = "detached S/MIME signature";
			MimeTypes.Add( "application/pkcs7-signature", mt );

			mt = new MimeType();
			mt.Comment = "PostScript document";
			MimeTypes.Add( "application/postscript", mt );

			mt = new MimeType();
			mt.Comment = "Rich Text Format";
			MimeTypes.Add( "application/rtf", mt );

			mt = new MimeType();
			mt.Comment = "Synchronized Multimedia Integration Language";
			MimeTypes.Add( "application/smil", mt );

			mt = new MimeType();
			mt.Comment = "StuffIt archive";
			MimeTypes.Add( "application/stuffit", mt );

			mt = new MimeType();
			mt.Comment = "Family history file";
			MimeTypes.Add( "application/x-gedcom", mt );

			mt = new MimeType();
			mt.Comment = "Corel Draw drawing";
			MimeTypes.Add( "application/vnd.corel-draw", mt );

			mt = new MimeType();
			mt.Comment = "HP Graphics Language (plotter)";
			MimeTypes.Add( "application/vnd.hp-hpgl", mt );

			mt = new MimeType();
			mt.Comment = "HP Printer Control Language file";
			MimeTypes.Add( "application/vnd.hp-pcl", mt );

			mt = new MimeType();
			mt.Comment = "Lotus 1-2-3 spreadsheet";
			MimeTypes.Add( "application/vnd.lotus-1-2-3", mt );

			mt = new MimeType();
			mt.Comment = "XML User Interface Language document";
			MimeTypes.Add( "application/vnd.mozilla.xul+xml", mt );

			mt = new MimeType();
			mt.Comment = "Microsoft Access Jet Database";
			MimeTypes.Add( "application/vnd.ms-access", mt );

			mt = new MimeType();
			mt.Comment = "Microsoft Excel spreadsheet";
			MimeTypes.Add( "application/vnd.ms-excel", mt );

			mt = new MimeType();
			mt.Comment = "Microsoft PowerPoint presentation";
			MimeTypes.Add( "application/vnd.ms-powerpoint", mt );

			mt = new MimeType();
			mt.Comment = "Microsoft Word document";
			MimeTypes.Add( "application/msword", mt );

			mt = new MimeType();
			mt.Comment = "Palmpilot database/document";
			MimeTypes.Add( "application/vnd.palm", mt );

			mt = new MimeType();
			mt.Comment = "RealAudio/Video document";
			MimeTypes.Add( "application/vnd.rn-realmedia", mt );

			mt = new MimeType();
			mt.Comment = "StarCalc spreadsheet";
			MimeTypes.Add( "application/vnd.stardivision.calc", mt );

			mt = new MimeType();
			mt.Comment = "StarChart chart";
			MimeTypes.Add( "application/vnd.stardivision.chart", mt );

			mt = new MimeType();
			mt.Comment = "StarDraw drawing";
			MimeTypes.Add( "application/vnd.stardivision.draw", mt );

			mt = new MimeType();
			mt.Comment = "StarImpress presentation";
			MimeTypes.Add( "application/vnd.stardivision.impress", mt );

			mt = new MimeType();
			mt.Comment = "StarMail email";
			MimeTypes.Add( "application/vnd.stardivision.mail", mt );

			mt = new MimeType();
			mt.Comment = "StarMath formula";
			MimeTypes.Add( "application/vnd.stardivision.math", mt );

			mt = new MimeType();
			mt.Comment = "StarWriter document";
			MimeTypes.Add( "application/vnd.stardivision.writer", mt );

			mt = new MimeType();
			mt.Comment = "OpenOffice.org Calc spreadsheet";
			MimeTypes.Add( "application/vnd.sun.xml.calc", mt );

			mt = new MimeType();
			mt.Comment = "OpenOffice.org Calc spreadsheet template";
			MimeTypes.Add( "application/vnd.sun.xml.calc.template", mt );

			mt = new MimeType();
			mt.Comment = "OpenOffice.org Draw drawing";
			MimeTypes.Add( "application/vnd.sun.xml.draw", mt );

			mt = new MimeType();
			mt.Comment = "OpenOffice.org Draw drawing template";
			MimeTypes.Add( "application/vnd.sun.xml.draw.template", mt );

			mt = new MimeType();
			mt.Comment = "OpenOffice.org Impress presentation";
			MimeTypes.Add( "application/vnd.sun.xml.impress", mt );

			mt = new MimeType();
			mt.Comment = "OpenOffice.org Impress presentation template";
			MimeTypes.Add( "application/vnd.sun.xml.impress.template", mt );

			mt = new MimeType();
			mt.Comment = "OpenOffice.org Math formula";
			MimeTypes.Add( "application/vnd.sun.xml.math", mt );

			mt = new MimeType();
			mt.Comment = "OpenOffice.org Writer document";
			MimeTypes.Add( "application/vnd.sun.xml.writer", mt );

			mt = new MimeType();
			mt.Comment = "OpenOffice.org Writer global document";
			MimeTypes.Add( "application/vnd.sun.xml.writer.global", mt );

			mt = new MimeType();
			mt.Comment = "OpenOffice.org Writer document template";
			MimeTypes.Add( "application/vnd.sun.xml.writer.template", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Text";
			MimeTypes.Add( "application/vnd.oasis.opendocument.text", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Text Template";
			MimeTypes.Add( "application/vnd.oasis.opendocument.text-template", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument HTML Document Template";
			MimeTypes.Add( "application/vnd.oasis.opendocument.text-web", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Master Document";
			MimeTypes.Add( "application/vnd.oasis.opendocument.text-master", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Drawing";
			MimeTypes.Add( "application/vnd.oasis.opendocument.graphics", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Drawing Template";
			MimeTypes.Add( "application/vnd.oasis.opendocument.graphics-template", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Presentation";
			MimeTypes.Add( "application/vnd.oasis.opendocument.presentation", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Presentation Template";
			MimeTypes.Add( "application/vnd.oasis.opendocument.presentation-template", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Spreadsheet";
			MimeTypes.Add( "application/vnd.oasis.opendocument.spreadsheet", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Spreadsheet Template";
			MimeTypes.Add( "application/vnd.oasis.opendocument.spreadsheet-template", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Chart";
			MimeTypes.Add( "application/vnd.oasis.opendocument.chart", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Formula";
			MimeTypes.Add( "application/vnd.oasis.opendocument.formula", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Database";
			MimeTypes.Add( "application/vnd.oasis.opendocument.database", mt );

			mt = new MimeType();
			mt.Comment = "OpenDocument Image";
			MimeTypes.Add( "application/vnd.oasis.opendocument.image", mt );

			mt = new MimeType();
			mt.Comment = "WordPerfect document";
			MimeTypes.Add( "application/vnd.wordperfect", mt );

			mt = new MimeType();
			mt.Comment = "XBEL bookmarks";
			MimeTypes.Add( "application/x-xbel", mt );

			mt = new MimeType();
			mt.Comment = "7-zip archive";
			MimeTypes.Add( "application/x-7z-compressed", mt );

			mt = new MimeType();
			mt.Comment = "AbiWord document";
			MimeTypes.Add( "application/x-abiword", mt );

			mt = new MimeType();
			mt.Comment = "CD image cuesheet";
			MimeTypes.Add( "application/x-cue", mt );

			mt = new MimeType();
			mt.Comment = "Lotus AmiPro document";
			MimeTypes.Add( "application/x-amipro", mt );

			mt = new MimeType();
			mt.Comment = "Applix Spreadsheets spreadsheet";
			MimeTypes.Add( "application/x-applix-spreadsheet", mt );

			mt = new MimeType();
			mt.Comment = "Applix Words document";
			MimeTypes.Add( "application/x-applix-word", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-arc", mt );

			mt = new MimeType();
			mt.Comment = "AR archive";
			MimeTypes.Add( "application/x-archive", mt );

			mt = new MimeType();
			mt.Comment = "ARJ archive";
			MimeTypes.Add( "application/x-arj", mt );

			mt = new MimeType();
			mt.Comment = "active server page";
			MimeTypes.Add( "application/x-asp", mt );

			mt = new MimeType();
			mt.Comment = "AWK script";
			MimeTypes.Add( "application/x-awk", mt );

			mt = new MimeType();
			mt.Comment = "BCPIO document";
			MimeTypes.Add( "application/x-bcpio", mt );

			mt = new MimeType();
			mt.Comment = "BitTorrent seed file";
			MimeTypes.Add( "application/x-bittorrent", mt );

			mt = new MimeType();
			mt.Comment = "Blender scene";
			MimeTypes.Add( "application/x-blender", mt );

			mt = new MimeType();
			mt.Comment = "bzip archive";
			MimeTypes.Add( "application/x-bzip", mt );

			mt = new MimeType();
			mt.Comment = "tar archive (bzip-compressed)";
			MimeTypes.Add( "application/x-bzip-compressed-tar", mt );

			mt = new MimeType();
			mt.Comment = "raw CD image";
			MimeTypes.Add( "application/x-cd-image", mt );

			mt = new MimeType();
			mt.Comment = "CGI script";
			MimeTypes.Add( "application/x-cgi", mt );

			mt = new MimeType();
			mt.Comment = "PGN chess game";
			MimeTypes.Add( "application/x-chess-pgn", mt );

			mt = new MimeType();
			mt.Comment = "Compiled HTML Help Format";
			MimeTypes.Add( "application/x-chm", mt );

			mt = new MimeType();
			mt.Comment = "Java byte code";
			MimeTypes.Add( "application/x-class-file", mt );

			mt = new MimeType();
			mt.Comment = "UNIX-compressed file";
			MimeTypes.Add( "application/x-compress", mt );

			mt = new MimeType();
			mt.Comment = "tar archive (gzip-compressed)";
			MimeTypes.Add( "application/x-compressed-tar", mt );

			mt = new MimeType();
			mt.Comment = "program crash data";
			MimeTypes.Add( "application/x-core", mt );

			mt = new MimeType();
			mt.Comment = "CPIO archive";
			MimeTypes.Add( "application/x-cpio", mt );

			mt = new MimeType();
			mt.Comment = "CPIO archive (gzip-compressed)";
			MimeTypes.Add( "application/x-cpio-compressed", mt );

			mt = new MimeType();
			mt.Comment = "C shell script";
			MimeTypes.Add( "application/x-csh", mt );

			mt = new MimeType();
			mt.Comment = "Xbase document";
			MimeTypes.Add( "application/x-dbf", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-dbm", mt );

			mt = new MimeType();
			mt.Comment = "Dreamcast ROM";
			MimeTypes.Add( "application/x-dc-rom", mt );

			mt = new MimeType();
			mt.Comment = "Debian package";
			MimeTypes.Add( "application/x-deb", mt );

			mt = new MimeType();
			mt.Comment = "Qt Designer file";
			MimeTypes.Add( "application/x-designer", mt );

			mt = new MimeType();
			mt.Comment = "desktop configuration file";
			MimeTypes.Add( "application/x-desktop", mt );

			mt = new MimeType();
			mt.Comment = "Dia diagram";
			MimeTypes.Add( "application/x-dia-diagram", mt );

			mt = new MimeType();
			mt.Comment = "TeX DVI document";
			MimeTypes.Add( "application/x-dvi", mt );

			mt = new MimeType();
			mt.Comment = "Enlightenment theme";
			MimeTypes.Add( "application/x-e-theme", mt );

			mt = new MimeType();
			mt.Comment = "Egon Animator animation";
			MimeTypes.Add( "application/x-egon", mt );

			mt = new MimeType();
			mt.Comment = "executable";
			MimeTypes.Add( "application/x-executable", mt );

			mt = new MimeType();
			mt.Comment = "font";
			MimeTypes.Add( "application/x-font-type1", mt );

			mt = new MimeType();
			mt.Comment = "Adobe font metrics";
			MimeTypes.Add( "application/x-font-afm", mt );

			mt = new MimeType();
			mt.Comment = "BDF font";
			MimeTypes.Add( "application/x-font-bdf", mt );

			mt = new MimeType();
			mt.Comment = "DOS font";
			MimeTypes.Add( "application/x-font-dos", mt );

			mt = new MimeType();
			mt.Comment = "Adobe FrameMaker font";
			MimeTypes.Add( "application/x-font-framemaker", mt );

			mt = new MimeType();
			mt.Comment = "LIBGRX font";
			MimeTypes.Add( "application/x-font-libgrx", mt );

			mt = new MimeType();
			mt.Comment = "Linux PSF console font";
			MimeTypes.Add( "application/x-font-linux-psf", mt );

			mt = new MimeType();
			mt.Comment = "PCF font";
			MimeTypes.Add( "application/x-font-pcf", mt );

			mt = new MimeType();
			mt.Comment = "OpenType font";
			MimeTypes.Add( "application/x-font-otf", mt );

			mt = new MimeType();
			mt.Comment = "Speedo font";
			MimeTypes.Add( "application/x-font-speedo", mt );

			mt = new MimeType();
			mt.Comment = "SunOS News font";
			MimeTypes.Add( "application/x-font-sunos-news", mt );

			mt = new MimeType();
			mt.Comment = "TeX font";
			MimeTypes.Add( "application/x-font-tex", mt );

			mt = new MimeType();
			mt.Comment = "TeX font metrics";
			MimeTypes.Add( "application/x-font-tex-tfm", mt );

			mt = new MimeType();
			mt.Comment = "TrueType font";
			MimeTypes.Add( "application/x-font-ttf", mt );

			mt = new MimeType();
			mt.Comment = "V font";
			MimeTypes.Add( "application/x-font-vfont", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-frame", mt );

			mt = new MimeType();
			mt.Comment = "Game Boy ROM";
			MimeTypes.Add( "application/x-gameboy-rom", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-gdbm", mt );

			mt = new MimeType();
			mt.Comment = "Genesis ROM";
			MimeTypes.Add( "application/x-genesis-rom", mt );

			mt = new MimeType();
			mt.Comment = "translated messages (machine-readable)";
			MimeTypes.Add( "application/x-gettext-translation", mt );

			mt = new MimeType();
			mt.Comment = "Glade project";
			MimeTypes.Add( "application/x-glade", mt );

			mt = new MimeType();
			mt.Comment = "GMC link";
			MimeTypes.Add( "application/x-gmc-link", mt );

			mt = new MimeType();
			mt.Comment = "GnuCash spreadsheet";
			MimeTypes.Add( "application/x-gnucash", mt );

			mt = new MimeType();
			mt.Comment = "Gnumeric spreadsheet";
			MimeTypes.Add( "application/x-gnumeric", mt );

			mt = new MimeType();
			mt.Comment = "Graphite scientific graph";
			MimeTypes.Add( "application/x-graphite", mt );

			mt = new MimeType();
			mt.Comment = "gtar archive";
			MimeTypes.Add( "application/x-gtar", mt );

			mt = new MimeType();
			mt.Comment = "GTKtalog catalog";
			MimeTypes.Add( "application/x-gtktalog", mt );

			mt = new MimeType();
			mt.Comment = "gzip archive";
			MimeTypes.Add( "application/x-gzip", mt );

			mt = new MimeType();
			mt.Comment = "PostScript document (gzip-compressed)";
			MimeTypes.Add( "application/x-gzpostscript", mt );

			mt = new MimeType();
			mt.Comment = "HDF document";
			MimeTypes.Add( "application/x-hdf", mt );

			mt = new MimeType();
			mt.Comment = "iPod firmware";
			MimeTypes.Add( "application/x-ipod-firmware", mt );

			mt = new MimeType();
			mt.Comment = "Java archive";
			MimeTypes.Add( "application/x-jar", mt );

			mt = new MimeType();
			mt.Comment = "Java class";
			MimeTypes.Add( "application/x-java", mt );

			mt = new MimeType();
			mt.Comment = "Java Network Launched Application";
			MimeTypes.Add( "application/x-java-jnlp-file", mt );

			mt = new MimeType();
			mt.Comment = "Javascript program";
			MimeTypes.Add( "application/x-javascript", mt );

			mt = new MimeType();
			mt.Comment = "JBuilder project";
			MimeTypes.Add( "application/x-jbuilder-project", mt );

			mt = new MimeType();
			mt.Comment = "Karbon14 drawing";
			MimeTypes.Add( "application/x-karbon", mt );

			mt = new MimeType();
			mt.Comment = "KChart chart";
			MimeTypes.Add( "application/x-kchart", mt );

			mt = new MimeType();
			mt.Comment = "KFormula formula";
			MimeTypes.Add( "application/x-kformula", mt );

			mt = new MimeType();
			mt.Comment = "KIllustrator drawing";
			MimeTypes.Add( "application/x-killustrator", mt );

			mt = new MimeType();
			mt.Comment = "Kivio flowchart";
			MimeTypes.Add( "application/x-kivio", mt );

			mt = new MimeType();
			mt.Comment = "Kontour drawing";
			MimeTypes.Add( "application/x-kontour", mt );

			mt = new MimeType();
			mt.Comment = "KPovModeler scene";
			MimeTypes.Add( "application/x-kpovmodeler", mt );

			mt = new MimeType();
			mt.Comment = "KPresenter presentation";
			MimeTypes.Add( "application/x-kpresenter", mt );

			mt = new MimeType();
			mt.Comment = "Krita document";
			MimeTypes.Add( "application/x-krita", mt );

			mt = new MimeType();
			mt.Comment = "KSpread spreadsheet";
			MimeTypes.Add( "application/x-kspread", mt );

			mt = new MimeType();
			mt.Comment = "KSpread spreadsheet (encrypted)";
			MimeTypes.Add( "application/x-kspread-crypt", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-ksysv-package", mt );

			mt = new MimeType();
			mt.Comment = "Kugar document";
			MimeTypes.Add( "application/x-kugar", mt );

			mt = new MimeType();
			mt.Comment = "KWord document";
			MimeTypes.Add( "application/x-kword", mt );

			mt = new MimeType();
			mt.Comment = "KWord document (encrypted)";
			MimeTypes.Add( "application/x-kword-crypt", mt );

			mt = new MimeType();
			mt.Comment = "LHA archive";
			MimeTypes.Add( "application/x-lha", mt );

			mt = new MimeType();
			mt.Comment = "LHZ archive";
			MimeTypes.Add( "application/x-lhz", mt );

			mt = new MimeType();
			mt.Comment = "message catalog";
			MimeTypes.Add( "application/x-linguist", mt );

			mt = new MimeType();
			mt.Comment = "LyX document";
			MimeTypes.Add( "text/x-lyx", mt );

			mt = new MimeType();
			mt.Comment = "LZO archive";
			MimeTypes.Add( "application/x-lzop", mt );

			mt = new MimeType();
			mt.Comment = "MagicPoint presentation";
			MimeTypes.Add( "application/x-magicpoint", mt );

			mt = new MimeType();
			mt.Comment = "Macintosh MacBinary file";
			MimeTypes.Add( "application/x-macbinary", mt );

			mt = new MimeType();
			mt.Comment = "Matroska video";
			MimeTypes.Add( "application/x-matroska", mt );

			mt = new MimeType();
			mt.Comment = "FrameMaker MIF document";
			MimeTypes.Add( "application/x-mif", mt );

			mt = new MimeType();
			mt.Comment = "Mozilla bookmarks";
			MimeTypes.Add( "application/x-mozilla-bookmarks", mt );

			mt = new MimeType();
			mt.Comment = "DOS/Windows executable";
			MimeTypes.Add( "application/x-ms-dos-executable", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-mswinurl", mt );

			mt = new MimeType();
			mt.Comment = "Microsoft Write document";
			MimeTypes.Add( "application/x-mswrite", mt );

			mt = new MimeType();
			mt.Comment = "MSX ROM";
			MimeTypes.Add( "application/x-msx-rom", mt );

			mt = new MimeType();
			mt.Comment = "Nintendo64 ROM";
			MimeTypes.Add( "application/x-n64-rom", mt );

			mt = new MimeType();
			mt.Comment = "Nautilus link";
			MimeTypes.Add( "application/x-nautilus-link", mt );

			mt = new MimeType();
			mt.Comment = "NES ROM";
			MimeTypes.Add( "application/x-nes-rom", mt );

			mt = new MimeType();
			mt.Comment = "Unidata NetCDF document";
			MimeTypes.Add( "application/x-netcdf", mt );

			mt = new MimeType();
			mt.Comment = "Netscape bookmarks";
			MimeTypes.Add( "application/x-netscape-bookmarks", mt );

			mt = new MimeType();
			mt.Comment = "object code";
			MimeTypes.Add( "application/x-object", mt );

			mt = new MimeType();
			mt.Comment = "Ogg Vorbis audio";
			MimeTypes.Add( "application/ogg", mt );

			mt = new MimeType();
			mt.Comment = "OLE2 compound document storage";
			MimeTypes.Add( "application/x-ole-storage", mt );

			mt = new MimeType();
			mt.Comment = "GNU Oleo spreadsheet";
			MimeTypes.Add( "application/x-oleo", mt );

			mt = new MimeType();
			mt.Comment = "Palm OS database";
			MimeTypes.Add( "application/x-palm-database", mt );

			mt = new MimeType();
			mt.Comment = "PAR2 Parity File";
			MimeTypes.Add( "application/x-par2", mt );

			mt = new MimeType();
			mt.Comment = "PEF executable";
			MimeTypes.Add( "application/x-pef-executable", mt );

			mt = new MimeType();
			mt.Comment = "Perl script";
			MimeTypes.Add( "application/x-perl", mt );

			mt = new MimeType();
			mt.Comment = "PHP script";
			MimeTypes.Add( "application/x-php", mt );

			mt = new MimeType();
			mt.Comment = "PKCS#12 certificate bundle";
			MimeTypes.Add( "application/x-pkcs12", mt );

			mt = new MimeType();
			mt.Comment = "profiler results";
			MimeTypes.Add( "application/x-profile", mt );

			mt = new MimeType();
			mt.Comment = "Pathetic Writer document";
			MimeTypes.Add( "application/x-pw", mt );

			mt = new MimeType();
			mt.Comment = "Python bytecode";
			MimeTypes.Add( "application/x-python-bytecode", mt );

			mt = new MimeType();
			mt.Comment = "Quattro Pro spreadsheet";
			MimeTypes.Add( "application/x-quattropro", mt );

			mt = new MimeType();
			mt.Comment = "Quicken document";
			MimeTypes.Add( "application/x-qw", mt );

			mt = new MimeType();
			mt.Comment = "RAR archive";
			MimeTypes.Add( "application/x-rar", mt );

			mt = new MimeType();
			mt.Comment = "rejected patch";
			MimeTypes.Add( "application/x-reject", mt );

			mt = new MimeType();
			mt.Comment = "RPM package";
			MimeTypes.Add( "application/x-rpm", mt );

			mt = new MimeType();
			mt.Comment = "Ruby script";
			MimeTypes.Add( "application/x-ruby", mt );

			mt = new MimeType();
			mt.Comment = "SC/xspread file";
			MimeTypes.Add( "application/x-sc", mt );

			mt = new MimeType();
			mt.Comment = "shell archive";
			MimeTypes.Add( "application/x-shar", mt );

			mt = new MimeType();
			mt.Comment = "shared library (la)";
			MimeTypes.Add( "application/x-shared-library-la", mt );

			mt = new MimeType();
			mt.Comment = "shared library";
			MimeTypes.Add( "application/x-sharedlib", mt );

			mt = new MimeType();
			mt.Comment = "shell script";
			MimeTypes.Add( "application/x-shellscript", mt );

			mt = new MimeType();
			mt.Comment = "Shockwave Flash file";
			MimeTypes.Add( "application/x-shockwave-flash", mt );

			mt = new MimeType();
			mt.Comment = "Siag spreadsheet";
			MimeTypes.Add( "application/x-siag", mt );

			mt = new MimeType();
			mt.Comment = "Stampede package";
			MimeTypes.Add( "application/x-slp", mt );

			mt = new MimeType();
			mt.Comment = "SMS/Game Gear ROM";
			MimeTypes.Add( "application/x-sms-rom", mt );

			mt = new MimeType();
			mt.Comment = "Macintosh StuffIt archive";
			MimeTypes.Add( "application/x-stuffit", mt );

			mt = new MimeType();
			mt.Comment = "SV4 CPIO archive";
			MimeTypes.Add( "application/x-sv4cpio", mt );

			mt = new MimeType();
			mt.Comment = "SV4 CPIP archive (with CRC)";
			MimeTypes.Add( "application/x-sv4crc", mt );

			mt = new MimeType();
			mt.Comment = "tar archive";
			MimeTypes.Add( "application/x-tar", mt );

			mt = new MimeType();
			mt.Comment = "tar archive (compressed)";
			MimeTypes.Add( "application/x-tarz", mt );

			mt = new MimeType();
			mt.Comment = "generic font file";
			MimeTypes.Add( "application/x-tex-gf", mt );

			mt = new MimeType();
			mt.Comment = "packed font file";
			MimeTypes.Add( "application/x-tex-pk", mt );

			mt = new MimeType();
			mt.Comment = "TGIF document";
			MimeTypes.Add( "application/x-tgif", mt );

			mt = new MimeType();
			mt.Comment = "theme";
			MimeTypes.Add( "application/x-theme", mt );

			mt = new MimeType();
			mt.Comment = "ToutDoux document";
			MimeTypes.Add( "application/x-toutdoux", mt );

			mt = new MimeType();
			mt.Comment = "backup file";
			MimeTypes.Add( "application/x-trash", mt );

			mt = new MimeType();
			mt.Comment = "Troff document";
			MimeTypes.Add( "application/x-troff", mt );

			mt = new MimeType();
			mt.Comment = "Troff document (with manpage macros)";
			MimeTypes.Add( "application/x-troff-man", mt );

			mt = new MimeType();
			mt.Comment = "manual page (compressed)";
			MimeTypes.Add( "application/x-troff-man-compressed", mt );

			mt = new MimeType();
			mt.Comment = "tar archive (LZO-compressed)";
			MimeTypes.Add( "application/x-tzo", mt );

			mt = new MimeType();
			mt.Comment = "ustar archive";
			MimeTypes.Add( "application/x-ustar", mt );

			mt = new MimeType();
			mt.Comment = "WAIS source code";
			MimeTypes.Add( "application/x-wais-source", mt );

			mt = new MimeType();
			mt.Comment = "WordPerfect/Drawperfect image";
			MimeTypes.Add( "application/x-wpg", mt );

			mt = new MimeType();
			mt.Comment = "DER/PEM/Netscape-encoded X.509 certificate";
			MimeTypes.Add( "application/x-x509-ca-cert", mt );

			mt = new MimeType();
			mt.Comment = "empty document";
			MimeTypes.Add( "application/x-zerosize", mt );

			mt = new MimeType();
			mt.Comment = "zoo archive";
			MimeTypes.Add( "application/x-zoo", mt );

			mt = new MimeType();
			mt.Comment = "XHTML page";
			MimeTypes.Add( "application/xhtml+xml", mt );

			mt = new MimeType();
			mt.Comment = "ZIP archive";
			MimeTypes.Add( "application/zip", mt );

			mt = new MimeType();
			mt.Comment = "Dolby Digital audio";
			MimeTypes.Add( "audio/ac3", mt );

			mt = new MimeType();
			mt.Comment = "ULAW (Sun) audio";
			MimeTypes.Add( "audio/basic", mt );

			mt = new MimeType();
			mt.Comment = "Commodore 64 audio";
			MimeTypes.Add( "audio/prs.sid", mt );

			mt = new MimeType();
			mt.Comment = "PCM audio";
			MimeTypes.Add( "audio/x-adpcm", mt );

			mt = new MimeType();
			mt.Comment = "AIFC audio";
			MimeTypes.Add( "audio/x-aifc", mt );

			mt = new MimeType();
			mt.Comment = "AIFF/Amiga/Mac audio";
			MimeTypes.Add( "audio/x-aiff", mt );

			mt = new MimeType();
			mt.Comment = "AIFF audio";
			MimeTypes.Add( "audio/x-aiffc", mt );

			mt = new MimeType();
			mt.Comment = "Impulse Tracker audio";
			MimeTypes.Add( "audio/x-it", mt );

			mt = new MimeType();
			mt.Comment = "FLAC audio";
			MimeTypes.Add( "audio/x-flac", mt );

			mt = new MimeType();
			mt.Comment = "MIDI audio";
			MimeTypes.Add( "audio/midi", mt );

			mt = new MimeType();
			mt.Comment = "MPEG-4 audio";
			MimeTypes.Add( "audio/mp4", mt );

			mt = new MimeType();
			mt.Comment = "MPEG-4 video";
			MimeTypes.Add( "video/mp4", mt );

			mt = new MimeType();
			mt.Comment = "Amiga SoundTracker audio";
			MimeTypes.Add( "audio/x-mod", mt );

			mt = new MimeType();
			mt.Comment = "MP3 audio";
			MimeTypes.Add( "audio/mpeg", mt );

			mt = new MimeType();
			mt.Comment = "MP3 playlist";
			MimeTypes.Add( "audio/x-mp3-playlist", mt );

			mt = new MimeType();
			mt.Comment = "MP3 audio";
			MimeTypes.Add( "audio/x-mpeg", mt );

			mt = new MimeType();
			mt.Comment = "MP3 audio (streamed)";
			MimeTypes.Add( "audio/x-mpegurl", mt );

			mt = new MimeType();
			mt.Comment = "Playlist";
			MimeTypes.Add( "audio/x-ms-asx", mt );

			mt = new MimeType();
			mt.Comment = "RealAudio broadcast";
			MimeTypes.Add( "audio/x-pn-realaudio", mt );

			mt = new MimeType();
			mt.Comment = "RIFF audio";
			MimeTypes.Add( "audio/x-riff", mt );

			mt = new MimeType();
			mt.Comment = "Scream Tracker 3 audio";
			MimeTypes.Add( "audio/x-s3m", mt );

			mt = new MimeType();
			mt.Comment = "MP3 ShoutCast playlist";
			MimeTypes.Add( "audio/x-scpls", mt );

			mt = new MimeType();
			mt.Comment = "Scream Tracker audio";
			MimeTypes.Add( "audio/x-stm", mt );

			mt = new MimeType();
			mt.Comment = "VOC audio";
			MimeTypes.Add( "audio/x-voc", mt );

			mt = new MimeType();
			mt.Comment = "WAV audio";
			MimeTypes.Add( "audio/x-wav", mt );

			mt = new MimeType();
			mt.Comment = "Scream Tracker instrument";
			MimeTypes.Add( "audio/x-xi", mt );

			mt = new MimeType();
			mt.Comment = "FastTracker II audio";
			MimeTypes.Add( "audio/x-xm", mt );

			mt = new MimeType();
			mt.Comment = "Windows BMP image";
			MimeTypes.Add( "image/bmp", mt );

			mt = new MimeType();
			mt.Comment = "Computer Graphics Metafile";
			MimeTypes.Add( "image/cgm", mt );

			mt = new MimeType();
			mt.Comment = "CCITT G3 fax";
			MimeTypes.Add( "image/fax-g3", mt );

			mt = new MimeType();
			mt.Comment = "G3 fax image";
			MimeTypes.Add( "image/g3fax", mt );

			mt = new MimeType();
			mt.Comment = "GIF image";
			MimeTypes.Add( "image/gif", mt );

			mt = new MimeType();
			mt.Comment = "IEF image";
			MimeTypes.Add( "image/ief", mt );

			mt = new MimeType();
			mt.Comment = "JPEG image";
			MimeTypes.Add( "image/jpeg", mt );

			mt = new MimeType();
			mt.Comment = "JPEG-2000 image";
			MimeTypes.Add( "image/jpeg2000", mt );

			mt = new MimeType();
			mt.Comment = "Macintosh Quickdraw/PICT drawing";
			MimeTypes.Add( "image/x-pict", mt );

			mt = new MimeType();
			mt.Comment = "PNG image";
			MimeTypes.Add( "image/png", mt );

			mt = new MimeType();
			mt.Comment = "Run Length Encoded bitmap";
			MimeTypes.Add( "image/rle", mt );

			mt = new MimeType();
			mt.Comment = "scalable SVG image";
			MimeTypes.Add( "image/svg+xml", mt );

			mt = new MimeType();
			mt.Comment = "TIFF image";
			MimeTypes.Add( "image/tiff", mt );

			mt = new MimeType();
			mt.Comment = "AutoCAD image";
			MimeTypes.Add( "image/vnd.dwg", mt );

			mt = new MimeType();
			mt.Comment = "DXF vector image";
			MimeTypes.Add( "image/vnd.dxf", mt );

			mt = new MimeType();
			mt.Comment = "3D Studio image";
			MimeTypes.Add( "image/x-3ds", mt );

			mt = new MimeType();
			mt.Comment = "Applix Graphics image";
			MimeTypes.Add( "image/x-applix-graphics", mt );

			mt = new MimeType();
			mt.Comment = "CMU raster image";
			MimeTypes.Add( "image/x-cmu-raster", mt );

			mt = new MimeType();
			mt.Comment = "GIMP image (compressed)";
			MimeTypes.Add( "image/x-compressed-xcf", mt );

			mt = new MimeType();
			mt.Comment = "Digital Imaging and Communications in Medicine image";
			MimeTypes.Add( "application/dicom", mt );

			mt = new MimeType();
			mt.Comment = "Device Independant Bitmap";
			MimeTypes.Add( "image/x-dib", mt );

			mt = new MimeType();
			mt.Comment = "DjVu image";
			MimeTypes.Add( "image/vnd.djvu", mt );

			mt = new MimeType();
			mt.Comment = "Digital Moving Picture Exchange image";
			MimeTypes.Add( "image/dpx", mt );

			mt = new MimeType();
			mt.Comment = "Encapsulated PostScript image";
			MimeTypes.Add( "image/x-eps", mt );

			mt = new MimeType();
			mt.Comment = "Flexible Image Transport System";
			MimeTypes.Add( "image/x-fits", mt );

			mt = new MimeType();
			mt.Comment = "FlashPix image";
			MimeTypes.Add( "image/x-fpx", mt );

			mt = new MimeType();
			mt.Comment = "Truevision Targa image";
			MimeTypes.Add( "image/x-icb", mt );

			mt = new MimeType();
			mt.Comment = "Microsoft Windows icon";
			MimeTypes.Add( "image/x-ico", mt );

			mt = new MimeType();
			mt.Comment = "IFF image";
			MimeTypes.Add( "image/x-iff", mt );

			mt = new MimeType();
			mt.Comment = "ILBM image";
			MimeTypes.Add( "image/x-ilbm", mt );

			mt = new MimeType();
			mt.Comment = "JNG image";
			MimeTypes.Add( "image/x-jng", mt );

			mt = new MimeType();
			mt.Comment = "LightWave object";
			MimeTypes.Add( "image/x-lwo", mt );

			mt = new MimeType();
			mt.Comment = "LightWave scene";
			MimeTypes.Add( "image/x-lws", mt );

			mt = new MimeType();
			mt.Comment = "Microsoft Office drawing";
			MimeTypes.Add( "image/x-msod", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "image/x-niff", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "image/x-pcx", mt );

			mt = new MimeType();
			mt.Comment = "PhotoCD image";
			MimeTypes.Add( "image/x-photo-cd", mt );

			mt = new MimeType();
			mt.Comment = "PNM image";
			MimeTypes.Add( "image/x-portable-anymap", mt );

			mt = new MimeType();
			mt.Comment = "Portable Bitmap File Format";
			MimeTypes.Add( "image/x-portable-bitmap", mt );

			mt = new MimeType();
			mt.Comment = "Portable Graymap File Format";
			MimeTypes.Add( "image/x-portable-graymap", mt );

			mt = new MimeType();
			mt.Comment = "Portable Pixmap File Format";
			MimeTypes.Add( "image/x-portable-pixmap", mt );

			mt = new MimeType();
			mt.Comment = "Photoshop image";
			MimeTypes.Add( "image/x-psd", mt );

			mt = new MimeType();
			mt.Comment = "RGB image";
			MimeTypes.Add( "image/x-rgb", mt );

			mt = new MimeType();
			mt.Comment = "Silicon Graphics IRIS image";
			MimeTypes.Add( "image/x-sgi", mt );

			mt = new MimeType();
			mt.Comment = "SUN Rasterfile image";
			MimeTypes.Add( "image/x-sun-raster", mt );

			mt = new MimeType();
			mt.Comment = "TarGA image";
			MimeTypes.Add( "image/x-tga", mt );

			mt = new MimeType();
			mt.Comment = "Windows cursor";
			MimeTypes.Add( "image/x-win-bitmap", mt );

			mt = new MimeType();
			mt.Comment = "Microsoft WMF file";
			MimeTypes.Add( "image/x-wmf", mt );

			mt = new MimeType();
			mt.Comment = "X BitMap image";
			MimeTypes.Add( "image/x-xbitmap", mt );

			mt = new MimeType();
			mt.Comment = "GIMP image";
			MimeTypes.Add( "image/x-xcf", mt );

			mt = new MimeType();
			mt.Comment = "XFig image";
			MimeTypes.Add( "image/x-xfig", mt );

			mt = new MimeType();
			mt.Comment = "X PixMap image";
			MimeTypes.Add( "image/x-xpixmap", mt );

			mt = new MimeType();
			mt.Comment = "X window image";
			MimeTypes.Add( "image/x-xwindowdump", mt );

			mt = new MimeType();
			mt.Comment = "block device";
			MimeTypes.Add( "inode/blockdevice", mt );

			mt = new MimeType();
			mt.Comment = "character device";
			MimeTypes.Add( "inode/chardevice", mt );

			mt = new MimeType();
			mt.Comment = "folder";
			MimeTypes.Add( "inode/directory", mt );

			mt = new MimeType();
			mt.Comment = "pipe";
			MimeTypes.Add( "inode/fifo", mt );

			mt = new MimeType();
			mt.Comment = "mount point";
			MimeTypes.Add( "inode/mount-point", mt );

			mt = new MimeType();
			mt.Comment = "socket";
			MimeTypes.Add( "inode/socket", mt );

			mt = new MimeType();
			mt.Comment = "symbolic link";
			MimeTypes.Add( "inode/symlink", mt );

			mt = new MimeType();
			mt.Comment = "mail delivery report";
			MimeTypes.Add( "message/delivery-status", mt );

			mt = new MimeType();
			mt.Comment = "mail disposition report";
			MimeTypes.Add( "message/disposition-notification", mt );

			mt = new MimeType();
			mt.Comment = "reference to remote file";
			MimeTypes.Add( "message/external-body", mt );

			mt = new MimeType();
			mt.Comment = "Usenet news message";
			MimeTypes.Add( "message/news", mt );

			mt = new MimeType();
			mt.Comment = "partial email message";
			MimeTypes.Add( "message/partial", mt );

			mt = new MimeType();
			mt.Comment = "email message";
			MimeTypes.Add( "message/rfc822", mt );

			mt = new MimeType();
			mt.Comment = "GNU mail message";
			MimeTypes.Add( "message/x-gnu-rmail", mt );

			mt = new MimeType();
			mt.Comment = "VRML document";
			MimeTypes.Add( "model/vrml", mt );

			mt = new MimeType();
			mt.Comment = "message in several formats";
			MimeTypes.Add( "multipart/alternative", mt );

			mt = new MimeType();
			mt.Comment = "Macintosh AppleDouble-encoded file";
			MimeTypes.Add( "multipart/appledouble", mt );

			mt = new MimeType();
			mt.Comment = "message digest";
			MimeTypes.Add( "multipart/digest", mt );

			mt = new MimeType();
			mt.Comment = "encrypted message";
			MimeTypes.Add( "multipart/encrypted", mt );

			mt = new MimeType();
			mt.Comment = "compound documents";
			MimeTypes.Add( "multipart/mixed", mt );

			mt = new MimeType();
			mt.Comment = "compound document";
			MimeTypes.Add( "multipart/related", mt );

			mt = new MimeType();
			mt.Comment = "mail system report";
			MimeTypes.Add( "multipart/report", mt );

			mt = new MimeType();
			mt.Comment = "signed message";
			MimeTypes.Add( "multipart/signed", mt );

			mt = new MimeType();
			mt.Comment = "stream of data (server push)";
			MimeTypes.Add( "multipart/x-mixed-replace", mt );

			mt = new MimeType();
			mt.Comment = "vCalendar interchange file";
			MimeTypes.Add( "text/calendar", mt );

			mt = new MimeType();
			mt.Comment = "Cascading Style Sheet";
			MimeTypes.Add( "text/css", mt );

			mt = new MimeType();
			mt.Comment = "Electronic Business Card";
			MimeTypes.Add( "text/directory", mt );

			mt = new MimeType();
			mt.Comment = "enriched text document";
			MimeTypes.Add( "text/enriched", mt );

			mt = new MimeType();
			mt.Comment = "help page";
			MimeTypes.Add( "text/htmlh", mt );

			mt = new MimeType();
			mt.Comment = "MathML document";
			MimeTypes.Add( "text/mathml", mt );

			mt = new MimeType();
			mt.Comment = "plain text document";
			MimeTypes.Add( "text/plain", mt );

			mt = new MimeType();
			mt.Comment = "Resource Description Framework (RDF) file";
			MimeTypes.Add( "text/rdf", mt );

			mt = new MimeType();
			mt.Comment = "email headers";
			MimeTypes.Add( "text/rfc822-headers", mt );

			mt = new MimeType();
			mt.Comment = "rich text document";
			MimeTypes.Add( "text/richtext", mt );

			mt = new MimeType();
			mt.Comment = "RDF Site Summary";
			MimeTypes.Add( "text/rss", mt );

			mt = new MimeType();
			mt.Comment = "SGML document";
			MimeTypes.Add( "text/sgml", mt );

			mt = new MimeType();
			mt.Comment = "Spreadsheet interchange document";
			MimeTypes.Add( "text/spreadsheet", mt );

			mt = new MimeType();
			mt.Comment = "text document (with tab-separated values)";
			MimeTypes.Add( "text/tab-separated-values", mt );

			mt = new MimeType();
			mt.Comment = "WML document";
			MimeTypes.Add( "text/vnd.wap.wml", mt );

			mt = new MimeType();
			mt.Comment = "Ada source code";
			MimeTypes.Add( "text/x-adasrc", mt );

			mt = new MimeType();
			mt.Comment = "author list";
			MimeTypes.Add( "text/x-authors", mt );

			mt = new MimeType();
			mt.Comment = "Bibtex bibliographic data";
			MimeTypes.Add( "text/x-bibtex", mt );

			mt = new MimeType();
			mt.Comment = "C++ source code header";
			MimeTypes.Add( "text/x-c++hdr", mt );

			mt = new MimeType();
			mt.Comment = "C++ source code";
			MimeTypes.Add( "text/x-c++src", mt );

			mt = new MimeType();
			mt.Comment = "C source code header";
			MimeTypes.Add( "text/x-chdr", mt );

			mt = new MimeType();
			mt.Comment = "text document (with comma-separated values)";
			MimeTypes.Add( "text/x-comma-separated-values", mt );

			mt = new MimeType();
			mt.Comment = "software license terms";
			MimeTypes.Add( "text/x-copying", mt );

			mt = new MimeType();
			mt.Comment = "software author credits";
			MimeTypes.Add( "text/x-credits", mt );

			mt = new MimeType();
			mt.Comment = "C source code";
			MimeTypes.Add( "text/x-csrc", mt );

			mt = new MimeType();
			mt.Comment = "C# source code";
			MimeTypes.Add( "text/x-csharp", mt );

			mt = new MimeType();
			mt.Comment = "DCL script";
			MimeTypes.Add( "text/x-dcl", mt );

			mt = new MimeType();
			mt.Comment = "DSSSL document";
			MimeTypes.Add( "text/x-dsl", mt );

			mt = new MimeType();
			mt.Comment = "D source code";
			MimeTypes.Add( "text/x-dsrc", mt );

			mt = new MimeType();
			mt.Comment = "document type definition";
			MimeTypes.Add( "text/x-dtd", mt );

			mt = new MimeType();
			mt.Comment = "Emacs Lisp source code";
			MimeTypes.Add( "text/x-emacs-lisp", mt );

			mt = new MimeType();
			mt.Comment = "Fortran source code";
			MimeTypes.Add( "text/x-fortran", mt );

			mt = new MimeType();
			mt.Comment = "translated messages";
			MimeTypes.Add( "text/x-gettext-translation", mt );

			mt = new MimeType();
			mt.Comment = "message translation template";
			MimeTypes.Add( "text/x-gettext-translation-template", mt );

			mt = new MimeType();
			mt.Comment = "HTML page";
			MimeTypes.Add( "text/html", mt );

			mt = new MimeType();
			mt.Comment = "GTK configuration";
			MimeTypes.Add( "text/x-gtkrc", mt );

			mt = new MimeType();
			mt.Comment = "Haskell source code";
			MimeTypes.Add( "text/x-haskell", mt );

			mt = new MimeType();
			mt.Comment = "IDL document";
			MimeTypes.Add( "text/x-idl", mt );

			mt = new MimeType();
			mt.Comment = "software installation instructions";
			MimeTypes.Add( "text/x-install", mt );

			mt = new MimeType();
			mt.Comment = "Java source code";
			MimeTypes.Add( "text/x-java", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "text/x-ksysv-log", mt );

			mt = new MimeType();
			mt.Comment = "Literate haskell source code";
			MimeTypes.Add( "text/x-literate-haskell", mt );

			mt = new MimeType();
			mt.Comment = "application log";
			MimeTypes.Add( "text/x-log", mt );

			mt = new MimeType();
			mt.Comment = "Makefile";
			MimeTypes.Add( "text/x-makefile", mt );

			mt = new MimeType();
			mt.Comment = "Qt Meta Object file";
			MimeTypes.Add( "text/x-moc", mt );

			mt = new MimeType();
			mt.Comment = "Objective-C source code";
			MimeTypes.Add( "text/x-objcsrc", mt );

			mt = new MimeType();
			mt.Comment = "Pascal source code";
			MimeTypes.Add( "text/x-pascal", mt );

			mt = new MimeType();
			mt.Comment = "differences between files";
			MimeTypes.Add( "text/x-patch", mt );

			mt = new MimeType();
			mt.Comment = "Python script";
			MimeTypes.Add( "text/x-python", mt );

			mt = new MimeType();
			mt.Comment = "README document";
			MimeTypes.Add( "text/x-readme", mt );

			mt = new MimeType();
			mt.Comment = "Scheme source code";
			MimeTypes.Add( "text/x-scheme", mt );

			mt = new MimeType();
			mt.Comment = "Setext document";
			MimeTypes.Add( "text/x-setext", mt );

			mt = new MimeType();
			mt.Comment = "Speech document";
			MimeTypes.Add( "text/x-speech", mt );

			mt = new MimeType();
			mt.Comment = "SQL code";
			MimeTypes.Add( "text/x-sql", mt );

			mt = new MimeType();
			mt.Comment = "Tcl script";
			MimeTypes.Add( "text/x-tcl", mt );

			mt = new MimeType();
			mt.Comment = "TeX document";
			MimeTypes.Add( "text/x-tex", mt );

			mt = new MimeType();
			mt.Comment = "TeXInfo document";
			MimeTypes.Add( "text/x-texinfo", mt );

			mt = new MimeType();
			mt.Comment = "Troff ME input document";
			MimeTypes.Add( "text/x-troff-me", mt );

			mt = new MimeType();
			mt.Comment = "Troff MM input document";
			MimeTypes.Add( "text/x-troff-mm", mt );

			mt = new MimeType();
			mt.Comment = "Troff MS input document";
			MimeTypes.Add( "text/x-troff-ms", mt );

			mt = new MimeType();
			mt.Comment = "X-Motif UIL table";
			MimeTypes.Add( "text/x-uil", mt );

			mt = new MimeType();
			mt.Comment = "resource location";
			MimeTypes.Add( "text/x-uri", mt );

			mt = new MimeType();
			mt.Comment = "XML Metadata Interchange file";
			MimeTypes.Add( "text/x-xmi", mt );

			mt = new MimeType();
			mt.Comment = "XSL Formating Object file";
			MimeTypes.Add( "text/x-xslfo", mt );

			mt = new MimeType();
			mt.Comment = "XSLT stylesheet";
			MimeTypes.Add( "application/xml", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "text/xmcd", mt );

			mt = new MimeType();
			mt.Comment = "eXtensible Markup Language document";
			MimeTypes.Add( "text/xml", mt );

			mt = new MimeType();
			mt.Comment = "ISI video";
			MimeTypes.Add( "video/isivideo", mt );

			mt = new MimeType();
			mt.Comment = "MPEG video";
			MimeTypes.Add( "video/mpeg", mt );

			mt = new MimeType();
			mt.Comment = "QuickTime video";
			MimeTypes.Add( "video/quicktime", mt );

			mt = new MimeType();
			mt.Comment = "Vivo video";
			MimeTypes.Add( "video/vivo", mt );

			mt = new MimeType();
			mt.Comment = "Wavelet video";
			MimeTypes.Add( "video/wavelet", mt );

			mt = new MimeType();
			mt.Comment = "ANIM animation";
			MimeTypes.Add( "video/x-anim", mt );

			mt = new MimeType();
			mt.Comment = "AVI video";
			MimeTypes.Add( "video/x-avi", mt );

			mt = new MimeType();
			mt.Comment = "AutoDesk FLIC animation";
			MimeTypes.Add( "video/x-flic", mt );

			mt = new MimeType();
			mt.Comment = "MNG animation";
			MimeTypes.Add( "video/x-mng", mt );

			mt = new MimeType();
			mt.Comment = "Microsoft ASF video";
			MimeTypes.Add( "video/x-ms-asf", mt );

			mt = new MimeType();
			mt.Comment = "Microsoft WMV video";
			MimeTypes.Add( "video/x-ms-wmv", mt );

			mt = new MimeType();
			mt.Comment = "Microsoft AVI video";
			MimeTypes.Add( "video/x-msvideo", mt );

			mt = new MimeType();
			mt.Comment = "Nullsoft video";
			MimeTypes.Add( "video/x-nsv", mt );

			mt = new MimeType();
			mt.Comment = "RealVideo video";
			MimeTypes.Add( "video/x-real-video", mt );

			mt = new MimeType();
			mt.Comment = "SGI video";
			MimeTypes.Add( "video/x-sgi-movie", mt );

			mt = new MimeType();
			mt.Comment = "Canon RAW File";
			MimeTypes.Add( "application/x-crw", mt );

			mt = new MimeType();
			mt.Comment = "Plan Perfect document";
			MimeTypes.Add( "application/x-planperfect", mt );

			mt = new MimeType();
			mt.Comment = "Linear and integer program expression format";
			MimeTypes.Add( "application/x-mps", mt );

			mt = new MimeType();
			mt.Comment = "gDesklets display";
			MimeTypes.Add( "application/x-gdesklets-display", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "text/x-msil", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "text/x-nemerle", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "text/x-vb", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "text/x-js", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-aspx", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-ashx", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-ascx", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-asix", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-axd", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-web-config", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-machine-config", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-config", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-master-page", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-resources", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-resourcesx", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-remoting", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-soap-remoting", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-asmx", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-prjx", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-cmbx", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-mdsx", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-mdp", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-mds", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-disco", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-asax", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-wsdl", mt );

			mt = new MimeType();
			mt.Comment = "Planner project plan";
			MimeTypes.Add( "application/x-planner", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-scribus", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-vmware-vm", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-vmware-vmdisk", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-vmware-team", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-vmware-snapshot", mt );

			mt = new MimeType();
			mt.Comment = "";
			MimeTypes.Add( "application/x-vmware-vmfoundry", mt );

			Match match0 = null;

			match0 = new Match();
			match0.MimeType = "application/mac-binhex40";
			match0.Priority = 50;
			match0.Offset = 11;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 29 ] { 
					109, 117, 115, 116, 32, 98, 101, 32, 
					99, 111, 110, 118, 101, 114, 116, 101, 
					100, 32, 119, 105, 116, 104, 32, 66, 
					105, 110, 72, 101, 120 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/mathematica";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 53 ] { 
					40, 42, 42, 42, 42, 42, 42, 42, 
					42, 42, 42, 42, 42, 42, 42, 32, 
					67, 111, 110, 116, 101, 110, 116, 45, 
					116, 121, 112, 101, 58, 32, 97, 112, 
					112, 108, 105, 99, 97, 116, 105, 111, 
					110, 47, 109, 97, 116, 104, 101, 109, 
					97, 116, 105, 99, 97 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/mathematica";
			match0.Priority = 50;
			match0.Offset = 100;
			match0.OffsetLength = 156;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 65 ] { 
					84, 104, 105, 115, 32, 110, 111, 116, 
					101, 98, 111, 111, 107, 32, 99, 97, 
					110, 32, 98, 101, 32, 117, 115, 101, 
					100, 32, 111, 110, 32, 97, 110, 121, 
					32, 99, 111, 109, 112, 117, 116, 101, 
					114, 32, 115, 121, 115, 116, 101, 109, 
					32, 119, 105, 116, 104, 32, 77, 97, 
					116, 104, 101, 109, 97, 116, 105, 99, 
					97 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/mathematica";
			match0.Priority = 50;
			match0.Offset = 10;
			match0.OffsetLength = 246;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 60 ] { 
					84, 104, 105, 115, 32, 105, 115, 32, 
					97, 32, 77, 97, 116, 104, 101, 109, 
					97, 116, 105, 99, 97, 32, 78, 111, 
					116, 101, 98, 111, 111, 107, 32, 102, 
					105, 108, 101, 46, 32, 32, 73, 116, 
					32, 99, 111, 110, 116, 97, 105, 110, 
					115, 32, 65, 83, 67, 73, 73, 32, 
					116, 101, 120, 116 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/octet-stream";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 30 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/octet-stream";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeHost16;
			match0.WordSize = 2;
			match0.ByteValue = new byte[ 2 ] { 
					31, 31 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/octet-stream";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeHost16;
			match0.WordSize = 2;
			match0.ByteValue = new byte[ 2 ] { 
					31, 255 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/octet-stream";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					255, 31 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/octet-stream";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeHost16;
			match0.WordSize = 2;
			match0.ByteValue = new byte[ 2 ] { 
					5, 203 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/pdf";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					37, 80, 68, 70, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/postscript";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					4, 37, 33 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/postscript";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					37, 33 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/rtf";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					123, 92, 114, 116, 102 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/smil";
			match0.Priority = 80;
			match0.Offset = 0;
			match0.OffsetLength = 256;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					60, 115, 109, 105, 108, 32 };
			Matches80Plus.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/stuffit";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 7 ] { 
					83, 116, 117, 102, 102, 73, 116 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-gedcom";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					48, 32, 72, 69, 65, 68 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/vnd.corel-draw";
			match0.Priority = 80;
			match0.Offset = 8;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					67, 68, 82, 88, 118, 114, 115, 110 };
			match0.Mask = new byte[ 8 ] { 
					255, 255, 255, 0, 255, 255, 255, 255 };
			Matches80Plus.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/vnd.lotus-1-2-3";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 14 ] { 
					0, 0, 2, 0, 6, 4, 6, 0, 
					8, 0, 0, 0, 0, 0 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/vnd.ms-access";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 19 ] { 
					0, 1, 0, 0, 83, 116, 97, 110, 
					100, 97, 114, 100, 32, 74, 101, 116, 
					32, 68, 66 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/vnd.ms-excel";
			match0.Priority = 50;
			match0.Offset = 2080;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 29 ] { 
					77, 105, 99, 114, 111, 115, 111, 102, 
					116, 32, 69, 120, 99, 101, 108, 32, 
					53, 46, 48, 32, 87, 111, 114, 107, 
					115, 104, 101, 101, 116 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/vnd.ms-powerpoint";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle32;
			match0.ByteValue = new byte[ 4 ] { 
					207, 208, 224, 17 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/msword";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					49, 190, 0, 0 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/msword";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					80, 79, 94, 81, 96 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/msword";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					254, 55, 0, 35 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/msword";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					219, 165, 45, 0, 0, 0 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/msword";
			match0.Priority = 50;
			match0.Offset = 2080;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 27 ] { 
					77, 105, 99, 114, 111, 115, 111, 102, 
					116, 32, 87, 111, 114, 100, 32, 54, 
					46, 48, 32, 68, 111, 99, 117, 109, 
					101, 110, 116 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/msword";
			match0.Priority = 50;
			match0.Offset = 2112;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 28 ] { 
					77, 105, 99, 114, 111, 115, 111, 102, 
					116, 32, 87, 111, 114, 100, 32, 100, 
					111, 99, 117, 109, 101, 110, 116, 32, 
					100, 97, 116, 97 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/vnd.sun.xml.calc";
			match0.Priority = 50;
			match0.Offset = 30;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 36 ] { 
					109, 105, 109, 101, 116, 121, 112, 101, 
					97, 112, 112, 108, 105, 99, 97, 116, 
					105, 111, 110, 47, 118, 110, 100, 46, 
					115, 117, 110, 46, 120, 109, 108, 46, 
					99, 97, 108, 99 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/vnd.wordperfect";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					87, 80, 67 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-xbel";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 14 ] { 
					60, 33, 68, 79, 67, 84, 89, 80, 
					69, 32, 120, 98, 101, 108 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-abiword";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					60, 97, 98, 105, 119, 111, 114, 100 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-abiword";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 17 ] { 
					60, 33, 68, 79, 67, 84, 89, 80, 
					69, 32, 97, 98, 105, 119, 111, 114, 
					100 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-applix-spreadsheet";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 19 ] { 
					42, 66, 69, 71, 73, 78, 32, 83, 
					80, 82, 69, 65, 68, 83, 72, 69, 
					69, 84, 83 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-applix-spreadsheet";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					42, 66, 69, 71, 73, 78 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 7;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 12 ] { 
						83, 80, 82, 69, 65, 68, 83, 72, 
						69, 69, 84, 83 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-applix-word";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					42, 66, 69, 71, 73, 78 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 7;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 5 ] { 
						87, 79, 82, 68, 83 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-arc";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle32;
			match0.ByteValue = new byte[ 4 ] { 
					0, 0, 8, 26 };
			match0.Mask = new byte[ 4 ] { 
					128, 128, 255, 255 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-arc";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle32;
			match0.ByteValue = new byte[ 4 ] { 
					0, 0, 9, 26 };
			match0.Mask = new byte[ 4 ] { 
					128, 128, 255, 255 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-arc";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle32;
			match0.ByteValue = new byte[ 4 ] { 
					0, 0, 2, 26 };
			match0.Mask = new byte[ 4 ] { 
					128, 128, 255, 255 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-arc";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle32;
			match0.ByteValue = new byte[ 4 ] { 
					0, 0, 3, 26 };
			match0.Mask = new byte[ 4 ] { 
					128, 128, 255, 255 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-arc";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle32;
			match0.ByteValue = new byte[ 4 ] { 
					0, 0, 4, 26 };
			match0.Mask = new byte[ 4 ] { 
					128, 128, 255, 255 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-arc";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle32;
			match0.ByteValue = new byte[ 4 ] { 
					0, 0, 6, 26 };
			match0.Mask = new byte[ 4 ] { 
					128, 128, 255, 255 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-archive";
			match0.Priority = 45;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					60, 97, 114, 62 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-archive";
			match0.Priority = 45;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 7 ] { 
					33, 60, 97, 114, 99, 104, 62 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-arj";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle16;
			match0.ByteValue = new byte[ 2 ] { 
					234, 96 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-awk";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 11 ] { 
					35, 33, 47, 98, 105, 110, 47, 103, 
					97, 119, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-awk";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 12 ] { 
					35, 33, 32, 47, 98, 105, 110, 47, 
					103, 97, 119, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-awk";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 15 ] { 
					35, 33, 47, 117, 115, 114, 47, 98, 
					105, 110, 47, 103, 97, 119, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-awk";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 16 ] { 
					35, 33, 32, 47, 117, 115, 114, 47, 
					98, 105, 110, 47, 103, 97, 119, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-awk";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 21 ] { 
					35, 33, 47, 117, 115, 114, 47, 108, 
					111, 99, 97, 108, 47, 98, 105, 110, 
					47, 103, 97, 119, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-awk";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 22 ] { 
					35, 33, 32, 47, 117, 115, 114, 47, 
					108, 111, 99, 97, 108, 47, 98, 105, 
					110, 47, 103, 97, 119, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-awk";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 10 ] { 
					35, 33, 47, 98, 105, 110, 47, 97, 
					119, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-awk";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 11 ] { 
					35, 33, 32, 47, 98, 105, 110, 47, 
					97, 119, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-awk";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 14 ] { 
					35, 33, 47, 117, 115, 114, 47, 98, 
					105, 110, 47, 97, 119, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-awk";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 15 ] { 
					35, 33, 32, 47, 117, 115, 114, 47, 
					98, 105, 110, 47, 97, 119, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-bittorrent";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 11 ] { 
					100, 56, 58, 97, 110, 110, 111, 117, 
					110, 99, 101 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-blender";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 7 ] { 
					66, 76, 69, 78, 68, 69, 82 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-bzip";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					66, 90, 104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-compress";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 157 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-core";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 17 ] { 
					127, 69, 76, 70, 32, 32, 32, 32, 
					32, 32, 32, 32, 32, 32, 32, 32, 
					4 };
			match0.Mask = new byte[ 17 ] { 
					255, 255, 255, 255, 0, 0, 0, 0, 
					0, 0, 0, 0, 0, 0, 0, 0, 
					255 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-core";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					127, 69, 76, 70 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 5;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						1 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 16;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeLittle16;
					match2.ByteValue = new byte[ 2 ] { 
							4, 0 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-core";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					127, 69, 76, 70 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 5;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						2 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 16;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeBig16;
					match2.ByteValue = new byte[ 2 ] { 
							0, 4 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-core";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					67, 111, 114, 101, 1 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-core";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					67, 111, 114, 101, 2 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-cpio";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeHost16;
			match0.WordSize = 2;
			match0.ByteValue = new byte[ 2 ] { 
					199, 113 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-cpio";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					48, 55, 48, 55, 48, 49 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-cpio";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					48, 55, 48, 55, 48, 50 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-cpio";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeHost16;
			match0.WordSize = 2;
			match0.ByteValue = new byte[ 2 ] { 
					113, 199 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-csh";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 15;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 9 ] { 
					47, 98, 105, 110, 47, 116, 99, 115, 
					104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-csh";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 15;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					47, 98, 105, 110, 47, 99, 115, 104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-csh";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 18 ] { 
					35, 33, 47, 117, 115, 114, 47, 98, 
					105, 110, 47, 101, 110, 118, 32, 99, 
					115, 104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-csh";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 19 ] { 
					35, 33, 47, 117, 115, 114, 47, 98, 
					105, 110, 47, 101, 110, 118, 32, 116, 
					99, 115, 104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-dbm";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 3 ] { 
					6, 21, 97 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-deb";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 7 ] { 
					33, 60, 97, 114, 99, 104, 62 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 8;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 6 ] { 
						100, 101, 98, 105, 97, 110 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-desktop";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 32;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 15 ] { 
					91, 68, 101, 115, 107, 116, 111, 112, 
					32, 69, 110, 116, 114, 121, 93 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-desktop";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 15 ] { 
					91, 68, 101, 115, 107, 116, 111, 112, 
					32, 65, 99, 116, 105, 111, 110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-desktop";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 19 ] { 
					91, 75, 68, 69, 32, 68, 101, 115, 
					107, 116, 111, 112, 32, 69, 110, 116, 
					114, 121, 93 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-desktop";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 13 ] { 
					35, 32, 67, 111, 110, 102, 105, 103, 
					32, 70, 105, 108, 101 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-desktop";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 17 ] { 
					35, 32, 75, 68, 69, 32, 67, 111, 
					110, 102, 105, 103, 32, 70, 105, 108, 
					101 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-dia-diagram";
			match0.Priority = 50;
			match0.Offset = 5;
			match0.OffsetLength = 95;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					60, 100, 105, 97, 58 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-dvi";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle16;
			match0.ByteValue = new byte[ 2 ] { 
					2, 247 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-executable";
			match0.Priority = 40;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					127, 69, 76, 70 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 5;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						1 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 16;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeLittle16;
					match2.ByteValue = new byte[ 2 ] { 
							2, 0 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-executable";
			match0.Priority = 40;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					127, 69, 76, 70 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 5;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						2 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 16;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeBig16;
					match2.ByteValue = new byte[ 2 ] { 
							0, 2 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-executable";
			match0.Priority = 40;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					77, 90 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-executable";
			match0.Priority = 40;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle16;
			match0.ByteValue = new byte[ 2 ] { 
					82, 28 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-executable";
			match0.Priority = 40;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeHost16;
			match0.WordSize = 2;
			match0.ByteValue = new byte[ 2 ] { 
					16, 1 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-executable";
			match0.Priority = 40;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeHost16;
			match0.WordSize = 2;
			match0.ByteValue = new byte[ 2 ] { 
					17, 1 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-executable";
			match0.Priority = 40;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle16;
			match0.ByteValue = new byte[ 2 ] { 
					131, 1 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-type1";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					76, 87, 70, 78 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-type1";
			match0.Priority = 50;
			match0.Offset = 65;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					76, 87, 70, 78 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-type1";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 17 ] { 
					37, 33, 80, 83, 45, 65, 100, 111, 
					98, 101, 70, 111, 110, 116, 45, 49, 
					46 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-type1";
			match0.Priority = 50;
			match0.Offset = 6;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 17 ] { 
					37, 33, 80, 83, 45, 65, 100, 111, 
					98, 101, 70, 111, 110, 116, 45, 49, 
					46 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-type1";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 14 ] { 
					37, 33, 70, 111, 110, 116, 84, 121, 
					112, 101, 49, 45, 49, 46 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-type1";
			match0.Priority = 50;
			match0.Offset = 6;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 14 ] { 
					37, 33, 70, 111, 110, 116, 84, 121, 
					112, 101, 49, 45, 49, 46 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-bdf";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 10 ] { 
					83, 84, 65, 82, 84, 70, 79, 78, 
					84, 32 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-dos";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					255, 70, 79, 78 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-dos";
			match0.Priority = 50;
			match0.Offset = 7;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					0, 69, 71, 65 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-dos";
			match0.Priority = 50;
			match0.Offset = 7;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					0, 86, 73, 68 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-framemaker";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 16 ] { 
					60, 77, 97, 107, 101, 114, 83, 99, 
					114, 101, 101, 110, 70, 111, 110, 116 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-libgrx";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					20, 2, 89, 25 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-linux-psf";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					54, 4 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-pcf";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					1, 102, 99, 112 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-otf";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					79, 84, 84, 79 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-speedo";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					68, 49, 46, 48, 13 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-sunos-news";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 9 ] { 
					83, 116, 97, 114, 116, 70, 111, 110, 
					116 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-sunos-news";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					19, 122, 41 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-sunos-news";
			match0.Priority = 50;
			match0.Offset = 8;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					19, 122, 43 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-tex";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					247, 131 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-tex";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					247, 89 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-tex";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					247, 202 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-tex-tfm";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					0, 17 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-tex-tfm";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					0, 18 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-ttf";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					70, 70, 73, 76 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-ttf";
			match0.Priority = 50;
			match0.Offset = 65;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					70, 70, 73, 76 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-ttf";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					0, 1, 0, 0, 0 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-font-vfont";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					70, 79, 78, 84 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-frame";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 10 ] { 
					60, 77, 97, 107, 101, 114, 70, 105, 
					108, 101 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-frame";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					60, 77, 73, 70, 70, 105, 108, 101 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-frame";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 16 ] { 
					60, 77, 97, 107, 101, 114, 68, 105, 
					99, 116, 105, 111, 110, 97, 114, 121 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-frame";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 15 ] { 
					60, 77, 97, 107, 101, 114, 83, 99, 
					114, 101, 101, 110, 70, 111, 110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-frame";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					60, 77, 77, 76 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-frame";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					60, 66, 111, 111, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-frame";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					60, 77, 97, 107, 101, 114 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-gdbm";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 4 ] { 
					19, 87, 154, 206 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-gdbm";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle32;
			match0.ByteValue = new byte[ 4 ] { 
					19, 87, 154, 206 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-gdbm";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					71, 68, 66, 77 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-gmc-link";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 32;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					85, 82, 76, 58 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-gnumeric";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 12 ] { 
					103, 109, 114, 58, 87, 111, 114, 107, 
					98, 111, 111, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-gtktalog";
			match0.Priority = 50;
			match0.Offset = 4;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 9 ] { 
					103, 116, 107, 116, 97, 108, 111, 103, 
					32 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-gzip";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 139 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-ipod-firmware";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 7 ] { 
					83, 32, 84, 32, 79, 32, 80 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-java";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeHost16;
			match0.WordSize = 2;
			match0.ByteValue = new byte[ 2 ] { 
					202, 254 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 2;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeHost16;
				match1.WordSize = 2;
				match1.ByteValue = new byte[ 2 ] { 
						186, 190 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-java-jnlp-file";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					60, 106, 110, 108, 112 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-karbon";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 139 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 10;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 7 ] { 
						75, 79, 102, 102, 105, 99, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 18;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 22 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 97, 
							114, 98, 111, 110, 4, 6 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-karbon";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					80, 75, 3, 4 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 30;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 8 ] { 
						109, 105, 109, 101, 116, 121, 112, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 38;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 20 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 97, 
							114, 98, 111, 110 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kchart";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 139 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 10;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 7 ] { 
						75, 79, 102, 102, 105, 99, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 18;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 22 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 99, 
							104, 97, 114, 116, 4, 6 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kchart";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					80, 75, 3, 4 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 30;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 8 ] { 
						109, 105, 109, 101, 116, 121, 112, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 38;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 20 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 99, 
							104, 97, 114, 116 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kformula";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 139 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 10;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 7 ] { 
						75, 79, 102, 102, 105, 99, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 18;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 24 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 102, 
							111, 114, 109, 117, 108, 97, 4, 6 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kformula";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					80, 75, 3, 4 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 30;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 8 ] { 
						109, 105, 109, 101, 116, 121, 112, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 38;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 22 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 102, 
							111, 114, 109, 117, 108, 97 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-killustrator";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 139 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 10;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 7 ] { 
						75, 79, 102, 102, 105, 99, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 18;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 28 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 105, 
							108, 108, 117, 115, 116, 114, 97, 116, 
							111, 114, 4, 6 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kivio";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 139 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 10;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 7 ] { 
						75, 79, 102, 102, 105, 99, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 18;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 21 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 105, 
							118, 105, 111, 4, 6 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kivio";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					80, 75, 3, 4 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 30;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 8 ] { 
						109, 105, 109, 101, 116, 121, 112, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 38;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 19 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 105, 
							118, 105, 111 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kontour";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 139 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 10;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 7 ] { 
						75, 79, 102, 102, 105, 99, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 18;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 23 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 111, 
							110, 116, 111, 117, 114, 4, 6 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kontour";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					80, 75, 3, 4 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 30;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 8 ] { 
						109, 105, 109, 101, 116, 121, 112, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 38;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 21 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 111, 
							110, 116, 111, 117, 114 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kpresenter";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 139 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 10;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 7 ] { 
						75, 79, 102, 102, 105, 99, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 18;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 26 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 112, 
							114, 101, 115, 101, 110, 116, 101, 114, 
							4, 6 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kpresenter";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					80, 75, 3, 4 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 30;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 8 ] { 
						109, 105, 109, 101, 116, 121, 112, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 38;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 24 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 112, 
							114, 101, 115, 101, 110, 116, 101, 114 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-krita";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 139 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 10;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 7 ] { 
						75, 79, 102, 102, 105, 99, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 18;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 21 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 114, 
							105, 116, 97, 4, 6 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-krita";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					80, 75, 3, 4 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 30;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 8 ] { 
						109, 105, 109, 101, 116, 121, 112, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 38;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 19 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 114, 
							105, 116, 97 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kspread";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 139 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 10;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 7 ] { 
						75, 79, 102, 102, 105, 99, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 18;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 23 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 115, 
							112, 114, 101, 97, 100, 4, 6 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kspread";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					80, 75, 3, 4 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 30;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 8 ] { 
						109, 105, 109, 101, 116, 121, 112, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 38;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 21 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 115, 
							112, 114, 101, 97, 100 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kspread-crypt";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 4 ] { 
					13, 26, 39, 2 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-ksysv-package";
			match0.Priority = 50;
			match0.Offset = 4;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					75, 83, 121, 115, 86 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kword";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					31, 139 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 10;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 7 ] { 
						75, 79, 102, 102, 105, 99, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 18;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 21 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 119, 
							111, 114, 100, 4, 6 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kword";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					80, 75, 3, 4 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 30;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 8 ] { 
						109, 105, 109, 101, 116, 121, 112, 101 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 38;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeString;
					match2.ByteValue = new byte[ 19 ] { 
							97, 112, 112, 108, 105, 99, 97, 116, 
							105, 111, 110, 47, 120, 45, 107, 119, 
							111, 114, 100 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-kword-crypt";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 4 ] { 
					13, 26, 39, 1 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-lha";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					45, 108, 104, 32, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-lha";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					45, 108, 104, 48, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-lha";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					45, 108, 104, 49, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-lha";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					45, 108, 104, 50, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-lha";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					45, 108, 104, 51, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-lha";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					45, 108, 104, 52, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-lha";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					45, 108, 104, 53, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-lha";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					45, 108, 104, 52, 48, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-lha";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					45, 108, 104, 100, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-lha";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					45, 108, 122, 52, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-lha";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					45, 108, 122, 53, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-lha";
			match0.Priority = 50;
			match0.Offset = 2;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					45, 108, 122, 115, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-lyx";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					35, 76, 121, 88 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-macbinary";
			match0.Priority = 50;
			match0.Offset = 102;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					109, 66, 73, 78 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-matroska";
			match0.Priority = 50;
			match0.Offset = 8;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					109, 97, 116, 114, 111, 115, 107, 97 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-mozilla-bookmarks";
			match0.Priority = 80;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 35 ] { 
					60, 33, 68, 79, 67, 84, 89, 80, 
					69, 32, 78, 69, 84, 83, 67, 65, 
					80, 69, 45, 66, 111, 111, 107, 109, 
					97, 114, 107, 45, 102, 105, 108, 101, 
					45, 49, 62 };
			Matches80Plus.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-ms-dos-executable";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					77, 90 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-mswinurl";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 16 ] { 
					73, 110, 116, 101, 114, 110, 101, 116, 
					83, 104, 111, 114, 116, 99, 117, 116 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-nautilus-link";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 32;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 30 ] { 
					60, 110, 97, 117, 116, 105, 108, 117, 
					115, 95, 111, 98, 106, 101, 99, 116, 
					32, 110, 97, 117, 116, 105, 108, 117, 
					115, 95, 108, 105, 110, 107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-netscape-bookmarks";
			match0.Priority = 80;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 35 ] { 
					60, 33, 68, 79, 67, 84, 89, 80, 
					69, 32, 78, 69, 84, 83, 67, 65, 
					80, 69, 45, 66, 111, 111, 107, 109, 
					97, 114, 107, 45, 102, 105, 108, 101, 
					45, 49, 62 };
			Matches80Plus.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-object";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					127, 69, 76, 70 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 5;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						1 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 16;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeLittle16;
					match2.ByteValue = new byte[ 2 ] { 
							1, 0 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-object";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					127, 69, 76, 70 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 5;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						2 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 16;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeBig16;
					match2.ByteValue = new byte[ 2 ] { 
							0, 1 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/ogg";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					79, 103, 103, 83 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-ole-storage";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					208, 207, 17, 224, 161, 177, 26, 225 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-oleo";
			match0.Priority = 50;
			match0.Offset = 31;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					79, 108, 101, 111 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-par2";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					80, 65, 82, 50 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-pef-executable";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					74, 111, 121, 33 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-perl";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 30 ] { 
					101, 118, 97, 108, 32, 34, 101, 120, 
					101, 99, 32, 47, 117, 115, 114, 47, 
					108, 111, 99, 97, 108, 47, 98, 105, 
					110, 47, 112, 101, 114, 108 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-perl";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 15;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 9 ] { 
					47, 98, 105, 110, 47, 112, 101, 114, 
					108 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-perl";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 15;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 13 ] { 
					47, 98, 105, 110, 47, 101, 110, 118, 
					32, 112, 101, 114, 108 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-php";
			match0.Priority = 80;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					60, 63, 112, 104, 112 };
			Matches80Plus.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-python-bytecode";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 4 ] { 
					153, 78, 13, 10 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-rar";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					82, 97, 114, 33 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-rpm";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					237, 171, 238, 219 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-ruby";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 15;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 13 ] { 
					47, 98, 105, 110, 47, 101, 110, 118, 
					32, 114, 117, 98, 121 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-ruby";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 15;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 9 ] { 
					47, 98, 105, 110, 47, 114, 117, 98, 
					121 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-sc";
			match0.Priority = 50;
			match0.Offset = 38;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 11 ] { 
					83, 112, 114, 101, 97, 100, 115, 104, 
					101, 101, 116 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-sharedlib";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					127, 69, 76, 70 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 5;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						1 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 16;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeLittle16;
					match2.ByteValue = new byte[ 2 ] { 
							3, 0 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-sharedlib";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					127, 69, 76, 70 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 5;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						2 };

				if ( match1.Matches.Count > 0 )
				{
					Match match2 = null;
					match2 = new Match();
					match2.Offset = 16;
					match2.OffsetLength = 1;
					match2.MatchType = MatchTypes.TypeBig16;
					match2.ByteValue = new byte[ 2 ] { 
							0, 3 };
					match1.Matches.Add( match2 );

				}
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-sharedlib";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle16;
			match0.ByteValue = new byte[ 2 ] { 
					131, 1 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 22;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeLittle16;
				match1.ByteValue = new byte[ 2 ] { 
						0, 32 };
				match1.Mask = new byte[ 3 ] { 
						3, 0, 0 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-sharedlib";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 17 ] { 
					127, 69, 76, 70, 32, 32, 32, 32, 
					32, 32, 32, 32, 32, 32, 32, 32, 
					3 };
			match0.Mask = new byte[ 17 ] { 
					255, 255, 255, 255, 0, 0, 0, 0, 
					0, 0, 0, 0, 0, 0, 0, 0, 
					255 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-shellscript";
			match0.Priority = 50;
			match0.Offset = 10;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 25 ] { 
					35, 32, 84, 104, 105, 115, 32, 105, 
					115, 32, 97, 32, 115, 104, 101, 108, 
					108, 32, 97, 114, 99, 104, 105, 118, 
					101 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-shellscript";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 15;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 9 ] { 
					47, 98, 105, 110, 47, 98, 97, 115, 
					104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-shellscript";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 15;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 9 ] { 
					47, 98, 105, 110, 47, 110, 97, 119, 
					107 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-shellscript";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 15;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					47, 98, 105, 110, 47, 122, 115, 104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-shellscript";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 15;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 7 ] { 
					47, 98, 105, 110, 47, 115, 104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-shellscript";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 15;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					47, 98, 105, 110, 47, 107, 115, 104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-shellscript";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 17 ] { 
					35, 33, 47, 117, 115, 114, 47, 98, 
					105, 110, 47, 101, 110, 118, 32, 115, 
					104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-shellscript";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 19 ] { 
					35, 33, 47, 117, 115, 114, 47, 98, 
					105, 110, 47, 101, 110, 118, 32, 98, 
					97, 115, 104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-shellscript";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 18 ] { 
					35, 33, 47, 117, 115, 114, 47, 98, 
					105, 110, 47, 101, 110, 118, 32, 122, 
					115, 104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-shellscript";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 18 ] { 
					35, 33, 47, 117, 115, 114, 47, 98, 
					105, 110, 47, 101, 110, 118, 32, 107, 
					115, 104 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-shockwave-flash";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					70, 87, 83 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-stuffit";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					83, 116, 117, 102, 102, 73, 116, 32 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-stuffit";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					83, 73, 84, 33 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-tar";
			match0.Priority = 50;
			match0.Offset = 257;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					117, 115, 116, 97, 114, 0 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-tar";
			match0.Priority = 50;
			match0.Offset = 257;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					117, 115, 116, 97, 114, 32, 32, 0 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-tgif";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					37, 84, 71, 73, 70 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-troff";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					46, 92, 34 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-troff";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					39, 92, 34 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-troff";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					39, 46, 92, 34 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-troff";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					92, 34 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-zoo";
			match0.Priority = 50;
			match0.Offset = 20;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle32;
			match0.ByteValue = new byte[ 4 ] { 
					253, 196, 167, 220 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/zip";
			match0.Priority = 40;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					80, 75, 3, 4 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/basic";
			match0.Priority = 40;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					46, 115, 110, 100 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/prs.sid";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					80, 83, 73, 68 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-adpcm";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					46, 115, 110, 100 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeBig32;
				match1.ByteValue = new byte[ 4 ] { 
						0, 0, 0, 19 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-adpcm";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle32;
			match0.ByteValue = new byte[ 4 ] { 
					0, 100, 115, 46 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeLittle32;
				match1.ByteValue = new byte[ 4 ] { 
						1, 0, 0, 0 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeLittle32;
				match1.ByteValue = new byte[ 4 ] { 
						2, 0, 0, 0 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeLittle32;
				match1.ByteValue = new byte[ 4 ] { 
						3, 0, 0, 0 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeLittle32;
				match1.ByteValue = new byte[ 4 ] { 
						4, 0, 0, 0 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeLittle32;
				match1.ByteValue = new byte[ 4 ] { 
						5, 0, 0, 0 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeLittle32;
				match1.ByteValue = new byte[ 4 ] { 
						6, 0, 0, 0 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeLittle32;
				match1.ByteValue = new byte[ 4 ] { 
						7, 0, 0, 0 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeLittle32;
				match1.ByteValue = new byte[ 4 ] { 
						19, 0, 0, 0 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-aifc";
			match0.Priority = 50;
			match0.Offset = 8;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					65, 73, 70, 67 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-aiff";
			match0.Priority = 50;
			match0.Offset = 8;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					65, 73, 70, 70 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-aiff";
			match0.Priority = 50;
			match0.Offset = 8;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					65, 73, 70, 67 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-aiff";
			match0.Priority = 50;
			match0.Offset = 8;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					56, 83, 86, 88 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-it";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					73, 77, 80, 77 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/midi";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					77, 84, 104, 100 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/mp4";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 7 ] { 
					102, 116, 121, 112, 77, 52, 65 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/mp4";
			match0.Priority = 50;
			match0.Offset = 4;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					102, 116, 121, 112, 105, 115, 111, 109 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/mp4";
			match0.Priority = 50;
			match0.Offset = 4;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					102, 116, 121, 112, 109, 112, 52, 50 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/mpeg";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 2 ] { 
					255, 251 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/mpeg";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					73, 68, 51 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-mpegurl";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 7 ] { 
					35, 69, 88, 84, 77, 51, 85 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-ms-asx";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					65, 83, 70, 32 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-ms-asx";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					60, 65, 83, 88 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-ms-asx";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					60, 97, 115, 120 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-pn-realaudio";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					46, 114, 97, 253 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-pn-realaudio";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					46, 82, 77, 70 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-riff";
			match0.Priority = 45;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					82, 73, 70, 70 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-scpls";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 10 ] { 
					91, 112, 108, 97, 121, 108, 105, 115, 
					116, 93 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-scpls";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 10 ] { 
					91, 80, 108, 97, 121, 108, 105, 115, 
					116, 93 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-scpls";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 10 ] { 
					91, 80, 76, 65, 89, 76, 73, 83, 
					84, 93 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-wav";
			match0.Priority = 50;
			match0.Offset = 8;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					87, 65, 86, 69 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "audio/x-wav";
			match0.Priority = 50;
			match0.Offset = 8;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					87, 65, 86, 32 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/bmp";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					66, 77, 120, 120, 120, 120, 0, 0 };
			match0.Mask = new byte[ 8 ] { 
					255, 255, 0, 0, 0, 0, 255, 255 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/bmp";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					66, 77 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 14;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						12 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 14;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						64 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 14;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						40 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/gif";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					71, 73, 70 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/jpeg";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					255, 216, 255 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/jpeg";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig16;
			match0.ByteValue = new byte[ 2 ] { 
					255, 216 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/png";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					137, 80, 78, 71 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/svg+xml";
			match0.Priority = 80;
			match0.Offset = 0;
			match0.OffsetLength = 256;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 13 ] { 
					60, 33, 68, 79, 67, 84, 89, 80, 
					69, 32, 115, 118, 103 };
			Matches80Plus.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/svg+xml";
			match0.Priority = 80;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					60, 115, 118, 103 };
			Matches80Plus.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/tiff";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					77, 77, 0, 42 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/tiff";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					73, 73, 42, 0 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-applix-graphics";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					42, 66, 69, 71, 73, 78 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 7;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 8 ] { 
						71, 82, 65, 80, 72, 73, 67, 83 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/dicom";
			match0.Priority = 50;
			match0.Offset = 128;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					68, 73, 67, 77 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-dib";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					40, 0, 0, 0 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/vnd.djvu";
			match0.Priority = 50;
			match0.Offset = 4;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					70, 79, 82, 77 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 4 ] { 
						68, 74, 86, 85 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 4 ] { 
						68, 74, 86, 77 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 4 ] { 
						66, 77, 52, 52 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 12;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 4 ] { 
						80, 77, 52, 52 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/dpx";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 4 ] { 
					83, 68, 80, 88 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-eps";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					37, 33 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 15;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 3 ] { 
						69, 80, 83 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-eps";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					4, 37, 33 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 16;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 3 ] { 
						69, 80, 83 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-fits";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 9 ] { 
					83, 73, 77, 80, 76, 69, 32, 32, 
					61 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-fpx";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 4 ] { 
					70, 80, 105, 120 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-icb";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 4 ] { 
					0, 0, 2, 0 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-niff";
			match0.Priority = 80;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					73, 73, 78, 49 };
			Matches80Plus.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-pcx";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeByte;
			match0.ByteValue = new byte[ 1 ] { 
					10 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 1;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						0 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 1;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						2 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 1;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						3 };
				match0.Matches.Add( match1 );

				match1 = new Match();
				match1.Offset = 1;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeByte;
				match1.ByteValue = new byte[ 1 ] { 
						5 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-portable-bitmap";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					80, 49 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-portable-bitmap";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					80, 52 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-portable-graymap";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					80, 50 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-portable-graymap";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					80, 53 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-portable-pixmap";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					80, 51 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-portable-pixmap";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					80, 54 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-psd";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 10 ] { 
					56, 66, 80, 83, 32, 32, 0, 0, 
					0, 0 };
			match0.Mask = new byte[ 10 ] { 
					255, 255, 255, 255, 0, 0, 255, 255, 
					255, 255 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-sun-raster";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 4 ] { 
					89, 166, 106, 149 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-xfig";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					35, 70, 73, 71 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "image/x-xpixmap";
			match0.Priority = 80;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					47, 42, 32, 88, 80, 77 };
			Matches80Plus.Add( match0 );

			match0 = new Match();
			match0.MimeType = "message/news";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 7 ] { 
					65, 114, 116, 105, 99, 108, 101 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "message/news";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					80, 97, 116, 104, 58 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "message/news";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					88, 114, 101, 102, 58 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "message/rfc822";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					35, 33, 32, 114, 110, 101, 119, 115 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "message/rfc822";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 10 ] { 
					70, 111, 114, 119, 97, 114, 100, 32, 
					116, 111 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "message/rfc822";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					70, 114, 111, 109, 58 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "message/rfc822";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 9 ] { 
					78, 35, 33, 32, 114, 110, 101, 119, 
					115 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "message/rfc822";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 7 ] { 
					80, 105, 112, 101, 32, 116, 111 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "message/rfc822";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 9 ] { 
					82, 101, 99, 101, 105, 118, 101, 100, 
					58 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "message/rfc822";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 14 ] { 
					82, 101, 108, 97, 121, 45, 86, 101, 
					114, 115, 105, 111, 110, 58 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "message/rfc822";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 12 ] { 
					82, 101, 116, 117, 114, 110, 45, 80, 
					97, 116, 104, 58 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/calendar";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 15 ] { 
					66, 69, 71, 73, 78, 58, 86, 67, 
					65, 76, 69, 78, 68, 65, 82 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/calendar";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 15 ] { 
					98, 101, 103, 105, 110, 58, 118, 99, 
					97, 108, 101, 110, 100, 97, 114 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/directory";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 11 ] { 
					66, 69, 71, 73, 78, 58, 86, 67, 
					65, 82, 68 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/directory";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 11 ] { 
					98, 101, 103, 105, 110, 58, 118, 99, 
					97, 114, 100 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/plain";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 12 ] { 
					84, 104, 105, 115, 32, 105, 115, 32, 
					84, 101, 88, 44 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/plain";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 17 ] { 
					84, 104, 105, 115, 32, 105, 115, 32, 
					77, 69, 84, 65, 70, 79, 78, 84, 
					44 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/spreadsheet";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					73, 68, 59 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-emacs-lisp";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 2 ] { 
					10, 40 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-emacs-lisp";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					59, 69, 76, 67, 19, 0, 0, 0 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 14 ] { 
					60, 33, 68, 79, 67, 84, 89, 80, 
					69, 32, 72, 84, 77, 76 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 14 ] { 
					60, 33, 100, 111, 99, 116, 121, 112, 
					101, 32, 104, 116, 109, 108 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					60, 72, 69, 65, 68 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					60, 104, 101, 97, 100 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					60, 84, 73, 84, 76, 69 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					60, 116, 105, 116, 108, 101 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					60, 104, 116, 109, 108 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 64;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					60, 72, 84, 77, 76 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					60, 66, 79, 68, 89 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					60, 98, 111, 100, 121 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					60, 84, 73, 84, 76, 69 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					60, 116, 105, 116, 108, 101 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					60, 33, 45, 45 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					60, 104, 49 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 3 ] { 
					60, 72, 49 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 14 ] { 
					60, 33, 100, 111, 99, 116, 121, 112, 
					101, 32, 72, 84, 77, 76 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/html";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 14 ] { 
					60, 33, 68, 79, 67, 84, 89, 80, 
					69, 32, 104, 116, 109, 108 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-ksysv-log";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 24 ] { 
					75, 68, 69, 32, 83, 121, 115, 116, 
					101, 109, 32, 86, 32, 73, 110, 105, 
					116, 32, 69, 100, 105, 116, 111, 114 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-patch";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					100, 105, 102, 102, 9 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-patch";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					100, 105, 102, 102, 32 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-patch";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					42, 42, 42, 9 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-patch";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					42, 42, 42, 32 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-patch";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					79, 110, 108, 121, 32, 105, 110, 9 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-patch";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					79, 110, 108, 121, 32, 105, 110, 32 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-patch";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 23 ] { 
					67, 111, 109, 109, 111, 110, 32, 115, 
					117, 98, 100, 105, 114, 101, 99, 116, 
					111, 114, 105, 101, 115, 58, 32 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-patch";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					73, 110, 100, 101, 120, 58 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-python";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 13 ] { 
					35, 33, 47, 98, 105, 110, 47, 112, 
					121, 116, 104, 111, 110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-python";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 14 ] { 
					35, 33, 32, 47, 98, 105, 110, 47, 
					112, 121, 116, 104, 111, 110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-python";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 22 ] { 
					101, 118, 97, 108, 32, 34, 101, 120, 
					101, 99, 32, 47, 98, 105, 110, 47, 
					112, 121, 116, 104, 111, 110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-python";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 17 ] { 
					35, 33, 47, 117, 115, 114, 47, 98, 
					105, 110, 47, 112, 121, 116, 104, 111, 
					110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-python";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 18 ] { 
					35, 33, 32, 47, 117, 115, 114, 47, 
					98, 105, 110, 47, 112, 121, 116, 104, 
					111, 110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-python";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 26 ] { 
					101, 118, 97, 108, 32, 34, 101, 120, 
					101, 99, 32, 47, 117, 115, 114, 47, 
					98, 105, 110, 47, 112, 121, 116, 104, 
					111, 110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-python";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 23 ] { 
					35, 33, 47, 117, 115, 114, 47, 108, 
					111, 99, 97, 108, 47, 98, 105, 110, 
					47, 112, 121, 116, 104, 111, 110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-python";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 24 ] { 
					35, 33, 32, 47, 117, 115, 114, 47, 
					108, 111, 99, 97, 108, 47, 98, 105, 
					110, 47, 112, 121, 116, 104, 111, 110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-python";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 32 ] { 
					101, 118, 97, 108, 32, 34, 101, 120, 
					101, 99, 32, 47, 117, 115, 114, 47, 
					108, 111, 99, 97, 108, 47, 98, 105, 
					110, 47, 112, 121, 116, 104, 111, 110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/x-python";
			match0.Priority = 50;
			match0.Offset = 1;
			match0.OffsetLength = 15;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 15 ] { 
					47, 98, 105, 110, 47, 101, 110, 118, 
					32, 112, 121, 116, 104, 111, 110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/xmcd";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 6 ] { 
					35, 32, 120, 109, 99, 100 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "text/xml";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 5 ] { 
					60, 63, 120, 109, 108 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/mpeg";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					71, 63, 255, 16 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/mpeg";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 4 ] { 
					0, 0, 1, 179 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/mpeg";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 4 ] { 
					0, 0, 1, 186 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/quicktime";
			match0.Priority = 50;
			match0.Offset = 12;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					109, 100, 97, 116 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/quicktime";
			match0.Priority = 50;
			match0.Offset = 4;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					109, 100, 97, 116 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/quicktime";
			match0.Priority = 50;
			match0.Offset = 4;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					109, 111, 111, 118 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/x-flic";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle16;
			match0.ByteValue = new byte[ 2 ] { 
					175, 17 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/x-flic";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeLittle16;
			match0.ByteValue = new byte[ 2 ] { 
					175, 18 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/x-ms-asf";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeBig32;
			match0.ByteValue = new byte[ 4 ] { 
					48, 38, 178, 117 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/x-msvideo";
			match0.Priority = 50;
			match0.Offset = 8;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					65, 86, 73, 32 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/x-msvideo";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					82, 73, 70, 70 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/x-nsv";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					78, 83, 86, 102 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "video/x-sgi-movie";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					77, 79, 86, 73 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-gdesklets-display";
			match0.Priority = 60;
			match0.Offset = 0;
			match0.OffsetLength = 128;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 8 ] { 
					60, 100, 105, 115, 112, 108, 97, 121 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-planner";
			match0.Priority = 50;
			match0.Offset = 20;
			match0.OffsetLength = 120;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 17 ] { 
					109, 114, 112, 114, 111, 106, 101, 99, 
					116, 45, 118, 101, 114, 115, 105, 111, 
					110 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-vmware-vm";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 4096;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 18 ] { 
					99, 111, 110, 102, 105, 103, 46, 118, 
					101, 114, 115, 105, 111, 110, 32, 61, 
					32, 34 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-vmware-vmdisk";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 21 ] { 
					35, 32, 68, 105, 115, 107, 32, 68, 
					101, 115, 99, 114, 105, 112, 116, 111, 
					114, 70, 105, 108, 101 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-vmware-vmdisk";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 4 ] { 
					75, 68, 77, 86 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-vmware-team";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 21 ] { 
					60, 70, 111, 117, 110, 100, 114, 121, 
					32, 118, 101, 114, 115, 105, 111, 110, 
					61, 34, 49, 34, 62 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 23;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 8 ] { 
						60, 86, 77, 84, 101, 97, 109, 62 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-vmware-snapshot";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 16 ] { 
					0, 120, 68, 48, 0, 120, 66, 69, 
					0, 120, 68, 48, 0, 120, 66, 69 };
			MatchesBelow80.Add( match0 );

			match0 = new Match();
			match0.MimeType = "application/x-vmware-vmfoundry";
			match0.Priority = 50;
			match0.Offset = 0;
			match0.OffsetLength = 1;
			match0.MatchType = MatchTypes.TypeString;
			match0.ByteValue = new byte[ 21 ] { 
					60, 70, 111, 117, 110, 100, 114, 121, 
					32, 118, 101, 114, 115, 105, 111, 110, 
					61, 34, 49, 34, 62 };

			if ( match0.Matches.Count > 0 )
			{
				Match match1 = null;
				match1 = new Match();
				match1.Offset = 23;
				match1.OffsetLength = 1;
				match1.MatchType = MatchTypes.TypeString;
				match1.ByteValue = new byte[ 4 ] { 
						60, 86, 77, 62 };
				match0.Matches.Add( match1 );

			}
			MatchesBelow80.Add( match0 );

		}
	}
}

#endregion

