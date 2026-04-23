namespace Octopath_Traveler_View;

internal sealed class MainConsoleInputReader
{
    private readonly View _view;

    public MainConsoleInputReader(View view)
    {
        _view = view;
    }

    public int ReadOption(int min, int max)
    {
        while (true)
        {
            var input = _view.ReadLine();
            if (int.TryParse(input, out var value) && value >= min && value <= max)
            {
                return value;
            }
        }
    }

    public int ReadNonNegativeInt()
    {
        while (true)
        {
            var input = _view.ReadLine();
            if (int.TryParse(input, out var value) && value >= 0)
            {
                return value;
            }
        }
    }
}

