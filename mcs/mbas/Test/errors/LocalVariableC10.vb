'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError:  BC30616
REM ErrorMessage: Variable 'i' hides a variable in an enclosing block.

Module M
        sub main()
                Dim i as Integer
                If true then
	                Dim i as Integer
		    End if
        End sub
End Module
