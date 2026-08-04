namespace HexagonalDDD.Infraestructure.Configuration
{
    public enum PersistenceProvider
    {
        /// <summary>Repositorios en memoria: la aplicacion arranca sin ninguna dependencia externa.</summary>
        InMemory = 0,

        Oracle = 1,

        SqlServer = 2
    }
}
