using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EasyORM.Utils;
using EasyORM.VSExtension.Utils;

namespace EasyORM.VSExtension.Configuration
{
    public class ConfigurationFile
    {
        private string _fileName;
        List<XElement> _elements;
        XDocument _doc;
        private XElement _connectionStringsElement;
        private XElement _configSectionsElement;
        private List<XElement> _configSectionAddingElements = new List<XElement>();
        private List<XElement> _sectionAddingElements = new List<XElement>();
        private List<XElement> _connectionStringAddingElements = new List<XElement>();
        private XElement _configurationElement;

        internal ConfigurationFile(string fileName)
        {
            this._fileName = fileName;
            FileStream fs;
            if (File.Exists(fileName))
            {
                fs = File.OpenRead(fileName);
                _doc = XDocument.Load(fs);
                fs.Dispose();
            }
            else
            {
                fs = File.Create(fileName);
                _doc = XDocument.Load(fs);
                fs.Dispose();
            }
            _configurationElement = _doc.Elements().FirstOrDefault(x => x.Name == "configuration");
            if (_configurationElement == null)
            {
                _configurationElement = new XElement("confiration");
                _doc.Add(_configurationElement);
            }
            _elements = _configurationElement.Elements().ToList();
            _connectionStringsElement = _elements.FirstOrDefault(x => x.Name == "connectionStrings");
            if (_connectionStringsElement == null)
            {
                _connectionStringsElement = new XElement("connectionStrings");
                _configurationElement.Add(_connectionStringsElement);
            }
            _configSectionsElement = _elements.FirstOrDefault(x => x.Name == "configSections");
            if (_configSectionsElement == null)
            {
                _configSectionsElement = new XElement("configSections");
                _configurationElement.AddFirst(_configSectionsElement);
            }
        }

        /// <summary>
        /// Gets all connection string
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetConnectionStrings()
        {
            return _connectionStringsElement.Elements().Select(x => new
            {
                Key = x.Attribute("name").Value,
                Value = x.Attribute("connectionString").Value
            }).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Sets the configuration file
        /// </summary>
        public void SetConnectionString(string name, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(GetConnectionStrings().GetOrDefault(name)))
            {
                XElement element = new XElement("add");
                element.SetAttributeValue("name", name);
                element.SetAttributeValue("connectionString", connectionString);
                _connectionStringsElement.Add(element);
                _connectionStringAddingElements.Add(element);
            }
            else
            {
                var connectionStringElement = _connectionStringsElement.Elements().FirstOrDefault(x => x.GetAttributeValue("name") == name);
                connectionStringElement.SetAttributeValue("connectionString", connectionString);
            }
        }

        /// <summary>
        /// Gets the config section by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ConfigSection GetConfigSection(string name)
        {
            var xmlSection = _configSectionsElement.Elements().FirstOrDefault(x =>
             {
                 var attr = x.Attribute("name");
                 if (attr == null)
                 {
                     return false;
                 }
                 return attr.Value == name;
             });
            if (xmlSection == null)
            {
                return null;
            }
            return new ConfigSection(xmlSection);
        }

        /// <summary>
        /// Adds the config section
        /// </summary>
        /// <param name="configSection"></param>
        public void AddConfigSection(string name, string type)
        {
            var configSectionElement = new XElement("section");
            configSectionElement.SetAttributeValue("name", name);
            configSectionElement.SetAttributeValue("type", type);
            _configSectionsElement.Add(configSectionElement);
            // _configSectionAddingElements.Add(configSectionElement);
        }

        /// <summary>
        /// Gets the section by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetSection(string name)
        {
            var section = GetConfigSection(name);
            if (section == null)
            {
                return null;
            }
            var sectionNode = _configurationElement.Elements().FirstOrDefault(x => x.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (sectionNode == null)
            {
                return null;
            }
            Type sectionType = null;
            object sectionObject = null;
            var typeArray = section.Type.Split(',');
            if (typeArray.Length < 1)
            {
                sectionType = Type.GetType(typeArray[0]);
            }
            else
            {
                sectionType = Assembly.Load(typeArray[1]).GetType(typeArray[0]);
            }
            sectionObject = Activator.CreateInstance(sectionType);
            foreach (var propertyInfo in sectionType.GetProperties())
            {
                var attr = (ConfigurationPropertyAttribute)propertyInfo.GetCustomAttribute(typeof(ConfigurationPropertyAttribute), false);
                if (attr == null)
                {
                    continue;
                }
                if(!propertyInfo.CanWrite)
                {
                    continue;
                }
                propertyInfo.SetValue(sectionObject, sectionNode.Attribute(attr.Name).Value);
            }
            return sectionObject;
        }

        /// <summary>
        /// Adds the section
        /// </summary>
        /// <param name="name"></param>
        /// <param name="section"></param>
        public void AddSection(string name, object section)
        {
            if (_elements == null)
            {
                _elements = new List<XElement>();
            }
            var elements = _elements;
            var sectionType = section.GetType();
            var properties = sectionType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.SetProperty | BindingFlags.GetProperty);
            XElement element = new XElement(name);
            foreach (var property in properties)
            {
                var attr = AttributeHelper.GetAttribute<ConfigurationPropertyAttribute>(property);
                if (attr == null)
                {
                    continue;
                }
                element.SetAttributeValue(attr.Name, property.GetValue(section));
            }
            elements.Add(element);
            _sectionAddingElements.Add(element);
        }
        /// <summary>
        /// Updates the section
        /// </summary>
        /// <param name="name"></param>
        /// <param name="section"></param>
        public void UpdateSection(string name, object section)
        {
            if (_elements == null)
            {
                _elements = new List<XElement>();
            }
            var elements = _elements;
            var sectionType = section.GetType();
            var properties = sectionType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.SetProperty | BindingFlags.GetProperty);
            XElement element = _configurationElement.Elements().FirstOrDefault(x => x.Name == name);
            if (element == null)
            {
                throw new KeyNotFoundException("The specific element is not exist");
            }
            foreach (var property in properties)
            {
                var attr = AttributeHelper.GetAttribute<ConfigurationPropertyAttribute>(property);
                if (attr == null)
                {
                    continue;
                }
                element.SetAttributeValue(attr.Name, property.GetValue(section));
            }
        }

        /// <summary>
        /// Save configuration file
        /// </summary>
        public void Save()
        {
            //foreach (var configSectionAddingElement in _configSectionAddingElements)
            //{
            //    _configSectionsElement.Add(configSectionAddingElement);
            //}
            foreach (var sectionAddingElement in _sectionAddingElements)
            {
                _configurationElement.Add(sectionAddingElement);
            }
            _doc.Save(_fileName);
        }
    }
}
