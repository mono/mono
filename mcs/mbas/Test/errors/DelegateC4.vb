'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30219
REM ErrorMessage: Event or delegate declaration cannot have Optional or ParamArray parameters.

Imports System

Module M
        Delegate Function SD(Optional i as Integer=20)as Integer
        Function f(i as integer)as Integer
		return 10
        End Function 
        Function f(Byval i as Single)as Integer
		return 12
        End Function

        Sub Main()
                dim d1 as SD
                d1= new SD(AddressOf f)
                d1.Invoke(10)
        End Sub
End Module
