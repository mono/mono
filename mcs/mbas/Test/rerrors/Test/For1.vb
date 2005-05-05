'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class For1
        <Test, ExpectedException (GetType (ArrayTypeMismatchException))> _
        Public Sub TestMismatchException ()
			dim j as string = "hello"
			dim count as integer
			for i as Integer  = j to j step j
				count = count+1 
			next i
  	  End sub
End Class
