using EntryPoint.Features.DrivingApplication.Models.Responses;
using FluentAssertions;
using Domain.Utilities;
using IntegrationTests.TestBase;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tests.Tests.IntegrationTests;

public class ApplicationTests : IntegrationTestBase
{
    private readonly JsonSerializerOptions _options;
    private readonly string _baseRoute = "api/applications";
    public ApplicationTests(SystemFixture fixture) : base(fixture)
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    }

    [Fact]
    public async Task CreateApplication_ShouldSucceed()
    {
        //CREATE APPLICATION//
        var createRequest = new
        {
            firstName = "John",
            lastName = "Doe",
            dateofBirth = "2004-06-11T09:45:09.195Z",
            nationalId = "123ABCD",
            email = "john@testing.com",
            phoneNumber = "69999823",
            address = "test",
            licenceCategory = "B"
        };

        var response = await Client.PostAsJsonAsync(_baseRoute, createRequest);

        response.EnsureSuccessStatusCode();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await response.Content.ReadFromJsonAsync<CreateApplicationResponse>(_options);

        created.Should().NotBeNull();
        created.Status.Should().Be(ApplicationStatus.Pending);
    }

    [Fact]
    public async Task CreateApplication_ShouldFail()
    {
        //CREATE APPLICATION//
        var createRequest = new
        {
            firstName = "John",
            lastName = "Doe",
            dateofBirth = "2020-06-11T09:45:09.195Z",
            nationalId = "123ABCD",
            email = "john",
            phoneNumber = "test",
            address = "test",
            licenceCategory = "B"
        };

        var response = await Client.PostAsJsonAsync(_baseRoute, createRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetApplication_ShouldSucceed()
    {
        //CREATE APPLICATION//
        var createRequest = new
        {
            firstName = "John",
            lastName = "Doe",
            dateofBirth = "2006-06-11T09:45:09.195Z",
            nationalId = "123ABC",
            email = "john@test.com",
            phoneNumber = "69999999",
            address = "test",
            licenceCategory = "B"
        };

        var response = await Client.PostAsJsonAsync(_baseRoute, createRequest);

        response.EnsureSuccessStatusCode();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await response.Content.ReadFromJsonAsync<CreateApplicationResponse>(_options);

        created.Should().NotBeNull();
        created.Status.Should().Be(ApplicationStatus.Pending);

        //GET APPLICATION//
        var id = created.ApplicationId;

        var getResponse = await Client.GetAsync($"{_baseRoute}/{id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await getResponse.Content.ReadFromJsonAsync<CreateApplicationResponse>(_options);

        result.Should().NotBeNull();
        result.ApplicationId.Should().Be(id);
    }

    [Fact]
    public async Task GetApplication_WithUnknownApplicationId_ShouldReturnNotFound()
    {

        //GET APPLICATION//
        Guid applicationGuid = Guid.NewGuid();

        var getResponse = await Client.GetAsync($"{_baseRoute}/{applicationGuid}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //[Fact]
    public async Task FullyIntegratedTestCreatingDrivingApplication_ShouldReturnOk()
    {
        //CREATE APPLICATION//
        var createRequest = new
        {
            firstName = "John",
            lastName = "Doe",
            dateofBirth = "2001-06-11T09:45:09.195Z",
            nationalId = "1231IICD",
            email = "johnny@test.com",
            phoneNumber = "69123123",
            address = "address",
            licenceCategory = "A"
        };

        var createApplicationResponse = await Client.PostAsJsonAsync(_baseRoute, createRequest);

        createApplicationResponse.EnsureSuccessStatusCode();

        createApplicationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await createApplicationResponse.Content.ReadFromJsonAsync<CreateApplicationResponse>(_options);

        created.Should().NotBeNull();
        created.Status.Should().Be(ApplicationStatus.Pending);

        //UPLOAD PHOTO//
        var bytes = new byte[15 * 1024];
        using (var content = new MultipartFormDataContent())
        {
            var fileContent = new ByteArrayContent(bytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            content.Add(fileContent, "Photo", "test.jpg");

            var photoResponse = await Client.PutAsync($"{_baseRoute}/upload/photo/{created.ApplicationId}", content);

            photoResponse.EnsureSuccessStatusCode();

            photoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        //GET APPLICATION SHOULD BE PENDING//
        var getInitialResponse = await Client.GetAsync($"{_baseRoute}/{created.ApplicationId}");

        getInitialResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getInitialResult = await getInitialResponse.Content.ReadFromJsonAsync<CreateApplicationResponse>(_options);

        getInitialResult.Should().NotBeNull();

        getInitialResult.Status.Should().Be(ApplicationStatus.Pending);

        //SUBMIT APPLICATION//
        var submitApplicationResponse = await Client.PutAsync($"{_baseRoute}/submit/{created.ApplicationId}", null);

        submitApplicationResponse.EnsureSuccessStatusCode();

        submitApplicationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //WAIT FOR APPLICATION TO BE PROCESSED//
        /*int timeoutMs = 5000;
        int intervalMs = 200;
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < TimeSpan.FromMilliseconds(timeoutMs))
        {
            if (await condition())
                return;

            await Task.Delay(intervalMs);
        }*/

        //GET APPLICATION SHOULD BE DIFFERENT FROM SUBMITTED//
        var getResponse = await Client.GetAsync($"{_baseRoute}/{created.ApplicationId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResult = await getResponse.Content.ReadFromJsonAsync<CreateApplicationResponse>(_options);

        getResult.Should().NotBeNull();

        await Task.Delay(15000);

        getResult.Status.Should().NotBe(ApplicationStatus.Submitted);
    }
}