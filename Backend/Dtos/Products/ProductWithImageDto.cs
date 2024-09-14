namespace Backend.Dtos.Products;

public class ProductWithImageDto
{
    public string SerialNumber { get; set; }
    public string Name { get; set; }
    public float Weight { get; set; }
    public string Material { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public int Stock { get; set; }
    public IFormFile Image { get; set; }
    public List<string> CategoryNames { get; set; }
}