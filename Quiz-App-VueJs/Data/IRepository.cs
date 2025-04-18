namespace Quiz_App_VueJs.Data
{
    public interface IRepository
    {
        Task<ICollection<T>> GetCollectionAsync<T>(string filePath, Func<T, bool>? filter = null);
        Task<ICollection<T>> UpdateCollectionAsync<T>(string filePath, Func<T, bool>? filter = null, Action<T>? updateAction = null);
    }
}
