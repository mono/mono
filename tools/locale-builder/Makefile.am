
MCS = mcs
RUNTIME = mono
MCSFLAGS = -g
# To build a reduced mono runtime with support only for some locales, # run:
# 	make minimal
# To build with a single locale (en_US), run:
# 	make minimal MINIMAL_LOCALES=en_US
# MINIMAL_LOCALES is a regular expression over the filenames in locales.
# make minimal saves about 60 KB of the mono binary size.
# To create the tables fro all the supported locales, use:
# 	make culture-table
# After make minimal or make culture-table, you need to run:
# 	make install-culture-table
# to copy the needed files where the mono build will pick them up.

MINIMAL_LOCALES=en

CLEANFILES = locale-builder.exe culture-info-tables.h

locale_builder_sources = Driver.cs		\
			 CultureInfoEntry.cs	\
			 DateTimeFormatEntry.cs	\
			 NumberFormatEntry.cs	\
			 TextInfoEntry.cs	\
			 Entry.cs

supp_data_files = supp/ar_AE.xml supp/ar_EG.xml supp/ar_KW.xml supp/ar_MA.xml	\
		 supp/ar_YE.xml supp/en_PH.xml supp/en_ZA.xml supp/th.xml    	\
		 supp/ar_IQ.xml supp/ar_LB.xml supp/ar_OM.xml supp/ar_TN.xml 	\
		 supp/en_US.xml supp/en_ZW.xml supp/ar_DZ.xml supp/ar_JO.xml 	\
		 supp/ar_QA.xml supp/ar.xml    supp/en_IE.xml supp/en.xml		\
		 supp/ar_SY.xml supp/ar_BH.xml supp/en_CA.xml supp/ar_LY.xml	\
		 supp/root.xml

EXTRA_DIST = $(locale_builder_sources) $(supp_data_files) lcids.xml supplementalData.xml textinfo.xml

locale-builder.exe: $(locale_builder_sources)
	$(MCS) $(MCSFLAGS) /out:$@ $^

culture-table: locale-builder.exe lang-data locale-data
	$(RUNTIME) locale-builder.exe

minimal: locale-builder.exe lang-data locale-data
	$(RUNTIME) locale-builder.exe --locales '$(MINIMAL_LOCALES)'

lang-data:
	if ! test -f langs/en.xml ; then \
		wget http://primates.ximian.com/~jackson/icu_langs.tar.gz ; \
		tar xzvf icu_langs.tar.gz ; \
	fi

locale-data:
	if ! test -f locales/en_US.xml ; then \
		wget http://primates.ximian.com/~jackson/icu_locales.tar.gz ; \
		tar xzvf icu_locales.tar.gz ; \
	fi

install-culture-table: culture-info-tables.h
	cp -f culture-info-tables.h ../../mono/metadata/.

