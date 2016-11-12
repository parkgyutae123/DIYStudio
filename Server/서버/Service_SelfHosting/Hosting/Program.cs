using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Hosting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server");
            ServiceHost host = new ServiceHost(typeof(Service_SelfHosting.Service1));
            host.Open();
            Console.ReadLine();
            host.Close();

        }
    }
}
