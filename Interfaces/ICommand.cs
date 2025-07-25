namespace UrbanZenith.Interfaces
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        void Execute(string args);
        void ShowMenu();
    }
}

