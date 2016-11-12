namespace SF_DIY.Interfaces
{
    public interface IFileDialogService
    {
        string OpenFileDialog(string initialDirectory, string defaultPath, string filter, string defaultExtension);
        string SaveFileDialog(string initialDirectory, string defaultPath, string filter, string defaultExtension);
    }
}
