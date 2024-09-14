﻿namespace Backend.Dtos.Orders;

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Amount { get; set; }
    public int OrderedPrice { get; set; }
    public ProductDto Product { get; set; }
}