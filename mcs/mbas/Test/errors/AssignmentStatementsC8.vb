'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 18
REM ExpectedError:  BC30533
REM ErrorMessage: Conversion from 'Double' to 'Date' requires calling the 'Date.FromOADate' method.

Module Test
    Private PValue As Integer
    Public WriteOnly Property Pro As Date
		Set (ByVal a as Date)
		End Set
    End Property

    Sub Main()
	  Pro = 10.5
    End Sub
End Module
