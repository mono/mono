'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 17
REM ExpectedError: BC30268
REM ErrorMessage: 'Public Overrides Sub fun()' cannot override 'Public Shared Sub fun()' because it is declared 'Shared'.

Class C1
        Public Shared Sub fun()
	  End Sub
End Class

Class C2
        Inherits C1
        Public Overrides Sub fun()
        End Sub
End Class

Module InheritanceM
        Sub Main()            
        End Sub
End Module
