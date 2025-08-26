using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace TodoApp
{
    public partial class EditTodoForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime SelectedDate
        {
            get { return datePicker.Value; }
            set { datePicker.Value = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TodoText
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }

        public EditTodoForm()
        {
            InitializeComponent();
        }
    }
}
