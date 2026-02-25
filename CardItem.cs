using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IRAPROM.MyCore.Model
{
    public class CardItem: INotifyPropertyChanged
    {
        public string TitleName { get => _titleName; set { _titleName = value; OnPropertyChanged(); } }
        public string CenterBottomText { get => _centerBottomText; set { _centerBottomText = value; OnPropertyChanged(); } }
        public string Text { get => _text; set { _text = value; OnPropertyChanged(); } }
        public int Id { get; set; }

        public int LeftFirstNumber { get => _leftFirstNumber; set { _leftFirstNumber = value; OnPropertyChanged(); } }
        public int LeftSecondNumber { get => _leftSecondNumber; set { _leftSecondNumber = value; OnPropertyChanged(); } }
        public int RightFirstNumber { get => _rightFirstNumber; set { _rightFirstNumber = value; OnPropertyChanged(); } }
        public int RightSecondNumber { get => _rightSecondNumber; set { _rightSecondNumber = value; OnPropertyChanged(); } }
        public bool Trigger { get => _trigger; set { _trigger = value; OnPropertyChanged(); } }
        
        public bool OnlineStatus
        {
            get => _onlineStatus;
            set
            {
                _onlineStatus = value;
                OnPropertyChanged();
            }
        }

        private int _leftFirstNumber;
        private int _leftSecondNumber;
        private int _rightFirstNumber;
        private int _rightSecondNumber;
        private bool _trigger;
        private string _centerBottomText;
        private string _titleName;
        private string _text;
        private bool _onlineStatus;

        public void Clean()
        {
            CenterBottomText = Text = "";
            LeftFirstNumber = LeftSecondNumber = RightFirstNumber = RightSecondNumber = 0;
        }








        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
