using my_cv_gen_api.Models;

namespace my_cv_gen_api.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
