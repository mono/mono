#!/bin/sh

alias gmcs='mono ../gmcs/gmcs.exe'

for gen in gen-1.cs gen-2.cs gen-3.cs gen-4.cs gen-5.cs gen-6.cs gen-7.cs gen-8.cs gen-9.cs gen-10.cs gen-11.cs gen-12.cs gen-14.cs ; do
	echo "Compiling $gen ..."
	gmcs /out:gen-test.exe $gen
	echo "Running monodis"
	monodis gen-test.exe > /dev/null
	echo "Running mono"
	mono gen-test.exe > /dev/null
done

echo "Compiling gen-13-dll.cs"
gmcs /out:gen-test.dll /target:library gen-13-dll.cs
echo "Running monodis"
monodis gen-test.dll > /dev/null
echo "Compiling gen-13-exe.cs"
gmcs /out:gen-test.exe /r:gen-test.dll gen-13-exe.cs
