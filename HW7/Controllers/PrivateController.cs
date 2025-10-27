using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PrivateController : ControllerBase
{
    [Authorize]
    [HttpGet("data")]
    public IActionResult GetPrivateData()
    {
        return Ok(new { secret = "Đây là dữ liệu mật chỉ xem khi đăng nhập thành công!" });
    }
}
