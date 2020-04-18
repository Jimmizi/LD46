using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public static class Service
{
    public class ProvidedService<T>
    {
        private T myService;
        
        public T Get()
        {
            return myService;
        }

        public void Set(T newService)
        {
            myService = newService;
        }
    }

    public static ProvidedService<OptionsMonobehaviour> Options = new ProvidedService<OptionsMonobehaviour>();

}
