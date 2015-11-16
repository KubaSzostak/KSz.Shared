using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Windows.Forms
{

    public static class BindingEx
    {

        // Inspired by Ian Ringrose
        // http://stackoverflow.com/users/57159/ian-ringrose
        // http://stackoverflow.com/questions/1329138/how-to-make-databinding-type-safe-and-support-refactoring


        private static string GetPropertyMemberName(System.Linq.Expressions.LambdaExpression propertyExpresssion)
        {
            if (propertyExpresssion == null)
                throw new ArgumentNullException("Property access expression cannot be null.");

            var memberExpression = propertyExpresssion.Body as MemberExpression;
            if (memberExpression == null)
            {
                var unaryExpression = propertyExpresssion.Body as UnaryExpression;
                if (unaryExpression != null)
                {
                    memberExpression = unaryExpression.Operand as MemberExpression;
                }
            }
            if ((memberExpression != null) && (memberExpression.Member.MemberType == Reflection.MemberTypes.Property))
            {
                return memberExpression.Member.Name;
            }
            throw new ArgumentException("The expression is not a property access expression: " + propertyExpresssion.ToString());
        }

        public static string GetMemberName<T, T2>(Expression<Func<T, T2>> expression)
        {
            return GetPropertyMemberName(expression);
        }

        public static string GetMemberName<T>(Expression<Func<T>> expression)
        {
            return GetPropertyMemberName(expression);
        }

        // --- Core Bind() function -------------------------------

        public static Binding BindProperty<TControl, TData, TControlMember, TDataMember>(this TControl control, Expression<Func<TControl, TControlMember>> controlProperty, TData dataSource, Expression<Func<TData, TDataMember>> dataMember)
            where TControl : Control
        {
            return control.DataBindings.Add(GetMemberName(controlProperty), dataSource, GetMemberName(dataMember));
        }

        // --- Enabled, Visible, Color ----------------------------

        public static Binding BindEnabled<TControl, TData>(this TControl control, TData dataSource, Expression<Func<TData, bool>> dataMember)
            where TControl : Control
        {
            return control.BindProperty(c => c.Enabled, dataSource, dataMember);
        }

        public static Binding BindVisible<TControl, TData>(this TControl control, TData dataSource, Expression<Func<TData, bool>> dataMember)
            where TControl : Control
        {
            return control.BindProperty(c => c.Visible, dataSource, dataMember);
        }

        public static Binding BindColor<TControl, TData>(this TControl control, TData dataSource, Expression<Func<TData, Drawing.Color>> dataMember)
            where TControl : Control
        {
            return control.BindProperty(c => c.ForeColor, dataSource, dataMember);
        }

        // --- Specific Controls --------------------------------------

        public static Binding Bind<TD>(this Label control, TD dataSource, Expression<Func<TD, object>> dataMember)
        {
            return control.BindProperty(c => c.Text, dataSource, dataMember);
        }

        public static Binding Bind<TD>(this TextBoxBase control, TD dataSource, Expression<Func<TD, object>> dataMember)
        {
            var b = control.BindProperty(c => c.Text, dataSource, dataMember);
            b.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
            return b;
        }

        public static Binding Bind<TD>(this CheckBox control, TD dataSource, Expression<Func<TD, bool>> dataMember)
        {
            var b = control.BindProperty(c => c.Checked, dataSource, dataMember);
            b.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
            return b;
        }

        public static void Bind(this Button control, Input.ICommand command)
        {
            command.CanExecuteChanged += (s, e) =>
            {
                control.Enabled = command.CanExecute(null);
            };
            control.Click += (s, e) =>
            {
                command.Execute(null);
            };
        }

        public static Binding Bind<TD, TI>(this ComboBox control, TD dataSource, Expression<Func<TD, TI>> selectedItemMember, IEnumerable<TI> items)
        {
            control.DataSource = items; // Items first, then binding.
            control.SelectedIndexChanged += (s, e) => {
                control.WriteBindings();
            };
            var b = control.BindProperty(c => c.SelectedItem, dataSource, selectedItemMember);
            b.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
            b.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;

            if ((control.SelectedIndex < 0) && (control.Items.Count > 0))
                control.SelectedIndex = 0;

            control.WriteBindings(); //  Update dataSource property after selecting first item

            // Above does not work, so:
            var propInfo = dataSource.GetType().GetProperty(GetPropertyMemberName(selectedItemMember));
            var propVal = propInfo.GetValue(dataSource, null);
            if (propVal == null)
            {
                propInfo.SetValue(dataSource, control.SelectedItem, null);
            }
            else
            {
                control.SelectedItem = propVal;
            }

            return b;
        }


    }
}
