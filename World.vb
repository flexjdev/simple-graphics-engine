Public Class World
    Public Camera As Game_Camera
    Public Objects As List(Of Game_Object)
    Public Ambient_Light_Level As Double

    Public Sub New(ByVal _cam As Game_Camera,
                   ByVal _objects As List(Of Game_Object),
                   ByVal _ambient As Double)
        Ambient_Light_Level = _ambient
        Objects = _objects
        Camera = _cam
    End Sub

    Public Sub Set_Ambient(ByVal ambient As Double)
        Ambient_Light_Level = Math.Max(0, (Math.Min(1, ambient)))
    End Sub

    Shared Function Search_Models(ByVal name As String, ByRef List_Model As List(Of Model)) As Integer
        For i As Integer = 0 To List_Model.Count - 1
            If name = List_Model(i).Name Then
                Return i
            End If
        Next
        Return -1
    End Function

    Shared Function FromFile(ByVal PathName As String,
                             ByRef List_Models As List(Of Model)) As World
        Dim SR As New IO.StreamReader(PathName)

        Dim Name As String = SR.ReadLine
        Dim list_objects As New List(Of Game_Object)
        Dim cam As Game_Camera

        Dim model_name As String
        Dim model_index As Integer
        Dim model As Model

        Dim position As VEC3
        Dim rotation As VEC3
        Dim scale As VEC3

        For i As Integer = 0 To SR.ReadLine - 1 'Add objects
            model_name = SR.ReadLine
            model_index = Search_Models(model_name, List_Models)

            If model_index >= 0 Then
                model = List_Models(model_index)
            Else
                MsgBox("Could not find model file")
            End If

            position = VEC3.Parse(SR.ReadLine)
            rotation = VEC3.Parse(SR.ReadLine)
            scale = VEC3.Parse(SR.ReadLine)

            list_objects.Add(New Game_Object(model,
                                             position,
                                             rotation,
                                             scale))
            If i = 0 Then
                cam = New Game_Camera(list_objects(i), 0)
            End If
        Next

        SR.Close()
        Return New World(cam,
                         list_objects,
                         0.8)
    End Function

End Class