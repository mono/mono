' Authors:
'   Alexandre Rocha Lima e Marcondes (alexandre@psl-pr.softwarelivre.org)
'   Maverson Eduardo Schulze Rosa (maverson@gmail.com)
'
' GrupoTIC - UFPR - Federal University of Paraná

Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class errorstmt

		<Test, ExpectedException (GetType (System.FormatException))> _
        Public Sub TestErrorString()
        	error "aaa"
        End Sub
        <Test, ExpectedException (GetType (System.InvalidCastException))> _
        Public Sub TestErrorObject()
            error new Object()
        End Sub
End Class

