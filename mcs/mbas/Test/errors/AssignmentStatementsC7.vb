'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 18
REM ExpectedError:  BC30524
REM ErrorMessage: Property 'Pro' is 'WriteOnly'

Module Test
    Private PValue As Integer
    Public WriteOnly Property Pro As Integer      
		Set (ByVal a as Integer )
		End Set
    End Property

    Sub Main()
	  Dim a as Integer = Pro
    End Sub
End Module
