using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace RoboTrader
{
    public class RoboArbitragem
    {


        
       

        public List<Ordem> ListaOrdem = new List<Ordem>();

       

        public decimal margemLucro = 0.1M;

        public decimal quantidadeMaxima = 1M;

        public decimal spreadCompra = 2M;

        public decimal spreadVenda = 4M;

        public decimal quantidadeOrdem = 0.01M;

        public decimal variacaoPermitida = 0.1M;

        private APIWrapper api;

        public RoboArbitragem()
        {
            Console.WriteLine("RoboArbitragem is alive!");

            try
            {
                api = new APIWrapper(Constantes.NOME_ARBITRAGEM);
                // criaOrdensIniciais();
                monitorarArbitragem();
                //monitorarOrdens();
            }
            catch (Exception aException)
            {

                Console.WriteLine(aException.Message);
            }
        }

       
        public void criaOrdensIniciais()
        {
            //Ticker b2u = obterTickerB2U();
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal dolar = api.obterCotacaoDolar();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
            decimal precoVenda = ((precoBitstamp * spreadVenda / 100) + precoBitstamp) * dolar;
            api.criarOrdemVenda(quantidadeOrdem, precoVenda);
            decimal precoCompra = ((precoBitstamp * spreadCompra / 100) + precoBitstamp) * dolar;
            api.criarOrdemCompra(quantidadeOrdem, precoCompra);
        }

        private void criarOrdemVendaArbitragem()
        {           
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal dolar = api.obterCotacaoDolar();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);            
            decimal precoVenda = ((precoBitstamp * spreadVenda / 100) + precoBitstamp) * dolar;
            api.criarOrdemVenda(quantidadeOrdem, precoVenda);
        }

        private void criarOrdemCompraArbitragem()
        {
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal dolar = api.obterCotacaoDolar();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);            
            decimal precoCompra = ((precoBitstamp * spreadCompra / 100) + precoBitstamp) * dolar;
            api.criarOrdemCompra(quantidadeOrdem, precoCompra);
        }

        private decimal obterPrecoCompraBRL()
        {
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
            decimal dolar = api.obterCotacaoDolar();
            decimal precoCompra = ((precoBitstamp * spreadCompra / 100) + precoBitstamp) * dolar;
            return precoCompra;
        }

        private decimal obterPrecoCompraUSD()
        {
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
            
            decimal precoCompra = ((precoBitstamp * spreadCompra / 100) + precoBitstamp);
            return precoCompra;
        }

        private decimal obterPrecoVendaBRL()
        {
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
            decimal dolar = api.obterCotacaoDolar();
            decimal precoVenda = ((precoBitstamp * spreadVenda / 100) + precoBitstamp) * dolar;
            return precoVenda;
        }

        private decimal obterPrecoVendaUSD()
        {
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
            
            decimal precoVenda = ((precoBitstamp * spreadVenda / 100) + precoBitstamp);
            return precoVenda;
        }

        public void monitorarArbitragem()
        {
            //Se preço variar em 0.1% cancela ordens e cria outra
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal dolar = api.obterCotacaoDolar();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
            bool temOC = false;
            bool temOV = false;
            popularListaOrdens();
            Console.WriteLine("Inicio monitorando ordens Arbitragem: " + ListaOrdem.Count);
            for (int i = ListaOrdem.Count - 1; i >= 0; --i)
            {
                Ordem ordem = ListaOrdem[i];
                api.imprimirOrdem("",ordem);
                if (ordem.action == "buy")
                {
                    temOC = true;
                    decimal precoCompraBRL = obterPrecoCompraBRL();
                    decimal precoOrdemAtual = Convert.ToDecimal(ordem.price);
                    decimal limitePrecoCompraPermitido = (precoCompraBRL * 0.1M / 100) + precoCompraBRL;
                    
                    if (precoOrdemAtual > limitePrecoCompraPermitido)
                    {
                        api.cancelarOrdem(ordem);
                        ListaOrdem.RemoveAt(i);

                        criarOrdemResidualNovoPreco(ordem, obterPrecoCompraBRL());

                    }
                }
                else if (ordem.action == "sell")
                {
                    temOV = true;
                    decimal precoVendaBRL = obterPrecoVendaBRL();
                    decimal precoOrdemAtual = Convert.ToDecimal(ordem.price);
                    decimal limitePrecoVendaPermitido = precoVendaBRL - (precoVendaBRL * 0.1M / 100);
                    if (precoOrdemAtual < limitePrecoVendaPermitido)
                    {
                        api.cancelarOrdem(ordem);
                        ListaOrdem.RemoveAt(i);

                        criarOrdemResidualNovoPreco(ordem, obterPrecoVendaBRL());

                    }
                }
            }

            if (!temOC)
            {
                temOC = false;
                criarOrdemCompraArbitragem();                
            }
            if (!temOV)
            {
                temOV = false;
                criarOrdemVendaArbitragem();                
            }

            Console.WriteLine("Fim do ciclo arbitragem, qtd ordens :" + ListaOrdem.Count);
            System.Threading.Thread.Sleep(3000);
            monitorarArbitragem();
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
                        api.imprimirOrdem("Nova ordem encontrada:", ordem);
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
                    if (ordem.id == cOrdem.id  &&
                        cOrdem.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_CANCELADA)
                    {
                        Console.WriteLine("Ordem cancelada no site ID: " + ordem.id);
                        remover = true;
                        break;
                    }
                    if (ordem.id == cOrdem.id 
                        && cOrdem.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_EXECUTADA)
                    {
                        Console.WriteLine("Ordem executada ID: " + ordem.id);
                        remover = true;
                        break;
                    }
                }
                if (remover)
                {
                   
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

        private void criarOrdemResidualNovoPreco(Ordem ordem, decimal preco)
        {
            Console.WriteLine("Criando ordem residual novo preco");
            decimal amount = Convert.ToDecimal(ordem.amount) - Convert.ToDecimal(ordem.executedAmount);
            OrderList orderList = null;
            if (ordem.action == "buy")
            {
                orderList = api.criarOrdemCompra(Convert.ToDecimal(amount), preco);
            }
            else if (ordem.action == "sell")
            {
                orderList = api.criarOrdemVenda(Convert.ToDecimal(amount), preco);
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
