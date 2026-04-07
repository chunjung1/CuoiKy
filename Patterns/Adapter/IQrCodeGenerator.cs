namespace CuoiKy.Patterns;

public interface IQrCodeGenerator
{
    string BuildImageUrl(string data, int size);
}

public class QrServerClient
{
    public string Create(string data, int size)
    {
        var encoded = Uri.EscapeDataString(data);
        return $"https://api.qrserver.com/v1/create-qr-code/?size={size}x{size}&data={encoded}";
    }
}

public class QrServerQrCodeGenerator : IQrCodeGenerator
{
    private readonly QrServerClient _client;

    public QrServerQrCodeGenerator(QrServerClient client)
    {
        _client = client;
    }

    public string BuildImageUrl(string data, int size)
    {
        return _client.Create(data, size);
    }
}

