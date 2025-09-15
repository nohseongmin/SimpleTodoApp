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
            this.todoListBox.MouseDown += new MouseEventHandler(this.todoListBox_MouseDown);
            // 저장 경로 옵션: AppData(기본), ProgramFiles(권한 필요)
            _todoManager = new TodoManager(TodoManager.StorageLocation.AppData);
            _todoManager.LoadItems();
            _todoManager.SortItemsByDate();
            PopulateList();
        }
        catch (Exception ex)
        {
            MessageBox.Show("An unexpected error occurred on startup: " + ex.Message, "Startup Error");
            Application.Exit();
        }
    }

    // 날짜 부분 클릭 시 DatePicker
    private void todoListBox_MouseDown(object? sender, MouseEventArgs e)
    {
        int idx = todoListBox.IndexFromPoint(e.Location);
        if (idx < 0 || _todoManager == null || idx >= _todoManager.Items.Count) return;
        var item = _todoManager.Items[idx];
        int indent = item.IndentLevel * 24;
        // 이모지(32px) + 공백(8px) 이후 날짜(40px) 영역 클릭 시
        int dateStart = indent + 32;
        int dateEnd = dateStart + 40;
        if (e.X >= dateStart && e.X <= dateEnd)
        {
            using (var editForm = new EditTodoForm())
            {
                editForm.SelectedDate = item.DueDate;
                editForm.TodoText = item.Text;
                var dialogResult = editForm.ShowDialog(this);
                if (dialogResult == DialogResult.OK)
                {
                    item.DueDate = editForm.SelectedDate;
                    item.Text = editForm.TodoText;
                    _todoManager.SortItemsByDate();
                    PopulateList();
                }
                else if (dialogResult == DialogResult.Abort)
                {
                    _todoManager.Items.RemoveAt(idx);
                    PopulateList();
                }
            }
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
        Font font = (item.IsComplete) ? _strikethroughFont! : _defaultFont;
        string emoji = item.IsComplete ? "✅" : "🟥";
        int indent = item.IndentLevel * 24; // 24px per indent
        e.DrawBackground();
        // 이모지
        TextRenderer.DrawText(e.Graphics, emoji, new Font("Segoe UI Emoji", 16F), new System.Drawing.Point(e.Bounds.Left + indent, e.Bounds.Top + 2), this.ForeColor);
        // 텍스트(날짜+내용)
        string text = $" {item.DueDate:MMdd} {item.Text}";
        TextRenderer.DrawText(e.Graphics, text, font, new System.Drawing.Point(e.Bounds.Left + indent + 32, e.Bounds.Top + 4), this.ForeColor);
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
        if (_todoManager is null) return;
        // 들여쓰기(Tab), 아웃덴트(Shift+Tab)
        if ((e.KeyCode == Keys.Tab) && todoListBox.SelectedIndex >= 0)
        {
            int maxIndent = 3;
            foreach (int idx in todoListBox.SelectedIndices)
            {
                var item = _todoManager.Items[idx];
                if (e.Shift)
                {
                    if (item.IndentLevel > 0) item.IndentLevel--;
                }
                else
                {
                    if (item.IndentLevel < maxIndent) item.IndentLevel++;
                }
            }
            int[] selected = new int[todoListBox.SelectedIndices.Count];
            todoListBox.SelectedIndices.CopyTo(selected, 0);
            PopulateList();
            // 선택 유지
            foreach (int idx in selected)
                todoListBox.SetSelected(idx, true);
            e.Handled = true;
        }
        // 삭제
        else if (e.KeyCode == Keys.Delete && todoListBox.SelectedIndex >= 0)
        {
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