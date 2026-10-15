namespace EmailService.Helper
{
    public class EmailHelper
    {
        public string GenerateOtp()
        {
            Random random = new Random();

            return random.Next(
                100000,
                999999)
                .ToString();
        }
    }
}