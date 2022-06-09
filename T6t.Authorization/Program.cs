using Microsoft.EntityFrameworkCore;
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

app.MapPost("/signIn", async (SignIn signIn, AuthorizationDbContext db) =>{
    var account = await db.Accounts.Include(account => account.Roles).SingleOrDefaultAsync(x => x.AuthenticationProvider == signIn.AuthenticationProvider && x.AuthenticationToken == signIn.AuthenticationToken);
    //Todo: Consultar e JWT
    return Results.Ok(Guid.NewGuid().ToString());
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
