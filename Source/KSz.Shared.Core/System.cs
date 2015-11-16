using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System
{

    // Delegates
    
    public delegate void Event<TEventArgs>(TEventArgs e);
    public delegate void ProgressChangedEvent(double percent, string info);
    public delegate void StatusChangedEvent(string info);



    // public delegate void EventHandler<TEventArgs>(object sender, TEventArgs e);

    public class EventArgs<TArg> : EventArgs
    {
        public EventArgs(TArg arg)
        {
            Arg = arg;

        }

        public TArg Arg { get; private set; }
    }

    // Attributes 

    public class NotEditablePropertyAttribute : Attribute { }


    public class AssemblyTrialDaysAttribute : Attribute
    {
        public AssemblyTrialDaysAttribute(int trialDays)
        {
            this.TrialDays = trialDays;
        }

        public readonly int TrialDays;
    }



    public class AssemblyLimitAttribute : Attribute
    {

        public AssemblyLimitAttribute(string limit)
        {
            this.Limit = limit;
        }

        public readonly string Limit;
    }


    // Interfaces

    public interface ITagedObject
    {
        System.Collections.IDictionary Tags { get; }
    }

    public interface IAssignable
    {
        void AssignFromText(string s);
    }



    // Exceptions

    public class NotifyException : Exception
    {
        public NotifyException(string message) : base(message) { }
        public NotifyException(string message, Exception inner) : base(message, inner) { }
        public NotifyException(string message, params string[] args) : base(string.Format(message, args)) { }
    }

    
    public class WarningException : Exception
    {
        public WarningException(string message) : base(message) { }
        public WarningException(string message, Exception inner) : base(message, inner) { }
        public WarningException(string message, params string[] args) : base(string.Format(message, args)) { }
    }


    //     



    public class CtrlChars
    {

        public static readonly char Null = (char)0;
        public static readonly char Back = (char)8;
        public static readonly char Tab = (char)9;
        public static readonly char Cr = (char)13;
        public static readonly char Lf = (char)10;
        public static readonly string CrLf = "\r\n";
        public static readonly char[] EmptyCharArray = string.Empty.ToCharArray();

        public static readonly char[] WhitespaceChars = new char[] {
            ' ', '\t', '\n', '\v', '\f', '\r', '\x0085', '\x00a0', '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200a', '\u200b', '\u2028', '\u2029', '\u3000', '\ufeff'
        };

    }


    public static class TagedObjectEx
    {
        public static void AddTags(this ITagedObject thisObject, ITagedObject sourceObject)
        {
            foreach (var key in sourceObject.Tags.Keys)
            {
                thisObject.Tags[key] = sourceObject.Tags[key];
            }
        }
    }




}
