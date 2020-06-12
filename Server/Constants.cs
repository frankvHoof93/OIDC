namespace Server
{
    public static class Constants
    {
        public const string Issuer = Audience;
        public const string Audience = "https://localhost:44379/";

        public const string Secret = "not_too_short_secret_so_that_it_actually_works";
    }
}
