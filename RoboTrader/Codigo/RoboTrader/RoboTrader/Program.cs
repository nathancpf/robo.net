using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboTrader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Bem vindo aos robo.net da Bitcointoyou!");
            Console.WriteLine("Qual robô você deseja executar?");
            Console.WriteLine("1 - Robo war");
            Console.WriteLine("2 - Robo trader v2");
            Console.WriteLine("3 - Robo trader v3");
            Console.WriteLine("Digite um número de 1 a 3 e pressione enter.");

            string opcao = Console.ReadLine();

            if (opcao == "1")
            {
                RoboWar robo = new RoboWar();
            }
            else if (opcao == "2")
            {
                RoboTrader2 robo = new RoboTrader2();
            }
            else if (opcao == "3")
            {
                Robo3 robo = new Robo3();
            }
        }
    }
}
