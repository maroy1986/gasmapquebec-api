using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class PriceController : ControllerBase
{
    // GET
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
    
    [HttpGet("{id}")]   
    public IActionResult Get(int id)
    {
        return Ok();
    }
    
    [HttpPost]
    public IActionResult Add() 
    {
        return Ok();
    }
    
    [HttpPut]
    public IActionResult Update()
    {
        return Ok();
    }
}