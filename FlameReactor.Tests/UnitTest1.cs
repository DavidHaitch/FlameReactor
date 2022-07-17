using FlameReactor.DB;
using FlameReactor.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using Xunit;
using Xunit.Abstractions;

namespace FlameReactor.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;
        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test1()
        {
            var contextOptions = new DbContextOptionsBuilder<FlameReactorContext>()
               .UseInMemoryDatabase("FlameReactor_ShuffleTest")
               .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
               .Options;

            using var context = new FlameReactorContext(contextOptions);

            for (int i = 0; i < 10; i++)
            {
                context.Flames.Add(new Flame
                {
                    Name = "Rating_" + i.ToString(),
                    Rating = i
                });
            }

            var child = new Flame
            {
                Name = "ChildOf1and2"
            };
            child.Birth = new Breeding();
            child.Birth.Parents.Add(context.Flames.Find(1));
            child.Birth.Parents.Add(context.Flames.Find(2));
            context.Flames.Add(child);
            context.SaveChanges();
            var grandchild = new Flame
            {
                Name = "Grandchild"
            };
            grandchild.Birth = new Breeding();
            grandchild.Birth.Parents.Add(context.Flames.Find(3));
            grandchild.Birth.Parents.Add(context.Flames.Find(11));
            context.Flames.Add(grandchild);
            context.SaveChanges();

            var emberService = new EmberService("./", contextOptions, true);
            emberService.FlameConfig.SelectionInstability = 100;
            var flames = emberService.GetEligibleFlames(10, grandchild);
            output.WriteLine("Instablility: " + emberService.FlameConfig.SelectionInstability);
            foreach(var f in flames)
            {
                output.WriteLine(f.Name);
            }
        }
    }
}
