namespace DAL;

/// <summary>
/// Crud operations for Game and Configuration
/// </summary>
/// <typeparam name="TData">either Game or Configuration</typeparam>
public interface IGameRepository<TData>
{
    List<(string id, string description)> List();
    
    Task<List<(string id, string description)>> ListAsync();

    // crud
    string Save(TData data);
    TData Load(string id);
    string Update(TData data, string fileName);
    void Delete(string fileName);
}
