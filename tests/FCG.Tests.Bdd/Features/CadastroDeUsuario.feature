# language: pt-BR

Funcionalidade: Cadastro de usuários
  Como visitante da FIAP Cloud Games
  Quero criar minha conta informando nome, e-mail e senha
  Para acessar o catálogo e minha biblioteca de jogos

  Contexto:
    Dado que a plataforma não possui usuários cadastrados

  Cenário: Cadastro público cria uma conta de jogador
    Quando eu me cadastro com o nome "Alice", o e-mail "alice@fcg.com" e a senha "Senha@123"
    Então o cadastro deve ser concluído com sucesso
    E a conta "alice@fcg.com" deve ter o perfil "Player"
    E a conta "alice@fcg.com" deve estar ativa

  Cenário: A senha nunca é guardada em texto aberto
    Quando eu me cadastro com o nome "Alice", o e-mail "alice@fcg.com" e a senha "Senha@123"
    Então a senha armazenada da conta "alice@fcg.com" não deve ser "Senha@123"

  Esquema do Cenário: Senha fora da política de segurança é recusada
    Quando eu me cadastro com o nome "Alice", o e-mail "alice@fcg.com" e a senha "<senha>"
    Então o cadastro deve ser recusado por dados inválidos
    E nenhuma conta deve ter sido criada

    Exemplos:
      | senha     | motivo                    |
      | Se@1      | menos de oito caracteres  |
      | senha@abc | não possui número         |
      | 1234@5678 | não possui letra          |
      | Senha1234 | não possui caractere especial |

  Esquema do Cenário: E-mail em formato inválido é recusado
    Quando eu me cadastro com o nome "Alice", o e-mail "<email>" e a senha "Senha@123"
    Então o cadastro deve ser recusado por dados inválidos
    E nenhuma conta deve ter sido criada

    Exemplos:
      | email          |
      | alice          |
      | alice@fcg      |
      | alice fcg.com  |

  Cenário: E-mail já cadastrado é recusado
    Dado que existe um jogador "alice@fcg.com" com a senha "Senha@123"
    Quando eu me cadastro com o nome "Outra Alice", o e-mail "alice@fcg.com" e a senha "Outra@456"
    Então o cadastro deve ser recusado por conflito
    E a plataforma deve ter 1 conta cadastrada

  Cenário: E-mail é normalizado antes de ser gravado
    Quando eu me cadastro com o nome "Alice", o e-mail "  ALICE@FCG.COM  " e a senha "Senha@123"
    Então o cadastro deve ser concluído com sucesso
    E a conta "alice@fcg.com" deve ter o perfil "Player"

  Cenário: A área administrativa cria uma conta de administrador
    Quando um administrador cadastra a conta "root@fcg.com" com o perfil "Administrator"
    Então o cadastro deve ser concluído com sucesso
    E a conta "root@fcg.com" deve ter o perfil "Administrator"
