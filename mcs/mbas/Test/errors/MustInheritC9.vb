'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 16 
REM ExpectedError: BC30284
REM ErrorMessage: function 'fun' cannot be declared 'Overrides' because it does not override a function in a base class.

MustInherit Class A
	MustOverride Function fun(ByVal a As Integer)
End Class

Class AB
	Inherits A	
	Overrides Function fun(ByVal a As String)
	End Function
End Class

Module MustInheritE 
	Sub Main()
	End Sub
End Module

