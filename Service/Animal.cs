using IService;
using LContainer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service
{
    public class Animal : IAnimal
    {
        //[LInjectionConstructor]
        public Animal(IDog dog)
        {
            Console.WriteLine("This is Animal");
        }
    }
}
