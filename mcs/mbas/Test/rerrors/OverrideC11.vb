'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 17
REM ExpectedError: BC30266
REM ErrorMessage: 'Protected Overrides Sub fun()' cannot override 'Protected Friend Overridable Sub fun()' because they have different access levels.

Class C1
	  Protected Friend Overridable Sub fun()
	  End Sub
End Class

Class C2
        Inherits C1
        Protected Overrides Sub fun()
        End Sub
End Class

Module InheritanceM
        Sub Main()            
        End Sub
End Module
