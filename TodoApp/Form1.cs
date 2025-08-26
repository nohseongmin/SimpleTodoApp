using System.Drawing;
using System.IO;
using System.Windows.Forms; // Added for MethodInvoker

namespace TodoApp;

public partial class Form1 : Form
{
    private readonly TodoManager? _todoManager;
    private readonly Font _defaultFont = new Font("Segoe UI", 12F);
    private readonly Font? _strikethroughFont;

    public Form1()
    {
        try
        {
            InitializeComponent();
            
            _strikethroughFont = new Font(_defaultFont, FontStyle.Strikeout);
            this.FormClosing += Form1_FormClosing;
            this.todoListBox.MouseDoubleClick += new MouseEventHandler(this.todoListBox_MouseDoubleClick);

            _todoManager = new TodoManager();
            _todoManager.LoadItems();
            _todoManager.SortItemsByDate(); // Call Sort here
            PopulateList();
        }
        catch (Exception ex)
        {
            MessageBox.Show("An unexpected error occurred on startup: " + ex.Message, "Startup Error");
            Application.Exit();
        }
    }

    private void PopulateList()
    {
        todoListBox.Items.Clear();
        if (_todoManager is null) return; // Added null check
        foreach (var item in _todoManager.Items)
        {
            todoListBox.Items.Add(item, item.IsComplete);
        }
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _todoManager?.SaveItems(); // Use null-conditional operator
    }

    private void todoListBox_DrawItem(object sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        var item = (TodoItem)todoListBox.Items[e.Index];
        Font font = (item.IsComplete) ? _strikethroughFont! : _defaultFont; // Use null-forgiving operator
        
        // Prevent selection color from obscuring the checkbox
        e.DrawBackground();
        TextRenderer.DrawText(e.Graphics, item.ToString(), font, e.Bounds, this.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
        {
            e.DrawFocusRectangle();
        }
    }

    private void todoListBox_ItemCheck(object sender, ItemCheckEventArgs e)
    {
        if (e.Index < 0 || _todoManager is null || e.Index >= _todoManager.Items.Count) return; // Added null check

        var item = _todoManager.Items[e.Index];
        item.IsComplete = (e.NewValue == CheckState.Checked);

        // Defer the deselection to prevent the blue highlight from sticking
        this.BeginInvoke((MethodInvoker)delegate 
        {
            todoListBox.ClearSelected();
        });

        todoListBox.Invalidate();
    }

    private void addButton_Click(object sender, EventArgs e)
    {
        string text = newItemTextBox.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;
        if (_todoManager is null) return; // Added null check

        var newItem = new TodoItem { Text = text };
        _todoManager.Items.Add(newItem);
        _todoManager.SortItemsByDate(); // Call Sort here
        
        newItemTextBox.Clear();
        PopulateList();
        todoListBox.TopIndex = todoListBox.Items.Count - 1;
    }

    private void todoListBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete && todoListBox.SelectedIndex >= 0)
        {
            if (_todoManager is null) return; // Added null check
            // To allow deleting multiple selected items
            for (int i = todoListBox.SelectedIndices.Count - 1; i >= 0; i--)
            {
                _todoManager.Items.RemoveAt(todoListBox.SelectedIndices[i]);
            }
            PopulateList();
        }
    }

    private void todoListBox_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        int index = this.todoListBox.IndexFromPoint(e.Location);
        if (index != System.Windows.Forms.ListBox.NoMatches)
        {
            if (_todoManager is null) return; // Added null check
            var item = _todoManager.Items[index];
            using (var editForm = new EditTodoForm())
            {
                editForm.SelectedDate = item.DueDate;
                editForm.TodoText = item.Text;
                
                var dialogResult = editForm.ShowDialog(this);

                if (dialogResult == DialogResult.OK)
                {
                    item.DueDate = editForm.SelectedDate;
                    item.Text = editForm.TodoText;
                    _todoManager.SortItemsByDate(); // Call Sort here
                    PopulateList();
                }
                else if (dialogResult == DialogResult.Abort) // We set Abort for the Remove button
                {
                    _todoManager.Items.RemoveAt(index);
                    PopulateList();
                }
            }
        }
    }
}