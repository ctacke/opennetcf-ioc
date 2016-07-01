// LICENSE
// -------
// This software was originally authored by Christopher Tacke of OpenNETCF Consulting, LLC
// On March 10, 2009 is was placed in the public domain, meaning that all copyright has been disclaimed.
//
// You may use this code for any purpose, commercial or non-commercial, free or proprietary with no legal 
// obligation to acknowledge the use, copying or modification of the source.
//
// OpenNETCF will maintain an "official" version of this software at www.opennetcf.com and public 
// submissions of changes, fixes or updates are welcomed but not required
//

using System;

namespace OpenNETCF.IoC
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class InjectionConstructorAttribute : Attribute
    {
        public InjectionConstructorAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InjectionMethodAttribute : Attribute
    {
        public InjectionMethodAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Event)]
    public sealed class EventPublication : Attribute
    {
        public EventPublication(string eventName, PublicationScope scope)
        {
            if(eventName == null) throw new ArgumentNullException();
            if(eventName == string.Empty) throw new ArgumentException();

            this.EventName = eventName;
            this.PublicationScope = scope;
        }

        public EventPublication(string eventName)
            : this(eventName, PublicationScope.Global)
        {
        }

        public string EventName { get; set; }    
        public PublicationScope PublicationScope { get; set; }
    }

    public enum ThreadOption
    {
        Caller,
        UserInterface
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EventSubscription : Attribute, IEquatable<EventSubscription>
    {
        public EventSubscription(string eventName, ThreadOption threadOption)
        {
            if(eventName == null) throw new ArgumentNullException();
            if(eventName == string.Empty) throw new ArgumentException();

            this.EventName = eventName;
            this.ThreadOption = threadOption;
        }

        public string EventName { get; set; }
        public ThreadOption ThreadOption { get; set; }

        public bool Equals(EventSubscription other)
        {
            return other.EventName == this.EventName;
        }
    }
}
