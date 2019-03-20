using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LContainer
{
    public class Container
    {
        private static Dictionary<string, object> ContainerTypeDictionary = new Dictionary<string, object>();
        private static Dictionary<string, object> ContainerExampleDictionary = new Dictionary<string, object>();
        private static int _scopeType = (int)Scope.Singleton;
        private static readonly object locker = new object();

        public int scopeType
        {
            get
            {
                return _scopeType;
            }
            set
            {
                _scopeType = value;
            }
        }
        public enum Scope
        {
            Singleton = 0,
            Transient = 1,
            Scoped = 2
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        /// <typeparam name="IT"></typeparam>
        /// <typeparam name="T"></typeparam>
        public void ResgisterType<IT,T>()
        {
            if (!ContainerTypeDictionary.ContainsKey(typeof(IT).FullName))
                ContainerTypeDictionary.Add(typeof(IT).FullName, typeof(T));
        }

        /// <summary>
        /// 根据注册信息生成实例
        /// </summary>
        /// <typeparam name="IT"></typeparam>
        /// <returns></returns>
        public IT Rerolve<IT>()
        {
            string key = typeof(IT).FullName;
            Type type = (Type)ContainerTypeDictionary[key];

            return (IT)CreateType(type);
        }

        /// <summary>
        /// 根据提供的类型创建类型实例并返回
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private object CreateType(Type type)
        {
            var ctorArray = type.GetConstructors();
            if (ctorArray.Count(c => c.IsDefined(typeof(LInjectionConstructorAttribute), true)) > 0)
            {
                //获取带特性标记的构造函数参数
                foreach (var cotr in type.GetConstructors().Where(c => c.IsDefined(typeof(LInjectionConstructorAttribute), true)))
                {
                    var paraArray = cotr.GetParameters();//获取参数数组
                    if (paraArray.Length == 0)
                    {
                        return GetSocpe(type);
                    }

                    List<object> listPara = new List<object>();
                    foreach (var para in paraArray)
                    {
                        string paraKey = para.ParameterType.FullName;//参数类型名称
                                                                     //从字典中取出缓存的目标对象并创建对象
                        Type paraTargetType = (Type)ContainerTypeDictionary[paraKey];
                        object oPara = CreateType(paraTargetType);//递归
                        listPara.Add(oPara);
                    }
                    return GetSocpe(type, listPara.ToArray());
                }

                return GetSocpe(type);
                //return Activator.CreateInstance(type);
            }
            else
            {
                //没有标记特性则使用参数最多的构造函数
                var ctor = ctorArray.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
                var paraArray = ctor.GetParameters();//获取参数数组
                if (paraArray.Length == 0)
                {
                    //return Activator.CreateInstance(type);
                    return GetSocpe(type);
                }

                List<object> listPara = new List<object>();
                foreach (var para in paraArray)
                {
                    string paraKey = para.ParameterType.FullName;//参数类型名称
                                                                 //从字典中取出缓存的目标对象并创建对象
                    Type paraTargetType = (Type)ContainerTypeDictionary[paraKey];
                    object oPara = CreateType(paraTargetType);//递归
                    listPara.Add(oPara);
                }
                return GetSocpe(type, listPara.ToArray());
                //return Activator.CreateInstance(type, listPara.ToArray());
            }
        }
        private object GetSocpe(Type type, params object[] listPara)
        {
            if (_scopeType == (int)Scope.Singleton)
            {
                return GetTypeSingleton(type, listPara);
            }
            else if (_scopeType == (int)Scope.Transient)
            {
                return GetTypeTransient(type, listPara);
            }
            else
            {
                return GetTypeScoped(type, listPara);
            }
        }

        #region 生命周期
        /// <summary>
        /// 设置获取实例对象生命周期为Singleton
        /// </summary>
        /// <param name="type"></param>
        /// <param name="listPara"></param>
        /// <returns></returns>
        private object GetTypeSingleton(Type type, params object[] listPara)
        {
            if (ContainerExampleDictionary.ContainsKey(type.FullName))
            {
                lock (locker)
                {
                    if (ContainerExampleDictionary.ContainsKey(type.FullName))
                    {
                        return ContainerExampleDictionary[type.FullName];
                    }
                }
            }

            if (listPara.Length == 0)
            {
                var Example = Activator.CreateInstance(type);
                ContainerExampleDictionary.Add(type.FullName, Example);
                return Example;
            }
            else
            {
                var Example = Activator.CreateInstance(type, listPara.ToArray());
                ContainerExampleDictionary.Add(type.FullName, Example);
                return Example;
            }
        }

        /// <summary>
        /// 设置获取实例对象生命周期为Transient
        /// </summary>
        /// <param name="type"></param>
        /// <param name="listPara"></param>
        /// <returns></returns>
        private object GetTypeTransient(Type type, params object[] listPara)
        {
            if (listPara.Length == 0)
            {
                var Example = Activator.CreateInstance(type);
                return Example;
            }
            else
            {
                var Example = Activator.CreateInstance(type, listPara.ToArray());
                return Example;
            }
        }

        /// <summary>
        /// 设置获取实例对象生命周期为Scoped
        /// </summary>
        /// <param name="type"></param>
        /// <param name="listPara"></param>
        /// <returns></returns>
        private object GetTypeScoped(Type type, params object[] listPara)
        {
            var pid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (ContainerExampleDictionary.ContainsKey(type.FullName + pid))
            {
                lock (locker)
                {
                    if (ContainerExampleDictionary.ContainsKey(type.FullName + pid))
                    {
                        return ContainerExampleDictionary[type.FullName + pid];
                    }
                }
            }

            if (listPara.Length == 0)
            {
                var Example = Activator.CreateInstance(type);
                ContainerExampleDictionary.Add(type.FullName + pid, Example);
                return Example;
            }
            else
            {
                var Example = Activator.CreateInstance(type, listPara.ToArray());
                ContainerExampleDictionary.Add(type.FullName + pid, Example);
                return Example;
            }
        }
        #endregion
    }
}
