'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError:  BC30288
REM ErrorMessage: Local variable 'i' is already declared in the current block.

Module M
        sub main()
	        Dim i as Integer
              i = 0 
              Dim i as Char
		  i = 1
	   End sub
End Module
