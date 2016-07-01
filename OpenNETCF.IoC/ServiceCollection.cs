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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.IoC
{
    public class ServiceCollection : ICollection, IEnumerable, IEnumerable<KeyValuePair<Type, object>>
    {
        public event EventHandler<DataEventArgs<object>> Added;
        public event EventHandler<DataEventArgs<object>> Removed;

        private List<ComponentDescriptor> m_services = new List<ComponentDescriptor>();
        internal Dictionary<Type, Type> TypeRegistrations = new Dictionary<Type, Type>();
        private object m_syncRoot = new object();
        private WorkItem m_root;

        internal ServiceCollection(WorkItem root)
        {
            m_root = root;

            ComponentDescriptor descriptor = new ComponentDescriptor
            {
                ClassType = m_root.GetType(),
                Instance = m_root,
                Name = "WorkItem",
                RegistrationType = m_root.GetType()
            };

            m_services.Add(descriptor);
        }

        public void RegisterType<TConcrete, TRegisterAs>()
        {
            RegisterType(typeof(TConcrete), typeof(TRegisterAs));
        }

        public void RegisterType(Type concreteType, Type registerAs)
        {
            if (TypeRegistrations.ContainsKey(registerAs))
            {
                // replace the registration
                TypeRegistrations[registerAs] = concreteType;
            }
            else
            {
                TypeRegistrations.Add(registerAs, concreteType);
            }
        }

        internal int GetInstanciatedServiceCount()
        {
            return (from s in m_services
                    where s.Instance != null
                    select s)
                   .Count();
        }

        public TService AddNew<TService>()
            where TService : class
        {
            return (TService)AddNew(typeof(TService));
        }

        public TService AddNew<TService, TRegisterAs>()
            where TRegisterAs : class
            where TService : class, TRegisterAs
        {
            return (TService)AddNew(typeof(TService), typeof(TRegisterAs));
        }

        public object AddNew(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException("serviceType");

            var descriptor = this.m_services.FirstOrDefault(s => s.ClassType.Equals(serviceType));
            if (descriptor == null)
            {
                var instance = ObjectFactory.CreateObject(serviceType, m_root);
                Add(instance, null, serviceType, null);
                return instance;
            }
            else
            {
                throw new RegistrationTypeInUseException(string.Format("Service already registered with type '{0}'", serviceType.Name));
            }
        }

        public object AddNew(Type serviceType, Type registerAs)
        {
            if (serviceType == null) throw new ArgumentNullException("serviceType");
            if (registerAs == null) throw new ArgumentNullException("registerAs");
#if !PCL
            if (registerAs.IsInterface)
            {
                if (!serviceType.Implements(registerAs))
                {
                    throw new ArgumentException(string.Format("instance must derive from {0} to be registered as that type", registerAs.Name));
                }
            }
            else if ((serviceType != registerAs) && (!serviceType.IsSubclassOf(registerAs)))
            {
                throw new ArgumentException(string.Format("instance must derive from {0} to be registered as that type", registerAs.Name));
            }
#endif
            object instance = ObjectFactory.CreateObject(serviceType, m_root);
            Add(instance, null, serviceType, registerAs);
            return instance;
        }

        public void AddOnDemand<TService>()
            where TService : class
        {
            Type t = typeof(TService);
            Add(null, null, t, null);
        }

        public void AddOnDemand<TService, TRegisterAs>()
            where TRegisterAs : class
            where TService : class, TRegisterAs
        {
            Type t = typeof(TService);
            Add(null, null, t, typeof(TRegisterAs));
        }

        public void Add<TService>(object serviceInstance)
            where TService : class
        {
            if (serviceInstance == null) throw new ArgumentNullException("serviceInstance");

            Add(serviceInstance, null, serviceInstance.GetType(), typeof(TService));
        }

        public void Add(object serviceInstance)
        {
            Add(serviceInstance.GetType(), serviceInstance);
        }

        public void Add(Type registerAs, object serviceInstance)
        {
            if (serviceInstance == null) throw new ArgumentNullException("serviceInstance");
            if (registerAs == null) throw new ArgumentNullException("serviceType");
#if !PCL
            if (registerAs.IsInterface)
            {
                if (!serviceInstance.GetType().Implements(registerAs))
                {
                    throw new ArgumentException(string.Format("instance must derive from {0} to be registered as that type", registerAs.Name));
                }
            }
            else if ((serviceInstance.GetType() != registerAs) && (!serviceInstance.GetType().IsSubclassOf(registerAs)))
            {
                throw new ArgumentException(string.Format("instance must derive from {0} to be registered as that type", registerAs.Name));
            }
#endif
            // generate a name
            Add(serviceInstance, null, serviceInstance.GetType(), registerAs);
        }

        private void Add(object instance, string name, Type classType, Type registrationType)
        {
            if (classType == null) throw new ArgumentNullException("classType");

            if (registrationType == null) registrationType = classType;

            lock (m_syncRoot)
            {
                ComponentDescriptor descriptor = GetDescriptor(registrationType);

                if (descriptor != null)
                {
                    throw new RegistrationTypeInUseException(string.Format("Service already registered with type '{0}'", registrationType.Name));
                }

                if (name == null) name = ObjectFactory.GenerateServiceName(registrationType);

                descriptor = new ComponentDescriptor
                {
                    Name = name,
                    ClassType = classType,
                    Instance = instance,
                    RegistrationType = registrationType
                };

                m_services.Add(descriptor);
#if !PCL
                if (instance != null)
                {
                    ObjectFactory.DoInjections(instance, m_root);
                }
#endif
                if (Added == null) return;

                Added(this, new DataEventArgs<object>(descriptor.Instance));
            }
        }

        private ComponentDescriptor GetDescriptor(Type registrationType)
        {
            lock (m_syncRoot)
            {
                var d = m_services.FirstOrDefault(s => s.RegistrationType == registrationType);

                if(d == null)
                {
                    d = m_services.FirstOrDefault(s => s.ClassType == registrationType);
                }

                return d;
            }
        }

        private void UpdateDescriptor(ComponentDescriptor descriptor)
        {
            lock (m_syncRoot)
            {
                int index = m_services.IndexOf(descriptor);

                if (index < 0) throw new Exception("Cannot update descriptor");

                m_services[index] = descriptor;
            }
        }

        public TService Get<TService>() where TService : class
        {
            return Get<TService>(false);
        }

        public TService Get<TService>(bool ensureExists) where TService : class
        {
            Type t = typeof(TService);
            return Get(t, ensureExists) as TService;
        }

        public TService GetOrCreate<TService>() where TService : class
        {
            TService instance = Get<TService>(false);
            if (instance != null) return instance;

            return AddNew<TService>();
        }

        public TService GetOrCreate<TService, TRegisterAs>()
            where TRegisterAs : class
            where TService : class, TRegisterAs
        {
            TRegisterAs instance = Get<TRegisterAs>(false);
            if (instance != null) return instance as TService;

            return AddNew<TService, TRegisterAs>();
        }

        public object GetOrCreate(Type serviceType)
        {
            object instance = Get(serviceType, false);
            if (instance != null) return instance;

            return AddNew(serviceType);
        }

        public object Get(Type serviceType)
        {
            return Get(serviceType, false);
        }

        public object Get(Type serviceType, bool ensureExists)
        {
            lock (m_syncRoot)
            {
                ComponentDescriptor desc = GetDescriptor(serviceType);

                if (desc == null)
                {
                    if (ensureExists)
                    {
                        throw new ServiceMissingException(string.Format("Cannot get service of type '{0}'", serviceType.Name));
                    }
                    return null;
                }

                if (desc.Instance == null)
                {
                    desc.Instance = ObjectFactory.CreateObject(desc.ClassType, m_root);

                }
                return desc.Instance;
            }
        }

        public void Remove(object instance)
        {
            if (instance == null) throw new ArgumentNullException();

            Remove(instance.GetType());
        }

        public void Remove<TService>()
        {
            Remove(typeof(TService));

        }

        private void Remove(Type t)
        {
            lock (m_syncRoot)
            {
                ComponentDescriptor descriptor = GetDescriptor(t);

                if (descriptor == null) return;

                // be nice and dispose of disposable items
                if (descriptor.Instance != null)
                {
                    IDisposable d = descriptor.Instance as IDisposable;
                    if (d != null) d.Dispose();
                }

                m_services.Remove(descriptor);

                if (Removed == null) return;

                Removed(this, new DataEventArgs<object>(descriptor.Instance));
            }
        }

        public bool Contains(Type registrationType)
        {
            return GetDescriptor(registrationType) != null;
        }

        public bool Contains<TRegistrationType>()
        {
            return GetDescriptor(typeof(TRegistrationType)) != null;
        }

        public void CopyTo(Array array, int index)
        {
            lock (m_syncRoot)
            {
            m_services.ToArray().CopyTo(array, index);
        }
        }

        public int Count
        {
            get 
            {
                lock (m_syncRoot)
                {
                    return m_services.Count;
                }
            }
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        public object SyncRoot
        {
            get { return m_syncRoot; }
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            lock (m_syncRoot)
            {
            for (int i = 0; i < m_services.Count; i++)
            {
                yield return m_services[i];
            }
        }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            lock (m_syncRoot)
            {

            var vals = (from i in this
                        select i.Value).ToArray();

            for (int i = 0; i < vals.Length; i++)
            {
                this.Remove(vals[i]);
            }

            m_services.Clear();
        }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in this)
            {
                sb.Append(string.Format("{0} : {1}", item.Key, item.Value.ToString()));
            }
            return sb.ToString();
        }
    }
}
