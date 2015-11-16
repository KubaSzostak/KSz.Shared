using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{



    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


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
        public virtual void NotifyAllPropertiesChanged()
        {
            NotifyPropertyChanged(allPropertiesArgs);
        }

        public virtual void NotifyPropertyChanged(PropertyChangedEventArgs args)
        {
            // http://stackoverflow.com/questions/2553333/wpf-databinding-thread-safety
            // In WPF .NET 3.5 INotifyPropertyChanged is thread safe

            VerifyProperty(args.PropertyName);

            // In multi-thread approach this.PropertyChanged can change its value between  [PropertyChanged == null] and [PropertyChanged(...)]
            var propCh = this.PropertyChanged;
            if (propCh != null)
            {
                propCh(this, args);
            }

            //http://10rem.net/blog/2012/01/10/threading-considerations-for-binding-and-change-notification-in-silverlight-5
            //Deployment.Current.Dispatcher.CheckAccess()
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            NotifyPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void NotifyPropertyChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propName = GetPropertyMemberName(propertyExpresssion);
            NotifyPropertyChanged(propName);
        }


        public virtual bool NotifyPropertyChanged<T>(ref T propVal, T newVal, PropertyChangedEventArgs args)
        {
            if (!object.Equals(propVal, newVal))
            {
                propVal = newVal;
                NotifyPropertyChanged(args);
                return true;
            }
            return false;
        }

        protected bool NotifyPropertyChanged<T>(ref T propVal, T newVal, Expression<Func<T>> propertyExpresssion)
        {
            if (!object.Equals(propVal, newVal))
            {
                propVal = newVal;
                var propName = GetPropertyMemberName(propertyExpresssion);
                NotifyPropertyChanged(new PropertyChangedEventArgs(propName));
                return true;
            }
            return false;
        }

        public void OnPropertyChanged(string propertyName)
        {
            var propertyArgs = new PropertyChangedEventArgs(propertyName);
            NotifyPropertyChanged(propertyArgs);

            //  Always run OnPropertyChanged(string propertyName) -> DbRecord.IsChanged
            //if (PropertyChanged == null)
            //    return;  
        }

        /// <summary>
        /// You can use:
        /// NotifyPropertyChanged(() => MyProperty);
        /// </summary>
        /// <typeparam name="T">Leave Empty</typeparam>
        /// <param name="propertyExpresssion">() => MyProperty</param>
        public void OnPropertyChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propName = GetPropertyMemberName(propertyExpresssion);
            OnPropertyChanged(propName);
        }

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


        public static string GetPropertyName<T>(Expression<Func<T, object>> propertyExpresssion)
        {
            return GetPropertyMemberName(propertyExpresssion);
        }

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
        public static PropertyChangedEventArgs GetPropertyArgs<T>(Expression<Func<T, object>> propertyExpresssion)
        {
            return new PropertyChangedEventArgs(GetPropertyMemberName(propertyExpresssion));
        }



    }



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
        public string DisplayName { get; set; }

        public virtual void RefreshData()
        {
        }

        private static PropertyChangedEventArgs ErrorMessageArgs = GetPropertyArgs<Presenter>(x => x.ErrorMessage);
        private static PropertyChangedEventArgs HasErrorArgs = GetPropertyArgs<Presenter>(x => x.HasError);
        private static PropertyChangedEventArgs HasNoErrorArgs = GetPropertyArgs<Presenter>(x => x.HasNoError);
        public string ErrorMessage { get; private set; }
        public bool HasError { get; private set; }
        public bool HasNoError { get; private set; }

        protected void SetError(string errMessage)
        {
            if (!object.Equals(this.ErrorMessage, errMessage))
            {
                ErrorMessage = errMessage;
                HasError = !string.IsNullOrEmpty(errMessage);
                HasNoError = !HasError;

                NotifyPropertyChanged(ErrorMessageArgs);
                NotifyPropertyChanged(HasErrorArgs);
                NotifyPropertyChanged(HasNoErrorArgs);
            }
        }

        protected void SetErrorFmt(string errMessage, params object[] args)
        {
            this.SetError(string.Format(errMessage, args));
        }
    }


}
