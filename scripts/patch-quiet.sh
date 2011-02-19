#!/bin/sh

#
# patch-quiet.sh: Shell script to rewrite Makefiles using libtool to be less verbose
#

if [ "$1" = "" ]; then
	echo "Usage: patch-quiet.sh <path to Makefile>"
	exit 1
fi

src=$1

if head -n1 $src | grep -q '# Postprocessed with patch-quiet\.sh'; then
    # already handled
    exit 0
fi

echo "# Postprocessed with patch-quiet.sh" > $src.tmp && cat $src >> $src.tmp && cp $src.tmp $src && rm -f $src.tmp
# Try to find GNU sed
SED="sed"
sed --version >/dev/null 2>&1
if [ $? -ne 0 ] ; then
	gsed --version > /dev/null 2>&1 && SED="gsed"
fi

# compile
${SED} -e 's/^\t\(if \)\?$(COMPILE)/\t$(if $(V),,@echo -e "CC\\t$@";) \1$(COMPILE)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
${SED} -e 's/^\t\(if \)\?$(LTCOMPILE)/\t$(if $(V),,@echo -e "CC\\t$@";) \1$(LTCOMPILE)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
${SED} -e 's/^\t\(if \)\?$(LTCXXCOMPILE)/\t$(if $(V),,@echo -e "CC\\t$@";) \1$(LTCXXCOMPILE)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
${SED} -e 's/^\t\(if \)\?$(LIBTOOL)/\t$(if $(V),,@echo -e "CC\\t$@";) \1$(LIBTOOL)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
# link
# automake defines multiple symbols ending with LINK
${SED} -e 's/^\t$(\(.*LINK\))/\t$(if $(V),,@echo -e "LD\\t$@";) $(\1)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
#sed -e 's/LINK = $(LIBTOOL)/LINK = $(if $(V),,@echo -e "LD\\t$@";) $(LIBTOOL)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
# CC
${SED} -e 's/^\t\(if \)\?$(CC)/\t$(if $(V),,@echo -e "CC\\t$@";) \1$(CC)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
# mv
${SED} -e 's/^\tmv -f/\t$(if $(V),,@)mv -f/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
${SED} -e 's/^am__mv = /&$(if $(V),,@)/' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
# libtool messages
${SED} -e 's/\$(LIBTOOL)/$(LIBTOOL) $(if $(V),,--quiet)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp

# This causes this script to be rerun if Makefile.am changes
${SED} -e 's/am__depfiles_maybe = depfiles/& quiet/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp

