public class TumorDetectionHistory
{
    public int Id { get; set; } // Otomatik artan ID
    public int UserId { get; set; } // Kullanıcı ID'si
    public string ImageUrl { get; set; } // Yüklenen görüntü URL'si
    public string TumorType { get; set; } // Tespit edilen tümör tipi
    public DateTime DetectionDate { get; set; } // Tespit tarihi
}
