using System;
using System.Configuration;
using System.Globalization;

namespace HexagonalDDD.Infraestructure.Configuration
{
    /// <summary>
    /// Proveedor de persistencia elegido y su cadena de conexion. Se lee del App.config del
    /// ejecutable que hospeda la aplicacion, de modo que el evaluador puede cambiar de InMemory
    /// a Oracle o SQL Server sin recompilar.
    /// </summary>
    public class PersistenceOptions
    {
        public const string ProviderSettingKey = "PersistenceProvider";
        public const string OracleConnectionStringName = "OracleDb";
        public const string SqlServerConnectionStringName = "SqlServerDb";

        public PersistenceOptions(PersistenceProvider provider, string connectionString)
        {
            Provider = provider;
            ConnectionString = connectionString;
        }

        public PersistenceProvider Provider { get; private set; }

        public string ConnectionString { get; private set; }

        public static PersistenceOptions InMemory()
        {
            return new PersistenceOptions(PersistenceProvider.InMemory, null);
        }

        public static PersistenceOptions FromAppConfig()
        {
            string configuredProvider = ConfigurationManager.AppSettings[ProviderSettingKey];

            if (string.IsNullOrWhiteSpace(configuredProvider))
            {
                return InMemory();
            }

            PersistenceProvider provider;
            if (!Enum.TryParse(configuredProvider.Trim(), true, out provider))
            {
                throw new ConfigurationErrorsException(string.Format(
                    CultureInfo.CurrentCulture,
                    "El valor '{0}' de la clave '{1}' no es valido. Valores admitidos: InMemory, Oracle, SqlServer.",
                    configuredProvider,
                    ProviderSettingKey));
            }

            if (provider == PersistenceProvider.InMemory)
            {
                return InMemory();
            }

            string connectionStringName = provider == PersistenceProvider.Oracle
                ? OracleConnectionStringName
                : SqlServerConnectionStringName;

            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new ConfigurationErrorsException(string.Format(
                    CultureInfo.CurrentCulture,
                    "El proveedor '{0}' requiere la cadena de conexion '{1}' en la seccion connectionStrings del App.config.",
                    provider,
                    connectionStringName));
            }

            return new PersistenceOptions(provider, settings.ConnectionString);
        }
    }
}
