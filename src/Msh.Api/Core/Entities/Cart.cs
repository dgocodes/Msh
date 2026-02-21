namespace Msh.Api.Core.Entities;

public class Cart
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = [];
}
