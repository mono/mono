'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 28
REM ExpectedError: BC30516
REM ErrorMessage: Overload resolution failed because no accessible 'fun' accepts this number of arguments.

'This program is used to check the functioning of Shadows

Class A
        Public Sub fun()
	  End Sub	  
End Class

Class AB
        Inherits A
        Public Shadows Sub fun(ByVal i As Integer)
	  End Sub
	  Private Shadows Sub fun()	
	  End Sub
End Class

Class AC
        Inherits AB
        Sub fun1()
		fun()
	  End Sub	
End Class

Module ShadowG
        Sub Main()
        End Sub
End Module
