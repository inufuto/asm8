using System;
using System.Collections.Generic;

namespace Inu.Language;

public class TokenReader(Tokenizer tokenizer)
{
    public const int Failure = 1;
    public const int Success = 0;


    public Token LastToken { get; private set; } = new ReservedWord(new SourcePosition("", 0), ReservedWord.EndOfFile);
    private readonly Dictionary<SourcePosition, string> errors = new();
    private readonly Stack<Token> stack = new();

    protected void OpenSourceFile(string fileName)
    {
        tokenizer.OpenSourceFile(fileName);
    }

    public Token NextToken()
    {
        if (stack.Count > 0) {
            return LastToken = stack.Pop();
        }
        return LastToken = tokenizer.GetToken();
    }

    public void ShowError(SourcePosition position, string error)
    {
        if (errors.ContainsKey(position)) return;
        var s = $"{position.ToString()}: {error}";
        errors[position] = s;
        Console.Error.WriteLine(s);
    }

    public void ShowSyntaxError(Token token)
    {
        ShowError(token.Position, "Syntax error: " + token);
    }

    public void ShowSyntaxError()
    {
        ShowSyntaxError(LastToken);
    }

    public void ShowUndefinedError(Token identifier)
    {
        ShowError(identifier.Position, "Undefined: " + identifier.ToString());
    }

    public void ShowMissingIdentifier(SourcePosition position)
    {
        ShowError(position, "Missing identifier.");
    }

    public int ErrorCount => errors.Count;

    public Token AcceptReservedWord(int id)
    {
        if (LastToken is ReservedWord reservedWord && reservedWord.Id == id) return NextToken();
        ShowError(LastToken.Position, "Missing " + ReservedWord.FromId(id));
        return LastToken;
    }

    public void ReturnToken(Token token)
    {
        stack.Push(LastToken);
        LastToken = token;
    }
}