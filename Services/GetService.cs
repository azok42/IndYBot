using IndYBot.Responses;
using IndYBot.Responses.Interfaces;
using IndYBot.Components.Autocomplete;

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

    public async Task<IResponse> GetIndyDays(int month = -1)
    {
        List<IndyDay> indyDays;

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        DateOnly startDate;
        DateOnly endDate;

        if (month == -1)
        {
            startDate = today.AddDays(-10);
            endDate = today.AddDays(20);
        }
        else
        {
            startDate = new DateOnly(today.Year, month, 1);
            endDate = startDate.AddDays(DateTime.DaysInMonth(today.Year, month));
        }

        try
        {
            indyDays = await IndyClient.GetIndyDaysAsync(startDate, endDate);
        }
        catch (Exception e)
        {
            _logger.LogError("Error at getting IndY-Days: {Exception}", e.Message);

            return new ErrorResponse(
                    "Something went wrong!",
                    LogLevel.Error);
        }

        var response = new ListResponse<IndyDay>(
                indyDays,
                day => $"- **{day.DayName}**: {day.Date}\n",
                heading: "IndY-Days:\n");

        return response;
    }

    public async Task<IResponse> GetIndyHours(string teacher = "", Day? day = null, Hour? hour = null)
    {
        List<IndyHour> indyHours;

        try
        {
            indyHours = await IndyClient.GetIndyHoursAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("Error at getting IndY-Hours: {Exception}", e.Message);

            return new ErrorResponse(
                    "Something went wrong!",
                    LogLevel.Error);
        }

        if (!string.IsNullOrEmpty(teacher))
            indyHours = [.. indyHours.Where(indyHour => indyHour.TeacherId.Equals(teacher))];

        if (day != null)
            indyHours = [.. indyHours.Where(indyHour => indyHour.DayName.Equals(day.ToString()))];

        if (hour != null)
            indyHours = [.. indyHours.Where(indyHour => indyHour.Hour == (int) hour)];

        var response = new ListResponse<IndyHour>(
                indyHours,
                hour => $"- **{hour.TeacherId}**({hour.TeacherName}) in {hour.Room} on {hour.DayName} {hour.Hour}\n",
                heading: "IndY-Hours:\n");

        return response;
    }
}
