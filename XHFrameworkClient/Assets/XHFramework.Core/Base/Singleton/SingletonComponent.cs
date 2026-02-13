using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace XHFramework.Core
{
    /// <summary>
    /// 普通C#类的单例组件，组合该组件需要提供一个私有的构造方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonComponent<T> where T : class
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    //从所有私有构造方法里获取无参构造方法
                    ConstructorInfo[] ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                    ConstructorInfo ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);

                    if (ctor == null)
                    {
                        throw new Exception("组合C#单例组件的类没有提供私有无参构造方法：" + typeof(T).Name);
                    }
                    else
                    {
                        //调用构造方法
                        _instance = ctor.Invoke(null) as T;
                    }
                }

                return _instance;
            }

        }

    }
}


