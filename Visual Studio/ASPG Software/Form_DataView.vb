Imports System.ComponentModel
Imports System.Globalization
Imports PdfSharp.Pdf
Imports MigraDoc.Rendering
Imports MigraDoc.DocumentObjectModel
Imports MigraDoc.DocumentObjectModel.Tables
Imports System.Text.RegularExpressions

Public Class Form_DataView

    Private isSelectableTable As Boolean = False
    Private reRunLoad As Boolean

    Dim isDateSwitch As Boolean
    Dim pageTemp As Integer
    Dim pageIsFocused As Boolean
    Dim scrollVal As Integer = 0
    Dim maxVal As Integer = 15
    Dim orderBy As String = "visitor_datetimein"
    Dim arrangeBy As String = "ASC"

    Dim exportDate1 As Int16
    Dim exportDate2 As String

    Dim GripDrag As Boolean
    Dim InitialSizeX As Integer
    Dim InitialSizeY As Integer

    Private AeroEnabled As Boolean

    Protected Overloads Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            CheckAeroEnabled()
            Dim MyCreateParams As CreateParams = MyBase.CreateParams
            MyCreateParams.ExStyle = MyCreateParams.ExStyle Or &H80 Or &H2000000
            If Not AeroEnabled Then MyCreateParams.ClassStyle = MyCreateParams.ClassStyle Or NativeConstants.CS_DROPSHADOW
            Return MyCreateParams
        End Get
    End Property

    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case NativeConstants.WM_NCPAINT
                Dim val = 2
                If AeroEnabled Then
                    NativeMethods.DwmSetWindowAttribute(Handle, 2, val, 4)
                    Dim bla As New NativeStructs.MARGINS()
                    With bla
                        .bottomHeight = 1
                        .leftWidth = 0
                        .rightWidth = 0
                        .topHeight = 0
                    End With
                    NativeMethods.DwmExtendFrameIntoClientArea(Handle, bla)
                End If
                Exit Select
        End Select
        MyBase.WndProc(m)
    End Sub

    Private Sub CheckAeroEnabled()
        If Environment.OSVersion.Version.Major >= 6 Then
            Dim enabled As Integer = 0
            Dim response As Integer = NativeMethods.DwmIsCompositionEnabled(enabled)
            AeroEnabled = (enabled = 1)
        Else
            AeroEnabled = False
        End If
    End Sub

    Sub LayoutLoad()
        BunifuLoader1.Visible = True
        BunifuLabel3.Visible = False
        isSelectableTable = False

        dtSet.Tables("dataTbl").Clear()
        DataGridView1.Refresh()

        CheckDBnTblIfExist(True)
        If BunifuDropdown1.SelectedIndex = 0 Then       'alltime
            AdapterDBQuery("SELECT COUNT(*) FROM `tbl_logbook`", "dataTblCount")
        ElseIf BunifuDropdown1.SelectedIndex = 2 Then   'weekly
            AdapterDBQuery("SELECT COUNT(*) FROM `tbl_logbook` WHERE CONCAT(
                         DATE_FORMAT(DATE(`visitor_datetimein` + INTERVAL ( - WEEKDAY(`visitor_datetimein`)) DAY), ""%m/%d/%Y""), "" - "",
                         DATE_FORMAT(DATE(`visitor_datetimein` + INTERVAL (6 - WEEKDAY(`visitor_datetimein`)) DAY), ""%m/%d/%Y"")) = """ & BunifuDropdown4.Text & """", "dataTblCount")
        ElseIf BunifuDropdown1.SelectedIndex = 3 Then   'monthly
            AdapterDBQuery("SELECT COUNT(*) FROM `tbl_logbook` WHERE DATE_FORMAT(`visitor_datetimein`, ""%M %Y"") = """ & BunifuDropdown4.Text & """", "dataTblCount")
        ElseIf BunifuDropdown1.SelectedIndex = 4 Then   'yearly
            AdapterDBQuery("SELECT COUNT(*) FROM `tbl_logbook` WHERE DATE_FORMAT(`visitor_datetimein`, ""%Y"") = """ & BunifuDropdown4.Text & """", "dataTblCount")
        Else
            AdapterDBQuery("SELECT COUNT(*) FROM `tbl_logbook` WHERE DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") = """ & BunifuDropdown4.Text & """", "dataTblCount")
        End If

        If Math.Ceiling(dtSet.Tables("dataTblCount").Rows(0).Item(0) / maxVal) = 0 Then
            BunifuLabel_MaxPage.Text = "of&nbsp;&nbsp;&nbsp;&nbsp; 1"
        Else
            BunifuLabel_MaxPage.Text = "of&nbsp;&nbsp;&nbsp;&nbsp; " & Math.Ceiling(dtSet.Tables("dataTblCount").Rows(0).Item(0) / maxVal).ToString
        End If

        If Math.Ceiling(dtSet.Tables("dataTblCount").Rows(0).Item(0) / maxVal) <= 1 Then
            Btn_First.Enabled = False
            Btn_Prev.Enabled = False
            Btn_Next.Enabled = False
            Btn_Last.Enabled = False
        ElseIf BunifuTextBox_PageNum.Text = "1" And Math.Ceiling(dtSet.Tables("dataTblCount").Rows(0).Item(0) / maxVal) > 1 Then
            Btn_First.Enabled = False
            Btn_Prev.Enabled = False
            Btn_Next.Enabled = True
            Btn_Last.Enabled = True
        ElseIf BunifuTextBox_PageNum.Text = Math.Ceiling(dtSet.Tables("dataTblCount").Rows(0).Item(0) / maxVal).ToString Then
            Btn_First.Enabled = True
            Btn_Prev.Enabled = True
            Btn_Next.Enabled = False
            Btn_Last.Enabled = False
        Else
            Btn_First.Enabled = True
            Btn_Prev.Enabled = True
            Btn_Next.Enabled = True
            Btn_Last.Enabled = True
        End If

        If Not BackgroundWorker1.IsBusy Then BackgroundWorker1.RunWorkerAsync()
    End Sub

    Private Sub Form_DataView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BunifuDropdown2.SelectedIndex = 6
        BunifuDropdown3.SelectedIndex = 1
        BunifuTextBox_PageNum.Text = 1
        BunifuDropdown1.SelectedIndex = 1
        If BunifuDropdown4.Items.Count > 0 Then BunifuDropdown4.SelectedIndex = 0

        BunifuTextBox_ExportLocation.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        FolderBrowserDialog1.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)

        reRunLoad = True
        DataGridView1.AutoGenerateColumns = True
        DataGridView1.DataSource = dtSet
        DataGridView1.DataMember = "dataTbl"
        LayoutLoad()
    End Sub

    Private Sub BunifuButton_Close_Click(sender As Object, e As EventArgs) Handles BunifuButton_Close.Click
        Me.Opacity = 0
    End Sub

    Private Sub DataGridView1_LostFocus(sender As Object, e As EventArgs) Handles DataGridView1.LostFocus, DataGridView1.MouseLeave
        DataGridView1.ClearSelection()
    End Sub

    Private Sub BunifuImageButton1_Click(sender As Object, e As EventArgs) Handles BunifuImageButton1.Click
        If BunifuShapes5.Visible Then
            Me.WindowState = FormWindowState.Maximized
            BunifuShapes5.Visible = False
            BunifuImageButton1.Image = My.Resources.minimize_control_icon
        Else
            Me.WindowState = FormWindowState.Normal
            BunifuShapes5.Visible = True
            BunifuImageButton1.Image = My.Resources.maximize_control_icon
        End If
    End Sub

    Private Sub BunifuDropdown_SelectedIndexChanged(sender As Object, e As EventArgs) Handles BunifuDropdown2.SelectedIndexChanged, BunifuDropdown3.SelectedIndexChanged, BunifuDropdown1.SelectedIndexChanged, BunifuDropdown4.SelectedIndexChanged
        DataGridView1.ClientSize = New Size(DataGridView1.Width, 20)
        BunifuTextBox_PageNum.Text = 1
        scrollVal = 0

        Select Case BunifuDropdown2.SelectedIndex
            Case 0
                orderBy = "visitor_name"
            Case 1
                orderBy = "visitor_program"
            Case 2
                orderBy = "visitor_contact"
            Case 3
                orderBy = "visitor_address"
            Case 4
                orderBy = "visitor_purpose"
            Case 5
                orderBy = "visitor_temp"
            Case Else
                orderBy = "visitor_datetimein"
        End Select

        Select Case BunifuDropdown3.SelectedIndex
            Case 0
                arrangeBy = "ASC"
            Case Else
                arrangeBy = "DESC"
        End Select

        If sender.Equals(BunifuDropdown1) Then
            Select Case BunifuDropdown1.SelectedIndex
                Case 0 'all time
                    Button1.Hide()
                    Button2.Hide()
                    BunifuDropdown4.Hide()
                Case 1 'daily
                    Button1.Show()
                    Button2.Show()
                    BunifuDropdown4.Show()
                    BunifuDropdown4.Items.Clear()

                    CheckDBnTblIfExist(True)
                    AdapterDBQuery("SELECT DISTINCT DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") AS day FROM (
                                    SELECT DISTINCT `visitor_datetimein` FROM tbl_logbook ORDER BY `visitor_datetimein` DESC) AS tbl_temp", "dataTbldateday")
                    For Each row As DataRow In dtSet.Tables("dataTbldateday").Rows
                        BunifuDropdown4.Items.Add(row.Field(Of String)("day"))
                    Next
                    If BunifuDropdown4.Items.Count <= 0 Then
                        BunifuDropdown4.Text = "No data found."
                    Else
                        BunifuDropdown4.SelectedItem = dtSet.Tables("dataTbldateday").Rows(0).Item(0).ToString()
                    End If

                Case 2 'weekly
                    Button1.Show()
                    Button2.Show()
                    BunifuDropdown4.Show()

                    CheckDBnTblIfExist(True)
                    AdapterDBQuery("SELECT DISTINCT CONCAT(
                    DATE_FORMAT(DATE(`visitor_datetimein` + INTERVAL ( - WEEKDAY(`visitor_datetimein`)) DAY), ""%m/%d/%Y""), "" - "",
                    DATE_FORMAT(DATE(`visitor_datetimein` + INTERVAL (6 - WEEKDAY(`visitor_datetimein`)) DAY), ""%m/%d/%Y"")) AS week
                    FROM (SELECT DISTINCT `visitor_datetimein` FROM tbl_logbook ORDER BY `visitor_datetimein` DESC) AS tbl_temp", "dataTbldateweek")
                    BunifuDropdown4.Items.Clear()
                    For Each row As DataRow In dtSet.Tables("dataTbldateweek").Rows
                        BunifuDropdown4.Items.Add(row.Field(Of String)("week"))
                    Next
                    If BunifuDropdown4.Items.Count <= 0 Then
                        BunifuDropdown4.Text = "No data found."
                    Else
                        BunifuDropdown4.SelectedItem = dtSet.Tables("dataTbldateweek").Rows(0).Item(0).ToString()
                    End If

                Case 3 'monthly
                    Button1.Show()
                    Button2.Show()
                    BunifuDropdown4.Show()

                    CheckDBnTblIfExist(True)
                    AdapterDBQuery("SELECT DISTINCT DATE_FORMAT(`visitor_datetimein`, ""%M %Y"") As month FROM (
                    SELECT `visitor_datetimein` FROM tbl_logbook ORDER BY `visitor_datetimein` DESC) AS tbl_temp", "dataTbldatemonth")
                    BunifuDropdown4.Items.Clear()
                    For Each row As DataRow In dtSet.Tables("dataTbldatemonth").Rows
                        BunifuDropdown4.Items.Add(row.Field(Of String)("month"))
                    Next
                    If BunifuDropdown4.Items.Count <= 0 Then
                        BunifuDropdown4.Text = "No data found."
                    Else
                        BunifuDropdown4.SelectedItem = dtSet.Tables("dataTbldatemonth").Rows(0).Item(0).ToString()
                    End If

                Case 4 'yearly
                    Button1.Show()
                    Button2.Show()
                    BunifuDropdown4.Show()

                    CheckDBnTblIfExist(True)
                    AdapterDBQuery("SELECT DISTINCT DATE_FORMAT(`visitor_datetimein`, ""%Y"") As year FROM (
                    SELECT `visitor_datetimein` FROM tbl_logbook ORDER BY `visitor_datetimein` DESC) AS tbl_temp", "dataTbldateyear")
                    BunifuDropdown4.Items.Clear()
                    For Each row As DataRow In dtSet.Tables("dataTbldateyear").Rows
                        BunifuDropdown4.Items.Add(row.Field(Of String)("year"))
                    Next
                    If BunifuDropdown4.Items.Count <= 0 Then
                        BunifuDropdown4.Text = "No data found."
                    Else
                        BunifuDropdown4.SelectedItem = dtSet.Tables("dataTbldateyear").Rows(0).Item(0).ToString()
                    End If
            End Select

            If BunifuDropdown1.SelectedIndex <> 0 And isDateSwitch = True Then
                BunifuDropdown1.Location = New Point(BunifuDropdown1.Location.X - 170, BunifuDropdown1.Location.Y)
                BunifuLabel1.Location = New Point(BunifuLabel1.Location.X - 170, BunifuLabel1.Location.Y)
                isDateSwitch = False
            ElseIf BunifuDropdown1.SelectedIndex = 0 And isDateSwitch = False Then
                BunifuDropdown1.Location = New Point(BunifuDropdown1.Location.X + 170, BunifuDropdown1.Location.Y)
                BunifuLabel1.Location = New Point(BunifuLabel1.Location.X + 170, BunifuLabel1.Location.Y)
                isDateSwitch = True
            End If
        End If

        LayoutLoad()
    End Sub

    Private Sub Button_NxtPrvDates_Click(sender As Object, e As EventArgs) Handles Button1.Click, Button2.Click
        Try
            If sender.Equals(Button1) Then
                If BunifuDropdown4.Items.Count > 1 And BunifuDropdown4.SelectedIndex < BunifuDropdown4.Items.Count Then
                    BunifuDropdown4.SelectedIndex = BunifuDropdown4.SelectedIndex + 1
                End If
            End If
            If sender.Equals(Button2) Then
                If BunifuDropdown4.Items.Count > 1 And BunifuDropdown4.SelectedIndex > 0 Then
                    BunifuDropdown4.SelectedIndex = BunifuDropdown4.SelectedIndex - 1
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub DataGridView1_SelectionChanged(sender As Object, e As EventArgs) Handles DataGridView1.SelectionChanged
        If Not isSelectableTable Then DataGridView1.ClearSelection()
    End Sub

    Private Sub BackgroundWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        For i = 0 To 100
            BackgroundWorker1.ReportProgress(i)
            Threading.Thread.Sleep(1)
            i += 1
        Next
    End Sub

    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
    End Sub

    Private Sub BackgroundWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        CheckDBnTblIfExist(True)
        If dtSet.Tables("checkTbl").Rows.Count > 0 Then
            If BunifuDropdown1.SelectedIndex = 0 Then 'all time
                AdapterDBQuery("SELECT `visitor_name`, `visitor_program`, `visitor_contact`, `visitor_address`,
                        `visitor_purpose`, `visitor_temp`, `visitor_datetimein`, DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") AS 'Date' FROM `tbl_logbook` ORDER BY `" & orderBy & "` " & arrangeBy & " LIMIT " & maxVal & " OFFSET " & scrollVal, "dataTbl")
            ElseIf BunifuDropdown1.SelectedIndex = 1 Then 'daily
                AdapterDBQuery("SELECT `visitor_name`, `visitor_program`, `visitor_contact`, `visitor_address`,
                        `visitor_purpose`, `visitor_temp`, `visitor_datetimein` FROM `tbl_logbook` WHERE DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") = """ & BunifuDropdown4.Text & """ ORDER BY `" & orderBy & "` " & arrangeBy & " LIMIT " & maxVal & " OFFSET " & scrollVal, "dataTbl")
            ElseIf BunifuDropdown1.SelectedIndex = 2 Then 'weekly
                AdapterDBQuery("SELECT `visitor_name`, `visitor_program`, `visitor_contact`, `visitor_address`,
                        `visitor_purpose`, `visitor_temp`, `visitor_datetimein`, DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") AS 'Date' FROM `tbl_logbook` WHERE CONCAT(
                         DATE_FORMAT(DATE(`visitor_datetimein` + INTERVAL ( - WEEKDAY(`visitor_datetimein`)) DAY), ""%m/%d/%Y""), "" - "",
                         DATE_FORMAT(DATE(`visitor_datetimein` + INTERVAL (6 - WEEKDAY(`visitor_datetimein`)) DAY), ""%m/%d/%Y"")) = """ & BunifuDropdown4.Text & """ ORDER BY `" & orderBy & "` " & arrangeBy & " LIMIT " & maxVal & " OFFSET " & scrollVal, "dataTbl")
            ElseIf BunifuDropdown1.SelectedIndex = 3 Then 'monthly
                AdapterDBQuery("SELECT `visitor_name`, `visitor_program`, `visitor_contact`, `visitor_address`,
                        `visitor_purpose`, `visitor_temp`, `visitor_datetimein`, DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") AS 'Date' FROM `tbl_logbook` WHERE DATE_FORMAT(`visitor_datetimein`, ""%M %Y"") = """ & BunifuDropdown4.Text & """ ORDER BY `" & orderBy & "` " & arrangeBy & " LIMIT " & maxVal & " OFFSET " & scrollVal, "dataTbl")
            ElseIf BunifuDropdown1.SelectedIndex = 4 Then 'yearly
                AdapterDBQuery("SELECT `visitor_name`, `visitor_program`, `visitor_contact`, `visitor_address`,
                        `visitor_purpose`, `visitor_temp`, `visitor_datetimein`, DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") AS 'Date' FROM `tbl_logbook` WHERE DATE_FORMAT(`visitor_datetimein`, ""%Y"") = """ & BunifuDropdown4.Text & """ ORDER BY `" & orderBy & "` " & arrangeBy & " LIMIT " & maxVal & " OFFSET " & scrollVal, "dataTbl")
            End If
        Else
            dtSet.Tables("dataTbl").Clear()
        End If

        Try
            For column = 0 To DataGridView1.Columns.Count - 1
                DataGridView1.Columns.Item(column).SortMode = DataGridViewColumnSortMode.Programmatic
                DataGridView1.Columns(column).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft
            Next column

            With DataGridView1
                .ColumnHeadersDefaultCellStyle.Padding = New Padding(0, 0, 0, 0)
                .Columns(0).HeaderText = "Name"
                .Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                .Columns(1).HeaderText = "Program"
                .Columns(1).Width = 90
                .Columns(2).HeaderText = "Contact No."
                .Columns(2).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                .Columns(3).HeaderText = "Address"
                .Columns(3).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                .Columns(4).HeaderText = "Purpose"
                .Columns(4).Width = 125
                .Columns(4).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                .Columns(5).HeaderText = "Temperature"
                .Columns(5).AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
                .Columns(5).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                .Columns(6).HeaderText = "Time In"
                If My.Settings.displaySeconds Then
                    .Columns(6).DefaultCellStyle.Format = "h:mm:ss tt"
                Else
                    .Columns(6).DefaultCellStyle.Format = "h:mm tt"
                End If
                .Columns(6).AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
                .Columns(6).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                .ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False
                If .Columns.Count >= 8 Then
                    If BunifuDropdown1.SelectedIndex <> 1 Then 'daily
                        .Columns(7).Visible = True
                        .Columns(7).HeaderText = "Date In"
                        .Columns(7).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader
                        .Columns(7).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                    Else
                        .Columns(7).Visible = False
                    End If
                End If
            End With
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try

        If dtSet.Tables("dataTbl").Rows.Count > 0 Then
        Else
            BunifuLabel3.Visible = True
        End If
        Try
            dtSet.Tables("dataTbl").Columns.Add("id", GetType(String), "No.").SetOrdinal(0)
        Catch ex As Exception
        End Try

        Dim height As Integer = 0
        For Each row As DataGridViewRow In DataGridView1.Rows
            height += row.Height
        Next
        height += DataGridView1.ColumnHeadersHeight

        'Dim width As Integer = 0
        'For Each col As DataGridViewColumn In DataGridView1.Columns
        '    width += col.Width
        'Next
        'width += DataGridView1.RowHeadersWidth
        DataGridView1.ClientSize = New Size(DataGridView1.Width, height + 2)

        BunifuLoader1.Visible = False
        DataGridView1.Refresh()
        DataGridView1.ClearSelection()
        isSelectableTable = True

        If reRunLoad Then
            reRunLoad = False
            LayoutLoad()
        End If
    End Sub

    Private Sub Btn_Paging_Click(sender As Object, e As EventArgs) Handles Btn_Prev.Click, Btn_Next.Click, Btn_Last.Click, Btn_First.Click
        DataGridView1.ClientSize = New Size(DataGridView1.Width, 20)

        If pageIsFocused And pageTemp <> Integer.Parse(BunifuTextBox_PageNum.Text) Then
            BunifuTextBox_PageNum.Text = pageTemp.ToString
            pageIsFocused = False
        End If

        If sender.Equals(Btn_First) Then
            scrollVal = 0
            BunifuTextBox_PageNum.Text = "1"
        End If
        If sender.Equals(Btn_Prev) Then
            scrollVal -= maxVal
            If (Integer.Parse(BunifuTextBox_PageNum.Text)) > 1 Then BunifuTextBox_PageNum.Text = (Integer.Parse(BunifuTextBox_PageNum.Text) - 1).ToString
        End If
        If sender.Equals(Btn_Next) Then
            scrollVal += maxVal
            If (Integer.Parse(BunifuTextBox_PageNum.Text)) < Math.Ceiling(dtSet.Tables("dataTblCount").Rows(0).Item(0) / maxVal) Then BunifuTextBox_PageNum.Text = (Integer.Parse(BunifuTextBox_PageNum.Text) + 1).ToString
        End If
        If sender.Equals(Btn_Last) Then
            scrollVal = (dtSet.Tables("dataTblCount").Rows(0).Item(0) - (dtSet.Tables("dataTblCount").Rows(0).Item(0) Mod maxVal)) '- 1
            BunifuTextBox_PageNum.Text = Math.Ceiling(dtSet.Tables("dataTblCount").Rows(0).Item(0) / maxVal).ToString
        End If

        If scrollVal <= 0 Then
            scrollVal = 0
        End If

        LayoutLoad()
    End Sub

    Private Sub BunifuShapes5_MouseDown(sender As Object, e As MouseEventArgs) Handles BunifuShapes5.MouseDown
        BunifuLoader1.Visible = True
        BunifuPanel2.Visible = False
        'DataGridView1.Visible = False

        DataGridView1.ClientSize = New Size(DataGridView1.Width, 20)

        If e.Button = Windows.Forms.MouseButtons.Left Then
            GripDrag = True
            InitialSizeX = Me.Width + 1
            InitialSizeY = Me.Height + 1
        End If
    End Sub

    Private Sub BunifuShapes5_MouseMove(sender As Object, e As MouseEventArgs) Handles BunifuShapes5.MouseMove
        If GripDrag = True Then
            Me.Width = InitialSizeX + (Cursor.Position.X - (Me.Width + Me.Location.X)) + 1
            Me.Height = InitialSizeY + (Cursor.Position.Y - (Me.Height + Me.Location.Y)) + 1
            InitialSizeX = Me.Width
            InitialSizeY = Me.Height
            Me.Refresh()
        End If
    End Sub

    Private Sub BunifuShapes5_MouseUp(sender As Object, e As MouseEventArgs) Handles BunifuShapes5.MouseUp
        GripDrag = False

        BunifuPanel2.Visible = True
        DataGridView1.Visible = True

        maxVal = Math.Round(Me.Size.Height / 32.6)

        LayoutLoad()
    End Sub

    Private Sub ExportButtons_Click(sender As Object, e As EventArgs) Handles BunifuImageButton_ExportShow.Click, BunifuButton_ExportCancel.Click
        If sender.Equals(BunifuImageButton_ExportShow) Then
            If BunifuPanel_Export.Visible Then
                BunifuPanel_Export.Visible = False
            Else
                BunifuPanel_Export.Visible = True
            End If
        Else
            BunifuPanel2_Loading.Visible = False
            BunifuPanel_Export.Visible = False
            BunifuButton_ExportSave.Text = "Save"
            BunifuButton_ExportSave.IconLeftPadding = New Padding(47, 3, 3, 3)
        End If
    End Sub

    Private Sub BunifuTextBox2_OnIconRightClick(sender As Object, e As EventArgs) Handles BunifuTextBox_ExportLocation.OnIconRightClick
        If FolderBrowserDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            BunifuTextBox_ExportLocation.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Private Sub BunifuButton_ExportSave_Click(sender As Object, e As EventArgs) Handles BunifuButton_ExportSave.Click
        exportDate1 = BunifuDropdown1.SelectedIndex
        exportDate2 = BunifuDropdown4.Text

        BunifuButton_ExportSave.Text = "Saving"
        BunifuButton_ExportSave.IconLeftPadding = New Padding(43, 3, 3, 3)
        BunifuPanel2_Loading.Visible = True

        If My.Settings.usePassword = True And My.Settings.passwordSave = True Then
            Dim i As String = Form_InputBox.Show("Password is Required to Save", "Password", "", "", False)
            If i <> My.Settings.password Then Return
            If Not BackgroundWorker_Saving.IsBusy Then BackgroundWorker_Saving.RunWorkerAsync()
        Else
            If Not BackgroundWorker_Saving.IsBusy Then BackgroundWorker_Saving.RunWorkerAsync()
        End If
    End Sub

    Sub SaveAsDocument()
        Dim fileName As String = "VisitorReport-" & DateTime.Now.ToString("M_d_yy", CultureInfo.InvariantCulture) & ".pdf"
        Dim document As Document = CreatePDF()
        IO.DdlWriter.WriteToFile(document, "MigraDoc.mdddl")
        Dim renderer As New PdfDocumentRenderer(True) With {
                .Document = document
            }
        renderer.RenderDocument()

        renderer.PdfDocument.Save(BunifuTextBox_ExportLocation.Text & "\" & fileName)
        If BunifuCheckBox1.Checked Then Process.Start(BunifuTextBox_ExportLocation.Text & "\" & fileName)

        'SaveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Resources)
        'Dim fullpath As String = System.IO.Path.GetFullPath(Application.StartupPath & "\..\..\Resources\")
        '    Dim pdfTemp As PdfDocument = IO.PdfReader.Open("C:\Users\k1Nk0\Desktop\ASPG Software Project\tblTemplate.pdf", IO.PdfDocumentOpenMode.Import)
        '    Dim pdfDoc As New PdfDocument
        '    Dim pdfPage As PdfPage = Nothing

        '    For i = 0 To pdfTemp.PageCount - 1
        '        pdfPage = pdfDoc.AddPage(pdfTemp.Pages(0))
        '    Next

        '    ContentGen(pdfPage)

        '    pdfDoc.Save(fileName)
        '    Process.Start(fileName)
    End Sub

    Function CreatePDF()
        Dim doc As New Document()
        doc.Info.Title = "CDSGA Safety Prevention Gateway Report"
        doc.Info.Subject = "Record of visitors in Colegio de San Gabriel Arcangel."
        doc.Info.Author = "CDSGA Safety Prevention Gateway"

        doc.DefaultPageSetup.HeaderDistance = Unit.FromInch(0.2)
        doc.DefaultPageSetup.FooterDistance = Unit.FromInch(0.19)
        doc.DefaultPageSetup.TopMargin = Unit.FromInch(1.9)
        doc.DefaultPageSetup.BottomMargin = Unit.FromInch(0.38)
        doc.DefaultPageSetup.LeftMargin = Unit.FromInch(0.3)
        doc.DefaultPageSetup.RightMargin = Unit.FromInch(0.3)

        '---------Define Styles------------------------
        Dim style As Style = doc.Styles("Normal")
        style.Font.Name = "Arial Narrow"
        style.ParagraphFormat.AddTabStop("9.75cm", TabAlignment.Center)

        style = doc.Styles(StyleNames.Header)
        style.ParagraphFormat.AddTabStop("9.75cm", TabAlignment.Center)

        style = doc.Styles(StyleNames.Footer)
        style.Font.Size = 8
        style.ParagraphFormat.AddTabStop("9.75cm", TabAlignment.Center)

        '---------Define Content Section---------------
        Dim section As Section = doc.AddSection()
        section.PageSetup.StartingNumber = 1

        Dim image As Shapes.Image = section.Headers.Primary.AddImage(Application.StartupPath & "\School_logo3.png")
        image.Height = 68
        image.LockAspectRatio = True
        image.WrapFormat.Style = Shapes.WrapStyle.Through

        Dim image2 As Shapes.Image = section.Headers.Primary.AddImage(Application.StartupPath & "\School_logo3.png")
        image2.Height = 68
        image2.LockAspectRatio = True
        image2.WrapFormat.Style = Shapes.WrapStyle.Through
        image2.Left = 482

        Dim paraHeader As New Paragraph()
        paraHeader.AddTab()
        paraHeader.AddText("Colegio de San Gabriel Arcangel")
        paraHeader.Format.Font.Name = "Old English Text MT"
        paraHeader.Format.Font.Size = 23
        paraHeader.Format.Font.Color = Color.FromRgb(125, 0, 0)
        paraHeader.Format.Font.Bold = True
        section.Headers.Primary.Add(paraHeader)

        Dim paraHeader2 As New Paragraph()
        paraHeader2.AddTab()
        paraHeader2.AddText("Founded 1993")
        paraHeader2.Format.Font.Name = "Arial"
        paraHeader2.Format.Font.Size = 9
        paraHeader2.Format.Font.Bold = True
        section.Headers.Primary.Add(paraHeader2)

        Dim paraHeader3 As New Paragraph()
        paraHeader3.AddTab()
        paraHeader3.Format.Font.Name = "Arial"
        paraHeader3.Format.Font.Size = 10
        paraHeader3.AddText("Area E, Sapang Palay, City of San Jose del Monte, Bulacan, Philippines")
        paraHeader3.AddLineBreak()
        paraHeader3.AddText(vbTab & "Recognized by the Government: DepEd, TESDA and CHED; PACUCOA Level II Accredited")
        paraHeader3.AddLineBreak()
        paraHeader3.AddText(vbTab & "Call and/or Text: 0915 532 2643/ Telefax No: (044) 760 0301 / (044)760 0397 ")
        paraHeader3.AddLineBreak()
        paraHeader3.AddText(vbTab & "")
        paraHeader3.Format.Borders.Bottom = New Border() With {.Width = 1, .Color = Colors.Black, .Style = BorderStyle.Single}
        paraHeader3.Format.SpaceAfter = 10
        section.Headers.Primary.Add(paraHeader3)

        Dim paraHeader4 As New Paragraph()
        paraHeader4.AddTab()
        paraHeader4.AddText("RECORD OF VISITORS")
        paraHeader4.Format.Font.Name = "Arial"
        paraHeader4.Format.Font.Size = 10
        paraHeader4.Format.Font.Bold = True
        section.Headers.Primary.Add(paraHeader4)

        If exportDate1 <> 0 Then
            section.Headers.Primary.AddParagraph("Date: " & exportDate2)
        Else
            CheckDBnTblIfExist(True)
            AdapterDBQuery("SELECT DISTINCT DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") AS day FROM (
                                    SELECT DISTINCT `visitor_datetimein` FROM tbl_logbook ORDER BY `visitor_datetimein` DESC) AS tbl_temp", "dataTbldateday")

            If dtSet.Tables("dataTbldateday").Rows.Count > 0 Then
                section.Headers.Primary.AddParagraph("Date: " & dtSet.Tables("dataTbldateday").Rows(0).Item(0).ToString() & " - " & dtSet.Tables("dataTbldateday").Rows(dtSet.Tables("dataTbldateday").Rows.Count - 1).Item(0))
            Else
                section.Headers.Primary.AddParagraph("No data found.")
            End If
        End If

        Dim paraFooter As New Paragraph()
        paraFooter.AddTab()
        paraFooter.AddText("Page ")
        paraFooter.AddPageField()
        paraFooter.AddFormattedText(" of")
        paraFooter.AddSpace(2)
        paraFooter.AddNumPagesField()
        section.Footers.Primary.Add(paraFooter)
        section.Footers.EvenPage.Add(paraFooter.Clone())

        '---------Define Tales-------------------------
        Dim table As New Table()
        table.Borders.Width = 0.5

        Dim column As Column =
        table.AddColumn(Unit.FromCentimeter(0.7))       'No.
        column.Format.Alignment = ParagraphAlignment.Center
        table.AddColumn(Unit.FromCentimeter(3.0))       'Name
        If exportDate1 <> 1 Then
            table.AddColumn(Unit.FromCentimeter(2.1))   'Program
        Else
            table.AddColumn(Unit.FromCentimeter(2.8))   'Program
        End If
        If exportDate1 <> 1 Then
            table.AddColumn(Unit.FromCentimeter(1.9))   'Contact
        Else
            table.AddColumn(Unit.FromCentimeter(2.3))   'Contact
        End If
        table.AddColumn(Unit.FromCentimeter(3.3))       'Address
        table.AddColumn(Unit.FromCentimeter(3.3))       'Purpose
        If exportDate1 <> 1 Then
            table.AddColumn(Unit.FromCentimeter(2.1))   'Temperature
        Else
            table.AddColumn(Unit.FromCentimeter(2.5))   'Temperature
        End If
        If exportDate1 <> 1 Then
            table.AddColumn(Unit.FromCentimeter(1.7))   'Time In
        Else
            table.AddColumn(Unit.FromCentimeter(2.3))   'Time In
        End If
        If exportDate1 <> 1 Then table.AddColumn(Unit.FromCentimeter(1.6))   'Date In

        Dim row As Row = table.AddRow()
        row.HeadingFormat = True
        row.Format.Font.Size = 9
        row.Height = 20
        row.Shading.Color = Colors.White
        row.VerticalAlignment = VerticalAlignment.Center
        row.Format.Font.Bold = True
        row.Borders.Bottom.Width = 1

        Dim cell As Cell =
               row.Cells(0)
        cell.AddParagraph("#")
        cell = row.Cells(1)
        cell.AddParagraph("Name")
        cell = row.Cells(2)
        cell.AddParagraph("Program")
        cell = row.Cells(3)
        cell.AddParagraph("Contact")
        cell = row.Cells(4)
        cell.AddParagraph("Address")
        cell = row.Cells(5)
        cell.AddParagraph("Purpose")
        cell = row.Cells(6)
        cell.AddParagraph("Temperature")
        cell = row.Cells(7)
        cell.AddParagraph("Time In")
        If exportDate1 <> 1 Then
            cell = row.Cells(8)
            cell.AddParagraph("Date In")
        End If

        CheckDBnTblIfExist(True)
        If dtSet.Tables("checkTbl").Rows.Count > 0 Then
            dtSet.Tables("ExportTbl").Reset()
            If exportDate1 = 0 Then 'all time
                AdapterDBQuery("SELECT `visitor_name`, `visitor_program`, `visitor_contact`, `visitor_address`,
                        `visitor_purpose`, `visitor_temp`, `visitor_datetimein`, DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") AS 'Date' FROM `tbl_logbook` ORDER BY `" & orderBy & "` " & arrangeBy, "ExportTbl")
            ElseIf exportDate1 = 1 Then 'daily
                AdapterDBQuery("SELECT `visitor_name`, `visitor_program`, `visitor_contact`, `visitor_address`,
                        `visitor_purpose`, `visitor_temp`, `visitor_datetimein` FROM `tbl_logbook` WHERE DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") = """ & exportDate2 & """ ORDER BY `" & orderBy & "` " & arrangeBy, "ExportTbl")
            ElseIf exportDate1 = 2 Then 'weekly
                AdapterDBQuery("SELECT `visitor_name`, `visitor_program`, `visitor_contact`, `visitor_address`,
                        `visitor_purpose`, `visitor_temp`, `visitor_datetimein`, DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") AS 'Date' FROM `tbl_logbook` WHERE CONCAT(
                         DATE_FORMAT(DATE(`visitor_datetimein` + INTERVAL ( - WEEKDAY(`visitor_datetimein`)) DAY), ""%m/%d/%Y""), "" - "",
                         DATE_FORMAT(DATE(`visitor_datetimein` + INTERVAL (6 - WEEKDAY(`visitor_datetimein`)) DAY), ""%m/%d/%Y"")) = """ & exportDate2 & """ ORDER BY `" & orderBy & "` " & arrangeBy, "ExportTbl")
            ElseIf exportDate1 = 3 Then 'monthly
                AdapterDBQuery("SELECT `visitor_name`, `visitor_program`, `visitor_contact`, `visitor_address`,
                        `visitor_purpose`, `visitor_temp`, `visitor_datetimein`, DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") AS 'Date' FROM `tbl_logbook` WHERE DATE_FORMAT(`visitor_datetimein`, ""%M %Y"") = """ & exportDate2 & """ ORDER BY `" & orderBy & "` " & arrangeBy, "ExportTbl")
            ElseIf exportDate1 = 4 Then 'yearly
                AdapterDBQuery("SELECT `visitor_name`, `visitor_program`, `visitor_contact`, `visitor_address`,
                        `visitor_purpose`, `visitor_temp`, `visitor_datetimein`, DATE_FORMAT(`visitor_datetimein`, ""%m/%d/%Y"") AS 'Date' FROM `tbl_logbook` WHERE DATE_FORMAT(`visitor_datetimein`, ""%Y"") = """ & exportDate2 & """ ORDER BY `" & orderBy & "` " & arrangeBy, "ExportTbl")
            End If

            'table.SetEdge(0, 0, 8, 2, Edge.Box, BorderStyle.Single, 1.5, Colors.Black)

            For rowIndex = 0 To dtSet.Tables("ExportTbl").Rows.Count - 1
                row = table.AddRow()
                row.KeepWith = False
                row.Format.Font.Size = 8
                'row.Format.KeepWithNext = False
                'row.Format.WidowControl = False
                row.Height = 20
                row.VerticalAlignment = VerticalAlignment.Center

                cell = row.Cells(0)
                cell.AddParagraph((rowIndex + 1).ToString)

                For columnIndex = 0 To dtSet.Tables("ExportTbl").Columns.Count - 1
                    cell = row.Cells(columnIndex + 1)

                    If columnIndex = dtSet.Tables("ExportTbl").Columns.Count - 1 And exportDate1 = 1 Then
                        cell.AddParagraph(Convert.ToDateTime(dtSet.Tables("ExportTbl").Rows(rowIndex).Item(columnIndex)).ToString("h:mm:ss tt"))
                    ElseIf columnIndex = dtSet.Tables("ExportTbl").Columns.Count - 2 And exportDate1 = 1 Then
                        cell.AddParagraph(dtSet.Tables("ExportTbl").Rows(rowIndex).Item(columnIndex).ToString & "°C")
                        cell.Format.Alignment = ParagraphAlignment.Center
                        If BunifuCheckBox2.Checked And Double.Parse(dtSet.Tables("ExportTbl").Rows(rowIndex).Item(columnIndex).ToString) > My.Settings.tempFeverVal Then row.Shading.Color = Color.FromRgb(255, 200, 200)
                    ElseIf columnIndex = dtSet.Tables("ExportTbl").Columns.Count - 2 And exportDate1 <> 1 Then
                        cell.AddParagraph(Convert.ToDateTime(dtSet.Tables("ExportTbl").Rows(rowIndex).Item(columnIndex)).ToString("h:mm:ss tt"))
                    ElseIf columnIndex = dtSet.Tables("ExportTbl").Columns.Count - 3 And exportDate1 <> 1 Then
                        cell.AddParagraph(dtSet.Tables("ExportTbl").Rows(rowIndex).Item(columnIndex).ToString & "°C")
                        cell.Format.Alignment = ParagraphAlignment.Center
                        If BunifuCheckBox2.Checked And Double.Parse(dtSet.Tables("ExportTbl").Rows(rowIndex).Item(columnIndex).ToString) > My.Settings.tempFeverVal Then row.Shading.Color = Color.FromRgb(255, 200, 200)
                    Else
                        cell.AddParagraph(Regex.Replace(dtSet.Tables("ExportTbl").Rows(rowIndex).Item(columnIndex).ToString, ".{15}", "$0 "))
                    End If
                Next
            Next
        Else
            dtSet.Tables("ExportTbl").Clear()
        End If

        doc.LastSection.Add(table)
        doc.LastSection.AddParagraph(vbNewLine)
        doc.LastSection.AddParagraph("- - - - - - - - - - - - -  END OF DATA  - - - - - - - - - - - - -")

        '---------END----------------------------------
        Return doc
    End Function

    Private Sub BackgroundWorker_Saving_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker_Saving.DoWork
        Try
            SaveAsDocument()
        Catch ex As Exception
            e.Cancel = True
        End Try
    End Sub

    Private Sub BackgroundWorker_Saving_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundWorker_Saving.RunWorkerCompleted
        If e.Cancelled Then
            BunifuButton_ExportSave.Text = "Try Again"
            BunifuButton_ExportSave.IconLeftPadding = New Padding(37, 3, 3, 3)
        Else
            BunifuButton_ExportSave.Text = "Saved"
            BunifuButton_ExportSave.IconLeftPadding = New Padding(47, 3, 3, 3)
        End If
        BunifuPanel2_Loading.Visible = False
    End Sub

    Private Sub BunifuTextBox_PageNum_KeyPress(sender As Object, e As KeyPressEventArgs) Handles BunifuTextBox_PageNum.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            If BunifuTextBox_PageNum.Text.ToString.Trim = "" Then
                Return
            End If
            If pageIsFocused And Integer.Parse(BunifuTextBox_PageNum.Text.ToString) <> pageTemp Then
                If Integer.Parse(BunifuTextBox_PageNum.Text.ToString) <= 1 Then
                    scrollVal = 0
                    BunifuTextBox_PageNum.Text = "1"
                ElseIf Integer.Parse(BunifuTextBox_PageNum.Text.ToString) >= Math.Ceiling(dtSet.Tables("dataTblCount").Rows(0).Item(0) / maxVal) Then
                    scrollVal = dtSet.Tables("dataTblCount").Rows(0).Item(0) - (dtSet.Tables("dataTblCount").Rows(0).Item(0) Mod maxVal)
                    BunifuTextBox_PageNum.Text = Math.Ceiling(dtSet.Tables("dataTblCount").Rows(0).Item(0) / maxVal)
                Else
                    scrollVal = maxVal * (Integer.Parse(BunifuTextBox_PageNum.Text.ToString) - 1)
                    BunifuTextBox_PageNum.Text = maxVal * Integer.Parse(BunifuTextBox_PageNum.Text.ToString)
                End If
                pageIsFocused = False
                BunifuTextBox1.Focus()

                DataGridView1.ClientSize = New Size(DataGridView1.Width, 20)

                LayoutLoad()
            End If
        End If

        If InStr("1234567890", e.KeyChar) = 0 And Asc(e.KeyChar) <> 8 Then
            e.KeyChar = Chr(0)
            e.Handled = True
        End If
    End Sub

    Private Sub BunifuTextBox_PageNum_LostFocus(sender As Object, e As EventArgs) Handles BunifuTextBox_PageNum.LostFocus
        If Math.Ceiling(dtSet.Tables("dataTblCount").Rows(0).Item(0) / maxVal) < 1 Then
            BunifuTextBox_PageNum.Text = 1
        ElseIf Integer.Parse(BunifuTextBox_PageNum.Text.ToString) > Math.Ceiling(dtSet.Tables("dataTblCount").Rows(0).Item(0) / maxVal) Then
            BunifuTextBox_PageNum.Text = pageTemp.ToString
        End If
    End Sub

    Private Sub BunifuTextBox_PageNum_GotFocus(sender As Object, e As EventArgs) Handles BunifuTextBox_PageNum.GotFocus
        If Math.Ceiling(dtSet.Tables("dataTblCount").Rows(0).Item(0) / maxVal) < 1 Then
            pageTemp = 1
        Else
            pageTemp = Integer.Parse(BunifuTextBox_PageNum.Text.ToString)
        End If
        pageIsFocused = True
    End Sub

    Private Sub BunifuPanel1_MouseUp(sender As Object, e As MouseEventArgs) Handles BunifuPanel1.MouseUp
        If Me.Location.Y < 0 Then
            Me.Location = New Point(Me.Location.X, 0)
        End If
    End Sub

    Private Sub Form_DataView_Closing(sender As Object, e As FormClosingEventArgs) Handles Me.Closing
        If (e.CloseReason = CloseReason.UserClosing) Then
            e.Cancel = True
        End If
    End Sub
End Class