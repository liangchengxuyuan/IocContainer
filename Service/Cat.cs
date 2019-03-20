using IService;
using LContainer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service
{
    public class Cat : ICat
    {
        //[LInjectionConstructor]
        public Cat()
        {
            Console.WriteLine("This is Cat");
        }
    }
}
