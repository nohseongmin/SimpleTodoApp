using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TodoApp
{
    public class RichTextBoxEx : RichTextBox
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const string RichEdit50W = "msftedit.dll";
        private const int EM_SETTEXTMODE = 0x0459;
        private const int TM_ADVANCEDTYPOGRAPHY = 0x0001 | 0x0008 | 0x0040 | 0x0200 | 0x0800;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                try
                {
                    IntPtr lib = LoadLibrary(RichEdit50W);
                    if (lib != IntPtr.Zero)
                    {
                        cp.ClassName = "RICHEDIT50W";
                    }
                }
                catch
                {
                    // Fallback to default
                }
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try
            {
                SendMessage(this.Handle, EM_SETTEXTMODE, (IntPtr)TM_ADVANCEDTYPOGRAPHY, IntPtr.Zero);
            }
            catch
            {
                // Handle exceptions
            }
        }
    }
}
