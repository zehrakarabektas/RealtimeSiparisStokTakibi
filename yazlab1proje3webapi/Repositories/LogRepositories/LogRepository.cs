using Dapper;
using yazlab1proje3webapi.Dtos.LogDtos;
using yazlab1proje3webapi.Models.Context;

namespace yazlab1proje3webapi.Repositories.LogRepositories
{
    public class LogRepository : ILogRepository
    {
        private readonly Context _context;

        public LogRepository(Context context)
        {
            _context=context;
        }

        public async Task AddLog(CreateLogDto log)
        {
            string query = "INSERT INTO Logs (CustomerID, OrderID, LogDate, LogType, LogDetails)  VALUES (@CustomerID, @OrderID, @LogDate, @LogType, @LogDetails)";

            var parameters = new DynamicParameters();
            parameters.Add("@CustomerID", log.CustomerID);
            parameters.Add("@OrderID", log.OrderID);
            parameters.Add("@LogDate", log.LogDate);
            parameters.Add("@LogType", log.LogType);
            parameters.Add("@LogDetails", log.LogDetails);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
            }
        }

        public async void DeleteLog(int logId)
        {
            string query = "DELETE FROM Logs WHERE LogID = @LogID";

            using (var connection = _context.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@LogID", logId);
                await connection.ExecuteAsync(query, parameters);
            }
        }
       
        public async Task<List<ResultLogDto>> GetAllLog()
        {
            string query = @"SELECT  L.LogID, L.CustomerID, C.CustomerType, L.OrderID, P.ProductName, O.Quantity,O.OrderDate, L.LogDate, L.LogType,  L.LogDetails FROM Logs L
                           LEFT JOIN Customers C ON L.CustomerID = C.CustomerID
                           LEFT JOIN Orders O ON L.OrderID = O.OrderID
                           LEFT JOIN Products P ON O.ProductID = P.ProductID
                           ORDER BY L.LogDate DESC";

            using (var connection = _context.CreateConnection())
            {
                var logs = await connection.QueryAsync<ResultLogDto>(query);
                return logs.ToList();
            }
        }

        public async void UpdateLog(UpdateLogDto log)
        {
            string query = "UPDATE Logs SET CustomerID = @CustomerID, OrderID = @OrderID, LogDate = @LogDate, LogType = @LogType, LogDetails = @LogDetails WHERE LogID = @LogID";

            var parameters = new DynamicParameters();
            parameters.Add("@LogID", log.LogID);
            parameters.Add("@CustomerID", log.CustomerID);
            parameters.Add("@OrderID", log.OrderID);
            parameters.Add("@LogDate", log.LogDate);
            parameters.Add("@LogType", log.LogType);
            parameters.Add("@LogDetails", log.LogDetails);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
            }
        }

      
        //public Task<List<ResultLogWithCustomerDto>> GetAllOrderWithCustomerOrder()
        //{
        //    throw new NotImplementedException();
        //}

       


    }
}
