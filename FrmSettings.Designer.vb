<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmSettings
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
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel
        Me.OK_Button = New System.Windows.Forms.Button
        Me.Cancel_Button = New System.Windows.Forms.Button
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.LstLibraries = New System.Windows.Forms.ListBox
        Me.CmdAddLibrary = New System.Windows.Forms.Button
        Me.CmdDel = New System.Windows.Forms.Button
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.ChAutoSave = New System.Windows.Forms.CheckBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.TxtAutoSaveInterval = New System.Windows.Forms.NumericUpDown
        Me.OpenFileDialog = New System.Windows.Forms.OpenFileDialog
        Me.TableLayoutPanel1.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        CType(Me.TxtAutoSaveInterval, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.OK_Button, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Cancel_Button, 1, 0)
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(277, 274)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 1
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(146, 29)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'OK_Button
        '
        Me.OK_Button.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.OK_Button.Location = New System.Drawing.Point(3, 3)
        Me.OK_Button.Name = "OK_Button"
        Me.OK_Button.Size = New System.Drawing.Size(67, 23)
        Me.OK_Button.TabIndex = 0
        Me.OK_Button.Text = "OK"
        '
        'Cancel_Button
        '
        Me.Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel_Button.Location = New System.Drawing.Point(76, 3)
        Me.Cancel_Button.Name = "Cancel_Button"
        Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
        Me.Cancel_Button.TabIndex = 1
        Me.Cancel_Button.Text = "Cancel"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.CmdDel)
        Me.GroupBox1.Controls.Add(Me.CmdAddLibrary)
        Me.GroupBox1.Controls.Add(Me.LstLibraries)
        Me.GroupBox1.Location = New System.Drawing.Point(17, 55)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(402, 204)
        Me.GroupBox1.TabIndex = 1
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Default libraries"
        '
        'LstLibraries
        '
        Me.LstLibraries.FormattingEnabled = True
        Me.LstLibraries.Location = New System.Drawing.Point(11, 21)
        Me.LstLibraries.Name = "LstLibraries"
        Me.LstLibraries.Size = New System.Drawing.Size(325, 173)
        Me.LstLibraries.TabIndex = 0
        '
        'CmdAddLibrary
        '
        Me.CmdAddLibrary.Location = New System.Drawing.Point(342, 21)
        Me.CmdAddLibrary.Name = "CmdAddLibrary"
        Me.CmdAddLibrary.Size = New System.Drawing.Size(54, 25)
        Me.CmdAddLibrary.TabIndex = 1
        Me.CmdAddLibrary.Text = "Add"
        Me.ToolTip1.SetToolTip(Me.CmdAddLibrary, "Add new default library")
        Me.CmdAddLibrary.UseVisualStyleBackColor = True
        '
        'CmdDel
        '
        Me.CmdDel.Location = New System.Drawing.Point(342, 52)
        Me.CmdDel.Name = "CmdDel"
        Me.CmdDel.Size = New System.Drawing.Size(53, 25)
        Me.CmdDel.TabIndex = 2
        Me.CmdDel.Text = "Del"
        Me.ToolTip1.SetToolTip(Me.CmdDel, "Remove default library")
        Me.CmdDel.UseVisualStyleBackColor = True
        '
        'ChAutoSave
        '
        Me.ChAutoSave.AutoSize = True
        Me.ChAutoSave.Location = New System.Drawing.Point(17, 12)
        Me.ChAutoSave.Name = "ChAutoSave"
        Me.ChAutoSave.Size = New System.Drawing.Size(95, 17)
        Me.ChAutoSave.TabIndex = 2
        Me.ChAutoSave.Text = "Use auto save"
        Me.ChAutoSave.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(17, 32)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(95, 13)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "Auto save interval:"
        '
        'TxtAutoSaveInterval
        '
        Me.TxtAutoSaveInterval.Location = New System.Drawing.Point(118, 30)
        Me.TxtAutoSaveInterval.Name = "TxtAutoSaveInterval"
        Me.TxtAutoSaveInterval.Size = New System.Drawing.Size(73, 20)
        Me.TxtAutoSaveInterval.TabIndex = 5
        '
        'OpenFileDialog
        '
        Me.OpenFileDialog.Filter = "Eagle libraries (*.lbr)|*.lbr|All files (*.*)|*.*"
        '
        'FrmSettings
        '
        Me.AcceptButton = Me.OK_Button
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Cancel_Button
        Me.ClientSize = New System.Drawing.Size(435, 315)
        Me.Controls.Add(Me.TxtAutoSaveInterval)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.ChAutoSave)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmSettings"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Settings"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        CType(Me.TxtAutoSaveInterval, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents CmdDel As System.Windows.Forms.Button
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents CmdAddLibrary As System.Windows.Forms.Button
    Friend WithEvents LstLibraries As System.Windows.Forms.ListBox
    Friend WithEvents ChAutoSave As System.Windows.Forms.CheckBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents TxtAutoSaveInterval As System.Windows.Forms.NumericUpDown
    Friend WithEvents OpenFileDialog As System.Windows.Forms.OpenFileDialog

End Class
