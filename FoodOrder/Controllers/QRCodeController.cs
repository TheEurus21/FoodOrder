using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace FoodOrder.API.Controllers
{
    [ApiController]
    [Route("api/qrcode")]
    public class QRCodeController : ControllerBase
    {
        [HttpGet("restaurants")]
        public IActionResult GetRestaurantsQr()
        {
            string url = "http://localhost:5236/api/restaurants";

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using var bitmap = qrCode.GetGraphic(20);

            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);

            return File(stream.ToArray(), "image/png");
        }
    }
}
