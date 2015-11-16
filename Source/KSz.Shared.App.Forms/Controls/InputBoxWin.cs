using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace System.Windows
{
    public partial class InputBoxWin : Form
    {
        public InputBoxWin()
        {
            InitializeComponent();
            btnCancel.Text = SysUtils.Strings.Cancel;
            this.Text = AppUI.ProductName;
        }

        private void btnOK2_Click(object sender, EventArgs e)
        {
            this.DialogResult = Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = Forms.DialogResult.Cancel;
        }

        public static string Show(string question, string defaultAnswer)
        {
            var w = new InputBoxWin();
            w.QuestionLabel.Text = question;
            w.AnswerTextBox.Text = defaultAnswer;
            if (w.ShowDialog() == DialogResult.OK)
                return w.AnswerTextBox.Text ?? "";
            else
                return null;
        }
    }
}
