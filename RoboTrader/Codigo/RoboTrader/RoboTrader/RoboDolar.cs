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
    public class RoboDolar
    {

        
        public OrderList ListaOrdem = new OrderList();
        
        public decimal margemLucro = 0.1M;

        public decimal quantidadeMaxima = 1M;

        public decimal spreadCompra = 1M;

        public decimal spreadVenda = 5M;

        public decimal quantidadeOrdem = 0.1M;

        public decimal variacaoPermitida = 0.1M;

        private APIWrapper api;

        public RoboDolar()
        {
            Console.WriteLine("Robo Dolar is alive!");
            ListaOrdem.oReturn = new List<Ordem>();
            try
            {
                api = new APIWrapper(Constantes.NOME_ROBO_DOLAR);
                OrderList ordens = api.resgatarOrderListLocal();
                popularListaOrdem(ordens);


                criaOrdensIniciais();



                //monitorarArbitragem();
                monitorarOrdens();
            }
            catch (Exception aException)
            {

                Console.WriteLine(aException.Message);
            }
        }

        private void popularListaOrdem(OrderList ordens)
        {
            if (ordens.oReturn != null && ordens.oReturn.Count > 0)
            {
                foreach (Ordem ordem in ordens.oReturn)
                {
                    if (ordem.tipoRobo.Equals(Constantes.NOME_ROBO_DOLAR))
                    {
                        ListaOrdem.oReturn.Add(ordem);
                    }
                }
            }
        }

        private void atualizarListaOrdens(OrderList ordens)
        {
            foreach (Ordem ordem in ordens.oReturn)
            {
                ordem.tipoRobo = Constantes.NOME_ROBO_DOLAR;

                ListaOrdem.oReturn.Add(ordem);

            }

            api.armazenarOrderList(this.ListaOrdem);
        }

        public void criaOrdensIniciais()
        {
            if (ListaOrdem.oReturn.Count < 2)
            {

                if (api.saldo.saldoBRL > 0 || api.saldo.saldoBTC > 0)
                {
                    TickerBitstamp bitstamp = api.obterTickerBitstamp();
                    decimal dolar = api.obterCotacaoDolar();
                    decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
                    OrderList orderList = null;

                    bool criarOC = true;
                    bool criarOV = true;
                    if (ListaOrdem.oReturn.Count == 1)
                    {
                        if (ListaOrdem.oReturn[0].action == Constantes.TIPO_ORDEM_COMPRA)
                        {
                            criarOC = false;
                        }
                        if (ListaOrdem.oReturn[0].action == Constantes.TIPO_ORDEM_VENDA)
                        {
                            criarOV = false;
                        }

                    }

                    if (criarOC)
                    {
                        decimal precoCompra = ((precoBitstamp * spreadCompra / 100) + precoBitstamp) * dolar;
                        if (api.saldo.saldoBRL > quantidadeOrdem * precoCompra)
                        {
                            orderList = api.criarOrdemCompra(quantidadeOrdem, precoCompra);
                            atualizarListaOrdens(orderList);

                        }
                        else
                        {
                            Console.WriteLine("Sem saldo BRL");
                        }
                    }

                    if (criarOV)
                    {

                        if (api.saldo.saldoBTC > quantidadeOrdem)
                        {
                            decimal precoVenda = ((precoBitstamp * spreadVenda / 100) + precoBitstamp) * dolar;
                            orderList = api.criarOrdemVenda(quantidadeOrdem, precoVenda);
                            atualizarListaOrdens(orderList);
                        }
                        else
                        {
                            Console.WriteLine("Sem saldo BTC");
                        }
                    }

                }
            }
        }

        private void criarOrdemVendaArbitragem()
        {
            try
            {
                TickerBitstamp bitstamp = api.obterTickerBitstamp();
                decimal dolar = api.obterCotacaoDolar();
                decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
                decimal precoVenda = ((precoBitstamp * spreadVenda / 100) + precoBitstamp) * dolar;
                api.criarOrdemVenda(quantidadeOrdem, precoVenda);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void criarOrdemCompraArbitragem()
        {
            try
            {
                TickerBitstamp bitstamp = api.obterTickerBitstamp();
                decimal dolar = api.obterCotacaoDolar();
                decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
                decimal precoCompra = ((precoBitstamp * spreadCompra / 100) + precoBitstamp) * dolar;
                api.criarOrdemCompra(quantidadeOrdem, precoCompra);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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

        private void deletarOrdem(Ordem ordem, int i)
        {
            api.cancelarOrdem(ordem);
            ListaOrdem.oReturn.RemoveAt(i);
            atualizarListaOrdens(ListaOrdem);

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
            Console.WriteLine("Inicio monitorando ordens Arbitragem: " + ListaOrdem.oReturn.Count);
            for (int i = ListaOrdem.oReturn.Count - 1; i >= 0; --i)
            {
                Ordem ordem = ListaOrdem.oReturn[i];
                api.imprimirOrdem("", ordem);
                if (ordem.action == Constantes.TIPO_ORDEM_COMPRA)
                {
                    temOC = true;
                    decimal precoCompraBRL = obterPrecoCompraBRL();
                    decimal precoOrdemAtual = Convert.ToDecimal(ordem.price);
                    decimal limitePrecoCompraPermitido = (precoCompraBRL * 0.1M / 100) + precoCompraBRL;

                    if (precoOrdemAtual > limitePrecoCompraPermitido)
                    {
                        deletarOrdem(ordem, i);

                        criarOrdemResidualNovoPreco(ordem, obterPrecoCompraBRL());

                    }
                }
                else if (ordem.action == Constantes.TIPO_ORDEM_VENDA)
                {
                    temOV = true;
                    decimal precoVendaBRL = obterPrecoVendaBRL();
                    decimal precoOrdemAtual = Convert.ToDecimal(ordem.price);
                    decimal limitePrecoVendaPermitido = precoVendaBRL - (precoVendaBRL * 0.1M / 100);
                    if (precoOrdemAtual < limitePrecoVendaPermitido)
                    {
                        deletarOrdem(ordem, i);

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

            Console.WriteLine("Fim do ciclo arbitragem, qtd ordens :" + ListaOrdem.oReturn.Count);
            System.Threading.Thread.Sleep(3000);
            monitorarArbitragem();
        }


        public void monitorarOrdens()
        {
            //popularListaOrdens();
            Console.WriteLine("Inicio monitorando ordens: " + ListaOrdem.oReturn.Count);
            for (int i = ListaOrdem.oReturn.Count - 1; i >= 0; --i)
            {
                Ordem ordem = ListaOrdem.oReturn[i];
                Ordem cOrdem = api.obterOrdemPorID(ordem.id);
                api.imprimirOrdem("Monitorando: ", cOrdem);
                if (cOrdem.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_EXECUTADA)
                {
                    Console.Beep(); Console.Beep(); Console.Beep();
                    Console.WriteLine("Ordem executada :" + cOrdem.id);
                    ordem.status = Constantes.STATUS_ORDEM_EXECUTADA;

                    criarOrdemInversa(cOrdem);

                    ListaOrdem.oReturn.RemoveAt(i);

                }
                else if (cOrdem.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_PENDENTE)
                {
                    if (Convert.ToDecimal(cOrdem.executedAmount) > 0)
                    {
                        Console.Beep();
                        Console.WriteLine("Ordem parcialmente executada :" + cOrdem.id);
                        deletarOrdem(cOrdem, i);
                        criarOrdemInversa(cOrdem);
                        criarOrdemResidual(cOrdem);
                        ListaOrdem.oReturn.RemoveAt(i);

                    }
                    else
                    {
                        verificarPrecoDolar(ordem, i);
                    }
                }

                verificarPrecoDolar(cOrdem, i);




            }

            Console.WriteLine("Fim do ciclo, qtd ordens :" + ListaOrdem.oReturn.Count);
            System.Threading.Thread.Sleep(3000);
            monitorarOrdens();
        }

        private void verificarPrecoDolar(Ordem ordem, int i)
        {
            bool temOC = false;
            bool temOV = false;
            if (ordem.action == Constantes.TIPO_ORDEM_COMPRA)
            {
                temOC = true;
                decimal precoCompraBRL = obterPrecoCompraBRL();
                decimal precoOrdemAtual = Convert.ToDecimal(ordem.price);
                decimal limitePrecoCompraPermitido = (precoCompraBRL * 0.1M / 100) + precoCompraBRL;

                if (precoOrdemAtual > limitePrecoCompraPermitido)
                {
                    deletarOrdem(ordem, i);
                    ListaOrdem.oReturn.RemoveAt(i);

                    criarOrdemResidualNovoPreco(ordem, obterPrecoCompraBRL());

                }
            }
            else if (ordem.action == Constantes.TIPO_ORDEM_VENDA)
            {
                temOV = true;
                decimal precoVendaBRL = obterPrecoVendaBRL();
                decimal precoOrdemAtual = Convert.ToDecimal(ordem.price);
                decimal limitePrecoVendaPermitido = precoVendaBRL - (precoVendaBRL * 0.1M / 100);
                if (precoOrdemAtual < limitePrecoVendaPermitido)
                {
                    deletarOrdem(ordem, i);
                    ListaOrdem.oReturn.RemoveAt(i);

                    criarOrdemResidualNovoPreco(ordem, obterPrecoVendaBRL());

                }
            }


        }

        private void popularListaOrdens()
        {

            Console.WriteLine("Buscando ordens pendentes...");
            OrderList orderList = api.obterOrdensPendentes();
            //ListaOrdem.oReturn.Clear();
            foreach (Ordem ordem in orderList.oReturn)
            {
                bool add = true;

                if (ordem.status == Constantes.STATUS_ORDEM_PENDENTE &&
                   Convert.ToDecimal(ordem.amount) <= quantidadeMaxima)
                {
                    foreach (Ordem cOrdem in ListaOrdem.oReturn)
                    {
                        if (ordem.id == cOrdem.id)
                        {
                            add = false;
                        }
                    }
                    if (add)
                    {
                        api.imprimirOrdem("Nova ordem encontrada:", ordem);
                        ListaOrdem.oReturn.Add(ordem);
                    }

                }


            }

            for (int i = ListaOrdem.oReturn.Count - 1; i >= 0; --i)
            {
                bool remover = false;
                Ordem ordem = ListaOrdem.oReturn[i];
                foreach (Ordem cOrdem in orderList.oReturn)
                {
                    if (ordem.id == cOrdem.id &&
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

                    ListaOrdem.oReturn.RemoveAt(i);
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
            if (ordem.action == Constantes.TIPO_ORDEM_COMPRA)
            {
                orderList = api.criarOrdemCompra(Convert.ToDecimal(amount), Convert.ToDecimal(ordem.price));
            }
            else if (ordem.action == Constantes.TIPO_ORDEM_VENDA)
            {
                orderList = api.criarOrdemVenda(Convert.ToDecimal(amount), Convert.ToDecimal(ordem.price));
            }

            atualizarListaOrdens(orderList);
        }

        private void criarOrdemResidualNovoPreco(Ordem ordem, decimal preco)
        {
            try
            {
                Console.WriteLine("Criando ordem residual novo preco");
                decimal amount = Convert.ToDecimal(ordem.amount) - Convert.ToDecimal(ordem.executedAmount);
                OrderList orderList = null;
                if (ordem.action == Constantes.TIPO_ORDEM_COMPRA)
                {
                    orderList = api.criarOrdemCompra(Convert.ToDecimal(amount), preco);
                }
                else if (ordem.action == Constantes.TIPO_ORDEM_VENDA)
                {
                    orderList = api.criarOrdemVenda(Convert.ToDecimal(amount), preco);
                }

                foreach (Ordem novaOrdem in orderList.oReturn)
                {
                    ListaOrdem.oReturn.Add(novaOrdem);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        private void criarOrdemInversa(Ordem ordem)
        {
            Console.WriteLine("Criando ordem inversa:");
            OrderList orderList = null;
            if (ordem.action == Constantes.TIPO_ORDEM_COMPRA)
            {
                orderList = api.criarOrdemVenda(Convert.ToDecimal(ordem.amount), calcularPrecoVenda(Convert.ToDecimal(ordem.price)));
            }
            else if (ordem.action == Constantes.TIPO_ORDEM_VENDA)
            {
                orderList = api.criarOrdemCompra(Convert.ToDecimal(ordem.amount), calcularPrecoCompra(Convert.ToDecimal(ordem.price)));
            }

            atualizarListaOrdens(orderList);
        }







    }
}
