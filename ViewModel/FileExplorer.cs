using PT4.DialogWindow;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PT4.ViewModel
{
    public class FileExplorer : ViewModelBase
    {
        private DirectoryInfoViewModel root;
        private SortingViewModel sorting;
        private string statusMessage;
        private int currentMaxThread;
        private CancellationTokenSource source;

        public static readonly string[] TextFilesExtensions = new string[] { ".txt", ".ini", ".log" };
        public event EventHandler<FileInfoViewModel> OnOpenFileRequest;

        public ICommand OpenRootFolderCommand { get; private set; }
        public ICommand SortRootFolderCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }
        public ICommand OnCancelOperationCommand { get; private set; }

        public DirectoryInfoViewModel Root
        {
            get => root;
            set
            {
                if (value != null) root = value;
                NotifyPropertyChanged();
            }
        }

        public SortingViewModel Sorting
        {
            get { return sorting; }
            set { if (value != null) sorting = value; NotifyPropertyChanged(); }
        }

        public string Lang
        {
            get { return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName; }
            set
            {
                if (value != null)
                    if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != value)
                    {
                        CultureInfo.CurrentUICulture = new CultureInfo(value);
                        NotifyPropertyChanged();
                    }
            }
        }

        public string StatusMessage { 
            get 
            { 
                return statusMessage; 
            }
            set 
            {
                if (value != null && statusMessage != value)
                {
                    statusMessage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int CurrentMaxThread
        {
            get
            {
                return currentMaxThread;
            }
            set
            {
                if (currentMaxThread != value) { 
                    currentMaxThread = value;
                    NotifyPropertyChanged();
                }
            }
        }


        public FileExplorer()
        {
            NotifyPropertyChanged(nameof(Lang));

            root = new DirectoryInfoViewModel(this);
            root.PropertyChanged += Root_PropertyChanged;
            NotifyPropertyChanged(nameof(Root));

            sorting = new SortingViewModel();
            sorting.SortBy = Enum.SortBy.Alphabetical;
            sorting.Direction = Enum.Direction.Ascending;
            NotifyPropertyChanged(nameof(Sorting));

            Sorting.PropertyChanged += OnSortingPropertyChangedAsync;

            OpenRootFolderCommand = new RelayCommand(OpenRootFolderExecuteAsync);
            SortRootFolderCommand = new RelayCommand(SortRootFolderExecute, CanExecuteSort);
            ExitCommand = new RelayCommand(ExitExecute);

            OpenFileCommand = new RelayCommand(OnOpenFileCommand, OpenFileCanExecute);
            OnCancelOperationCommand = new RelayCommand(OnCancelClicked);
        }

        private void OnCancelClicked(object obj)
        {
            if(source != null)
            try
            {
                source.Cancel();
            } catch (Exception ex)
            {
                // do nothing
            }
        }

        private void Root_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StatusMessage" && sender is FileSystemInfoViewModel viewModel)
                this.StatusMessage = viewModel.StatusMessage;

            if(e.PropertyName == "CurrentMaxThread" && sender is FileSystemInfoViewModel viewModelThread) 
                this.currentMaxThread = viewModelThread.CurrentMaxThread;
        }

        public string GetFileContent(FileInfoViewModel viewModel)
        {
            var extension = viewModel.Extension?.ToLower();
            if (TextFilesExtensions.Contains(extension))
            {
                return GetTextFileContent(viewModel);
            }
            return "";
        }

        private string GetTextFileContent(FileInfoViewModel viewModel)
        {
            string result = "";
            
            using(var textReader = File.OpenText(viewModel.Model.FullName)) {
                result = textReader.ReadToEnd();
            }

            return result;
        }

        private void OnOpenFileCommand(object obj)
        {
            if (obj is not FileInfoViewModel) return;
            FileInfoViewModel viewModel = (FileInfoViewModel)obj;
            OnOpenFileRequest.Invoke(this, viewModel);
        }

        private bool OpenFileCanExecute(object parameter)
        {
            if (parameter is FileInfoViewModel viewModel)
            {
                var extension = viewModel.Extension?.ToLower();
                return TextFilesExtensions.Contains(extension);
            }
            return false;
        }


        private async void OnSortingPropertyChangedAsync(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            source = new CancellationTokenSource();
        
                await Task.Factory.StartNew(() =>
                {
                try
                {
                    Root.Sort(Sorting, source.Token);
                    Debug.WriteLine("Max thread id:" + CurrentMaxThread);
                    Debug.WriteLine("==================================================");
                    StatusMessage = Strings.Ready;
                    Root.CurrentMaxThread = 0;
                }
                    catch (Exception)
                    {
                        StatusMessage = Strings.Cancelled_Operation;
                    }
                    finally
                    {
                        source.Dispose();
                        source = new CancellationTokenSource();
                    }
                }, source.Token);
          
        }

        private void ExitExecute(object parameter)
        {
            if(parameter == null) return;

            if (parameter is not Window)
            {
                throw new ArgumentException("Not valid parameter passed into exit command");
            };

            Window window = (Window)parameter;
            window.Close();
        }

        private async void OpenRootFolderExecuteAsync(object parameter)
        {
            source = new CancellationTokenSource();
            var dlg = new System.Windows.Forms.FolderBrowserDialog() { Description = Strings.Open_Directory };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    await Task.Factory.StartNew(() =>
                    {
                        StatusMessage = Strings.Loading;
                        var path = dlg.SelectedPath;
                        bool result = Root.Open(path, source.Token);
                        if(result) StatusMessage = Strings.Ready;
                    }, source.Token);
                }
                catch (OperationCanceledException)
                {
                    StatusMessage = Strings.Cancelled_Operation;
                }
                finally
                {
                    source.Dispose();
                    source = new CancellationTokenSource();
                }
            }
        }

        private void OpenRoot(string path,CancellationToken token)
        {
            
        }

        private void SortRootFolderExecute(object parameter)
        {
            Window window = new SortingDialog(Sorting);
            window.Show();
        }

        private bool CanExecuteSort(object parameter)
        {
            return root.IsInitlized;
        }
    }
}
