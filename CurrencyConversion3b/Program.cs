using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CurrencyConversion3b
{
    class Program
    {
        static void Main(string[] args)
        {
            Conversion();

            Console.ReadKey();
        }

        private static void GetInfo()
        {
            string uri = "http://www.cbr.ru/scripts/XML_daily.asp";
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using (WebResponse response = request.GetResponse())
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                try
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\CurrencyXML.xml";
                    using (StreamWriter sw = new StreamWriter(path, false, Encoding.GetEncoding(1251)))
                    {
                        sw.WriteLine(reader.ReadToEnd());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void Conversion()
        {
            GetInfo();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\CurrencyXML.xml";
            List<ValuteInfo> valutes = new List<ValuteInfo>();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            XmlElement xmlRoot = xmlDocument.DocumentElement;
            if (xmlRoot != null)
            {
                XmlNode date = xmlRoot.Attributes.GetNamedItem("Date");
                XmlNode name = xmlRoot.Attributes.GetNamedItem("name");
                //Console.WriteLine($"Date: {date.Value}");
                //Console.WriteLine($"name: {name.Value}");
                foreach (XmlElement xmlElement in xmlRoot)
                {
                    ValuteInfo valuteInfo = new ValuteInfo();
                    XmlNode id = xmlElement.Attributes.GetNamedItem("ID");
                    valuteInfo.ID = id.Value;
                    foreach (XmlNode xmlChildNode in xmlElement.ChildNodes)
                    {
                        if (xmlChildNode.Name == "NumCode")
                            valuteInfo.NumCode = xmlChildNode.InnerText;
                        if (xmlChildNode.Name == "CharCode")
                            valuteInfo.CharCode = xmlChildNode.InnerText;
                        if (xmlChildNode.Name == "Nominal")
                        {
                            int result;
                            if (Int32.TryParse(xmlChildNode.InnerText, out result))
                                valuteInfo.Nominal = result;
                        }
                        if (xmlChildNode.Name == "Name")
                            valuteInfo.Name = xmlChildNode.InnerText;
                        if (xmlChildNode.Name == "Value")
                        {
                            decimal result;
                            if (Decimal.TryParse(xmlChildNode.InnerText, out result))
                                valuteInfo.Value = result;
                        }
                    }
                    valutes.Add(valuteInfo);
                }
            }
            var nok = from item in valutes
                      where item.CharCode == "NOK"
                      select item;
            var huf = from item in valutes
                      where item.CharCode == "HUF"
                      select item;
            foreach (var nokValute in nok)
            {
                foreach (var hufValute in huf)
                {
                    var result = (nokValute.Value / nokValute.Nominal) / (hufValute.Value / hufValute.Nominal);
                    Console.WriteLine($"1 NOK(норвежская крона) = {result} HUF(венгерская форинта)");
                }
            }
        }
    }
}
