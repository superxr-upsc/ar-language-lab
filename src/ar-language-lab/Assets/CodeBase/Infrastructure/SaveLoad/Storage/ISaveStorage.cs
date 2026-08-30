using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.SaveLoad.Storage
{
    public interface ISaveStorage
    {
        UniTask<bool> ExistsAsync();
        UniTask<string> ReadAsync();
        UniTask WriteAsync(string payload);
        UniTask DeleteAsync();
        string GetAbsolutePath();
    }
}


