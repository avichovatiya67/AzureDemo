namespace Stryker.SmartMedic.Models
{
    public class ErrorMessage : BaseModel
    {
        public string serial_number { get; set; }
        public string error_desc { get; set; }

        public ErrorMessage() : base()
        {

        }
    }
}
