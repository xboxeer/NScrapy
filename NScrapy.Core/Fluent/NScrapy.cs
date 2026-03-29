namespace NScrapy
{
    public static class NScrapy
    {
        public static global::NScrapy.Core.Fluent.ISpiderBuilder CreateSpider(string name)
        {
            return new global::NScrapy.Core.Fluent.SpiderBuilder().Name(name);
        }
    }
}
