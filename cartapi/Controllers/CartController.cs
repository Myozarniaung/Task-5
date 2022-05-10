using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using cartapi.Models;
using RabbitMQ.Client;
using System.Text;

namespace cartapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly cartDBContext _context;
        private readonly IConfiguration env;

        public CartController(cartDBContext context, IConfiguration env)
        {
            _context = context;
            this.env = env;
        }

        // GET: api/Cart
        [HttpGet]
        public async Task<ActionResult<IEnumerable<cart>>> Getcarts()
        {
            return await _context.carts.ToListAsync();
        }

        // GET: api/Cart/5
        [HttpGet("{id}")]
        public async Task<ActionResult<cart>> Getcart(int id)
        {
            var cart = await _context.carts.FindAsync(id);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }

        // PUT: api/Cart/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putcart(int id, cart cart)
        {
            if (id != cart.cartId)
            {
                return BadRequest();
            }

            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!cartExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Cart
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<cart>> Postcart(cart cart)
        {

            cart.Status = "SUCCESS";
            _context.carts.Add(cart);
            await _context.SaveChangesAsync();

            var factory = new ConnectionFactory()
            {
                HostName = env.GetSection("RABBITMQHOST").Value,
                Port = Convert.ToInt32(env.GetSection("RABBITMQPORT").Value),
                UserName = env.GetSection("RABBITUSER").Value,
                Password = env.GetSection("RABBITPASSWORD").Value
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "orders", durable: false, exclusive: false, autoDelete: false, arguments: null);

                string message = "Type: ORDER_CREATED | Cart ID:" + cart.cartId + "| Total:" + cart.total + "| Order Status:" + cart.Status + "| Order ID:" + cart.orderId;
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "", routingKey: "orders", basicProperties: null, body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }
            return CreatedAtAction("GetCart", new { id = cart.cartId }, cart);

        }

        // DELETE: api/Cart/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletecart(int id)
        {
            var cart = await _context.carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }

            _context.carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool cartExists(int id)
        {
            return _context.carts.Any(e => e.cartId == id);
        }
    }
}
