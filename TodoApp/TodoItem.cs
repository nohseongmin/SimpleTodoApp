using System;
using System.Drawing;
using System.Text;

namespace TodoApp
{
    public class TodoItem
    {
        public string Text { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public DateTime DueDate { get; set; }
        public int IndentLevel { get; set; } // This can be used for visual padding

        public TodoItem()
        {
            DueDate = DateTime.Now;
        }

        // For saving to file
        public string ToFileString()
        {
            var builder = new StringBuilder();
            builder.Append(new string('	', IndentLevel));
            builder.Append(IsComplete ? "[x]" : "[ ]");
            builder.Append(" ");
            builder.Append(DueDate.ToString("MMdd"));
            builder.Append(" ");
            builder.Append(Text);
            return builder.ToString();
        }

        // For displaying in the ListBox
        public override string ToString()
        {
            return $"{DueDate:MMdd} {Text}";
        }
    }
}