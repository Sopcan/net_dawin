using System;
using WebApplication1.Services;
using Xunit;

namespace WebApplication1.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void CreateShopContext()
        {
            using(var context = new ShopContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }
    }
}
