using Milo2.Parser;
using Antlr4.Runtime;

namespace Milo2
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var fileContents = File.ReadAllText("./test.milo");

            AntlrInputStream inputStream = new AntlrInputStream(fileContents);
            MiloLexer miloLexer = new MiloLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(miloLexer);
            MiloParser miloParser = new MiloParser(commonTokenStream);
            var miloProgramContext = miloParser.program();
            MiloVisitor miloVisitor = new MiloVisitor();
            miloVisitor.Visit(miloProgramContext);

            MiloContentBuilder.commitAndBuildFile();
        }
    }
}