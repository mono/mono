monodir = $(sysconfdir)/mono

EXTRA_DIST =  config.in \
	     machine.config \
	     DefaultWsdlHelpGenerator.aspx \
	     browscap.ini.gz

mono_DATA =  config \
	     machine.config \
	     DefaultWsdlHelpGenerator.aspx \
	     browscap.ini

CLEANFILES = browscap.ini

browscap.ini: browscap.ini.gz
	gunzip -c $^ > $@

