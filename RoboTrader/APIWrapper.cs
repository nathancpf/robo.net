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
    public class APIWrapper
    {

        public Parametros parametros { get; set; }
        public SaldoB2U saldo { get; set; }

        private string NomeRobo;

        public APIWrapper(string nomeRobo)
        {
            NomeRobo = nomeRobo;
            obterParametrosLocal();
            obterSaldoLocal();

        }
        public Ticker obterTickerB2U()
        {
            System.Threading.Thread.Sleep(1000);
            string retorno = getTicker();

            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(RootObjectTicker));
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(retorno));


            RootObjectTicker ticker = (RootObjectTicker)js.ReadObject(ms);

            return ticker.ticker;
            //return Convert.ToDecimal(balance.oReturn[0].BRL);
        }



        public TickerBitstamp obterTickerBitstamp()
        {
            System.Threading.Thread.Sleep(1000);
            string retorno = getTickerBitstamp();

            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(TickerBitstamp));
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(retorno));


            TickerBitstamp ticker = (TickerBitstamp)js.ReadObject(ms);

            return ticker;
        }

        public decimal obterCotacaoDolar()
        {
            System.Threading.Thread.Sleep(1000);
            string retorno = getDolar();

            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(RootObjectDolar));
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(retorno));


            RootObjectDolar dolar = (RootObjectDolar)js.ReadObject(ms);

            return Convert.ToDecimal(dolar.rates.BRL);
            //return Convert.ToDecimal(balance.oReturn[0].BRL);
        }






        public void salvarParametros(Parametros parametros)
        {
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Parametros));
            ser.WriteObject(stream1, parametros);
            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            salvarArquivoGenerico(sr.ReadToEnd(), Constantes.NOME_PARAMETROS);
        }

        private void salvarArquivoGenerico(string dados, string nomeArquivo)
        {
            try
            {
                string fullname = criarArquivo(nomeArquivo);
                StreamWriter sw = new StreamWriter(fullname);
                sw.WriteLine(dados);
                sw.Close();                
            }
            catch (Exception e)
            {
                log(e.Message);
            }
            finally
            {

            }
        }

        private string obterArquivoGenerico(string nomeArquivo)
        {
            try
            {
                string fullname = criarArquivo(nomeArquivo);
                StreamReader sr = new StreamReader(fullname);
                string line = sr.ReadLine();
                sr.Close();
                return line;
            }
            catch (Exception e)
            {
                log(e.Message);
                return e.Message;
            }
            
        }
        private void salvarSaldoLocal()
        {

            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SaldoB2U));
            ser.WriteObject(stream1, saldo);
            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            salvarArquivoGenerico(sr.ReadToEnd(), "SALDO");

        }
        
        private void debitarSaldoBRL(decimal valorBRL)
        {
            this.saldo.saldoBRL = this.saldo.saldoBRL - valorBRL;
            salvarSaldoLocal();
        }

        private void debitarSaldoBTC(decimal quantidade)
        {
            this.saldo.saldoBTC = this.saldo.saldoBTC - quantidade;
            salvarSaldoLocal();
        }

        

        private void obterParametrosLocal()
        {
            string dados = obterArquivoGenerico(Constantes.NOME_PARAMETROS);
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Parametros));
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(dados));
            parametros = (Parametros)js.ReadObject(ms);
            ms.Close();

        }

        private void obterSaldoLocal()
        {
            String line;
            try
            {
                line = obterArquivoGenerico("SALDO");
                
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(SaldoB2U));
                MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(line));
                this.saldo = (SaldoB2U)js.ReadObject(ms);

                ms.Close();

            }
            catch (Exception e)
            {
                log(e.Message);
                throw e;

            }
            finally
            {

            }
        }
        public decimal obterSaldoDisponivelBRL()
        {
            string nonce = obterNonce();
            string balanco = getBalance(parametros.chaveAPI, nonce, gerarAssinatura(nonce));

            MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(balanco));

            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Balance));
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(balanco));

            Balance balance = (Balance)js.ReadObject(ms);

            return Convert.ToDecimal(balance.oReturn[0].BRL);
        }

        public void atualizarSaldoOrdemExecutada(Ordem ordemAtualizada)
        {
            if (ordemAtualizada.action == Constantes.TIPO_ORDEM_COMPRA)
            {
                this.saldo.saldoBTC = this.saldo.saldoBTC + Convert.ToDecimal(ordemAtualizada.executedAmount);
            }
            else if (ordemAtualizada.action == Constantes.TIPO_ORDEM_VENDA)
            {
                this.saldo.saldoBRL = this.saldo.saldoBRL +
                    (Convert.ToDecimal(ordemAtualizada.executedAmount) * Convert.ToDecimal(ordemAtualizada.price));
            }
            salvarSaldoLocal();
        }

        public decimal obterSaldoDisponivelBTC()
        {
            string nonce = obterNonce();
            string balanco = getBalance(parametros.chaveAPI, nonce, gerarAssinatura(nonce));

            MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(balanco));

            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Balance));
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(balanco));

            Balance balance = (Balance)js.ReadObject(ms);

            return Convert.ToDecimal(balance.oReturn[0].BTC);
        }

        public string obterNonce()
        {
            String line;
            try
            {
                line = obterArquivoGenerico("nonce");
               

                string nonce = line;
                
                int iNonce = Convert.ToInt32(nonce);
                nonce = "" + (iNonce + 1);
                salvarArquivoGenerico(nonce, "nonce");
                
                return nonce;
            }
            catch (Exception e)
            {
                log(e.Message);
                return "0";

            }
            finally
            {

            }
        }

        
        private string criarArquivo(string nomeArquivo)
        {
            try
            {
                String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/RoboB2U/";
                string fileName = NomeRobo + "_" + nomeArquivo + ".txt";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!File.Exists(path + fileName))
                {

                    File.WriteAllText(path + fileName, String.Empty);
                }

                return path + fileName;


            }
            catch (Exception e)
            {

                throw e;
            }
           
        }

        public void log(string msg)
        {
            try
            {
                string fullname = criarArquivo("log");
                StreamWriter sw = new StreamWriter(fullname);                
                sw.WriteLine(msg);                
                sw.Close();
            }
            catch (Exception e)
            {
                throw e;
            }            
        }


        public Ordem obterOrdemPorID(string sIDOrdem)
        {
            Console.WriteLine("Obtendo ordem :" + sIDOrdem);
            long IDOrdem = Convert.ToInt64(sIDOrdem);
            string nonce = obterNonce();
            string retorno = getOrderId(parametros.chaveAPI, nonce, gerarAssinatura(nonce), IDOrdem);


            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(OrderList));
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(retorno));

            OrderList createOrder = (OrderList)js.ReadObject(ms);


            ms.Close();

            return createOrder.oReturn[0];
        }

        public void imprimirOrdem(string descricao, Ordem ordem)
        {
            Console.WriteLine(string.Format(descricao + " ID {0}, TIPO {1}, PRECO {2}, QTD {3}, EXEC {4}, TIPO {7}, STATUS {5}, DATA {6}",
                ordem.id, ordem.action, ordem.price, ordem.amount, ordem.executedAmount, ordem.status.ToUpper(), ordem.dateCreated, ordem.tipoRobo));
            Console.WriteLine(" ");
        }
        public OrderList criarOrdemCompra(decimal quantidade, decimal preco)
        {
            Console.WriteLine("saldo: " + saldo.saldoBRL + ", valor ordem: " + quantidade * preco);

            if (saldo.saldoBRL >= quantidade * preco)
            {

                string nonce = obterNonce();
                string retorno = CreateOrder(parametros.chaveAPI, nonce, gerarAssinatura(nonce), "BTC", "buy", quantidade, preco);


                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(OrderList));
                MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(retorno));
                OrderList orderList = (OrderList)js.ReadObject(ms);
                ms.Close();





                if (orderList.success == "1")
                {
                    foreach (Ordem ordem in orderList.oReturn)
                    {
                        ordem.tipoRobo = NomeRobo;
                        imprimirOrdem("Ordem criada", ordem);
                    }
                    armazenarOrderList(orderList);

                    debitarSaldoBRL(quantidade * preco);

                    // Console.WriteLine("Saldo.saldoBRL: " + saldo.saldoBRL);
                }
                else
                {
                    throw new Exception("Erro ao criar ordem de compra: " + retorno);
                }

                return orderList;
            }
            else
            {
                throw new Exception("Sem saldo de BRL, saldo: " + saldo.saldoBRL + ", quantidade Ordem :" + quantidade + ", preço: " + preco);
            }
        }

        public OrderList criarOrdemCompra(decimal quantidade, decimal preco, string tipo)
        {

            if (saldo.saldoBRL >= 0)
            {
                if (saldo.saldoBRL < quantidade * preco)
                {
                    quantidade = saldo.saldoBRL / preco;
                }
                string nonce = obterNonce();
                string retorno = CreateOrder(parametros.chaveAPI, nonce, gerarAssinatura(nonce), "BTC", "buy", quantidade, preco);


                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(OrderList));
                MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(retorno));
                OrderList orderList = (OrderList)js.ReadObject(ms);
                ms.Close();





                if (orderList.success == "1")
                {
                    foreach (Ordem ordem in orderList.oReturn)
                    {
                        ordem.tipoRobo = tipo;
                        imprimirOrdem("Ordem criada", ordem);
                    }

                    armazenarOrderList(orderList);

                    debitarSaldoBRL(quantidade * preco);

                    Console.WriteLine("Saldo.saldoBRL: " + saldo.saldoBRL);
                }
                else
                {
                    throw new Exception("Erro ao criar ordem de compra: " + retorno);
                }

                return orderList;
            }
            else
            {
                throw new Exception("Sem saldo de BRL, saldo: " + saldo.saldoBRL + ", quantidade Ordem :" + quantidade + ", preço: " + preco);
            }
        }

        public OrderList resgatarOrderListLocal()
        {//resgatar do arquivo
            OrderList orderList = new OrderList();

            try
            {
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(OrderList));
                MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(obterOrdensLocais()));
                orderList = (OrderList)js.ReadObject(ms);
                ms.Close();
            }
            catch (Exception e)
            {
                log(e.Message);
                orderList.oReturn = new List<Ordem>();
                return orderList;
            }

            return orderList;

        }





        public void armazenarOrderList(OrderList orderList)
        {//gravar no arquivo
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(OrderList));
            ser.WriteObject(stream1, orderList);
            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            salvarArquivoGenerico(sr.ReadToEnd(), "orderList");
        }
        



        public static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        public string obterOrdensLocais()
        {
            String line;
            try
            {
                line = obterArquivoGenerico("orderList");                
                return line;
            }
            catch (Exception e)
            {
                log(e.Message);
                return "0";

            }
            finally
            {

            }
        }



        public OrderList criarOrdemVenda(decimal quantidade, decimal preco, string tipo)
        {

            if (saldo.saldoBTC >= quantidade)
            {
                Console.WriteLine("Criando ordem de venda. Quantidade " + quantidade + " preço " + preco);

                string nonce = obterNonce();
                string retorno = CreateOrder(parametros.chaveAPI, nonce, gerarAssinatura(nonce), "BTC", "sell", quantidade, preco);


                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(OrderList));
                MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(retorno));
                OrderList orderList = (OrderList)js.ReadObject(ms);
                ms.Close();





                if (orderList.success == "1")
                {

                    foreach (Ordem ordem in orderList.oReturn)
                    {
                        ordem.tipoRobo = tipo;
                        imprimirOrdem("Ordem criada", ordem);
                    }

                    debitarSaldoBTC(quantidade);
                    //Console.WriteLine("Saldo BTC: " + saldo.saldoBTC);
                }
                else
                {
                    throw new Exception("Erro ao criar ordem de venda: " + retorno);
                }

                return orderList;
            }
            else
            {
                throw new Exception("Sem saldo de BTC, saldo : " + saldo.saldoBTC + ", quantidade Ordem :" + quantidade);
            }
        }

        public OrderList criarOrdemVenda(decimal quantidade, decimal preco)
        {
            Console.WriteLine("saldo: " + saldo.saldoBTC + ", qtd " + quantidade);

            if (saldo.saldoBTC >= quantidade)
            {
                Console.WriteLine("Criando ordem de venda. Quantidade " + quantidade + " preço " + preco);

                string nonce = obterNonce();
                string retorno = CreateOrder(parametros.chaveAPI, nonce, gerarAssinatura(nonce), "BTC", "sell", quantidade, preco);


                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(OrderList));
                MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(retorno));
                OrderList orderList = (OrderList)js.ReadObject(ms);
                ms.Close();





                if (orderList.success == "1")
                {
                    foreach (Ordem ordem in orderList.oReturn)
                    {
                        ordem.tipoRobo = NomeRobo;
                        imprimirOrdem("Ordem criada", ordem);
                    }
                    debitarSaldoBTC(quantidade);
                    Console.WriteLine("Saldo BTC: " + saldo.saldoBTC);
                }
                else
                {
                    throw new Exception("Erro ao criar ordem de venda: " + retorno);
                }

                return orderList;
            }
            else
            {
                throw new Exception("Sem saldo de BTC, saldo : " + saldo.saldoBTC + ", quantidade Ordem :" + quantidade);
            }
        }

        public void cancelarOrdem(Ordem ordem)
        {
            if (ordem.status.ToUpper().ToUpper() == Constantes.STATUS_ORDEM_CANCELADA ||
                ordem.status.ToUpper().ToUpper() == Constantes.STATUS_ORDEM_EXECUTADA)
            {
                return;
            }
            Console.WriteLine("Cancelando ordem:" + ordem.id);
            long IDOrdem = Convert.ToInt64(ordem.id);
            string nonce = obterNonce();
            string retorno = deleteOrders(parametros.chaveAPI, nonce, gerarAssinatura(nonce), IDOrdem);
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(OrderList));
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(retorno));

            OrderList createOrder = (OrderList)js.ReadObject(ms);


            ms.Close();

            if (!createOrder.success.Equals("1"))
            {
                Console.WriteLine("Falha ao excluir a ordem ID: " + ordem.id);
                System.Threading.Thread.Sleep(1000);
                cancelarOrdem(ordem);
            }
            else
            {
                if (ordem.action == Constantes.TIPO_ORDEM_VENDA)
                {
                    saldo.saldoBTC = saldo.saldoBTC + (Convert.ToDecimal(ordem.amount) - Convert.ToDecimal(ordem.executedAmount));

                }
                else if (ordem.action == Constantes.TIPO_ORDEM_COMPRA)
                {
                    saldo.saldoBRL = saldo.saldoBRL + ((Convert.ToDecimal(ordem.amount) - Convert.ToDecimal(ordem.executedAmount)) * Convert.ToDecimal(ordem.price));
                }
                salvarSaldoLocal();
            }

            //return createOrder.oReturn[0];
        }

        public void cancelarTodasOrdensPendentes()
        {
            OrderList orderList = obterOrdensPendentes();
            foreach (Ordem ordem in orderList.oReturn)
            {
                cancelarOrdem(ordem);
            }
        }

        public OrderList obterOrdensPendentes()
        {
            string nonce = obterNonce();
            string retorno = getOrders(parametros.chaveAPI, nonce, gerarAssinatura(nonce), Constantes.STATUS_ORDEM_PENDENTE);


            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(OrderList));
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(retorno));

            OrderList orderList = (OrderList)js.ReadObject(ms);


            ms.Close();

            return orderList;
        }

        public OrderBook obterLivroOrdens()
        {
            string retorno = this.orderBook();
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(OrderBook));
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(retorno));
            OrderBook book = (OrderBook)js.ReadObject(ms);
            ms.Close();
            return book;
        }

        // Returns JSON string
        public string GET(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                log(ex.Message);
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    // log errorText
                }
                throw;
            }
        }

        // POST a JSON string
        void POST(string url, string jsonContent)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(jsonContent);

            request.ContentLength = byteArray.Length;
            request.ContentType = @"application/json";

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            long length = 0;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    length = response.ContentLength;
                    string teste = response.ToString();
                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                }
            }
            catch (WebException e)
            {
                log(e.Message);
                // Log exception and throw as for GET example above
            }
        }

        private string gerarAssinatura(string nonce)
        {
            System.Threading.Thread.Sleep(4000);
            return CreateToken(nonce + parametros.chaveAPI, parametros.chaveSecreta);
        }

        public static string CreateToken(string message, string secret)
        {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new System.Security.Cryptography.HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage).ToUpper();
            }
        }

        public string CreateOrder(string key, string nonce, string signature, string asset, string action, decimal amount, decimal price)
        {
            Hashtable post_values = new Hashtable();
            //Parameters
            post_values.Add("asset", asset);
            post_values.Add("action", action);
            post_values.Add("amount", amount);
            post_values.Add("price", price);

            //Call
            string response = this.url("https://www.bitcointoyou.com/API/createOrder.aspx?", post_values, key, nonce, signature);


            return response;
        }

        public string getOrders(string key, string nonce, string signature, string status)
        {
            Hashtable post_values = new Hashtable();

            //Parameters
            //post_values.Add("status", null);

            //Call
            string response = this.url("https://www.bitcointoyou.com/API/getOrders.aspx?", post_values, key, nonce, signature);
            return response;
        }

        public string getBalance(string key, string nonce, string signature)
        {
            Hashtable post_values = new Hashtable();
            //Call
            string response = this.url("https://www.bitcointoyou.com/API/balance.aspx", post_values, key, nonce, signature);

            return response;
        }

        public string orderBook()
        {
            //Call
            string response = this.url("https://www.bitcointoyou.com/API/orderBook.aspx?");
            return response;
        }

        public string trades(Int64 tid)
        {
            Hashtable post_values = new Hashtable();
            //Parameters
            post_values.Add("tid", tid);

            //Call
            string response = this.url("https://www.bitcointoyou.com/API/trades.aspx?", post_values);
            return response;
        }

        public string getTicker()
        {
            Hashtable post_values = new Hashtable();
            //Call
            string response = this.getURL("https://www.bitcointoyou.com/API/ticker.aspx");
            return response;
        }

        public string getTickerBitstamp()
        {
            Hashtable post_values = new Hashtable();
            //Call
            string response = this.url("https://www.bitstamp.net/api/ticker/");
            return response;
        }

        public string getDolar()
        {
            Hashtable post_values = new Hashtable();
            //Call
            string response = this.getURL("http://api.fixer.io/latest?base=USD&symbols=BRL");
            return response;
        }

        public string deleteOrders(string key, string nonce, string signature, Int64 id)
        {
            Hashtable post_values = new Hashtable();

            //Parameters
            post_values.Add("id", id);

            //Call
            string response = this.url("https://www.bitcointoyou.com/API/deleteOrders.aspx?", post_values, key, nonce, signature);
            return response;
        }



        public string getOrderId(string key, string nonce, string signature, Int64 id)
        {
            Hashtable post_values = new Hashtable();
            //Parameters
            post_values.Add("id", id);

            //Call
            string response = this.url("https://www.bitcointoyou.com/API/getOrdersId.aspx?", post_values, key, nonce, signature);
            return response;
        }

        private string url(string url, Hashtable postValue, string key, string nonce, string signature)
        {
            String post_response = "";
            String post_string = "";
            try
            {
                foreach (DictionaryEntry field in postValue)
                {
                    post_string += field.Key + "=" + field.Value + "&";
                }
                post_string = post_string.TrimEnd('&');

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + post_string);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = 0;
                //preencho o cabeçalho
                webRequest.Headers.Add("key", key);
                webRequest.Headers.Add("nonce", nonce);
                webRequest.Headers.Add("signature", signature);

                webRequest.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

                webRequest.AllowAutoRedirect = false;
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                //Returns "MovedPermanently", not 301 which is what I want.
                StreamReader sr = new StreamReader(response.GetResponseStream());
                post_response = sr.ReadToEnd();
                sr.Close();
                return post_response;
            }
            catch (Exception e)
            {
                log(e.Message);
                throw new Exception("Error ocurred in HttpWebRequest");
            }
            return post_response;
        }

        private string url(string url, string total, string notification, string customId, string redirect, string key, string nonce, string signature)
        {
            String post_response = "";


            try
            {

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = 0;
                //preencho o cabeçalho
                webRequest.Headers.Add("key", key);
                webRequest.Headers.Add("nonce", nonce);
                webRequest.Headers.Add("signature", signature);
                webRequest.Headers.Add("total", total);
                webRequest.Headers.Add("NotificationEmail", notification);
                webRequest.Headers.Add("customId", customId);
                //webRequest.Headers.Add("redirectCommission", "50.00".ToString());
                webRequest.Headers.Add("redirectEmail", "thiagohortal@gmail.com");



                webRequest.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

                webRequest.AllowAutoRedirect = false;
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                //Returns "MovedPermanently", not 301 which is what I want.
                StreamReader sr = new StreamReader(response.GetResponseStream());
                post_response = sr.ReadToEnd();
                sr.Close();
                return post_response;
            }
            catch (Exception e)
            {
                log(e.Message);
                throw new Exception("Error ocurred in HttpWebRequest");
            }
            return post_response;
        }

        private string url(string url, Hashtable post_values)
        {
            String post_string = "";
            String post_response = "";
            HttpWebRequest objRequest;
            foreach (DictionaryEntry field in post_values)
            {
                post_string += field.Key + "=" + field.Value + "&";
            }
            post_string = post_string.TrimEnd('&');

            try
            {
                //Monto a URL com os parametros
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + post_string);
                webRequest.Method = "POST";
                const string contentType = "application/x-www-form-urlencoded";
                webRequest.AllowAutoRedirect = false;
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                //Returns "MovedPermanently", not 301 which is what I want.
                StreamReader sr = new StreamReader(response.GetResponseStream());
                post_response = sr.ReadToEnd();
                sr.Close();
                return post_response;
            }
            catch (Exception e)
            {
                log(e.Message);
                throw new Exception("Error ocurred in HttpWebRequest");
            }
            return post_response;
        }

        private string url(string url)
        {
            String post_response = "";

            try
            {


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = 0;
                //preencho o cabeçalho
                webRequest.Headers.Add("key", parametros.chaveAPI);


                webRequest.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

                webRequest.AllowAutoRedirect = false;
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                //Returns "MovedPermanently", not 301 which is what I want.
                StreamReader sr = new StreamReader(response.GetResponseStream());
                post_response = sr.ReadToEnd();
                sr.Close();
                return post_response;
            }
            catch (Exception e)
            {
                log(e.Message);
                throw new Exception("Error ocurred in HttpWebRequest");
            }
            return post_response;
        }

        private string getURL(string url)
        {
            String post_response = "";

            try
            {


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "GET";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = 0;
                //preencho o cabeçalho
                webRequest.Headers.Add("key", parametros.chaveAPI);


                webRequest.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

                webRequest.AllowAutoRedirect = false;
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                //Returns "MovedPermanently", not 301 which is what I want.
                StreamReader sr = new StreamReader(response.GetResponseStream());
                post_response = sr.ReadToEnd();
                sr.Close();
                return post_response;
            }
            catch (Exception e)
            {
                log(e.Message);
                throw new Exception("Error ocurred in HttpWebRequest");
            }
            return post_response;
        }


    }
}
