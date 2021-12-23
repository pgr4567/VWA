using System;

public class Command {
    public Command(string name, string[] aliases, PermissionTable[] permissions, Func<string[], string, string> executor, int argumentCount, string usage) {
        this.name = name;
        this.aliases = aliases;
        this.permissions = permissions;
        this.executor = executor;
        this.argumentCount = argumentCount;
        this.usage = usage;
    }
    public string name;
    public string[] aliases;
    public PermissionTable[] permissions;
    public Func<string[], string, string> executor;
    public int argumentCount;
    public string usage;

    public string ExecuteCommand (string[] args, string sender) {
        return args.Length == argumentCount || argumentCount == -1 ? executor(args, sender) : usage;
    }
}