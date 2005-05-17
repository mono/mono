Imports System
Module OP1_0_0
        Function F(ByVal telephoneNo as Long, Optional ByVal code as Integer = 080,Optional ByVal code1  As Integer = 091, Optional ByRef name As String="Sinha") As Boolean
                if (code <> 080 and code1 <> 091 and name<>"Manish")
                        return false
                else
			name = "Sinha"
                        return true
                end if
        End Function

   function foo () as integer
        return 0
   end function

   Sub Main()
      Dim telephoneNo As Long = 9886066432
        Dim name As String ="Manish"
        Dim status as Boolean

      status = F(telephoneNo,,,name)
        if(status = false or name <> "Sinha")
               Throw New System.Exception("#A1, Unexcepted behaviour in string of OP1_0_1")
      end if
   End Sub

End Module
