'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30500
REM ErrorMessage: Constant 'b' cannot depend on its own value.

Module M
	Sub Main()
		Const b as Short = b + 1
	End sub
End Module
