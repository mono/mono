'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Option Compare text
Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class LikeOperator2
		  <Test, ExpectedException (GetType (System.Exception))> _
	Public Sub TestForException ()
		dim a as boolean
		a = "f" Like "[Z-A]"
		If a <> True Then
		    Throw new System.Exception("#A1-LikeOperator:Failed")
		End If
	end sub
End class
