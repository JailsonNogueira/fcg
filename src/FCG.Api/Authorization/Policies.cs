namespace FCG.Api.Authorization;

public static class Policies
{
    /// <summary>Leitura do catálogo: disponível a qualquer conta autenticada.</summary>
    public const string Catalog = "Catalog";

    /// <summary>Manutenção do catálogo de jogos: exclusiva de administradores.</summary>
    public const string ManageCatalog = "ManageCatalog";

    /// <summary>Gestão de contas da plataforma: exclusiva de administradores.</summary>
    public const string ManageUsers = "ManageUsers";

    /// <summary>Gestão de promoções: exclusiva de administradores.</summary>
    public const string ManagePromotions = "ManagePromotions";

    /// <summary>Biblioteca pessoal: jogadores adquirem e consultam os próprios jogos.</summary>
    public const string Library = "Library";
}
