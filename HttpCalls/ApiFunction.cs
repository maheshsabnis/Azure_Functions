using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using HttpCalls.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HttpCalls
{
    public class ApiFunction
    {
        

        private readonly CompanyContext _context;

        public ApiFunction(CompanyContext ctx)
        {
            _context = ctx;
        }
        [FunctionName("Get")]
        public async Task<IActionResult> Get(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Departments")] HttpRequest req,
           ILogger log)
        {

            try
            {
                // check for the querystring count for keys
                if (req.Query.Keys.Count > 0)
                {
                    // read the 'id' value from the querystring
                    int id = Convert.ToInt32(req.Query["id"]);
                    if (id > 0)
                    {
                        // read data based in 'id'
                        Department Department = new Department();
                        Department = await _context.Departments.FindAsync(id);
                        return new OkObjectResult(Department);

                    }
                    else
                    {
                        // return all records
                        List<Department> Departments = new List<Department>();
                        Departments = await _context.Departments.ToListAsync();
                        return new OkObjectResult(Departments);
                    }
                }
                else
                {
                    List<Department> Departments = new List<Department>();
                    Departments = await _context.Departments.ToListAsync();
                    return new OkObjectResult(Departments);
                }
            }
            catch (Exception ex)
            {
                return new OkObjectResult(ex.Message);
            }


        }

        [FunctionName("Post")]
        public async Task<IActionResult> Post(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Departments")] HttpRequest req,
           ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Department Department = JsonConvert.DeserializeObject<Department>(requestBody);
                var prd = await _context.Departments.AddAsync(Department);
                await _context.SaveChangesAsync();
                return new OkObjectResult(prd.Entity);
            }
            catch (Exception ex)
            {
                return new OkObjectResult($"{ex.Message} {ex.InnerException}");
            }
        }

        [FunctionName("Put")]
        public async Task<IActionResult> Put(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = "Departments/{id:int}")] HttpRequest req, int id,
          ILogger log)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Department Department = JsonConvert.DeserializeObject<Department>(requestBody);
                if (Department.DeptNo == id)
                {
                    _context.Entry<Department>(Department).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return new OkObjectResult(Department);
                }
                else
                {
                    return new OkObjectResult($"Record is not found against the Department Row Id as {id}");
                }

            }
            catch (Exception ex)
            {
                return new OkObjectResult($"{ex.Message} {ex.InnerException}");
            }
        }

        [FunctionName("Delete")]
        public async Task<IActionResult> Delete(
          [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Departments/{id:int}")] HttpRequest req, int id,
          ILogger log)
        {
            try
            {
                var prd = await _context.Departments.FindAsync(id);
                if (prd == null)
                {
                    return new OkObjectResult($"Record is not found against the Department Row Id as {id}");
                }
                else
                {
                    _context.Departments.Remove(prd);
                    await _context.SaveChangesAsync();
                    return new OkObjectResult($"Record deleted successfully based on Department Row Id {id}");
                }
            }
            catch (Exception ex)
            {
                return new OkObjectResult($"{ex.Message} {ex.InnerException}");
            }
        }
    }
}
