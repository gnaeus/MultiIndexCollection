using System;

namespace MultiIndexCollection.Tests.Data
{
    public class Article : NotifyPropertyChanged
    {
        private int _id;
        public int Id
        {
            get { return _id; }
            set { Set(ref _id, value); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set { Set(ref _content, value); }
        }
    }
}
