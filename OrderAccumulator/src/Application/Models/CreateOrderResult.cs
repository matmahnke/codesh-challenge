namespace Application.Models
{
    public class CreateOrderResult
    {
        public CreateOrderResult(bool success, decimal currentExposure, string errorMessage)
        {
            Success = success;
            CurrentExposure = currentExposure;
            ErrorMessage = errorMessage;
        }

        public bool Success { get; set; }
        public decimal CurrentExposure { get; set; }
        public string ErrorMessage { get; set; }
    }
}
