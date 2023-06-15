using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace WebStore.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Login { get; set; }
        public byte[]? Password { get; set; }
    }
}
