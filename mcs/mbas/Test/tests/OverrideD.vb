'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Class A
	Public Overridable Sub fun(i As Integer)
	End Sub	
End Class

Class B
	Inherits A
	Public NotOverridable Overrides Sub fun(i As Integer)
	End Sub
End Class

Module Default1	
	Sub Main()
	End Sub
End Module
