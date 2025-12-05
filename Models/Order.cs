using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShopOnWeb.Models;

/// <summary>
/// Customer order persisted after checkout.
/// </summary>
public class Order
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
