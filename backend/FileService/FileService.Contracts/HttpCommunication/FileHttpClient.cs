using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos.GetMediaAssetInfo;
using FileService.Contracts.Dtos.GetMediaAssetsInfo;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Contracts.HttpCommunication;

internal sealed class FileHttpClient : IFileService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileHttpClient> _logger;

    public FileHttpClient(HttpClient httpClient, ILogger<FileHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<Result<GetMediaAssetInfoResponse, Errors>> GetMediaAssetInfo(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/files/{mediaAssetId}", cancellationToken);
            
            return await response.HandleResponseAsync<GetMediaAssetInfoResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media asset for {mediaAssetId}", mediaAssetId);
            
            return Error.Failure("server.internal", "Failed to request media asset info").ToErrors();
        }
    }

    public async Task<Result<GetMediaAssetsInfoResponse, Errors>>
        GetMediaAssetsInfo(GetMediaAssetsInfoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/files/batch", cancellationToken);
            
            return await response.HandleResponseAsync<GetMediaAssetsInfoResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media assets for {MediaAssetIds}", request.MediaAssetIds);
            
            return Error.Failure("server.internal", "Failed to request media assets info").ToErrors();
        }
    }
}