CSC=csc.exe
CSCFLAGS=/nologo /debug+ /debug:full 
CORCOMPARE_SRC=CorCompare.cs \
	ToDoProperty.cs \
	ToDoNameSpace.cs \
	MissingProperty.cs \
	ToDoMethod.cs \
	MissingMethod.cs \
	ToDoType.cs \
	MissingType.cs \
	ToDoAssembly.cs

all: CorCompare.exe

CorCompare.exe: $(CORCOMPARE_SRC)
	$(CSC) $(CSCFLAGS) $(CORCOMPARE_SRC) ..\\XMLUtil.cs
	./CorCompare.exe -x cormissing.xml ../../class/lib/corlib_cmp.dll
clean:
	rm -f *.exe *.pdb *.dll
	rm -f cormissing.xml
