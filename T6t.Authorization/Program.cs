using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AuthorizationDbContext>(options => {
    options.UseInMemoryDatabase("Authorization");
});
builder.Services.AddControllers()
    .AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/account", async (AuthorizationDbContext db) => {
    return Results.Ok(await db.Accounts.ToListAsync());
}).WithName("GetAccount");

app.MapGet("/account/{id}", async (Guid id, AuthorizationDbContext db) => {
    return Results.Ok(await db.Accounts.Include(account => account.Roles).SingleOrDefaultAsync(x => x.Id == id));
}).WithName("GetAccountById");

app.MapPost("/account", async (Account account, AuthorizationDbContext db) => { 
    await db.Accounts.AddAsync(account);
    await db.SaveChangesAsync();
    return Results.Ok(account);
}).WithName("PostAccount");

app.MapPut("/account/{id}/roles", async (Guid id, AccountRoles accountRoles, AuthorizationDbContext db) => {
    var account = await db.Accounts.Include(account => account.Roles).SingleOrDefaultAsync(x => x.Id == id);
    var roles = await db.Roles.Where(role => accountRoles.RoleIds.Contains(role.Id)).ToListAsync();

    if(account == default(Account))
        return Results.NotFound();

    account.UpdateRoles(roles);
    db.Entry<Account>(account).State = EntityState.Modified;
    await db.SaveChangesAsync();

    return Results.Ok(account);
}).WithName("PutAccountRoles");

app.MapPost("/signIn", async (SignIn signIn, AuthorizationDbContext db, IEnumerable<IAuthenticationProvider> authenticationProviders) =>{
    var authenticationProvider = authenticationProviders.Single(authenticationProvider => authenticationProvider.Name == signIn.AuthenticationProvider);
    var authenticationKey = await authenticationProvider.GetKeyByTokenAsync(signIn.AuthenticationToken);
    var account = await db.Accounts.Include(account => account.Roles).SingleAsync(x => x.AuthenticationProvider == signIn.AuthenticationProvider && x.AuthenticationKey == authenticationKey);
    var claims = new List<Claim> { 
        new Claim(ClaimTypes.Name, account.Name),
    };
    foreach (var role in account.Roles)    
        claims.Add(new Claim(role.Api, role.Name));

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("AppSettings:AuthorizationToken")));

    var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddDays(1),
        signingCredentials: signingCredentials
        );

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(jwt);
}).WithName("PostAccountSignIn");

app.MapPost("/role", async (Role role, AuthorizationDbContext db) => {
    await db.Roles.AddAsync(role);
    await db.SaveChangesAsync();
    return Results.Ok(role);
}).WithName("PostRole");

app.MapGet("/role", async (AuthorizationDbContext db) => {    
    return Results.Ok(await db.Roles.ToListAsync());
}).WithName("GetRoles");

app.Run();
