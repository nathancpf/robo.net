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

    ///<summary>
    ///Este robo monitora as ordens criadas pelo usuário no site. 
    ///Caso seja executadas, cria a ordem inversa (compra ou venda),
    ///com objetivo de ter lucro.
    ///</summary>


    public class RoboTrader2
    {

        public List<Ordem> ListaOrdem = new List<Ordem>();


        

        private APIWrapper api;

       

        public RoboTrader2()
        {
            Console.WriteLine("Robotrader 2.0 is alive!");

            try
            {
                api = new APIWrapper(Constantes.NOME_ROBO_V2);
                monitorarOrdens();
            }
            catch (Exception aException)
            {
                Console.WriteLine(aException.Message);
                Console.ReadLine();
            }
        }


        public void criaOrdens()
        {
            if (api.saldo.saldoBRL > 0 || api.saldo.saldoBTC > 0)
            {

                TickerBitstamp bitstamp = api.obterTickerBitstamp();
                decimal dolar = api.obterCotacaoDolar();
                decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
                OrderList orderList = null;


                decimal precoCompra = ((precoBitstamp * api.parametros.spreadCompra / 100) + precoBitstamp) * dolar;
                if (api.saldo.saldoBRL > api.parametros.quantidadeOrdem * precoCompra)
                {

                    orderList = api.criarOrdemCompra(api.parametros.quantidadeOrdem, precoCompra);
                    foreach (Ordem ordem in orderList.oReturn)
                    {
                        api.imprimirOrdem("Nova ordem", ordem);
                        ListaOrdem.Add(ordem);
                    }
                }

                if (api.saldo.saldoBTC > api.parametros.quantidadeOrdem)
                {
                    decimal precoVenda = ((precoBitstamp * api.parametros.spreadVenda / 100) + precoBitstamp) * dolar;
                    orderList = api.criarOrdemVenda(api.parametros.quantidadeOrdem, precoVenda);
                    foreach (Ordem ordem in orderList.oReturn)
                    {
                        api.imprimirOrdem("Nova ordem", ordem);
                        ListaOrdem.Add(ordem);
                    }
                }


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
                api.imprimirOrdem("Monitorando", ordem);
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

            //criaOrdens();
            Console.WriteLine("*********************************** Fim do ciclo, qtd ordens :" + ListaOrdem.Count + "***********************************");
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
                   Convert.ToDecimal(ordem.amount) <= api.parametros.quantidadeMaxima)
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
            decimal aux = (preco * api.parametros.margemLucro) / 100;
            decimal novoPreco = preco + aux;

            decimal precoVendaBRL = obterPrecoVendaBRL();
            decimal limitePrecoVendaPermitido = precoVendaBRL - (precoVendaBRL * 0.1M / 100);
            if (preco < limitePrecoVendaPermitido)
            {
                novoPreco = limitePrecoVendaPermitido;
            }
            return novoPreco;
        }

        private decimal obterPrecoVendaBRL()
        {
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
            decimal dolar = api.obterCotacaoDolar();
            decimal precoVenda = ((precoBitstamp * api.parametros.spreadVenda / 100) + precoBitstamp) * dolar;
            return precoVenda;
        }

        public decimal calcularPrecoCompra(decimal preco)
        {
            decimal aux = (preco * api.parametros.margemLucro) / 100;
            decimal novoPreco = preco - aux;

            decimal precoCompraBRL = obterPrecoCompraBRL();

            decimal limitePrecoCompraPermitido = (precoCompraBRL * 0.1M / 100) + precoCompraBRL;

            if (preco > limitePrecoCompraPermitido)
            {
                novoPreco = limitePrecoCompraPermitido;
            }
            return novoPreco;
        }
        private decimal obterPrecoCompraBRL()
        {
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
            decimal dolar = api.obterCotacaoDolar();
            decimal precoCompra = ((precoBitstamp * api.parametros.spreadCompra / 100) + precoBitstamp) * dolar;
            return precoCompra;
        }

        private void criarOrdemResidual(Ordem ordem)
        {
            try
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
                    api.imprimirOrdem("Nova ordem", ordem);
                    ListaOrdem.Add(novaOrdem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        private void criarOrdemInversa(Ordem ordem)
        {
            try
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
                    api.imprimirOrdem("Nova ordem", ordem);
                    ListaOrdem.Add(novaOrdem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }



    }

}
