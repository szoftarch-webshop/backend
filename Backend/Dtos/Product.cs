using Backend.Dal.Entities;

namespace Backend.Dtos
{
	public record Product
	(
		int Id,
		string Name,
		double Weight,
		string Material,
		string Description,
		int Price,
		int Stock,
		string ImageUrl,
		List<int> CategoryIds
	);
}
