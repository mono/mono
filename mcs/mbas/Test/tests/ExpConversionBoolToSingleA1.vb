Module ExpConversionofBoolToSingle
        Sub Main()
                Dim a as Boolean = False
                Dim b as Single = CSng(a)
                if b <> 0 then
                        Throw New System.Exception("Explicit Conversion of Bool(False) to Single has Failed. Expected 0, but got " & b)
                End if
        End Sub
End Module
