using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace System.Windows
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();
            if (Application.Current != null)
            {
                if (Application.Current.MainWindow != null)
                    this.Title = Application.Current.MainWindow.Title;
                else
                    this.Title = Application.Current.ToString();
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public static string Show(string question, string defaultAnswer)
        {
            var w = new InputBox();
            w.QuestionLabel.Content = question;
            w.AnswerTextBox.Text = defaultAnswer;
            if (w.ShowDialog() == true)
                return w.AnswerTextBox.Text ?? "";
            else
                return null;
        }
    }
}
