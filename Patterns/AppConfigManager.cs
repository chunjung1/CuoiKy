namespace CuoiKy.Patterns;

// [Design Pattern: Singleton] - [Nhóm: Creational]
// Mục đích: Quản lý cấu hình ứng dụng chung.
public sealed class AppConfigManager
{
    private static readonly Lazy<AppConfigManager> lazy = new(() => new AppConfigManager());
    public static AppConfigManager Instance => lazy.Value;

    private AppConfigManager() { }

    public string AppName { get; set; } = "TechStore";
    public string Company { get; set; } = "TechStore Co.";
}
