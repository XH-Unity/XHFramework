using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace XHFramework.Core
{
    /// <summary>
    /// 普通C#类的单例模板基类，继承该类需要提供一个私有的构造方法
    /// </summary>
    public abstract class ScriptSingleton<T> where T : ScriptSingleton<T>
    {
        protected static T _instance = null;

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
                        throw new Exception("继承C#单例模板的类没有提供私有无参构造方法：" + typeof(T).Name);
                    }
                    else
                    {
                        //调用构造方法并初始化
                        _instance = ctor.Invoke(null) as T;

                    }
                }
                return _instance;
            }
        }
    }
}
