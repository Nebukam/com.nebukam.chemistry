using Nebukam.Cluster;

namespace Nebukam.Chemistry
{
    public struct ConstrainedSlotInfos : ISlotInfos<ConstrainedSlot>
    {

        public int i;
        public ByteTrio c;

        public int index
        {
            get { return i; }
            set { i = value; }
        }

        public ByteTrio coord
        {
            get { return c; }
            set { c = value; }
        }

        public void Capture(ConstrainedSlot slot)
        {

        }

    }
}
