using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class MetroLogInjector : IInjector
{

    public void Init(AssemblyDefinition reference, ModuleDefinition moduleDefinition)
    {
        var logManagerFactoryType = reference.MainModule.Types.First(x => x.Name == "LogManagerFactory");
        var getDefaultLogManagerDefinition = logManagerFactoryType.Methods.First(x => x.Name == "get_DefaultLogManager");
        getDefaultLogManager = moduleDefinition.Import(getDefaultLogManagerDefinition);


        var logManagerType = reference.MainModule.Types.First(x => x.Name == "ILogManager");
        var getLoggerDefinition = logManagerType.Methods.First(x => x.Name == "GetLogger" && x.IsMatch("String", "LoggingConfiguration"));
        buildLoggerMethod = moduleDefinition.Import(getLoggerDefinition);
        var loggerTypeDefinition = reference.MainModule.Types.First(x => x.Name == "ILogger");

        DebugMethod = moduleDefinition.Import(loggerTypeDefinition.FindMethod("Debug", "String", "Exception"));
        DebugExceptionMethod = moduleDefinition.Import(loggerTypeDefinition.FindMethod("Debug", "String", "Exception"));
        InfoMethod = moduleDefinition.Import(loggerTypeDefinition.FindMethod("Info", "String", "Exception"));
        InfoExceptionMethod = moduleDefinition.Import(loggerTypeDefinition.FindMethod("Info", "String", "Exception"));
        WarnMethod = moduleDefinition.Import(loggerTypeDefinition.FindMethod("Warn", "String", "Exception"));
        WarnExceptionMethod = moduleDefinition.Import(loggerTypeDefinition.FindMethod("Warn", "String", "Exception"));
        ErrorMethod = moduleDefinition.Import(loggerTypeDefinition.FindMethod("Error", "String", "Exception"));
        ErrorExceptionMethod = moduleDefinition.Import(loggerTypeDefinition.FindMethod("Error", "String", "Exception"));
        LoggerType = moduleDefinition.Import(loggerTypeDefinition);
    }


    public MethodReference DebugMethod { get; set; }
    public MethodReference DebugExceptionMethod { get; set; }
    public MethodReference InfoMethod { get; set; }
    public MethodReference InfoExceptionMethod { get; set; }
    public MethodReference WarnMethod { get; set; }
    public MethodReference WarnExceptionMethod { get; set; }
    public MethodReference ErrorMethod { get; set; }
    public MethodReference ErrorExceptionMethod { get; set; }

    public TypeReference LoggerType { get; set; }

    MethodReference buildLoggerMethod;
    

    public IAssemblyResolver AssemblyResolver;
    MethodReference getDefaultLogManager;

    public void AddField(TypeDefinition type, MethodDefinition constructor, FieldDefinition fieldDefinition)
    {
        var instructions = constructor.Body.Instructions;

        instructions.Insert(0, Instruction.Create(OpCodes.Call, getDefaultLogManager));
        instructions.Insert(1, Instruction.Create(OpCodes.Ldstr, type.FullName));
        instructions.Insert(2, Instruction.Create(OpCodes.Ldnull));
        instructions.Insert(3, Instruction.Create(OpCodes.Callvirt, buildLoggerMethod));
        instructions.Insert(4, Instruction.Create(OpCodes.Stsfld, fieldDefinition));
    }

    public string ReferenceName { get { return "MetroLog"; } }
}