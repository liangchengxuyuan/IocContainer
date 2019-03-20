using IService;
using LContainer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service
{
    public class Dog : IDog
    {
        //[LInjectionConstructor]
        public Dog(ICat cat)
        {
            Console.WriteLine("This is Dog");
        }
    }
}
