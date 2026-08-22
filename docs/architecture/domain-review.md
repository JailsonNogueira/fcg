# Revisão de domínio — agregados e contextos delimitados

Data da revisão: 22 de agosto de 2026.

## Contextos delimitados

| Contexto | Raiz do agregado | Responsabilidade |
| --- | --- | --- |
| Usuários | `User` | Identidade, perfil, credenciais e ativação de contas. |
| Catálogo | `Game` | Dados comerciais e disponibilidade de jogos. |
| Biblioteca | `LibraryItem` | Registro imutável de uma aquisição de jogo por um usuário. |
| Promoções | `Promotion` | Vigência e cálculo de descontos de um jogo. |

Os agregados se referenciam exclusivamente por identificadores (`Guid`); não há dependência direta entre os objetos dos contextos. Isso mantém as regras locais e permite persistência independente.

## Decisões confirmadas

- `Email` e `Password` são objetos de valor do contexto de Usuários; `User` persiste somente `PasswordHash`.
- O nome normalizado de `Game` pertence ao contexto de Catálogo e é a chave de comparação para unicidade no repositório.
- `LibraryItem` armazena o preço efetivamente pago, preservando o histórico mesmo após alterações no preço do jogo ou em promoções.
- `Promotion` calcula o desconto sem alterar o preço-base de `Game` e verifica sua própria vigência.
- Regras de unicidade, autorização e coordenação entre agregados pertencem à futura camada Application, pois exigem consultas aos repositórios ou políticas de acesso.

## Pontos de atenção para as próximas camadas

- Validar unicidade de e-mail e de nome normalizado de jogo nos casos de uso, usando os contratos de repositório.
- Restringir criação e manutenção de jogos e promoções a administradores na API.
- Garantir que a criação de `LibraryItem` somente ocorra após uma aquisição válida.
- Não expor `Password` nem `PasswordHash` em DTOs de resposta ou logs.
