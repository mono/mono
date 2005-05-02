'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30438
REM ErrorMessage: Constants must have a value.

Module M
	Sub Main()
		Const a AS Integer= 10, b as Short 
	End sub
End Module
