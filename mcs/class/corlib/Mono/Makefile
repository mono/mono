#
# This is just a Makefile to test DataConverter
#
DataConverter.exe: DataConverter.cs a.cs
	mcs -g a.cs -define:MONO_DATACONVERTER_PUBLIC -define:MONO_DATACONVERTER_STATIC_METHODS -unsafe DataConverter.cs -nowarn:3021
