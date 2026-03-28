namespace NScrapy
{
    public static class NScrapy
    {
        public static ISpiderBuilder CreateSpider(string name)
        {
            return new SpiderBuilder().Name(name);
        }
    }
}
