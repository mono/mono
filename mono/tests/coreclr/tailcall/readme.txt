Goal is maybe to find tailcall tests, flatten the structure but notice duplicates.

cd /dev2
git clone https://github.com/dotnet/coreclr
cd /dev2/mono/mono/tests
mkdir corclr
cd coreclr
cp -prv /dev2/coreclr/tests/src/* .
find . | grep proj$ | xargs rm
find . | grep -vi tail | xargs rm
a few times:
find . | xargs rmdir

and that this point optional:
	for a in `find . -type f`; do if  [ ! -e $(basename $a) ] ; then mv $a $(basename $a)  ; fi done
	a few:
	find . | grep -vi tail | xargs rmdir


	and then optionally pick up stragglers.
