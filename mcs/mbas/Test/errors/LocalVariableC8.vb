'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError:  BC32000
REM ErrorMessage: Local variable 'i' cannot be referred to before it is declared.

Module M
        Private i as Integer
        sub main()
                i = 0
                Dim i as Integer
                i = 9
        End sub
End Module
