using System.Net;
using GasMapQuebec.Api.IntegrationTests.TestInfrastructure;
using GasMapQuebec.Pricing.Application.Contracts;
using GasMapQuebec.Shared.Abstractions;

namespace GasMapQuebec.Api.IntegrationTests;

public class PriceCorrectionEndpointTests
{
    private const string Body =
        """{"stationId":"00000000-0000-0000-0000-000000000001","fuelType":"regular","priceCents":169.9}""";

    [Fact]
    public async Task Valid_signature_returns_200_and_passes_device_id_as_submitter()
    {
        await using var host = await PriceCorrectionTestHost.StartAsync();

        var response = await host.Client.SendAsync(host.SignedRequest(Body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(PriceCorrectionTestHost.DeviceId, host.CorrectionService.LastSubmitterId);
    }

    [Fact]
    public async Task Pending_result_returns_202()
    {
        await using var host = await PriceCorrectionTestHost.StartAsync();
        host.CorrectionService.Handler = (_, _) =>
            new PriceCorrectionResultDto(Guid.NewGuid(), "Pending", 0.20m);

        var response = await host.Client.SendAsync(host.SignedRequest(Body));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task Service_validation_failure_returns_400()
    {
        await using var host = await PriceCorrectionTestHost.StartAsync();
        host.CorrectionService.Handler = (_, _) =>
            Result.Failure<PriceCorrectionResultDto>(Error.Validation("bad fuel type"));

        var response = await host.Client.SendAsync(host.SignedRequest(Body));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Service_not_found_returns_404()
    {
        await using var host = await PriceCorrectionTestHost.StartAsync();
        host.CorrectionService.Handler = (_, _) =>
            Result.Failure<PriceCorrectionResultDto>(Error.NotFound("no station"));

        var response = await host.Client.SendAsync(host.SignedRequest(Body));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Missing_signature_headers_returns_401()
    {
        await using var host = await PriceCorrectionTestHost.StartAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, PriceCorrectionTestHost.Path)
        {
            Content = new StringContent(Body, System.Text.Encoding.UTF8, "application/json")
        };

        var response = await host.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Tampered_signature_returns_401()
    {
        await using var host = await PriceCorrectionTestHost.StartAsync();

        var response = await host.Client.SendAsync(
            host.SignedRequest(Body, overrideSignature: "not-a-valid-signature"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Body_tampered_after_signing_returns_401()
    {
        await using var host = await PriceCorrectionTestHost.StartAsync();

        // Sign the original body, then swap the content so the body hash no longer matches.
        var request = host.SignedRequest(Body);
        request.Content = new StringContent(
            Body.Replace("169.9", "150.0"), System.Text.Encoding.UTF8, "application/json");

        var response = await host.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Stale_timestamp_returns_401()
    {
        await using var host = await PriceCorrectionTestHost.StartAsync();

        var response = await host.Client.SendAsync(
            host.SignedRequest(Body, timestamp: DateTimeOffset.UtcNow.AddMinutes(-10)));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Replayed_nonce_returns_401_on_second_use()
    {
        await using var host = await PriceCorrectionTestHost.StartAsync();
        const string nonce = "fixed-nonce-value";

        var first = await host.Client.SendAsync(host.SignedRequest(Body, nonce: nonce));
        var second = await host.Client.SendAsync(host.SignedRequest(Body, nonce: nonce));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode);
    }

    [Fact]
    public async Task Exceeding_rate_limit_returns_429()
    {
        await using var host = await PriceCorrectionTestHost.StartAsync(permitLimit: 2);

        var first = await host.Client.SendAsync(host.SignedRequest(Body));
        var second = await host.Client.SendAsync(host.SignedRequest(Body));
        var third = await host.Client.SendAsync(host.SignedRequest(Body));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
    }
}
