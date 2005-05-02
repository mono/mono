'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30235
REM ErrorMessage: 'Static' is not valid on a member variable declaration.

Module M
	Structure fun
	 	 Static Dim y as Integer 
	end Structure

        Sub Main()		
        End Sub
End Module
