
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ClienteApp.Tests.Api;

public class ClientesApiTests
{
    [Fact]
    public async Task Registrar_ClienteValido_DebeRetornar201()
    {
        // Arrange
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        var solicitud = new
        {
            Nombre = "Ana López",
            Correo = $"ana_{Guid.NewGuid():N}@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Clientes",
            solicitud);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var cliente = await response.Content
            .ReadFromJsonAsync<ClienteRespuesta>();

        Assert.NotNull(cliente);
        Assert.Equal(solicitud.Nombre, cliente.Nombre);
        Assert.Equal(solicitud.Correo, cliente.Correo);
    }

    [Fact]
    public async Task Registrar_NombreVacio_DebeRetornar400()
    {
        // Arrange
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        var solicitud = new
        {
            Nombre = "",
            Correo = $"correo_{Guid.NewGuid():N}@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Clientes",
            solicitud);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    // EXAMEN FINAL - PRUEBA API 200 OK
    [Fact]
    public async Task ObtenerPorId_ClienteExistente_DebeRetornar200()
    {
        // Arrange: preparar la API y el cliente HTTP
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        // Registrar primero un cliente para consultarlo
        var solicitud = new
        {
            Nombre = "Carlos Pérez",
            Correo = $"carlos_{Guid.NewGuid():N}@example.com"
        };

        var registro = await client.PostAsJsonAsync(
            "/api/Clientes",
            solicitud);

        // Comprobar que el cliente fue creado correctamente
        Assert.Equal(
            HttpStatusCode.Created,
            registro.StatusCode);

        var clienteRegistrado = await registro.Content
            .ReadFromJsonAsync<ClienteRespuesta>();

        Assert.NotNull(clienteRegistrado);

        // Act: consultar el cliente existente mediante HttpClient
        var response = await client.GetAsync(
            $"/api/Clientes/{clienteRegistrado.Id}");

        // Assert: comprobar que la API devuelve 200 OK
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        // Comprobar también los datos del cliente obtenido
        var clienteObtenido = await response.Content
            .ReadFromJsonAsync<ClienteRespuesta>();

        Assert.NotNull(clienteObtenido);
        Assert.Equal(clienteRegistrado.Id, clienteObtenido.Id);
        Assert.Equal(solicitud.Nombre, clienteObtenido.Nombre);
        Assert.Equal(solicitud.Correo, clienteObtenido.Correo);
    }

    // EXAMEN FINAL - PRUEBA API 404 NOT FOUND
    [Fact]
    public async Task ObtenerPorId_ClienteInexistente_DebeRetornar404()
    {
        // Arrange: preparar la API y el cliente HTTP
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        // Identificador inexistente en la base de datos
        int idInexistente = 9999;

        // Act: consultar el cliente inexistente
        var response = await client.GetAsync(
            $"/api/Clientes/{idInexistente}");

        // Assert: comprobar que la API devuelve 404 Not Found
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    private sealed record ClienteRespuesta(
        int Id,
        string Nombre,
        string Correo);
}

// Configuración de la API para las pruebas automatizadas
public sealed class ClienteApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"clientes_api_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:ClientesDb"] =
                        $"Data Source={_dbPath};Pooling=False"
                });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
