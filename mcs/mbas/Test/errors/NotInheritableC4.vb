'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 10
REM ExpectedError: BC31089
REM ErrorMessage: 'NotInheritable' is not valid on an Interface declaration.

NotInheritable Interface A
        Sub G()        
End Interface

Module InheritanceN
        Sub Main()
        End Sub
End Module
