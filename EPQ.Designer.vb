<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class EPQ
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.PB = New System.Windows.Forms.PictureBox
        CType(Me.PB, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PB
        '
        Me.PB.BackColor = System.Drawing.Color.Black
        Me.PB.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PB.Location = New System.Drawing.Point(0, 0)
        Me.PB.Name = "PB"
        Me.PB.Size = New System.Drawing.Size(512, 288)
        Me.PB.TabIndex = 0
        Me.PB.TabStop = False
        '
        'EPQ
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(512, 288)
        Me.Controls.Add(Me.PB)
        Me.DoubleBuffered = True
        Me.Name = "EPQ"
        Me.Text = "EPQ"
        CType(Me.PB, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents PB As System.Windows.Forms.PictureBox

End Class
