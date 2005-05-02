'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30288
REM ErrorMessage: Local variable 'y' is already declared in the current block..

Module M
	Sub fun()
	 	 Static Dim y as Integer = 10
	       Dim y as Char
	end Sub
      Sub Main()		
      End Sub
End Module
