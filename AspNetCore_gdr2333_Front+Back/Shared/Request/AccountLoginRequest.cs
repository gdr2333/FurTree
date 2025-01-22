using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Request;

public class AccountLoginRequest
{
    public required string Name { get; set; }
    public required string PasswordHash { get; set; }
    public required string CapchaId { get; set; }
    public required string CapchaResult { get; set; }
}
