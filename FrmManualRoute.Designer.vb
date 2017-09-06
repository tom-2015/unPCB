<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmManualRoute
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
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmManualRoute))
        Me.Label1 = New System.Windows.Forms.Label
        Me.CmdConnected = New System.Windows.Forms.Button
        Me.CmdNotConnected = New System.Windows.Forms.Button
        Me.CmdSkip = New System.Windows.Forms.Button
        Me.PgbCompleted = New System.Windows.Forms.ProgressBar
        Me.CmdNext = New System.Windows.Forms.Button
        Me.ToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.CmdNotConnectedToAll = New System.Windows.Forms.Button
        Me.lblCurrentPads = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(0, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(780, 29)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Please check if the highlighted pads are connected and click the buttons below"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'CmdConnected
        '
        Me.CmdConnected.BackColor = System.Drawing.Color.Lime
        Me.CmdConnected.Location = New System.Drawing.Point(28, 81)
        Me.CmdConnected.Name = "CmdConnected"
        Me.CmdConnected.Size = New System.Drawing.Size(133, 37)
        Me.CmdConnected.TabIndex = 1
        Me.CmdConnected.Text = "Connected"
        Me.ToolTip.SetToolTip(Me.CmdConnected, "Click this button if the 2 pads are connected.")
        Me.CmdConnected.UseVisualStyleBackColor = False
        '
        'CmdNotConnected
        '
        Me.CmdNotConnected.BackColor = System.Drawing.Color.Red
        Me.CmdNotConnected.Location = New System.Drawing.Point(179, 81)
        Me.CmdNotConnected.Name = "CmdNotConnected"
        Me.CmdNotConnected.Size = New System.Drawing.Size(133, 37)
        Me.CmdNotConnected.TabIndex = 2
        Me.CmdNotConnected.Text = "Not Connected"
        Me.ToolTip.SetToolTip(Me.CmdNotConnected, "Click this button if the 2 selected pads are not connected.")
        Me.CmdNotConnected.UseVisualStyleBackColor = False
        '
        'CmdSkip
        '
        Me.CmdSkip.Location = New System.Drawing.Point(481, 80)
        Me.CmdSkip.Name = "CmdSkip"
        Me.CmdSkip.Size = New System.Drawing.Size(133, 37)
        Me.CmdSkip.TabIndex = 3
        Me.CmdSkip.Text = "Skip"
        Me.ToolTip.SetToolTip(Me.CmdSkip, "Click to skip and go to next pad")
        Me.CmdSkip.UseVisualStyleBackColor = True
        '
        'PgbCompleted
        '
        Me.PgbCompleted.Location = New System.Drawing.Point(28, 32)
        Me.PgbCompleted.Name = "PgbCompleted"
        Me.PgbCompleted.Size = New System.Drawing.Size(737, 23)
        Me.PgbCompleted.TabIndex = 4
        '
        'CmdNext
        '
        Me.CmdNext.Location = New System.Drawing.Point(632, 80)
        Me.CmdNext.Name = "CmdNext"
        Me.CmdNext.Size = New System.Drawing.Size(133, 37)
        Me.CmdNext.TabIndex = 5
        Me.CmdNext.Text = "Search"
        Me.ToolTip.SetToolTip(Me.CmdNext, "Click to search for next unconnected pads")
        Me.CmdNext.UseVisualStyleBackColor = True
        '
        'CmdNotConnectedToAll
        '
        Me.CmdNotConnectedToAll.BackColor = System.Drawing.Color.Red
        Me.CmdNotConnectedToAll.ForeColor = System.Drawing.SystemColors.ControlText
        Me.CmdNotConnectedToAll.Location = New System.Drawing.Point(330, 80)
        Me.CmdNotConnectedToAll.Name = "CmdNotConnectedToAll"
        Me.CmdNotConnectedToAll.Size = New System.Drawing.Size(133, 37)
        Me.CmdNotConnectedToAll.TabIndex = 6
        Me.CmdNotConnectedToAll.Text = "Not connected to others"
        Me.ToolTip.SetToolTip(Me.CmdNotConnectedToAll, "Click this button if the main pad is not connected to any other pads.")
        Me.CmdNotConnectedToAll.UseVisualStyleBackColor = False
        '
        'lblCurrentPads
        '
        Me.lblCurrentPads.AutoSize = True
        Me.lblCurrentPads.Location = New System.Drawing.Point(28, 59)
        Me.lblCurrentPads.Name = "lblCurrentPads"
        Me.lblCurrentPads.Size = New System.Drawing.Size(13, 13)
        Me.lblCurrentPads.TabIndex = 7
        Me.lblCurrentPads.Text = "  "
        '
        'FrmAutoRoute
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(780, 129)
        Me.Controls.Add(Me.lblCurrentPads)
        Me.Controls.Add(Me.CmdNotConnectedToAll)
        Me.Controls.Add(Me.CmdNext)
        Me.Controls.Add(Me.PgbCompleted)
        Me.Controls.Add(Me.CmdSkip)
        Me.Controls.Add(Me.CmdNotConnected)
        Me.Controls.Add(Me.CmdConnected)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "FrmAutoRoute"
        Me.Text = "Manual Router"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents CmdConnected As System.Windows.Forms.Button
    Friend WithEvents CmdNotConnected As System.Windows.Forms.Button
    Friend WithEvents CmdSkip As System.Windows.Forms.Button
    Friend WithEvents PgbCompleted As System.Windows.Forms.ProgressBar
    Friend WithEvents CmdNext As System.Windows.Forms.Button
    Friend WithEvents ToolTip As System.Windows.Forms.ToolTip
    Friend WithEvents CmdNotConnectedToAll As System.Windows.Forms.Button
    Friend WithEvents lblCurrentPads As System.Windows.Forms.Label
End Class
