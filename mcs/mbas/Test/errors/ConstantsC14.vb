'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30500
REM ErrorMessage: Constant 'a' cannot depend on its own value.

Module M
	Const a as Short = b + 1
	Const b as Short = a + 1
	Sub Main()
	End Sub
End Module
