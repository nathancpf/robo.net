using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json; //Adicionado por referência ao projeto System.Runtime.Serialization
using System.Runtime.Serialization;
using System.Collections;

namespace RoboTrader
{




    public class RoboTrader
    {
        
        public List<Ordem> ListaOrdem = new List<Ordem>();

        
        public decimal margemLucro = 0.1M;

        public decimal quantidadeMaxima = 1M;

        private APIWrapper api;



        public RoboTrader()
        {
            Console.WriteLine("Robotrader is alive!");

            try
            {
                api = new APIWrapper(Constantes.NOME_ROBO_TRADER);
                monitorarOrdens();
            }
            catch (Exception aException)
            {

                Console.WriteLine(aException.Message);
            }
        }

        private void criaOrdensIniciais()
        {
           
                OrderList orderList = api.criarOrdemCompra(0.0001M, 10500);

                foreach (Ordem ordem in orderList.oReturn)
                {
                    ListaOrdem.Add(ordem);

                }
           
                 orderList = api.criarOrdemVenda(0.0001M, 10501);

                foreach (Ordem ordem in orderList.oReturn)
                {
                    ListaOrdem.Add(ordem);

                }
           
        }

        
        public void monitorarOrdens()
        {
            popularListaOrdens();
            Console.WriteLine("Inicio monitorando ordens: " + ListaOrdem.Count);
            for (int i = ListaOrdem.Count - 1; i >= 0; --i)
            {
                Ordem ordem = ListaOrdem[i];


                Ordem cOrdem = api.obterOrdemPorID(ordem.id);
                Console.WriteLine(string.Format("ID {0}, TIPO {1}, PRECO {2}, QTD{3}", ordem.id, ordem.action, ordem.price, ordem.amount));
                if (cOrdem.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_EXECUTADA)
                {
                    Console.Beep(); Console.Beep(); Console.Beep();
                    Console.WriteLine("Ordem executada :" + cOrdem.id);
                    ordem.status = Constantes.STATUS_ORDEM_EXECUTADA;

                    criarOrdemInversa(cOrdem);

                    ListaOrdem.RemoveAt(i);

                }
                else if (cOrdem.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_PENDENTE)
                {
                    if (Convert.ToDecimal(cOrdem.executedAmount) > 0)
                    {
                        Console.Beep();
                        Console.WriteLine("Ordem parcialmente executada :" + cOrdem.id);
                        api.cancelarOrdem(cOrdem);
                        criarOrdemInversa(cOrdem);
                        criarOrdemResidual(cOrdem);
                        ListaOrdem.RemoveAt(i);

                    }
                }
            }

            Console.WriteLine("Fim do ciclo, qtd ordens :" + ListaOrdem.Count);
            System.Threading.Thread.Sleep(3000);
            monitorarOrdens();
        }

        private void popularListaOrdens()
        {

            Console.WriteLine("Buscando ordens pendentes...");
            OrderList orderList = api.obterOrdensPendentes();
            //ListaOrdem.Clear();
            foreach (Ordem ordem in orderList.oReturn)
            {
                bool add = true;

                if (ordem.status == Constantes.STATUS_ORDEM_PENDENTE &&
                   Convert.ToDecimal(ordem.amount) <= quantidadeMaxima)
                {
                    foreach (Ordem cOrdem in ListaOrdem)
                    {
                        if (ordem.id == cOrdem.id)
                        {
                            add = false;
                        }
                    }
                    if (add)
                    {
                        Console.WriteLine("Nova ordem encontrada ID: " + ordem.id);
                        ListaOrdem.Add(ordem);
                    }

                }

                
            }

            for (int i = ListaOrdem.Count - 1; i >= 0; --i)
            {
                bool remover = false;
                Ordem ordem = ListaOrdem[i];
                foreach (Ordem cOrdem in orderList.oReturn)
                {
                    if (ordem.id == cOrdem.id &&
                        cOrdem.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_CANCELADA)
                    {
                        remover = true;
                        break;
                    }
                }
                if (remover)
                {
                    Console.WriteLine("Ordem cancelada no site ID: " + ordem.id);
                    ListaOrdem.RemoveAt(i);
                }
            }
        }
        
        public decimal calcularPrecoVenda(decimal preco)
        {
            decimal aux = (preco * margemLucro) / 100;
            decimal novoPreco = preco + aux;
            return novoPreco;
        }

        public decimal calcularPrecoCompra(decimal preco)
        {
            decimal aux = (preco * margemLucro) / 100;
            decimal novoPreco = preco - aux;
            return novoPreco;
        }

        private void criarOrdemResidual(Ordem ordem)
        {
            Console.WriteLine("Criando ordem residual");
            decimal amount = Convert.ToDecimal(ordem.amount) - Convert.ToDecimal(ordem.executedAmount);
            OrderList orderList = null;
            if (ordem.action == "buy")
            {
                orderList = api.criarOrdemCompra(Convert.ToDecimal(amount), Convert.ToDecimal(ordem.price));
            }
            else if (ordem.action == "sell")
            {
                orderList = api.criarOrdemVenda(Convert.ToDecimal(amount), Convert.ToDecimal(ordem.price));
            }

            foreach (Ordem novaOrdem in orderList.oReturn)
            {
                ListaOrdem.Add(novaOrdem);

            }
        }
        
        private void criarOrdemInversa(Ordem ordem)
        {
            Console.WriteLine("Criando ordem inversa:");
            OrderList orderList = null;
            if (ordem.action == "buy")
            {
                orderList = api.criarOrdemVenda(Convert.ToDecimal(ordem.amount), calcularPrecoVenda(Convert.ToDecimal(ordem.price)));
            }
            else if (ordem.action == "sell")
            {
                orderList = api.criarOrdemCompra(Convert.ToDecimal(ordem.amount), calcularPrecoCompra(Convert.ToDecimal(ordem.price)));
            }

            foreach (Ordem novaOrdem in orderList.oReturn)
            {
                ListaOrdem.Add(novaOrdem);

            }
        }

        
        
    }

}
