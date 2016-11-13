using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace System
{



    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _beginUpdateStarted = false;

        protected void BeginUpdate()
        {
            _beginUpdateStarted = true;
        }

        protected void EndUpdate()
        {
            _beginUpdateStarted = false;
            OnPropertyChanged(allPropertiesArgs);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (_beginUpdateStarted)
                return;

            // http://stackoverflow.com/questions/2553333/wpf-databinding-thread-safety
            // In WPF .NET 3.5 INotifyPropertyChanged is thread safe

            VerifyProperty(args.PropertyName);

            // In multi-thread approach this.PropertyChanged can change its value between  [PropertyChanged == null] and [PropertyChanged(...)]
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            var handler = this.PropertyChanged;
            handler?.Invoke(this, args);

            //http://10rem.net/blog/2012/01/10/threading-considerations-for-binding-and-change-notification-in-silverlight-5
            //Deployment.Current.Dispatcher.CheckAccess()
        }

        public void OnPropertyChanged(string propertyName)
        {
            var propertyArgs = new PropertyChangedEventArgs(propertyName);
            OnPropertyChanged(propertyArgs);

            //  Always run OnPropertyChanged(string propertyName) -> DbRecord.IsChanged
            //if (PropertyChanged == null)
            //    return;  
        }

        protected bool OnPropertyChanged<T>(ref T propVal, T newVal, PropertyChangedEventArgs propArgs)
        {
            if (!object.Equals(propVal, newVal))
            {
                propVal = newVal;
                OnPropertyChanged(propArgs);
                return true;
            }
            return false;
        }

        protected bool OnPropertyChanged<T>(ref T propVal, T newVal, string propName)
        {
            if (!object.Equals(propVal, newVal))
            {
                propVal = newVal;
                OnPropertyChanged(propName);
                return true;
            }
            return false;
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private void VerifyProperty(string propertyName)
        {
            var type = this.GetType();
            if (type.GetProperty(propertyName) == null)
            {
                string msg = string.Format("'{0}' is not a public property of {1}", propertyName, type.FullName);
                Debug.WriteLine(msg);
                throw new Exception(msg);
            }
        }

        private static PropertyChangedEventArgs allPropertiesArgs = new PropertyChangedEventArgs(null);
        [Obsolete]
        public virtual void NotifyAllPropertiesChanged()
        {
            NotifyPropertyChanged(allPropertiesArgs);
        }

        [Obsolete]
        public virtual void NotifyPropertyChanged(PropertyChangedEventArgs args)
        {
            OnPropertyChanged(args);
        }

        [Obsolete]
        protected void NotifyPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        [Obsolete]
        protected bool NotifyPropertyChanged<T>(ref T propVal, T newVal, string propName)
        {
            return OnPropertyChanged(ref propVal, newVal, propName);
        }

        [Obsolete]
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propName = GetPropertyMemberName(propertyExpresssion);
            NotifyPropertyChanged(propName);
        }

        [Obsolete]
        public virtual bool NotifyPropertyChanged<T>(ref T propVal, T newVal, PropertyChangedEventArgs args)
        {
            return OnPropertyChanged<T>(ref propVal, newVal, args);
        }

        [Obsolete]
        protected bool NotifyPropertyChanged<T>(ref T propVal, T newVal, Expression<Func<T>> propertyExpresssion)
        {
            return NotifyPropertyChanged(ref propVal, newVal, GetPropertyMemberName(propertyExpresssion));
        }
        
        [Obsolete("Use nameof() keyword")]
        public void OnPropertyChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propName = GetPropertyMemberName(propertyExpresssion);
            OnPropertyChanged(propName);
        }

        [Obsolete]
        private static string GetPropertyMemberName(LambdaExpression propertyExpresssion)
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
            if ((memberExpression != null) ) //&& (memberExpression.Member.MemberType == MemberTypes.Property))
            {
                return memberExpression.Member.Name;
            }
            throw new ArgumentException("The expression is not a property access expression: " + propertyExpresssion.ToString());
        }

        [Obsolete]
        public static string GetPropertyName<T>(Expression<Func<T, object>> propertyExpresssion)
        {
            return GetPropertyMemberName(propertyExpresssion);
        }

        [Obsolete]
        protected string TypePropertyName<T>(Expression<Func<T>> propertyExpresssion)
        {
            return this.GetType().Name + "." + GetPropertyMemberName(propertyExpresssion);
        }

        /// <summary>
        /// Use it to ininitialize static PropertyChangedEventArgs. It is about 10 times faster than
        /// calling everytime OnPropertyChanged(() => MyProperty);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpresssion"></param>
        /// <returns></returns>
        [Obsolete]
        public static PropertyChangedEventArgs GetPropertyArgs<T>(Expression<Func<T, object>> propertyExpresssion)
        {
            return new PropertyChangedEventArgs(GetPropertyMemberName(propertyExpresssion));
        }
        

    }


    /* C# 5.0 + .NET 3.5

    namespace System.Runtime.CompilerServices
    {
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
        public sealed class CallerMemberNameAttribute : Attribute { }
    }
    


    public class Data : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    */





    public class EditableObject : ObservableObject, IEditableObject  //, IClientChangeTracking, INotifyDataErrorInfo, IDataErrorInfo
    {
        // AcceptChanges(), RejectChanges(), HasChanges and GetObjectGraphChanges() 
        protected Dictionary<string, object> PropsBackup = new Dictionary<string, object>();

        private IEnumerable<PropertyInfo> GetProperties()
        {
            return this.GetType().GetProperties().Where(p => p.IsEditableValue());
        }

        virtual public void BeginEdit()
        {
            PropsBackup.Clear();
            foreach (var pi in GetProperties())
            {
                PropsBackup[pi.Name] = pi.GetValue(this, null);
            }
        }

        virtual public void CancelEdit()
        {
            if (PropsBackup.Count < 1)
                throw new Exception(string.Format("{0}.BeginEdit() not invoked.", this.GetType().Name));
            foreach (var pi in GetProperties())
            {
                var thisVal = pi.GetValue(this, null);
                var bacVal = PropsBackup[pi.Name];

                if (!object.Equals(thisVal, bacVal))
                {
                    pi.SetValue(this, PropsBackup[pi.Name], null);
                    OnPropertyChanged(pi.Name);
                }
            }
            PropsBackup.Clear();
        }

        virtual public void EndEdit()
        {
            if (PropsBackup.Count < 1)
                throw new Exception(string.Format("{0}.BeginEdit() not invoked.", this.GetType().Name));
            // Nothing to do
            PropsBackup.Clear();
        }
    }


    public class Presenter : ObservableObject
    {

        public Presenter()
        {
            IsInDesignMode = Application.Current != null;
            //IsInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
            //Windows.ApplicationModel.DesignMode.DesignModeEnabled
        }

        /// <summary>
        /// <UserControl.Resources>
        ///   <ViewModels:MockXViewModel x:Key="DesignViewModel"/>
        /// </UserControl.Resources>
        /// <Grid DataContext="{Binding Source={StaticResource DesignViewModel}}" />
        /// </summary>
        // http://stackoverflow.com/questions/1889966/what-approaches-are-available-to-dummy-design-time-data-in-wpf
        public bool IsInDesignMode { get; private set; }
        public string DisplayName { get; set; }

        public virtual void RefreshData()
        {
        }
        
        private static PropertyChangedEventArgs ErrorMessageArgs = new PropertyChangedEventArgs(nameof(ErrorMessage)); 
        private static PropertyChangedEventArgs HasErrorArgs = new PropertyChangedEventArgs(nameof(HasError));
        private static PropertyChangedEventArgs HasNoErrorArgs = new PropertyChangedEventArgs(nameof(HasNoError));

        public string ErrorMessage { get; private set; }
        public bool HasError { get { return !string.IsNullOrEmpty(ErrorMessage); } }
        public bool HasNoError { get { return !HasError; } }

        protected void SetError(string errMessage)
        {
            if (!object.Equals(this.ErrorMessage, errMessage))
            {
                ErrorMessage = errMessage;

                OnPropertyChanged(ErrorMessageArgs);
                OnPropertyChanged(HasErrorArgs);
                OnPropertyChanged(HasNoErrorArgs);
            }
        }

        protected void SetErrorFmt(string errMessage, params object[] args)
        {
            this.SetError(string.Format(errMessage, args));
        }

    }


}
