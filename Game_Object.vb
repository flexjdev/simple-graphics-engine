Public Class Game_Object
    Dim Object_Velocity As VEC3
    Dim Object_Position As VEC3
    Dim Object_Rotation As VEC3
    Dim Object_Scale As New VEC3
    Public Object_Model As Model
    Public matrix As MAT44

    Public Property Velocity() As VEC3
        Get
            Return Object_Velocity
        End Get
        Set(ByVal newVelocity As VEC3)
            Object_Velocity = newVelocity
        End Set
    End Property

    Public Property Rotation() As VEC3
        Get
            Return Object_Rotation
        End Get
        Set(ByVal newRotation As VEC3)
            Object_Rotation = newRotation
        End Set
    End Property

    Public Property Position() As VEC3
        Get
            Return Object_Position
        End Get
        Set(ByVal new_position As VEC3)
            Object_Position = new_position
        End Set
    End Property

    Public Sub Move(ByVal Displacement As VEC3)
        Object_Position += Displacement
    End Sub

    Public Sub RotateX(ByVal angleX As Double)
        Object_Rotation.X += angleX
    End Sub

    Public Sub RotateY(ByVal angleY As Double)
        Object_Rotation.Y += angleY
    End Sub

    Public Sub RotateZ(ByVal angleZ As Double)
        Object_Rotation.Z += angleZ
    End Sub

    Public Sub New(ByVal New_Model As Model,
                   ByVal Velocity As VEC3,
                   ByVal Position As VEC3,
                   ByVal Rotation As VEC3,
                   ByVal Scale As VEC3)
        'Object_Model = New_Model
        'Object_Model_Textured = New_Model_Textured
        Object_Model = New Model(New_Model.Name,
                                 New_Model.Type,
                                 New_Model.Vertices_Local,
                                 New_Model.Triangles,
                                 New_Model.Colours,
                                 New_Model.texture,
                                 New_Model.Texture_Coordinates)
        Object_Velocity = Velocity
        Object_Position = Position
        Object_Rotation = Rotation
        Object_Scale = Scale
    End Sub

    Public Sub New(ByVal New_Model As Model,
                   ByVal Position As VEC3,
                   ByVal Rotation As VEC3,
                   ByVal Scale As VEC3)
        Object_Model = New Model(New_Model.Name,
                                 New_Model.Type,
                                 New_Model.Vertices_Local,
                                 New_Model.Model_Triangles,
                                 New_Model.Model_Colours,
                                 New_Model.texture,
                                 New_Model.Model_Texture_Coordinates)
        Object_Position = Position
        Object_Rotation = Rotation
        Object_Scale = Scale
    End Sub

End Class