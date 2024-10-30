namespace ToAsterixConverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            HDLCAnalyzer hdlcAnalyzer = new HDLCAnalyzer();
            hdlcAnalyzer.OpenPort();
            hdlcAnalyzer.DataFrameExtracted += HdlcAnalyzer_DataFrameExtracted;
            Console.ReadLine();
        }

        private static void HdlcAnalyzer_DataFrameExtracted(object? sender, byte[] e)
        {
            Console.WriteLine("********************************");
            Console.WriteLine(string.Join(" ",e.Select(u=>"0x"+u.ToString("X2"))));
            Console.WriteLine("################################");
        }
    }
}
