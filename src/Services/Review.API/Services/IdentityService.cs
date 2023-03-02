namespace Review.API.Services;

public class IdentityService : IIdentityService
{
    private IHttpContextAccessor _context;

    public IdentityService(IHttpContextAccessor context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public string GetUserIdentity()
    {
        foreach (var s1 in _context.HttpContext.User.Claims.Select(s => s.Value))
        {
            Console.WriteLine(s1);
        }
        
        Console.WriteLine("USER: " + _context.HttpContext.User.Claims.Count());
        
        return _context.HttpContext.User.FindFirst("sub").Value;
    }
}

