using System;
using System.Windows.Forms;

namespace TodoApp
{
    public class DateTimePickerForm : Form
    {
        public DateTimePicker Picker { get; private set; }
        public DateTime SelectedDate => Picker.Value;

        public DateTimePickerForm()
        {
            this.Text = "날짜 선택";
            this.Width = 250;
            this.Height = 120;
            Picker = new DateTimePicker();
            Picker.Format = DateTimePickerFormat.Custom;
            Picker.CustomFormat = "MMdd";
            Picker.Dock = DockStyle.Top;
            var ok = new Button { Text = "확인", Dock = DockStyle.Bottom, DialogResult = DialogResult.OK };
            this.Controls.Add(Picker);
            this.Controls.Add(ok);
            this.AcceptButton = ok;
        }
    }
}