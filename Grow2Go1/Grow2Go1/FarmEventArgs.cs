using System;

namespace Grow2Go1
{
    public class FarmEventArgs : EventArgs
    {
        public int FarmId { get; set; }
        public string FarmName { get; set; }
    }
}