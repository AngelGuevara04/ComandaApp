using ComandaApp.Models;
using CommunityToolkit.Mvvm.Input;

namespace ComandaApp.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}