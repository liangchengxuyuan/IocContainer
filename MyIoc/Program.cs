using IService;
using LContainer;
using Service;
using System;
using System.Reflection;
using System.Threading.Tasks;


namespace MyIoc
{
    class Program
    {
        static void Main(string[] args)
        {
            Container container = new Container();
            container.scopeType = (int)Container.Scope.Transient;
            container.ResgisterType<IAnimal, Animal>();
            container.ResgisterType<IDog, Dog>();
            container.ResgisterType<ICat, Cat>();

            IAnimal animal = container.Rerolve<IAnimal>();

            Task.Run(() =>
            {
                IAnimal animal1 = container.Rerolve<IAnimal>();
            });
            IAnimal animal2 = container.Rerolve<IAnimal>();
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
