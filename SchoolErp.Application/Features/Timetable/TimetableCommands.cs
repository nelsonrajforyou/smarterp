using Dapper;
using FluentValidation;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Domain.Entities;
using System.Data;

namespace SchoolErp.Application.Features.Timetable;

public class GenerateTimetableCommand : IRequest<bool>
{
    public string AcademicYearId { get; set; } = default!;
}

public class GenerateTimetableCommandHandler : IRequestHandler<GenerateTimetableCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GenerateTimetableCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(GenerateTimetableCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        // Very basic mock implementation of constraint-based generation
        // 1. Fetch configs
        var config = await connection.QueryFirstOrDefaultAsync<TimetableConfig>(TimetableQueries.GetConfig);
        if (config == null) return false;

        var periods = await connection.QueryAsync<Period>(TimetableQueries.GetPeriods, new { ConfigId = config.CONFIG_ID });
        var classes = await connection.QueryAsync<dynamic>("SELECT CLASS_ID FROM CLASSES WHERE IS_DELETED = 0 AND IS_ACTIVE = 1");
        var subjects = await connection.QueryAsync<Subject>(TimetableQueries.GetSubjects);
        var staff = await connection.QueryAsync<dynamic>("SELECT STAFF_ID FROM STAFF_INFO WHERE IS_DELETED = 0");
        var rooms = await connection.QueryAsync<Room>(TimetableQueries.GetRooms);
        var classSubjectStaff = await connection.QueryAsync<ClassSubjectStaff>(TimetableQueries.GetClassSubjectStaff);

        // 2. Clear old entries
        using var transaction = connection.BeginTransaction();
        try
        {
            await connection.ExecuteAsync(TimetableQueries.ClearEntries, transaction: transaction);

            var entriesToInsert = new List<TimetableEntry>();
            var random = new Random();

            // 3. Auto-allocate simple mock
            // In a real scenario, this would be a genetic algorithm or backtracking solver.
            foreach (var cls in classes)
            {
                var reqs = classSubjectStaff.Where(c => c.CLASS_ID == cls.CLASS_ID).ToList();
                
                // Fallback for demo purposes if no config exists
                if (!reqs.Any() && subjects.Any())
                {
                    foreach (var s in subjects.Take(5))
                    {
                        reqs.Add(new ClassSubjectStaff { SUBJECT_ID = s.SUBJECT_ID, STAFF_ID = staff.FirstOrDefault()?.STAFF_ID, MAX_HR_PER_WEEK = 5 });
                    }
                }
                
                // Group by Subject to get PERIODS_PER_WEEK based on count of entries, 
                // but since the original logic expects PERIODS_PER_WEEK per subject, let's group it.
                var subjectGroups = reqs.GroupBy(r => r.SUBJECT_ID).ToList();

                foreach (var subjectGroup in subjectGroups)
                {
                    var subjectId = subjectGroup.Key;
                    int periodsPerWeek = subjectGroup.FirstOrDefault()?.MAX_HR_PER_WEEK ?? 5; // Use max hr per week or fallback
                    
                    int placed = 0;
                    for (int day = 1; day <= config.DAYS_PER_WEEK; day++)
                    {
                        foreach (var period in periods.Where(p => p.IS_BREAK == 0))
                        {
                            if (placed >= periodsPerWeek) break;
                            
                            // Prevent double booking the class for the same period
                            if (entriesToInsert.Any(e => e.CLASS_ID == cls.CLASS_ID && e.DAY_OF_WEEK == day && e.PERIOD_ID == period.PERIOD_ID))
                                continue;

                            // Find a teacher for this subject
                            var teacher = subjectGroup.FirstOrDefault()?.STAFF_ID;
                            if (teacher == null) teacher = staff.FirstOrDefault()?.STAFF_ID;

                            // Find a free teacher to prevent teacher clash
                            var assignedTeacher = teacher;
                            if (entriesToInsert.Any(e => e.TEACHER_ID == assignedTeacher && e.DAY_OF_WEEK == day && e.PERIOD_ID == period.PERIOD_ID))
                            {
                                assignedTeacher = staff.FirstOrDefault(s => !entriesToInsert.Any(e => e.TEACHER_ID == s.STAFF_ID && e.DAY_OF_WEEK == day && e.PERIOD_ID == period.PERIOD_ID))?.STAFF_ID;
                            }

                            entriesToInsert.Add(new TimetableEntry
                            {
                                ENTRY_ID = Guid.NewGuid().ToString(),
                                CLASS_ID = cls.CLASS_ID,
                                DAY_OF_WEEK = day,
                                PERIOD_ID = period.PERIOD_ID,
                                SUBJECT_ID = subjectId,
                                TEACHER_ID = assignedTeacher
                            });

                            placed++;
                            break; // Move to next day for next period of same subject (spread it out)
                        }
                    }
                }
            }

            foreach (var entry in entriesToInsert)
            {
                await connection.ExecuteAsync(TimetableQueries.InsertEntry, entry, transaction);
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}

// Queries Handlers
public class GetTimetableEntriesQuery : IRequest<IEnumerable<object>> { }
public class GetTimetableEntriesQueryHandler : IRequestHandler<GetTimetableEntriesQuery, IEnumerable<object>>
{
    private readonly IDbConnectionFactory _connectionFactory;
    public GetTimetableEntriesQueryHandler(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<IEnumerable<object>> Handle(GetTimetableEntriesQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<object>(TimetableQueries.GetEntries);
    }
}
