using System;

namespace NScrapy.Infra.Attributes.SpiderAttributes
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class URLAttribute : Attribute
    {
        readonly string[] urls;
        public  URLAttribute(params string[] urls)
        {
            this.urls = urls;            
        }

        public string[] URLs
        {
            get { return urls; }
        }

        // This is a named argument
        public int NamedInt { get; set; }
    }
}
