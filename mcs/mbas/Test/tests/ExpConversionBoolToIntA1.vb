Module ExpConversionofBoolToInt
        Sub Main()
                Dim a as Boolean = False
                Dim b as Int = CInt(a)
                if b <> 0 then
                        Throw New System.Exception("Explicit Conversion of Bool(False) to Int has Failed. Expected 0, but got " & b)
                End if
        End Sub
End Module
