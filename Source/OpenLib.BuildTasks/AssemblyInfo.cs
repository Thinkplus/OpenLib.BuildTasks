using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenLib.Utils;
using System;
using System.Reflection;

namespace OpenLib.BuildTasks
{
    /// <summary>
    /// The <c>AssemblyInfo</c> type provides a custom MSBuild task for reading
    /// assembly information attributes for C# and VB projects.
    /// </summary>
    public class AssemblyInfo : Task, IProjectInfo
    {
        /// <summary>
        /// Gets or sets a reference to the I/O utilities.
        /// </summary>
        protected IIoUtils IoUtils { get; set; }

        /// <summary>
        /// Gets or sets the path to the assembly for the project as a required
        /// task property.
        /// </summary>
        [Required]
        public string AssemblyPath { get; set; }

        /// <summary>
        /// Gets or sets the assembly title as a task output property.
        /// </summary>
        [Output]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the assembly description as a task output property.
        /// </summary>
        [Output]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the assembly configuration as a task output property.
        /// </summary>
        [Output]
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets the assembly company as a task output property.
        /// </summary>
        [Output]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the assembly product as a task output property.
        /// </summary>
        [Output]
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the assembly copyright as a task output property.
        /// </summary>
        [Output]
        public string Copyright { get; set; }

        /// <summary>
        /// Gets or sets the assembly trademark as a task output property.
        /// </summary>
        [Output]
        public string Trademark { get; set; }

        /// <summary>
        /// Gets or sets the assembly culture as a task output property.
        /// </summary>
        [Output]
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the assembly version as a task output property.
        /// </summary>
        [Output]
        public string Version { get; set; }

        /// <summary>
        /// Creates a new instance of the <c>AssemblyInfo</c> class.
        /// </summary>
        public AssemblyInfo()
        {
            this.IoUtils = new IoUtils();
        }

        /// <summary>
        /// Executes the custom task when it is invoked in a MSBuild script.
        /// </summary>
        /// <returns>A value indicating if the task completely successfully.</returns>
        public override bool Execute()
        {
            Console.WriteLine("Executing AssemblyInfo MSBuild task...");

            if (!string.IsNullOrWhiteSpace(this.AssemblyPath))
            {
                Console.WriteLine("Attempting to obtain assembly information for '{0}'", this.AssemblyPath);

                bool obtained = this.Obtain();

                Console.WriteLine("Title: {0}", this.Title);
                Console.WriteLine("Description: {0}", this.Description);
                Console.WriteLine("Configuration: {0}", this.Configuration);
                Console.WriteLine("Company: {0}", this.Company);
                Console.WriteLine("Product: {0}", this.Product);
                Console.WriteLine("Copyright: {0}", this.Copyright);
                Console.WriteLine("Trademark: {0}", this.Trademark);
                Console.WriteLine("Culture: {0}", this.Culture);
                Console.WriteLine("Version: {0}", this.Version);

                if (obtained)
                {
                    Console.WriteLine("SUCCESSFULLY obtained assembly information!");

                    return true;
                }
            }

            Console.WriteLine("FAILED to obtain assembly information");

            return false;
        }

        /// <summary>
        /// Obtains the assembly information attributes from the specified
        /// assembly.
        /// </summary>
        /// <returns>A value indicating if the assembly information attributes were obtained.</returns>
        private bool Obtain()
        {
            if (this.IoUtils.FileExists(this.AssemblyPath))
            {
                Assembly assembly = Assembly.LoadFrom(this.AssemblyPath);

                this.Title = this.GetAttribute<AssemblyName>(assembly, "Title", true);
                this.Description = this.GetAttribute<AssemblyDescriptionAttribute>(assembly, "Description", true);
                this.Configuration = this.GetAttribute<AssemblyConfigurationAttribute>(assembly, "Configuration");
                this.Company = this.GetAttribute<AssemblyCompanyAttribute>(assembly, "Company", true);
                this.Product = this.GetAttribute<AssemblyProductAttribute>(assembly, "Product");
                this.Copyright = this.GetAttribute<AssemblyCopyrightAttribute>(assembly, "Copyright");
                this.Trademark = this.GetAttribute<AssemblyTrademarkAttribute>(assembly, "Trademark");
                this.Culture = this.GetAttribute<AssemblyCultureAttribute>(assembly, "Culture");
                this.Version = this.GetAttribute<AssemblyInformationalVersionAttribute>(assembly, "InformationalVersion", true);
            }

            return (!string.IsNullOrWhiteSpace(this.Title) &&
                    !string.IsNullOrWhiteSpace(this.Description) &&
                    !string.IsNullOrWhiteSpace(this.Company) &&
                    !string.IsNullOrWhiteSpace(this.Version));
        }

        /// <summary>
        /// Gets the specified assembly attribute value for the specified
        /// assembly.
        /// </summary>
        /// <typeparam name="T">The generic type of the assembly attribute.</typeparam>
        /// <param name="assembly">A reference to the assembly.</param>
        /// <param name="attributeName">The name of the assembly attribute.</param>
        /// <param name="required">A value indicating if the assembly attribute is required.</param>
        /// <returns>The attribute value of the assembly.</returns>
        /// <exception cref="ArgumentException">Occurs when an <see cref="ArgumentNullException" />
        /// or <see cref="InvalidCastException" /> is caught by this method for a
        /// required assembly attribute, otherwise, the default value for the
        /// assembly attribute <c>T</c> is returned.</exception>
        private string GetAttribute<T>(Assembly assembly, string attributeName, bool required = false)
        {
            string attributeValue = null;

            try
            {
                // get assembly title attribute value
                if (attributeName.Equals("Title"))
                {
                    AssemblyName assemblyName = assembly.GetName();

                    if (assemblyName == null || string.IsNullOrWhiteSpace(assemblyName.Name))
                    {
                        throw new ArgumentNullException(typeof(T).Name, "Assembly attribute is null");
                    }

                    return assemblyName.Name;
                }

                // get custom assembly attribute
                Attribute attribute = Attribute.GetCustomAttribute(assembly, typeof(T));

                if (attribute == null)
                {
                    throw new ArgumentNullException(typeof(T).Name, "Assembly attribute is null");
                }

                // cast assembly attribute to specific attribute type of T
                T t = (T)(object)attribute;

                // get assembly attribute value using reflection
                object o = t.GetType().GetProperty(attributeName).GetValue(t);

                if (o != null)
                    attributeValue = o.ToString();

                // get assembly version attribute value
                if (attributeName.Equals("Version"))
                {
                    if (string.IsNullOrWhiteSpace(attributeValue))
                    {
                        AssemblyName assemblyName = assembly.GetName();

                        if (assemblyName == null || assemblyName.Version == null)
                        {
                            throw new ArgumentNullException(typeof(T).Name, "Assembly attribute is null");
                        }

                        attributeValue = assemblyName.Version.ToString();
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                if (required)
                    throw new ArgumentException(string.Format("The {0} assembly attribute is required", attributeName), ex);
            }
            catch (InvalidCastException ex)
            {
                if (required)
                    throw new ArgumentException(string.Format("The {0} assembly attribute is required", attributeName), ex);
            }

            return attributeValue;
        }
    }
}
