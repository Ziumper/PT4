using PT4.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT4.ViewModel
{
    public class SortingViewModel : ViewModelBase
    {
        private SortBy sortBy;
        private Direction direction;

        public SortBy SortBy
        {
            get { return sortBy; }  
            set
            {
                sortBy = value;
                NotifyPropertyChanged();
            }   
        }

        public Direction Direction
        {
            get { return direction; }
            set { direction = value; NotifyPropertyChanged(); }
        }
    }
}
