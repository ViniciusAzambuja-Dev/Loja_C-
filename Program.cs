using Microsoft.EntityFrameworkCore;
using loja.models;
using loja.data;
using loja.services;
using loja.controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)

    .AddJwtBearer(options =>
    {

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("Z1HscBdX8ztx7myUdVMl67NebM8fkX1c"))
        };
    });

    
builder.Services.AddAuthorization();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<FornecedorService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TokenValidationService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<StockService>();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LojaDbContext>(options=>options.UseMySql(connectionString,new MySqlServerVersion(new Version(8, 0, 26))));

var app = builder.Build();

app.UseHttpsRedirection();
// Configurar as requisições HTTP

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/usuarios") &&
        !context.Request.Path.StartsWithSegments("/login"))
    {
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token não fornecido");
            return;
        }

        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var tokenValidationService = context.RequestServices.GetRequiredService<TokenValidationService>();

        if (!await tokenValidationService.ValidateTokenAsync(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token inválido");
            return;
        }
    }

    await next();
});

// Habilitar autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", async (HttpContext context) =>
{
    using var reader = new System.IO.StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();

    var json = JsonDocument.Parse(body);
    var nome = json.RootElement.GetProperty("nome").GetString();
    var email = json.RootElement.GetProperty("email").GetString();
    var senha = json.RootElement.GetProperty("senha").GetString();

    var dbContext = context.RequestServices.GetRequiredService<LojaDbContext>();
    var userController = new UserController(dbContext);

    var token = "";
    var errorMessage = "Nenhum";

    if (userController.Login(senha))
    {
        TokenController tokenController = new TokenController();
        token = tokenController.GenerateToken();
    }
    else
    {
      
        errorMessage = "Usuário não encontrado ou senha incorreta";
    }

    var jsonResponse = JsonSerializer.Serialize(new { Token = token, Error = errorMessage});
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(jsonResponse);
});

app.MapGet("/rotaSegura", async (HttpContext context) =>
{
    if (!context.Request.Headers.ContainsKey("Authorization"))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Token não fornecido");
        return;
    }

    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

    var tokenValidationService = context.RequestServices.GetRequiredService<TokenValidationService>();

    if (!await tokenValidationService.ValidateTokenAsync(token))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Token inválido");
        return;
    }

    await context.Response.WriteAsync("Autorizado");
});


//produtos 


app.MapGet("/produtos", async ([FromServices] ProductService productService) =>
{
    var produtos = await productService.GetAllProductsAsync();
    return Results.Ok(produtos);
});

app.MapGet("/produtos/{id}", async (int id,[FromServices]  ProductService productService) =>
{
    var produto = await productService.GetProductByIdAsync(id);
    if (produto == null)
    {
        return Results.NotFound($"Product with ID {id} not found.");
    }
    return Results.Ok(produto);
});

app.MapPost("/produtos", async ( [FromBody] Produto produto, [FromServices] ProductService productService) =>
{
    await productService.AddProductAsync(produto);
    return Results.Created($"/produtos/{produto.Id}", produto);

});

app.MapPut("/produtos/{id}", async (int id, [FromBody] Produto produto, [FromServices] ProductService productService) =>
{
    if (id != produto.Id)
    {
        return Results.BadRequest("Product ID mismatch.");
    }
    await productService.UpdateProductAsync(produto);
    return Results.Ok();
});

app.MapDelete("/produtos/{id}", async (int id, [FromServices] ProductService productService) =>
{
    await productService.DeleteProductAsync(id);
    return Results.Ok();
});


//clientes

app.MapGet("/clientes", async ([FromServices] ClientService clientService) =>
{
    var clientes = await clientService.GetAllClientsAsync();
    return Results.Ok(clientes);
    
});

app.MapGet("/clientes/{id}", async (int id, [FromServices] ClientService clientService) =>
{
    var cliente = await clientService.GetClientByIdAsync(id);
    if (cliente == null)
    {
        return Results.NotFound($"Client with ID {id} not found.");
    }
    return Results.Ok(cliente);
});


app.MapPost("/clientes", async ( [FromBody] Cliente cliente, [FromServices]ClientService clientService) =>
{
    await clientService.AddClientAsync(cliente);
    return Results.Created($"/clientes/{cliente.Id}", cliente);
    
});

app.MapPut("/clientes/{id}", async (int id, [FromBody] Cliente cliente,[FromServices] ClientService clientService) =>
{
    if (id != cliente.Id)
    {
        return Results.BadRequest("Client ID mismatch.");
    }
    await clientService.UpdateClientAsync(cliente);
    return Results.Ok();
});

app.MapDelete("/clientes/{id}", async (int id, [FromServices] ClientService clientService) =>
{
    await clientService.DeleteClientAsync(id);
    return Results.Ok();
});



//Fornecedor

app.MapGet("/fornecedores", async ([FromServices] FornecedorService fornecedorService) =>
{
    var fornecedores = await fornecedorService.GetAllFornecedoresAsync();
    return Results.Ok(fornecedores);
    
});

app.MapGet("/fornecedores/{id}", async (int id, [FromServices] FornecedorService fornecedorService) =>
{
    var fornecedor = await fornecedorService.GetFornecedorByIdAsync(id);
    if (fornecedor == null)
    {
        return Results.NotFound($"Fornecedor with ID {id} not found.");
    }
    return Results.Ok(fornecedor);
});


app.MapPost("/fornecedores", async ( [FromBody] Fornecedor fornecedor, [FromServices]FornecedorService fornecedorService) =>
{
    await fornecedorService.AddFornecedorAsync(fornecedor);
    return Results.Created($"/fornecedores/{fornecedor.Id}", fornecedor);
    
});

app.MapPut("/fornecedores/{id}", async (int id, [FromBody] Fornecedor fornecedor,[FromServices]FornecedorService fornecedorService) =>
{
    if (id != fornecedor.Id)
    {
        return Results.BadRequest("Fornecedor ID mismatch.");
    }
    await fornecedorService.UpdateFornecedorAsync(fornecedor);
    return Results.Ok();
});

app.MapDelete("/fornecedores/{id}", async (int id, [FromServices] FornecedorService fornecedorService) =>
{
    await fornecedorService.DeleteFornecedorAsync(id);
    return Results.Ok();
});


//Usuarios

app.MapGet("/usuarios", async ([FromServices] UserService usuarioService) =>
{
    var usuarios = await usuarioService.GetAllUsersAsync();
    return Results.Ok(usuarios);
    
}).RequireAuthorization();

app.MapGet("/usuarios/{id}", async (int id, [FromServices] UserService usuarioService) =>
{
    var usuario = await usuarioService.GetUserByIdAsync(id);
    if (usuario == null)
    {
        return Results.NotFound($"User with ID {id} not found.");
    }
    return Results.Ok(usuario);
}).RequireAuthorization();


app.MapPost("/usuarios", async ( [FromBody] Usuario usuario, [FromServices]UserService usuarioService) =>
{
    await usuarioService.AddUserAsync(usuario);
    return Results.Created($"/usuarios/{usuario.Id}", usuario);
    
}).AllowAnonymous();

app.MapPut("/usuarios/{id}", async (int id, [FromBody] Usuario usuario,[FromServices]UserService usuarioService) =>
{
    if (id != usuario.Id)
    {
        return Results.BadRequest("User ID mismatch.");
    }
    await usuarioService.UpdateUserAsync(usuario);
    return Results.Ok();
}).RequireAuthorization();

app.MapDelete("/usuarios/{id}", async (int id, [FromServices]UserService usuarioService) =>
{
    await usuarioService.DeleteUserAsync(id);
    return Results.Ok();
}).RequireAuthorization();


//Deposito

app.MapPost("/deposito", async ( [FromBody] Deposito deposito, [FromServices] StockService stockService) =>
{
    await stockService.AddStockAsync(deposito);
    return Results.Created($"/deposito/{deposito.Id}", deposito);

});

app.MapGet("/deposito/{depositoId}/produtos", async (int depositoId, [FromServices] StockService stockService) =>
{
    try
    {
        var produtosSumarizados = await stockService.GetProductsInDepositoAsync(depositoId);
        return Results.Ok(produtosSumarizados);
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(ex.Message);
    }
});

app.MapGet("/deposito/produtos/{produtoId}/quantidade", async (int produtoId, [FromServices] StockService stockService) =>
{
    try
    {
        var produtoQuantidade = await stockService.GetProductQuantityInDepositoAsync(produtoId);
        return Results.Ok(produtoQuantidade);
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(ex.Message);
    }
});


//vendas

app.MapPost("/vendas", async (Venda venda, [FromServices] SaleService saleService) =>
{
    try
    {
        await saleService.RecordSaleAsync(venda);
        return Results.Created($"/vendas/{venda.Id}", venda);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapGet("/vendas/produto/{produtoId}/detalhada", async (int produtoId, [FromServices] SaleService saleService) =>
{
    var sales = await saleService.GetSalesByProductDetailedAsync(produtoId);
    return Results.Ok(sales);
});

app.MapGet("/vendas/produto/{produtoId}/sumarizada", async (int produtoId, [FromServices] SaleService saleService) =>
{
    var summarized = await saleService.GetSalesByProductSummarizedAsync(produtoId);
    return Results.Ok(summarized);
});

app.MapGet("/vendas/cliente/{clienteId}/detalhada", async (int clienteId, [FromServices] SaleService saleService) =>
{
    var sales = await saleService.GetSalesByCustomerDetailedAsync(clienteId);
    return Results.Ok(sales);
});

app.MapGet("/vendas/cliente/{clienteId}/sumarizada", async (int clienteId, [FromServices] SaleService saleService) =>
{
    var summarized = await saleService.GetSalesByCustomerSummarizedAsync(clienteId);
    return Results.Ok(summarized);
});



var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}



