'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

MustInherit Class A
	MustOverride Function fun(ByVal a As Integer)
End Class

MustInherit Class AB
	Inherits A
	MustOverride Function fun1(ByVal a As String)
End Class

Class ABC
	Inherits AB
	Overrides Function fun(ByVal a As Integer)
	End Function
	Overrides Function fun1(ByVal a As String)
	End Function
End Class

Module MustInheritF
	Sub Main()
	End Sub
End Module

