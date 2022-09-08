Imports System.ComponentModel

Public NotInheritable Class SplashScreen

    'Private Const CS_DROPSHADOW As Integer = 131072

    'Protected Overrides ReadOnly Property CreateParams() As System.Windows.Forms.CreateParams
    '    Get
    '        Dim cp As CreateParams = MyBase.CreateParams
    '        cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW
    '        Return cp
    '    End Get
    'End Property

    'TODO: This form can easily be set as the splash screen for the application by going to the "Application" tab
    '  of the Project Designer ("Properties" under the "Project" menu).


    Private Sub SplashScreen1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'Set up the dialog text at runtime according to the application's assembly information.  

        'TODO: Customize the application's assembly information in the "Application" pane of the project 
        '  properties dialog (under the "Project" menu).

        'Application title
        If My.Application.Info.Title <> "" Then
            TitleApplication.Text = My.Application.Info.Title
        Else
            'If the application title is missing, use the application name, without the extension
            TitleApplication.Text = System.IO.Path.GetFileNameWithoutExtension(My.Application.Info.AssemblyName)
        End If

        'Format the version information using the text set into the Version control at design time as the
        '  formatting string.  This allows for effective localization if desired.
        '  Build and revision information could be included by using the following code and changing the 
        '  Version control's designtime text to "Version {0}.{1:00}.{2}.{3}" or something similar.  See
        '  String.Format() in Help for more information.
        '
        '    Version.Text = System.String.Format(Version.Text, My.Application.Info.Version.Major, My.Application.Info.Version.Minor, My.Application.Info.Version.Build, My.Application.Info.Version.Revision)

        Version.Text = System.String.Format(Version.Text, My.Application.Info.Version.Major, My.Application.Info.Version.Minor)

        'Copyright info
        Copyright.Text = My.Application.Info.Copyright
        Timer1.Start()
        Form_Main.Show()
        Form_Settings.Show()
        Form_DataView.Show()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If BunifuProgressBar1.Value < 100 Then
            System.Threading.Thread.Sleep(50)
            BunifuProgressBar1.Value = BunifuProgressBar1.Value + 5
        Else
            System.Threading.Thread.Sleep(500)
            'Form_Main.ShowInTaskbar = True
            Form_Main.Opacity = 100
            Timer1.Stop()
            'Me.Hide()
            Close()
        End If
    End Sub
End Class
