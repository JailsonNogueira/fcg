namespace FCG.Application.Common;

/// <summary>
/// Normaliza os parâmetros de paginação recebidos pela API.
/// </summary>
public static class PageRequest
{
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;

    public static (int Page, int PageSize, int Skip) Normalize(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize switch
        {
            < 1 => DefaultPageSize,
            > MaximumPageSize => MaximumPageSize,
            _ => pageSize
        };

        // Em long: ?page=2000000000 estouraria int e produziria um OFFSET negativo no banco.
        var skip = (long)(normalizedPage - 1) * normalizedPageSize;

        return (normalizedPage, normalizedPageSize, (int)Math.Min(skip, int.MaxValue));
    }
}
