using System;
using System.Collections.Generic;

namespace RTLS.Model.View
{
    public class TagDataView
    {
        public DateTime _dateSonOkumaSaati { get; set; }
        public DateTime _dateIlkOkumaSaati { get; set; }
        public DateTime _datePosizyonlamaZamani { get; set; }

        public ICollection<TagReaderView> ReaderData { get; set; }
    }

    public class TagReaderView
    {
        public DateTime LastReadTime { get; set; }
        public string ReaderId { get; set; }
        public double Rssi { get; set; }
    }
}
