﻿using System.Collections.Concurrent;

namespace PlayFabBuddy.PlayFabHelpers.Util.IoC
{
    // Thanks to @KDSBest and his awesome youtube channel and github project, check him out! https://www.youtube.com/playlist?list=PL7NJAOUPKXUtjgY_039m9OgPnWL5nCG8S
    public class DependencyInjection
    {
        private static readonly Lazy<DependencyInjection> lazyInstance = new Lazy<DependencyInjection>(() => new DependencyInjection());

        public static DependencyInjection Instance => lazyInstance.Value;

        private ConcurrentDictionary<Type, object> instanceList = new ConcurrentDictionary<Type, object>();
        private ConcurrentDictionary<Type, Registration> registrations = new ConcurrentDictionary<Type, Registration>();

        private DependencyInjection() { }

        public void Register<T>(Func<T> creationDelegateTyped, RegistrationType type)
        {
            if (creationDelegateTyped == null)
            {
                throw new ArgumentNullException($"{nameof(creationDelegateTyped)} can't be null.");
            }


            if (creationDelegateTyped is not Func<object> creationDelegate)
            {
                throw new ArgumentNullException($"{nameof(creationDelegateTyped)} must be assignable to Func<object>.");
            }

            var registration = new Registration(creationDelegate, type);
            registrations.AddOrUpdate(typeof(T), registration, (t, reg) => registration);
        }

        public T Resolve<T>()
        {
            if (registrations.TryGetValue(typeof(T), out Registration? registration))
            {
                switch (registration.Type)
                {
                    case RegistrationType.Singleton:
                        var singletonInstance = (T)instanceList.GetOrAdd(typeof(T), (t) => registration.CreationDelegate());
                        return singletonInstance;
                    case RegistrationType.New:
                        return (T)registration.CreationDelegate();
                }
            }

            throw new InvalidOperationException($"Couldn't find registration for {typeof(T).FullName}");
        }

    }
}
