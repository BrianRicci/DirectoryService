using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Contracts;

public static class HttpResponseMessageExtensions
{
    public static async Task<Result<TResponse, Errors>> HandleResponseAsync<TResponse>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        try
        {
            var multipartResponse =
                await response.Content.ReadFromJsonAsync<Envelope<TResponse>>(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return multipartResponse?.ErrorList ?? GeneralErrors.Failure("Error while reading response").ToErrors();
            }

            if (multipartResponse is null)
            {
                return GeneralErrors.Failure("Error while reading response").ToErrors();
            }

            if (multipartResponse.ErrorList is not null)
            {
                return GeneralErrors.Failure("Error while reading response").ToErrors();
            }

            if (multipartResponse.Result is null)
            {
                return GeneralErrors.Failure("Error while reading response. Result is null.").ToErrors();
            }

            return multipartResponse.Result;
        }
        catch
        {
            return GeneralErrors.Failure("Error while reading response").ToErrors();
        }
    }
    
    public static async Task<UnitResult<Errors>> HandleResponseAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var multipartResponse =
                await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return multipartResponse?.ErrorList ?? GeneralErrors.Failure("Error while reading response").ToErrors();
            }

            if (multipartResponse is null)
            {
                return GeneralErrors.Failure("Error while reading response").ToErrors();
            }

            if (multipartResponse.ErrorList is not null)
            {
                return GeneralErrors.Failure("Error while reading response").ToErrors();
            }

            if (multipartResponse.Result is null)
            {
                return GeneralErrors.Failure("Error while reading response. Result is null.").ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch
        {
            return GeneralErrors.Failure("Error while reading response").ToErrors();
        }
    } 
}