'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

'Note That fun need not be present in base class to get shadowed
Class A
	Sub fun1()
	End Sub
End Class

Class AB
	Inherits A
	Shadows Sub fun()
	End Sub
End Class

Module ShadowE
	Sub Main()
	End Sub
End Module
