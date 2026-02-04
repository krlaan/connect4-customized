using System.ComponentModel.DataAnnotations;

namespace Domain;

public class User: BaseEntity
{
    [MaxLength(128)]
    public string UserName { get; set; } = default!;
    
    public ICollection<Config>? Configs { get; set; }
    
    public ICollection<Game>? Games { get; set; }

    
    public override string ToString()
    {
        return $"User: {UserName}, Configs: {Configs?.Count ?? 0}, Games: {Games?.Count ?? 0}";
    }
}