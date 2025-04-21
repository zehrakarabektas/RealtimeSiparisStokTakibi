using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using yazlab1proje3webapi.Dtos.Customer;
using yazlab1proje3webapi.Repositories.CustomerRepositories;

namespace yazlab1proje3webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;
        private static readonly ConcurrentDictionary<int, Mutex> _userMutex = new();

        public CustomerController(ICustomerRepository customerRepository)
        {
            _customerRepository=customerRepository;
        }
        [HttpGet]
        public async Task<IActionResult> CustomerListesi()
        {
            var musteriler = await _customerRepository.GetAllCustomer();

            if (musteriler  == null ||  musteriler.Count == 0)
            {
                return NotFound("Hiç müşteri bulunamadı.");
            }
            return Ok(musteriler);
        }
        [HttpPost]
        public async Task<IActionResult> CustomerEkle(CreateCustomerDto yeniCustomer)
        {
            _customerRepository.AddCustomer(yeniCustomer);
            return Ok("Müşteri başarılı bir şekilde eklendi");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> CustomerSil(int id)
        {
            _customerRepository.DeleteCustomer(id);
            return Ok("Müşteri başarıyla silindi.");
        }
        [HttpPut]
        public async Task<IActionResult> CustomerGuncelle(UpdateCustomerDto guncelCustomer)
        {
            _customerRepository.UpdateCustomer(guncelCustomer);
            return Ok("Müşteri başarıyla guncellendi.");
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> CustomerGoruntuleme(int id)
        {
            var urun = await _customerRepository.GetByCustomer(id);

            if (urun == null)
            {
                return NotFound($"Müşteri ID {id} bulunamadı.");
            }
            return Ok(urun);
        }
        [HttpPut("UpdateBudget")]
        public async Task<IActionResult> UpdateCustomerBudgetAndTotalSpent(UpdateBudgetDto budget)
        {
            var customerMutex = _userMutex.GetOrAdd(budget.CustomerId, _ => new Mutex());
            bool lockAcquired = false;
            try
            {
                lockAcquired = customerMutex.WaitOne(TimeSpan.FromSeconds(30)); 
                if (!lockAcquired)
                {
                    throw new TimeoutException($"Müşteri ID {budget.CustomerId} için bütçe güncelleme işlemi zaman aşımına uğradı.");
                }

                if (budget.DecBudget <= 0)
                {
                    return BadRequest("Harcama miktarı sıfırdan büyük olmalıdır.");
                }
                await _customerRepository.UpdateCustomerBudgetAndTotalSpent(budget.CustomerId, budget.DecBudget);

                return Ok($"Müşteri ID {budget.CustomerId} için bütçe başarıyla güncellendi.");
            }
            finally
            {
                if (lockAcquired)
                {
                    customerMutex.ReleaseMutex();
                }
            }
           
           
        }

  

    }
}
