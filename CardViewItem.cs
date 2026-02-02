namespace IRAPROM.MyCore.Model
{
    public class CardViewItem
    {
        public string Title { get; set; }
        public string CenterBottomText { get; set; }
        public string Text { get; set; }
        public int Id { get; set; }
        public int LeftFirstNumber { get; set; }
        public int LeftSecondNumber { get; set; }
        public int RightFirstNumber { get; set; }
        public int RightSecondNumber { get; set; }
        public bool Trigger { get; set; }

        public void Clean()
        {
            CenterBottomText = Text = "";
            LeftFirstNumber = LeftSecondNumber = RightFirstNumber = RightSecondNumber = 0;
        }
    }
}
