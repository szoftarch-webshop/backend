using Backend.Dal.Entities;

namespace Backend.Dtos
{
	public record ProductDto
	(
		int Id,
		string SerialNumber,
		string Name,
		double Weight,
		string Material,
		string Description,
		int Price,
		int Stock,
		string ImageUrl,
		List<string> CategoryNames
	);
}
