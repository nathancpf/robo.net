# robo.net
Robô de trade para a Bitcointoyou.

Objetivo do Robo:

Criar ordens automaticamente buscando o lucro

Descrição:

Este robo automatiza a compra e venda de bitcoin e busca o lucro definido pelo usuário. Ao comprar, ele cria uma ordem de venda automáticamente, e ao vender, o robô cria uma ordem de compra automaticamente buscando o lucro, de acordo com a parametrização (opcional) via arquivo de texto definida pelo usuário.

Instruções

Este robô foi escrito em c#.net. Você pode executá-lo em Windows (https://www.microsoft.com/en-us/download/details.aspx?id=55170&desc=dotnet47) ou Linux (https://www.microsoft.com/net/download/linux) em seu próprio computador.
Opcionalmente, você pode executá-lo em um servidor da Amazon (possui 1 ano grátis) e segundo ano a partir de 5 dólares por mês (https://goo.gl/6qFDXi).

Configuração

1 - Crie seu acesso vai api na Bitcointoyou. Faça o login, acesse o menu API e cliquem em GERAR CHAVES;
2 - O robo criará um diretório em seu computador em C:\Users\SEU_NOME\OneDrive\Documents\RoboB2U - ele obtêm automaticamente o diretório documentos do usuário logado no computador (windows ou linux).
3 - Após o download completo de todo diretorio do robo. Copie os 4 arquivos abaixo para este diretório (este arquivos possuem configurações padronizadas que você pode alterar)
ROBO_V2_nonce.txt
ROBO_V2_orderlist.txt
ROBO_V2_SALDO.txt
ROBO_V2_parametros.txt

Início

Você deve criar a primeira ordem de compra e primeira ordem de venda manualmente no site da Bitcointoyou. A partir disto o robo começará a monitorá-las e revender ou recomprar ao serem executadas (lembre-se que a quantidade da ordem deve ser inferior ou igual ao parametro quantidadeMaxima definido no arquivo parametros)

Parametros customizáveis

Você pode alterar os seguintes parâmetros do arquivo ROBO_V2_parametros.txt
margemLucro: indicada a % do lucro a ser buscada ao revender ou recomprar
quantidadeMaxima: o robo irá trabalhar com ordens que tenham no máximo a quantidade definida aqui.
chaveAPI: obtenha sua chave api no menu API da Bitcointoyou.com - OBRIGATÓRIO
chaveSecreta: obtenha sua chave secreta no menu API da Bitcointoyou.com - OBRIGATÓRIO
ROBO_V2_SALDO.txt: defina o saldo em BRL e em BTC que você limitará seu robô
ROBO_V2_nonce.txt: defina um número qualquer que você goste

Caso tenha dúvidas, envie um e-mail para sac@bitcointoyou.com
Faremos eventualmente webinar da Bitcointoyou, você poderá perguntar a um especialista ao vivo.

ATENÇÃO: A Bitcointoyou não se responsabilisa pelo uso deste robô. Não há garantia de lucro do investimento ao utilizá-lo. Você é o único responsável pelo risco e possíveis perdas de capital. 
