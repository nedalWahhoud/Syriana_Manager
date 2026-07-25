global using Blazored.LocalStorage;
namespace Syriana_Manager.Components.LocalStorageF
{
    public class LocalStorageService(ILocalStorageService localStorage)
    {

        private readonly ILocalStorageService _localStorage = localStorage;

        public async Task<List<TItem>> SyncColumnOptionsAsync<TItem, TKey>(string storageKey, List<TItem> currentOptions, Func<TItem, TKey> keySelector)
        {
            try
            {
                var saved = await Get<List<TItem>>(storageKey);

                if (saved is not null)
                {
                    var currentKeys = currentOptions.Select(keySelector).ToList();
                    var savedKeys = saved.Select(keySelector).ToList();

                    bool isMismatch = currentKeys.Count != savedKeys.Count || currentKeys.Any(k => !savedKeys.Contains(k));

                    if (isMismatch)
                    {
                        await Remove(storageKey);
                        return currentOptions;
                    }

                    return saved;
                }

                return currentOptions;
            }
            catch 
            {
                return currentOptions;
            }
        }

        public async Task Set(string key,object newValue)
        {
            try
            {
                await _localStorage.SetItemAsync(key, newValue);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }
        }
        public async Task<TItem?> Get<TItem>(string key)
        {
            try
            {
                var value = await _localStorage.GetItemAsync<TItem>(key);
                return value;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                return default;
            }
        }
        public async Task Remove(string key)
        {
            try
            {
                await _localStorage.RemoveItemAsync(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
