'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 11
REM ExpectedError: BC30270
REM ErrorMessage: 'Overridable' is not valid on an interface method declaration.

Interface C1
	  Overridable Function fun()	  
End Interface

Interface C2
        Inherits C1
        Overrides Function fun()
End Interface

Module InheritanceM
        Sub Main()            
        End Sub
End Module
