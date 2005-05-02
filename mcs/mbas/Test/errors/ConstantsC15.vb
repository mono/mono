'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 15
REM ExpectedError: BC30233
REM ErrorMessage: 'Shared' is not valid on a constant declaration

REM LineNo: 15
REM ExpectedError: BC30593
REM ErrorMessage:  Variables in Modules cannot be declared 'Shared'

Module M
	Shared Const b as Short = 1
	Sub Main()
	End Sub
End Module
