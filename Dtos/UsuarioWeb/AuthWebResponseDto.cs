namespace inmobiliariaApi.Dtos.UsuarioWeb
{
    public class AuthWebResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public UsuarioWebDto Usuario { get; set; } = new();
    }
}
