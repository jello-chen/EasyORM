using EasyORM.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EasyORM.VSExtension.Utils
{
    public class ProjectHelper
    {
        const string WEBAPPLICATIONGUID = "{349C5851-65DF-11DA-9384-00065B846F21}";
        const string WEBSITEGUID = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";

        /// <summary>
        /// Gets project type from the specific project file path
        /// </summary>
        /// <param name="projFilePath"></param>
        /// <returns></returns>
        public static ProjectType GetProjectType(string projFilePath)
        {
            var text = File.ReadAllText(projFilePath);
            var typeString = Regex.Match(text, @"<ProjectTypeGuids>(.*?)</ProjectTypeGuids>").Groups[1].Value;
            var typeArray = typeString.Split(';');
            if (typeArray.Any(x => WEBAPPLICATIONGUID.Equals(x, StringComparison.OrdinalIgnoreCase)) 
                || typeArray.Any(x => WEBSITEGUID.Equals(x, StringComparison.OrdinalIgnoreCase)))
            {
                return ProjectType.WebApplication;
            }
            return ProjectType.Other;
        }

        /// <summary>
        /// Gets default namespace from the specific path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDefaultNamespace(string path)
        {
            var content = File.ReadAllText(path);
            return Regex.Match(content, @"<RootNamespace>(.*?)</RootNamespace>").Groups[1].Value;
        }

        /// <summary>
        /// Gets namespace from the specific path
        /// </summary>
        /// <param name="path">project path</param>
        /// <param name="relativePath">relative path</param>
        /// <returns></returns>
        public static string GetNamespace(string path, string relativePath)
        {
            var defaultNamespace = ProjectHelper.GetDefaultNamespace(path);
            var relativePaths = relativePath.Replace(Path.GetDirectoryName(path), string.Empty).Split('\\');
            foreach (var item in relativePaths)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }
                defaultNamespace = defaultNamespace + "." + StringHelper.ToPascal(item);
            }
            return defaultNamespace;
        }

        /// <summary>
        /// Gets project file from the specific path
        /// </summary>
        /// <returns></returns>
        public static string GetProjectFile(string path)
        {
            var projectFolder = path;
            var files = new string[0];
            while (true)
            {
                files = Directory.GetFiles(projectFolder, "*.csproj");
                if (files.Any())
                {
                    break;
                }
                files = Directory.GetFiles(projectFolder, "*.vbproj");
                if (files.Any())
                {
                    break;
                }
                projectFolder = System.IO.Path.GetDirectoryName(projectFolder);
            }
            return files[0];
        }

        /// <summary>
        /// Gets configuration file from the specific path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetConfigFile(string path)
        {
            var projPath = GetProjectFile(path);
            var type = ProjectHelper.GetProjectType(projPath);
            var fileName = "App.config";
            switch (type)
            {
                case ProjectType.WebApplication:
                    fileName = "Web.config";
                    break;
            }
            return Path.Combine(Path.GetDirectoryName(projPath), fileName);
        }
    }
}
