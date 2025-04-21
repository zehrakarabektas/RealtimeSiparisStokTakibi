using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using yazlab1proje3UI.Classes;
using yazlab1proje3UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddCookie(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.LoginPath="/Home/Index/";
    options.LogoutPath="/Home/Index/";
    options.AccessDeniedPath="/Home/Index/";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite=SameSiteMode.Strict;
    options.Cookie.SecurePolicy= CookieSecurePolicy.SameAsRequest;
    options.Cookie.Name="realjwt";

});


JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Hash>();
builder.Services.AddScoped<RandomCustomer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseWebSockets();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
