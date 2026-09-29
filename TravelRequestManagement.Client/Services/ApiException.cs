using System.Net;

namespace TravelRequestManagement.Client.Services;


public sealed class ApiException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
