using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SecretController : ControllerBase
{
    [Authorize] 
    [HttpGet("data")]
    public IActionResult GetSecretData()
    {
        return Ok(new { message = "Đây là dữ liệu mật chỉ dành cho người đã xác thực!" });
    }
}
