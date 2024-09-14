namespace Backend.Dtos;

public class CreateCategoryDto
{
    public string Name { get; set; }
    public int? ParentId { get; set; } // Null if there is no parent
}