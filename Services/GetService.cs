using IndYBot.Responses;
using IndYBot.Responses.Interfaces;

using IndYLib.Services;
using IndYLib.Models;

using Microsoft.Extensions.Logging;

namespace IndYBot.Services;

public class GetService(ILogger<GetService> logger)
    : IBotService
{
    private readonly ILogger<GetService> _logger = logger;

    public async Task<IResponse> GetSubjects()
    {
        List<Subject> subjects;

        try
        {
            subjects = await IndyClient.GetActiveSubjectsAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("Error at getting subjects: {Exception}", e.Message);

            return new ErrorResponse(
                    "Something went wrong!",
                    LogLevel.Error);
        }

        var response = new ListResponse<Subject>(
                subjects,
                subject => $"- **{subject.SubjectId}**: {subject.SubjectLong}\n",
                heading: "Subjects:\n");

        return response;
    }
}
