' Authors:
'   Alexandre Rocha Lima e Marcondes (alexandre@psl-pr.softwarelivre.org)
'   Maverson Eduardo Schulze Rosa (maverson@gmail.com)
'
' GrupoTIC - UFPR - Federal University of Paraná


Module errorstmt

        Public Dim error_number as integer = 11

        Sub Main()
        	Try
                error error_number
                
                Catch  ex As System.Exception
                	If not (ex.GetType() = GetType(System.DivideByZeroException)) Then
                		Throw new System.Exception("#A1 Error not working")
                	End If
                End Try	
        End Sub
End Module

