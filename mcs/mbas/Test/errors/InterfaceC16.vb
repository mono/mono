'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 15
REM ExpectedError: BC30258
REM ErrorMessage: : Classes can inherit only from other classes.

Module InterfaceC
	Interface A
	End Interface	

	Class B
		Inherits A
	End Class

	Sub Main()
	End Sub
End Module
 
