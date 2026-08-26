# language: pt-BR

Funcionalidade: Autenticação
  Como usuário cadastrado da FIAP Cloud Games
  Quero autenticar com e-mail e senha
  Para receber um token de acesso às áreas protegidas

  Contexto:
    Dado que a plataforma não possui usuários cadastrados
    E que existe um jogador "alice@fcg.com" com a senha "Senha@123"

  Cenário: Credenciais válidas devolvem token
    Quando eu autentico com o e-mail "alice@fcg.com" e a senha "Senha@123"
    Então a autenticação deve ser concluída com sucesso
    E o token devolvido deve identificar a conta "alice@fcg.com"
    E o token devolvido deve carregar o perfil "Player"

  Cenário: Senha incorreta não autentica
    Quando eu autentico com o e-mail "alice@fcg.com" e a senha "Errada@123"
    Então a autenticação deve ser recusada

  Cenário: Conta inexistente não autentica
    Quando eu autentico com o e-mail "ninguem@fcg.com" e a senha "Senha@123"
    Então a autenticação deve ser recusada

  Cenário: Conta inativada não autentica
    Dado que a conta "alice@fcg.com" foi inativada
    Quando eu autentico com o e-mail "alice@fcg.com" e a senha "Senha@123"
    Então a autenticação deve ser recusada

  Cenário: E-mail malformado não revela se a conta existe
    Quando eu autentico com o e-mail "nao-e-um-email" e a senha "Senha@123"
    Então a autenticação deve ser recusada
