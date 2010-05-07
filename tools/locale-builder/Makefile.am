
MCS = mcs
RUNTIME = mono
MCSFLAGS = -debug+
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
			 RegionInfoEntry.cs     \
			 TextInfoEntry.cs	\
			 Entry.cs

supp_data_files = 	\
	supp/af_ZA.xml	\
	supp/ar_AE.xml	\
	supp/ar_BH.xml	\
	supp/ar_DZ.xml	\
	supp/ar_EG.xml	\
	supp/ar_IQ.xml	\
	supp/ar_JO.xml	\
	supp/ar_KW.xml	\
	supp/ar_LB.xml	\
	supp/ar_LY.xml	\
	supp/ar_MA.xml	\
	supp/ar_OM.xml	\
	supp/ar_QA.xml	\
	supp/ar_SA.xml	\
	supp/ar_SY.xml	\
	supp/ar_TN.xml	\
	supp/ar.xml	\
	supp/ar_YE.xml	\
	supp/be_BY.xml	\
	supp/bg.xml	\
	supp/ca_ES.xml	\
	supp/ChangeLog	\
	supp/cs_CZ.xml	\
	supp/da.xml	\
	supp/de_AT.xml	\
	supp/de.xml	\
	supp/el.xml	\
	supp/en_AU.xml	\
	supp/en_CA.xml	\
	supp/en_GB.xml	\
	supp/en_IE.xml	\
	supp/en_NZ.xml	\
	supp/en_PH.xml	\
	supp/en_TT.xml	\
	supp/en_US.xml	\
	supp/en.xml	\
	supp/en_ZA.xml	\
	supp/en_ZW.xml	\
	supp/es_AR.xml	\
	supp/es_BO.xml	\
	supp/es_CL.xml	\
	supp/es_CO.xml	\
	supp/es_CR.xml	\
	supp/es_DO.xml	\
	supp/es_EC.xml	\
	supp/es_ES.xml	\
	supp/es_GT.xml	\
	supp/es_HN.xml	\
	supp/es_MX.xml	\
	supp/es_NI.xml	\
	supp/es_PA.xml	\
	supp/es_PE.xml	\
	supp/es_PR.xml	\
	supp/es_PY.xml	\
	supp/es_SV.xml	\
	supp/es_UY.xml	\
	supp/es_VE.xml	\
	supp/es.xml	\
	supp/et.xml	\
	supp/eu_ES.xml	\
	supp/fa.xml	\
	supp/fi.xml	\
	supp/fo_FO.xml	\
	supp/fr_BE.xml	\
	supp/fr_CA.xml	\
	supp/fr_CH.xml	\
	supp/fr_FR.xml	\
	supp/fr_LU.xml	\
	supp/fr.xml	\
	supp/he.xml	\
	supp/hi_IN.xml	\
	supp/hr.xml	\
	supp/hu.xml	\
	supp/hy.xml	\
	supp/id_ID.xml	\
	supp/is.xml	\
	supp/it_CH.xml	\
	supp/it_IT.xml	\
	supp/ja.xml	\
	supp/ka_GE.xml	\
	supp/ko_KR.xml	\
	supp/lt_LT.xml	\
	supp/lv.xml	\
	supp/mk.xml	\
	supp/nl_BE.xml	\
	supp/nl_NL.xml	\
	supp/pl.xml	\
	supp/pt_BR.xml	\
	supp/pt_PT.xml	\
	supp/pt.xml	\
	supp/root.xml	\
	supp/ro.xml	\
	supp/ru.xml	\
	supp/sk.xml	\
	supp/sl.xml	\
	supp/sq.xml	\
	supp/sv_FI.xml	\
	supp/sv_SE.xml	\
	supp/sw_KE.xml	\
	supp/th.xml	\
	supp/tr.xml	\
	supp/uk.xml	\
	supp/vi.xml

extra_langs = \
	langs/ka.xml	

extra_locales = \
	locales/ka_GE.xml

EXTRA_DIST = $(locale_builder_sources) $(supp_data_files) lcids.xml supplementalData.xml textinfo.xml \
	$(extra_langs) $(extra_locales)


locale-builder.exe: $(locale_builder_sources)
	$(MCS) $(MCSFLAGS) /out:$@ $^

culture-table: locale-builder.exe lang-data locale-data
	$(RUNTIME) locale-builder.exe

minimal: locale-builder.exe lang-data locale-data
	$(RUNTIME) locale-builder.exe --locales '$(MINIMAL_LOCALES)'

lang-data:
	if ! test -f langs/en.xml ; then \
		wget http://go-mono.com/icu/icu_langs.tar.gz ; \
		tar xzvf icu_langs.tar.gz ; \
	fi

locale-data:
	if ! test -f locales/en_US.xml ; then \
		wget http://go-mono.com/icu/icu_locales.tar.gz ; \
		tar xzvf icu_locales.tar.gz ; \
	fi

install-culture-table: culture-info-tables.h
	cp -f culture-info-tables.h ../../mono/metadata/.

