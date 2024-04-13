namespace CookingWithSatan.scripts;

public static class Util
{
    public static string FormatTime(double time)
    {
        var hours = (int) time / 3600;
        var minutes = (int) time / 60;
        var seconds = (int) time % 60;
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
    
    public static string FormatK(int number)
    {
        return number switch
        {
            < 1000 => number.ToString(),
            < 1000000 => $"{number / 1000}K",
            _ => $"{number / 1000000}M"
        };
    }
}