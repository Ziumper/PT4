using PT4.Enum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PT4.ViewModel
{
    public class DirectoryInfoViewModel : FileSystemInfoViewModel
    {
        private FileSystemWatcher? watcher;
        private string? path;
        private string imageSource = "Resources/FolderClose.png";

        public DirectoryInfoViewModel(ViewModelBase owner) : base(owner)
        {
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in args.NewItems.Cast<FileSystemInfoViewModel>())
                    {
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in args.NewItems.Cast<FileSystemInfoViewModel>())
                    {
                        item.PropertyChanged -= Item_PropertyChanged;
                    }
                    break;
            }

        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "StatusMessage" && sender is FileSystemInfoViewModel viewModel)
                this.StatusMessage = viewModel.StatusMessage;
        }


        public new FileSystemInfo? Model
        {
            get { return fileSystemInfo; }
            set
            {
                if (fileSystemInfo != value && value != null)
                {
                    SetProperties(value);

                    DirectoryInfo directoryInfo = new DirectoryInfo(value.FullName);
                    Size = directoryInfo.GetDirectories().Length + directoryInfo.GetFiles().Length;
                    
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsInitlized { 
            get 
            { 
                if(path == null) return false;

                if(path.Length == 0) return false; 

                return true;
            }  
        }

        public new string ImageSource { get => imageSource; private set { } }

        public DispatchedObservableCollection<FileSystemInfoViewModel> Items { get; private set; } = new DispatchedObservableCollection<FileSystemInfoViewModel>();
        public Exception? Exception { get; private set; }

        public void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            //Handling for multithread execution
            Application.Current.Dispatcher.Invoke(() => OnFileSystemChanged(e));
        }

        public bool Open(string path)
        {
            StatusMessage = Strings.Loading;
            this.path = path;
            bool result = false;
            try
            {
                Items.Clear();
                ReadCatalogs();
                ReadFiles();
                result = true;
            } catch (Exception ex)
            {
                Exception = ex;
            }

            if (result) InitlizeWatcher();

            return result;
        }

        public override void Sort(SortingViewModel sortingViewModel)
        {
            bool isEmpty = !IsInitlized;
            
            if (isEmpty) return;

            foreach (var item in Items)
            {
                item.Sort(sortingViewModel);              
            }

            var orderableItems = Items.OrderBy(OrderByType);

            if (sortingViewModel.Direction == Direction.Ascending)
            {
                if (sortingViewModel.SortBy == SortBy.Alphabetical)
                {
                   orderableItems = orderableItems.ThenBy(item => item.Caption);
                }

                if (sortingViewModel.SortBy == SortBy.ModificationDate)
                {
                    orderableItems = orderableItems.ThenBy(item => item.LastWriteTime);
                }

                if(sortingViewModel.SortBy == SortBy.Extension)
                {
                    orderableItems = orderableItems.ThenBy(item => item.Extension);
                }

                if(sortingViewModel.SortBy == SortBy.Size)
                {
                    orderableItems = orderableItems.ThenBy(item => item.Size);
                }
            }

            if(sortingViewModel.Direction == Direction.Descending)
            {
                if (sortingViewModel.SortBy == SortBy.Alphabetical)
                {
                    orderableItems = orderableItems.ThenByDescending(item => item.Caption);
                }

                if (sortingViewModel.SortBy == SortBy.ModificationDate)
                {
                    orderableItems = orderableItems.ThenByDescending(item => item.LastWriteTime);
                }

                if (sortingViewModel.SortBy == SortBy.Extension)
                {
                    orderableItems = orderableItems.ThenByDescending(item => item.Extension);
                }

                if(sortingViewModel.SortBy == SortBy.Size)
                {
                    orderableItems = orderableItems.ThenByDescending(item => item.Size);
                }
            }

             var itemList = orderableItems.ToList();

            foreach(var item in itemList)
            {
                var newIndex = itemList.IndexOf(item);
                var oldIndex = Items.IndexOf(item);
                if(newIndex > -1)
                {

                    Items.Move(oldIndex, newIndex);
                }
            }

        }

        private int OrderByType(ViewModelBase item)
        {
            if(item is DirectoryInfoViewModel)
            {
                return 0;
            }

            return 1;
        }

        private void ReadCatalogs()
        {
            foreach (var dirName in Directory.GetDirectories(path))
            {
                var dirInfo = new DirectoryInfo(dirName);
                DirectoryInfoViewModel itemViewModel = new DirectoryInfoViewModel(this);
                itemViewModel.Model = dirInfo;
                Items.Add(itemViewModel);

                //recurrecny load
                itemViewModel.Open(dirName);
            }
        }

        private void ReadFiles()
        {
            if (path == null) return;
            foreach (var fileName in Directory.GetFiles(path))
            {
                var fileInfo = new FileInfo(fileName);
                FileInfoViewModel itemViewModel = new FileInfoViewModel(this);
                itemViewModel.Model = fileInfo;
                Items.Add(itemViewModel);
            }
        }

        private void InitlizeWatcher()
        {
            if(path == null) return;    
            watcher = new FileSystemWatcher(path);
            watcher.Created += OnFileSystemChanged;
            watcher.Renamed += OnFileSystemChanged;
            watcher.Deleted += OnFileSystemChanged;
            watcher.Changed += OnFileSystemChanged;

            watcher.Error += OnWatcherError;
            watcher.EnableRaisingEvents = true;
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            MessageBox.Show(e.ToString());
        }

        private void OnFileSystemChanged(FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed) return;
            if(path != null) Open(path);
        }

        
    }
}
