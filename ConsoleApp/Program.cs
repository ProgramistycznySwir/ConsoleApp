namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var reader = new DatabaseStructure();
            // Literówka w nazwie pliku.
            // Zależne jaki rodzaj aplikacji to jest warto by wyekstraktować tego stringa do stałej, albo najlepiej
            //   pliku konfiguracyjnego, jednak jeśli jest to prosta aplikacja, sądzę, że jest to zbyteczne.
            reader.ImportAndPrintData("data.csv");
        }
    }
}
