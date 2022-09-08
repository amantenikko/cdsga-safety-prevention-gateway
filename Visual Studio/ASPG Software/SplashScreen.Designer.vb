<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SplashScreen
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SplashScreen))
        Me.MainLayoutPanel = New System.Windows.Forms.TableLayoutPanel()
        Me.TitleApplication = New gLabel.gLabel()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.BunifuProgressBar1 = New Bunifu.UI.WinForms.BunifuProgressBar()
        Me.Version = New gLabel.gLabel()
        Me.Copyright = New gLabel.gLabel()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.MainLayoutPanel.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'MainLayoutPanel
        '
        Me.MainLayoutPanel.BackColor = System.Drawing.Color.Transparent
        Me.MainLayoutPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.MainLayoutPanel.ColumnCount = 1
        Me.MainLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 243.0!))
        Me.MainLayoutPanel.Controls.Add(Me.TitleApplication, 0, 0)
        Me.MainLayoutPanel.Controls.Add(Me.TableLayoutPanel1, 0, 1)
        Me.MainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.MainLayoutPanel.Location = New System.Drawing.Point(0, -9)
        Me.MainLayoutPanel.Name = "MainLayoutPanel"
        Me.MainLayoutPanel.RowCount = 2
        Me.MainLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75.0!))
        Me.MainLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.MainLayoutPanel.Size = New System.Drawing.Size(658, 257)
        Me.MainLayoutPanel.TabIndex = 0
        '
        'TitleApplication
        '
        Me.TitleApplication.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TitleApplication.Font = New System.Drawing.Font("Microsoft Sans Serif", 30.0!)
        Me.TitleApplication.ForeColor = System.Drawing.Color.White
        Me.TitleApplication.Glow = 10
        Me.TitleApplication.GlowColor = System.Drawing.Color.Black
        Me.TitleApplication.Location = New System.Drawing.Point(3, 0)
        Me.TitleApplication.Name = "TitleApplication"
        Me.TitleApplication.Size = New System.Drawing.Size(652, 192)
        Me.TitleApplication.TabIndex = 7
        Me.TitleApplication.Text = "ApplicationTitle"
        Me.TitleApplication.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TableLayoutPanel1.BackColor = System.Drawing.Color.Transparent
        Me.TableLayoutPanel1.ColumnCount = 1
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.BunifuProgressBar1, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.Version, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.Copyright, 0, 0)
        Me.TableLayoutPanel1.ForeColor = System.Drawing.Color.White
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(3, 195)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 3
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 19.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(652, 59)
        Me.TableLayoutPanel1.TabIndex = 2
        '
        'BunifuProgressBar1
        '
        Me.BunifuProgressBar1.AllowAnimations = False
        Me.BunifuProgressBar1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BunifuProgressBar1.Animation = 0
        Me.BunifuProgressBar1.AnimationSpeed = 220
        Me.BunifuProgressBar1.AnimationStep = 10
        Me.BunifuProgressBar1.BackColor = System.Drawing.Color.FromArgb(CType(CType(223, Byte), Integer), CType(CType(223, Byte), Integer), CType(CType(223, Byte), Integer))
        Me.BunifuProgressBar1.BackgroundImage = CType(resources.GetObject("BunifuProgressBar1.BackgroundImage"), System.Drawing.Image)
        Me.BunifuProgressBar1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(223, Byte), Integer), CType(CType(223, Byte), Integer), CType(CType(223, Byte), Integer))
        Me.BunifuProgressBar1.BorderRadius = 9
        Me.BunifuProgressBar1.BorderThickness = 1
        Me.BunifuProgressBar1.Location = New System.Drawing.Point(3, 43)
        Me.BunifuProgressBar1.Maximum = 100
        Me.BunifuProgressBar1.MaximumValue = 100
        Me.BunifuProgressBar1.Minimum = 0
        Me.BunifuProgressBar1.MinimumValue = 0
        Me.BunifuProgressBar1.Name = "BunifuProgressBar1"
        Me.BunifuProgressBar1.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.BunifuProgressBar1.ProgressBackColor = System.Drawing.Color.FromArgb(CType(CType(223, Byte), Integer), CType(CType(223, Byte), Integer), CType(CType(223, Byte), Integer))
        Me.BunifuProgressBar1.ProgressColorLeft = System.Drawing.Color.LimeGreen
        Me.BunifuProgressBar1.ProgressColorRight = System.Drawing.Color.LimeGreen
        Me.BunifuProgressBar1.Size = New System.Drawing.Size(646, 13)
        Me.BunifuProgressBar1.TabIndex = 3
        Me.BunifuProgressBar1.Value = 0
        Me.BunifuProgressBar1.ValueByTransition = 0
        '
        'Version
        '
        Me.Version.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Version.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!)
        Me.Version.ForeColor = System.Drawing.Color.White
        Me.Version.GlowColor = System.Drawing.Color.Black
        Me.Version.Location = New System.Drawing.Point(3, 20)
        Me.Version.Name = "Version"
        Me.Version.Size = New System.Drawing.Size(646, 20)
        Me.Version.TabIndex = 5
        Me.Version.Text = "Version {0}.{1:00}"
        Me.Version.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Copyright
        '
        Me.Copyright.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Copyright.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!)
        Me.Copyright.ForeColor = System.Drawing.Color.White
        Me.Copyright.GlowColor = System.Drawing.Color.Black
        Me.Copyright.Location = New System.Drawing.Point(3, 0)
        Me.Copyright.Name = "Copyright"
        Me.Copyright.Size = New System.Drawing.Size(646, 20)
        Me.Copyright.TabIndex = 6
        Me.Copyright.Text = "Copyright"
        Me.Copyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Timer1
        '
        '
        'SplashScreen
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackgroundImage = Global.ASPG_Software_Project.My.Resources.Resources.LoadingBg
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(658, 248)
        Me.ControlBox = False
        Me.Controls.Add(Me.MainLayoutPanel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "SplashScreen"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.MainLayoutPanel.ResumeLayout(False)
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents MainLayoutPanel As TableLayoutPanel
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents BunifuProgressBar1 As Bunifu.UI.WinForms.BunifuProgressBar
    Friend WithEvents Timer1 As Timer
    Friend WithEvents Version As gLabel.gLabel
    Friend WithEvents Copyright As gLabel.gLabel
    Friend WithEvents TitleApplication As gLabel.gLabel
End Class
