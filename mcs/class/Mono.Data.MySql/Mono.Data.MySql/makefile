
SRCLIST = MySql.cs Test.cs

.SUFFIXES: .exe .dll .cs .lex .txt

#DEBUG = /debug

.cs.dll:
	csc $(DEBUG) /t:dll $*.cs

.cs.exe:
	csc $(DEBUG) /t:exe $*.cs

all : Test.exe MySql.dll

Test.exe : Test.cs MySql.dll
	csc $(DEBUG) /t:exe /r:MySql.dll Test.cs

MySql.dll : MySql.cs Field.cs
	csc $(DEBUG) /t:library /doc:MySql.xml MySql.cs Field.cs
