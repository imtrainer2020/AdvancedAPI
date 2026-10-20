using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static Practicals.Models.Day3;

namespace Practicals.Tests;

public class HttpTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private async Task<HttpClient> Client(string? user = null, string? password = null)
    {
        var c = factory.CreateClient();
        if (user is null) return c;
        var login = await c.PostAsJsonAsync("/api/auth/login", new { userName = user, password });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<LoginResult>())!.AccessToken;
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    [Fact]
    public async Task PublicCatalog_Is200()
    {
        var r = await (await Client()).GetAsync("/api/v1/catalog");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }
    [Fact]
    public async Task Me_WithoutToken_Is401()
    {
        var r = await (await Client()).GetAsync("/api/Day3/me");
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }
    [Fact]
    public async Task Delete_AsUser_Is403()
    {
        var r = await (await Client("student", "Classroom-User-42!")).DeleteAsync("/api/Day3/items/1");
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }
    [Fact]
    public async Task Delete_AsAdmin_Is204()
    {
        var r = await (await Client("admin", "Classroom-Admin-42!")).DeleteAsync("/api/Day3/items/1");
        Assert.Equal(HttpStatusCode.NoContent, r.StatusCode);
    }
    [Fact]
    public async Task BrokenToken_Is401()
    {
        var c = await Client();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not.a.token");
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/Day3/me")).StatusCode);
    }
    [Fact]
    public async Task LiveProbe_Is200()
    {
        var r = await (await Client()).GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }


}
