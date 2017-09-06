Public Class FrmProperties

    Protected m_PCB As PCB
    Protected m_Window As FrmLayer

    'Private Sub PropertyGrid1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PropertyGrid.Click

    'End Sub

    Private Sub PropertyGrid_PropertyValueChanged(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs) Handles PropertyGrid.PropertyValueChanged
        m_Window.UpdateGraphics()
        'Dim Grid As PropertyGrid = CType(s, PropertyGrid)
        'Dim Name As String = e.ChangedItem.PropertyDescriptor.Name

        'If Grid.SelectedObject IsNot Nothing Then
        '    CType(Grid.SelectedObject, LayerObject).PropertyGridItemChanged(Name, e.OldValue, e.ChangedItem.Value)
        'ElseIf IsArray(Grid.SelectedObjects) Then
        '    For Each obj As Object In Grid.SelectedObjects
        '        CType(obj, LayerObject).PropertyGridItemChanged(Name, e.OldValue, e.ChangedItem.Value)
        '    Next
        'End If
    End Sub

    Public Sub New(ByVal PCB As PCB, ByVal Window As FrmLayer)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        m_PCB = PCB
        m_Window = Window
    End Sub
End Class