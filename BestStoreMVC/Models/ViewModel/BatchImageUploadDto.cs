using System.ComponentModel.DataAnnotations;

namespace BestStoreMVC.Models.ViewModel
{
    public class BatchImageUploadDto
    {
        [Required(ErrorMessage = "請選擇要上傳的圖片檔案")]
        public List<IFormFile> ImageFiles { get; set; } = new List<IFormFile>();
        
        public string? UploadMessage { get; set; }
    }
}
