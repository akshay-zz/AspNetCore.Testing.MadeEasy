using AspNetCore.Testing.MadeEasy.Extensions;
using AspNetCore.Testing.MadeEasy.UnitTest;

namespace AspNetCore.Testing.MadeEasy.Test.Extensions
{
    public class DatabaseRelatedExtensionsTest
    {
        [Fact]
        public void Clear_should_remove_all_data()
        {
            var blog = new Blog();
            var blog2 = new Blog();
            var data = new List<Blog> { blog, blog2, new Blog() };
            var set = MockDb.CreateDbSet<Blog>(data);

            set.Object.Clear();

            var result = set.Object.ToList();

            Assert.Empty(result);
        }
    }
}
