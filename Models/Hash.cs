using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Geology_Api.Models;

public class Hash
{
    public int Id { get; set; } 
    
    [Required]
    public string HashPass { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}