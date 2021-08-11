using System;

class MyClass
{
    private readonly int hundred = 100;
    private bool enabled = true;
    public static string Text => "string";

    /// <summary>
    /// Documentation comment for <see cref="MyClass"/> member method.
    /// </summary>
    /// <param name="result">Some output value.</param>
    public void MyMethod(out int result)
    {
        try
        {
            foreach (var name in new string[] { "name1", "name2" })
            {
                Console.WriteLine(name);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
