using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TodoApp
{
    public class TodoManager
    {
        private readonly string _filePath;
        public List<TodoItem> Items { get; private set; }

        public TodoManager()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appDirectory = Path.Combine(appDataPath, "TodoApp");
            Directory.CreateDirectory(appDirectory);
            _filePath = Path.Combine(appDirectory, "list.md");
            Items = new List<TodoItem>();
        }

        public void LoadItems()
        {
            Items.Clear();
            if (!File.Exists(_filePath)) return;

            try
            {
                var lines = File.ReadAllLines(_filePath);
                foreach (var line in lines)
                {
                    if (TryParseLine(line, out var item))
                    {
                        Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading todo list: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SaveItems()
        {
            try
            {
                var lines = Items.Select(item => item.ToFileString()).ToArray();
                File.WriteAllLines(_filePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving todo list: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SortItemsByDate()
        {
            Items = Items.OrderBy(item => item.DueDate).ToList();
        }

        private bool TryParseLine(string line, out TodoItem item)
        {
            item = new TodoItem();
            if (string.IsNullOrWhiteSpace(line)) return false;

            var trimmedLine = line.TrimStart('\t');
            item.IndentLevel = line.Length - trimmedLine.Length;

            var match = Regex.Match(trimmedLine, @"^\s*\[( |x|X)\]\s+(\d{{4}})\s+(.*)");
            if (match.Success)
            {
                item.IsComplete = match.Groups[1].Value.Equals("x", StringComparison.OrdinalIgnoreCase);
                
                string dateStr = match.Groups[2].Value;
                if (DateTime.TryParseExact($"{dateStr}/{{DateTime.Now.Year}}", "MMdd/yyyy", null, System.Globalization.DateTimeStyles.None, out var date))
                {
                    item.DueDate = date;
                }
                else
                {
                    item.DueDate = DateTime.Now;
                }
                
                item.Text = match.Groups[3].Value;
                return true;
            }
            return false; // Ignore lines that are not valid todo items
        }
    }
}