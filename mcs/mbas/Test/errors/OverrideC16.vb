'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 17
REM ExpectedWarning: BC40003
REM WarningMessage: sub 'fun' shadows an overloadable member declared in the base class 'C1'.  If you want to overload the base method, this method must be declared 'Overloads'.

Class C1
	Public Overridable Sub fun()
	End Sub
End Class

Class C2
	Inherits C1
	Public Overridable Sub fun(i as Integer)
	End Sub
End Class

Module Default1	
	Sub Main()
	End Sub
End Module
