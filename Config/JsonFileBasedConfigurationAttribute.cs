using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToAsterixConverter.Config;

public class JsonFileBasedConfigurationAttribute : System.Attribute
{
    public JsonFileBasedConfigurationAttribute(string fileName)
    {
        FileName = fileName;
    }
    public string FileName { get; }
}