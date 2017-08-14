using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboTrader
{
    /*
     Luta para ter a melhor ordem de compra e venda.
         */
    public class RoboWar
    {
        public OrderList ListaOrdem = new OrderList();


        private APIWrapper api;


        public RoboWar()
        {
            Console.WriteLine("Robo war is alive!");

            ListaOrdem.oReturn = new List<Ordem>();
            try
            {
                api = new APIWrapper(Constantes.NOME_ROBO_WAR);

                resgatarOrderListLocal();
                //api.cancelarTodasOrdensPendentes();

                monitorarOrdens();
            }
            catch (Exception aException)
            {
                Console.WriteLine(aException.Message);
                Console.Beep(); Console.Beep(); Console.Beep(); Console.Beep(); Console.Beep(); Console.Beep(); Console.Beep(); Console.Beep(); Console.Beep(); Console.Beep();
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! FALHA INESPERADA !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                System.Threading.Thread.Sleep(30000);
                Console.Beep();
                RoboWar robo = new RoboWar();
            }
        }

        private void resgatarOrderListLocal()
        {
            ListaOrdem = api.resgatarOrderListLocal();
            Console.WriteLine("Monitorando " + ListaOrdem.oReturn.Count + " ordens.");


        }

        private void imprimirSaldos()
        {
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
            Console.WriteLine("Saldo BRL total: " + (saldoBRLordens + api.saldo.saldoBRL));
            Console.WriteLine("Saldo BTC total: " + (saldoBTCordens + api.saldo.saldoBTC));
        }

        private void monitorarOrdens()
        {
            imprimirSaldos();
            Console.WriteLine(" ");
            monitorar();
            //resgatarOrderListLocal();

            Console.WriteLine("****************************************** FIM ******************************************");
            //System.Threading.Thread.Sleep(1000);
            monitorarOrdens();
        }

        private bool menorPrecoVendaEhMeu(OrderBook book, Ordem ordemAtualizada)
        {
            double melhorPrecoVenda = book.asks[0][0];

            if (ordemAtualizada.obterPreco() <= melhorPrecoVenda)
            {
                return true;
            }
            else//verificar se as ordens menores são minhas também
            {
                double somaQuantidade = 0;
                for (int i = 0; i < book.asks.Count; i++)
                {
                    double quantidadeOrdemBook = book.asks[i][1];
                    somaQuantidade = somaQuantidade + quantidadeOrdemBook;//somo as qtds até chegar em 0.001
                    if (somaQuantidade >= 0.001)//percorre até a soma das qtd for maior que 0.001
                    {
                        double precoOrderBook = book.asks[i][0];

                        bool ehMinhaOrdem = false;
                        foreach (Ordem minhaOrdem in ListaOrdem.oReturn)//verifica se é outra ordem minha
                        {
                            if (precoOrderBook == minhaOrdem.obterPreco())
                            {
                                ehMinhaOrdem = true;
                                return true;
                            }
                        }

                        if (!ehMinhaOrdem &&
                            ordemAtualizada.precoLimite < precoOrderBook)
                        {
                            return false;
                        }

                    }
                }
            }

            return false;
            
        }

        private bool melhorPrecoCompraEhMeu(OrderBook book, Ordem ordemAtualizada)
        {
            double melhorPrecoVenda = book.bids[0][0];

            if (ordemAtualizada.obterPreco() >= melhorPrecoVenda)
            {
                return true;
            }
            else//verificar se as ordens menores são minhas também
            {
                double somaQuantidade = 0;
                for (int i = 0; i < book.bids.Count; i++)
                {
                    double quantidadeOrdemBook = book.bids[i][1];
                    somaQuantidade = somaQuantidade + quantidadeOrdemBook;//somo as qtds até chegar em 0.001
                    if (somaQuantidade >= 0.001)//percorre até a soma das qtd for maior que 0.001
                    {
                        double precoOrderBook = book.bids[i][0];

                        bool ehMinhaOrdem = false;
                        foreach (Ordem minhaOrdem in ListaOrdem.oReturn)//verifica se é outra ordem minha
                        {
                            if (precoOrderBook == minhaOrdem.obterPreco())
                            {
                                ehMinhaOrdem = true;
                                return true;//melhor ordem é outra ordem minha, não precisa criar ordem, tudo ok.
                            }
                        }

                        if (!ehMinhaOrdem &&
                            ordemAtualizada.precoLimite > precoOrderBook)
                        {
                            return false;//criar nova ordem
                        }

                    }
                }
            }

            return false;

        }

        //private bool melhorPrecoCompraEhMeu(OrderBook book, Ordem ordem)
        //{
        //    double melhorPrecoCompra = book.bids[0][0];
        //    if (melhorPrecoCompra > ordem.precoLimite)
        //    {
        //        double somaQuantidade = 0;
        //        for (int i = 0; i < book.bids.Count; i++)
        //        {
        //            double quantidadeOrdemBook = book.bids[i][1];
        //            somaQuantidade = somaQuantidade + quantidadeOrdemBook;//somo as qtds até chegar em 0.001
        //            if (somaQuantidade >= 0.001)//percorre até a soma das qtd for maior que 0.001
        //            {
        //                double precoOrdemBook = book.bids[i][0];
        //                if (precoOrdemBook <= ordem.precoLimite)
        //                {
        //                    melhorPrecoCompra = precoOrdemBook;
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    foreach (Ordem cOrdem in ListaOrdem.oReturn)
        //    {
        //        if (cOrdem.action == Constantes.TIPO_ORDEM_COMPRA &&
        //            cOrdem.obterPreco() >= melhorPrecoCompra &&
        //            cOrdem.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_PENDENTE)
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        private bool meuPrecoJaEstaNoLimite(OrderBook book, Ordem ordem)
        {

            if (ordem.obterPreco() == ordem.precoLimite)
            {
                return true;//nao fazer nada
            }

            return false;
        }



        private void imprimirComparativo(Ordem ordem, double melhorPreco)
        {
            if (ordem.obterPreco() >= melhorPreco &&
               ordem.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_PENDENTE &&
               ordem.action == Constantes.TIPO_ORDEM_COMPRA)
            {
                Console.WriteLine("Meu preço OC: " + ordem.obterPreco() + " Melhor " + melhorPreco + " - OK, tenho o melhor preço de compra");
            }
            else if (ordem.obterPreco() <= melhorPreco &&
               ordem.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_PENDENTE &&
               ordem.action == Constantes.TIPO_ORDEM_VENDA)
            {
                Console.WriteLine("Meu preço OV: " + ordem.obterPreco() + " Melhor " + melhorPreco + " - OK, tenho o melhor preço de venda");
            }

        }

        private void monitorar()
        {
            bool criarOrdemCompra = true;
            bool criarOrdemVenda = true;
            OrderBook book = api.obterLivroOrdens();//TODO: ACABEI DE TIRAR DO FOR
            //verifica se tem ordem minha criada
            if (ListaOrdem != null && ListaOrdem.oReturn != null && ListaOrdem.oReturn.Count > 0)
            {

                double melhorPrecoCompra = book.bids[0][0];
                double melhorPrecoVenda = book.asks[0][0];

                for (int i = ListaOrdem.oReturn.Count - 1; i >= 0; --i)
                {

                    Ordem ordemAtualizada = api.obterOrdemPorID(ListaOrdem.oReturn[i].id);
                    ordemAtualizada.precoLimite = ListaOrdem.oReturn[i].precoLimite;
                    ordemAtualizada.ordemPai = ListaOrdem.oReturn[i].ordemPai;
                    if (ordemAtualizada.action == Constantes.TIPO_ORDEM_COMPRA)
                    {
                        criarOrdemCompra = false;
                        imprimirComparativo(ordemAtualizada, melhorPrecoCompra);
                        //TODO: TROCAR != POR < E REVER LOGICA ABAIXO
                        bool executada = verificarSeFoiExecutada(book, ordemAtualizada, i);//trata se foi executada

                        if (!executada && !melhorPrecoCompraEhMeu(book, ordemAtualizada) && !meuPrecoJaEstaNoLimite(book, ordemAtualizada))//verifica se minha OC pendente é a melhor
                        {
                            if (ordemAtualizada.ordemPai)
                            {
                                ordemAtualizada.precoLimite = (melhorPrecoVenda - api.parametros.spreadCompraVenda);
                            }
                            cancelarOrdem(ordemAtualizada, i);
                            criarOrdemCompraNova(book, ordemAtualizada);
                        }
                    }
                    else if (ordemAtualizada.action == Constantes.TIPO_ORDEM_VENDA)
                    {
                        criarOrdemVenda = false;
                        imprimirComparativo(ordemAtualizada, melhorPrecoVenda);
                        bool executada = verificarSeFoiExecutada(book, ordemAtualizada, i);//trata se foi executada

                        if (!executada && !menorPrecoVendaEhMeu(book, ordemAtualizada) && !meuPrecoJaEstaNoLimite(book, ordemAtualizada))//verifica se minha OC pendente é a melhor
                        {
                            if (ordemAtualizada.ordemPai)
                            {
                                ordemAtualizada.precoLimite = (melhorPrecoCompra + api.parametros.spreadCompraVenda);
                            }
                            cancelarOrdem(ordemAtualizada, i);
                            criarOrdemVendaNova(book, ordemAtualizada);
                        }
                    }
                }

            }
            else
            {
                criarOrdemCompra = false;
                criarOrdemVenda = false;
                inicializar();
            }

            if (criarOrdemCompra)
            {
                criarOrdemCompra = false;
                inicializarOrdemCompra(book);
            }
            if (criarOrdemVenda)
            {
                criarOrdemVenda = false;
                inicializarOrdemVenda(book);
            }
        }


        private void inicializar()
        {
            OrderBook book = api.obterLivroOrdens();
            inicializarOrdemCompra(book);
            inicializarOrdemVenda(book);
        }

        private void inicializarOrdemVenda(OrderBook book)
        {
            double melhorPrecoCompra = book.bids[0][0];
            Ordem ov = new Ordem();
            ov.amount = gerarQuantidadeAleatoria();
            ov.precoLimite = (melhorPrecoCompra - api.parametros.spreadCompraVenda);
            ov.ordemPai = true;
            criarOrdemVendaNova(book, ov);

        }

        private void inicializarOrdemCompra(OrderBook book)
        {
            double melhorPrecoVenda = book.asks[0][0];
            Ordem oc = new Ordem();
            oc.amount = gerarQuantidadeAleatoria();
            oc.precoLimite = (melhorPrecoVenda - api.parametros.spreadCompraVenda);
            oc.ordemPai = true;
            criarOrdemCompraNova(book, oc);

        }

        private string gerarQuantidadeAleatoria()
        {
            double quantidade = api.parametros.obterQuantidadeOrdem();
            double variacao = quantidade * 5 / 100;
            double maximum = quantidade + variacao;
            double minimum = quantidade - variacao;

            Random random = new Random();
            quantidade = random.NextDouble() * (maximum - minimum) + minimum;


            return Convert.ToString(quantidade);
        }

        private bool verificarSeFoiExecutada(OrderBook book, Ordem ordemAtualizada, int i)
        {

            if (ordemAtualizada.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_EXECUTADA)
            {
                Console.Beep(); Console.Beep(); Console.Beep();
                Console.WriteLine("Ordem executada :" + ordemAtualizada.id);
                removerOrdem(ordemAtualizada, i);
                inicializarOrdemCompra(book);
                criarOrdemInversa(book, ordemAtualizada);
                api.atualizarSaldoOrdemExecutada(ordemAtualizada);
                return true;
            }
            else if (ordemAtualizada.status.ToUpper().Trim() == Constantes.STATUS_ORDEM_PENDENTE)
            {
                if (Convert.ToDecimal(ordemAtualizada.executedAmount) > 0)
                {
                    Console.Beep();
                    Console.WriteLine("Ordem parcialmente executada :" + ordemAtualizada.id);
                    cancelarOrdem(ordemAtualizada, i);
                    criarOrdemResidual(book, ordemAtualizada);
                    criarOrdemInversa(book, ordemAtualizada);
                    api.atualizarSaldoOrdemExecutada(ordemAtualizada);
                    return true;
                }

            }
            return false;
        }

        private void removerOrdem(Ordem ordem, int i)
        {
            Console.WriteLine("Removendo ordem ID: " + ordem.id);
            ListaOrdem.oReturn.RemoveAt(i);
            api.armazenarOrderList(this.ListaOrdem);
        }



        private decimal calcularPrecoCompra(decimal v)
        {
            throw new NotImplementedException();
        }





        private void criarOrdemInversa(OrderBook book, Ordem ordemExecutada)
        {
            try
            {
                Console.WriteLine("Criando ordem inversa");
                OrderList orderList = null;
                double precoLimite = 0;
                ordemExecutada.definirQuantidade(ordemExecutada.obterQuantidadeExecutada());
                if (ordemExecutada.action == Constantes.TIPO_ORDEM_COMPRA)
                {
                    ordemExecutada.precoLimite = ordemExecutada.obterPreco() + api.parametros.minimoLucroBRL;
                    criarOrdemVendaNova(book, ordemExecutada);
                }
                else if (ordemExecutada.action == Constantes.TIPO_ORDEM_VENDA)
                {
                    ordemExecutada.precoLimite = Convert.ToDouble(ordemExecutada.price) - api.parametros.minimoLucroBRL;
                    criarOrdemCompraNova(book, ordemExecutada);
                }

                //atualizarListaOrdens(orderList, precoLimite);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Erro ao criar ordem inversa " + ex.Message);
                Console.WriteLine(" ");
            }
        }





        private void criarOrdemResidual(OrderBook book, Ordem ordem)
        {
            try
            {
                Console.WriteLine("Criando ordem de trade residual:");
                ordem.definirQuantidade(ordem.obterQuantidade() - ordem.obterQuantidadeExecutada());
                OrderList orderList = null;
                if (ordem.action == Constantes.TIPO_ORDEM_COMPRA)
                {
                    criarOrdemCompraNova(book, ordem);
                }
                else if (ordem.action == Constantes.TIPO_ORDEM_VENDA)
                {
                    criarOrdemVendaNova(book, ordem);
                }

                // atualizarListaOrdens(orderList, ordem.precoLimite);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao criar ordem residual " + ex.Message);
            }
        }


        private void cancelarOrdem(Ordem ordem, int i)
        {
            api.cancelarOrdem(ordem);
            ListaOrdem.oReturn.RemoveAt(i);
            api.armazenarOrderList(this.ListaOrdem);
        }



        private void criarOrdemCompraNova(OrderBook book, Ordem ordemModelo)
        {
            double somaQuantidade = 0;
            for (int i = 0; i < book.bids.Count; i++)
            {
                double quantidadeOrdemBook = book.bids[i][1];
                somaQuantidade = somaQuantidade + quantidadeOrdemBook;//somo as qtds até chegar em 0.001
                if (somaQuantidade >= 0.001)//percorre até a soma das qtd for maior que 0.001
                {
                    double precoOrdemBook = book.bids[i][0];
                    if (precoOrdemBook <= ordemModelo.precoLimite)
                    {
                        precoOrdemBook = precoOrdemBook + api.parametros.incrementoOrdem;
                        criarOrdemCompra(precoOrdemBook, ordemModelo.obterQuantidade(), ordemModelo.precoLimite);
                        break;
                    }
                }
            }
        }

        private void criarOrdemVendaNova(OrderBook book, Ordem ordemModelo)
        {
            double somaQuantidade = 0;
            for (int i = 0; i < book.bids.Count; i++)
            {
                double quantidadeOrdemBook = book.asks[i][1];
                somaQuantidade = somaQuantidade + quantidadeOrdemBook;//somo as qtds até chegar em 0.001
                if (somaQuantidade >= 0.001)//percorre até a soma das qtd for maior que 0.001
                {
                    double precoOrdemBook = book.asks[i][0];
                    if (precoOrdemBook >= ordemModelo.precoLimite)
                    {
                        precoOrdemBook = precoOrdemBook - api.parametros.incrementoOrdem;
                        criarOrdemVenda(precoOrdemBook, ordemModelo.obterQuantidade(), ordemModelo.precoLimite);
                        break;
                    }
                }
            }
        }

        private void criarOrdemVenda(double preco, double quantidade, double precoLimite)
        {
            try
            {
                Console.WriteLine("Criando ordem venda");
                OrderList orderList = null;
                orderList = api.criarOrdemVenda(Convert.ToDecimal(quantidade), Convert.ToDecimal(preco), Constantes.TIPO_ORDEM_WAR);
                atualizarListaOrdens(orderList, precoLimite);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Erro ao criar ordem de commpra " + ex.Message);
                Console.WriteLine(" ");
            }
        }

        private void criarOrdemCompra(double preco, double quantidade, double precoLimite)
        {
            try
            {
                Console.WriteLine("Criando ordem compra");
                OrderList orderList = null;
                orderList = api.criarOrdemCompra(Convert.ToDecimal(quantidade), Convert.ToDecimal(preco), Constantes.TIPO_ORDEM_WAR);
                atualizarListaOrdens(orderList, precoLimite);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Erro ao criar ordem de commpra " + ex.Message);
                Console.WriteLine(" ");
            }
        }


        private void atualizarListaOrdens(OrderList ordens, double precoLimite)
        {
            foreach (Ordem ordem in ordens.oReturn)
            {
                ordem.precoLimite = precoLimite;
                ListaOrdem.oReturn.Add(ordem);
            }
            api.armazenarOrderList(this.ListaOrdem);
        }



    }
}
