using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TodoApp;

public partial class Form1 : Form
{
    private readonly TodoManager? _todoManager;
    private Font _defaultFont = new Font("Segoe UI", 12F);
    private Font? _strikethroughFont;

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    internal static extern void DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, ref int pvAttribute, uint cbAttribute);

    public enum DWMWINDOWATTRIBUTE : uint
    {
        DWMWA_CAPTION_COLOR = 35
    }

    public Form1()
    {
        try
        {
            InitializeComponent();
            _strikethroughFont = new Font(_defaultFont, FontStyle.Strikeout);
            this.FormClosing += Form1_FormClosing;
            this.todoListBox.MouseDoubleClick += new MouseEventHandler(this.todoListBox_MouseDoubleClick);
            this.todoListBox.MouseDown += new MouseEventHandler(this.todoListBox_MouseDown);
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

    private void todoListBox_MouseDown(object? sender, MouseEventArgs e)
    {
        int idx = todoListBox.IndexFromPoint(e.Location);
        if (idx < 0 || _todoManager == null || idx >= _todoManager.Items.Count) return;
        var item = _todoManager.Items[idx];
        int indent = item.IndentLevel * 24;
        int emojiEnd = indent + 32;

        if (e.X <= emojiEnd)
        {
            item.IsComplete = !item.IsComplete;
            todoListBox.Invalidate();
        }
        else
        {
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
    }

    private void PopulateList()
    {
        todoListBox.Items.Clear();
        if (_todoManager is null) return;
        foreach (var item in _todoManager.Items)
        {
            todoListBox.Items.Add(item, item.IsComplete);
        }
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _todoManager?.SaveItems();
    }

    private void todoListBox_DrawItem(object sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        var item = (TodoItem)todoListBox.Items[e.Index];
        Font font = (item.IsComplete) ? _strikethroughFont! : _defaultFont;
        string emoji = item.IsComplete ? "✅" : "🟥";
        int indent = item.IndentLevel * 24;
        e.DrawBackground();
        TextRenderer.DrawText(e.Graphics, emoji, new Font("Segoe UI Emoji", 16F), new System.Drawing.Point(e.Bounds.Left + indent, e.Bounds.Top + 2), this.ForeColor);
        string text = $" {item.DueDate:MMdd} {item.Text}";
        TextRenderer.DrawText(e.Graphics, text, font, new System.Drawing.Point(e.Bounds.Left + indent + 32, e.Bounds.Top + 4), this.ForeColor);
        if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
        {
            e.DrawFocusRectangle();
        }
    }

    private void newItemTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            string text = newItemTextBox.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;
            if (_todoManager is null) return;

            var newItem = new TodoItem { Text = text };
            _todoManager.Items.Add(newItem);
            _todoManager.SortItemsByDate();
            
            newItemTextBox.Clear();
            PopulateList();
            todoListBox.TopIndex = todoListBox.Items.Count - 1;
            e.SuppressKeyPress = true;
        }
    }

    private void darkToolStripMenuItem_Click(object sender, EventArgs e)
    {
        SetTheme(Color.FromArgb(45, 45, 48), Color.White);
    }

    private void lightToolStripMenuItem_Click(object sender, EventArgs e)
    {
        SetTheme(Color.White, Color.Black);
    }

    private void yellowToolStripMenuItem_Click(object sender, EventArgs e)
    {
        SetTheme(Color.FromArgb(255, 253, 208), Color.Black);
    }

    private void greenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        SetTheme(Color.FromArgb(208, 255, 209), Color.Black);
    }

    private void redToolStripMenuItem_Click(object sender, EventArgs e)
    {
        SetTheme(Color.FromArgb(255, 208, 208), Color.Black);
    }

    private void SetTheme(Color backColor, Color foreColor)
    {
        this.BackColor = backColor;
        this.ForeColor = foreColor;
        this.todoListBox.BackColor = backColor;
        this.todoListBox.ForeColor = foreColor;
        this.newItemTextBox.BackColor = backColor;
        this.newItemTextBox.ForeColor = foreColor;
        this.titleLabel.ForeColor = foreColor;

        try
        {
            int color = backColor.R << 16 | backColor.G << 8 | backColor.B;
            DwmSetWindowAttribute(this.Handle, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_COLOR, ref color, sizeof(int));
        }
        catch { }
    }

    private void fontToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using (var fontDialog = new FontDialog())
        {
            fontDialog.Font = _defaultFont;
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                SetFont(fontDialog.Font);
            }
        }
    }

    private void SetFont(Font font)
    {
        _defaultFont = font;
        _strikethroughFont = new Font(_defaultFont, FontStyle.Strikeout);
        this.todoListBox.Font = _defaultFont;
        this.newItemTextBox.Font = _defaultFont;
        this.titleLabel.Font = new Font(font.FontFamily, 18F, FontStyle.Bold);
        PopulateList();
    }

    private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
    {
        this.TopMost = alwaysOnTopToolStripMenuItem.Checked;
    }

    private void todoListBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (_todoManager is null) return;
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
            foreach (int idx in selected)
                todoListBox.SetSelected(idx, true);
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Delete && todoListBox.SelectedIndex >= 0)
        {
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
            if (_todoManager is null) return;
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
                    _todoManager.SortItemsByDate();
                    PopulateList();
                }
                else if (dialogResult == DialogResult.Abort)
                {
                    _todoManager.Items.RemoveAt(index);
                    PopulateList();
                }
            }
        }
    }
}
