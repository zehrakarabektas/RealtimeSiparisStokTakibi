using yazlab1proje3webapi.Dtos.LogDtos;

namespace yazlab1proje3webapi.Repositories.LogRepositories
{
    public interface ILogRepository
    {
        Task<List<ResultLogDto>> GetAllLog();

        Task AddLog(CreateLogDto log);
        void UpdateLog(UpdateLogDto log);
        void DeleteLog(int logId);
    }
}
