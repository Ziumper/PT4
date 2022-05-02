using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT4.ViewModel
{
    public class FileSystemInfoViewModel : ViewModelBase
    {
        protected FileSystemInfo? fileSystemInfo;
        private DateTime lastWriteTime;
        private string? caption;
        private string? imageSource;
        private string? extension;
        private long size;
        private string statusMessage;

        public FileSystemInfoViewModel(ViewModelBase owner)
        {
            Owner = owner;
        }

        public FileExplorer OwnerExplorer
        {
            get
            {
                var owner = Owner;
                while (owner is DirectoryInfoViewModel ownerDirectory)
                {
                    if (ownerDirectory.Owner is FileExplorer explorer)
                        return explorer;
                    owner = ownerDirectory.Owner;
                }
                return null;
            }
        }

        public FileSystemInfo? Model
        {
            get { return fileSystemInfo; }
            set { 
                if (fileSystemInfo != value && value != null)
                {
                    SetProperties(value);
                    
                    FileInfo fileInfo = new FileInfo(value.FullName);
                    Size = fileInfo.Length;

                    NotifyPropertyChanged();
                }
            }
        }

        public ViewModelBase Owner { get; private set; }

        public string? Extension {  get { return extension; } set
            {
                if(extension != value)
                {
                    extension = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string? ImageSource { get { return imageSource; } 
            set 
            {
                if(imageSource != value)
                {
                    imageSource = value;
                    NotifyPropertyChanged();
                }
            } 
        }

        public DateTime LastWriteTime
        {
            get { return lastWriteTime; }
            set
            {
                if (lastWriteTime != value)
                {
                    lastWriteTime = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string? Caption
        {
            get { return caption; }
            set 
            { 
                if(caption != value)
                {
                    caption = value; 
                    NotifyPropertyChanged();
                }
            }
        }

        public long Size
        {
            get { return size; }
            set
            {
                if(size != value)
                {
                    size = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string StatusMessage
        {
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




        public virtual void Sort(SortingViewModel sorting)
        {

        }

        protected void SetProperties(FileSystemInfo model)
        {
            fileSystemInfo = model;
            LastWriteTime = model.LastWriteTime;
            Caption = model.Name;
            Extension = model.Extension;
        }
        
    }
}
