using Antlr4.Runtime.Misc;
using Milo2.Parser;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;

namespace Milo2;

// MILO: 0.241
// GENERIC: 
public struct MiloObject
{
    public int response;
    public string result;
}


public class MiloContentBuilder
{
    public static string outputFileName = "out";
    public static StringBuilder outputFileContent = new StringBuilder();
    public static StringBuilder outputFileHeader = new StringBuilder();
    
    public static int IndentScope = 0;

    public static Dictionary<string, List<string>> importLists = new Dictionary<string, List<string>>();

    public static int commitAndBuildFile()
    {
        if (!File.Exists($"./{outputFileName}.py"))
        {
            File.Delete($"./{outputFileName}.py");
        }

        using (FileStream fs = File.Create($"./{outputFileName}.py"))
        {
            foreach (KeyValuePair<string, List<string>> kvp in importLists)
            {
                string package = kvp.Key;
                List<string> list = kvp.Value;
                writeHeaderLine($"from {package} import (");
                writeHeaderText($"\t{list[0]}");

                bool skipFirst = true;
                foreach (var obj in list)
                {
                    if (skipFirst)
                    {
                        skipFirst = false;
                        continue;
                    }
                    else
                    {
                        writeHeaderText(",\n");
                        writeHeaderText($"\t{obj}");
                    }
                }
            }
            writeHeaderLine("\n)");

            byte[] info = new UTF8Encoding(true).GetBytes($"{outputFileHeader.ToString()}\n\n{outputFileContent.ToString()}\n");
            fs.Write(info, 0, info.Length);
        }
        return 0;
    }

    public static int createImportList(string package)
    {
        importLists.Add(package, new List<string>());
        return 0;
    }

    public static int appendImportList(string package, string newImport)
    {
        if (importLists[package].Contains(newImport))
            return 1;

        importLists[package].Add(newImport);
        return 0;
    }

    public static int writeHeaderLine(string line)
    {
        outputFileHeader.AppendLine(line);
        return 0;
    }
    public static int writeHeaderText(string text)
    {
        outputFileHeader.Append(text);
        return 0;
    }

    public static int writeContentLine(string line)
    {
        outputFileContent.AppendLine($"{new string('\t', IndentScope)}{line}");
        return 0;
    }

    public static int writeContentText(string text)
    {
        outputFileContent.Append(text);
        return 0;
    }
}
public class MiloVisitor : MiloBaseVisitor<MiloObject>
{

    public MiloVisitor()
    {

        MiloContentBuilder.writeContentLine($"globals()[\"PI\"] = {Math.PI}\n\n");
        MiloContentBuilder.writeContentLine($"if __name__ == \"__main__\":");
        MiloContentBuilder.IndentScope += 1;
    }

    public override MiloObject VisitConstant([NotNull] MiloParser.ConstantContext context)
    {
        string value = "0x00";

        if (context.STRING() is {  } s)
            value = s.GetText();

        if (context.INTEGER() is { } i)
            value = i.GetText();

        if (context.FLOAT() is { } f)
            value = f.GetText();

        if (context.BOOL() is { } b)
            if (b.GetText() == "true")
                value = "True";
            else value = "False";

        if (context.NULL() is { })
            value = "None";


        var result = new MiloObject();
        result.response = 0;
        result.result = value;
        return result;
    }

    public override MiloObject VisitIdentExpr([NotNull] MiloParser.IdentExprContext context)
    {
        var result = new MiloObject();
        result.response = 0;
        result.result = $"local_{context.IDENTIFIER().GetText()}";
        return result;
    }

    public override MiloObject VisitAddOpExpr([NotNull] MiloParser.AddOpExprContext context)
    {
        var left = Visit(context.expr(0)).result;
        var right = Visit(context.expr(1)).result;

        var sign = context.addOp().GetText();
        
        var result = new MiloObject();
        result.response = 0;
        result.result = $"{left} {sign} {right}";
        return result;
    }

    public override MiloObject VisitMultOpExpr([NotNull] MiloParser.MultOpExprContext context)
    {
        var left = Visit(context.expr(0)).result;
        var right = Visit(context.expr(1)).result;

        var sign = context.multOp().GetText();

        var result = new MiloObject();
        result.response = 0;
        result.result = $"{left} {sign} {right}";
        return result;
    }

    public override MiloObject VisitCompOpExpr([NotNull] MiloParser.CompOpExprContext context)
    {
        var left = Visit(context.expr(0)).result;
        var right = Visit(context.expr(1)).result;

        var sign = context.compOp().GetText();

        var result = new MiloObject();
        result.response = 0;
        result.result = $"{left} {sign} {right}";
        return result;
    }

    public override MiloObject VisitBoolOpExpr([NotNull] MiloParser.BoolOpExprContext context)
    {
        var left = Visit(context.expr(0)).result;
        var right = Visit(context.expr(1)).result;

        var sign = context.boolOp().GetText();

        if (sign == "&&")
            sign = "and";

        if (sign == "||")
            sign = "or";

        var result = new MiloObject();
        result.response = 0;
        result.result = $"{left} {sign} {right}";
        return result;
    }

    public override MiloObject VisitNotExpr([NotNull] MiloParser.NotExprContext context)
    {
        var result = new MiloObject();
        result.response = 0;
        result.result = $"not {Visit(context.expr()).result}";
        return result;
    }

    public override MiloObject VisitParenExpr([NotNull] MiloParser.ParenExprContext context)
    {
        var result = new MiloObject();
        result.response = 0;
        result.result = $"({Visit(context.expr()).result})";
        return result;
    }

    public override MiloObject VisitAssignment([NotNull] MiloParser.AssignmentContext context)
    {
        string datatype = context.DATATYPE().GetText();
        string identifier = context.IDENTIFIER().GetText();

        string value = "None";
        if (Visit(context.expr()).result != null)
            value = Visit(context.expr()).result;

        if (datatype != "str")
        {
            if (!MiloContentBuilder.importLists.ContainsKey("ctypes"))
                MiloContentBuilder.createImportList("ctypes");

            if (datatype == "int")
                MiloContentBuilder.appendImportList("ctypes", "c_int");
            else if (datatype == "float")
                MiloContentBuilder.appendImportList("ctypes", "c_float");
            else if (datatype == "bool")
                MiloContentBuilder.appendImportList("ctypes", "c_bool");
            datatype = $"c_{datatype}";
        }

        MiloContentBuilder.writeContentLine($"local_{identifier}: {datatype} = {value}");

        

        var result = new MiloObject();
        result.response = 0;
        result.result = identifier;
        return result;
    }

    public override MiloObject VisitReAssignment([NotNull] MiloParser.ReAssignmentContext context)
    {
        string identifier = context.IDENTIFIER().GetText();
        string value = "None";
        if (Visit(context.expr()).result != null)
            value = Visit(context.expr()).result;

        MiloContentBuilder.writeContentLine($"local_{identifier} = {value}");

        var result = new MiloObject();
        result.response = 0;
        result.result = identifier;
        return result;
    }
}
