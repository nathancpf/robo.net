# robo.net
Robô de trade para a Bitcointoyou.

Você encontrará aqui vários robôs (bots) para operar de forma automatizada na Bitcointoyou. Entre em cada pasta/diretório e veja as instruções específicas de cada robô.

Robos disponíveis:

RoboTrader V2: 
Você deve criar duas ordens pendentes no site da Bitcointoyou, uma de compra e venda. Quando uma destas for executada, o bot automaticametne criará a ordem inversa para buscar o lucro.

Robo V3: 
Este bot cria a ordem de compra (OC) e venda (OV) automaticamente baseada no preço do bitstamp * dólar comercial * spread. Caso as cotações se alterem, o próprio bot ajusta sua ordem. Ele também faz o mesmo que o RoboV2 (cria ordem inversa ao ser executada).

RoboWar: 
Buscar ficar sempre em primeiro no book, tanto em ordem de compra quanto em ordem de venda. Se a ordem for executada, cria automaticamente a ordem inversa.

Como começar

1 - Os bots (robôs) já estão prontos para uso, após o download (clique em clone or download), dê duplo clique em RoboTrader.exe

2 - Crie seu acesso vai api na Bitcointoyou. Faça o login, acesse o menu API e cliquem em GERAR CHAVES;

3 - Personalize seu robo através do arquivo NOME_ROBO_parametros.txt:

Parâmetros comuns para todos os bots (v2, v3 e war):

chaveAPI:  Obtenha no meu API ao fazer login an Bitcointoyou.com

chaveSecreta: Obtenha no meu API ao fazer login an Bitcointoyou.com

margemLucro: Defina a porcentagem de lucro que você deseja ter. (Ex: Caso sua ordem de compra de R$ 10.000,00 tenha sido executada, o robô criará uma ordem de venda de R$ 10.100,00, caso você defina margeLucro: 1)

quantidadeMaxima: Ordens que tenham até esta quantidade definida, serão monitoradas pelo robô.

Parâmetros RoboV3:

quantidadeOrdem: Especifique qual o volume da ordem a ser criada pelo robô

spreadCompra: Em relação ao preço do Bitstamp, quantos % a mais ou a menos você quer pagar no Bitcoin na Bitcointoyou

spreadVenda: Em relação ao preço do Bitstamp, quantos % a mais ou a menos você quer vender o Bitcoin na Bitcointoyou 

variacaoPermitida: margem de variação da cotação permitida.

Parâmetros RoboWar

incrementoOrdem: Define quanto o preço da ordem será incrementado (0.001 = um centavo de reais)

spreadCompraVenda: Valor da diferença em reais da compra e da venda

minimoLucroBRL: Ao ter uma ordem executada, deseja ter de lucro no mínimo quantos reais.


Caso tenha dúvidas, envie um e-mail para sac@bitcointoyou.com
Faremos eventualmente webinar da Bitcointoyou, você poderá perguntar a um especialista ao vivo.

ATENÇÃO: A Bitcointoyou não se responsabilisa pelo uso deste robô. Não há garantia de lucro do investimento ao utilizá-lo. Você é o único responsável pelo risco e possíveis perdas de capital. 
