
namespace EightAmps
{
    public interface ICommand
    {
        int Retries { get; set; }

        void Execute();
    }
}
