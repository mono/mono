'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 10
REM ExpectedError: BC30397
REM ErrorMessage: 'MustInherit' is not valid on an Interface declaration.

MustInherit Interface A
	MustOverride Sub fun1()
End Interface

Interface AB
	Inherits A
	Overrides Sub fun1()
End Interface

Module ShadowE
	Sub Main()
	End Sub
End Module
