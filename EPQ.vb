Option Strict On

Public Class EPQ

    Dim WithEvents Paint_Clock As New Timer
    Dim List_Models As New List(Of Model)
    Dim List_Worlds As New List(Of World)
    Public List_Textures As New List(Of Texture)
    Dim Engine As Engine3D
    Dim console_font As New Font(FontFamily.GenericMonospace, 12, FontStyle.Regular)

    Dim Current_World As Integer
    Dim KI As New KeyBoardInfo
    Dim MI As New MouseInfo

    Dim Last_Update As DateTime
    Dim Player_Velocity As Double = 5
    Dim Player_Rotation_Speed As Double = Math.PI
    Dim Camera_Rotation_Velocity As VEC3
    Dim Camera_Rotation_Bleed As Double = 0.95

#Region "File Loading"

    Private Function Trim_Name(ByVal Pathname As String) As String
        Pathname = Pathname.Substring(Pathname.LastIndexOf("/") + 1)
        Pathname = Pathname.Substring(0, Pathname.LastIndexOf("."))
        Return Pathname

    End Function

    ''' <summary>
    ''' Loads all textures in the specified path into memory,
    ''' plus one placeholder texture (1x1 magenta texture).
    ''' </summary>
    '''
    Private Sub Load_Textures(ByVal path As String)
        'Create placeholder texture
        Dim placeholder_bitmap As New Memory_Bitmap(1, 1)
        placeholder_bitmap.SetPixel(0, 0, Color.FromArgb(255, 0, 255))
        Dim placeholder As New Texture("placeholder", placeholder_bitmap)
        'Add the placeholder texture into the list of textures.
        List_Textures.Add(placeholder)

        Dim Texture_Location() As String = System.IO.Directory.GetFiles(path)

        'Load textures from the directory.
        For Each PathName As String In Texture_Location
            Dim File_Extension As String = PathName.Substring(PathName.LastIndexOf(".") + 1).ToLower
            If File_Extension = "bmp" Then

                List_Textures.Add(New Texture(Trim_Name(PathName), Memory_Bitmap.FromFile(PathName)))

            End If
        Next
    End Sub

    Private Sub Load_Models(ByVal Path As String)
        Dim Model_Location() As String = System.IO.Directory.GetFiles(Path)
        'Get a list of all files contained in the directory

        For Each PathName As String In Model_Location
            Dim File_Extension As String = PathName.Substring(PathName.LastIndexOf(".") + 1).ToLower
            'Get the file extension of the file
            If File_Extension = "model" Then

                List_Models.Add(Model.FromFile(PathName, List_Textures))

            End If
        Next
    End Sub

    Private Sub Load_Worlds(ByVal Path As String)
        Dim World_Location() As String = System.IO.Directory.GetFiles(Path)
        'Get a list of all files contained in the directory

        For Each PathName As String In World_Location
            Dim File_Extension As String = PathName.Substring(PathName.LastIndexOf(".") + 1).ToLower
            'Get the file extension of the file

            If File_Extension = "world" Then

                List_Worlds.Add(World.FromFile(PathName, List_Models))

            End If
        Next
    End Sub

#End Region

#Region "Keyboard"

    Class KeyBoardInfo
        Public Ak As Boolean = False
        Public Sk As Boolean = False
        Public Dk As Boolean = False
        Public Wk As Boolean = False
    End Class

    Private Sub Key_Down(ByVal sender As System.Object, ByVal e As KeyEventArgs) Handles MyBase.KeyDown
        Select Case e.KeyCode
            Case Keys.A : KI.Ak = True
            Case Keys.S : KI.Sk = True
            Case Keys.D : KI.Dk = True
            Case Keys.W : KI.Wk = True
        End Select
    End Sub

    Private Sub Key_Up(ByVal sender As System.Object, ByVal e As KeyEventArgs) Handles MyBase.KeyUp
        Select Case e.KeyCode
            Case Keys.A : KI.Ak = False
            Case Keys.S : KI.Sk = False
            Case Keys.D : KI.Dk = False
            Case Keys.W : KI.Wk = False
        End Select
    End Sub

#End Region

#Region "Mouse"

    Class MouseInfo
        Public Rightk As Boolean = False
        Public Leftk As Boolean = False
        Public Middlek As Boolean = False
        Public lastpos As New Point
        Public currentpos As New Point
    End Class

    Private Sub PB_mousescroll(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles Me.MouseWheel
        List_Worlds(Current_World).Camera.ChangeDistance(-e.Delta() * 0.003)
    End Sub

    Private Sub PB_mousemove(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles PB.MouseMove
        MI.currentpos = New Point(e.X, e.Y)
    End Sub

    Private Sub PB_mousedown(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles PB.MouseDown
        Select Case e.Button
            Case Windows.Forms.MouseButtons.Left
                MI.Leftk = True
            Case Windows.Forms.MouseButtons.Right
                MI.Rightk = True
            Case Windows.Forms.MouseButtons.Middle
                MI.Middlek = True
        End Select
        MI.currentpos = New Point(e.X, e.Y)
    End Sub

    Private Sub PB_mouseup(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles PB.MouseUp
        Select Case e.Button
            Case Windows.Forms.MouseButtons.Left : MI.Leftk = False
            Case Windows.Forms.MouseButtons.Right : MI.Rightk = False
            Case Windows.Forms.MouseButtons.Middle : MI.Middlek = False
        End Select
    End Sub

#End Region

    Sub Handle_Mouse()
        Dim dx As Double
        Dim dy As Double
        If MI.Rightk Then
            dx = MI.currentpos.X - MI.lastpos.X
            dy = MI.currentpos.Y - MI.lastpos.Y
            Dim Add_Velocity As New VEC3(dx, dy, 0)
            Camera_Rotation_Velocity += Add_Velocity
        ElseIf MI.Leftk Then
            Camera_Rotation_Velocity = New VEC3
            dx = MI.currentpos.X - MI.lastpos.X
            dy = MI.currentpos.Y - MI.lastpos.Y
            List_Worlds(Current_World).Camera.RotateCameraX(dy * Player_Rotation_Speed * 0.001)
            List_Worlds(Current_World).Camera.RotateCameraY(dx * Player_Rotation_Speed * 0.001)
        End If

        If Not (Camera_Rotation_Velocity = New VEC3(0, 0, 0)) Then
            dx = Camera_Rotation_Velocity.X
            dy = Camera_Rotation_Velocity.Y
            If Math.Abs(Camera_Rotation_Velocity.X) < 0.001F Then Camera_Rotation_Velocity.X = 0
            If Math.Abs(Camera_Rotation_Velocity.Y) < 0.001F Then Camera_Rotation_Velocity.Y = 0
            List_Worlds(Current_World).Camera.RotateCameraX(dy * Player_Rotation_Speed * 0.0001)
            List_Worlds(Current_World).Camera.RotateCameraY(dx * Player_Rotation_Speed * 0.0001)
            List_Worlds(Current_World).Camera.Camera_Object.Rotation = List_Worlds(Current_World).Camera.Rotation
        End If

        Camera_Rotation_Velocity *= Camera_Rotation_Bleed

        MI.lastpos = MI.currentpos
    End Sub

    Sub Handle_Camera(ByVal TimePassed As Double)
        With List_Worlds(Current_World).Camera
            Dim Stagger As Double = 1
            Dim Velocity As VEC3
            If KI.Wk = True Then Velocity.Z += 1
            'If KI.Sp = True Then Velocity.Y += 1 'IF FLYING ENABLED
            'If KI.Sh = True Then Velocity.Y -= 1 'IF FLYING ENABLED
            If KI.Ak = True Then Velocity.X -= 0.75 ': Stagger = 0.75
            If KI.Dk = True Then Velocity.X += 0.75 ': Stagger = 0.75
            If KI.Sk = True Then Velocity.Z -= 0.5 : Stagger = 0.5

            If Not Velocity = New VEC3 Then

                .Camera_Object.Velocity = MAT44.RotationMatrixYAxis(.Camera_Object.Rotation.Y, New VEC3).MVector(VEC3.UnitVector(Velocity) * (Player_Velocity * Stagger))
                .Camera_Object.Move(.Camera_Object.Velocity * TimePassed)

            End If
        End With
    End Sub

    Private Sub Form_Resized(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Resize
        If Not (Engine Is Nothing OrElse PB.Width = 0 OrElse PB.Height = 0) Then
            Engine.Resize_Display(PB.Width, PB.Height)
        End If
    End Sub

    Private Sub Form_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Load_Textures("../../resources/Textures/")
        Load_Models("../../resources/Models/")
        Load_Worlds("../../resources/Worlds/")

        Last_Update = Now

        Engine = New Engine3D(PB.Width, PB.Height)
        Paint_Clock.Interval = 1
        Paint_Clock.Start()
    End Sub

    Private Sub Paint_Clock_Tick(ByVal sender As System.Object, ByVal e As EventArgs) Handles Paint_Clock.Tick
        Dim TimePassed As Double = (Now - Last_Update).TotalMilliseconds * 0.001
        Handle_Camera(TimePassed)
        Handle_Mouse()
        Engine.Draw(List_Worlds(Current_World))
        Engine.Set_Fill_Mode(Engine3D.Fill_Mode.Gouraud)
        Engine.Set_Face_Culling(Engine3D.Face_Cull_Mode.None)
        PB.Refresh()
        Last_Update = Now
    End Sub

    Private Sub PB_Paint(ByVal sender As System.Object, ByVal e As PaintEventArgs) Handles PB.Paint
        Dim MB As Memory_Bitmap = Engine.Front_Buffer
        e.Graphics.DrawImage(MB.Bitmap, 0, 0, PB.Width, PB.Height)

        Dim text_height As Integer = 12
        Dim current_height As Integer = 0
        For Each S As String In Engine.Debug_Info
            e.Graphics.DrawString(S, console_font, Brushes.White, 0, current_height)
            current_height += text_height
        Next
    End Sub

End Class