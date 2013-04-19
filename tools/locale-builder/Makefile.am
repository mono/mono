
MCS = mcs
RUNTIME = mono
MCSFLAGS = -debug+
# To build a reduced mono runtime with support only for some locales, # run:
# 	make minimal
# To build with a single locale (en_US), run:
# 	make minimal MINIMAL_LOCALES=en_US
# MINIMAL_LOCALES is a regular expression over the filenames in locales.
# make minimal saves about 60 KB of the mono binary size.
# To create the tables for all the supported locales, use:
# 	make culture-table
# After make minimal or make culture-table, you need to run:
# 	make install-culture-table
# to copy the needed files where the mono build will pick them up.

MINIMAL_LOCALES=en

CLEANFILES = locale-builder.exe culture-info-tables.h

locale_builder_sources = Driver.cs		\
			CalendarType.cs \
			Constants.cs \
			Patterns.cs \
			 CultureInfoEntry.cs	\
			 DateTimeFormatEntry.cs	\
			 NumberFormatEntry.cs	\
			 RegionInfoEntry.cs     \
			 TextInfoEntry.cs	\
			 Entry.cs

EXTRA_DIST = $(locale_builder_sources) lcids.xml

locale-builder.exe: $(locale_builder_sources)
	$(MCS) $(MCSFLAGS) /out:$@ $^

culture-table: locale-builder.exe locale-data
	$(RUNTIME) locale-builder.exe

minimal: locale-builder.exe locale-data
	$(RUNTIME) locale-builder.exe --locales '$(MINIMAL_LOCALES)'

locale-data:
	if ! test -d CLDR/common ; then \
		curl http://www.unicode.org/Public/cldr/21/core.zip -o core.zip ; \
		unzip core.zip -d CLDR ; \
	fi

install-culture-table: culture-info-tables.h
	cp -f culture-info-tables.h ../../mono/metadata/.

