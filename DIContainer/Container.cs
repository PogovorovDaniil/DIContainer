using System;
using System.Collections.Generic;

namespace DIContainer
{
    public class Container
    {
        private Dictionary<Type, InjectionType> container;
        private Dictionary<Type, List<Type>> inheritances;
        private Dictionary<Type, object> singletons;
        private Type entryPoint;
        private string entryPointMethod;

        public Container() 
        {
            container = new Dictionary<Type, InjectionType>();
            inheritances = new Dictionary<Type, List<Type>>();
            singletons = new Dictionary<Type, object>();
        }

        public void AddTransient<T>() where T : class => container.Add(typeof(T), InjectionType.Transient);
        public void AddTransient<IT, T>() where T : IT
        {
            container.Add(typeof(T), InjectionType.Transient);
            if(inheritances.ContainsKey(typeof(IT)))
            {
                inheritances[typeof(IT)].Add(typeof(T));
            }
            else
            {
                inheritances[typeof(IT)] = new List<Type>() { typeof(T) };
            }
        }
        public void AddSingleton<T>() where T : class => container.Add(typeof(T), InjectionType.Singleton);
        public void AddEntryPoint<T>(string methodName = "Run", bool asSingleton = true) 
        {
            if (entryPoint is not null) throw new Exception("Точка входа уже задана");
            container.Add(typeof(T), asSingleton ? InjectionType.Singleton : InjectionType.Transient);
            entryPoint = typeof(T); 
            entryPointMethod = methodName;
        }
        public void Run() => entryPoint.GetMethod(entryPointMethod).Invoke(GetService(entryPoint), null);
        public T GetService<T>() => (T)GetService(typeof(T));
        private object GetService(Type type)
        {
            InjectionType injectionType = container[type];
            if(injectionType == InjectionType.Singleton && singletons.ContainsKey(type))
            {
                return singletons[type];
            }

            var constructors = type.GetConstructors();
            if (constructors.Length != 1) throw new Exception($"У типа {type.FullName} должен быть один конструктор");
            var constructor = constructors[0];
            var parameters = constructor.GetParameters();
            object[] arguments = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                Type parameterType = parameters[i].ParameterType;
                if (parameterType.IsInterface)
                {
                    List<Type> types = inheritances[parameterType];
                    if(types.Count != 1) throw new Exception($"У интерфейса {parameterType.FullName} должна быть одна реализация");
                    parameterType = types[0];
                }
                else if (parameterType.IsArray)
                {
                    Type elementType = parameterType.GetElementType();
                    List<Type> types = inheritances[elementType];
                    Array arrayOfInstance = (Array)Activator.CreateInstance(parameterType, types.Count);
                    for(int j = 0; j < types.Count; j++)
                    {
                        arrayOfInstance.SetValue(GetService(types[j]), j);
                    }
                    arguments[i] = arrayOfInstance;
                    continue;
                }
                arguments[i] = GetService(parameterType);
            }
            object instance = Activator.CreateInstance(type, args: arguments);

            if (injectionType == InjectionType.Singleton) singletons[type] = instance;
            return instance;
        }
    }
}