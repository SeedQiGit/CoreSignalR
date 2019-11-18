using Microsoft.AspNetCore.DataProtection.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FeiCheSignalR.Services
{
    public class CustomFileXmlRepository : IXmlRepository
    {
        public virtual IReadOnlyCollection<XElement> GetAllElements()
        {
            return GetAllElementsCore().ToList().AsReadOnly();
        }

        private IEnumerable<XElement> GetAllElementsCore()
        {
            //跨平台要使用Combine进行文件连接，因为windows和lixun的文件分割符不一致
            var filePath=Path.Combine(Directory.GetCurrentDirectory(),"key.xml");
            yield return XElement.Load(filePath);
        }

        public virtual void StoreElement(XElement element, string friendlyName)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            StoreElementCore(element, friendlyName);
        }

        private void StoreElementCore(XElement element, string filename)
        {
        }
    }
}
