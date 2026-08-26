# language: pt-BR

Funcionalidade: Biblioteca de jogos adquiridos
  Como jogador autenticado da FIAP Cloud Games
  Quero adquirir jogos do catálogo pelo preço vigente
  Para acessá-los na minha biblioteca

  Contexto:
    Dado que a plataforma não possui usuários cadastrados
    E que existe um jogador "alice@fcg.com" com a senha "Senha@123"
    E que existe o jogo "FIAP Adventure" custando 100,00

  Cenário: Aquisição sem promoção registra o preço-base
    Quando o jogador "alice@fcg.com" adquire o jogo "FIAP Adventure"
    Então a aquisição deve ser concluída com sucesso
    E a biblioteca de "alice@fcg.com" deve conter 1 jogo
    E o jogo "FIAP Adventure" deve constar na biblioteca de "alice@fcg.com" por 100,00

  Cenário: Aquisição durante promoção vigente registra o preço com desconto
    Dado que o jogo "FIAP Adventure" está com uma promoção vigente de 30 por cento
    Quando o jogador "alice@fcg.com" adquire o jogo "FIAP Adventure"
    Então a aquisição deve ser concluída com sucesso
    E o jogo "FIAP Adventure" deve constar na biblioteca de "alice@fcg.com" por 70,00

  Cenário: Promoção fora da vigência não altera o preço cobrado
    Dado que o jogo "FIAP Adventure" está com uma promoção encerrada de 30 por cento
    Quando o jogador "alice@fcg.com" adquire o jogo "FIAP Adventure"
    Então a aquisição deve ser concluída com sucesso
    E o jogo "FIAP Adventure" deve constar na biblioteca de "alice@fcg.com" por 100,00

  Cenário: O mesmo jogo não é adquirido duas vezes
    Dado que o jogador "alice@fcg.com" já possui o jogo "FIAP Adventure"
    Quando o jogador "alice@fcg.com" adquire o jogo "FIAP Adventure"
    Então a aquisição deve ser recusada por conflito
    E a biblioteca de "alice@fcg.com" deve conter 1 jogo

  Cenário: Jogo retirado do catálogo não pode ser adquirido
    Dado que o jogo "FIAP Adventure" foi retirado do catálogo
    Quando o jogador "alice@fcg.com" adquire o jogo "FIAP Adventure"
    Então a aquisição deve ser recusada por conflito
    E a biblioteca de "alice@fcg.com" deve conter 0 jogos

  Cenário: Jogo retirado do catálogo permanece na biblioteca de quem já comprou
    Dado que o jogador "alice@fcg.com" já possui o jogo "FIAP Adventure"
    E que o jogo "FIAP Adventure" foi retirado do catálogo
    Então a biblioteca de "alice@fcg.com" deve conter 1 jogo

  Cenário: Conta inativada não adquire jogos
    Dado que a conta "alice@fcg.com" foi inativada
    Quando o jogador "alice@fcg.com" adquire o jogo "FIAP Adventure"
    Então a aquisição deve ser recusada por conflito

  Cenário: Um jogador não enxerga a biblioteca de outro
    Dado que existe um jogador "bruno@fcg.com" com a senha "Senha@123"
    E que o jogador "alice@fcg.com" já possui o jogo "FIAP Adventure"
    Então a biblioteca de "bruno@fcg.com" deve conter 0 jogos
