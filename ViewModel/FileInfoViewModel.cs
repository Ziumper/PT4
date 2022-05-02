using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PT4.ViewModel
{
    public class FileInfoViewModel: FileSystemInfoViewModel
    {
        private Dictionary<string, string> imageSource = new Dictionary<string, string>();
        public ICommand OpenFileCommand { get; private set; }
        public new string ImageSource { 
            get 
            {
                string extension =  base.Model.Extension;
                string source;

                imageSource.TryGetValue(extension, out source);
                if (source == null) return "Resources/icon-file.png";

                return source;

            } 
        }

        public FileInfoViewModel(ViewModelBase owner) : base(owner)
        {
            imageSource.Add(".txt", "Resources/icon-txt.png");
            imageSource.Add(".pdf", "Resources/icon-pdf.png");

            OpenFileCommand = new RelayCommand(OnOpenFileCommand, CanExecuteOnOpenFileCommand);
        }

        private bool CanExecuteOnOpenFileCommand(object obj)
        {
            return OwnerExplorer.OpenFileCommand.CanExecute(obj);
        }

        private void OnOpenFileCommand(object obj)
        {
            OwnerExplorer.OpenFileCommand.Execute(obj);
        }
    }
}
