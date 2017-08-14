using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboTrader
{
    ///<summary>
    ///Este robo cria ordem de compra e venda baseado
    ///no dolar comercial * bitstamp * porcentagem definiada pelo usuário
    ///Se executadas, cria ordem inversa (compra ou venda) novamente
    ///</summary>
   
    public class Robo3
    {

        
        public OrderList ListaOrdem = new OrderList();

        private Parametros parametros = new Parametros();

        private APIWrapper api;

        
        public Robo3()
        {
            Console.WriteLine("Robo v3 is alive!");

            ListaOrdem.oReturn = new List<Ordem>();
            //try
            //{
            api = new APIWrapper(Constantes.NOME_ROBO_V3);
            
            //api.cancelarTodasOrdensPendentes();
            monitorarOrdens();
            //}
            //catch (Exception aException)
            //{

            //    Console.WriteLine(aException.Message);
            //}
        }

        private void monitorarOrdens()
        {
            Console.WriteLine("Saldo BRL: " + api.saldo.saldoBRL + " saldo BTC " + api.saldo.saldoBTC);
            Console.WriteLine(" ");
            resgatarOrderListLocal();
            verificarOrdensDolar();
            Console.WriteLine("****************************************** FIM ******************************************");
            System.Threading.Thread.Sleep(10000);
            monitorarOrdens();
        }

        private void resgatarOrderListLocal()
        {
            ListaOrdem = api.resgatarOrderListLocal();
            Console.WriteLine("Monitorando " + ListaOrdem.oReturn.Count + " ordens.");
            decimal saldoBRLordens = 0;
            decimal saldoBTCordens = 0;
            foreach (Ordem ordem in ListaOrdem.oReturn)
            {
                if (ordem.action == Constantes.TIPO_ORDEM_COMPRA)
                {
                    saldoBRLordens = saldoBRLordens + (Convert.ToDecimal(ordem.amount) * Convert.ToDecimal(ordem.price));
                }
                else
                {
                    saldoBTCordens = saldoBTCordens + (Convert.ToDecimal(ordem.amount));
                }
            }
            Console.WriteLine("Saldo BRL em ordens: " + saldoBRLordens);
            Console.WriteLine("Saldo BTC em ordens: " + saldoBTCordens);
        }
        
        private void verificarOrdensDolar()
        {
            bool criarOCDolar = true;
            bool criarOVDolar = true;
            if (null != ListaOrdem.oReturn && ListaOrdem.oReturn.Count != 0)
            {
                for (int i = ListaOrdem.oReturn.Count - 1; i >= 0; --i)
                {
                    Ordem ordemAtualizada = api.obterOrdemPorID(ListaOrdem.oReturn[i].id);
                    Ordem ordemLocal = ListaOrdem.oReturn[i];
                    if (ordemLocal.tipoRobo.Equals(Constantes.TIPO_ORDEM_DOLAR))
                    {
                        tratarOrdemDolar(ref criarOCDolar, ref criarOVDolar, ordemAtualizada, i);

                    }
                    else if (ordemLocal.tipoRobo.Equals(Constantes.TIPO_ORDEM_TRADE))
                    {
                        tratarOrdemTrade(ordemAtualizada, i);
                    }
                }
            }
            else
            {
                criarOrdemCompraDolar();
                criarOCDolar = false;
                criarOrdemVendaDolar();
                criarOVDolar = false;
            }
            if (criarOCDolar)
            {
                criarOrdemCompraDolar();
            }
            if (criarOVDolar)
            {
                criarOrdemVendaDolar();
            }
        }

        private void tratarOrdemDolar(ref bool criarOCDolar, ref bool criarOVDolar, Ordem ordemAtualizada, int i)
        {
            if (ordemAtualizada.action == Constantes.TIPO_ORDEM_COMPRA)
            {
                criarOCDolar = false;
            }
            else if (ordemAtualizada.action == Constantes.TIPO_ORDEM_VENDA)
            {
                criarOVDolar = false;
            }
            api.imprimirOrdem("Ordem dolar encontrada", ordemAtualizada);
            if (ordemAtualizada.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_CANCELADA)
            {
                removerOrdem(ordemAtualizada, i);
                criarOrdemDolarDoMesmoTipo(ordemAtualizada);
            }
            else if (ordemAtualizada.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_EXECUTADA)
            {
                removerOrdem(ordemAtualizada, i);
                criarOrdemDolarDoMesmoTipo(ordemAtualizada);
                criarOrdemInversa(ordemAtualizada);
                api.atualizarSaldoOrdemExecutada(ordemAtualizada);
            }
            else if (ordemAtualizada.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_PENDENTE &&
                Convert.ToDecimal(ordemAtualizada.executedAmount) > 0)
            {
                cancelarOrdem(ordemAtualizada, i);
                criarOrdemDolarDoMesmoTipo(ordemAtualizada);//Criar nova ordem dolar cheia do mesmo tipo
                criarOrdemInversaResidual(ordemAtualizada);//criar ordem inversa de trade  
                api.atualizarSaldoOrdemExecutada(ordemAtualizada);
            }
            else
            {
                ajustarPrecoDolar(ordemAtualizada, i);

            }
        }

        private void tratarOrdemTrade(Ordem ordemAtualizada, int i)
        {
            api.imprimirOrdem("Ordem trade encontrada", ordemAtualizada);
            if (ordemAtualizada.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_EXECUTADA)
            {
                Console.Beep(); Console.Beep(); Console.Beep();
                Console.WriteLine("Ordem executada :" + ordemAtualizada.id);
                removerOrdem(ordemAtualizada, i);
                criarOrdemInversa(ordemAtualizada);
                api.atualizarSaldoOrdemExecutada(ordemAtualizada);
            }
            else if (ordemAtualizada.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_PENDENTE)
            {
                if (Convert.ToDecimal(ordemAtualizada.executedAmount) > 0)
                {
                    Console.Beep();
                    Console.WriteLine("Ordem parcialmente executada :" + ordemAtualizada.id);
                    cancelarOrdem(ordemAtualizada, i);
                    criarOrdemResidual(ordemAtualizada);
                    criarOrdemInversaResidual(ordemAtualizada);
                    api.atualizarSaldoOrdemExecutada(ordemAtualizada);
                }
            }
        }

        private void ajustarPrecoDolar(Ordem ordemAtualizada, int i)
        {
            Console.WriteLine("Ajustando preço baseado no dólar e bitstamp...");
            if (ordemAtualizada.action == "buy")
            {
                decimal precoOrdemAtual = Convert.ToDecimal(ordemAtualizada.price);
                if (precoOrdemAtual > obterLimitePrecoCompra())
                {
                    Console.WriteLine("---------- Ajustando preço OC ----------");
                    cancelarOrdem(ordemAtualizada, i);
                    criarOrdemCompraDolar();
                }
            }
            else if (ordemAtualizada.action == "sell")
            {
                decimal precoOrdemAtual = Convert.ToDecimal(ordemAtualizada.price);

                if (precoOrdemAtual < obterLimitePrecoVenda())
                {
                    Console.WriteLine("---------- Ajustando preço OV ----------");
                    cancelarOrdem(ordemAtualizada, i);
                    criarOrdemVendaDolar();

                }
            }
            Console.WriteLine("Fim ajuste preço baseado no dólar e bistamp");
            Console.WriteLine(" ");
        }

        private decimal obterLimitePrecoCompra()
        {
            decimal precoCompraBRL = obterPrecoCompraBRL();
            decimal limitePrecoCompraPermitido = (precoCompraBRL * 0.1M / 100) + precoCompraBRL;
            return limitePrecoCompraPermitido;

        }

        private decimal obterLimitePrecoVenda()
        {
            decimal precoVendaBRL = obterPrecoVendaBRL();
            decimal limitePrecoVendaPermitido = precoVendaBRL - (precoVendaBRL * 0.1M / 100);
            return limitePrecoVendaPermitido;

        }


        private void criarOrdemResidual(Ordem ordem)
        {
            try
            {
                Console.WriteLine("Criando ordem de trade residual:");
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
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao criar ordem residual " + ex.Message);
            }
        }
        private void criarOrdemInversa(Ordem ordem)
        {
            try
            {
                Console.WriteLine("Criando ordem de trade inversa:");
                OrderList orderList = null;
                if (ordem.action == Constantes.TIPO_ORDEM_COMPRA)
                {
                    orderList = api.criarOrdemVenda(Convert.ToDecimal(ordem.amount), calcularPrecoVenda(Convert.ToDecimal(ordem.price)), Constantes.TIPO_ORDEM_TRADE);
                }
                else if (ordem.action == Constantes.TIPO_ORDEM_VENDA)
                {
                    orderList = api.criarOrdemCompra(Convert.ToDecimal(ordem.amount), calcularPrecoCompra(Convert.ToDecimal(ordem.price)), Constantes.TIPO_ORDEM_TRADE);
                }

                atualizarListaOrdens(orderList);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Erro ao criar ordem inversa " + ex.Message);
                Console.WriteLine(" ");
            }
        }

        private void criarOrdemInversaResidual(Ordem ordem)
        {
            try
            {
                Console.WriteLine("Criando ordem de trade residual inversa:");
                OrderList orderList = null;
                //decimal amount = Convert.ToDecimal(ordem.amount) - Convert.ToDecimal(ordem.executedAmount);
                decimal amount = Convert.ToDecimal(ordem.executedAmount);

                if (ordem.action == Constantes.TIPO_ORDEM_COMPRA)
                {
                    orderList = api.criarOrdemVenda(amount, calcularPrecoVenda(Convert.ToDecimal(ordem.price)), Constantes.TIPO_ORDEM_TRADE);
                }
                else if (ordem.action == Constantes.TIPO_ORDEM_VENDA)
                {
                    orderList = api.criarOrdemCompra(amount, calcularPrecoCompra(Convert.ToDecimal(ordem.price)), Constantes.TIPO_ORDEM_TRADE);
                }

                atualizarListaOrdens(orderList);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Erro ao criar ordem inversa residual " + ex.Message);
                Console.WriteLine(" ");
            }
        }

        public decimal calcularPrecoVenda(decimal preco)
        {
            decimal aux = (preco * parametros.margemLucro) / 100;
            decimal novoPreco = preco + aux;
            return novoPreco;
        }

        public decimal calcularPrecoCompra(decimal preco)
        {
            decimal aux = (preco * parametros.margemLucro) / 100;
            decimal novoPreco = preco - aux;
            return novoPreco;
        }


        private void cancelarOrdem(Ordem ordem, int i)
        {
            api.cancelarOrdem(ordem);
            ListaOrdem.oReturn.RemoveAt(i);
            api.armazenarOrderList(this.ListaOrdem);
        }

        private void removerOrdem(Ordem ordem, int i)
        {
            Console.WriteLine("Removendo ordem ID: " + ordem.id);
            ListaOrdem.oReturn.RemoveAt(i);
            api.armazenarOrderList(this.ListaOrdem);
        }

        private void criarOrdemDolarDoMesmoTipo(Ordem pOrdem)
        {
            Console.WriteLine("Criando ordem dolar do mesmo tipo :" + pOrdem.action);
            if (pOrdem.action == Constantes.TIPO_ORDEM_COMPRA)
            {
                criarOrdemCompraDolar();
            }
            else if (pOrdem.action == Constantes.TIPO_ORDEM_VENDA)
            {
                criarOrdemVendaDolar();
            }
        }

        private decimal obterPrecoCompraBRL()
        {
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
            decimal dolar = api.obterCotacaoDolar();
            decimal precoCompra = ((precoBitstamp * parametros.spreadCompra / 100) + precoBitstamp) * dolar;
            return precoCompra;
        }

        private decimal obterPrecoCompraUSD()
        {
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);

            decimal precoCompra = ((precoBitstamp * parametros.spreadCompra / 100) + precoBitstamp);
            return precoCompra;
        }

        private decimal obterPrecoVendaBRL()
        {
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);
            decimal dolar = api.obterCotacaoDolar();
            decimal precoVenda = ((precoBitstamp * parametros.spreadVenda / 100) + precoBitstamp) * dolar;
            return precoVenda;
        }

        private decimal obterPrecoVendaUSD()
        {
            TickerBitstamp bitstamp = api.obterTickerBitstamp();
            decimal precoBitstamp = Convert.ToDecimal(bitstamp.last);

            decimal precoVenda = ((precoBitstamp * parametros.spreadVenda / 100) + precoBitstamp);
            return precoVenda;
        }

        private void criarOrdemCompraDolar()
        {
            try
            {
                Console.WriteLine("Criando ordem compra dolar");
                OrderList orderList = null;
                decimal precoCompra = obterPrecoCompraBRL();
                orderList = api.criarOrdemCompra(Convert.ToDecimal(parametros.quantidadeOrdem), precoCompra, Constantes.TIPO_ORDEM_DOLAR);
                atualizarListaOrdens(orderList);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Erro ao criar ordem de commpra dolar " + ex.Message);
                Console.WriteLine(" ");
            }
        }

        private void criarOrdemVendaDolar()//SE JA CRIAR EXECUTADA, TEM QUE CRIAR UMA DE TRADE INVERSA
        {
            try
            {
                Console.WriteLine("Criando ordem venda dolar");

                OrderList orderList = null;
                decimal precoVenda = obterPrecoVendaBRL();
                orderList = api.criarOrdemVenda(Convert.ToDecimal(parametros.quantidadeOrdem), precoVenda, Constantes.TIPO_ORDEM_DOLAR);
                atualizarListaOrdens(orderList);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Erro ao criar ordem de venda dolar " + ex.Message);
                Console.WriteLine(" ");
            }
        }


        private void atualizarListaOrdens(OrderList ordens)
        {
            foreach (Ordem ordem in ordens.oReturn)
            {
                ListaOrdem.oReturn.Add(ordem);
                //if (ordem.status.ToUpper().Trim() != Constantes.STATUS_ORDEM_EXECUTADA)
                //{
                //    Console.Beep(); Console.Beep();
                //    api.imprimirOrdem("Ordem criada já executada ", ordem);
                //}
                //if (ordem.status.ToUpper().Trim() != Constantes.STATUS_ORDEM_EXECUTADA &&
                //    ordem.status.ToUpper().Trim() != Constantes.STATUS_ORDEM_CANCELADA)
                //{
                //    ListaOrdem.oReturn.Add(ordem);
                //}
                //else
                //{
                //    Console.Beep(); Console.Beep(); Console.Beep();
                //}
            }
            api.armazenarOrderList(this.ListaOrdem);
        }


    }
}
