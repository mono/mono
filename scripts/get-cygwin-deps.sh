if ! which wget > /dev/null 2>&1; then 
	echo "You must have the 'wget' package installed. Its in the 'web' section in the cygwin setup program."
	exit 1
fi

if ! which unzip > /dev/null 2>&1; then 
	echo "You must have the 'unzip' package installed."
	exit 1
fi

# Download glib for win32
rm -rf cygwin-deps
mkdir -p cygwin-deps
wget -P cygwin-deps ftp://ftp.gtk.org/pub/gtk/v2.6/win32/glib-2.6.6.zip || exit 1
wget -P cygwin-deps ftp://ftp.gtk.org/pub/gtk/v2.6/win32/glib-dev-2.6.6.zip || exit 1

cd cygwin-deps && unzip glib-2.6.6.zip && unzip glib-dev-2.6.6.zip
