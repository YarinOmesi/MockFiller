namespace Sample.Tests;

public interface IDependency
{
    public string MakeString(int number);
    
    public void Add(string name);

    public Response ExecuteRequest(Request request);
}