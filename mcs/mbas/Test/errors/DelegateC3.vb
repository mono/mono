'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 23
REM ExpectedError: BC30408
REM ErrorMessage: Method 'Public Function f(ByRef i As Integer) As Integer' does not have the same signature as delegate 'Delegate Function SD(i As Integer) As Integer'.

Imports System

Module M
        Delegate Function SD(Byval i as Integer)as Integer
        Function f(ByRef i as integer)as Integer
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
