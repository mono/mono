'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 17
REM ExpectedWarning: BC40005
REM WarningMessage: sub 'fun' shadows an overridable method in a base class. To override the base method, this method must be declared 'Overrides'.

Class C1
	Public Overridable Sub fun()
	End Sub
End Class

Class C2
	Inherits C1
	Public Overridable Sub fun()
	End Sub
End Class

Module Default1	
	Sub Main()
	End Sub
End Module
