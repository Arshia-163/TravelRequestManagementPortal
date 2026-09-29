namespace TravelRequestManagement.Client.Services;

public static class ApiErrors
{
    public const string Generic =
        "Something went wrong while processing your request. Please try again, and contact support if the problem continues.";

    public static string Describe(Exception ex) => ex switch
    {
        ApiException api when !string.IsNullOrWhiteSpace(api.Message) => api.Message,
        _ => Generic
    };
}
